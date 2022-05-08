module FSharp.Constellation.Expression

open System
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

type UpdateInfo =
  { Path: string
    Value: obj }

let rec parseCapitalized (expr: Expr) =
  let getValueOfSome (instance: Expr) (info: PropertyInfo) s =
    let innerInfo = instance |> parseCapitalized
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

let parse (expr: Expr) =
  let updateInfo = expr |> parseCapitalized
  updateInfo.Path
  |> fun s -> s.Split([|"/"|], StringSplitOptions.RemoveEmptyEntries)
  |> Array.map (fun s -> $"/{Char.ToLower(s[0])}{s.Substring(1)}")
  |> String.concat ""
  |> fun s -> { updateInfo with Path = s }
