namespace FSharp.Constellation.Tests

module AttributeTests =

  open Expecto
  open FSharp.Constellation.Attributes
  open Microsoft.Azure.Cosmos
  open System

  type TestStringType =
    { [<PartitionKey>]
      StringProp: string }

  let private PartitionKeyAttribute_ShouldAllowUsageOnStringProperties () =
    let subject = { StringProp = "Some" }
    let expected = Nullable(PartitionKey("Some"))

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestBooleanType =
    { [<PartitionKey>]
      BooleanProp: bool }

  let private PartitionKeyAttribute_ShouldAllowUsageOnBooleanProperties () =
    let subject = { BooleanProp = true }
    let expected = Nullable(PartitionKey(true))

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestFloatType =
    { [<PartitionKey>]
      FloatProp: double }

  let private PartitionKeyAttribute_ShouldAllowUsageOnFloatProperties () =
    let subject = { FloatProp = 1.0 }
    let expected = Nullable(PartitionKey(1.0))

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestNoAttributeType = { SomeProp: int }

  let private PartitionKeyAttribute_MissingAttributeShouldThrow () =
    let subject = { SomeProp = 1 }

    Expect.throwsT<ArgumentNullException>
    <| (fun _ ->
      AttributeHelpers.getPartitionKeyFrom subject
      |> ignore)
    <| "If no property has PartitionKeyAttribute, then it should throw an exception"

  type TestPropertyWithUnsupportedType =
    { [<PartitionKey>]
      UnsupportedProp: byte }

  let private PartitionKeyAttribute_AttributeOnPropertyWithUnsupportedTypeShouldThrow () =
    let subject = { UnsupportedProp = byte 5 }

    Expect.throwsT<ArgumentException>
    <| (fun _ ->
      AttributeHelpers.getPartitionKeyFrom subject
      |> ignore)
    <| "If the type pf the property that has PartitionKeyAttribute is not bool, string or double, then it should throw an exception"

  [<Tests>]
  let attributeTests =
    testList
      "Attribute Tests"
      [ testCase
          " A Partition Key from a string property should be valid and properly parsed"
          PartitionKeyAttribute_ShouldAllowUsageOnStringProperties

        testCase
          " A Partition Key from a boolean property should be valid and properly parsed"
          PartitionKeyAttribute_ShouldAllowUsageOnBooleanProperties

        testCase
          " A Partition Key from a double property should be valid and properly parsed"
          PartitionKeyAttribute_ShouldAllowUsageOnFloatProperties

        testCase " A property must use the PartitionKeyAttribute" PartitionKeyAttribute_MissingAttributeShouldThrow

        testCase
          " The PartitionKeyAttribute must be used on a property of supported type"
          PartitionKeyAttribute_AttributeOnPropertyWithUnsupportedTypeShouldThrow ]
