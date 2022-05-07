/// Define custom types used in operations with Cosmos Container.
module FSharp.Constellation.Models

open Microsoft.FSharp.Quotations
open Microsoft.Azure.Cosmos
open FSharp.Control

/// Define types that represent keys used by the Cosmos Container.
module Keys =
  
  /// Represents all keys supported by operations on CosmosDB.
  type PartitionKeys =
  /// A partition key of string.
  | StringKey of string
  /// A partition key of a boolean value.
  | BooleanKey of bool
  /// A partition key of numeric value.
  | NumericKey of double
  /// Represents a key that allows for operations providing no partition key.
  | NoKey
  /// Represents a key that allows for operations providing a null partition key.
  | Null

    /// The actual PartitionKey from this type. 
    member k.Key =
      match k with
      | StringKey s -> PartitionKey(s)
      | BooleanKey b -> PartitionKey(b)
      | NumericKey n -> PartitionKey(n)
      | NoKey -> PartitionKey.None
      | Null -> PartitionKey.Null
  
  /// Represents a choice, between a PartitionKey that also is the ID of a document in a Container. 
  type KeyParam =
  /// The PartitionKey and document ID share the same value.
  | SameIdPartition of string
  /// The PartitionKey and document ID have different values.
  | IdAndKey of string * PartitionKeys

/// Represents a parameter used in queries. Where left is the parameter to replace (with '@') and right is the parameter value.
type QueryParam = string * obj

/// Represents a query to execute.
type Query =
  { /// The query's text.
    Query: string
    /// A list with all the parameters to replace on the query's text.
    Parameters: QueryParam list }

type CosmosResponse<'a> = 
  | Response of Response<'a>
  | Feed of FeedResponse<'a>

/// <summary>Represents an operation yet to be executed.</summary>
/// <typeparam name="'a">The type returned by this operation.</typeparam>
type PendingOperation<'a> = Operation of (unit -> AsyncSeq<CosmosResponse<'a>>)

type UpdateOperations =
  | Add of Expr
  | Remove of Expr
  | Increment of Expr<double>
