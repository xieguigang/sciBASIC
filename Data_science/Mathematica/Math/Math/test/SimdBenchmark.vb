#Region "Microsoft.VisualBasic::eb2e70b41132e815fd587b318dab51f1, Data_science\Mathematica\Math\Math\test\SimdBenchmark.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 244
    '    Code Lines: 157 (64.34%)
    ' Comment Lines: 31 (12.70%)
    '    - Xml Docs: 48.39%
    ' 
    '   Blank Lines: 56 (22.95%)
    '     File Size: 9.77 KB


    ' Module SimdBenchmark
    ' 
    '     Function: BenchVersus, BestOf, Checksum, ChecksumFromMatrix, ChecksumFromVector
    '               Fill, FillSquare, RunAll, SimdParallelWithParallelism
    ' 
    '     Sub: BenchApi
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SimdBenchmark.vb — Vector / NumericMatrix 的标量 vs SIMD 加速比基准
' ----------------------------------------------------------------------------
' 测量方式：对**同一个公开 API** 跑两遍，一遍通过
'   SIMDEnvironment.config = disable  强制走库内的标量回退路径（等价于重构前的实现）
' 另一遍走默认的向量化路径。两侧都在同一个程序集、同一优化级别下执行，
' 因此加速比反映的确实是「向量化」带来的收益，而不掺杂跨程序集的代码质量差异。
'
' 数据规模刻意选在 SimdParallel.MinParallelLength * 2 以下，
' 以隔离出「向量化」本身的收益；并行加速单独在最后一节测量。
' ============================================================================

Imports System.Diagnostics
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SimdCapabilities = Microsoft.VisualBasic.Math.SIMD.SimdCapabilities
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine
Imports SimdParallel = Microsoft.VisualBasic.Math.SIMD.SimdParallel
Imports SimdReduce = Microsoft.VisualBasic.Math.SIMD.SimdReduce
Imports SIMDConfiguration = Microsoft.VisualBasic.Math.SIMD.SIMDConfiguration
Imports SIMDEnvironment = Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment
Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics
Imports std = System.Math

Public Module SimdBenchmark

    ''' <summary>
    ''' 累加器：把各基准的结果累加进来，防止 JIT 把整段计算当作无用代码消除。
    ''' </summary>
    Private sink As Double = 0

    Public Function RunAll() As Integer
        Console.WriteLine("==================================================================")
        Console.WriteLine(" SIMD 加速基准（同一 API：disable 标量路径 vs 向量化路径）")
        Console.WriteLine("==================================================================")
        Console.WriteLine($" 处理器: {SimdCapabilities.Description}")
        Console.WriteLine($" SIMD 启用: {SIMDEnvironment.IsEnabled}")
        Console.WriteLine($" 并行阈值: {SimdParallel.MinParallelLength * 2}")
        Console.WriteLine()

        ' 低于并行阈值：只对比向量化收益
        Const len As Integer = 60000

        Dim a As Double() = Fill(len, 1.25)
        Dim b As Double() = Fill(len, 0.75)

        Console.WriteLine($"--- 向量内核（长度 = {len}） ---")
        BenchApi("SimdEngine.Add", Function() Checksum(SimdEngine.Add(Of Double)(a, b)))
        BenchApi("SimdEngine.Subtract", Function() Checksum(SimdEngine.Subtract(Of Double)(a, b)))
        BenchApi("SimdReduce.SumSquares", Function() SimdReduce.SumSquares(a))
        BenchApi("SimdReduce.Dot", Function() SimdReduce.Dot(a, b))
        BenchApi("SimdReduce.L1Norm", Function() SimdReduce.L1Norm(a))
        BenchApi("SimdEngine.DivideZeroSafe", Function() Checksum(SimdEngine.DivideZeroSafe(a, b)))
        BenchApi("Vector + Vector", Function() ChecksumFromVector(New Vector(a) + New Vector(b)))
        BenchApi("Vector.Mod", Function() New Vector(a).Mod)
        BenchApi("Vector.SumMagnitude", Function() New Vector(a).SumMagnitude)
        Console.WriteLine()

        ' FMA 内核额外检查硬件能力（不随 disable 关闭），单独对比
        Dim fmaGain As Double = BenchVersus(
            "SimdReduce.SumSquares → SumSquaresFma",
            Function() SimdReduce.SumSquares(a),
            Function() SIMDIntrinsics.SumSquaresFma(a))

        If fmaGain > 0 Then
            Console.WriteLine()
        End If

        Const order As Integer = 256
        Dim ma As New NumericMatrix(FillSquare(order, 1.5))
        Dim mb As New NumericMatrix(FillSquare(order, 0.5))

        Console.WriteLine($"--- 矩阵运算（{order} x {order}） ---")
        BenchApi("矩阵逐元素加法", Function() ChecksumFromMatrix(ma + mb))
        BenchApi("矩阵数乘", Function() ChecksumFromMatrix(ma.Multiply(2.5)))
        BenchApi("矩阵转置", Function() ChecksumFromMatrix(ma.Transpose()))
        BenchApi("矩阵绝对值", Function() ChecksumFromMatrix(ma.Abs()))
        BenchApi("Norm1", Function() ma.Norm1())
        BenchApi("NormInf", Function() ma.NormInf())
        BenchApi("NormF", Function() ma.NormF())
        Console.WriteLine()

        ' 矩阵乘法：小规模时每个点积的调用开销占比高，
        ' 规模跨过 SimdParallel 的并行阈值后才能真正体现「并行 + FMA」的收益
        Const mulOrder As Integer = 384
        Dim ka As New NumericMatrix(FillSquare(mulOrder, 1.25))

        Console.WriteLine($"--- 矩阵乘法（{mulOrder} x {mulOrder} x {mulOrder}） ---")
        BenchApi("DotProduct", Function() ChecksumFromMatrix(ka.Multiply(ka)))
        Console.WriteLine()

        ' ---- 并行加速（只对比 SimdParallel 的开关） ----
        Const bigLen As Integer = 4 << 20
        Dim big As Double() = Fill(bigLen, 1.5)

        Console.WriteLine($"--- 分块并行（长度 = {bigLen}） ---")

        Dim parallelGain As Double = BenchVersus(
            "SimdParallel.Sum：串行 → 并行",
            Function() SimdParallel.Sum(big),
            Function() SimdParallelWithParallelism(True, big))

        Console.WriteLine()
        Console.WriteLine($" （校验和 sink = {sink:G6}，仅用于阻止 JIT 死代码消除）")

        Return 0
    End Function

#Region "harness"

    ''' <summary>
    ''' 同一个 API 在「标量回退」与「向量化」两条路径下的耗时对比。
    ''' </summary>
    Private Sub BenchApi(name As String, action As Func(Of Double))
        Dim original As SIMDConfiguration = SIMDEnvironment.config
        Dim msScalar As Double

        Try
            SIMDEnvironment.config = SIMDConfiguration.disable

            sink += action()
            msScalar = BestOf(action)
        Finally
            SIMDEnvironment.config = original
        End Try

        sink += action()

        Dim msSimd As Double = BestOf(action)
        Dim speedup As Double = If(msSimd > 0, msScalar / msSimd, 0)

        Console.WriteLine($" {name,-24} 标量 {msScalar,8:F3} ms | SIMD {msSimd,8:F3} ms | 加速 {speedup,5:F2}x")
    End Sub

    ''' <summary>
    ''' 两个实现（都在向量化路径下）之间的耗时对比，返回加速比；无法比较时返回 0。
    ''' </summary>
    Private Function BenchVersus(name As String, baseline As Func(Of Double), improved As Func(Of Double)) As Double
        sink += baseline()
        sink += improved()

        Dim msBaseline As Double = BestOf(baseline)
        Dim msImproved As Double = BestOf(improved)
        Dim speedup As Double = If(msImproved > 0, msBaseline / msImproved, 0)

        Console.WriteLine($" {name,-40} 前 {msBaseline,8:F3} ms | 后 {msImproved,8:F3} ms | 加速 {speedup,5:F2}x")

        Return speedup
    End Function

    ''' <summary>
    ''' 以指定的并行开关运行 <see cref="SimdParallel.Sum"/>，用于量化多核收益。
    ''' </summary>
    Private Function SimdParallelWithParallelism(enable As Boolean, v As Double()) As Double
        Dim original As Boolean = SimdParallel.Enable

        Try
            SimdParallel.Enable = enable

            Return SimdParallel.Sum(v)
        Finally
            SimdParallel.Enable = original
        End Try
    End Function

    Private Function BestOf(action As Func(Of Double)) As Double
        Const rounds As Integer = 3

        Dim best As Double = Double.MaxValue

        For i As Integer = 0 To rounds - 1
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim r As Double = action()
            sw.Stop()

            sink += r

            If sw.Elapsed.TotalMilliseconds < best Then
                best = sw.Elapsed.TotalMilliseconds
            End If
        Next

        Return best
    End Function

#End Region

#Region "data"

    Private Function Fill(length As Integer, value As Double) As Double()
        Dim data As Double() = New Double(length - 1) {}

        For i As Integer = 0 To length - 1
            data(i) = value + (i Mod 7) * 0.001
        Next

        Return data
    End Function

    Private Function FillSquare(order As Integer, value As Double) As Double()()
        Dim m As Double()() = New Double(order - 1)() {}

        For i As Integer = 0 To order - 1
            m(i) = New Double(order - 1) {}

            For j As Integer = 0 To order - 1
                m(i)(j) = value + ((i + j) Mod 11) * 0.01
            Next
        Next

        Return m
    End Function

    ''' <summary>
    ''' 抽样求和（每 97 个取一个），用于把结果折叠成一个标量而不引入额外开销。
    ''' </summary>
    Private Function Checksum(v As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1 Step 97
            s += v(i)
        Next

        Return s
    End Function

    Private Function ChecksumFromVector(v As Vector) As Double
        Return Checksum(v.Array)
    End Function

    Private Function ChecksumFromMatrix(m As GeneralMatrix) As Double
        Dim data As Double()() = m.ArrayPack(deepcopy:=False)
        Dim s As Double = 0

        For i As Integer = 0 To data.Length - 1 Step 7
            s += Checksum(data(i))
        Next

        Return s
    End Function

#End Region

End Module

