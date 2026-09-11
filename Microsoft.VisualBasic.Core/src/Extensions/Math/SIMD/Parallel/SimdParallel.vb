#Region "Microsoft.VisualBasic::356434074d96da7978acf93bb20ad3d9, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Parallel\SimdParallel.vb"

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

    '   Total Lines: 518
    '    Code Lines: 325 (62.74%)
    ' Comment Lines: 84 (16.22%)
    '    - Xml Docs: 94.05%
    ' 
    '   Blank Lines: 109 (21.04%)
    '     File Size: 21.30 KB


    '     Class SimdParallel
    ' 
    '         Properties: Enable, MinParallelLength
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Add, ChunkSize, Dot, DotRange, Elementwise
    '                   L1Norm, L2Norm, MatrixDot, Max, Min
    '                   Multiply, MultiplyScalar, ShouldParallelize, Subtract, Sum
    '                   SumRange, SumSquares, Transpose
    ' 
    '         Sub: BinaryRange, MultiplyScalarRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' “分块并行 + 块内向量化”的混合计算驱动。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 单个 <c>Vector(Of T)</c> 指令已经能吃满一个核心的浮点吞吐，因此当数据规模超过
    ''' 缓存容量之后，进一步的收益只能来自多核并行。这里的策略是把数组切分成若干个
    ''' <b>远大于缓存行</b>的块（避免伪共享与线程调度开销），每个块内部继续使用
    ''' <see cref="SimdEngine"/> / <see cref="SimdReduce"/> 的向量化内核。
    ''' </para>
    ''' <para>
    ''' <b>阈值</b>：只有当长度达到 <see cref="MinParallelLength"/> 的两倍以上时才会真正
    ''' 启用并行；小数组会直接退化为单线程向量化路径，避免并行调度开销反而拖慢计算。
    ''' </para>
    ''' <para>
    ''' <b>结果确定性</b>：分块归约的浮点累加顺序与单线程实现不同，因此
    ''' <see cref="Sum"/> / <see cref="Dot"/> 等函数的结果与单线程版本可能存在
    ''' 末位 ULP 级别的差异（这是浮点归约的固有性质，不是 bug）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdParallel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 并行计算的全局开关，默认启用。
        ''' </summary>
        Public Shared Property Enable As Boolean = True

        ''' <summary>
        ''' 触发并行的最小数据长度（单块的最小规模）。
        ''' </summary>
        Public Shared ReadOnly Property MinParallelLength As Integer = 65536

        ''' <summary>
        ''' 当前数据长度是否值得启用并行。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ShouldParallelize(len As Integer) As Boolean
            If Not Enable Then Return False

            Return len >= MinParallelLength * 2
        End Function

        ''' <summary>
        ''' 依据数据长度与处理器个数计算分块大小。
        ''' </summary>
        Private Shared Function ChunkSize(len As Integer) As Integer
            Dim workers As Integer = std.Max(1, Environment.ProcessorCount)
            Dim chunk As Integer = len \ std.Max(1, workers * 8)

            Return std.Max(MinParallelLength, chunk)
        End Function

#Region "range kernels"

        ''' <summary>
        ''' 对 <c>[start, ends)</c> 区间做 4 路累加器求和。
        ''' </summary>
        Private Shared Function SumRange(v As Double(), start As Integer, ends As Integer) As Double
            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = start
            Dim total As Double = 0

            If ends - start >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = ends - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, New Vector(Of Double)(v, i))
                    acc1 = Vector.Add(Of Double)(acc1, New Vector(Of Double)(v, i + count))
                    acc2 = Vector.Add(Of Double)(acc2, New Vector(Of Double)(v, i + count * 2))
                    acc3 = Vector.Add(Of Double)(acc3, New Vector(Of Double)(v, i + count * 3))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))
                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            For k As Integer = i To ends - 1
                total += v(k)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 对 <c>[start, ends)</c> 区间做 4 路累加器点积。
        ''' </summary>
        Private Shared Function DotRange(v1 As Double(), v2 As Double(), start As Integer, ends As Integer) As Double
            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = start
            Dim total As Double = 0

            If ends - start >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = ends - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i), New Vector(Of Double)(v2, i)))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count), New Vector(Of Double)(v2, i + count)))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count * 2), New Vector(Of Double)(v2, i + count * 2)))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count * 3), New Vector(Of Double)(v2, i + count * 3)))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))
                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            For k As Integer = i To ends - 1
                total += v1(k) * v2(k)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 对 <c>[start, ends)</c> 区间做逐元素二元运算并写入 <paramref name="out"/>。
        ''' </summary>
        Private Shared Sub BinaryRange(Of T As Structure)(v1 As T(), v2 As T(), out As T(),
                                                          start As Integer, ends As Integer,
                                                          op As SimdEngine.VectorBinaryOp(Of T))
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = start

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= count Then
                Dim last As Integer = ends - count

                Do While i <= last
                    op(New Vector(Of T)(v1, i), New Vector(Of T)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < ends Then
                    op(New Vector(Of T)(v1, last), New Vector(Of T)(v2, last)).CopyTo(out, last)
                End If

                Return
            End If

            Do While i < ends
                out(i) = op(New Vector(Of T)(v1(i)), New Vector(Of T)(v2(i))).GetElement(0)
                i += 1
            Loop
        End Sub

#End Region

#Region "reduce"

        ''' <summary>
        ''' 分块并行求和。空数组返回 0。
        ''' </summary>
        Public Shared Function Sum(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0
            If Not ShouldParallelize(len) Then Return SimdReduce.Sum(v)

            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk
            Dim partials As Double() = New Double(nChunks - 1) {}

            System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, len)

                    partials(c) = SumRange(v, start, ends)
                End Sub)

            Dim total As Double = 0

            For i As Integer = 0 To nChunks - 1
                total += partials(i)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 分块并行点积。空数组返回 0。
        ''' </summary>
        Public Shared Function Dot(v1 As Double(), v2 As Double()) As Double
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v1.Length
            If len <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {len} vs {v2.Length}!")
            End If
            If len = 0 Then Return 0.0
            If Not ShouldParallelize(len) Then Return SimdReduce.Dot(v1, v2)

            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk
            Dim partials As Double() = New Double(nChunks - 1) {}

            System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, len)

                    partials(c) = DotRange(v1, v2, start, ends)
                End Sub)

            Dim total As Double = 0

            For i As Integer = 0 To nChunks - 1
                total += partials(i)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 分块并行平方和。
        ''' </summary>
        Public Shared Function SumSquares(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0
            If Not ShouldParallelize(len) Then Return SimdReduce.SumSquares(v)

            Return SimdReduce.Dot(v, v)
        End Function

        ''' <summary>
        ''' 分块并行 L1 范数。
        ''' </summary>
        Public Shared Function L1Norm(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0
            If Not ShouldParallelize(len) Then Return SimdReduce.L1Norm(v)

            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk
            Dim partials As Double() = New Double(nChunks - 1) {}

            ' 注意这里不能先对整体做一次 Abs 再求和：那会多出一次完整数组的
            ' 分配与读写（大数组下就是 80MB 级别的额外内存流量）
            System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, len)

                    partials(c) = SimdReduce.L1Norm(v, start, ends)
                End Sub)

            Dim total As Double = 0

            For i As Integer = 0 To nChunks - 1
                total += partials(i)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 分块并行 L2 范数。
        ''' </summary>
        Public Shared Function L2Norm(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Return std.Sqrt(SumSquares(v))
        End Function

        ''' <summary>
        ''' 分块并行最小值。
        ''' </summary>
        Public Shared Function Min(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Throw New ArgumentException("the input vector can not be empty for a reduce operation!")
            If Not ShouldParallelize(len) Then Return SimdReduce.Min(v)

            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk
            Dim partials As Double() = New Double(nChunks - 1) {}

            For c As Integer = 0 To nChunks - 1
                Dim start As Integer = c * chunk
                Dim ends As Integer = std.Min(start + chunk, len)

                partials(c) = SimdReduce.Min(v, start, ends)
            Next

            Return SimdReduce.Min(partials)
        End Function

        ''' <summary>
        ''' 分块并行最大值。
        ''' </summary>
        Public Shared Function Max(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Throw New ArgumentException("the input vector can not be empty for a reduce operation!")
            If Not ShouldParallelize(len) Then Return SimdReduce.Max(v)

            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk
            Dim partials As Double() = New Double(nChunks - 1) {}

            For c As Integer = 0 To nChunks - 1
                Dim start As Integer = c * chunk
                Dim ends As Integer = std.Min(start + chunk, len)

                partials(c) = SimdReduce.Max(v, start, ends)
            Next

            Return SimdReduce.Max(partials)
        End Function

#End Region

#Region "elementwise"

        ''' <summary>
        ''' 分块并行逐元素相加：<c>out(i) = v1(i) + v2(i)</c>
        ''' </summary>
        Public Shared Function Add(v1 As Double(), v2 As Double()) As Double()
            Return Elementwise(v1, v2, Function(a, b) Vector.Add(Of Double)(a, b),
                               Function(x, y) SimdEngine.Add(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 分块并行逐元素相减：<c>out(i) = v1(i) - v2(i)</c>
        ''' </summary>
        Public Shared Function Subtract(v1 As Double(), v2 As Double()) As Double()
            Return Elementwise(v1, v2, Function(a, b) Vector.Subtract(Of Double)(a, b),
                               Function(x, y) SimdEngine.Subtract(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 分块并行逐元素相乘：<c>out(i) = v1(i) * v2(i)</c>
        ''' </summary>
        Public Shared Function Multiply(v1 As Double(), v2 As Double()) As Double()
            Return Elementwise(v1, v2, Function(a, b) Vector.Multiply(Of Double)(a, b),
                               Function(x, y) SimdEngine.Multiply(Of Double)(x, y))
        End Function

        Private Shared Function Elementwise(v1 As Double(), v2 As Double(),
                                            blockOp As SimdEngine.VectorBinaryOp(Of Double),
                                            scalarOp As Func(Of Double(), Double(), Double())) As Double()

            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v1.Length
            If len <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {len} vs {v2.Length}!")
            End If
            If len = 0 Then Return Array.Empty(Of Double)()
            If Not ShouldParallelize(len) Then Return scalarOp(v1, v2)

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk

            System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, len)

                    Call BinaryRange(Of Double)(v1, v2, out, start, ends, blockOp)
                End Sub)

            Return out
        End Function

        ''' <summary>
        ''' 分块并行标量乘法：<c>out(i) = scalar * v(i)</c>
        ''' </summary>
        Public Shared Function MultiplyScalar(scalar As Double, v As Double()) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()
            If Not ShouldParallelize(len) Then Return SimdEngine.MultiplyScalar(Of Double)(scalar, v)

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim chunk As Integer = ChunkSize(len)
            Dim nChunks As Integer = (len + chunk - 1) \ chunk

            ' 直接在分块内构造标量广播向量，避免再额外materialize一个和输入等长的
            ' splat 数组（在大数组下那会是一整趟多余的分配 + 读写）
            System.Threading.Tasks.Parallel.For(0, nChunks,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim ends As Integer = std.Min(start + chunk, len)

                    Call MultiplyScalarRange(v, out, start, ends, scalar)
                End Sub)

            Return out
        End Function

        Private Shared Sub MultiplyScalarRange(v As Double(), out As Double(), start As Integer, ends As Integer,
                                               scalar As Double)
            Dim count As Integer = Vector(Of Double).Count
            Dim splat As New Vector(Of Double)(scalar)
            Dim i As Integer = start

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= count Then
                Dim last As Integer = ends - count

                Do While i <= last
                    Vector.Multiply(Of Double)(New Vector(Of Double)(v, i), splat).CopyTo(out, i)
                    i += count
                Loop
                If i < ends Then
                    Vector.Multiply(Of Double)(New Vector(Of Double)(v, last), splat).CopyTo(out, last)
                End If

                Return
            End If

            Do While i < ends
                out(i) = v(i) * scalar
                i += 1
            Loop
        End Sub

#End Region

#Region "matrix"

        ''' <summary>
        ''' 计算两个矩阵的乘积，行方向并行、行内使用 SIMD 点积。
        ''' </summary>
        ''' <param name="a">左矩阵，形状 <c>nrowA x n</c></param>
        ''' <param name="b">右矩阵，形状 <c>n x ncolB</c></param>
        Public Shared Function MatrixDot(a As Double()(), b As Double()()) As Double()()
            If a Is Nothing Then Throw New ArgumentNullException(NameOf(a))
            If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))

            Dim nrowA As Integer = a.Length
            Dim n As Integer = b.Length
            Dim ncolB As Integer = If(n = 0, 0, b(0).Length)
            Dim c As Double()() = New Double(nrowA - 1)() {}

            For i As Integer = 0 To nrowA - 1
                c(i) = New Double(ncolB - 1) {}
            Next
            If nrowA = 0 OrElse n = 0 OrElse ncolB = 0 Then Return c

            ' 预先转置右矩阵，使得内层可以直接对两行做连续内存的向量点积
            Dim transposed As Double()() = Transpose(b, n, ncolB)

            If ShouldParallelize(nrowA * ncolB) Then
                System.Threading.Tasks.Parallel.For(0, nrowA,
                    Sub(i)
                        Dim row As Double() = a(i)
                        Dim dst As Double() = c(i)

                        For j As Integer = 0 To ncolB - 1
                            dst(j) = SIMDIntrinsics.DotFma(row, transposed(j))
                        Next
                    End Sub)
            Else
                For i As Integer = 0 To nrowA - 1
                    Dim row As Double() = a(i)
                    Dim dst As Double() = c(i)

                    For j As Integer = 0 To ncolB - 1
                        dst(j) = SIMDIntrinsics.DotFma(row, transposed(j))
                    Next
                Next
            End If

            Return c
        End Function

        Private Shared Function Transpose(b As Double()(), n As Integer, ncolB As Integer) As Double()()
            Dim t As Double()() = New Double(ncolB - 1)() {}

            For j As Integer = 0 To ncolB - 1
                Dim column As Double() = New Double(n - 1) {}

                For k As Integer = 0 To n - 1
                    column(k) = b(k)(j)
                Next

                t(j) = column
            Next

            Return t
        End Function

#End Region
    End Class
End Namespace
