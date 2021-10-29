/// Define types and functions related to a Cosmos Container.
[<RequireQualifiedAccess>]
module Constellation.Container

open System
open System.Threading
open Constellation.Attributes
open FSharp.Control
open Microsoft.Azure.Cosmos
open Constellation.Operators

/// Define custom types used in operations with Cosmos Container.
module Models =

  /// Represents a parameter used in queries. Where left is the parameter to replace (with '@') and right is the parameter value.
  type QueryParam = string * obj

  /// Represents a query to execute.
  type Query =
    { /// The query's text.
      Query: string
      /// A list with all the parameters to replace on the query's text.
      Parameters: QueryParam list }

  /// <summary>Represents an operation yet to be executed.</summary>
  /// <typeparam name="'a">The type returned by this operation.</typeparam>
  type PendingOperation<'a> =
    /// Encapsulates a query operation.
    | Query of (unit -> AsyncSeq<FeedResponse<'a>>)
    /// Encapsulates a operation that returns a single value (I.e GetById).
    | Single of (unit -> Async<ItemResponse<'a>>)
    /// Encapsulates a delete operation.
    | Delete of (unit -> AsyncSeq<ItemResponse<'a>>)
    /// Encapsulates a default operation, something not defined but that also returns a sequence of results.
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

/// <summary>Wrapper around a Container that exposes some of the main CRUD operations for CosmosDB.</summary>
/// <typeparam name="'a">The type handled by this container.</typeparam>
type ConstellationContainer<'a> =
  /// The Container wrapped by this record.
  | Container of Container

  /// The container wrapped by this instance.
  member this.container =
    this
    |> function
      | Container c -> c

  (* ----------------------- Insert ----------------------- *)

  /// <summary>Inserts a new item(s) with the options specified.</summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="items">The items to insert.</param>
  /// <returns>An Default PendingOperation of the this container's type.</returns>
  member this.InsertWithOptions
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

  /// <summary>Inserts a new item(s)</summary>
  /// <param name="items">The items(s) to insert</param>
  /// <returns>An Default PendingOperation of the this container's type.</returns>
  member this.Insert items =
    items |> this.InsertWithOptions None None

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
    item
    =
    item
    |> List.map
         (fun l ->
           let id, partitionKey =
             l >|| AttributeHelpers.getIdFromTypeFrom
             <| AttributeHelpers.getPartitionKeyFrom

           (id, partitionKey))
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

let insertItem item (container: ConstellationContainer<'a>) = container.Insert item

let insertItemWithOptions itemOption cancelToken item (container: ConstellationContainer<'a>) =
  container.InsertWithOptions itemOption cancelToken item

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

  let q =
    { Query = fq.QueryText
      Parameters = fq.Parameters }

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
    raise (
      InvalidOperationException(
        "Wrapped results for query execution must be obtained through the dedicated method 'execQueryWrapped'"
      )
    )

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
