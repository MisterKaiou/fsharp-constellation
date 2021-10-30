#r "paket:  
nuget Fake.Core.Target
nuget Fake.DotNet.MSBuild
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

let buildDir = "./build/"
let testDir = "./test/"

Target.create "Clean" (fun _ ->
  Shell.cleanDirs [buildDir; testDir]
)

Target.create "BuildLib" (fun _ ->
  !! "src/Constellation/*.fsproj"
    |> MSBuild.runRelease id buildDir "Build"
    |> Trace.logItems "LibBuild-Output: "
)

Target.create "BuildTest" (fun _ ->
  !! "src/Constellation.Tests/*.fsproj"
    |> MSBuild.runDebug id testDir "Build"
    |> Trace.logItems "TestBuild-Output: "
)

Target.create "Test" (fun _ ->
  !! (testDir + "/Constellation.Tests.dll")
    |> Expecto.run id
)

Target.create "Default" (fun _ ->
  Trace.trace "Hello from FAKE!"
)

"Clean"
  ==> "BuildLib"
  ==> "BuildTest"
  ==> "Test"

Target.runOrDefault "Test"