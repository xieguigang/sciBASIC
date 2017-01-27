# RandomExtensions
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

Some extension methods for @``T:System.Random`` for creating a few more kinds of random stuff.

> Imports from https://github.com/rvs76/superbest-random.git 


### Methods

#### NextBoolean
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.NextBoolean(System.Random)
```
Equally likely to return true or false. Uses @``M:System.Random.Next(System.Int32)``.
> 
>  ```vbnet
>  1 > 0 OR 0 > 0
>  ```
>  

#### NextDouble
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.NextDouble(System.Random,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange)
```
Returns a random floating-point number that is greater than or equal to min of the range,
 and less than the max of the range.

|Parameter Name|Remarks|
|--------------|-------|
|rnd|-|
|range|-|


#### NextGaussian
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.NextGaussian(System.Random,System.Double,System.Double)
```
Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.

|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|mu|Mean of the distribution|
|sigma|Standard deviation|


#### NextTriangular
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.NextTriangular(System.Random,System.Double,System.Double,System.Double)
```
Generates values from a triangular distribution.

|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|a|Minimum|
|b|Maximum|
|c|Mode (most frequent value)|

> 
>  See http://en.wikipedia.org/wiki/Triangular_distribution for a description of the triangular probability distribution and the algorithm for generating one.
>  

#### Permutation
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.Permutation(System.Random,System.Int32,System.Int32)
```
Returns n unique random numbers in the range [1, n], inclusive. 
 This is equivalent to getting the first n numbers of some random permutation of the sequential numbers from 1 to max. 
 Runs in O(k^2) time.

|Parameter Name|Remarks|
|--------------|-------|
|rand|-|
|n|Maximum number possible.(最大值)|
|k|How many numbers to return.(返回的数据的数目)|


#### Shuffle
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.Shuffle(System.Random,System.Collections.IList@)
```
Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.

|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|list|-|


#### Shuffle``1
```csharp
Microsoft.VisualBasic.Mathematical.RandomExtensions.Shuffle``1(System.Random,System.Collections.Generic.List{``0}@)
```
Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.

|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|list|-|



