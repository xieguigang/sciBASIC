# LinguisticVariable
_namespace: [Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic](./index.md)_

Represents a linguistic variable.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.LinguisticVariable.#ctor(System.String,Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.MembershipFunctionCollection)
```
Default constructor.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name that identificates the linguistic variable.|
|membershipFunctionCollection|A membership functions collection for the lingusitic variable.|


#### Fuzzify
```csharp
Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.LinguisticVariable.Fuzzify(System.String)
```
Implements the fuzzification of the linguistic variable.

|Parameter Name|Remarks|
|--------------|-------|
|membershipFunctionName|The membership function for which fuzzify the variable.|


_returns: The degree of membership._

#### MaxValue
```csharp
Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.LinguisticVariable.MaxValue
```
Returns the maximum value of the linguistic variable.

_returns: The maximum value of the linguistic variable._

#### MinValue
```csharp
Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.LinguisticVariable.MinValue
```
Returns the minimum value of the linguistic variable.

_returns: The minimum value of the linguistic variable._

#### Range
```csharp
Microsoft.VisualBasic.Mathematical.Logical.FuzzyLogic.LinguisticVariable.Range
```
Returns the difference between MaxValue() and MinValue().

_returns: The difference between MaxValue() and MinValue()._


### Properties

#### InputValue
The input value for the linguistic variable.
#### MembershipFunctionCollection
A membership functions collection for the lingusitic variable.
#### Name
The name that identificates the linguistic variable.
