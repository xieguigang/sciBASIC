#Region "Microsoft.VisualBasic::simdIntrinsics::Extensions\Math\SIMD\Intrinsics.vb"

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

    '     Class SIMDIntrinsics

    '         Function: Axpy, DotFma, HorizontalSum4, MultiplyAdd, SumSquaresFma, VectorAddAvx, VectorAddAvx2

    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86

Namespace Math.SIMD

    ''' <summary>
    ''' 基于 <c>System.Runtime.Intrinsics.X86</c> 的指令级融合内核。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这里的函数都只负责“<see cref="Double"/> 与 <see cref="Single"/> 的 256 位
    ''' 融合乘加(FMA)”这一类**确实存在指令级收益**的场景；普通的逐元素加减乘除
    ''' 请使用 <see cref="SimdEngine"/> —— 因为 <c>Vector256(Of Double)</c> 的宽度
    ''' 与 <c>Vector(Of Double).Count</c> 完全一致，再单独写一条 256 位路径只会
    ''' 增加代码量而不会提升吞吐。
    ''' </para>
    ''' <para>
    ''' 所有的 FMA 内核都会在运行期检查 <see cref="SimdCapabilities.IsFma"/>：
    ''' 当处理器不支持 FMA 时自动退回到等价的
    ''' <see cref="SimdEngine"/> 实现，因此这些函数在任何平台上都是安全的。
    ''' </para>
    ''' <para>
    ''' 与旧实现的关键差别：这里使用批量装载/存储
    ''' (<c>New Vector256(Of T)(array, offset)</c> + <c>Vector256(Of T).CopyTo</c>)
    ''' 取代旧版的 <c>Vector256.Create(...)</c> 逐元素构造 + <c>GetElement</c>
    ''' 逐元素回写（后者会因为多次插入/提取通道而失去向量化的全部收益）。
    ''' </para>
    ''' </remarks>
    Public Class SIMDIntrinsics

        Private Sub New()
        End Sub

        ''' <summary>
        ''' <see cref="Double"/> 的 4 通道水平求和。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function HorizontalSum4(v As Vector256(Of Double)) As Double
            Return v.GetElement(0) + v.GetElement(1) + v.GetElement(2) + v.GetElement(3)
        End Function

        ''' <summary>
        ''' 从 <see cref="Double"/> 数组的 <paramref name="offset"/> 处批量装载一个 256 位向量。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Load4(v As Double(), offset As Integer) As Vector256(Of Double)
            Return Vector256.LoadUnsafe(Of Double)(v(offset))
        End Function

        ''' <summary>
        ''' 从 <see cref="Single"/> 数组的 <paramref name="offset"/> 处批量装载一个 256 位向量。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Load8(v As Single(), offset As Integer) As Vector256(Of Single)
            Return Vector256.LoadUnsafe(Of Single)(v(offset))
        End Function

        ''' <summary>
        ''' 把一个 256 位向量批量写回 <see cref="Double"/> 数组。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub Store4(value As Vector256(Of Double), v As Double(), offset As Integer)
            Call Vector256.StoreUnsafe(value, v(offset))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function HorizontalSum8(v As Vector256(Of Single)) As Double
            Dim sum As Double = 0

            For i As Integer = 0 To 7
                sum += v.GetElement(i)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Double"/>）。
        ''' </summary>
        ''' <remarks>
        ''' 保留这个名称仅为兼容旧代码；内部走 <see cref="SimdEngine.Add(Of T)"/>
        ''' 这条跨平台的主干路径。
        ''' </remarks>
        Public Shared Function VectorAddAvx(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Add(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function VectorAddAvx(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Add(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Double"/>）。
        ''' </summary>
        ''' <remarks>
        ''' 保留这个名称仅为兼容旧代码；对 <see cref="Double"/> 而言 AVX2 与 AVX
        ''' 的浮点加法是同一条指令，因此与 <see cref="VectorAddAvx(Double(), Double())"/>
        ''' 完全等价。
        ''' </remarks>
        Public Shared Function VectorAddAvx2(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Add(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function VectorAddAvx2(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Add(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 点积（<see cref="Double"/>），使用 FMA 融合乘加。
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns><c>SUM(v1(i) * v2(i))</c></returns>
        Public Shared Function DotFma(v1 As Double(), v2 As Double()) As Double
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"vector size not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return 0.0
            If Not SimdCapabilities.IsFma Then
                Return SimdReduce.Dot(v1, v2)
            End If

            Dim count As Integer = Vector256(Of Double).Count
            Dim acc As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim i As Integer = 0

            If len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    acc = Fma.MultiplyAdd(Load4(v1, i), Load4(v2, i), acc)
                    i += count
                Loop
            End If

            Dim sum As Double = HorizontalSum4(acc)

            Do While i < len
                sum += v1(i) * v2(i)
                i += 1
            Loop

            Return sum
        End Function

        ''' <summary>
        ''' 点积（<see cref="Single"/>），使用 FMA 融合乘加并以 <see cref="Double"/> 累加。
        ''' </summary>
        Public Shared Function DotFma(v1 As Single(), v2 As Single()) As Double
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"vector size not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return 0.0
            If Not SimdCapabilities.IsFma Then
                Return SimdReduce.Dot(v1, v2)
            End If

            Dim count As Integer = Vector256(Of Single).Count
            Dim acc As Vector256(Of Single) = Vector256(Of Single).Zero
            Dim i As Integer = 0

            If len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    acc = Fma.MultiplyAdd(Load8(v1, i), Load8(v2, i), acc)
                    i += count
                Loop
            End If

            Dim sum As Double = HorizontalSum8(acc)

            Do While i < len
                sum += v1(i) * v2(i)
                i += 1
            Loop

            Return sum
        End Function

        ''' <summary>
        ''' 平方和：<c>SUM(v(i) ^ 2)</c>，使用 FMA 融合乘加。
        ''' </summary>
        Public Shared Function SumSquaresFma(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
            If v.Length = 0 Then Return 0.0
            If Not SimdCapabilities.IsFma Then
                Return SimdReduce.SumSquares(v)
            End If

            Dim len As Integer = v.Length
            Dim count As Integer = Vector256(Of Double).Count
            Dim acc As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim i As Integer = 0

            If len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    acc = Fma.MultiplyAdd(Load4(v, i), Load4(v, i), acc)
                    i += count
                Loop
            End If

            Dim sum As Double = HorizontalSum4(acc)

            Do While i < len
                sum += v(i) * v(i)
                i += 1
            Loop

            Return sum
        End Function

        ''' <summary>
        ''' AXPY：<c>out(i) = alpha * x(i) + y(i)</c>，使用 FMA 融合乘加。
        ''' </summary>
        Public Shared Function Axpy(alpha As Double, x As Double(), y As Double()) As Double()
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))

            Dim len As Integer = x.Length
            If len = 0 Then Return Array.Empty(Of Double)()
            If Not SimdCapabilities.IsFma Then
                Return SimdEngine.Add(Of Double)(SimdEngine.MultiplyScalar(Of Double)(alpha, x), y)
            End If

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim count As Integer = Vector256(Of Double).Count
            Dim a As Vector256(Of Double) = Vector256.Create(Of Double)(alpha)
            Dim i As Integer = 0

            If len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call Store4(Fma.MultiplyAdd(a, Load4(x, i), Load4(y, i)), out, i)
                    i += count
                Loop
            End If

            Do While i < len
                out(i) = alpha * x(i) + y(i)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 融合乘加：<c>out(i) = v1(i) * v2(i) + acc(i)</c>
        ''' </summary>
        Public Shared Function MultiplyAdd(v1 As Double(), v2 As Double(), acc As Double()) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If acc Is Nothing Then Throw New ArgumentNullException(NameOf(acc))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()
            If Not SimdCapabilities.IsFma Then
                Return SimdEngine.Add(Of Double)(SimdEngine.Multiply(Of Double)(v1, v2), acc)
            End If

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim count As Integer = Vector256(Of Double).Count
            Dim i As Integer = 0

            If len >= count Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call Store4(Fma.MultiplyAdd(Load4(v1, i), Load4(v2, i), Load4(acc, i)), out, i)
                    i += count
                Loop
            End If

            Do While i < len
                out(i) = v1(i) * v2(i) + acc(i)
                i += 1
            Loop

            Return out
        End Function
    End Class
End Namespace
