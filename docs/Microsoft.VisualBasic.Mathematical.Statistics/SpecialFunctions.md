# SpecialFunctions
_namespace: [Microsoft.VisualBasic.Mathematical.Statistics](./index.md)_

@author Will_and_Sara



### Methods

#### EvaluatePolynomial
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.EvaluatePolynomial(System.Double,System.Double[])
```
https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was polevl

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|coefficients|-|


#### Factorial
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.Factorial(System.Int32,System.Int32)
```
could be improved with unsigned integers or the gamma function

|Parameter Name|Remarks|
|--------------|-------|
|N|-|
|k|-|


#### gamma
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.gamma(System.Double)
```
testing showed Ben's code and this code were roughly equivalent (also to excel) however, Ben's code executed faster in the time trials.
 https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### gammaln
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.gammaln(System.Double)
```
testing showed Ben's code and this code were roughly equivalent (also to excel) however, Ben's code executed faster in the time trials.

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### incbcf
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.incbcf(System.Double,System.Double,System.Double)
```
Continued fraction expansion #1
 * for incomplete beta integral

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|
|x|-|


#### incbd
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.incbd(System.Double,System.Double,System.Double)
```
Continued fraction expansion #2
 * for incomplete beta integral

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|
|x|-|


#### IncompleteGamma
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.IncompleteGamma(System.Double,System.Double)
```
https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was igam

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|x|-|


#### IncompleteGammaComplement
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.IncompleteGammaComplement(System.Double,System.Double)
```
https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was igamc

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|x|-|


#### MutualProbability
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.MutualProbability(System.Double[])
```
http://lethalman.blogspot.com/2011/08/probability-of-union-of-independent.html

|Parameter Name|Remarks|
|--------------|-------|
|probabilities|-|


#### pseries
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.pseries(System.Double,System.Double,System.Double)
```
Power series for incomplete beta integral.
 Use when b*x is small and x not too close to 1.

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|
|x|-|


#### RegularizedIncompleteBetaFunction
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.RegularizedIncompleteBetaFunction(System.Double,System.Double,System.Double)
```
###### The regularized incomplete beta function
 https://en.wikipedia.org/wiki/Beta_function#Incomplete_beta_function

|Parameter Name|Remarks|
|--------------|-------|
|aa|-|
|bb|-|
|xx|-|


#### StirlingsFormula
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.SpecialFunctions.StirlingsFormula(System.Double)
```
https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was stirf

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



