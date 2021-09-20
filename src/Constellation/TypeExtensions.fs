module Constellation.TypeExtensions

open Constellation.Context
open Constellation
open Microsoft.Azure.Cosmos
open System.Threading
open FSharp.Control

type ConstellationContainer with 
    member this.insertWithOptionsAsync (itemOptions: ItemRequestOptions option) (cancelToken: CancellationToken option) items =
        let getPk this = Attributes.PartitionKeyAttributeHelpers.getPartitionKeyFromType this
        let container = this.container
        let options = itemOptions |> Option.toObj
        let token = cancelToken |> function Some c -> c | None -> defaultArg cancelToken (CancellationToken())           
        
        match items with
        | [ single ] ->
            let pk = getPk single
            
            [ container
                .CreateItemAsync(single, pk, options, token)
                |> Async.AwaitTask ]
        | _ ->
            items
            |> List.map
                   (fun curr ->
                        let pk = getPk curr
                        
                        container
                            .CreateItemAsync(curr, pk, options, token)
                            |> Async.AwaitTask
                   )
        |> AsyncSeq.ofSeqAsync

type CosmosContext with
    member this.GetContainer containerId =
        Context.Container (this.Client.GetContainer(this.DatabaseId, containerId))
