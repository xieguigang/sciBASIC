# MathUtils
_namespace: [Microsoft.VisualBasic.Language.Java](./index.md)_

Handy utility functions which have some Mathematical relavance.
 
 @author Matthew Goode
 @author Alexei Drummond
 @author Gerton Lunter
 @version $Id: MathUtils.java,v 1.13 2006/08/31 14:57:24 rambaut Exp $



### Methods

#### getNormalized
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.getNormalized(System.Double[])
```


|Parameter Name|Remarks|
|--------------|-------|
|array|
            to normalize |


_returns:  a new double array where all the values sum to 1. Relative ratios
         are preserved. _

#### getTotal
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.getTotal(System.Double[])
```


|Parameter Name|Remarks|
|--------------|-------|
|array|
            to sum over |


_returns:  the total of the values in an array _

#### hypot
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.hypot(System.Double,System.Double)
```
Returns sqrt(a^2 + b^2) without under/overflow.

#### nextBoolean
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextBoolean
```
Access a default instance of this class, access is synchronized

#### nextByte
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextByte
```
Access a default instance of this class, access is synchronized

#### nextBytes
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextBytes(System.SByte[])
```
Access a default instance of this class, access is synchronized

#### nextChar
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextChar
```
Access a default instance of this class, access is synchronized

#### nextDouble
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextDouble
```
Access a default instance of this class, access is synchronized

_returns:  a pseudo random double precision floating point number in [01) _

#### nextExponential
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextExponential(System.Double)
```
Access a default instance of this class, access is synchronized

#### nextFloat
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextFloat
```
Access a default instance of this class, access is synchronized

#### nextGaussian
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextGaussian
```
Access a default instance of this class, access is synchronized

#### nextInt
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextInt(System.Int32)
```
Access a default instance of this class, access is synchronized

#### nextInverseGaussian
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextInverseGaussian(System.Double,System.Double)
```
Access a default instance of this class, access is synchronized

#### nextLong
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextLong
```
Access a default instance of this class, access is synchronized

#### nextShort
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.nextShort
```
Access a default instance of this class, access is synchronized

#### permute
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.permute(System.Int32[])
```
Permutes an array.

#### permuted
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.permuted(System.Int32)
```
Returns a uniform random permutation of 0,...,l-1

|Parameter Name|Remarks|
|--------------|-------|
|l|
            length of the array required. |


#### randomChoicePDF
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.randomChoicePDF(System.Double[])
```


|Parameter Name|Remarks|
|--------------|-------|
|pdf|
            array of unnormalized probabilities |


_returns:  a sample according to an unnormalized probability distribution _

#### randomLogDouble
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.randomLogDouble
```


_returns:  log of random variable in [0,1] _

#### shuffle
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.shuffle(System.Int32[],System.Int32)
```
Shuffles an array. Shuffles numberOfShuffles times

#### shuffled
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.shuffled(System.Int32)
```
Returns an array of shuffled indices of length l.

|Parameter Name|Remarks|
|--------------|-------|
|l|
            length of the array required. |


#### uniform
```csharp
Microsoft.VisualBasic.Language.Java.MathUtils.uniform(System.Double,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|low|-|
|high|-|


_returns:  uniform between low and high _


### Properties

#### random
A random number generator that is initialized with the clock when this
 class is loaded into the JVM. Use this for all random numbers. Note: This
 method or getting random numbers in not thread-safe. Since
 MersenneTwisterFast is currently (as of 9/01) not synchronized using this
 function may cause concurrency issues. Use the static get methods of the
 MersenneTwisterFast class for access to a single instance of the class,
 that has synchronization.
#### Seed
Access a default instance of this class, access is synchronized
