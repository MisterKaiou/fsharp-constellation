# Release Notes

### [v0.6.3](https://www.nuget.org/packages/FSharp.Constellation/0.6.3)

##### Changed
- A singleton by default CosmosContext is no longer a rule

##### Fixed
- Fixed a bug where parameterized queries would fail

##### Added
- More methods have been added to Options builders
- New method `configureAuditingRequestHandler` on `CosmosContext` allow for logging of the `RequestMessage` body.
