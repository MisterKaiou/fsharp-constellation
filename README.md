# Constellation

![Build Status](https://github.com/MisterKaiou/fsharp-constellation/actions/workflows/ci.yml/badge.svg)

## What is Constellation?

Constellation is a wrapper library around parts of the CosmosDB SDK, mainly CRUD operations, to make use with F# more friendly and straight-forward.

## Motivation

The reason why I decided to make this package came from [FSharp.CosmosDb](https://github.com/aaronpowell/FSharp.CosmosDb) (a package by [@aaronpowell](https://github.com/aaronpowell)), [this section](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/performance-tips-dotnet-sdk-v3-sql#sdk-usage) of the Microsoft Performance Tips for CosmosDB, a little bit of the context syntax sugar from Entity Framework, and a personal project that I am working on at the date of writing.

The library that Aaron created is great, but I needed something a little too specific for my use case. So I've decided to create something that can be a little more friendly with DI Frameworks, like ASP.NET's default one, and that does not instantiate multiple instances of the CosmosClient, as suggested in the Tips from Microsoft.

## Goal

The goal is to provide F# syntax to CosmosDB SDK while also allowing to use all the options provided by the SDK.

## How does it work?

It has two wrappers around CosmosDB SDK v3, since v4 is still preview. One around CosmosClient, named `Constellation.CosmosContext` and another one around the Container named `Constellation.Container`. These wrappers are just a proxy to provide you with a more F# friendly syntax to communicate with CosmosDB.

### Attributes

There are only three attributes you might need to use when interacting with this library.
They are on the module `Constellation.Attributes`

#### Id
This attribute marks the property as an id field and will be used to represent an id when needed. Keep in mind that the id field must be a string.
````f#
open Constellation.Attributes

type User =
  { [<Id>] Id: string }
````

#### PartitionKey
For almost all operations the PartitionKey is needed. Fields marked with this attribute can only be of type **string, bool, double or float**:

````f#
open Constellation.Attributes

type User =
  { [<Id>] Id: string
    [<PartitionKey>] Username: string }
````

Or your PartitionKey can be inside a property on your root class. The example below sets
the PartitionKey to one level below (I.e.: In a reference type property of the root class)
but it can be as deep as you need it to. Just keep in mind that reflection is used to
navigate into these properties and get the value; if performance is a concern you might not
want to use this convenience.

````f#
open Constellation.Attributes

type ContactInfo =
  { [<PartitionKey>] Email: string }

type User =
  { [<Id>] Id: string
    Username: string
    ContactInformation: ContactInfo }
````

If your data is partitioned by id, for example, a user is it's own partition, then you can do:

````f#
open Constellation.Attributes

type User =
  { [<Id; PartitionKey>] Id: string
    Username: string }
````

#### Container

This attributes allow you the specify the id of the container right in the model.

````f#
open Constellation.Attributes

[<Container("Users")>]
type User =
  { [<Id; PartitionKey>] Id: string
    Username: string }
````

### Builders

This library exposes _some_ builders for the SDK option models. For the sake of brevity, I won't be going through all the builder methods (there is plenty since the models have lots of properties as well), but be sure that you'll be able to find practically all the method you need by being following the naming convention; that is, every method name corresponds to a option model's property but with words separated by underscores (snake_case), I.e. If a model has a property `SomeProperty` the builder exposes the method `some_property` that takes an input of the same type as the property.

There are currently builder for the following SDK option models:

- `RequestOptions` with name `RequestOptionsBuilder`; instantiate with `requestOptions`
- `ItemRequestOptions` with name `ItemRequestOptionsBuilder`; instantiate with `itemRequestOptions`
- `ChangeFeedRequestOptions` with name `ChangeFeedRequestOptionsBuilder`; instantiate with `changeFeedRequestOptions`
- `ContainerRequestOptions` with name `ContainerRequestOptionsBuilder`; instantiate with `containerRequestOptions`

- `QueryRequestOptions` with name `QueryRequestOptionsBuilder`; instantiate with `queryRequestOptions`

- `StorageProcedureRequestOptions` with name `StorageProcedureRequestOptionsBuilder`; instantiate with `storageProcedureRequestOptions`

- `TransactionalBatchItemRequestOptions` with name `TransactionalBatchItemRequestOptionsBuilder`; instantiate with `transactionalBatchItemRequestOptions`

- `TransactionalBatchRequestOptions` with name `TransactionalBatchRequestOptionsBuilder`; instantiate with `transactionalBatchRequestOptions`

- `CosmosClientOptions` with name `CosmosClientOptionsBuilder`; instantiate with `cosmosClientOptions`

- `CosmosSerializationOptions` with name `CosmosSerializationOptionsBuilder`; instantiate with `cosmosSerializationOptions`

#### Example Usage

````f#
open Constellation.TypeBuilders

let itemOption =
  itemRequestOptions {
    if_match_etag "SomeTag"
    enable_content_response_on_write
  }

//Is the same as doing

let itemOption =
  ItemRequestOptions(
    IfMatchEtag = "SomeTag",
    EnableContentResponseOnWrite = true
  )
````

### CosmosContext

The wrapper around `CosmosClient`, holds a reference to a single `CosmosClient` through multiple `CosmosContext` instances.

By default, this library replaces the default SDK JSON Serializer, for something that better handles F# types; [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson), is used with union encoding set to `JsonUnionEncoding.FSharpLuLike ||| JsonUnionEncoding.NamedFields`. Please, refer to that project if you have any questions about how it works.

To override this behavior, on the constructor send the optional third parameter with your custom serializer set, or send `clientOptions=null` and you'll use the default SDK serializer (Not really recommended with F#).


#### Instantiating the Context - Using the base class
````f#
open Constellation.Context


let context = new CosmosContext("connectionString", "databaseId") //Disposable

//Or

let endpointInfo = { Endpoint = "End/point"; AccountKey = "Key" }
let context = new CosmosContext(endpointInfo, "databaseId") //Disposable

(* Gets the underlying SDK client if you need to do something not yet covered by this library *)
let client = context.Client
````
Keep in mind that **all instances share the same underlying CosmosClient**, and that the CosmosContext is in fact disposable. So, if you are going to use a DI Container, register this class as a singleton. If you dispose, but create a new Context, it will re-instantiate the Client, but until then, all references point to a disposed object, so be mindful of that.
<br>

#### Using the Context - Inheriting from the base class

````f#
open Constellation.Attributes

[<Container("Users")>]
type User = 
  { Name: string }

type MyContext(ConnString: string, DatabaseId: string) =
  inherit CosmosContext(ConnString, DatabaseId)
  
  member this.Users = this.GetContainer<User>()
  
//Or

type MyContext(ConnString: string, DatabaseId: string) =
  inherit CosmosContext(ConnString, DatabaseId)
  
  member this.Users = this.GetContainer<User> "Users"
````
From this example, we see we can achieve some kind of "DbContext". Register this class in your DI container or Composition Root and you should be good to go.

Notice that, the type `CosmosContext` exposes a method `GetContainer<'from>` where you can either send the target ContainerId or a type that uses the attribute `Container(string)`. But note that the type is necessary since it is used on that `ConstellationContainer` instance to map from/to the type specified during interactions with the database.

### ConstellationContainer

This is actually just a wrapper around the SDK `Container` class. As of now, it exposes the main CRUD operations in both "dot" syntax and fluent syntax. Also, the methods exposes a version accepting a SDK option model, that has the suffix _WithOption_, for example, the method `getItem` has a version `getItemWithOptions`. The methods that accepts the models options also accepts a `CancellationToken`. So basically, their signatures are: `OptionModel -> CancellationToken -> arg1 -> argN -> ConstellationContainer<'a> -> PendingOperation<'a>`.

#### What is `PendingOperation<'a>`?

As the name might suggest it is an operation yet to be executed, it is a union of function types that defines operations that returns an `AsyncSeq<WrapperType<'a>>` where `WrapperType<'a>` is one of the SDK response types that wraps a specific operation, for example, the insert operation returns the type `ItemResponse<'a>`; you can see it in more detail in the SDK documentation.

This module `Constellation.Container` exposes two methods for dealing with `PendingOperation<'a>`.

- `Container.execAsync` with signature `PendingOperation<'a> -> AsyncSeq<'a>`
- `Container.execAsyncWrapped` with signature `PendingOperation<'a> -> AsyncSeq<ItemResponse<'a>>`
- `Container.execQueryWrapped` with signature `FluentQuery<'a> -> AsyncSeq<FeedResponse<'a>>`, this method is dedicated to query operations, since queries returns a `FeedResponse<'a>`, therefore if you need the wrapped query result, you have to use this method, trying to use the method `execAsyncWrapped` will throw an exception.


As said before, every operation returns `AsyncSeq<'WrapperType>`, if you want to you can iter through each item as it is used, or do `AsyncSeq.toListSynchronously` to get list of the objects used in the operation. See below usage examples.

For the sake of simplicity, I'll be showcasing only the fluent syntax. But all methods are also available with "dot" syntax.

#### Get

Returns a single result. Even though its return type is an `AsyncSeq`, this sequence will only contain either one item or nothing in it.

````f#
//Gets the container from a model with the attribute Container
let container = ctx |> Context.getContainer<User>

let getResults = //AsyncSeq<User>
  container
  |> Container.getItem "itemId" (PartitionKey("SomeKey"))
  |> Container.execAsync
  
// Or wrapped
  
let getResults = //AsyncSeq<ItemResponse<User>>
  container 
  |> Container.getItem "itemId" (PartitionKey("SomeKey"))
  |> Container.execAsyncWrapped
````

#### Query

Returns a collection of results of the container type that may contain none, one or more items.

````f#
//Gets the container from a model with the attribute Container
let container = ctx |> Context.getContainer<User>

let queryResult = //AsyncSeq<User>
  container
  |> Container.query "SELECT u.Id, u.UserName FROM User u WHERE u.Id LIKE @someId"
  |> Container.withParameters [ ("@someId", box <| "userId") ]
  |> Container.execAsync
  
//Or parameterless

let queryResult = //AsyncSeq<User>
  container
  |> Container.parameterlessQuery "SELECT u.Id, u.UserName FROM User u"
  |> Container.execAsync
  
// Or wrapped
  
let queryResult = //AsyncSeq<FeedResponse<User>>
  container
  |> Container.query "SELECT u.Id, u.UserName FROM User u WHERE u.Id LIKE @someId"
  |> Container.withParameters [ ("@someId", box <| "userId") ]
  |> Container.execQueryWrapped
````

#### Insert

Inserts the given collection in the database.

````f#
//Gets the container from a model with the attribute Container
let container = ctx |> Context.getContainer<User>

let user = { Id = "SomeId"; Username = "User_Name" }

let insertResult = //AsyncSeq<User>
  container
  |> Container.insertItem [ user ]
  |> Container.execAsync
  
// Or wrapped
  
let insertResult = //AsyncSeq<ItemResponse<User>>
  container
  |> Container.insertItem [ user ]
  |> Container.execAsyncWrapped
````

#### Update (Change)

Replaces an item with the same Id and PartitionKey from the Database

```f#
//Gets the container from a model with the attribute Container
let container = ctx |> Context.getContainer<User>

let updateWithThis = { Id = "SomeId"; Username = "UpdatedUserName" }

let updateResult = //AsyncSeq<User>
  container
  |> Container.changeItem updateWithThis
  |> Container.execAsync
  
//Or

let updateResult = //AsyncSeq<ItemResponse<User>>
  container
  |> Container.changeItem updateWithThis
  |> Container.execAsyncWrapped
```

#### Delete

Removes an item from the database.
Note that, if you want to get the resource from the wrapped response, you can't; it is not returned by the database since it has been deleted.
```f#
//Gets the container from a model with the attribute Container
let container = ctx |> Context.getContainer<User>

let deleteThis = { Id = "SomeId"; Username = "UpdatedUserName" }

let deleteResult = //AsyncSeq<User>
  container
  |> Container.deleteItem [ deleteThis ] 
  |> Container.execAsync
  
//Or

let deleteResult = //AsyncSeq<ItemResponse<User>>
  container
  |> Container.deleteItem [ deleteThis ]
  |> Container.execAsyncWrapped
```

## Planned Future Features

- Give support to Transactional Batches
- Give support to Batch operations (Insert, Change, Delete, etc...)
- Update to CosmosDb SDK v4 when it's released