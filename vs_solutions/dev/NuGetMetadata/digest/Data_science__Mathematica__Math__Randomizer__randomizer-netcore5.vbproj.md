# Data_science/Mathematica/Math/Randomizer/randomizer-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.Math
- AssemblyName  : Microsoft.VisualBasic.Math.Randomizer
- TargetFramework: net10.0
- Source files  : 6
- Existing Title: Random Number Generators and Seeded Samplers Library
- Existing Desc : Random number generation for sciBASIC#: Mersenne Twister, a fast xorshift-style generator, a thread-safe wrapper and a table-driven RAND randomizer for normal deviates and Monte Carlo simulation.
- Existing Tags : scibasic;random;mersenne-twister;monte-carlo;pseudorandom;sampling

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math)

## Public types
- Module crandn (crandn.vb) - 正态分布随机数
- Class FastRandom (FastRandom\FastRandom.vb) - 4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at construction time which then executes a relatively expensive initialisation routine. This provides a vast speed improvement if you need to reset the pseudo-random number sequence many times, e.g. if you want to re-generate the same
- Interface IProvideRandomValues (FastRandom\IProvideRandomValues.vb)
- Module ThreadSafeFastRandom (FastRandom\ThreadSafeFastRandom.vb)
- Class MersenneTwisterFast (MersenneTwisterFast.vb)
- Class Randomizer (Randomizer.vb) - as well a testament to the patience and persistence of researchers in the early days of RAND. The tables of random numbers in this book have become a standard reference in engineering and econometrics textbooks and have been widely used in gaming and simulations

## Notable public members
- Public Function randn(m As Integer, seed As Integer) As Vector
- Public Function randn(m As Integer, n As Integer, seed As Integer) As MAT
- Public Function rand(m As Integer, seed As Integer) As Vector
- Public Function rand(m As Integer, n As Integer, seed As Integer) As MAT
- Public Sub New()
- Public Sub New(seed As Integer)
- Public Sub Reinitialise(seed As Integer)
- Public Function NextDouble() As Double
- Public Function NextFloat() As Single
- Public Sub NextFloats(buffer As Double())
- Public Sub NextBytes(buffer As Byte())
- Public Function NextUInt() As UInteger
- Public Function NextInt() As Integer
- Public Function NextBool() As Boolean
- Public Function NextFloat() As Single
- Public Sub NextFloats(buffer As Double())
- Public Sub New()
- Public Property Seed As Long
- Public Function nextInt() As Integer
- Public Function nextShort() As Short
- Public Function nextChar() As Char
- Public Function nextBoolean() As Boolean
- Public Function nextByte() As SByte
- Public Sub nextBytes(bytes As SByte())
- Public Function nextLong() As Long
- Public Function nextDouble() As Double
- Public Function nextGaussian() As Double
- Public Function nextFloat() As Single
- Public Overridable Function nextInt(n As Integer) As Integer
- Public Sub permute(array As Integer())
- Public Sub shuffle(array As Integer())
- Public Sub shuffle(array As Integer(), numberOfShuffles As Integer)
- Public Overridable Function shuffled(l As Integer) As Integer()
- Public Overridable Function permuted(l As Integer) As Integer()
- Public Function nextGamma(alpha As Double, lambda As Double) As Double
- Public Function GetRandomInts(n As Integer) As Integer()
- Public Function GetRandomPercentages(n As Integer) As Double()
- Public Overrides Function NextDouble() As Double
- Public Overrides Sub NextBytes(buffer() As Byte)
- Public Function GetRandomNormalDeviates(n As Integer) As Double()
- Public Overloads Function Sample() As Double

## Imports
- MAT = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.GeneralMatrix
- Microsoft.VisualBasic.ComponentModel.DataStructures
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math.LinearAlgebra
- Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
- std = System.Math
- System.Runtime.CompilerServices
- System.Text.RegularExpressions

## File tree
- crandn.vb
- FastRandom\FastRandom.vb
- FastRandom\IProvideRandomValues.vb
- FastRandom\ThreadSafeFastRandom.vb
- MersenneTwisterFast.vb
- Randomizer.vb

