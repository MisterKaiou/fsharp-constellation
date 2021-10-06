module Constalation.ConstellationContainer

open System
open System.Threading
open Constellation.Attributes
open FSharp.Control
open Microsoft.Azure.Cosmos

let private getCancelToken token =
    token
    |> function
        | Some c -> c
        | None -> defaultArg token (CancellationToken())

type ConstellationContainer =
    | Container of Container

    member internal this.container =
        this
        |> function
            | Container c -> c

    member this.InsertWithOptionsAsync
        (itemOptions: ItemRequestOptions option)
        (cancelToken: CancellationToken option)
        items
        =
        let options = itemOptions |> Option.toObj
        let token = cancelToken |> getCancelToken

        let getPk this =
            AttributeHelpers.getPartitionKeyFrom this

        let createItem item =
            let pk = getPk item

            this.container.CreateItemAsync(item, pk, options, token)
            |> Async.AwaitTask

        match items with
        | [ single ] -> [ createItem single ]
        | many -> many |> List.map createItem
        |> AsyncSeq.ofSeqAsync

    member this.InsertAsync items =
        items |> this.InsertWithOptionsAsync None None

    member this.DeleteWithOptions
        (itemOptions: ItemRequestOptions option)
        (cancelToken: CancellationToken option)
        item
        =
        let options = itemOptions |> Option.toObj
        let token = cancelToken |> getCancelToken

        let id =
            AttributeHelpers.getIdFromTypeFrom item

        let partitionKey =
            AttributeHelpers.getPartitionKeyFrom item

        match Option.ofNullable partitionKey with
        | None -> 
            raise (
                ArgumentException(
                    nameof item,
                    "For Delete operations it is mandatory that the object have a field decorated with PartitionKey attribute and a value assigned to it"
                )
            )
        | Some p ->
            this.container.DeleteItemAsync(id, p, options, token)
            |> Async.AwaitTask

    member this.Delete item = this.DeleteWithOptions None None item

let insertItem item (container: ConstellationContainer) = container.InsertAsync item

let insertItemWithOptions itemOption cancelToken item (container: ConstellationContainer) =
    container.InsertWithOptionsAsync itemOption cancelToken item
    
let deleteItem item (container: ConstellationContainer) = container.Delete item

let deleteWithOptions itemOption cancelToken item (container: ConstellationContainer) =
    container.DeleteWithOptions itemOption cancelToken item