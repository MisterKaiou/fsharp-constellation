namespace FSharp.Constellation.Tests

module AttributeTests =

  open Expecto
  open FSharp.Constellation.Attributes
  open FSharp.Constellation.Models.Keys
  open Microsoft.Azure.Cosmos
  open System

  type TestStringType =
    { [<PartitionKey>]
      StringProp: string }

  let private PartitionKeyAttribute_ShouldAllowUsageOnStringProperties () =
    let subject = { StringProp = "Some" }
    let expected = StringKey "Some"

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestBooleanType =
    { [<PartitionKey>]
      BooleanProp: bool }

  let private PartitionKeyAttribute_ShouldAllowUsageOnBooleanProperties () =
    let subject = { BooleanProp = true }
    let expected = BooleanKey true

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestFloatType =
    { [<PartitionKey>]
      FloatProp: double }

  let private PartitionKeyAttribute_ShouldAllowUsageOnFloatProperties () =
    let subject = { FloatProp = 1.0 }
    let expected = NumericKey 1.0

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "Final result must be equal to expected value"

  type TestNoAttributeType = { SomeProp: int }

  let private PartitionKeyAttribute_MissingAttributeShouldUsePartitionKeyNone () =
    let subject = { SomeProp = 1 }

    let result =
      AttributeHelpers.getPartitionKeyFrom subject
    
    Expect.equal 
      result
      NoKey 
      "When no PartitionKeyAttribute is present, PartitionKey.None should be used"

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

  type TestTypeWithManyPropertiesOfTheSameType =
    { MustIgnoreThis: string
      [<PartitionKey>] Goal: string }

  type TestTypeWithClassProperty =
   { StringType: TestTypeWithManyPropertiesOfTheSameType }
  
  type Dummy =
    { SomeNumber: int }

  type TestRootClass =
    { TestType: TestTypeWithClassProperty
      MustIgnoreThisProperty: string
      SomeDummy: Dummy }

  let private PartitionKeyAttribute_ShouldAllowAttributeOnClassTypeProperty () =
    let subject = { TestType = { StringType = { Goal = "Some"; MustIgnoreThis = "Should not be picked up" } }; MustIgnoreThisProperty = ""; SomeDummy = { SomeNumber = 1 } }
    let expected = StringKey "Some"

    let result =
      AttributeHelpers.getPartitionKeyFrom subject

    Expect.equal result expected "A PartitionKey should be found even if on a Reference Type Property."

  type TestIdType =
    { [<Id>] SomeIdProperty: string }

  let private AnIdAttributeShouldBeValidOnAStringProperty () =
    let expectedId = Guid.NewGuid().ToString("N")
    let subject = { SomeIdProperty = expectedId }

    let result =
      AttributeHelpers.getIdFrom subject
    
    Expect.equal 
      result
      expectedId
      "The ID must be properly found on the class."

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

        testCase 
          " A Class that has no PartitionKey should use PartitionKey.None" 
          PartitionKeyAttribute_MissingAttributeShouldUsePartitionKeyNone

        testCase
          " The PartitionKeyAttribute must be used on a property of supported type"
          PartitionKeyAttribute_AttributeOnPropertyWithUnsupportedTypeShouldThrow 

        testCase
          " The PartitionKeyAttribute should be found on a Reference Type Property"
          PartitionKeyAttribute_ShouldAllowAttributeOnClassTypeProperty
          
        testCase
          " The IdAttribute should be properly found"
          AnIdAttributeShouldBeValidOnAStringProperty ]
