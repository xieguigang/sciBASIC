#Region "Microsoft.VisualBasic::ed83d3b06e13679f426f27d432343e4e, Data_science\Mathematica\Math\Math\test\DistributionsPerfTest.vb"

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

    '   Total Lines: 150
    '    Code Lines: 109 (72.67%)
    ' Comment Lines: 5 (3.33%)
    '    - Xml Docs: 40.00%
    ' 
    '   Blank Lines: 36 (24.00%)
    '     File Size: 5.11 KB


    '  
    ' 
    '     Function: BenchVersus, BestOf, Checksum, ChecksumInt, MakeRandom
    ' 
    '     Sub: (+2 Overloads) AssertArrayEqual, AssertEqual, Benchmark
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Public Module DistributionsPerfTest

    Private sink As Double = 0
    Private pass As Integer = 0
    Private fail As Integer = 0

    ''' <summary>
    ''' TrIQ / ECDF 性能优化的正确性校验与提速基准入口
    ''' </summary>
    ''' <returns>0 表示全部通过</returns>
    Public Function RunAll() As Integer
        pass = 0
        fail = 0

        Benchmark()

        Console.WriteLine()
        Console.WriteLine($"断言通过 = {pass}, 失败 = {fail}")

        Return If(fail = 0, 0, 1)
    End Function

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

    ''' <summary>
    ''' TrIQ 上界钳制优化前的参考实现：逐元素标量比较。
    ''' </summary>
    Private Function RefCutThreshold(data As Double(), cut As Double) As Double()
        Dim out As Double() = New Double(data.Length - 1) {}

        For i As Integer = 0 To data.Length - 1
            out(i) = std.Min(data(i), cut)
        Next

        Return out
    End Function

    ''' <summary>
    ''' DiscreteLevels 优化前的参考实现：逐元素标量仿射映射（减 -> 除 -> 乘）。
    ''' </summary>
    Private Function RefDiscrete(data As Double(), n As Integer, T As Double?) As Integer()
        Dim f As Double() = data.ToArray
        Dim minf As Double = f.Min
        Dim maxf As Double = f.Max
        Dim out As Integer() = New Integer(f.Length - 1) {}

        If T Is Nothing Then
            T = maxf
        End If

        Dim span As Double = T.Value - minf

        If span = 0.0 Then
            For i As Integer = 0 To f.Length - 1
                out(i) = n - 1
            Next

            Return out
        End If

        For i As Integer = 0 To f.Length - 1
            Dim scaled As Double = (f(i) - minf) / span * n

            out(i) = If(f(i) >= T.Value, n - 1, CInt(scaled))
        Next

        Return out
    End Function

    ''' <summary>
    ''' ECDF.FindThreshold 优化前的参考实现：在循环内反复切片并从头累加 CDF（O(n^2)）。
    ''' </summary>
    ''' <remarks>
    ''' 与 <see cref="ECDF.FindThreshold"/> 的注释一致：优化后改用包含式前缀和 cum(k-1)/N，
    ''' 这里的参考实现刻意保留原来的 ``ECDF.CDF(sample.Take(k), N)`` 写法，用于耗时对比。
    ''' </remarks>
    Private Function RefFindThreshold(data As Double(), q As Double, eps As Double) As Double
        Dim sample As DataBinBox(Of Double)() = CutBins.FixedWidthBins(data, 100, Function(xi) xi) _
            .OrderBy(Function(b) b.Boundary.Min) _
            .ToArray
        Dim n As Integer = Aggregate bi As DataBinBox(Of Double) In sample Into Sum(bi.Count)
        Dim minK As Integer = 1
        Dim minD As Double = Double.MaxValue

        For k As Integer = 1 To sample.Length - 1
            Dim cdf As Double = ECDF.CDF(sample.Take(k), n)

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
