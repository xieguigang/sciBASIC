# Randomizer
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_

##### Random generator based on the random table.(请注意，这个模块之中的所有函数都是线程不安全的)
 
 ###### A Million Random Digits with 100,000 Normal Deviates
 
 Not long after research began at RAND in 1946, the need arose for random numbers that 
 could be used to solve problems of various kinds of experimental probability procedures. 
 These applications, called Monte Carlo methods, required a large supply of random 
 digits and normal deviates of high quality, and the tables presented here were produced 
 to meet those requirements. This book was a product of RAND's pioneering work in computing, 
 as well a testament to the patience and persistence of researchers in the early days of 
 RAND. The tables of random numbers in this book have become a standard reference in 
 engineering and econometrics textbooks and have been widely used in gaming and simulations 
 that employ Monte Carlo trials. Still the largest published source of random digits and 
 normal deviates, the work is routinely used by statisticians, physicists, polltakers, 
 market analysts, lottery administrators, and quality control engineers. A 2001 article 
 in the New York Times on the value of randomness featured the original edition of the book, 
 published in 1955 by the Free Press. The rights have since reverted to RAND, and in this 
 digital age, we thought it appropriate to reissue a new edition of the book in its original 
 format, with a new foreword by Michael D. Rich, RAND's Executive Vice President.
 
 > http://www.rand.org/pubs/monograph_reports/MR1418.html



### Methods

#### GetRandomNormalDeviates
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.GetRandomNormalDeviates(System.Int32)
```
返回一组符合标准正态分布的实数

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### GetRandomPercentages
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.GetRandomPercentages(System.Int32)
```
返回随机的0-1之间的百分比数值

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### Next
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.Next(System.Int32,System.Int32)
```
Returns a random integer that is within a specified range.

|Parameter Name|Remarks|
|--------------|-------|
|minValue|The inclusive lower bound of the random number returned.|
|maxValue|The exclusive upper bound of the random number returned. maxValue must be greater
 than or equal to minValue.|


_returns: A 32-bit signed integer greater than or equal to minValue and less than maxValue;
 that is, the range of return values includes minValue but not maxValue. If minValue
 equals maxValue, minValue is returned._

#### NextBytes
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.NextBytes(System.Byte[])
```
Fills the elements of a specified array of bytes with random numbers.

|Parameter Name|Remarks|
|--------------|-------|
|buffer|An array of bytes to contain random numbers.|


#### NextDouble
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.NextDouble
```
Returns a random floating-point number that is greater than or equal to 0.0,
 and less than 1.0.

_returns: A double-precision floating point number that is greater than or equal to 0.0,
 and less than 1.0._

#### Sample
```csharp
Microsoft.VisualBasic.Mathematical.Randomizer.Sample
```
Normal deviates


### Properties

#### DigitsRowLength
每一行有10个随机数
#### len
@``F:Microsoft.VisualBasic.Mathematical.Randomizer._digits`` max integer
#### max
@``F:Microsoft.VisualBasic.Mathematical.Randomizer._digits`` max integer
#### min
@``F:Microsoft.VisualBasic.Mathematical.Randomizer._digits`` max integer
