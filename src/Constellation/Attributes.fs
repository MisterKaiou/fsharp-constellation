module Constellation.Attributes

open System
open System.Reflection
open Microsoft.Azure.Cosmos

[<AttributeUsage(AttributeTargets.Field
                 ||| AttributeTargets.Property,
                 AllowMultiple = true,
                 Inherited = true)>]
type PartitionKeyAttribute() =
    inherit Attribute()
    
[<RequireQualifiedAccess>]
module PartitionKeyAttributeHelpers =
    
    let private getPropertyValue (from: obj) (prop: PropertyInfo) =
        let value = prop.GetValue(from)
        
        match value with
        | :? string as s -> Nullable(PartitionKey(s))
        | :? bool as b -> Nullable(PartitionKey(b))
        | :? double as f -> Nullable(PartitionKey(f))
        | _ -> raise (ArgumentException("The type of the PartitionKey property is not supported"))
    
    let getPartitionKeyFromType (obj: 'a) =
        obj.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        |> Array.choose
            (fun p ->
                let partitionKey = p.GetCustomAttribute<PartitionKeyAttribute>()
                
                partitionKey
                |> box
                |> isNull
                |> not
                |> function
                    | true -> Some p
                    | false -> None)
        |> Array.tryHead
        |> function
            | Some p -> p
            | None -> raise (ArgumentNullException(nameof obj, "The given obj does not contain a property or field with PartitionKey Attribute"))
        |> getPropertyValue obj
