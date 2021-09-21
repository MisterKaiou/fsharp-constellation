﻿namespace Constellation.Tests

open System
open Microsoft.Azure.Cosmos.Scripts

module TypeBuilderTests =

    open Constellation.Tests.CustomBuilders
    open Constellation.TypeBuilders
    open Expecto
    open Microsoft.Azure.Cosmos

    [<Tests>]
    let typeBuildersTests =
        testList
            "Type Builders Tests"
            [ testCase " A RequestOptions computation expression should return a RequestOption as configured"
              <| fun _ ->
                  let expected = RequestOptions()
                  expected.IfMatchEtag <- "Some Tag"
                  expected.IfNoneMatchEtag <- "Some other tag"

                  let subject =
                      requestOptions {
                          ifMatchEtag "Some Tag"
                          ifNoneMatchEtag "Some other tag"
                      }

                  check {
                      equal subject.IfMatchEtag expected.IfMatchEtag
                      equal subject.IfNoneMatchEtag expected.IfNoneMatchEtag
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal"

              testCase " A ItemRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = ItemRequestOptions()
                  expected.IfMatchEtag <- "Some tag"
                  expected.IfNoneMatchEtag <- "Some other tag"
                  expected.ConsistencyLevel <- ConsistencyLevel.BoundedStaleness
                  expected.IndexingDirective <- IndexingDirective.Include
                  expected.PostTriggers <- [ "Some post-triggers list" ]
                  expected.PreTriggers <- [ "Some pre-triggers list" ]
                  expected.SessionToken <- "Some token"
                  expected.EnableContentResponseOnWrite <- true

                  let subject =
                      itemRequestOptions {
                          ifMatchEtag "Some tag"
                          ifNoneMatchEtag "Some other tag"
                          preTriggers [ "Some pre-triggers list" ]
                          postTriggers [ "Some post-triggers list" ]
                          indexingDirective IndexingDirective.Include
                          consistencyLevel ConsistencyLevel.BoundedStaleness
                          sessionToken "Some token"
                          enableContentResponseOnWrite
                      }

                  check {
                      equal subject.IfMatchEtag expected.IfMatchEtag
                      equal subject.IfNoneMatchEtag expected.IfNoneMatchEtag
                      equal subject.ConsistencyLevel expected.ConsistencyLevel
                      equal subject.IndexingDirective expected.IndexingDirective
                      equal subject.PostTriggers expected.PostTriggers
                      equal subject.PreTriggers expected.PreTriggers
                      equal subject.SessionToken expected.SessionToken
                      equal subject.EnableContentResponseOnWrite expected.EnableContentResponseOnWrite
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal"

              testCase " A ChangeFeedRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = ChangeFeedRequestOptions()
                  expected.PageSizeHint <- 5

                  let subject =
                      changeFeedRequestOptions { pageSizeHint 5 }

                  Expect.equal subject.PageSizeHint expected.PageSizeHint "These properties should be equal"

              testCase " A ContainerRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = ContainerRequestOptions()
                  expected.PopulateQuotaInfo <- true

                  let subject =
                      containerRequestOptions { populateQuota }

                  Expect.equal subject.PopulateQuotaInfo expected.PopulateQuotaInfo "These properties should be equal"

              testCase " A QueryRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = QueryRequestOptions()
                  expected.ConsistencyLevel <- ConsistencyLevel.Eventual
                  expected.EnableLowPrecisionOrderBy <- true
                  expected.EnableScanInQuery <- true
                  expected.MaxBufferedItemCount <- 5
                  expected.MaxConcurrency <- 10
                  expected.MaxItemCount <- 20
                  expected.PartitionKey <- PartitionKey("Key")
                  expected.PopulateIndexMetrics <- true
                  expected.ResponseContinuationTokenLimitInKb <- 500
                  expected.SessionToken <- "Some token"

                  let subject =
                      queryRequestOptions {
                          consistencyLevel ConsistencyLevel.Eventual
                          enableLowPrecisionOrderBy
                          enableScanInQuery
                          maxBufferedItemCount 5
                          maxConcurrency 10
                          maxItemCount 20
                          partitionKey (PartitionKey("Key"))
                          populateIndexMetrics
                          responseContinuationTokenLimitInKb 500
                          sessionToken "Some token"
                      }

                  check {
                      equal subject.ConsistencyLevel expected.ConsistencyLevel
                      equal subject.EnableLowPrecisionOrderBy expected.EnableLowPrecisionOrderBy
                      equal subject.EnableScanInQuery expected.EnableScanInQuery
                      equal subject.MaxBufferedItemCount expected.MaxBufferedItemCount
                      equal subject.MaxConcurrency expected.MaxConcurrency
                      equal subject.MaxItemCount expected.MaxItemCount
                      equal subject.PartitionKey expected.PartitionKey
                      equal subject.PopulateIndexMetrics expected.PopulateIndexMetrics
                      equal subject.ResponseContinuationTokenLimitInKb expected.ResponseContinuationTokenLimitInKb
                      equal subject.SessionToken expected.SessionToken
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal"

              testCase " A ReadManyRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = ReadManyRequestOptions()
                  expected.SessionToken <- "Some token"
                  expected.ConsistencyLevel <- ConsistencyLevel.Eventual

                  let subject =
                      readManyRequestOptions {
                          sessionToken "Some token"
                          consistencyLevel ConsistencyLevel.Eventual
                      }

                  check {
                      equal subject.SessionToken expected.SessionToken
                      equal subject.ConsistencyLevel expected.ConsistencyLevel
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal"
                  
              testCase " A StoredProcedureRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = StoredProcedureRequestOptions()
                  expected.SessionToken <- "Some token"
                  expected.ConsistencyLevel <- ConsistencyLevel.Eventual
                  expected.EnableScriptLogging <- true

                  let subject =
                      storageProcedureRequestOptions {
                          sessionToken "Some token"
                          consistencyLevel ConsistencyLevel.Eventual
                          enableScriptLogging
                      }

                  check {
                      equal subject.SessionToken expected.SessionToken
                      equal subject.ConsistencyLevel expected.ConsistencyLevel
                      equal subject.EnableScriptLogging expected.EnableScriptLogging
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal"
                  
              testCase " A TransactionalBatchItemRequestOptions computation expression should return the object as configured"
              <| fun _ ->
                  let expected = TransactionalBatchItemRequestOptions()
                  expected.EnableContentResponseOnWrite <- true
                  expected.IndexingDirective <- IndexingDirective.Include

                  let subject =
                      transactionalBatchItemRequestOptions {
                          enableContentResponseOnWrite
                          indexingDirective IndexingDirective.Include
                      }

                  check {
                      equal subject.EnableContentResponseOnWrite expected.EnableContentResponseOnWrite
                      equal subject.IndexingDirective expected.IndexingDirective
                  }
                  |> Expect.isTrue
                  <| "These objects should be equal" ]
