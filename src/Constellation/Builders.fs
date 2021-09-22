module Constellation.TypeBuilders

open Microsoft.Azure.Cosmos
open Microsoft.Azure.Cosmos.Scripts

type RequestOptionsBuilder() =

    member inline _.Yield _ = RequestOptions()

    member inline _.Run(options: #RequestOptions) = options

    [<CustomOperation("ifMatchEtag")>]
    member inline _.WithIfMatchEtag(requestOption: #RequestOptions, tag) =
        requestOption.IfMatchEtag <- tag
        requestOption

    [<CustomOperation("ifNoneMatchEtag")>]
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

    [<CustomOperation("preTriggers")>]
    member inline _.WithPreTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PreTriggers <- triggers
        options

    [<CustomOperation("postTriggers")>]
    member inline _.WithPostTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PostTriggers <- triggers
        options

    [<CustomOperation("indexingDirective")>]
    member inline _.WithIndexingDirective(options: ItemRequestOptions, directive: IndexingDirective) =
        options.IndexingDirective <- directive
        options

    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(options: ItemRequestOptions, level: ConsistencyLevel) =
        options.ConsistencyLevel <- level
        options

    [<CustomOperation("sessionToken")>]
    member inline _.WithSessionToken(options: ItemRequestOptions, token: string) =
        options.SessionToken <- token
        options

    [<CustomOperation("enableContentResponseOnWrite")>]
    member inline _.WithEnableContentResponseOnWrite(options: ItemRequestOptions) =
        options.EnableContentResponseOnWrite <- true
        options

let itemRequestOptions = ItemRequestOptionsBuilder()

[<Sealed>]
type ChangeFeedRequestOptionsBuilder() =

    member inline _.Yield _ = ChangeFeedRequestOptions()

    member inline _.Run(last) = last

    [<CustomOperation("pageSizeHint")>]
    member inline _.WithPageSizeHint(options: ChangeFeedRequestOptions, pageSize) =
        options.PageSizeHint <- pageSize
        options

let changeFeedRequestOptions = ChangeFeedRequestOptionsBuilder()

[<Sealed>]
type ContainerRequestOptionsBuilder() =
    inherit RequestOptionsBuilder()

    member inline _.Yield _ = ContainerRequestOptions()

    [<CustomOperation("populateQuota")>]
    member inline _.WithQuota(opt: ContainerRequestOptions) =
        opt.PopulateQuotaInfo <- true
        opt

let containerRequestOptions = ContainerRequestOptionsBuilder()

[<Sealed>]
type QueryRequestOptionsBuilder() =
    inherit RequestOptionsBuilder()

    member inline _.Yield _ = QueryRequestOptions()

    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(opt: QueryRequestOptions, level) =
        opt.ConsistencyLevel <- level
        opt

    [<CustomOperation("enableLowPrecisionOrderBy")>]
    member inline _.WithEnableLowPrecisionOrderBy(opt: QueryRequestOptions) =
        opt.EnableLowPrecisionOrderBy <- true
        opt

    [<CustomOperation("enableScanInQuery")>]
    member inline _.WithEnableScanInQuery(opt: QueryRequestOptions) =
        opt.EnableScanInQuery <- true
        opt

    [<CustomOperation("maxBufferedItemCount")>]
    member inline _.WithMaxBufferedItemCount(opt: QueryRequestOptions, count) =
        opt.MaxBufferedItemCount <- count
        opt

    [<CustomOperation("maxConcurrency")>]
    member inline _.WithMaxConcurrency(opt: QueryRequestOptions, count) =
        opt.MaxConcurrency <- count
        opt

    [<CustomOperation("maxItemCount")>]
    member inline _.WithMaxItemCount(opt: QueryRequestOptions, count) =
        opt.MaxItemCount <- count
        opt

    [<CustomOperation("partitionKey")>]
    member inline _.WithPartitionKey(opt: QueryRequestOptions, partitionKey) =
        opt.PartitionKey <- partitionKey
        opt

    [<CustomOperation("populateIndexMetrics")>]
    member inline _.WithPopulateIndexMetrics(opt: QueryRequestOptions) =
        opt.PopulateIndexMetrics <- true
        opt

    [<CustomOperation("responseContinuationTokenLimitInKb")>]
    member inline _.WithResponseContinuationTokenLimitInKb(opt: QueryRequestOptions, limit) =
        opt.ResponseContinuationTokenLimitInKb <- limit
        opt

    [<CustomOperation("sessionToken")>]
    member inline _.WithSessionToken(opt: QueryRequestOptions, token) =
        opt.SessionToken <- token
        opt

let queryRequestOptions = QueryRequestOptionsBuilder()

type ReadManyRequestOptionsBuilder() =
    inherit RequestOptions()

    member inline _.Yield _ = ReadManyRequestOptions()

    [<CustomOperation("sessionToken")>]
    member inline _.WithSessionToken(opt: ReadManyRequestOptions, token) =
        opt.SessionToken <- token
        opt

    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(opt: ReadManyRequestOptions, level) =
        opt.ConsistencyLevel <- level
        opt

let readManyRequestOptions = ReadManyRequestOptionsBuilder()

type StorageProcedureRequestOptionsBuilder() =
    inherit RequestOptions()
    
    member inline _.Yield _ = StoredProcedureRequestOptions()
    
    [<CustomOperation("sessionToken")>]
    member inline _.WithSessionToken(opt: StoredProcedureRequestOptions, token) =
        opt.SessionToken <- token
        opt
        
    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(opt: StoredProcedureRequestOptions, level) =
        opt.ConsistencyLevel <- level
        opt

    [<CustomOperation("enableScriptLogging")>]
    member inline _.WithEnableScriptLogging(opt: StoredProcedureRequestOptions) =
        opt.EnableScriptLogging <- true
        opt
        
let storageProcedureRequestOptions = StorageProcedureRequestOptionsBuilder()

type TransactionalBatchItemRequestOptionsBuilder() =
    inherit RequestOptionsBuilder()
    
    member inline _.Yield _ = TransactionalBatchItemRequestOptions()
    
    [<CustomOperation("enableContentResponseOnWrite")>]
    member inline _.WithEnableContentResponseOnWrite(opt: TransactionalBatchItemRequestOptions) =
        opt.EnableContentResponseOnWrite <- true
        opt
        
    [<CustomOperation("indexingDirective")>]
    member inline _.WithIndexingDirective(opt: TransactionalBatchItemRequestOptions, directive) =
        opt.IndexingDirective <- directive
        opt
        
let transactionalBatchItemRequestOptions = TransactionalBatchItemRequestOptionsBuilder()

type TransactionalBatchRequestOptionsBuilder() =
    inherit RequestOptionsBuilder()
    
    member inline _.Yield _ = TransactionalBatchRequestOptions()
    
    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(opt: TransactionalBatchRequestOptions, level) =
        opt.ConsistencyLevel <- level
        opt

    [<CustomOperation("sessionToken")>]
    member inline _.WithSessionToken(opt: TransactionalBatchRequestOptions, token) =
        opt.SessionToken <- token
        opt
        
let transactionalBatchRequestOptions = TransactionalBatchRequestOptionsBuilder()


type CosmosClientOptionsBuilder() =
    
    member inline _.Yield _ = CosmosClientOptions()
    
    member inline _.Run(options: CosmosClientOptions) = options
    
    [<CustomOperation("allowBulkExecution")>]
    member inline _.WithAllowBulkExecution(opt: CosmosClientOptions) =
        opt.AllowBulkExecution <- true
        opt
        
    [<CustomOperation("applicationName")>]
    member inline _.WithApplicationName(opt: CosmosClientOptions, name) =
        opt.ApplicationName <- name
        opt
        
    [<CustomOperation("applicationPreferredRegions")>]
    member inline _.WithApplicationPreferredRegions(opt: CosmosClientOptions, regions: string list) =
        opt.ApplicationPreferredRegions <- regions
        opt
        
    [<CustomOperation("applicationRegion")>]
    member inline _.WithApplicationRegion(opt: CosmosClientOptions, region) =
        opt.ApplicationRegion <- region
        opt
        
    [<CustomOperation("connectionMode")>]
    member inline _.WithConnectionMode(opt: CosmosClientOptions, mode) =
        opt.ConnectionMode <- mode
        opt
        
    [<CustomOperation("consistencyLevel")>]
    member inline _.WithConsistencyLevel(opt: CosmosClientOptions, level) =
        opt.ConsistencyLevel <- level
        opt
        
    [<CustomOperation("enableContentResponseOnWrite")>]
    member inline _.WithEnableContentResponseOnWrite(opt: CosmosClientOptions) =
        opt.EnableContentResponseOnWrite <- true
        opt
        
    [<CustomOperation("enableTcpConnectionEndpointRediscovery")>]
    member inline _.WithEnableTcpConnectionEndpointRediscovery(opt: CosmosClientOptions) =
        opt.EnableTcpConnectionEndpointRediscovery <- true
        opt
        
    [<CustomOperation("gatewayModeMaxConnectionLimit")>]
    member inline _.WithGatewayModeMaxConnectionLimit(opt: CosmosClientOptions, limit) =
        opt.GatewayModeMaxConnectionLimit <- limit
        opt
        
    [<CustomOperation("httpClientFactory")>]
    member inline _.WithHttpClientFactory(opt: CosmosClientOptions, func) =
        opt.HttpClientFactory <- func
        opt
        
    [<CustomOperation("idleTcpConnectionTimeout")>]
    member inline _.WithIdleTcpConnectionTimeout(opt: CosmosClientOptions, timeoutIn) =
        opt.IdleTcpConnectionTimeout <- timeoutIn
        opt
        
    [<CustomOperation("limitToEndpoint")>]
    member inline _.WithLimitToEndpoint(opt: CosmosClientOptions) =
        opt.LimitToEndpoint <- true
        opt
        
    [<CustomOperation("maxRequestsPerTcpConnection")>]
    member inline _.WithMaxRequestsPerTcpConnection(opt: CosmosClientOptions, maxCount) =
        opt.MaxRequestsPerTcpConnection <- maxCount
        opt
        
    [<CustomOperation("maxRetryAttemptsOnRateLimitedRequests")>]
    member inline _.WithMaxRetryAttemptsOnRateLimitedRequests(opt: CosmosClientOptions, maxCount) =
        opt.MaxRetryAttemptsOnRateLimitedRequests <- maxCount
        opt

    [<CustomOperation("maxRetryWaitTimeOnRateLimitedRequests")>]
    member inline _.WithMaxRetryWaitTimeOnRateLimitedRequests(opt: CosmosClientOptions, maxWait) =
        opt.MaxRetryWaitTimeOnRateLimitedRequests <- maxWait
        opt

            
    [<CustomOperation("maxTcpConnectionsPerEndpoint")>]
    member inline _.WithMaxTcpConnectionsPerEndpoint(opt: CosmosClientOptions, maxCount) =
        opt.MaxTcpConnectionsPerEndpoint <- maxCount
        opt

    [<CustomOperation("openTcpConnectionTimeout")>]
    member inline _.WithOpenTcpConnectionTimeout(opt: CosmosClientOptions, timeoutIn) =
        opt.OpenTcpConnectionTimeout <- timeoutIn
        opt

    [<CustomOperation("portReuseMode")>]
    member inline _.WithPortReuseMode(opt: CosmosClientOptions, mode) =
        opt.PortReuseMode <- mode
        opt
        
    [<CustomOperation("requestTimeout")>]
    member inline _.WithRequestTimeout(opt: CosmosClientOptions, timeoutIn) =
        opt.RequestTimeout <- timeoutIn
        opt
        
    [<CustomOperation("serializer")>]
    member inline _.WithSerializer(opt: CosmosClientOptions, serializer) =
        opt.Serializer <- serializer
        opt
        
    [<CustomOperation("serializerOptions")>]
    member inline _.WithSerializerOptions(opt: CosmosClientOptions, options) =
        opt.SerializerOptions <- options
        opt

let cosmosClientOptions = CosmosClientOptionsBuilder()

type CosmosSerializationOptionsBuilder() =
    
    member inline _.Yield _ = CosmosSerializationOptions()
    
    member inline _.Run(options: CosmosSerializationOptions) = options
    
    [<CustomOperation("ignoreNullValues")>]
    member inline _.WithIgnoreNullValues(opt: CosmosSerializationOptions) =
        opt.IgnoreNullValues <- true
        opt

    [<CustomOperation("indented")>]
    member inline _.WithIndented(opt: CosmosSerializationOptions) =
        opt.Indented <- true
        opt

    [<CustomOperation("propertyNamingPolicy")>]
    member inline _.WithPropertyNamingPolicy(opt: CosmosSerializationOptions, policy) =
        opt.PropertyNamingPolicy <- policy
        opt

let cosmosSerializationOptions = CosmosSerializationOptionsBuilder()