module Constellation.TypeBuilders

open System
open Microsoft.Azure.Cosmos

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
    member inline _.WithEnableContentResponseOnWrite(options: ItemRequestOptions, shouldEnable: bool) =
        options.EnableContentResponseOnWrite <- shouldEnable
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