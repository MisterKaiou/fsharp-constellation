/// Contains all the attributes used to ease use and communication with CosmosDB.
module FSharp.Constellation.Attributes

open System
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

///<summary>Decorates a class with a name representing the container ID.</summary>
[<AttributeUsage(AttributeTargets.Class,
                 AllowMultiple = false,
                 Inherited = true)>]
type ContainerAttribute(Name: string) =
  inherit Attribute()
  
  ///The name set for this attribute.
  member this.Name = Name

[<RequireQualifiedAccess>]
module internal AttributeHelpers =

  open FSharp.Reflection
  open System.Reflection

  let private isNullBoxing from = from |> box |> isNull

  let private getPropertiesFrom obj =
    obj
      .GetType()
      .GetProperties(BindingFlags.Public ||| BindingFlags.Instance)

  let private createPathToTargetProperty (targetProperty: PropertyInfo) (rootProperties: PropertyInfo array) = 
    let rec findMatchingProperty (pathState: PropertyInfo array) (currentRoot: PropertyInfo array) : PropertyInfo array =
      currentRoot
      |> Array.tryFind (fun p -> p.PropertyType = targetProperty.DeclaringType)
      |> function
          | None -> 
              currentRoot
              |> Array.where (fun p -> p.PropertyType.IsClass && p.PropertyType <> typeof<string>)
              |> Array.collect (fun c -> findMatchingProperty [| c |] (c.PropertyType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)))
          | Some p -> Array.append pathState [| p |]

    rootProperties
    |> findMatchingProperty [||]

  let private getPropertyValue<'from, 'propType> (root: 'from) (rootProperties: PropertyInfo array) (target: PropertyInfo) : 'propType = 
    let castValue (prop: PropertyInfo) (obj: obj) = prop.GetValue(obj) :?> 'propType

    let rec navigate (root: obj) (propertyPath: PropertyInfo list) =
      propertyPath
      |> function
      | head :: tail -> tail |> navigate (head.GetValue(root))
      | [] -> root
    
    match root.GetType().Name = target.DeclaringType.Name with
    | true -> castValue target root
    | false -> 
      rootProperties
      |> createPathToTargetProperty target
      |> List.ofArray
      |> navigate root
      |> castValue target

  let private getPartitionKeyFromProperty<'a> (parent: 'a) (parentProperties: PropertyInfo array) (prop: PropertyInfo) =
    let getNullPartitionKeyIfNull ifNot input : Nullable<PartitionKey> = 
      if (input |> isNullBoxing) then
        Nullable(PartitionKey.Null)
      else
        ifNot(input)

    let targetPropType = prop.PropertyType

    if (targetPropType = typeof<string>) 
      then getPropertyValue<'a, string> parent parentProperties prop |> getNullPartitionKeyIfNull (fun s -> Nullable(PartitionKey(s)))
    elif (targetPropType = typeof<bool>) 
      then getPropertyValue<'a, bool> parent parentProperties prop |> getNullPartitionKeyIfNull (fun s -> Nullable(PartitionKey(s)))
    elif (targetPropType = typeof<double>) 
      then getPropertyValue<'a, double> parent parentProperties prop |> getNullPartitionKeyIfNull (fun s -> Nullable(PartitionKey(s)))
    else raise (ArgumentException("The type of the PartitionKey property is not supported"))

  let rec private searchFor<'a when 'a :> Attribute> (in': PropertyInfo array) =
    let findId () = 
      in'
      |> Array.choose
           (fun p ->
             p.GetCustomAttribute<'a>()
             |> isNullBoxing
             |> function
                | true -> None
                | false -> Some p
           )

    let findPartitionKey () =
      in'
      |> Array.choose
           (fun p ->
             (p.PropertyType.IsClass && p.PropertyType <> typeof<string> && (FSharpType.IsUnion(p.PropertyType) = false))
             |> function
                  | false -> 
                     p.GetCustomAttribute<'a>()
                     |> isNullBoxing = false
                     |> function
                       | true -> Some p
                       | false -> None
                  | true -> 
                     searchFor<'a> (p.PropertyType.GetProperties())
                     |> Array.tryHead
           )

    if (typeof<'a> = typeof<IdAttribute>) then
      findId()
    else
      findPartitionKey()

  let getPartitionKeyFrom (obj: 'a) =
    let properties = getPropertiesFrom obj

    properties
    |> searchFor<PartitionKeyAttribute>
    |> Array.tryHead
    |> function
      | Some p -> p |> getPartitionKeyFromProperty obj properties
      | None -> Nullable(PartitionKey.None)

  let getIdFrom (obj: 'a) =
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
