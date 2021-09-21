module Constellation.TypeBuilders

open Microsoft.Azure.Cosmos

type ConstellationRequestOptionsBuilder() =
        
    member _.Yield _ = RequestOptions()
    
    member _.Run(options: #RequestOptions) = options
        
    [<CustomOperation("ifMatchEtag")>]
    member _.WithIfMatchEtag(requestOption: #RequestOptions, tag: string) =
        requestOption.IfMatchEtag <- tag
        requestOption
        
    [<CustomOperation("ifNoneMatchEtag")>]
    member _.WithIfNoneMatchEtag(requestOption: #RequestOptions, tag: string) =
        requestOption.IfNoneMatchEtag <- tag
        requestOption
        
let requestOptions = ConstellationRequestOptionsBuilder()

[<Sealed>]
type ConstellationItemRequestOptionsBuilder() =
    inherit ConstellationRequestOptionsBuilder()
    
    member _.Yield _ = ItemRequestOptions()
    
    [<CustomOperation("withPreTriggers")>]
    member _.WithPreTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PreTriggers <- triggers
        options
        
    [<CustomOperation("withPostTriggers")>]
    member _.WithPostTriggers(options: ItemRequestOptions, triggers: string list) =
        options.PostTriggers <- triggers
        options
        
    [<CustomOperation("withIndexingDirective")>]
    member _.WithIndexingDirective(options: ItemRequestOptions, directive: IndexingDirective) =
        options.IndexingDirective <- directive
        options
        
        
    [<CustomOperation("withConsistencyLevel")>]
    member _.WithConsistencyLevel(options: ItemRequestOptions, level: ConsistencyLevel) =
        options.ConsistencyLevel <- level
        options
        
    [<CustomOperation("withSessionToken")>]
    member _.WithSessionToken(options: ItemRequestOptions, token: string) =
        options.SessionToken <- token
        options
        
    [<CustomOperation("withEnableContentResponseOnWrite")>]
    member _.WithEnableContentResponseOnWrite(options: ItemRequestOptions, shouldEnable: bool) =
        options.EnableContentResponseOnWrite <- shouldEnable
        options
        
let itemRequestOptions = ConstellationItemRequestOptionsBuilder()