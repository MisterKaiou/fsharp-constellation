# Release Notes

### [v0.7.0](https://www.nuget.org/packages/FSharp.Constellation/0.7.0)

#### Breaking Change
- ConstellationContainer member methods removed. Reason: maintainability and it was not playing well with type inference. It was a better idea to remove it if not it would get in the way of other functionalities that have been added to the library.

#### Changed
- Changed wrapped types, refactored to simplify usage;
- New wrapper type `PartitionKeys` to wrap around `Cosmos.PartitionKeys` to better represent them in F# code;
- New type `KeyParams` to represent ID strings and PartitionKey arguments;
- Serialization module now require qualified access, changed just for readability;
- Refactored for better type inference;
- The majority of operation now hava a version suffix `Of`. More info on the README file at the end of **_ConstellationContainer_** section;
- Updated README file;

#### Added 
- Introduced implementation of new update (patch) functionality; Old Change/Update methods now renamed to replace. Information on Patch [here](https://docs.microsoft.com/en-us/azure/cosmos-db/partial-document-update);
- Added `filter_predicate` operation to `PatchItemRequestOptionsBuilder`;
- More code documentation!

#### Fixed
- Cleaned up some code;

### [v0.6.3](https://www.nuget.org/packages/FSharp.Constellation/0.6.3)

##### Changed
- A singleton by default CosmosContext is no longer a rule;

##### Fixed
- Fixed a bug where parameterized queries would fail;

##### Added
- More methods have been added to Options builders;
- New method `configureAuditingRequestHandler` on `CosmosContext` allow for logging of the `RequestMessage` body;
