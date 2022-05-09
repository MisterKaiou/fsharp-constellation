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

/// <summary> Represents how the Constellation library sees the responses returned by the SDK. </summary>
/// <typeparam name="'a"> The type of the resource contained in the response. </typeparam>
type CosmosResponse<'a> =
  /// A Response of type <typeparamref name="'a"/>.
  | Response of Response<'a>
  /// A FeedResponse of type <typeparamref name="'a"/>, usually returned by queries or operations that supports continuation.
  | Feed of FeedResponse<'a>

/// <summary>Represents an operation yet to be executed.</summary>
/// <typeparam name="'a">The type returned by this operation.</typeparam>
type PendingOperation<'a> = Operation of (unit -> AsyncSeq<CosmosResponse<'a>>)

/// <summary> Represents operations that can be sent to Azure. </summary>
/// <remarks> More info: https://docs.microsoft.com/en-us/azure/cosmos-db/partial-document-update#supported-operations </remarks>
type UpdateOperations =
  /// Operation to add (replace) a (new) field value on the document. 
  | Add of Expr
  /// Operation similar to Add, but if its a valid array index the entry is updated.
  | Set of Expr
  /// Operation similar to Set, but if the field is not found it returns an error.
  | Replace of Expr
  /// Removed a field from the document, if not found returns an error. If it is a valid array index, the entry is removed 
  | Remove of Expr
  /// <summary> Increments the value of the field by the value returned by the expression. A whole float number results in an integer
  /// on CosmosDB. </summary>
  /// <remarks> If, say, you increment 1 by 0.5 only then the number would become a float. If you increment again by
  /// -0.5 (basically decrementing) the number would become an integer again. </remarks>
  | Increment of Expr<double>
