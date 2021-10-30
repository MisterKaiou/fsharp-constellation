/// Contains builders for the CosmosSDK option types.
module FSharp.Constellation.TypeBuilders

open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Scripts

type RequestOptionsBuilder() =

  member inline _.Yield _ = RequestOptions()

  member inline _.Run(options: #RequestOptions) = options

  [<CustomOperation("if_match_etag")>]
  member inline _.WithIfMatchEtag(requestOption: #RequestOptions, tag) =
    requestOption.IfMatchEtag <- tag
    requestOption

  [<CustomOperation("if_none_match_etag")>]
  member inline _.WithIfNoneMatchEtag(requestOption: #RequestOptions, tag) =
    requestOption.IfNoneMatchEtag <- tag
    requestOption

  [<CustomOperation("properties")>]
  member inline _.WithProperties(requestOption: #RequestOptions, properties) =
    requestOption.Properties <- properties
    requestOption

let requestOptions = RequestOptionsBuilder()

[<Sealed>]
type ItemRequestOptionsBuilder() =
  inherit RequestOptionsBuilder()

  member inline _.Yield _ = ItemRequestOptions()

  [<CustomOperation("pre_triggers")>]
  member inline _.WithPreTriggers(options: ItemRequestOptions, triggers: string list) =
    options.PreTriggers <- triggers
    options

  [<CustomOperation("post_triggers")>]
  member inline _.WithPostTriggers(options: ItemRequestOptions, triggers: string list) =
    options.PostTriggers <- triggers
    options

  [<CustomOperation("indexing_directive")>]
  member inline _.WithIndexingDirective(options: ItemRequestOptions, directive: IndexingDirective) =
    options.IndexingDirective <- directive
    options

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(options: ItemRequestOptions, level: ConsistencyLevel) =
    options.ConsistencyLevel <- level
    options

  [<CustomOperation("session_token")>]
  member inline _.WithSessionToken(options: ItemRequestOptions, token: string) =
    options.SessionToken <- token
    options

  [<CustomOperation("enable_content_response_on_write")>]
  member inline _.WithEnableContentResponseOnWrite(options: ItemRequestOptions) =
    options.EnableContentResponseOnWrite <- true
    options

let itemRequestOptions = ItemRequestOptionsBuilder()

[<Sealed>]
type ChangeFeedRequestOptionsBuilder() =

  member inline _.Yield _ = ChangeFeedRequestOptions()

  member inline _.Run(last) = last

  [<CustomOperation("page_size_hint")>]
  member inline _.WithPageSizeHint(options: ChangeFeedRequestOptions, pageSize) =
    options.PageSizeHint <- pageSize
    options

let changeFeedRequestOptions = ChangeFeedRequestOptionsBuilder()

[<Sealed>]
type ContainerRequestOptionsBuilder() =
  inherit RequestOptionsBuilder()

  member inline _.Yield _ = ContainerRequestOptions()

  [<CustomOperation("populate_quota_info")>]
  member inline _.WithQuota(opt: ContainerRequestOptions) =
    opt.PopulateQuotaInfo <- true
    opt

let containerRequestOptions = ContainerRequestOptionsBuilder()

[<Sealed>]
type QueryRequestOptionsBuilder() =
  inherit RequestOptionsBuilder()

  member inline _.Yield _ = QueryRequestOptions()

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(opt: QueryRequestOptions, level) =
    opt.ConsistencyLevel <- level
    opt

  [<CustomOperation("enable_low_precision_order_by")>]
  member inline _.WithEnableLowPrecisionOrderBy(opt: QueryRequestOptions) =
    opt.EnableLowPrecisionOrderBy <- true
    opt

  [<CustomOperation("enable_scan_in_query")>]
  member inline _.WithEnableScanInQuery(opt: QueryRequestOptions) =
    opt.EnableScanInQuery <- true
    opt

  [<CustomOperation("max_buffered_item_count")>]
  member inline _.WithMaxBufferedItemCount(opt: QueryRequestOptions, count) =
    opt.MaxBufferedItemCount <- count
    opt

  [<CustomOperation("max_concurrency")>]
  member inline _.WithMaxConcurrency(opt: QueryRequestOptions, count) =
    opt.MaxConcurrency <- count
    opt

  [<CustomOperation("max_item_count")>]
  member inline _.WithMaxItemCount(opt: QueryRequestOptions, count) =
    opt.MaxItemCount <- count
    opt

  [<CustomOperation("partition_key")>]
  member inline _.WithPartitionKey(opt: QueryRequestOptions, partitionKey) =
    opt.PartitionKey <- partitionKey
    opt

  [<CustomOperation("populate_index_metrics")>]
  member inline _.WithPopulateIndexMetrics(opt: QueryRequestOptions) =
    opt.PopulateIndexMetrics <- true
    opt

  [<CustomOperation("response_continuation_token_limit_in_kb")>]
  member inline _.WithResponseContinuationTokenLimitInKb(opt: QueryRequestOptions, limit) =
    opt.ResponseContinuationTokenLimitInKb <- limit
    opt

  [<CustomOperation("session_token")>]
  member inline _.WithSessionToken(opt: QueryRequestOptions, token) =
    opt.SessionToken <- token
    opt

let queryRequestOptions = QueryRequestOptionsBuilder()

[<Sealed>]
type ReadManyRequestOptionsBuilder() =
  inherit RequestOptions()

  member inline _.Yield _ = ReadManyRequestOptions()

  [<CustomOperation("session_token")>]
  member inline _.WithSessionToken(opt: ReadManyRequestOptions, token) =
    opt.SessionToken <- token
    opt

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(opt: ReadManyRequestOptions, level) =
    opt.ConsistencyLevel <- level
    opt

let readManyRequestOptions = ReadManyRequestOptionsBuilder()

[<Sealed>]
type StorageProcedureRequestOptionsBuilder() =
  inherit RequestOptions()

  member inline _.Yield _ = StoredProcedureRequestOptions()

  [<CustomOperation("session_token")>]
  member inline _.WithSessionToken(opt: StoredProcedureRequestOptions, token) =
    opt.SessionToken <- token
    opt

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(opt: StoredProcedureRequestOptions, level) =
    opt.ConsistencyLevel <- level
    opt

  [<CustomOperation("enableScript_logging")>]
  member inline _.WithEnableScriptLogging(opt: StoredProcedureRequestOptions) =
    opt.EnableScriptLogging <- true
    opt

let storageProcedureRequestOptions = StorageProcedureRequestOptionsBuilder()

[<Sealed>]
type TransactionalBatchItemRequestOptionsBuilder() =
  inherit RequestOptionsBuilder()

  member inline _.Yield _ = TransactionalBatchItemRequestOptions()

  [<CustomOperation("enable_content_response_on_write")>]
  member inline _.WithEnableContentResponseOnWrite(opt: TransactionalBatchItemRequestOptions) =
    opt.EnableContentResponseOnWrite <- true
    opt

  [<CustomOperation("indexing_directive")>]
  member inline _.WithIndexingDirective(opt: TransactionalBatchItemRequestOptions, directive) =
    opt.IndexingDirective <- directive
    opt

let transactionalBatchItemRequestOptions =
  TransactionalBatchItemRequestOptionsBuilder()

[<Sealed>]
type TransactionalBatchRequestOptionsBuilder() =
  inherit RequestOptionsBuilder()

  member inline _.Yield _ = TransactionalBatchRequestOptions()

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(opt: TransactionalBatchRequestOptions, level) =
    opt.ConsistencyLevel <- level
    opt

  [<CustomOperation("session_token")>]
  member inline _.WithSessionToken(opt: TransactionalBatchRequestOptions, token) =
    opt.SessionToken <- token
    opt

let transactionalBatchRequestOptions =
  TransactionalBatchRequestOptionsBuilder()

[<Sealed>]
type CosmosClientOptionsBuilder() =

  member inline _.Yield _ = CosmosClientOptions()

  member inline _.Run(options: CosmosClientOptions) = options

  [<CustomOperation("allow_bulk_execution")>]
  member inline _.WithAllowBulkExecution(opt: CosmosClientOptions) =
    opt.AllowBulkExecution <- true
    opt

  [<CustomOperation("application_name")>]
  member inline _.WithApplicationName(opt: CosmosClientOptions, name) =
    opt.ApplicationName <- name
    opt

  [<CustomOperation("application_preferred_regions")>]
  member inline _.WithApplicationPreferredRegions(opt: CosmosClientOptions, regions: string list) =
    opt.ApplicationPreferredRegions <- regions
    opt

  [<CustomOperation("application_region")>]
  member inline _.WithApplicationRegion(opt: CosmosClientOptions, region) =
    opt.ApplicationRegion <- region
    opt

  [<CustomOperation("connection_mode")>]
  member inline _.WithConnectionMode(opt: CosmosClientOptions, mode) =
    opt.ConnectionMode <- mode
    opt

  [<CustomOperation("consistency_level")>]
  member inline _.WithConsistencyLevel(opt: CosmosClientOptions, level) =
    opt.ConsistencyLevel <- level
    opt

  [<CustomOperation("enable_content_response_on_write")>]
  member inline _.WithEnableContentResponseOnWrite(opt: CosmosClientOptions) =
    opt.EnableContentResponseOnWrite <- true
    opt

  [<CustomOperation("enable_tcp_connection_endpoint_rediscovery")>]
  member inline _.WithEnableTcpConnectionEndpointRediscovery(opt: CosmosClientOptions) =
    opt.EnableTcpConnectionEndpointRediscovery <- true
    opt

  [<CustomOperation("gateway_mode_max_connection_limit")>]
  member inline _.WithGatewayModeMaxConnectionLimit(opt: CosmosClientOptions, limit) =
    opt.GatewayModeMaxConnectionLimit <- limit
    opt

  [<CustomOperation("http_client_factory")>]
  member inline _.WithHttpClientFactory(opt: CosmosClientOptions, func) =
    opt.HttpClientFactory <- func
    opt

  [<CustomOperation("idle_tcp_connection_timeout")>]
  member inline _.WithIdleTcpConnectionTimeout(opt: CosmosClientOptions, timeoutIn) =
    opt.IdleTcpConnectionTimeout <- timeoutIn
    opt

  [<CustomOperation("limit_to_endpoint")>]
  member inline _.WithLimitToEndpoint(opt: CosmosClientOptions) =
    opt.LimitToEndpoint <- true
    opt

  [<CustomOperation("max_requests_per_tcp_connection")>]
  member inline _.WithMaxRequestsPerTcpConnection(opt: CosmosClientOptions, maxCount) =
    opt.MaxRequestsPerTcpConnection <- maxCount
    opt

  [<CustomOperation("max_retry_attempts_on_rate_limited_requests")>]
  member inline _.WithMaxRetryAttemptsOnRateLimitedRequests(opt: CosmosClientOptions, maxCount) =
    opt.MaxRetryAttemptsOnRateLimitedRequests <- maxCount
    opt

  [<CustomOperation("max_retry_wait_time_on_rate_limited_requests")>]
  member inline _.WithMaxRetryWaitTimeOnRateLimitedRequests(opt: CosmosClientOptions, maxWait) =
    opt.MaxRetryWaitTimeOnRateLimitedRequests <- maxWait
    opt


  [<CustomOperation("max_tcp_connections_per_endpoint")>]
  member inline _.WithMaxTcpConnectionsPerEndpoint(opt: CosmosClientOptions, maxCount) =
    opt.MaxTcpConnectionsPerEndpoint <- maxCount
    opt

  [<CustomOperation("open_tcp_connection_timeout")>]
  member inline _.WithOpenTcpConnectionTimeout(opt: CosmosClientOptions, timeoutIn) =
    opt.OpenTcpConnectionTimeout <- timeoutIn
    opt

  [<CustomOperation("port_reuse_mode")>]
  member inline _.WithPortReuseMode(opt: CosmosClientOptions, mode) =
    opt.PortReuseMode <- mode
    opt

  [<CustomOperation("request_timeout")>]
  member inline _.WithRequestTimeout(opt: CosmosClientOptions, timeoutIn) =
    opt.RequestTimeout <- timeoutIn
    opt

  [<CustomOperation("serializer")>]
  member inline _.WithSerializer(opt: CosmosClientOptions, serializer) =
    opt.Serializer <- serializer
    opt

  [<CustomOperation("serializer_options")>]
  member inline _.WithSerializerOptions(opt: CosmosClientOptions, options) =
    opt.SerializerOptions <- options
    opt

let cosmosClientOptions = CosmosClientOptionsBuilder()

[<Sealed>]
type CosmosSerializationOptionsBuilder() =

  member inline _.Yield _ = CosmosSerializationOptions()

  member inline _.Run(options: CosmosSerializationOptions) = options

  [<CustomOperation("ignore_null_values")>]
  member inline _.WithIgnoreNullValues(opt: CosmosSerializationOptions) =
    opt.IgnoreNullValues <- true
    opt

  [<CustomOperation("indented")>]
  member inline _.WithIndented(opt: CosmosSerializationOptions) =
    opt.Indented <- true
    opt

  [<CustomOperation("property_naming_policy")>]
  member inline _.WithPropertyNamingPolicy(opt: CosmosSerializationOptions, policy) =
    opt.PropertyNamingPolicy <- policy
    opt

let cosmosSerializationOptions = CosmosSerializationOptionsBuilder()
