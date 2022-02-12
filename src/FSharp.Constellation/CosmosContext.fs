/// Base module for Context related objects.
module FSharp.Constellation.Context

open System
open System.IO
open FSharp.Constellation
open FSharp.Constellation.Attributes
open FSharp.Constellation.Serialization
open FSharp.Constellation.TypeBuilders
open Microsoft.Azure.Cosmos
  
  ///Contains the types and functions used to handle endpoint information.
  module Endpoint =
    
    type CosmosAccountKeyInfo =
      { Endpoint: string
        AccountKey: string }
      
    let createEndpointInfo endpoint accountKey = { Endpoint = endpoint; AccountKey = accountKey }

open Endpoint
open System.Threading

/// The default Serializer used with Cosmos containers.
let defaultCosmosSerializer =
  { new CosmosSerializer() with
      member this.FromStream<'T>(stream: Stream) : 'T = deserialize stream

      member this.ToStream<'T>(input: 'T) : Stream = serialize input }

/// Defines the used connection modes.
type ConnectionMode =
    /// Connect using a connection string.
  | ConnectionString of string
    /// Connection using an AccountKey.
  | AccountKey of CosmosAccountKeyInfo
    /// Not yet defined connection mode.
  | Undefined

/// <summary>
///     A wrapper around an instance of CosmosClient. Providing a EF like usage for DI Containers.
/// </summary>
type CosmosContext private () =
  let mutable _databaseId = ""

  let mutable _disposed = false
  let mutable _authMode = Undefined
  let mutable _client: CosmosClient = null

  member private this.setupContextClient(clientOptions: CosmosClientOptions option) =
    let getDefaultOptions () =
      cosmosClientOptions { serializer defaultCosmosSerializer }

    let option =
      match clientOptions with
      | Some o -> o
      | None -> getDefaultOptions ()

    match _authMode with
    | ConnectionString s -> this.Client <- new CosmosClient(s, option)
    | AccountKey ei -> this.Client <- new CosmosClient(ei.Endpoint, ei.AccountKey, option)
    | _ -> ()

    _disposed <- false

  /// The Database ID that this Context is connected to.
  member this.DatabaseId
    with get () = _databaseId
    and private set v = _databaseId <- v

  /// The connection mode used for this Context.
  member this.ConnectionMode
    with get () = _authMode
    and private set v =
      if _authMode = Undefined then
        _authMode <- v
      else
        ()

  ///<summary>
  ///   This shared CosmosClient. It's the same throughout all of <see cref="CosmosContext"></see>.
  ///</summary>
  member this.Client
    with get () = _client
    and private set v =
      if _disposed || _client |> isNull then
        _client <- v
      else
        ()

  /// <summary>
  ///    Gets a new container using the specified <paramref name="containerId"/>.
  /// </summary>
  /// <param name="containerId">The name of the container on CosmosDB.</param>
  /// <typeparam name="'of'">The type that the returned Container will handle.</typeparam>
  /// <returns>A new instance of the ConstellationContainer for the type defined by <typeparamref name="'of'"/>.</returns>
  member this.GetContainer<'of'>(containerId) : Container.ConstellationContainer<'of'> =
    Container.Container(this.Client.GetContainer(this.DatabaseId, containerId))

  /// <summary>
  ///    Gets a new container using the specified type as the source of the container ID.
  /// </summary>
  /// <typeparam name="from">The type from which to take the ContainerId.</typeparam>
  /// <returns>A new instance of the ConstellationContainer for the type defined by <typeparamref name="'from'"/>.</returns>
  member this.GetContainer<'from>() : Container.ConstellationContainer<'from> =
    let containerId = AttributeHelpers.getContainerIdFromType<'from>
    
    Container.Container(this.Client.GetContainer(this.DatabaseId, containerId))

  
  /// <summary>
  ///     Creates a new instance of the this class with a client configured to connect with endpoint and
  /// account key, if not already present.
  /// </summary>
  /// <param name="clientOptions">
  ///     The options with which to create the new client. If not specified, a default value with a custom
  /// json parser will be used (FSharp.SystemTextJson).
  /// </param>
  /// <param name="databaseId">The database at which this client points to.</param>
  /// <param name="cosmosEndpointInfo">This client's endpoint configuration.</param>
  new(cosmosEndpointInfo: CosmosAccountKeyInfo, databaseId, ?clientOptions: CosmosClientOptions) as this =
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
  /// json parser will be used (FSharp.SystemTextJson).
  /// </param>
  /// <param name="databaseId">The database at which this client points to.</param>
  /// <param name="connString">This client's connection string.</param>
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

/// <summary>
///    Gets a new container using the specified type as the source of the container name.
/// </summary>
/// <param name="ctx">The context used to retrieve the container.</param>
/// <typeparam name="'from">The type from which take the ContainerId.</typeparam>
/// <returns>A new instance of the ConstellationContainer for the type defined by <typeparamref name="'from'"/>.</returns>
let getContainer<'from> (ctx: CosmosContext) = ctx.GetContainer<'from>()

/// <summary>
///    Gets a new container using the specified <paramref name="containerId"/>.
/// </summary>
/// <param name="containerId">The ID of the container to retrieve.</param>
/// <param name="ctx">The context used to retrieve the container.</param>
/// <typeparam name="'of'">The type that the returned Container will handle.</typeparam>
/// <returns>A new instance of the ConstellationContainer for the type defined by <typeparamref name="'of'"/>.</returns>
let getContainerWithId<'of'> containerId (ctx: CosmosContext) = ctx.GetContainer<'of'> containerId 

/// <summary>
/// Configures a new RequestHandler that reads the request body.
/// </summary>
/// <param name="ctx">The CosmosContext on which to apply the configuration.</param>
/// <param name="auditFunc">The function to apply on the RequestMessage body.</param>
let configureAuditingRequestHandler (ctx: CosmosContext) auditFunc =
  let handler =
    { new RequestHandler() with
        member this.SendAsync(request: RequestMessage, cancellationToken: CancellationToken) = 
          let buffer = Array.zeroCreate<byte> ((int)request.Content.Length)
          request.Content.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
          |> Async.AwaitTask |> Async.RunSynchronously
          |> ignore

          System.Text.Encoding.UTF8.GetString(buffer)
          |> auditFunc

          this.InnerHandler.SendAsync(request, cancellationToken) }

  ctx.Client.ClientOptions.CustomHandlers.Add(handler)
