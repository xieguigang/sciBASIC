#Region "Microsoft.VisualBasic::triq_ecdf_perf, Data_science\Mathematica\Math\Math\test\DistributionsPerfTest.vb"

' ============================================================================
' DistributionsPerfTest.vb — TrIQ / ECDF 性能优化的「正确性校验 + 提速基准」
' ----------------------------------------------------------------------------
' 三个公共 API 在重构前后的行为必须逐元素一致：
'   * TrIQ.CutThreshold    —— 逐元素上界钳制（原 If xi > cut Then cut Else xi）
'   * TrIQ.DiscreteLevels  —— 仿射映射 + w >= T 取 n-1（原 ScaleMapping + 分支）
'   * ECDF.FindThreshold   —— 阈值查找（原 O(k^2) 切片重算 -> 前缀和 O(k)）
'
' 校验方式：把「重构前」的语义用纯标量代码内联成 reference 实现，与重构后的
' 公开 API 逐项比对；再在 ~1e6 规模数据上测量 reference（前）vs 优化实现（后）
' 的耗时与加速比。
'
' 注意：ECDF.FindThreshold 的复杂度由分箱数 k（默认 100）界定，而非数据点数，
' 因此其绝对收益很小；本测试主要验证其数值一致性与算法降阶，真正的 SIMD 收益
' 来自 TrIQ 两个逐元素函数（遍历的是全部数据点）。
' ============================================================================

Imports System.Diagnostics
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Public Module DistributionsPerfTest

    ''' <summary>累加器：阻止 JIT 把整段计算当作无用代码消除。</summary>
    Private sink As Double = 0
    Private pass As Integer = 0
    Private fail As Integer = 0

    Public Function RunAll() As Integer
        Console.WriteLine("==================================================================")
        Console.WriteLine(" TrIQ / ECDF 性能优化 —— 正确性校验 + 提速基准")
        Console.WriteLine("==================================================================")
        Console.WriteLine($" 处理器: {Microsoft.VisualBasic.Math.SIMD.SimdCapabilities.Description}")
        Console.WriteLine($" SIMD 启用: {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.IsEnabled}")
        Console.WriteLine()

        Correctness()
        Console.WriteLine()
        Benchmark()
        Console.WriteLine()

        Console.WriteLine($"校验: {pass} 通过, {fail} 失败")
        Console.WriteLine($"（校验和 sink = {sink:G6}，仅用于阻止 JIT 死代码消除）")

        Return If(fail > 0, 1, 0)
    End Function

#Region "correctness"

    Private Sub Correctness()
        Console.WriteLine("--- 正确性校验（重构前 reference 逐项对比重构后公开 API） ---")

        ' 1) CutThreshold：普通数据 + 含超过 cut 的尾部
        Dim data1 As Double() = MakeRandom(12345, 200000, 0.0, 1000.0)
        Dim q As Double = 0.9
        Dim cut As Double = data1.FindThreshold(q)
        Dim refCut As Double() = RefCutThreshold(data1, cut)
        AssertArrayEqual("CutThreshold 元素一致", refCut, data1.CutThreshold(q).ToArray)

        ' 2) DiscreteLevels：默认 T = max，以及显式 T 小于 max（触发 w >= T 分支）
        Dim refDisc1 As Integer() = RefDiscrete(data1, 30, Nothing)
        AssertArrayEqual("DiscreteLevels 默认 T 一致", refDisc1, data1.DiscreteLevels(30).ToArray)

        Dim Tval As Double = 800.0
        Dim refDisc2 As Integer() = RefDiscrete(data1, 30, Tval)
        AssertArrayEqual("DiscreteLevels 显式 T 一致", refDisc2, data1.DiscreteLevels(30, Tval).ToArray)

        ' 3) DiscreteLevels：全部相等的退化情形（span = 0 -> 全返回 n - 1）
        Dim flat As Double() = Enumerable.Repeat(5.0, 1000).ToArray
        Dim refFlat As Integer() = RefDiscrete(flat, 16, Nothing)
        AssertArrayEqual("DiscreteLevels 全相等一致", refFlat, flat.DiscreteLevels(16).ToArray)

        ' 4) FindThreshold：reference（原 O(k^2) 算法）vs 重构后前缀和
        Dim data2 As Double() = MakeRandom(98765, 500000, 0.0, 1.0)
        For Each qq As Double In {0.5, 0.9, 0.99, 0.1}
            Dim refThr As Double = RefFindThreshold(data2, qq, 0.1)
            Dim newThr As Double = New ECDF(data2, 100).FindThreshold(qq, 0.1)
            AssertEqual($"FindThreshold(q={qq}) 一致", refThr, newThr)
        Next

        Console.WriteLine()
    End Sub

    ''' <summary>重构前的 CutThreshold 语义：上界钳制。</summary>
    Private Function RefCutThreshold(v As Double(), cut As Double) As Double()
        Return v.Select(Function(xi) If(xi > cut, cut, xi)).ToArray
    End Function

    ''' <summary>重构前的 DiscreteLevels 语义（含 w >= T 分支与 ScaleMapping）。</summary>
    Private Function RefDiscrete(data As Double(), n As Integer, T As Double?) As Integer()
        Dim f As Double() = data
        Dim minf As Double = f.Min

        If T Is Nothing Then
            T = f.Max
        End If

        Dim levelRange As New DoubleRange(0, n)
        Dim scaler As New DoubleRange(minf, T)

        Return (From w As Double
                In f
                Let q As Double = If(w >= T, n - 1, scaler.ScaleMapping(w, levelRange))
                Select CInt(q)).ToArray
    End Function

    ''' <summary>重构前的 FindThreshold 语义：循环内 sample.Take(k) + CDF 重算（O(k^2)）。</summary>
    Private Function RefFindThreshold(data As Double(), q As Double, eps As Double) As Double
        Dim sample As DataBinBox(Of Double)() = CutBins _
            .FixedWidthBins(data, 100, Function(xi) xi) _
            .ToArray

        If sample.Length = 0 Then
            Return 0
        End If

        Dim N As Integer = Aggregate point As DataBinBox(Of Double) In sample Into Sum(point.Count)
        Dim minK As Integer = 1
        Dim minD As Double = Double.MaxValue

        For k As Integer = 1 To sample.Length - 1
            Dim cdf As Double = ECDF.CDF(sample.Take(k), N)

            If cdf > q Then
                Exit For
            End If

            Dim d As Double = std.Abs(cdf - q)

            If d <= eps Then
                Return sample(k).Boundary.Min
            ElseIf d < minD Then
                minD = d
                minK = k
            End If
        Next

        Return sample(minK - 1).Boundary.Max
    End Function

#End Region

#Region "benchmark"

    Private Sub Benchmark()
        Console.WriteLine("--- 提速基准（reference 前 vs 优化后，~1e6 规模） ---")

        Const len As Integer = 1000000
        Dim data As Double() = MakeRandom(2024, len, 0.0, 1000.0)

        ' CutThreshold：隔离比较「上界钳制」本身（两端的阈值查找 ECDF 开销相同，不在本次优化范围内）
        Dim cut As Double = data.FindThreshold(0.9)
        BenchVersus("TrIQ.CutThreshold 钳制",
            Function() Checksum(RefCutThreshold(data, cut)),
            Function() Checksum(data.SimdClamp(Double.MinValue, cut)))

        ' DiscreteLevels（默认 T）
        BenchVersus("TrIQ.DiscreteLevels",
            Function() ChecksumInt(RefDiscrete(data, 30, Nothing)),
            Function() ChecksumInt(data.DiscreteLevels(30).ToArray))

        ' FindThreshold：两者都包含分箱开销，仅对比阈值查找算法本身
        BenchVersus("ECDF.FindThreshold",
            Function() RefFindThreshold(data, 0.9, 0.1),
            Function() New ECDF(data, 100).FindThreshold(0.9, 0.1))

        Console.WriteLine()
    End Sub

    ''' <summary>两个实现（都在向量化路径下）之间的耗时对比，返回加速比。</summary>
    Private Function BenchVersus(name As String, before As Func(Of Double), after As Func(Of Double)) As Double
        sink += before()
        sink += after()

        Dim msBefore As Double = BestOf(before)
        Dim msAfter As Double = BestOf(after)
        Dim speedup As Double = If(msAfter > 0, msBefore / msAfter, 0)

        Console.WriteLine($" {name,-22} 前 {msBefore,9:F3} ms | 后 {msAfter,9:F3} ms | 加速 {speedup,6:F2}x")

        Return speedup
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

    ''' <summary>抽样求和（每 97 个取一个），把 Double() 折叠成标量。</summary>
    Private Function Checksum(v As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1 Step 97
            s += v(i)
        Next

        Return s
    End Function

    Private Function ChecksumInt(v As Integer()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1 Step 97
            s += v(i)
        Next

        Return s
    End Function

#End Region

#Region "harness helpers"

    Private Function MakeRandom(seed As Integer, length As Integer, min As Double, max As Double) As Double()
        Dim rnd As New Random(seed)
        Dim span As Double = max - min
        Dim v As Double() = New Double(length - 1) {}

        For i As Integer = 0 To length - 1
            v(i) = min + rnd.NextDouble() * span
        Next

        Return v
    End Function

    Private Sub AssertEqual(name As String, expected As Double, actual As Double)
        If expected = actual Then
            pass += 1
            Console.WriteLine($"  [PASS] {name}: {expected}")
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {expected}, 实际 {actual}")
        End If
    End Sub

    Private Sub AssertArrayEqual(name As String, expected As Double(), actual As Double())
        If expected.Length <> actual.Length Then
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 长度不一致 {expected.Length} vs {actual.Length}")
            Return
        End If

        For i As Integer = 0 To expected.Length - 1
            If expected(i) <> actual(i) Then
                fail += 1
                Console.WriteLine($"  [FAIL] {name}: 索引 {i} 期望 {expected(i)}, 实际 {actual(i)}")
                Return
            End If
        Next

        pass += 1
        Console.WriteLine($"  [PASS] {name}: {expected.Length} 元素一致")
    End Sub

    Private Sub AssertArrayEqual(name As String, expected As Integer(), actual As Integer())
        If expected.Length <> actual.Length Then
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 长度不一致 {expected.Length} vs {actual.Length}")
            Return
        End If

        For i As Integer = 0 To expected.Length - 1
            If expected(i) <> actual(i) Then
                fail += 1
                Console.WriteLine($"  [FAIL] {name}: 索引 {i} 期望 {expected(i)}, 实际 {actual(i)}")
                Return
            End If
        Next

        pass += 1
        Console.WriteLine($"  [PASS] {name}: {expected.Length} 元素一致")
    End Sub

#End Region

End Module

#End Region
