# ContinuousDistribution
_namespace: [Microsoft.VisualBasic.Mathematical.Statistics.Distributions](./index.md)_

@author Will_and_Sara



### Methods

#### Clone
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.Clone
```
Creates a clone of the current ContinuousDistribution.

_returns:  A ContinuousDistribution of the same type as the one this function is called on. _

#### GetCDF
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.GetCDF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
This function is the Cumulative Distribution Function. It returns a Non Exceedance probability for any value. It will be implemented by all concrete implementations of this abstract class.

|Parameter Name|Remarks|
|--------------|-------|
|value| the value that a probability will be produced for. |


_returns:  a probability that this value will be exceeded by any other value from the sample set. _

#### GetInvCDF
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.GetInvCDF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
This function produces a value for a given probability, this value will represent the Non-Exceedance value for that probability.

|Parameter Name|Remarks|
|--------------|-------|
|probability| a number between 0 and 1. |


_returns:  a value distributed by the distribution defined in the concrete implementation of this abstract class. _

#### GetParamNames
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.GetParamNames
```
This function will return string representations of the parameter names for each distribution.

_returns:  a String array of all of the declared fields composing the concrete implementation of this ContinuousDistribution _

#### GetParamValues
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.GetParamValues
```
This function determines the current values for each parameter in this concrete implementation of the ContinuousDistribution

_returns:  an array of object for each parameter in this class. _

#### GetPDF
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Distributions.ContinuousDistribution.GetPDF(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
This is the Probability Density Function. It describes the likelihood any given value will occur within a dataset.

|Parameter Name|Remarks|
|--------------|-------|
|value| the value that a likelihood will be returned for. |


_returns:  the likelihood (defined by the concrete distribution) the specified value will occur in any given sample dataset (assuming the value is from the underlying distribution). _


