/// <summary>
///     Base module for Context related objects
/// </summary>
module Constellation.Context

open System
open System.IO
open System.Text.Json
open Microsoft.Azure.Cosmos
open System.Text.Json.Serialization
open Constellation.TypeBuilders

let defaultJsonSerializer =
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter(unionEncoding = JsonUnionEncoding.FSharpLuLike))
    options

let private deserialize<'a> (stream: Stream) =
    try
        if typeof<Stream>.IsAssignableFrom (typeof<'a>) then
            (box stream) :?> 'a
        else
            use memoryStream = new MemoryStream()
            stream.CopyTo(memoryStream)
            let span = ReadOnlySpan(memoryStream.ToArray())
            JsonSerializer.Deserialize(span, options = defaultJsonSerializer)

    finally
        stream.Dispose()

let private serialize input =
    let payload = new MemoryStream()
    let options = JsonWriterOptions()
    use writer = new Utf8JsonWriter(payload, options)

    JsonSerializer.Serialize(writer, input, defaultJsonSerializer)
    payload :> Stream

let private defaultCosmosSerializer =
    { new CosmosSerializer() with
        member this.FromStream<'T>(stream: Stream) : 'T = deserialize stream

        member this.ToStream<'T>(input: 'T) : Stream = serialize input }

type CosmosEndpointInfo =
    { Endpoint: string
      AccountKey: string }

type ConnectionMode =
    | ConnectionString of string
    | AccountKey of CosmosEndpointInfo
    | Undefined

/// <summary>
///     A wrapper around a single shared instance of CosmosClient. Providing a EF like usage for DI Containers.
/// </summary>
/// <remarks>
///     It must be noted that, since the intended usage is for the client to be a single instance throughout all context
///     instances, and all context instances are supposed to be a singleton, there are no safe guards against unintentional
///     disposing; except that a new context always checks if the client has been disposed, and creates a new one if so.
///     The proper behaviour can only be guaranteed if the context instances are created as singletons through the
///     application's lifetime.
/// </remarks>
type CosmosContext private () =
    let mutable _databaseId = ""

    static let mutable _disposed = false
    static let mutable _connectionMode = Undefined
    static let mutable _client: CosmosClient = null

    member private this.setupContextClient(clientOptions: CosmosClientOptions option) =
        let getDefaultOptions () =
            cosmosClientOptions { serializer defaultCosmosSerializer }

        let option =
            match clientOptions with
            | Some o -> o
            | None -> getDefaultOptions ()

        match _connectionMode with
        | ConnectionString s -> this.Client <- new CosmosClient(s, option)
        | AccountKey ei -> this.Client <- new CosmosClient(ei.Endpoint, ei.AccountKey, option)
        | _ -> ()

        _disposed <- false

    member this.DatabaseId
        with get () = _databaseId
        and private set v = _databaseId <- v

    member this.ConnectionMode
        with get () = _connectionMode
        and private set v =
            if _connectionMode = Undefined then
                _connectionMode <- v
            else
                ()

    ///<summary>
    ///     This shared CosmosClient. It's the same throughout all of <see cref="CosmosContext"></see>.
    ///</summary>
    member this.Client
        with get () = _client
        and private set v =
            if _disposed || _client |> isNull then
                _client <- v
            else
                ()

    /// <summary>
    ///     Creates a new instance of the this class with a client configured to connect with endpoint and
    /// account key, if not already present.
    /// </summary>
    /// <param name="clientOptions">
    ///     The options with which to create the new client. If not specified, a default value with a custom
    /// json parser will be used. (FSharp.SystemTextJson)
    /// </param>
    /// <param name="databaseId">The database at which this client points to</param>
    /// <param name="cosmosEndpointInfo">This client's endpoint configuration</param>
    new(cosmosEndpointInfo: CosmosEndpointInfo, databaseId, ?clientOptions: CosmosClientOptions) as this =
        new CosmosContext()
        then
            this.ConnectionMode <- AccountKey cosmosEndpointInfo
            this.DatabaseId <- databaseId
            this.setupContextClient clientOptions

    /// <summary>
    ///     Creates a new instance of the this class with a client configured to connect with endpoint and
    /// account key, if not already present.
    /// </summary>
    /// <param name="clientOptions">
    ///     The options with which to create the new client. If not specified, a default value with a custom
    /// json parser will be used. (FSharp.SystemTextJson)
    /// </param>
    /// <param name="databaseId">The database at which this client points to</param>
    /// <param name="connString">This client's connection string</param>
    new(connString, databaseId, ?clientOptions: CosmosClientOptions) as this =
        new CosmosContext()
        then
            this.ConnectionMode <- ConnectionString connString
            this.DatabaseId <- databaseId
            this.setupContextClient clientOptions

    interface IDisposable with
        member this.Dispose() =
            if not _disposed then
                _disposed <- true
                this.Client.Dispose()
                this.ConnectionMode <- Undefined
            else
                ()

type ConstellationContainer =
    | Container of Container

    member internal this.container =
        this
        |> function
            | Container c -> c
