#Region "Microsoft.VisualBasic::58fc4291569317b0f816a29bbe38e838, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Engine\SimdReduce.vb"

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

    '   Total Lines: 591
    '    Code Lines: 382 (64.64%)
    ' Comment Lines: 75 (12.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 134 (22.67%)
    '     File Size: 22.90 KB


    '     Class SimdReduce
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ArgMax, ArgMin, (+2 Overloads) Dot, (+2 Overloads) L1Norm, L2Norm
    '                   (+3 Overloads) Max, (+2 Overloads) Mean, (+2 Overloads) Min, (+2 Overloads) Sum, SumScalar
    '                   SumSquares, SumTail
    ' 
    '         Sub: CheckAgree, CheckEmpty, CheckNull, CheckRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 归约运算（把整个向量折叠成一个标量）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <see cref="SimdEngine"/> 不同，这里的函数**会**校验输入长度并抛出
    ''' <see cref="ArgumentException"/>：归约运算的长度不一致是调用方的逻辑错误，
    ''' 静默截断只会带来更难定位的数值 bug。
    ''' </para>
    ''' <para>
    ''' 求和/点积类内核使用 4 路累加器来打断浮点加法的依赖链（<c>vaddpd</c> 的
    ''' 延时大约是 3~4 个周期，单累加器会让吞吐被延时吃满），在长数组上可以拿到
    ''' 接近 4 倍的额外提升。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdReduce

        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckNull(v As Array, name As String)
            If v Is Nothing Then
                Throw New ArgumentNullException(name, "the input vector can not be NULL!")
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckAgree(len1 As Integer, len2 As Integer)
            If len1 <> len2 Then
                Throw New ArgumentException($"the length of the two vectors not agree: {len1} vs {len2}!")
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckEmpty(len As Integer, name As String)
            If len = 0 Then
                Throw New ArgumentException($"the input vector '{name}' can not be empty for a reduce operation!")
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CheckRange(len As Integer, start As Integer, ends As Integer)
            If start < 0 OrElse ends > len OrElse start >= ends Then
                Throw New ArgumentOutOfRangeException($"invalid range [{start}, {ends}) for a vector of length {len}!")
            End If
        End Sub

#Region "sum"

        ''' <summary>
        ''' 求和：<c>SUM(v)</c>。空数组返回 0。
        ''' </summary>
        Public Shared Function Sum(v As Double()) As Double
            CheckNull(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, New Vector(Of Double)(v, i))
                    acc1 = Vector.Add(Of Double)(acc1, New Vector(Of Double)(v, i + count))
                    acc2 = Vector.Add(Of Double)(acc2, New Vector(Of Double)(v, i + count * 2))
                    acc3 = Vector.Add(Of Double)(acc3, New Vector(Of Double)(v, i + count * 3))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))
                Dim sum4 As Double = Vector.Dot(Of Double)(acc0, ones)

                Return sum4 + SumTail(v, i)
            End If

            Return SumScalar(v, i, len)
        End Function

        ''' <summary>
        ''' 单线程标量求和（同时用作纯标量回退路径）。
        ''' </summary>
        Private Shared Function SumScalar(v As Double(), offset As Integer, len As Integer) As Double
            Dim sum As Double = 0

            For i As Integer = offset To len - 1
                sum += v(i)
            Next

            Return sum
        End Function

        Private Shared Function SumTail(v As Double(), offset As Integer) As Double
            Dim sum As Double = 0

            For i As Integer = offset To v.Length - 1
                sum += v(i)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 求和：<c>SUM(v)</c>。空数组返回 0。
        ''' </summary>
        Public Shared Function Sum(v As Single()) As Single
            CheckNull(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0F

            Dim count As Integer = Vector(Of Single).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Single)(1.0F)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc1 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc2 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc3 As Vector(Of Single) = Vector(Of Single).Zero
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Single)(acc0, New Vector(Of Single)(v, i))
                    acc1 = Vector.Add(Of Single)(acc1, New Vector(Of Single)(v, i + count))
                    acc2 = Vector.Add(Of Single)(acc2, New Vector(Of Single)(v, i + count * 2))
                    acc3 = Vector.Add(Of Single)(acc3, New Vector(Of Single)(v, i + count * 3))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Single)(Vector.Add(Of Single)(acc0, acc1),
                                             Vector.Add(Of Single)(acc2, acc3))

                Dim total As Single = Vector.Dot(Of Single)(acc0, ones)

                For k As Integer = i To len - 1
                    total += v(k)
                Next

                Return total
            End If

            Dim fallback As Single = 0

            For k As Integer = 0 To len - 1
                fallback += v(k)
            Next

            Return fallback
        End Function

        ''' <summary>
        ''' 平方和：<c>SUM(v(i) ^ 2)</c>。空数组返回 0。
        ''' </summary>
        Public Shared Function SumSquares(v As Double()) As Double
            CheckNull(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = len - step4
                Dim x0, x1, x2, x3 As Vector(Of Double)

                Do While i <= last4
                    x0 = New Vector(Of Double)(v, i)
                    x1 = New Vector(Of Double)(v, i + count)
                    x2 = New Vector(Of Double)(v, i + count * 2)
                    x3 = New Vector(Of Double)(v, i + count * 3)

                    acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(x0, x0))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(x1, x1))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(x2, x2))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(x3, x3))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))

                Dim sum As Double = Vector.Dot(Of Double)(acc0, ones)

                For k As Integer = i To len - 1
                    sum += v(k) * v(k)
                Next

                Return sum
            End If

            Dim fallback As Double = 0

            For k As Integer = 0 To len - 1
                fallback += v(k) * v(k)
            Next

            Return fallback
        End Function

#End Region

#Region "dot"

        ''' <summary>
        ''' 点积：<c>SUM(v1(i) * v2(i))</c>。
        ''' </summary>
        ''' <remarks>
        ''' 这个实现不使用 FMA 指令（以保持与 <see cref="SIMDIntrinsics.DotFma(Double(), Double())"/>
        ''' 的依赖方向单纯，避免相互递归）；需要 FMA 版本请直接调用
        ''' <see cref="SIMDIntrinsics.DotFma(Double(), Double())"/>，
        ''' 它会在这个实现之上自动选择更快的路径。
        ''' </remarks>
        Public Shared Function Dot(v1 As Double(), v2 As Double()) As Double
            CheckNull(v1, NameOf(v1))
            CheckNull(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            CheckAgree(len, v2.Length)
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i), New Vector(Of Double)(v2, i)))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count), New Vector(Of Double)(v2, i + count)))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count * 2), New Vector(Of Double)(v2, i + count * 2)))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(New Vector(Of Double)(v1, i + count * 3), New Vector(Of Double)(v2, i + count * 3)))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))

                Dim sum As Double = Vector.Dot(Of Double)(acc0, ones)

                For k As Integer = i To len - 1
                    sum += v1(k) * v2(k)
                Next

                Return sum
            End If

            Dim fallback As Double = 0

            For k As Integer = 0 To len - 1
                fallback += v1(k) * v2(k)
            Next

            Return fallback
        End Function

        ''' <summary>
        ''' 点积：<c>SUM(v1(i) * v2(i))</c>，累加结果为 <see cref="Double"/>。
        ''' </summary>
        Public Shared Function Dot(v1 As Single(), v2 As Single()) As Double
            CheckNull(v1, NameOf(v1))
            CheckNull(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            CheckAgree(len, v2.Length)
            If len = 0 Then Return 0.0

            Dim count As Integer = Vector(Of Single).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Single)(1.0F)
            Dim i As Integer = 0
            Dim sum As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= step4 Then
                Dim acc0 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc1 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc2 As Vector(Of Single) = Vector(Of Single).Zero
                Dim acc3 As Vector(Of Single) = Vector(Of Single).Zero
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Single)(acc0, Vector.Multiply(Of Single)(New Vector(Of Single)(v1, i), New Vector(Of Single)(v2, i)))
                    acc1 = Vector.Add(Of Single)(acc1, Vector.Multiply(Of Single)(New Vector(Of Single)(v1, i + count), New Vector(Of Single)(v2, i + count)))
                    acc2 = Vector.Add(Of Single)(acc2, Vector.Multiply(Of Single)(New Vector(Of Single)(v1, i + count * 2), New Vector(Of Single)(v2, i + count * 2)))
                    acc3 = Vector.Add(Of Single)(acc3, Vector.Multiply(Of Single)(New Vector(Of Single)(v1, i + count * 3), New Vector(Of Single)(v2, i + count * 3)))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Single)(Vector.Add(Of Single)(acc0, acc1),
                                             Vector.Add(Of Single)(acc2, acc3))
                sum = Vector.Dot(Of Single)(acc0, ones)
            End If

            For k As Integer = i To len - 1
                sum += CDbl(v1(k)) * CDbl(v2(k))
            Next

            Return sum
        End Function

#End Region

#Region "min max"

        ''' <summary>
        ''' 最小值。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function Min(v As Double()) As Double
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Min(v, 0, v.Length)
        End Function

        ''' <summary>
        ''' 求 <c>[start, ends)</c> 区间（前闭后开）的最小值。
        ''' </summary>
        Public Shared Function Min(v As Double(), start As Integer, ends As Integer) As Double
            CheckNull(v, NameOf(v))
            CheckRange(v.Length, start, ends)

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = start

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= count Then
                Dim acc0 As Vector(Of Double) = New Vector(Of Double)(v, start)
                Dim last As Integer = ends - count

                i = start + count

                Do While i <= last
                    acc0 = Vector.Min(Of Double)(acc0, New Vector(Of Double)(v, i))
                    i += count
                Loop

                Dim m As Double = acc0.GetElement(0)

                For k As Integer = 1 To count - 1
                    m = std.Min(m, acc0.GetElement(k))
                Next

                For k As Integer = i To ends - 1
                    m = std.Min(m, v(k))
                Next

                Return m
            End If

            Dim fallback As Double = v(start)

            For k As Integer = start + 1 To ends - 1
                fallback = std.Min(fallback, v(k))
            Next

            Return fallback
        End Function

        ''' <summary>
        ''' 最大值。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function Max(v As Double()) As Double
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Max(v, 0, v.Length)
        End Function

        ''' <summary>
        ''' 求 <c>[start, ends)</c> 区间（前闭后开）的最大值。
        ''' </summary>
        Public Shared Function Max(v As Double(), start As Integer, ends As Integer) As Double
            CheckNull(v, NameOf(v))
            CheckRange(v.Length, start, ends)

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = start

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= count Then
                Dim acc0 As Vector(Of Double) = New Vector(Of Double)(v, start)
                Dim last As Integer = ends - count

                i = start + count

                Do While i <= last
                    acc0 = Vector.Max(Of Double)(acc0, New Vector(Of Double)(v, i))
                    i += count
                Loop

                Dim m As Double = acc0.GetElement(0)

                For k As Integer = 1 To count - 1
                    m = std.Max(m, acc0.GetElement(k))
                Next

                For k As Integer = i To ends - 1
                    m = std.Max(m, v(k))
                Next

                Return m
            End If

            Dim fallback As Double = v(start)

            For k As Integer = start + 1 To ends - 1
                fallback = std.Max(fallback, v(k))
            Next

            Return fallback
        End Function

        ''' <summary>
        ''' 最大值。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function Max(v As Single()) As Single
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Dim count As Integer = Vector(Of Single).Count
            Dim len As Integer = v.Length
            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso len >= count Then
                Dim acc0 As Vector(Of Single) = New Vector(Of Single)(v, 0)
                Dim last As Integer = len - count

                i = count

                Do While i <= last
                    acc0 = Vector.Max(Of Single)(acc0, New Vector(Of Single)(v, i))
                    i += count
                Loop

                Dim m As Single = acc0.GetElement(0)

                For k As Integer = 1 To count - 1
                    m = std.Max(m, acc0.GetElement(k))
                Next

                For k As Integer = i To len - 1
                    m = std.Max(m, v(k))
                Next

                Return m
            End If

            Dim fallback As Single = v(0)

            For k As Integer = 1 To len - 1
                fallback = std.Max(fallback, v(k))
            Next

            Return fallback
        End Function

#End Region

#Region "norms and statistics"

        ''' <summary>
        ''' 均值：<c>SUM(v) / N</c>。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function Mean(v As Double()) As Double
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Sum(v) / v.Length
        End Function

        ''' <summary>
        ''' 均值：<c>SUM(v) / N</c>。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function Mean(v As Single()) As Single
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Sum(v) / v.Length
        End Function

        ''' <summary>
        ''' L1 范数：<c>SUM(|v(i)|)</c>。空数组返回 0。
        ''' </summary>
        Public Shared Function L1Norm(v As Double()) As Double
            CheckNull(v, NameOf(v))

            If v.Length = 0 Then Return 0.0

            Return L1Norm(v, 0, v.Length)
        End Function

        ''' <summary>
        ''' 求 <c>[start, ends)</c> 区间（前闭后开）的 L1 范数。
        ''' </summary>
        Public Shared Function L1Norm(v As Double(), start As Integer, ends As Integer) As Double
            CheckNull(v, NameOf(v))
            CheckRange(v.Length, start, ends)

            Dim count As Integer = Vector(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim ones As New Vector(Of Double)(1.0)
            Dim i As Integer = start
            Dim total As Double = 0

            If SIMDEnvironment.IsEnabled AndAlso ends - start >= step4 Then
                Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
                Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
                Dim last4 As Integer = ends - step4

                Do While i <= last4
                    acc0 = Vector.Add(Of Double)(acc0, Vector.Abs(Of Double)(New Vector(Of Double)(v, i)))
                    acc1 = Vector.Add(Of Double)(acc1, Vector.Abs(Of Double)(New Vector(Of Double)(v, i + count)))
                    acc2 = Vector.Add(Of Double)(acc2, Vector.Abs(Of Double)(New Vector(Of Double)(v, i + count * 2)))
                    acc3 = Vector.Add(Of Double)(acc3, Vector.Abs(Of Double)(New Vector(Of Double)(v, i + count * 3)))
                    i += step4
                Loop

                acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1),
                                             Vector.Add(Of Double)(acc2, acc3))
                total = Vector.Dot(Of Double)(acc0, ones)
            End If

            For k As Integer = i To ends - 1
                total += std.Abs(v(k))
            Next

            Return total
        End Function

        ''' <summary>
        ''' L2 范数（欧几里得范数）：<c>SQRT(SUM(v(i) ^ 2))</c>。空数组返回 0。
        ''' </summary>
        Public Shared Function L2Norm(v As Double()) As Double
            CheckNull(v, NameOf(v))

            Return std.Sqrt(SumSquares(v))
        End Function

        ''' <summary>
        ''' 最大值所在的下标（第一个匹配项）。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function ArgMax(v As Double()) As Integer
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Array.IndexOf(v, Max(v))
        End Function

        ''' <summary>
        ''' 最小值所在的下标（第一个匹配项）。空数组会抛出 <see cref="ArgumentException"/>。
        ''' </summary>
        Public Shared Function ArgMin(v As Double()) As Integer
            CheckNull(v, NameOf(v))
            CheckEmpty(v.Length, NameOf(v))

            Return Array.IndexOf(v, Min(v))
        End Function

#End Region
    End Class
End Namespace
