# Statistics
_namespace: [Microsoft.VisualBasic.DataMining](./index.md)_

Set of statistics functions.

> The class represents collection of simple functions used
>  in statistics.


### Methods

#### Entropy
```csharp
Microsoft.VisualBasic.DataMining.Statistics.Entropy(System.Int32[])
```
Calculate entropy value.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|


_returns: Returns entropy value of the specified histagram array._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).Sample usage:
>  '
>  // create histogram array with 2 values of equal probabilities
>  int[] histogram1 = new int[2] { 3, 3 };
>  // calculate entropy
>  double entropy1 = Statistics.Entropy( histogram1 );
>  // output it (1.000)
>  Console.WriteLine( "entropy1 = " + entropy1.ToString( "F3" ) );
>  
>  // create histogram array with 4 values of equal probabilities
>  int[] histogram2 = new int[4] { 1, 1, 1, 1 };
>  // calculate entropy
>  double entropy2 = Statistics.Entropy( histogram2 );
>  // output it (2.000)
>  Console.WriteLine( "entropy2 = " + entropy2.ToString( "F3" ) );
>  
>  // create histogram array with 4 values of different probabilities
>  int[] histogram3 = new int[4] { 1, 2, 3, 4 };
>  // calculate entropy
>  double entropy3 = Statistics.Entropy( histogram3 );
>  // output it (1.846)
>  Console.WriteLine( "entropy3 = " + entropy3.ToString( "F3" ) );
>  '
>  

#### GetRange
```csharp
Microsoft.VisualBasic.DataMining.Statistics.GetRange(System.Int32[],System.Double)
```
Get range around median containing specified percentage of values.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|
|percent|Values percentage around median.|


_returns: Returns the range which containes specifies percentage
 of values._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).The method calculates range of stochastic variable, which summary probability
>  comprises the specified percentage of histogram's hits.Sample usage:
>  '
>  // create histogram array
>  int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
>  // get 75% range around median
>  IntRange range = Statistics.GetRange( histogram, 0.75 );
>  // output it ([4, 8])
>  Console.WriteLine( "range = [" + range.Min + ", " + range.Max + "]" );
>  '
>  

#### Mean
```csharp
Microsoft.VisualBasic.DataMining.Statistics.Mean(System.Int32[])
```
Calculate mean value.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|


_returns: Returns mean value._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).Sample usage:
>  '
>  // create histogram array
>  int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
>  // calculate mean value
>  double mean = Statistics.Mean( histogram );
>  // output it (5.759)
>  Console.WriteLine( "mean = " + mean.ToString( "F3" ) );
>  '
>  

#### Median
```csharp
Microsoft.VisualBasic.DataMining.Statistics.Median(System.Int32[])
```
Calculate median value.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|


_returns: Returns value of median._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).The median value is calculated accumulating histogram's
>  values starting from the left point until the sum reaches 50% of
>  histogram's sum.Sample usage:
>  '
>  // create histogram array
>  int[] histogram = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
>  // calculate median value
>  int median = Statistics.Median( histogram );
>  // output it (6)
>  Console.WriteLine( "median = " + median );
>  '
>  

#### Mode
```csharp
Microsoft.VisualBasic.DataMining.Statistics.Mode(System.Int32[])
```
Calculate mode value.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|


_returns: Returns mode value of the histogram array._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).Returns the minimum mode value if the specified histogram is multimodal.Sample usage:
>  '
>  // create array
>  int[] values = new int[] { 1, 1, 2, 3, 6, 8, 11, 12, 7, 3 };
>  // calculate mode value
>  int mode = Statistics.Mode( values );
>  // output it (7)
>  Console.WriteLine( "mode = " + mode );
>  '
>  

#### StdDev
```csharp
Microsoft.VisualBasic.DataMining.Statistics.StdDev(System.Int32[],System.Double)
```
Calculate standard deviation.

|Parameter Name|Remarks|
|--------------|-------|
|values|Histogram array.|
|mean|Mean value of the histogram.|


_returns: Returns value of standard deviation._
> The input array is treated as histogram, i.e. its
>  indexes are treated as values of stochastic function, but
>  array values are treated as "probabilities" (total amount of
>  hits).The method is an equevalent to the @``M:Microsoft.VisualBasic.DataMining.Statistics.StdDev(System.Int32[])`` method,
>  but it relieas on the passed mean value, which is previously calculated
>  using @``M:Microsoft.VisualBasic.DataMining.Statistics.Mean(System.Int32[])`` method.


