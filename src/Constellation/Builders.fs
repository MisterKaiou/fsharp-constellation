module Constellation.TypeBuilders

open System
open Microsoft.Azure.Cosmos

type RequestOptionsBuilder() =

    member _.Yield _ = RequestOptions()

    member _.Run(options: #RequestOptions) = options

    [<CustomOperation("ifMatchEtag")>]
    member _.WithIfMatchEtag(requestOption: #RequestOptions, tag) =
        requestOption.IfMatchEtag <- tag
        requestOption

    [<CustomOperation("ifNoneMatchEtag")>]
    member _.WithIfNoneMatchEtag(requestOption: #RequestOptions, tag) =
        requestOption.IfNoneMatchEtag <- tag
        requestOption
        
    [<CustomOperation("properties")>]
    member _.WithProperties(requestOption: #RequestOptions, properties) =
        requestOption.Properties <- properties
        requestOption

let requestOptions = RequestOptionsBuilder()

[<Sealed>]
type ItemRequestOptionsBuilder() =
    inherit RequestOptionsBuilder()

    member _.Yield _ = ItemRequestOptions()

    [<CustomOperation("preTriggers")>]
    member _.WithPreTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PreTriggers <- triggers
        options

    [<CustomOperation("postTriggers")>]
    member _.WithPostTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PostTriggers <- triggers
        options

    [<CustomOperation("indexingDirective")>]
    member _.WithIndexingDirective(options: ItemRequestOptions, directive: IndexingDirective) =
        options.IndexingDirective <- directive
        options

    [<CustomOperation("consistencyLevel")>]
    member _.WithConsistencyLevel(options: ItemRequestOptions, level: ConsistencyLevel) =
        options.ConsistencyLevel <- level
        options

    [<CustomOperation("sessionToken")>]
    member _.WithSessionToken(options: ItemRequestOptions, token: string) =
        options.SessionToken <- token
        options

    [<CustomOperation("enableContentResponseOnWrite")>]
    member _.WithEnableContentResponseOnWrite(options: ItemRequestOptions, shouldEnable: bool) =
        options.EnableContentResponseOnWrite <- shouldEnable
        options

let itemRequestOptions = ItemRequestOptionsBuilder()
