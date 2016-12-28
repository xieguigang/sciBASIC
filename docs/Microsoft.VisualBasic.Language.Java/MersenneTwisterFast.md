# MersenneTwisterFast
_namespace: [Microsoft.VisualBasic.Language.Java](./index.md)_

MersenneTwisterFast:
 
 A simulation quality fast random number generator (MT19937) with the same
 public methods as java.util.Random.
 
 
 About the Mersenne Twister. This is a Java version of the C-program for
 MT19937: Integer version. next(32) generates one pseudorandom unsigned
 integer (32bit) which is uniformly distributed among 0 to 2^32-1 for each
 call. next(int bits) >>>'s by (32-bits) to get a value ranging between 0 and
 2^bits-1 long inclusive; hope that's correct. setSeed(seed) set initial
 values to the working area of 624 words. For setSeed(seed), seed is any
 32-bit integer except for 0.
 
 Reference. M. Matsumoto and T. Nishimura, "Mersenne Twister: A
 623-Dimensionally Equidistributed Uniform Pseudo-Random Number Generator",
 ACM Transactions on Modeling and Computer Simulation, Vol. 8, No. 1,
 January 1998, pp 3--30.
 
 
 Bug Fixes. This implementation implements the bug fixes made in Java 1.2's
 version of Random, which means it can be used with earlier versions of Java.
 See 
 the JDK 1.2 java.util.Random documentation for further documentation on
 the random-number generation contracts made. Additionally, there's an
 undocumented bug in the JDK java.util.Random.nextBytes() method, which this
 code fixes.
 
 
 Important Note. Just like java.util.Random, this generator accepts a long
 seed but doesn't use all of it. java.util.Random uses 48 bits. The Mersenne
 Twister instead uses 32 bits (int size). So it's best if your seed does not
 exceed the int range.
 
 
 Sean Luke's web page
 
 
 - added shuffling method (Alexei Drummond)
 
 - added gamma RV method (Marc Suchard)
 
 This is now package private - it should be accessed using the instance in
 Random



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.#ctor(System.Int64)
```
Constructor using a given seed. Though you pass this seed in as a long,
 it's best to make sure it's actually an integer.

|Parameter Name|Remarks|
|--------------|-------|
|seed|
            generator starting number, often the time of day. |


#### nextGamma
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.nextGamma(System.Double,System.Double)
```
****************************************************************
 * Gamma Distribution - Acceptance Rejection combined with *
 Acceptance Complement * *
 ****************************************************************** 
 * FUNCTION: - gds samples a random number from the standard * gamma
 distribution with parameter a > 0. * Acceptance Rejection gs for a <
 1 , * Acceptance Complement gd for a >= 1 . * REFERENCES: - J.H.
 Ahrens, U. Dieter (1974): Computer methods * for sampling from gamma,
 beta, Poisson and * binomial distributions, Computing 12, 223-246. *
 - J.H. Ahrens, U. Dieter (1982): Generating gamma * variates by a
 modified rejection technique, * Communications of the ACM 25, 47-54.
 * SUBPROGRAMS: - drand(seed) ... (0,1)-Uniform generator with *
 unsigned long integer *seed * - NORMAL(seed) ... Normal generator
 N(0,1). * *
 *****************************************************************

#### nextInt
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.nextInt(System.Int32)
```
Returns an integer drawn uniformly from 0 to n-1. Suffice it to say, n
 must be > 0, or an IllegalArgumentException is raised.

#### permute
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.permute(System.Int32[])
```
Returns a uniform random permutation of int objects in array

#### permuted
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.permuted(System.Int32)
```
Returns a uniform random permutation of ints 0,...,l-1

|Parameter Name|Remarks|
|--------------|-------|
|l|
            length of the array required. |


#### shuffle
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.shuffle(System.Int32[],System.Int32)
```
Shuffles an array. Shuffles numberOfShuffles times

#### shuffled
```csharp
Microsoft.VisualBasic.Language.Java.MersenneTwisterFast.shuffled(System.Int32)
```
Returns an array of shuffled indices of length l.

|Parameter Name|Remarks|
|--------------|-------|
|l|
            length of the array required. |



### Properties

#### Seed
Initalize the pseudo random number generator. The Mersenne Twister only
 uses an integer for its seed; It's best that you don't pass in a long
 that's bigger than an int.
#### serialVersionUID

