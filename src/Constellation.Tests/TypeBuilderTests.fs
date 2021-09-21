﻿namespace Constellation.Tests

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
                          withPreTriggers [ "Some pre-triggers list" ]
                          withPostTriggers [ "Some post-triggers list" ]
                          withIndexingDirective IndexingDirective.Include
                          withConsistencyLevel ConsistencyLevel.BoundedStaleness
                          withSessionToken "Some token"
                          withEnableContentResponseOnWrite true
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
                  <| "These objects should be equal" ]
