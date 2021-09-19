namespace Constellation

open System
open System.Reflection
open System.Runtime.InteropServices
open Microsoft.Azure.Cosmos
open FSharp.Control

module Attributes =
        
    [<AttributeUsage(AttributeTargets.Field
                     ||| AttributeTargets.Property,
                     AllowMultiple = true,
                     Inherited = true)>]
    type PartitionKeyAttribute() =
        inherit Attribute()    
        
    [<RequireQualifiedAccess>]
    module PartitionKeyAttributeHelpers =
        
        let private getPropertyValue (from: obj) (prop: PropertyInfo) =
            let value = prop.GetValue(from)
            
            match value with
            | :? String as s -> Nullable(PartitionKey(s))
            | :? bool as b -> Nullable(PartitionKey(b))
            | :? double as f -> Nullable(PartitionKey(f))
            | _ -> raise (ArgumentException("The type of the PartitionKey property is not supported"))
        
        let getPartitionKeyFromType (obj: 'a) =
            obj.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
            |> Array.choose
                (fun p ->
                    let partitionKey = p.GetCustomAttribute<PartitionKeyAttribute>()
                    
                    partitionKey
                    |> box
                    |> isNull
                    |> not
                    |> function
                        | true -> Some p
                        | false -> None)
            |> Array.tryHead
            |> function
                | Some p -> p
                | None -> raise (ArgumentNullException(nameof obj, "The given obj does not contain a property or field with PartitionKey Attribute"))
            |> getPropertyValue obj
    
module Constellation =
    type CosmosEndpointInfo =
        { Endpoint: string
          AccountKey: string }

    type ConnectionMode =
        | ConnectionString of string
        | AccountKey of CosmosEndpointInfo
        | Undefined 
        
    type CosmosContext private () =
        let mutable _databaseId = ""

        static let mutable _connectionMode = Undefined

        static let mutable _client: CosmosClient = null

        member private this.setupContextClient(clientOptions: CosmosClientOptions option) =
            let option = clientOptions |> Option.toObj
            
            match _connectionMode with
            | ConnectionString s -> _client <- new CosmosClient(s, option)
            | AccountKey ei -> _client <- new CosmosClient(ei.Endpoint, ei.AccountKey)
            | _ -> ()

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
                if _client = null then
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
            member this.Dispose() = _client.Dispose()
            
    type ConstellationContainer =
        | Container of Container
            
        member private this.container =
            match this with | Container c -> c
            
        member this.insertAsync (itemOptions: ItemRequestOptions option) items =
            let getPk this = Attributes.PartitionKeyAttributeHelpers.getPartitionKeyFromType this
            let container = this.container
            let options = itemOptions |> Option.toObj
            
            match items with
            | [ single ] ->
                let pk = getPk single
                
                [ container
                    .CreateItemAsync(single, pk, options)
                    |> Async.AwaitTask ]
            | _ ->
                items
                |> List.map
                       (fun curr ->
                            let pk = getPk curr
                            
                            container.CreateItemAsync(curr, pk)
                            |> Async.AwaitTask
                       )
            |> AsyncSeq.ofSeqAsync
    
    type CosmosContext with
        member this.GetContainer containerId =
            Container (this.Client.GetContainer(this.DatabaseId, containerId))
            