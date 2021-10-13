module Constellation.Container

open System
open System.Threading
open Constellation.Attributes
open FSharp.Control
open Microsoft.Azure.Cosmos
open Constellation.Operators

let private throwWhenPartitionKeyIsNull (partitionKey: Nullable<PartitionKey>) =
  partitionKey.HasValue
  |> function
    | true -> ()
    | false -> raise (ArgumentNullException("Provided item had null partition key, but it is non-optional"))

let private getCancelToken token =
  token
  |> function
    | Some c -> c
    | None -> defaultArg token (CancellationToken())

type CosmosKeyModel<'a> =
  { [<Id>]
    Id: string
    [<PartitionKey>]
    PartitionKey: 'a }

type Query =
  { Query: string
    Parameters: (string * obj) list }

type ConstellationContainer<'a> =
  | Container of Container

  member this.container =
    this
    |> function
      | Container c -> c

  (* ----------------------- Insert ----------------------- *)

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

  (* ----------------------- Delete ----------------------- *)

  member this.DeleteItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (items: CosmosKeyModel<string> list)
    =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let getIdAndPk item =
      item >|| AttributeHelpers.getIdFromTypeFrom
      <| AttributeHelpers.getPartitionKeyFrom

    let deleteItem item =
      let id, pk = getIdAndPk item

      match Option.ofNullable pk with
      | None ->
        raise (
          ArgumentException(
            nameof items,
            "For Delete operations it is mandatory that the object have a field decorated with PartitionKey attribute and a value assigned to it"
          )
        )
      | Some k ->
        this.container.DeleteItemAsync(id, k, options, token)
        |> Async.AwaitTask

    match items with
    | [ single ] -> [ deleteItem single ]
    | many -> many |> List.map deleteItem
    |> AsyncSeq.ofSeqAsync

  member this.DeleteItem item =
    this.DeleteItemWithOptions None None item

  (* ----------------------- Change ----------------------- *)

  member this.ChangeItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    item
    =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let id, partitionKey =
      item >|| AttributeHelpers.getIdFromTypeFrom
      <| AttributeHelpers.getPartitionKeyFrom

    this.container.ReplaceItemAsync(item, id, partitionKey, options, token)
    |> Async.AwaitTask

  member this.ChangeItem item =
    this.ChangeItemWithOptions None None item

  (* ----------------------- GetSingle ----------------------- *)

  member this.GetItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    id
    partitionKey
    =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    this.container.ReadItemAsync(id, partitionKey, options, token)
    |> Async.AwaitTask

  member this.GetItemWithOptionsFromItem
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (item: 'b)
    =
    let id, partitionKey =
      item >|| AttributeHelpers.getIdFromTypeFrom
      <| AttributeHelpers.getPartitionKeyFrom

    throwWhenPartitionKeyIsNull partitionKey

    this.GetItemWithOptions itemOptions cancelToken id partitionKey.Value

  member this.GetItem id partitionKey =
    this.GetItemWithOptions None None id partitionKey

  member this.GetItemFromItem(item: 'b) : Async<ItemResponse<'a>> =
    let id, partitionKey =
      item >|| AttributeHelpers.getIdFromTypeFrom
      <| AttributeHelpers.getPartitionKeyFrom

    throwWhenPartitionKeyIsNull partitionKey

    this.GetItemWithOptions None None id partitionKey.Value

  (* ----------------------- Query ----------------------- *)

  member this.QueryItemsWithOptions
    (queryOptions: QueryRequestOptions option)
    (continuationToken: string option)
    (cancelToken: CancellationToken option)
    (query: Query)
    =
    let definition =
      query.Parameters
      |> List.fold (fun (def: QueryDefinition) -> def.WithParameter) (QueryDefinition query.Query)

    let options = queryOptions |> Option.toObj
    let token = cancelToken |> getCancelToken
    let contToken = continuationToken |> Option.toObj

    use iterator =
      this.container.GetItemQueryIterator<'a>(definition, contToken, options)

    iterator
    |> AsyncSeq.unfoldAsync
        (fun state ->
            async {
              match state.HasMoreResults with
              | false -> return None
              | true ->
                  let a =
                    state.ReadNextAsync(token) |> Async.AwaitTask |> Async.RunSynchronously
                  return Some (a.Resource, state)
            } )

  member this.Query(query: Query) =
    this.QueryItemsWithOptions None None None query

(* ----------------------- Insert ----------------------- *)

let insertItem item (container: ConstellationContainer<'a>) = container.InsertAsync item

let insertItemWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.InsertWithOptionsAsync itemOption cancelToken item

(* ----------------------- Delete ----------------------- *)

let deleteItem item (container: ConstellationContainer<'a>) = container.DeleteItem item

let deleteItemWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.DeleteItemWithOptions itemOption cancelToken item

(* ----------------------- Change ----------------------- *)

let changeWithOptions itemOptions cancelToken item (container: ConstellationContainer<'a>) =
  container.ChangeItemWithOptions itemOptions cancelToken item

let changeItem item (container: ConstellationContainer<'a>) = container.ChangeItem item

(* ----------------------- GetSingle ----------------------- *)

let getItemWithOptions
  (itemOptions: ItemRequestOptions option)
  (cancelToken: CancellationToken option)
  id
  partitionKey
  (container: ConstellationContainer<'a>)
  =
  container.GetItemWithOptions itemOptions cancelToken id partitionKey

let getItemWithOptionsFromItem
  (itemOptions: ItemRequestOptions option)
  (cancelToken: CancellationToken option)
  item
  (container: ConstellationContainer<'a>)
  =
  container.GetItemWithOptionsFromItem itemOptions cancelToken item

let getItemFromItem item (container: ConstellationContainer<'a>) = container.GetItemFromItem item

(* ----------------------- Query ----------------------- *)

let queryItemsWithOptions
  (queryOptions: QueryRequestOptions option)
  (continuationToken: string option)
  (cancelToken: CancellationToken option)
  (query: Query)
  (container: ConstellationContainer<'a>)
  =
  container.QueryItemsWithOptions queryOptions continuationToken cancelToken query

let query (query: Query) (container: ConstellationContainer<'a>) = container.Query query
