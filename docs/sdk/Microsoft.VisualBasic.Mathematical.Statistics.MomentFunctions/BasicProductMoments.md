# BasicProductMoments
_namespace: [Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions](./index.md)_

@author Will_and_Sara



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.#ctor(System.Collections.Generic.IEnumerable{System.Double})
```
This constructor allows one to create an instance with some initial data, observations can be added after the constructor through the "AddObservations(double observation) call.

|Parameter Name|Remarks|
|--------------|-------|
|data| the dataset to calculate mean and standard deviation for. |


#### AddObservation
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.AddObservation(System.Double)
```
An inline algorithm for incrementing mean and standard of deviation. After this method call, the properties of this class should be updated to include this observation.

|Parameter Name|Remarks|
|--------------|-------|
|observation| the observation to be added |


#### IsConverged
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.IsConverged
```
This function can be used to determine if enough samples have been added to determine convergence of the data stream.

_returns:  this function will return false until the minimum number of observations have been added, and then will return the result of the convergence test after the most recent observation. _

#### SetConvergenceTolerance
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.SetConvergenceTolerance(System.Double)
```
This method sets the tolerance for convergence. This tolerance is used as an epsilon neighborhood around the confidence defined in SetZalphaForConvergence.

|Parameter Name|Remarks|
|--------------|-------|
|tolerance| the distance that is determined to be close enough to the alpha in question. |


#### SetMinValuesBeforeConvergenceTest
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.SetMinValuesBeforeConvergenceTest(System.Int32)
```
This method allows the user to define a minimum number of observations before testing for convergence.
 This would help to mitigate early convergence if similar observations are in close sequence early in the dataset.

|Parameter Name|Remarks|
|--------------|-------|
|numobservations| the minimum number of observations to wait until testing for convergence. |


#### SetZAlphaForConvergence
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions.BasicProductMoments.SetZAlphaForConvergence(System.Double)
```
This method defines the alpha value used to determine convergence. This value is based on a two sided confidence interval. It uses the upper Confidence Limit.

|Parameter Name|Remarks|
|--------------|-------|
|ConfidenceInterval| The value that would be used to determine the normal alpha value.  The default is a .9 Confidence interval, which corresponds to 1.96 alpha value. |



