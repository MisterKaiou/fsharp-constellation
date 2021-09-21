/// <summary>
///     Base module for Context related objects
/// </summary>
module Constellation.Context

open System
open Microsoft.Azure.Cosmos

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
        let option = clientOptions |> Option.toObj

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

    new(cosmosEndpointInfo: CosmosEndpointInfo,
        databaseId,
        ?clientOptions: CosmosClientOptions) as this =
        new CosmosContext()
        then
            this.ConnectionMode <- AccountKey cosmosEndpointInfo
            this.DatabaseId <- databaseId
            this.setupContextClient clientOptions

    new(connString,
        databaseId,
        ?clientOptions: CosmosClientOptions) as this =
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
        this |> function Container c -> c