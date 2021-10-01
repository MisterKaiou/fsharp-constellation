module Constellation.TypeExtensions

open Constellation.Context
open Constellation
open Constellation.Attributes
open Microsoft.Azure.Cosmos
open System.Threading
open FSharp.Control

let private getCancelToken token = token |> function Some c -> c | None -> defaultArg token (CancellationToken())

type ConstellationContainer with
    member this.insertWithOptionsAsync (itemOptions: ItemRequestOptions option) (cancelToken: CancellationToken option) items =
        let getPk this = PartitionKeyAttributeHelpers.getPartitionKeyFromType this
        let options = itemOptions |> Option.toObj
        let token = cancelToken |> getCancelToken           
                
        match items with
        | [ single ] ->
            let pk = getPk single
            
            [ this.container
                .CreateItemAsync(single, pk, options, token)
                |> Async.AwaitTask ]
        | _ ->
            items
            |> List.map
                   (fun curr ->
                        let pk = getPk curr
                        
                        this.container
                            .CreateItemAsync(curr, pk, options, token)
                            |> Async.AwaitTask
                   )
        |> AsyncSeq.ofSeqAsync

    member this.insertAsync items = items |> this.insertWithOptionsAsync None None

    member this.deleteWithOptions (itemOptions: ItemRequestOptions option) (cancelToken: CancellationToken option) id partitionKey =
        let options = itemOptions |> Option.toObj
        let token = cancelToken |> getCancelToken
        
        this.container.DeleteItemAsync(id, partitionKey, options, token)
        |> Async.AwaitTask

    member this.delete id partitionKey = (id, partitionKey) ||> this.deleteWithOptions None None

type CosmosContext with
    member this.GetContainer containerId =
        Context.Container (this.Client.GetContainer(this.DatabaseId, containerId))
