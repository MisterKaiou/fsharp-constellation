/// Contains all the attributes used to ease use and communication with CosmosDB.
module Constellation.Attributes

open System
open System.Reflection
open Microsoft.Azure.Cosmos

///<summary>Flags a field or property to be used as a PartitionKey for operations on CosmosDB.</summary>
///<remarks>Only string, bool and double can be set as PartitionKey.</remarks>
[<AttributeUsage(AttributeTargets.Field
                 ||| AttributeTargets.Property,
                 AllowMultiple = false,
                 Inherited = true)>]
type PartitionKeyAttribute() =
  inherit Attribute()

///<summary>Flags a field or property to be used as a ID for operations on CosmosDB.</summary>
///<remarks>Only string fields/properties may be used as a key for CosmosDB.</remarks>
[<AttributeUsage(AttributeTargets.Field
                 ||| AttributeTargets.Property,
                 AllowMultiple = false,
                 Inherited = true)>]
type IdAttribute() =
  inherit Attribute()

[<AttributeUsage(AttributeTargets.Class,
                 AllowMultiple = false,
                 Inherited = true)>]

///<summary>Decorates a class with a name representing the container ID.</summary>
type ContainerAttribute(Name: string) =
  inherit Attribute()
  
  ///The name set for this attribute.
  member this.Name = Name

[<RequireQualifiedAccess>]
module internal AttributeHelpers =

  let private isNullBoxing from = from |> box |> isNull

  let private partitionKeyFromProperty (from: obj) (prop: PropertyInfo) =
    let value = prop.GetValue(from)

    if isNullBoxing value then
      Nullable<PartitionKey>()
    else
      match value with
      | :? string as s -> Nullable(PartitionKey(s))
      | :? bool as b -> Nullable(PartitionKey(b))
      | :? double as f -> Nullable(PartitionKey(f))
      | _ -> raise (ArgumentException("The type of the PartitionKey property is not supported"))

  let private getPropertiesFrom obj =
    obj
      .GetType()
      .GetProperties(BindingFlags.Public ||| BindingFlags.Instance)

  let private searchFor<'a when 'a :> Attribute> (in': PropertyInfo array) =
    in'
    |> Array.choose
         (fun p ->
           p.GetCustomAttribute<'a>() |> isNullBoxing = false
           |> function
             | true -> Some p
             | false -> None)

  let getPartitionKeyFrom (obj: 'a) =
    getPropertiesFrom obj
    |> searchFor<PartitionKeyAttribute>
    |> Array.tryHead
    |> function
      | Some p -> p
      | None ->
        raise (
          ArgumentNullException(
            nameof obj,
            "The given obj does not contain a property or field with PartitionKey Attribute"
          )
        )
    |> partitionKeyFromProperty obj

  let getIdFromTypeFrom (obj: 'a) =
    getPropertiesFrom obj
    |> searchFor<IdAttribute>
    |> Array.tryHead
    |> function
      | Some p -> p
      | None ->
        raise (
          ArgumentNullException(nameof obj, "The given obj does not contain a property or field with Id Attribute")
        )
    |> (fun p ->
      match p.GetValue(obj) with
      | :? string as s -> s
      | _ -> raise (ArgumentException(nameof obj, "The given object had an invalid Id field, must be of type string")))

  let getContainerIdFromType<'a> =
    let attr =
      typeof<'a>.GetCustomAttribute<ContainerAttribute>(false)
      
    attr
    |> isNullBoxing
    |> function
       | false -> attr.Name
       | true ->
          raise (
            ArgumentNullException(
              nameof obj,
              "The given obj does not contain a property or field with PartitionKey Attribute"
            )
          )
