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

    member this.Client
        with get () = _client
        and private set v =
            if _disposed || _client = null then
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
            if _disposed = false then
                this.Client.Dispose()
                this.ConnectionMode <- Undefined
            else
                ()
        
type ConstellationContainer = 
    | Container of Container
        
    member internal this.container =
        match this with | Container c -> c
        