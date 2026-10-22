# Data_science/Mathematica/Math/Randomizer/randomizer-netcore5.vbproj

- RootNamespace : Microsoft.VisualBasic.Math
- AssemblyName  : Microsoft.VisualBasic.Math.Randomizer
- TargetFramework: net10.0
- Source files  : 11
- Existing Title: Random Number Generators and Seeded Samplers Library
- Existing Desc : Random number generation for sciBASIC#: Mersenne Twister, a fast xorshift-style generator, a thread-safe wrapper, a table-driven RAND randomizer for normal deviates and Monte Carlo simulation, plus TrueRandom - a hybrid true random generator driven by OS environment noise (PDH performance counters, process/GC/timing) and a CSPRNG, and a pure environmental entropy collector.
- Existing Tags : scibasic;random;mersenne-twister;monte-carlo;pseudorandom;sampling;true-random;entropy;environment-noise;pdh;csprng

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math)

## Public types
- Module crandn (crandn.vb) - 正态分布随机数
- Class FastRandom (FastRandom\FastRandom.vb) - 4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at construction time which then executes a relatively expensive initialisation routine. This provides a vast speed improvement if you need to reset the pseudo-random number sequence many times, e.g. if you want to re-generate the same
- Interface IProvideRandomValues (FastRandom\IProvideRandomValues.vb)
- Module ThreadSafeFastRandom (FastRandom\ThreadSafeFastRandom.vb)
- Class MersenneTwisterFast (MersenneTwisterFast.vb)
- Class NamespaceDoc (NamespaceDoc.vb) - Random number and sequence generation utilities.
- Class Randomizer (Randomizer.vb) - as well a testament to the patience and persistence of researchers in the early days of RAND. The tables of random numbers in this book have become a standard reference in engineering and econometrics textbooks and have been widely used in gaming and simulations
- Class EntropyMixer (TrueRandom\EntropyMixer.vb) - 基于 HMAC-SHA256 计数器（CTR）模式的密钥流混合器。 将 256-bit 密钥展开为任意长度密钥流：第 i 块 = HMAC-SHA256(密钥, 计数器[i])。 密钥可通过 <see cref="MixKey"/> 周期性折叠新的熵（CSPRNG 输出或环境噪声），
- Class EnvironmentEntropy (TrueRandom\EnvironmentEntropy.vb) - 纯环境噪声熵收集器（慢、高熵）。 通过采样操作系统运行时的各类不可预测量——高精度计时抖动、系统时钟、 进程 CPU/内存/线程/句柄统计、垃圾回收计数与堆大小、线程池积压、GUID、
- Class TrueRandom (TrueRandom\TrueRandom.vb) - 混合真随机数生成器（快、大规模）。 以操作系统加密级 CSPRNG（<see cref="RandomNumberGenerator"/>，由系统熵池驱动） 做批量展开，并用 <see cref="EntropyMixer"/>（HMAC-SHA256 计数器模式）将其与环境

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
- Public Sub New(key As Byte())
- Public Sub MixKey(additional As Byte())
- Public Function Expand(count As Integer) As Byte()
- Public Sub New()
- Public Function Sample() As Byte()
- Public Function GetBytes(count As Integer) As Byte()
- Public Function GetSeed() As Byte()
- Public Shared ReadOnly Property IsAvailable As Boolean =
- Public Shared Function GetShared() As PdhPerformanceCounters
- Public Function SampleDoubles() As Double()
- Public Function SampleBytes() As Byte()
- Public Overrides Function ToString() As String
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Sub New()
- Public ReadOnly Property IsThreadSafe As Boolean Implements IProvideRandomValues.IsThreadSafe
- Public Function GenerateBytes(count As Integer) As Byte()
- Public Function GenerateInt32s(count As Integer) As Integer()
- Public Function GenerateDoubles(count As Integer) As Double()
- Public Function NextDouble() As Double
- ... and 3 more

## Imports
- MAT = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.GeneralMatrix
- Microsoft.VisualBasic.ComponentModel.DataStructures
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math.LinearAlgebra
- Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
- std = System.Math
- System.Diagnostics
- System.Runtime.CompilerServices
- System.Runtime.InteropServices
- System.Security.Cryptography
- System.Text
- System.Text.RegularExpressions
- System.Threading
- System.Threading.Tasks

## File tree
- crandn.vb
- FastRandom\FastRandom.vb
- FastRandom\IProvideRandomValues.vb
- FastRandom\ThreadSafeFastRandom.vb
- MersenneTwisterFast.vb
- NamespaceDoc.vb
- Randomizer.vb
- TrueRandom\EntropyMixer.vb
- TrueRandom\EnvironmentEntropy.vb
- TrueRandom\PdhPerformanceCounters.vb
- TrueRandom\TrueRandom.vb

