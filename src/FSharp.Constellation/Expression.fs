module FSharp.Constellation.Expression

open System.Reflection
open Microsoft.FSharp.Core
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

type UpdateInfo =
  { Path: string
    Value: obj }

let rec parse (expr: Expr) =
  let getValueOfSome (instance: Expr) (info: PropertyInfo) s =
    let innerInfo = instance |> parse
    let f = info.GetValue(innerInfo.Value)
    { Path = $"{innerInfo.Path}/{info.Name}"
      Value = f }
  
  let getValueOfNone (info: PropertyInfo) =
    let f = info.GetValue(null, null)
    { Path = ""
      Value = f }
  
  let parseBuildingPath (expr: Expr) (path: string) =
    match expr with
    | PropertyGet (Some instance, pInfo, []) -> getValueOfSome instance pInfo path
    | PropertyGet (None, pInfo, []) -> getValueOfNone pInfo
    | _ -> failwith "Not supported"

  parseBuildingPath expr "/"
