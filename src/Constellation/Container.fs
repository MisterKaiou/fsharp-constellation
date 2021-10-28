[<RequireQualifiedAccess>]
module Constellation.Container

open System
open System.Threading
open Constellation.Attributes
open FSharp.Control
open Microsoft.Azure.Cosmos
open Constellation.Operators

module Models =

  type QueryParam = string * obj
  
  type Query =
    { Query: string
      Parameters: QueryParam list }

  type PendingOperation<'a> =
    | Query of (unit -> AsyncSeq<FeedResponse<'a>>)
    | Single of (unit -> Async<ItemResponse<'a>>)
    | Delete of (unit -> AsyncSeq<ItemResponse<'a>>)
    | Default of (unit -> AsyncSeq<ItemResponse<'a>>)

open Models

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
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let getPk this =
      AttributeHelpers.getPartitionKeyFrom this

    let createItem item =
      let pk = getPk item

      this.container.CreateItemAsync(item, pk, options, token)
      |> Async.AwaitTask

    Default
    <| fun _ ->
         match items with
         | [ single ] -> [ createItem single ]
         | many -> many |> List.map createItem
         |> AsyncSeq.ofSeqAsync

  member this.InsertAsync items =
    items |> this.InsertWithOptionsAsync None None

  (* ----------------------- Delete ----------------------- *)

  member this.DeleteItemByIdWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (items: (string * Nullable<PartitionKey>) list)
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let deleteItem (item: string * Nullable<PartitionKey>) =
      let id, pk = item

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

    Delete
    <| fun _ ->
         match items with
         | [ single ] -> [ deleteItem single ]
         | many -> many |> List.map deleteItem
         |> AsyncSeq.ofSeqAsync

  member this.DeleteItemById item =
    this.DeleteItemByIdWithOptions None None item

  member this.DeleteItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    item =
    item
    |> List.map (
          fun l ->
            let id, partitionKey =
              l
              >|| AttributeHelpers.getIdFromTypeFrom
              <| AttributeHelpers.getPartitionKeyFrom
              
            (id, partitionKey)
       )
    |> this.DeleteItemByIdWithOptions itemOptions cancelToken
  
  member this.DeleteItem item =
    this.DeleteItemWithOptions None None item
  
  (* ----------------------- Update ----------------------- *)

  member this.UpdateWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    item
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let id, partitionKey =
      item >|| AttributeHelpers.getIdFromTypeFrom
      <| AttributeHelpers.getPartitionKeyFrom

    Single
    <| fun _ ->
         this.container.ReplaceItemAsync(item, id, partitionKey, options, token)
         |> Async.AwaitTask

  member this.UpdateItem item = this.UpdateWithOptions None None item

  (* ----------------------- GetSingle ----------------------- *)

  member this.GetItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    id
    partitionKey
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    Single
    <| fun _ ->
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

  member this.GetItemFromItem(item: 'b) =
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

    Query
    <| fun _ ->
         iterator
         |> AsyncSeq.unfold
              (fun state ->
                printfn "Should be called before AsyncSeq iter"

                match state.HasMoreResults with
                | false -> None
                | true ->
                  let next =
                    state.ReadNextAsync(token)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously

                  Some(next, state))

  member this.Query(query: Query) =
    this.QueryItemsWithOptions None None None query

(* ----------------------- Insert ----------------------- *)

let insertItem item (container: ConstellationContainer<'a>) = container.InsertAsync item

let insertItemWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.InsertWithOptionsAsync itemOption cancelToken item

(* ----------------------- Delete ----------------------- *)

let deleteItemById item (container: ConstellationContainer<'a>) = container.DeleteItemById item
  
let deleteItemByIdWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.DeleteItemByIdWithOptions itemOption cancelToken item

let deleteItemWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.DeleteItemWithOptions itemOption cancelToken item

let deleteItem item (container: ConstellationContainer<'a>) = container.DeleteItem item

(* ----------------------- Change ----------------------- *)

let changeWithOptions itemOptions cancelToken item (container: ConstellationContainer<'a>) =
  container.UpdateWithOptions itemOptions cancelToken item

let changeItem item (container: ConstellationContainer<'a>) = container.UpdateItem item

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

let getItem id partitionKey (container: ConstellationContainer<'a>) = container.GetItem id partitionKey

(* ----------------------- Query ----------------------- *)

let queryItemsWithOptions
  (queryOptions: QueryRequestOptions option)
  (cancelToken: CancellationToken option)
  (continuationToken: string option)
  (query: Query)
  (container: ConstellationContainer<'a>)
  =
  container.QueryItemsWithOptions queryOptions continuationToken cancelToken query

type FluentQuery<'a> =
  { Container: ConstellationContainer<'a>
    QueryText: string
    Parameters: QueryParam list }

let query query (container: ConstellationContainer<'a>) =
  { Container = container
    QueryText = query
    Parameters = [] }

let parameterlessQuery query (container: ConstellationContainer<'a>) =
  let q = { Query = query; Parameters = [] }
  container.Query q

let withParameters<'a> params' (query: FluentQuery<'a>) =
  let fq = { query with Parameters = params' }
  let q = { Query = fq.QueryText; Parameters = fq.Parameters }
  query.Container.Query q

(* ----------------------- Execution ----------------------- *)

let execQueryWrapped op =
  op
  |> function
    | Query q -> q ()
    | _ -> failwith "This case should have not been hit!"

let execAsyncWrapped<'a> (pending: PendingOperation<'a>) =
  match pending with
  | Single s -> [ s () ] |> AsyncSeq.ofSeqAsync
  | Default f -> f ()
  | Delete d -> d ()
  | Query _ ->
    raise (InvalidOperationException("Wrapped results for query execution must be obtained through the dedicated method 'execQueryWrapped'"))

let execAsync<'a> (pending: PendingOperation<'a>) =
  match pending with
  | Query q ->
    q ()
    |> AsyncSeq.collect (fun item -> item.Resource |> AsyncSeq.ofSeq)
  | Single s ->
    [ s () ]
    |> AsyncSeq.ofSeqAsync
    |> AsyncSeq.map (fun item -> item.Resource)
  | Delete _ ->
    []
    |> AsyncSeq.ofSeq (* For delete operations, cosmos return null instead of a usable resource *)
  | Default f -> f () |> AsyncSeq.map (fun item -> item.Resource)
