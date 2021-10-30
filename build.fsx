#r "paket:  
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Testing.Expecto
nuget FSharp.Core 5.0.0"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.Testing

let sln = "FSharp.Constellation.sln"
let projName = "FSharp.Constellation"
let testProjName = "FSharp.Constellation.Tests"
let mainProjDir = $"./src/{projName}/"
let testProjDir = $"./src/{testProjName}/"

let clean = "Clean"
let buildLib = "BuildLib"
let runTest = "RunTests"
let all = "All"

Target.create clean (fun _ ->
  !! "./src/**/bin"
  ++ "./src/**/obj"
  |> Shell.cleanDirs 
)

Target.create buildLib (fun _ ->
    DotNet.build id sln 
)

Target.create runTest (fun _ ->
  !! $"{testProjDir}bin/Release/net5.0/{testProjName}.dll"
    |> Expecto.run id
)

Target.create all ignore

clean
  ==> buildLib
  ==> runTest
  ==> all

Target.runOrDefault all