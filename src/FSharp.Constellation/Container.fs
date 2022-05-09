/// Define types and functions related to a Cosmos Container.
[<RequireQualifiedAccess>]
module FSharp.Constellation.Container

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

/// <summary> Wrapper around a Container that exposes some of the main CRUD operations for CosmosDB. </summary>
/// <typeparam name="'a"> The type handled by this container. </typeparam>
type ConstellationContainer<'a> =
  /// The Container wrapped by this record.
  | Container of Container

  /// The container wrapped by this instance.
  member this.inner =
    this
    |> function
      | Container c -> c
    
(* ----------------------- Insert ----------------------- *)

/// <summary>Inserts (a) new item(s) with the options specified.</summary>
/// <param name="itemOptions">The options to use in this operation.</param>
/// <param name="cancelToken">The cancellation token to use in this operation.</param>
/// <param name="items">The items to insert.</param>
/// <param name="container">The container from which insert.</param>
/// <returns>A PendingOperation of the <paramref name="container"/> type.</returns>
let insertItemWithOptions itemOptions cancelToken items (container: ConstellationContainer<'a>) =
  let options = itemOptions |> Option.toObj
  let token = cancelToken |> getCancelToken

  let getPk this =
    AttributeHelpers.getPartitionKeyFrom this  
    |> fun it -> it.Key

  let createItem item =
    let pk = getPk item

    container.inner.CreateItemAsync(item, pk, options, token)  
    |> Async.AwaitTask

  Operation
  <| fun _ ->
       items  
       |> List.map createItem  
       |> AsyncSeq.ofSeqAsync  
       |> AsyncSeq.map (fun i -> Response i )  

/// <summary>Inserts a new item(s). </summary>
/// <param name="items">The items(s) to insert. </param>
/// <param name="container">The container from which to insert. </param>
/// <returns>A PendingOperation of the <paramref name="container"/> type. </returns>
/// <remarks> Same as calling insertItemWithOptions with no options and no CancellationToken. </remarks>
let rec insertItems items (container: ConstellationContainer<'a>) =
  insertItemWithOptions None None items container

(* ----------------------- Delete ----------------------- *)

/// <summary> Delete an item(s) by its id and PartitionKey with options and a cancellation token. </summary>
/// <param name="itemOptions"> The options to use in this operation. </param>
/// <param name="cancelToken"> The cancellation token to use in this operation. </param>
/// <param name="keys"> A list with KeyParams to be used in the search. </param>
/// <param name="container">The container from which to delete. </param>
/// <returns> A PendingOperation. </returns>
let deleteItemByIdWithOptions itemOptions cancelToken keys (container: ConstellationContainer<'a>) =
  let options = itemOptions |> Option.toObj
  let token = cancelToken |> getCancelToken

  let deleteItem (item: string * PartitionKeys) =
    let id, pk = item

    container.inner.DeleteItemAsync(id, pk.Key, options, token) 
    |> Async.AwaitTask
  
  Operation
  <| fun _ ->
       keys 
       |> List.map (getKeyAndId >> deleteItem) 
       |> AsyncSeq.ofSeqAsync 
       |> AsyncSeq.map (fun i -> Response i)

/// <summary> Deletes an item(s) by its id and PartitionKey. </summary>
/// <param name="keys"> A list of KeyParams to be used in the search. </param>
/// <param name="container">The container from which to delete. </param>
/// <remarks> Same as calling deleteItemByIdWithOptions with no itemOptions and no CancellationToken. </remarks>
/// <returns> A PendingOperation. </returns>
let deleteItemById keys (container: ConstellationContainer<'a>) =
  deleteItemByIdWithOptions None None keys container

/// <summary> Deletes an item from CosmosDB from an item(s) id and PartitionKey. </summary>
/// <param name="itemOptions"> The options to use in this operation. </param>
/// <param name="cancelToken"> The CancellationToken to use in this operation. </param>
/// <param name="items"> A list with the items from which to take the id and PartitionKey to use in the search. </param>
/// <param name="container"> The container from which to delete. </param>
/// <returns>A PendingOperation. </returns>
let deleteItemWithOptions itemOptions cancelToken items (container: ConstellationContainer<'a>) =
  items 
  |> List.map
       (fun l ->
         let id, partitionKey =
           l
           >|| AttributeHelpers.getIdFrom
           <| AttributeHelpers.getPartitionKeyFrom

         IdAndKey (id, partitionKey))
  |> fun l -> deleteItemByIdWithOptions itemOptions cancelToken l container

/// <summary> Deletes an item from the database retrieving the id and PartitionKey from another item. </summary>
/// <param name="items"> A list the items from which take the id and PartitionKey to be used in the search. </param>
/// <param name="container"> The container from which to delete. </param>
/// <returns>A PendingOperation. </returns>
/// <remarks> Same as calling deleteItemWithOptions with no options and no CancellationToken. </remarks>
let deleteItem items (container: ConstellationContainer<'a>) =
  deleteItemWithOptions None None items container

(* ----------------------- Update ----------------------- *)

/// <summary> Applies the given <paramref name="operations"/> on <paramref name="ids"/>. Alternatively, you can also
/// provide the <paramref name="options"/> to be used in the request, and a <paramref name="cancelToken"/> for
/// cancellation requests. </summary>
/// <param name="options"> The options to use on the request, if any. </param>
/// <param name="cancelToken"> The CancellationToken to provide for the Cosmos SDK. </param>
/// <param name="firstCharAsIs"> Flag indicating if the property names parsed from the quotations should be kept as is
/// or if the first character should be lowered. </param>
/// <param name="operations"> The operations to send to CosmosDB. </param>
/// <param name="ids"> The ID(s) on which run the operations. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of type <typeparamref name="'b"/>. </returns>
let updateItemsWithOptions<'a, 'b> options cancelToken firstCharAsIs operations ids (container: ConstellationContainer<'a>) =
  let parser =
    match firstCharAsIs with
    | true -> Expression.parseAsIs
    | false -> Expression.parseFirstLowered
    
  let translate (op: UpdateOperations) =
    match op with
    | Add expr -> expr |> parser |> fun i -> PatchOperation.Add(i.Path, i.Value)
    | Remove expr -> expr |> parser |> fun i -> PatchOperation.Remove(i.Path)
    | Increment expr -> expr |> parser |> fun i -> PatchOperation.Increment(i.Path, i.Value :?> double)
    
  let options = options |> Option.toObj
  let token = cancelToken |> getCancelToken
  let translatedOptions = operations |> List.map translate
  
  let patch id (key: PartitionKeys) =
    container.inner.PatchItemAsync<'b>(id, key.Key, translatedOptions, options, token)
    |> Async.AwaitTask
      
  Operation
  <| fun _ ->
      ids
      |> List.map (fun p -> p |> getKeyAndId ||> patch)
      |> AsyncSeq.ofSeqAsync
      |> AsyncSeq.map (fun i -> Response i)

/// <summary> Applies the given <paramref name="operations"/> on <paramref name="ids"/>. </summary>
/// <param name="operations"> The operations to send to CosmosDB. </param>
/// <param name="ids"> The ID(s) on which run the operations. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
/// <remarks> Same as calling updateItemsWithOptions with no options, no cancelToken and property name with first characters lowered. </remarks>
let updateItems operations ids (container: ConstellationContainer<'a>) =
  updateItemsWithOptions<'a, 'a> None None false operations ids container

/// <summary> Applies the given <paramref name="operations"/> on <paramref name="ids"/>. </summary>
/// <param name="operations"> The operations to send to CosmosDB. </param>
/// <param name="ids"> The ID(s) on which run the operations. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <typeparam name="'a"> The type handled by the container. </typeparam>
/// <typeparam name="'b"> The type as which read the response resource. </typeparam>
/// <returns> A PendingOperation whose resource value will be of type <typeparamref name="'b"/>. </returns>
/// <remarks> Same as calling updateItemsWithOptions with no options, no cancelToken and property name with first characters lowered. </remarks>
let updateItemsOf<'a, 'b> operations ids (container: ConstellationContainer<'a>) =
  updateItemsWithOptions<'a, 'b> None None false operations ids container

(* ----------------------- Replace ----------------------- *)

/// <summary> Replace an entry in the database for the object provided. </summary>
/// <param name="itemOptions"> The options to use in this operation. </param>
/// <param name="cancelToken"> The CancellationToken to use in this operation. </param>
/// <param name="items"> The item(s) to replace. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
let replaceWithOptions itemOptions cancelToken items (container: ConstellationContainer<'a>) =
  let options = itemOptions |> Option.toObj
  let token = cancelToken |> getCancelToken

  let replace item =
    let id, pk =
      item >|| AttributeHelpers.getIdFrom
      <| AttributeHelpers.getPartitionKeyFrom
  
    container.inner.ReplaceItemAsync(item, id, pk.Key, options, token) 
    |> Async.AwaitTask

  Operation
  <| fun _ ->
       items 
       |> List.map replace 
       |> AsyncSeq.ofSeqAsync 
       |> AsyncSeq.map (fun i -> Response i)

/// <summary> Replaces an item on the database with the given entity. </summary>
/// <param name="items"> The item(s) to replace. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
/// <remarks> Same as calling replaceWithOptions with no itemOptions and no CancellationToken</remarks>
let replaceItem items (container: ConstellationContainer<'a>) =
  replaceWithOptions None None items container

(* ----------------------- GetSingle ----------------------- *)

/// <summary>
/// Returns an item from the database. Or an empty list if not found.
/// </summary>
/// <param name="itemOptions"> The options to use in this operation. </param>
/// <param name="cancelToken"> The cancellation token to use in this operation. </param>
/// <param name="keys"> The keys to use in the search. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
let getItemWithOptions
  itemOptions
  cancelToken
  keys
  (container: ConstellationContainer<'a>)
  =
  let options = itemOptions |> Option.toObj
  let token = cancelToken |> getCancelToken
  let id, key = keys |> getKeyAndId

  Operation
  <| fun _ ->
       container.inner.ReadItemAsync<'a>(id, key.Key, options, token) 
       |> (Async.AwaitTask >> fun a -> [ a ]) 
       |> AsyncSeq.ofSeqAsync 
       |> AsyncSeq.map (fun i -> Response i)
       
/// <summary>
/// Returns an item from the database. Or an empty list if not found.
/// </summary>
/// <param name="itemOptions"> The options to use in this operation. </param>
/// <param name="cancelToken"> The cancellation token to use in this operation. </param>
/// <param name="item"> The item to get the ID and PartitionKey from. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
let getItemWithOptionsFromItem
  itemOptions
  cancelToken
  item
  (container: ConstellationContainer<'a>)
  =
  let keys =
    item >|| AttributeHelpers.getIdFrom
    <| AttributeHelpers.getPartitionKeyFrom
    |> IdAndKey
  
  getItemWithOptions itemOptions cancelToken keys container

/// <summary>
/// Returns an item from the database. Or an empty list if not found.
/// </summary>
/// <param name="item"> The item to get the ID and PartitionKey from. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
/// <remarks> Same as calling getItemWithOptionsFromItem with no itemOptions and no CancellationToken. </remarks>
let getItemFromItem item (container: ConstellationContainer<'a>) =
  getItemWithOptionsFromItem None None item container

/// <summary>
/// Returns an item from the database. Or an empty list if not found.
/// </summary>
/// <param name="keys"> The keys to use in the search. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource value will be of the same type of the Container. </returns>
let getItem keys (container: ConstellationContainer<'a>) =
  getItemWithOptions None None keys container 

(* ----------------------- Query ----------------------- *)

/// <summary> Sends a query request to CosmosDB. Alternatively accepts QueryRequestOptions, a CancellationToken and a
/// continuation token. </summary>
/// <param name="queryOptions"> The options to be passed to the SDK to use on the query. </param>
/// <param name="cancelToken"> A CancellationToken to pass to the SDK. </param>
/// <param name="continuationToken"> A continuation token to be passed by the SDK to CosmosDB. </param>
/// <param name="query"> A query record with the query text and parameters to be used. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <typeparam name="'a"> The type handled by the container. </typeparam>
/// <typeparam name="'b"> The type as which read the response resource. </typeparam>
/// <returns> A PendingOperation whose resource will be of type <typeparamref name="'b"/>. </returns>
let queryItemsWithOptions<'a, 'b>
  queryOptions
  cancelToken
  continuationToken
  query
  (container: ConstellationContainer<'a>)
  =
  let definition =
    query.Parameters
    |> List.fold (fun (def: QueryDefinition) -> def.WithParameter) (QueryDefinition query.Query)

  let options = queryOptions |> Option.toObj
  let token = cancelToken |> getCancelToken
  let contToken = continuationToken |> Option.toObj

  use iterator =
    container.inner.GetItemQueryIterator<'b>(definition, contToken, options)

  Operation
  <| fun _ ->
       iterator
       |> AsyncSeq.unfold
            (fun state ->
              match state.HasMoreResults with
              | false -> None
              | true ->
                Async.AwaitTask >> Async.RunSynchronously >> Feed
                <| state.ReadNextAsync(token)
                |> fun f -> Some(f, state))

/// A model that represents the process of building a query.
type FluentQuery<'a> =
  { /// The container to be used for the query.
    Container: ConstellationContainer<'a>
    /// The query text. 
    QueryText: string
    /// The parameters to be used in the query.
    Parameters: QueryParam list }

/// <summary> Prepares a query that will have parameters added to it. </summary>
/// <param name="queryText"> The text to be used in the query. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A FluentQuery used to build the final query that will be executed on the <paramref name="container"/>. </returns>
let query queryText (container: ConstellationContainer<'a>) =
  { Container = container
    QueryText = queryText
    Parameters = [] }

/// <summary> Runs a query on <paramref name="container"/> that has no parameters. </summary>
/// <param name="queryText"> The text to be used in the query. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <returns> A PendingOperation whose resource type will be of the same type handled by the <paramref name="container"/>. </returns>
let parameterlessQuery queryText (container: ConstellationContainer<'a>) =
  let q = { Query = queryText; Parameters = [] }
  queryItemsWithOptions<'a, 'a> None None None q container

/// <summary> Runs a query on <paramref name="container"/> that has no parameters and reads the response resource
/// as type <typeparamref name="'b"/></summary>
/// <param name="queryText"> The text to be used in the query. </param>
/// <param name="container"> The container from which send the requests. </param>
/// <typeparam name="'a"> The type handled by the container. </typeparam>
/// <typeparam name="'b"> The type as which read the response resource. </typeparam>
let parameterlessQueryOf<'a, 'b> queryText (container: ConstellationContainer<'a>) =
  let q = { Query = queryText; Parameters = [] }
  queryItemsWithOptions<'a, 'b> None None None q container

/// <summary> Adds to <paramref name="query"/> the parameters provided. </summary>
/// <param name="params'"> The parameters to be used in the query. </param>
/// <param name="query"> The query from which build the final operation with parameters added to it. </param>
/// <returns> A PendingOperation whose resource type will be of the same type as the container provided to query. </returns>
let withParameters params' (query: FluentQuery<'a>) =
  let fq = { query with Parameters = params' }

  let q =
    { Query = fq.QueryText
      Parameters = fq.Parameters }

  queryItemsWithOptions<'a, 'a> None None None q fq.Container

/// <summary> Adds to <paramref name="query"/> the parameters provided. </summary>
/// <param name="params'">The parameters to be used in the query.</param>
/// <param name="query"> The query from which build the final operation with parameters added to it. </param>
/// <typeparam name="'a"> The type handled by the container. </typeparam>
/// <typeparam name="'b"> The type as which read the response resource. </typeparam>
/// <returns> A PendingOperation whose resource type will be of type <typeparamref name="'b"/>. </returns>
let withParametersOf<'a, 'b> params' (query: FluentQuery<'a>) =
  let fq = { query with Parameters = params' }

  let q =
    { Query = fq.QueryText
      Parameters = fq.Parameters }

  queryItemsWithOptions<'a, 'b> None None None q fq.Container

(* ----------------------- Execution ----------------------- *)

/// <summary> Executes the PendingOperation, returning from it with the default types used by the SDK. </summary>
/// <param name="pending"> The operation to execute. </param>
/// <returns> The response wrapped by the default types used by the SDK which contain relevant information about resource
/// usage and other metrics. </returns>
let execAsyncWrapped (pending: PendingOperation<'a>) =
  match pending with
  | Operation op -> op ()

let private matchOnResponse response =
  response
  |> function 
      | Feed r -> r.Resource |> AsyncSeq.ofSeq
      | Response r -> [ r.Resource ] |> AsyncSeq.ofSeq

/// <summary> Execute the PendingOperation returning the inner resource from the SDK types. </summary>
/// <param name="pending"> The operation to execute. </param>
/// <returns> A AsyncSeq of the type specified by the PendingOperation being executed. </returns>
let execAsync (pending: PendingOperation<'a>) =
  match pending with
  | Operation op -> asyncSeq { for i in op () do yield! matchOnResponse i }
