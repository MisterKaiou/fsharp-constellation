namespace Constellation.Tests

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
                 if_match_etag "Some Tag"
                 if_none_match_etag "Some other tag"
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
                 if_match_etag "Some tag"
                 if_none_match_etag "Some other tag"
                 pre_triggers [ "Some pre-triggers list" ]
                 post_triggers [ "Some post-triggers list" ]
                 indexing_directive IndexingDirective.Include
                 consistency_level ConsistencyLevel.BoundedStaleness
                 session_token "Some token"
                 enable_content_response_on_write
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
               changeFeedRequestOptions { page_size_hint 5 }

             Expect.equal subject.PageSizeHint expected.PageSizeHint "These properties should be equal"

        testCase " A ContainerRequestOptions computation expression should return the object as configured"
        <| fun _ ->
             let expected = ContainerRequestOptions()
             expected.PopulateQuotaInfo <- true

             let subject =
               containerRequestOptions { populate_quota_info }

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
                 consistency_level ConsistencyLevel.Eventual
                 enable_low_precision_order_by
                 enable_scan_in_query
                 max_buffered_item_count 5
                 max_concurrency 10
                 max_item_count 20
                 partition_key (PartitionKey("Key"))
                 populate_index_metrics
                 response_continuation_token_limit_in_kb 500
                 session_token "Some token"
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
                 session_token "Some token"
                 consistency_level ConsistencyLevel.Eventual
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
                 session_token "Some token"
                 consistency_level ConsistencyLevel.Eventual
                 enableScript_logging
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
                 enable_content_response_on_write
                 indexing_directive IndexingDirective.Include
               }

             check {
               equal subject.EnableContentResponseOnWrite expected.EnableContentResponseOnWrite
               equal subject.IndexingDirective expected.IndexingDirective
             }
             |> Expect.isTrue
             <| "These objects should be equal"

        testCase " A TransactionalBatchRequestOptions computation expression should return the object as configured"
        <| fun _ ->
             let expected = TransactionalBatchRequestOptions()
             expected.ConsistencyLevel <- ConsistencyLevel.Eventual
             expected.SessionToken <- "Some token"

             let subject =
               transactionalBatchRequestOptions {
                 session_token "Some token"
                 consistency_level ConsistencyLevel.Eventual
               }

             check {
               equal subject.ConsistencyLevel expected.ConsistencyLevel
               equal subject.SessionToken expected.SessionToken
             }
             |> Expect.isTrue
             <| "These objects should be equal"

        testCase
          " A CosmosClientOptions and CosmosSerializationOptions computation expression should return the object as configured"
        <| fun _ ->
             let serializerOptions = CosmosSerializationOptions()
             serializerOptions.Indented <- true
             serializerOptions.IgnoreNullValues <- true
             serializerOptions.PropertyNamingPolicy <- CosmosPropertyNamingPolicy.CamelCase

             let expected = CosmosClientOptions()
             //expected.HttpClientFactory will be left unset since it's a factory

             expected.ConsistencyLevel <- ConsistencyLevel.Eventual
             expected.AllowBulkExecution <- true
             expected.ApplicationName <- "Hello World"
             expected.ApplicationPreferredRegions <- [ "Somewhere west"; "Somewhere South" ]
             expected.ApplicationRegion <- "Middle"
             expected.ConnectionMode <- ConnectionMode.Direct
             expected.EnableContentResponseOnWrite <- true
             expected.EnableTcpConnectionEndpointRediscovery <- true
             expected.GatewayModeMaxConnectionLimit <- 42
             expected.IdleTcpConnectionTimeout <- TimeSpan(0, 0, 69)
             expected.LimitToEndpoint <- true
             expected.MaxRetryAttemptsOnRateLimitedRequests <- 7
             expected.MaxRetryWaitTimeOnRateLimitedRequests <- TimeSpan(0, 0, 13)
             expected.MaxTcpConnectionsPerEndpoint <- 2
             expected.OpenTcpConnectionTimeout <- TimeSpan(0, 0, 24)
             expected.PortReuseMode <- PortReuseMode.PrivatePortPool
             expected.RequestTimeout <- TimeSpan(0, 0, 79)
             expected.SerializerOptions <- serializerOptions

             let subject =
               cosmosClientOptions {
                 consistency_level ConsistencyLevel.Eventual
                 allow_bulk_execution
                 application_name "Hello World"

                 application_preferred_regions [ "Somewhere west"
                                                 "Somewhere South" ]

                 application_region "Middle"
                 connection_mode ConnectionMode.Direct
                 enable_content_response_on_write
                 enable_tcp_connection_endpoint_rediscovery
                 gateway_mode_max_connection_limit 42
                 idle_tcp_connection_timeout (TimeSpan(0, 0, 69))
                 limit_to_endpoint
                 max_retry_attempts_on_rate_limited_requests 7
                 max_retry_wait_time_on_rate_limited_requests (TimeSpan(0, 0, 13))
                 max_tcp_connections_per_endpoint 2
                 open_tcp_connection_timeout (TimeSpan(0, 0, 24))
                 port_reuse_mode PortReuseMode.PrivatePortPool
                 request_timeout (TimeSpan(0, 0, 79))

                 serializer_options (
                   cosmosSerializationOptions {
                     indented
                     ignore_null_values
                     property_naming_policy CosmosPropertyNamingPolicy.CamelCase
                   }
                 )
               }

             check {
               equal subject.ConsistencyLevel expected.ConsistencyLevel
               equal subject.AllowBulkExecution expected.AllowBulkExecution
               equal subject.ApplicationName expected.ApplicationName
               equal subject.ApplicationPreferredRegions expected.ApplicationPreferredRegions
               equal subject.ApplicationRegion expected.ApplicationRegion
               equal subject.ConnectionMode expected.ConnectionMode
               equal subject.EnableContentResponseOnWrite expected.EnableContentResponseOnWrite
               equal subject.EnableTcpConnectionEndpointRediscovery expected.EnableTcpConnectionEndpointRediscovery
               equal subject.GatewayModeMaxConnectionLimit expected.GatewayModeMaxConnectionLimit
               equal subject.IdleTcpConnectionTimeout expected.IdleTcpConnectionTimeout
               equal subject.LimitToEndpoint expected.LimitToEndpoint
               equal subject.MaxRetryAttemptsOnRateLimitedRequests expected.MaxRetryAttemptsOnRateLimitedRequests
               equal subject.MaxRetryWaitTimeOnRateLimitedRequests expected.MaxRetryWaitTimeOnRateLimitedRequests
               equal subject.MaxTcpConnectionsPerEndpoint expected.MaxTcpConnectionsPerEndpoint
               equal subject.OpenTcpConnectionTimeout expected.OpenTcpConnectionTimeout
               equal subject.PortReuseMode expected.PortReuseMode
               equal subject.RequestTimeout expected.RequestTimeout

               is_true (
                 check {
                   equal subject.SerializerOptions.Indented expected.SerializerOptions.Indented
                   equal subject.SerializerOptions.IgnoreNullValues expected.SerializerOptions.IgnoreNullValues
                   equal subject.SerializerOptions.PropertyNamingPolicy expected.SerializerOptions.PropertyNamingPolicy
                 }
               )
             }
             |> Expect.isTrue
             <| "These objects should be equal" ]
