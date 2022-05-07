/// Define types and functions related to a Cosmos Container.
[<RequireQualifiedAccess>]
module FSharp.Constellation.Container

open System
open System.Threading
open FSharp.Constellation.Attributes
open FSharp.Control
open Microsoft.Azure.Cosmos
open FSharp.Constellation.Operators
open FSharp.Constellation.Models
open FSharp.Constellation.Models.Keys

let private getCancelToken token =
  token
  |> function
    | Some c -> c
    | None -> defaultArg token (CancellationToken())

let private getKeyAndId (keyParam: KeyParam) =
  match keyParam with
  | SameIdPartition s -> (s, StringKey s)
  | IdAndKey(s, key) -> (s, key)

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
      |> fun it -> it.Key

    let createItem item =
      let pk = getPk item

      this.container.CreateItemAsync(item, pk, options, token)
      |> Async.AwaitTask

    Operation
    <| fun _ ->
         match items with
         | [ single ] -> [ createItem single ]
         | many -> many |> List.map createItem
         |> AsyncSeq.ofSeqAsync
         |> AsyncSeq.map (fun i -> Response i )

  /// <summary>Inserts a new item(s)</summary>
  /// <param name="items">The items(s) to insert</param>
  /// <returns>An Default PendingOperation of this container's type.</returns>
  member this.Insert items =
    items |> this.InsertWithOptions None None

  (* ----------------------- Delete ----------------------- *)

  /// <summary>Delete an item(s) by its id and PartitionKey with options models and cancellation token.</summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="keys">A list with KeyParam to be used in the search.</param>
  /// <returns>A Delete PendingOperation.</returns>
  member this.DeleteItemByIdWithOptions<'b>
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (keys: KeyParam list)
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let deleteItem (item: string * PartitionKeys) =
      let id, pk = item

      this.container.DeleteItemAsync(id, pk.Key, options, token)
      |> Async.AwaitTask
    
    Operation
    <| fun _ ->
         keys
         |> List.map (getKeyAndId >> deleteItem)
         |> AsyncSeq.ofSeqAsync
         |> AsyncSeq.map (fun i -> Response i)

  /// <summary>Deletes an item(s) by its id and PartitionKey.</summary>
  /// <param name="item">A list with tupled id and PartitionKey.</param>
  member this.DeleteItemById item =
    this.DeleteItemByIdWithOptions None None item

  /// <summary>Deletes an item from the database from an item(s) id and PartitionKey</summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="items">A list the items from which take the id and PartitionKey to be used in the search.</param>
  /// <returns>A Delete PendingOperation</returns>
  member this.DeleteItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    items
    =
    items
    |> List.map
         (fun l ->
           let id, partitionKey =
             l
             >|| AttributeHelpers.getIdFrom
             <| AttributeHelpers.getPartitionKeyFrom

           IdAndKey (id, partitionKey))
    |> this.DeleteItemByIdWithOptions itemOptions cancelToken

  /// <summary>Deletes an item from the database from an item(s) id and PartitionKey</summary>
  /// <param name="items">A list the items from which take the id and PartitionKey to be used in the search.</param>
  /// <returns>A Delete PendingOperation</returns>
  member this.DeleteItem items =
    this.DeleteItemWithOptions None None items

  (* ----------------------- Replace ----------------------- *)

  /// <summary>Replace an entry in the database for the object provided.</summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="items">The item(s) to replace.</param>
  /// <returns>A Default PendingOperation</returns>
  member this.ReplaceWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    items
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    let update item =
      let id, pk =
        item >|| AttributeHelpers.getIdFrom
        <| AttributeHelpers.getPartitionKeyFrom
        
      this.container.ReplaceItemAsync(item, id, pk.Key, options, token)
      |> Async.AwaitTask

    Operation
    <| fun _ ->
         match items with
          | [ single ] -> [ update single ]
          | many -> many |> List.map update
         |> AsyncSeq.ofSeqAsync
         |> AsyncSeq.map (fun i -> Response i)

  /// <summary>Replaces an item on the database with the given entity.</summary>
  /// <param name="items">The item(s) to replace.</param>
  member this.ReplaceItem items = this.ReplaceWithOptions None None items

  (* ----------------------- Update ----------------------- *)
  
  member this.UpdateItemsWithOptions
    (requestOptions: PatchItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (operations: UpdateOperations list)
    (itemIds: (string * PartitionKey) list)
    : PendingOperation<'a> =
      
    let translate (op: UpdateOperations) =
      match op with
      | Add expr -> expr |> Expression.parse |> fun i -> PatchOperation.Add(i.Path, i.Value)
      | Remove expr -> expr |> Expression.parse |> fun i -> PatchOperation.Remove(i.Path)
      | Increment expr -> expr |> Expression.parse |> fun i -> PatchOperation.Increment(i.Path, i.Value :?> double)
      
    let options = requestOptions |> Option.toObj
    let token = cancelToken |> getCancelToken
    let translatedOptions = operations |> List.map translate
    
    let patch id key =
      this.container.PatchItemAsync<'a>(id, key, translatedOptions, options, token)
      |> Async.AwaitTask
    
    Operation
    <| fun _ ->
        itemIds
        |> List.map (fun (id, key) -> patch id key)
        |> AsyncSeq.ofSeqAsync
        |> AsyncSeq.map (fun i -> Response i)
  
  member this.UpdateItems
    (operations: UpdateOperations list)
    (itemIds: (string * PartitionKey) list)
    : PendingOperation<'a>
    =
    this.UpdateItemsWithOptions None None operations itemIds
  
  (* ----------------------- GetSingle ----------------------- *)

  /// <summary>
  /// Returns an item from the database. Or an empty list if not found.
  /// </summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="id">The id to search for.</param>
  /// <param name="partitionKey">The partition to search for.</param>
  member this.GetItemWithOptions
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    id
    partitionKey
    : PendingOperation<'a> =
    let options = itemOptions |> Option.toObj
    let token = cancelToken |> getCancelToken

    Operation
    <| fun _ ->
         [ this.container.ReadItemAsync<'a>(id, partitionKey, options, token)
           |> Async.AwaitTask ]
         |> AsyncSeq.ofSeqAsync
         |> AsyncSeq.map (fun i -> Response i)         

  /// <summary>
  /// Returns an item from the database. Or an empty list if not found.
  /// </summary>
  /// <param name="itemOptions">The options to use in this operation.</param>
  /// <param name="cancelToken">The cancellation token to use in this operation.</param>
  /// <param name="item">The item to get the ID and PartitionKey from.</param>
  member this.GetItemWithOptionsFromItem
    (itemOptions: ItemRequestOptions option)
    (cancelToken: CancellationToken option)
    (item: 'b)
    =
    let id, pk =
      item >|| AttributeHelpers.getIdFrom
      <| AttributeHelpers.getPartitionKeyFrom

    this.GetItemWithOptions itemOptions cancelToken id pk.Key

  /// <summary>
  /// Returns an item from the database. Or an empty list if not found.
  /// </summary>
  /// <param name="id">The id to search for.</param>
  /// <param name="partitionKey">The partition to search for.</param>
  member this.GetItem id partitionKey =
    this.GetItemWithOptions None None id partitionKey

  member this.GetItemFromItem(item: 'b) =
    let id, pk =
      item >|| AttributeHelpers.getIdFrom
      <| AttributeHelpers.getPartitionKeyFrom

    this.GetItemWithOptions None None id pk.Key

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

    Operation
    <| fun _ ->
         iterator
         |> AsyncSeq.unfold
              (fun state ->
                match state.HasMoreResults with
                | false -> None
                | true ->
                  let next =
                    state.ReadNextAsync(token)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously

                  Some(Feed next, state))

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

(* ----------------------- Update ----------------------- *)

let updateItems operations ids (container: ConstellationContainer<'a>) = container.UpdateItems operations ids

let updateItemsWithOptions opt cancelToken operations ids (container: ConstellationContainer<'a>) =
  container.UpdateItemsWithOptions opt cancelToken operations ids

(* ----------------------- Replace ----------------------- *)

let replaceWithOptions itemOptions cancelToken item (container: ConstellationContainer<'a>) =
  container.ReplaceWithOptions itemOptions cancelToken item

let replaceItem item (container: ConstellationContainer<'a>) = container.ReplaceItem item

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

let execAsyncWrapped<'a> (pending: PendingOperation<'a>) =
  match pending with
  | Operation op -> op ()

let private matchOnResponse response =
  response
  |> function 
      | Feed r -> r.Resource |> AsyncSeq.ofSeq
      | Response r -> [ r.Resource ] |> AsyncSeq.ofSeq

let execAsync<'a> (pending: PendingOperation<'a>) =
  match pending with
  | Operation op -> asyncSeq { for i in op () do yield! matchOnResponse i }
