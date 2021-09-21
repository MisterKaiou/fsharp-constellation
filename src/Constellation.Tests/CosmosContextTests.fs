module Tests

open Expecto
open Constellation.Context
open System

[<Literal>] 
let private connString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="

let private CosmosContext_Dispose_ShouldReflectOnAllInstances() = 
    use subjectOne = new CosmosContext(connString, "SomeDb")
    use subjectTwo = new CosmosContext(connString, "OtherDb")
    (subjectOne :> IDisposable).Dispose()
    
    Expect.throwsT<ObjectDisposedException>
      <| (fun _ -> subjectOne.Client.ClientOptions |> ignore) 
      <| "Any operation should throw ObjectDiposedException on all instances"
    
    Expect.throwsT<ObjectDisposedException>
      <| (fun _ -> subjectTwo.Client.ClientOptions |> ignore) 
      <| "Any operation should throw ObjectDiposedException on all instances"


let private CosmosContext_CosmosClient_ShouldBeSameInstanceOnAllCosmosContextInstances() =
    use subjectOne = new CosmosContext(connString, "SomeDb")
    use subjectTwo = new CosmosContext(connString, "OtherDb")

    Expect.isTrue 
      <| (obj.ReferenceEquals(subjectOne.Client, subjectTwo.Client)) 
      <| "The CosmosClient instances should be the same throughout all CosmosContextInstances"

(*  These tests almost looks like we are testing CLR behaviour, but that's not the case here. 
    Here we are guaranteeing that the disposing logic behaves the way intended, that is:
        - All instances must share the same client
        - All instances, when the client is disposed from any of them, would lose reference to the client
*)
[<Tests>]
let cosmosContextTests =
    testSequenced <| testList "CosmosContext Tests" [
        testCase
            " A disposed Constellation Context should reflect on all instances" 
            CosmosContext_Dispose_ShouldReflectOnAllInstances
        
        testCase
            " Only a single CosmosClient instance should exist"
            CosmosContext_CosmosClient_ShouldBeSameInstanceOnAllCosmosContextInstances
    ]