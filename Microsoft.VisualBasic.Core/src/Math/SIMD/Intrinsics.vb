#Region "Microsoft.VisualBasic::41ef72abb6c48bf171e0fade525dbf9d, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Intrinsics.vb"

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

    '   Total Lines: 301
    '    Code Lines: 176 (58.47%)
    ' Comment Lines: 74 (24.58%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 51 (16.94%)
    '     File Size: 11.84 KB


    '     Class SIMDIntrinsics
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Axpy, (+2 Overloads) DotFma, HorizontalSum4, HorizontalSum8, Load4
    '                   Load8, MultiplyAdd, SumSquaresFma, (+2 Overloads) VectorAddAvx, (+2 Overloads) VectorAddAvx2
    ' 
    '         Sub: Store4
    ' 
    ' 
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
    ''' 所有的 FMA 内核都会在运行期同时检查 <see cref="SimdCapabilities.IsFma"/> 与
    ''' <see cref="SIMDEnvironment.IsEnabled"/>：前者保证处理器确实有 FMA 指令，
    ''' 后者保证 <see cref="SIMDConfiguration.disable"/> 这个全局逃生开关依然有效。
    ''' 任一条件不满足时会退回到等价的 <see cref="SimdEngine"/> /
    ''' <see cref="SimdReduce"/> 实现，因此这些函数在任何平台与任何配置下都是安全的。
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
        ''' <remarks>
        ''' 与 <see cref="SumSquaresFma(Double())"/> 一样使用 4 路独立累加器来打断
        ''' FMA 的依赖链；归约顺序不同会带来 ULP 级差异。
        ''' </remarks>
        Public Shared Function DotFma(v1 As Double(), v2 As Double()) As Double
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"vector size not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return 0.0
            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
                Return SimdReduce.Dot(v1, v2)
            End If

            Dim count As Integer = Vector256(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim acc0 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc1 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc2 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc3 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim i As Integer = 0

            If len >= step4 Then
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    acc0 = Fma.MultiplyAdd(Load4(v1, i), Load4(v2, i), acc0)
                    acc1 = Fma.MultiplyAdd(Load4(v1, i + count), Load4(v2, i + count), acc1)
                    acc2 = Fma.MultiplyAdd(Load4(v1, i + count * 2), Load4(v2, i + count * 2), acc2)
                    acc3 = Fma.MultiplyAdd(Load4(v1, i + count * 3), Load4(v2, i + count * 3), acc3)
                    i += step4
                Loop
            End If

            ' 不足 4 路的整块继续用单路累加
            Do While i <= len - count
                acc0 = Fma.MultiplyAdd(Load4(v1, i), Load4(v2, i), acc0)
                i += count
            Loop

            Dim sum As Double = HorizontalSum4(
                Vector256.Add(Vector256.Add(acc0, acc1), Vector256.Add(acc2, acc3)))

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
            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
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
        ''' <remarks>
        ''' <para>
        ''' 使用 <b>4 路独立累加器</b>：FMA 的延时约为 4 个周期，若只用一个累加器，
        ''' 整个循环会被这条依赖链串行化，吞吐无法超过「1 条 FMA / 4 周期」；
        ''' 4 路累加器让乱序执行可以同时保持多条 FMA 在飞，长数组上能拿到接近
        ''' 3~4 倍的额外提升（这也是 <see cref="SimdReduce.SumSquares(Double())"/>
        ''' 采用同样策略的原因）。
        ''' </para>
        ''' <para>
        ''' 归约顺序与单累加器版本不同，因此结果可能存在浮点末位（ULP）级差异。
        ''' </para>
        ''' </remarks>
        Public Shared Function SumSquaresFma(v As Double()) As Double
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
            If v.Length = 0 Then Return 0.0
            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
                Return SimdReduce.SumSquares(v)
            End If

            Dim len As Integer = v.Length
            Dim count As Integer = Vector256(Of Double).Count
            Dim step4 As Integer = count * 4
            Dim acc0 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc1 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc2 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim acc3 As Vector256(Of Double) = Vector256(Of Double).Zero
            Dim i As Integer = 0

            If len >= step4 Then
                Dim last4 As Integer = len - step4

                Do While i <= last4
                    Dim x0 As Vector256(Of Double) = Load4(v, i)
                    Dim x1 As Vector256(Of Double) = Load4(v, i + count)
                    Dim x2 As Vector256(Of Double) = Load4(v, i + count * 2)
                    Dim x3 As Vector256(Of Double) = Load4(v, i + count * 3)

                    acc0 = Fma.MultiplyAdd(x0, x0, acc0)
                    acc1 = Fma.MultiplyAdd(x1, x1, acc1)
                    acc2 = Fma.MultiplyAdd(x2, x2, acc2)
                    acc3 = Fma.MultiplyAdd(x3, x3, acc3)
                    i += step4
                Loop
            End If

            ' 不足 4 路的整块继续用单路累加
            Do While i <= len - count
                Dim x As Vector256(Of Double) = Load4(v, i)

                acc0 = Fma.MultiplyAdd(x, x, acc0)
                i += count
            Loop

            Dim sum As Double = HorizontalSum4(
                Vector256.Add(Vector256.Add(acc0, acc1), Vector256.Add(acc2, acc3)))

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
            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
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
        ''' 就地 AXPY：<c>y(i) += alpha * x(i)</c>，使用 FMA 融合乘加。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 这是矩阵分解/求解器里出现频率最高的一类 BLAS-1 操作（列消元、Gram-Schmidt
        ''' 正交化、秩一更新的行部分），就地更新可以避免为每次迭代分配临时数组。
        ''' </para>
        ''' <para>
        ''' <b>为什么不使用“末块与末尾重叠”的技巧</b>：输入与输出共用同一块内存，
        ''' 重叠部分会把已经更新过的元素再算一次，因此尾块退化为单通道逐元素计算。
        ''' </para>
        ''' </remarks>
        Public Shared Sub AxpyInPlace(alpha As Double, x As Double(), y As Double())
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))

            Dim len As Integer = y.Length
            If len = 0 Then Return
            If x.Length <> len Then
                Throw New ArgumentException($"vector size not agree: {x.Length} vs {len}!")
            End If

            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
                ' 无 FMA 时退化为“先数乘再就地累加”，仍然是向量化路径
                Dim scaled As Double() = SimdEngine.MultiplyScalar(Of Double)(alpha, x)

                Call SimdEngine.AddInPlace(Of Double)(y, scaled)
                Return
            End If

            Dim count As Integer = Vector256(Of Double).Count
            Dim a As Vector256(Of Double) = Vector256.Create(Of Double)(alpha)
            Dim i As Integer = 0

            Do While i <= len - count
                Call Store4(Fma.MultiplyAdd(a, Load4(x, i), Load4(y, i)), y, i)
                i += count
            Loop

            Do While i < len
                y(i) += alpha * x(i)
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' 融合乘加：<c>out(i) = v1(i) * v2(i) + acc(i)</c>
        ''' </summary>
        Public Shared Function MultiplyAdd(v1 As Double(), v2 As Double(), acc As Double()) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If acc Is Nothing Then Throw New ArgumentNullException(NameOf(acc))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()
            If Not SimdCapabilities.IsFma OrElse Not SIMDEnvironment.IsEnabled Then
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
