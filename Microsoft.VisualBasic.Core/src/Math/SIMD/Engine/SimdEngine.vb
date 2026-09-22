#Region "Microsoft.VisualBasic::fab51b1a9395a6f57c16accf8ad3d932, Microsoft.VisualBasic.Core\src\Math\SIMD\Engine\SimdEngine.vb"

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

    '   Total Lines: 950
    '    Code Lines: 576 (60.63%)
    ' Comment Lines: 184 (19.37%)
    '    - Xml Docs: 96.74%
    ' 
    '   Blank Lines: 190 (20.00%)
    '     File Size: 36.90 KB


    '     Class SimdEngine
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: Add, AddAbsInPlace, AddInPlace, AddScalar, AddScalarInPlace
    '                       CanVectorize, CheckArgument, Compute, (+2 Overloads) Divide, DivideInPlace
    '                       (+2 Overloads) DivideScalar, (+2 Overloads) DivideZeroSafe, DivideZeroSafeInPlace, InPlace, InPlaceScalar
    '                       Max, MaxScalar, Min, MinScalar, Multiply
    '                       MultiplyInPlace, MultiplyScalar, MultiplyScalarInPlace, MultiplyZeroSafe, NewArray
    '                       ScalarCompute, (+2 Overloads) ScalarDivide, ScalarLane, ScalarSubtract, Subtract
    '                       SubtractInPlace, SubtractScalar
    ' 
    '             Sub: DivideZeroSafeBlock, DivideZeroSafeBlockSingle, MultiplyZeroSafeBlock
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 通用的逐元素向量化内核。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这个类型是整个 SIMD 模块的计算主干，所有运算都以
    ''' <see cref="System.Numerics.Vector(Of T)"/> 为基础：它由 JIT 自动映射到
    ''' SSE2/AVX/AVX2(<c>x86</c>) 或者 Advanced SIMD(<c>ARM64</c>)，
    ''' 因而对“按位宽自动取最大向量”这件事是跨平台正确的。
    ''' </para>
    ''' <para>
    ''' <b>长度契约</b>：这些逐元素函数假设 <c>v1</c> 与 <c>v2</c> 长度一致，
    ''' 不额外做长度校验（与旧版实现保持一致，避免在热路径上引入分支）。
    ''' 相邻数组越界会由运行时自身的边界检查捕获。
    ''' </para>
    ''' <para>
    ''' <b>尾部处理</b>：当长度不能被向量宽度整除时，会使用“最后一个向量块与
    ''' 末尾重叠”的技巧一次性覆盖剩余元素，因此循环体内不需要任何标量尾部补算
    ''' （这也是旧实现里 <c>Mod</c> 取余 + 标量循环的主要性能损失点之一）。
    ''' </para>
    ''' <para>
    ''' <b>零长度</b>：传入长度为 0 的数组会安全地返回 <see cref="Array.Empty(Of T)"/>，
    ''' 修正了旧实现中 <c>New Double(-1) {}</c> 导致的越界异常。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdEngine

        ''' <summary>
        ''' 两个向量之间的逐元素二元运算。
        ''' </summary>
        Public Delegate Function VectorBinaryOp(Of T)(a As Vector(Of T), b As Vector(Of T)) As Vector(Of T)

        ''' <summary>
        ''' 单向量逐元素一元运算。
        ''' </summary>
        Public Delegate Function VectorUnaryOp(Of T)(a As Vector(Of T)) As Vector(Of T)

        Private Sub New()
        End Sub

#Region "helpers"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function CheckArgument(Of T)(v As T(), name As String) As T()
            If v Is Nothing Then
                Throw New ArgumentNullException(name, "the input vector can not be NULL!")
            End If

            Return v
        End Function

        ''' <summary>
        ''' 分配结果数组，跳过运行时的零初始化。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <c>New T(n) {}</c> 会让运行时先把整块内存清零，再被我们的循环完整覆盖一次。
        ''' 对于大数组（例如 1e7 个 <see cref="Double"/> 就是 80MB）这次多余的清零会占用
        ''' 与真正计算同量级的内存带宽，是实测中最主要的开销之一。
        ''' <see cref="GC.AllocateUninitializedArray(Of T)"/> 可以跳过这一步。
        ''' </para>
        ''' <para>
        ''' <b>安全性</b>：只有当返回数组的每一个元素都会被显式写入时才可以使用这个方法。
        ''' 本模块中所有调用点都满足该前提（向量块 + 重叠末块，或者完整的标量循环，
        ''' 覆盖了 <c>[0, length)</c> 的全部下标）；对于元素类型包含对象引用的情况，
        ''' 运行时本身也会强制清零，因此不存在抛出未初始化引用的问题。
        ''' </para>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function NewArray(Of T)(len As Integer) As T()
            Return GC.AllocateUninitializedArray(Of T)(len)
        End Function

        ''' <summary>
        ''' 判断长度 <paramref name="len"/> 的数据是否可以走到量化路径。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CanVectorize(Of T As Structure)(len As Integer) As Boolean
            Return SIMDEnvironment.IsEnabled AndAlso len >= Vector(Of T).Count
        End Function

        ''' <summary>
        ''' 当无法走向量化路径时，使用“单通道向量”完成一次类型安全的逐元素运算。
        ''' </summary>
        ''' <remarks>
        ''' 向量在这里只是一个栈上的 <c>struct</c>，不会产生堆分配；这条路径只在
        ''' <see cref="SIMDConfiguration.disable"/> 或者数据长度小于向量宽度时使用。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function ScalarLane(Of T As Structure)(op As VectorBinaryOp(Of T), a As T, b As T) As T
            Return op(New Vector(Of T)(a), New Vector(Of T)(b)).GetElement(0)
        End Function

#End Region

#Region "vector - vector"

        ''' <summary>
        ''' 逐元素相加：<c>out(i) = v1(i) + v2(i)</c>
        ''' </summary>
        Public Shared Function Add(Of T As Structure)(v1 As T(), v2 As T()) As T()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Add(Of T)(New Vector(Of T)(v1, i), New Vector(Of T)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Add(Of T)(New Vector(Of T)(v1, last), New Vector(Of T)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = ScalarLane(Of T)(Function(a, b) Vector.Add(Of T)(a, b), v1(i), v2(i))
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相减：<c>out(i) = v1(i) - v2(i)</c>
        ''' </summary>
        Public Shared Function Subtract(Of T As Structure)(v1 As T(), v2 As T()) As T()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Subtract(Of T)(New Vector(Of T)(v1, i), New Vector(Of T)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Subtract(Of T)(New Vector(Of T)(v1, last), New Vector(Of T)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = ScalarLane(Of T)(Function(a, b) Vector.Subtract(Of T)(a, b), v1(i), v2(i))
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相乘：<c>out(i) = v1(i) * v2(i)</c>
        ''' </summary>
        Public Shared Function Multiply(Of T As Structure)(v1 As T(), v2 As T()) As T()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Multiply(Of T)(New Vector(Of T)(v1, i), New Vector(Of T)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Multiply(Of T)(New Vector(Of T)(v1, last), New Vector(Of T)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = ScalarLane(Of T)(Function(a, b) Vector.Multiply(Of T)(a, b), v1(i), v2(i))
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 带“任一因子为零则结果为零”语义的逐元素乘法。
        ''' </summary>
        ''' <remarks>
        ''' 这个语义来自旧版 <c>Vector</c> 的 <c>.*</c> 运算符实现：显式地把
        ''' <c>0 * Inf</c> 固定成 <c>0</c>，而不是让 IEEE 规则产生 <see cref="Double.NaN"/>。
        ''' 向量化实现用两次
        ''' <see cref="System.Numerics.Vector.ConditionalSelect(Of T)(System.Numerics.Vector(Of T), System.Numerics.Vector(Of T), System.Numerics.Vector(Of T))"/>
        ''' 覆盖零通道，全程不需要物化布尔掩码数组。
        ''' </remarks>
        Public Shared Function MultiplyZeroSafe(v1 As Double(), v2 As Double()) As Double()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim zero As Vector(Of Double) = Vector(Of Double).Zero
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call MultiplyZeroSafeBlock(v1, v2, out, i, zero)
                    i += count
                Loop
                If i < len Then
                    Call MultiplyZeroSafeBlock(v1, v2, out, last, zero)
                End If

                Return out
            End If

            Do While i < len
                If v1(i) = 0.0 OrElse v2(i) = 0.0 Then
                    out(i) = 0
                Else
                    out(i) = v1(i) * v2(i)
                End If

                i += 1
            Loop

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub MultiplyZeroSafeBlock(v1 As Double(), v2 As Double(), out As Double(),
                                                 offset As Integer, zero As Vector(Of Double))
            Dim a As New Vector(Of Double)(v1, offset)
            Dim b As New Vector(Of Double)(v2, offset)
            Dim product As Vector(Of Double) = Vector.Multiply(Of Double)(a, b)

            product = Vector.ConditionalSelect(Of Double)(Vector.Equals(Of Double)(a, zero), zero, product)
            product = Vector.ConditionalSelect(Of Double)(Vector.Equals(Of Double)(b, zero), zero, product)

            Call product.CopyTo(out, offset)
        End Sub

        ''' <summary>
        ''' 逐元素取最小值：<c>out(i) = Min(v1(i), v2(i))</c>
        ''' </summary>
        ''' <remarks>
        ''' 语义等同于 <see cref="System.Numerics.Vector.Min(Of T)(System.Numerics.Vector(Of T), System.Numerics.Vector(Of T))"/>。
        ''' </remarks>
        Public Shared Function Min(Of T As Structure)(v1 As T(), v2 As T()) As T()
            Return Compute(Of T)(v1, v2, Function(a, b) Vector.Min(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素取最大值：<c>out(i) = Max(v1(i), v2(i))</c>
        ''' </summary>
        Public Shared Function Max(Of T As Structure)(v1 As T(), v2 As T()) As T()
            Return Compute(Of T)(v1, v2, Function(a, b) Vector.Max(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 以委托形式给出的逐元素二元运算通用实现。
        ''' </summary>
        ''' <remarks>
        ''' 因为引入了间接调用，这个方法不适合放在最热点的主干上；
        ''' <see cref="Add(Of T)"/> 等高频算子都是手写展开的循环。
        ''' </remarks>
        Public Shared Function Compute(Of T As Structure)(v1 As T(), v2 As T(),
                                                         op As VectorBinaryOp(Of T)) As T()

            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    op(New Vector(Of T)(v1, i), New Vector(Of T)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    op(New Vector(Of T)(v1, last), New Vector(Of T)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = ScalarLane(Of T)(op, v1(i), v2(i))
                i += 1
            Loop

            Return out
        End Function

#End Region

#Region "vector - scalar"

        ''' <summary>
        ''' 向量加标量：<c>out(i) = v(i) + scalar</c>
        ''' </summary>
        Public Shared Function AddScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Add(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 向量减标量：<c>out(i) = v(i) - scalar</c>
        ''' </summary>
        Public Shared Function SubtractScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Subtract(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 标量减向量：<c>out(i) = scalar - v(i)</c>
        ''' </summary>
        Public Shared Function ScalarSubtract(Of T As Structure)(scalar As T, v As T()) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Subtract(Of T)(b, a))
        End Function

        ''' <summary>
        ''' 标量乘向量：<c>out(i) = scalar * v(i)</c>
        ''' </summary>
        Public Shared Function MultiplyScalar(Of T As Structure)(scalar As T, v As T()) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Multiply(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素取较大值：<c>out(i) = Max(v(i), scalar)</c>
        ''' </summary>
        Public Shared Function MaxScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Max(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素取较小值：<c>out(i) = Min(v(i), scalar)</c>
        ''' </summary>
        Public Shared Function MinScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return ScalarCompute(Of T)(v, scalar, Function(a, b) Vector.Min(Of T)(a, b))
        End Function

        Private Shared Function ScalarCompute(Of T As Structure)(v As T(), scalar As T,
                                                                op As VectorBinaryOp(Of T)) As T()

            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim splat As New Vector(Of T)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    op(New Vector(Of T)(v, i), splat).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    op(New Vector(Of T)(v, last), splat).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = op(New Vector(Of T)(v(i)), splat).GetElement(0)
                i += 1
            Loop

            Return out
        End Function

#End Region

#Region "in-place"

        ''' <summary>
        ''' 就地累加：<c>v(i) += operand(i)</c>
        ''' </summary>
        Public Shared Function AddInPlace(Of T As Structure)(v As T(), operand As T()) As T()
            Return InPlace(Of T)(v, operand, Function(a, b) Vector.Add(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 就地相减：<c>v(i) -= operand(i)</c>
        ''' </summary>
        Public Shared Function SubtractInPlace(Of T As Structure)(v As T(), operand As T()) As T()
            Return InPlace(Of T)(v, operand, Function(a, b) Vector.Subtract(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 就地累加绝对值：<c>v(i) += |operand(i)|</c>。
        ''' </summary>
        ''' <remarks>
        ''' 用于逐行累加绝对值的归约（例如 1-范数）：相比「先 <c>SimdMath.Abs</c> 生成
        ''' 临时数组再 <c>AddInPlace</c>」少了一整行数据的分配与往返读写。
        ''' </remarks>
        Public Shared Function AddAbsInPlace(v As Double(), operand As Double()) As Double()
            v = CheckArgument(v, NameOf(v))
            operand = CheckArgument(operand, NameOf(operand))

            Dim len As Integer = v.Length
            If len = 0 Then Return v

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                ' 就地运算不能使用重叠末块
                Do While i <= len - count
                    Vector.Add(Of Double)(
                        New Vector(Of Double)(v, i),
                        Vector.Abs(Of Double)(New Vector(Of Double)(operand, i))).CopyTo(v, i)
                    i += count
                Loop
            End If

            Do While i < len
                v(i) += std.Abs(operand(i))
                i += 1
            Loop

            Return v
        End Function

        ''' <summary>
        ''' 就地累乘：<c>v(i) *= operand(i)</c>
        ''' </summary>
        Public Shared Function MultiplyInPlace(Of T As Structure)(v As T(), operand As T()) As T()
            Return InPlace(Of T)(v, operand, Function(a, b) Vector.Multiply(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 就地累加标量：<c>v(i) += scalar</c>
        ''' </summary>
        Public Shared Function AddScalarInPlace(Of T As Structure)(v As T(), scalar As T) As T()
            Return InPlaceScalar(Of T)(v, scalar, Function(a, b) Vector.Add(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 就地累乘标量：<c>v(i) *= scalar</c>
        ''' </summary>
        Public Shared Function MultiplyScalarInPlace(Of T As Structure)(v As T(), scalar As T) As T()
            Return InPlaceScalar(Of T)(v, scalar, Function(a, b) Vector.Multiply(Of T)(a, b))
        End Function

        Private Shared Function InPlace(Of T As Structure)(v As T(), operand As T(),
                                                           op As VectorBinaryOp(Of T)) As T()

            v = CheckArgument(v, NameOf(v))
            operand = CheckArgument(operand, NameOf(operand))

            Dim len As Integer = v.Length
            If len = 0 Then Return v

            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                ' 就地运算**不能**使用“末块与末尾重叠”的技巧：输入与输出共用同一块内存，
                ' 重叠部分会把已经累加过的元素再读一次，导致尾部元素被重复计算。
                ' 因此这里只处理完整块，剩余元素退化为单通道向量逐元素计算。
                Do While i <= len - count
                    op(New Vector(Of T)(v, i), New Vector(Of T)(operand, i)).CopyTo(v, i)
                    i += count
                Loop
            End If

            Do While i < len
                v(i) = ScalarLane(Of T)(op, v(i), operand(i))
                i += 1
            Loop

            Return v
        End Function

        Private Shared Function InPlaceScalar(Of T As Structure)(v As T(), scalar As T,
                                                                 op As VectorBinaryOp(Of T)) As T()

            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return v

            Dim count As Integer = Vector(Of T).Count
            Dim splat As New Vector(Of T)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of T)(len) Then
                ' 理由同 InPlace：就地运算不能使用重叠末块
                Do While i <= len - count
                    op(New Vector(Of T)(v, i), splat).CopyTo(v, i)
                    i += count
                Loop
            End If

            Do While i < len
                v(i) = op(New Vector(Of T)(v(i)), splat).GetElement(0)
                i += 1
            Loop

            Return v
        End Function

#End Region

#Region "division"

        ''' <summary>
        ''' 逐元素相除（<see cref="Double"/>）：<c>out(i) = v1(i) / v2(i)</c>
        ''' </summary>
        Public Shared Function Divide(v1 As Double(), v2 As Double()) As Double()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Double)(New Vector(Of Double)(v1, i), New Vector(Of Double)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Double)(New Vector(Of Double)(v1, last), New Vector(Of Double)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = v1(i) / v2(i)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相除（<see cref="Single"/>）：<c>out(i) = v1(i) / v2(i)</c>
        ''' </summary>
        Public Shared Function Divide(v1 As Single(), v2 As Single()) As Single()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = NewArray(Of Single)(len)
            Dim count As Integer = Vector(Of Single).Count
            Dim i As Integer = 0

            If CanVectorize(Of Single)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Single)(New Vector(Of Single)(v1, i), New Vector(Of Single)(v2, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Single)(New Vector(Of Single)(v1, last), New Vector(Of Single)(v2, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = v1(i) / v2(i)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 带“分子为零则结果为零”语义的逐元素除法。
        ''' </summary>
        ''' <remarks>
        ''' 这个语义来自旧版的 <c>Divide.f64_op_divide_f64</c> 实现：当分子为 0 时
        ''' 直接输出 0，从而避免 <c>0 / 0</c> 产生 <see cref="Double.NaN"/>。
        ''' 向量化实现先用掩码找出分子为 0 的通道，再用
        ''' <see cref="System.Numerics.Vector.ConditionalSelect(Of T)(System.Numerics.Vector(Of T), System.Numerics.Vector(Of T), System.Numerics.Vector(Of T))"/>
        ''' 把这些通道覆盖为 0。
        ''' </remarks>
        Public Shared Function DivideZeroSafe(v1 As Double(), v2 As Double()) As Double()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim zero As Vector(Of Double) = Vector(Of Double).Zero
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call DivideZeroSafeBlock(v1, v2, out, i, zero)
                    i += count
                Loop
                If i < len Then
                    Call DivideZeroSafeBlock(v1, v2, out, last, zero)
                End If

                Return out
            End If

            Do While i < len
                If v1(i) = 0.0 Then
                    out(i) = 0
                Else
                    out(i) = v1(i) / v2(i)
                End If

                i += 1
            Loop

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub DivideZeroSafeBlock(v1 As Double(), v2 As Double(), out As Double(),
                                               offset As Integer, zero As Vector(Of Double))
            Dim numerator As New Vector(Of Double)(v1, offset)
            Dim quotient As Vector(Of Double) = Vector.Divide(Of Double)(numerator, New Vector(Of Double)(v2, offset))
            Dim mask As Vector(Of Double) = Vector.Equals(Of Double)(numerator, zero)

            Call Vector.ConditionalSelect(Of Double)(mask, zero, quotient).CopyTo(out, offset)
        End Sub

        ''' <summary>
        ''' 带“分子为零则结果为零”语义的逐元素除法（<see cref="Single"/>）。
        ''' </summary>
        ''' <remarks>
        ''' 与 <see cref="DivideZeroSafe(Double(), Double())"/> 语义一致，只是把累加/判零
        ''' 的通道宽度换成 <see cref="Single"/>（一条 256 位指令可处理 8 个通道）。
        ''' </remarks>
        Public Shared Function DivideZeroSafe(v1 As Single(), v2 As Single()) As Single()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = NewArray(Of Single)(len)
            Dim count As Integer = Vector(Of Single).Count
            Dim zero As Vector(Of Single) = Vector(Of Single).Zero
            Dim i As Integer = 0

            If CanVectorize(Of Single)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call DivideZeroSafeBlockSingle(v1, v2, out, i, zero)
                    i += count
                Loop
                If i < len Then
                    Call DivideZeroSafeBlockSingle(v1, v2, out, last, zero)
                End If

                Return out
            End If

            Do While i < len
                If v1(i) = 0.0F Then
                    out(i) = 0
                Else
                    out(i) = v1(i) / v2(i)
                End If

                i += 1
            Loop

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub DivideZeroSafeBlockSingle(v1 As Single(), v2 As Single(), out As Single(),
                                                     offset As Integer, zero As Vector(Of Single))
            Dim numerator As New Vector(Of Single)(v1, offset)
            Dim quotient As Vector(Of Single) = Vector.Divide(Of Single)(numerator, New Vector(Of Single)(v2, offset))
            Dim mask As Vector(Of Single) = Vector.Equals(Of Single)(numerator, zero)

            Call Vector.ConditionalSelect(Of Single)(mask, zero, quotient).CopyTo(out, offset)
        End Sub

        ''' <summary>
        ''' 就地逐元素相除（<see cref="Double"/>）：<c>v1(i) = v1(i) / v2(i)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 就地版**不能**使用“末块与末尾重叠”的技巧：输入与输出共用同一块内存，
        ''' 重叠部分会把已经写过的元素再算一次，因此尾块退化为单通道逐元素计算。
        ''' </remarks>
        Public Shared Function DivideInPlace(v1 As Double(), v2 As Double()) As Double()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return v1

            Dim count As Integer = Vector(Of Double).Count
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Do While i <= len - count
                    Vector.Divide(Of Double)(New Vector(Of Double)(v1, i), New Vector(Of Double)(v2, i)).CopyTo(v1, i)
                    i += count
                Loop
            End If

            Do While i < len
                v1(i) = v1(i) / v2(i)
                i += 1
            Loop

            Return v1
        End Function

        ''' <summary>
        ''' 就地“分子为零则结果为零”的逐元素除法（<see cref="Double"/>）。
        ''' </summary>
        Public Shared Function DivideZeroSafeInPlace(v1 As Double(), v2 As Double()) As Double()
            v1 = CheckArgument(v1, NameOf(v1))
            v2 = CheckArgument(v2, NameOf(v2))

            Dim len As Integer = v1.Length
            If len = 0 Then Return v1

            Dim count As Integer = Vector(Of Double).Count
            Dim zero As Vector(Of Double) = Vector(Of Double).Zero
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                ' DivideZeroSafeBlock 会先把分子装载到向量寄存器再回写，因此 out 与 v1 重合是安全的
                Do While i <= len - count
                    Call DivideZeroSafeBlock(v1, v2, v1, i, zero)
                    i += count
                Loop
            End If

            Do While i < len
                If v1(i) = 0.0 Then
                    v1(i) = 0
                Else
                    v1(i) = v1(i) / v2(i)
                End If

                i += 1
            Loop

            Return v1
        End Function

        ''' <summary>
        ''' 向量除以标量（<see cref="Double"/>）：<c>out(i) = v(i) / scalar</c>
        ''' </summary>
        Public Shared Function DivideScalar(v As Double(), scalar As Double) As Double()
            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim splat As New Vector(Of Double)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Double)(New Vector(Of Double)(v, i), splat).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Double)(New Vector(Of Double)(v, last), splat).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = v(i) / scalar
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 向量除以标量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function DivideScalar(v As Single(), scalar As Single) As Single()
            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = NewArray(Of Single)(len)
            Dim count As Integer = Vector(Of Single).Count
            Dim splat As New Vector(Of Single)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of Single)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Single)(New Vector(Of Single)(v, i), splat).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Single)(New Vector(Of Single)(v, last), splat).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = v(i) / scalar
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 标量除以向量（<see cref="Double"/>）：<c>out(i) = scalar / v(i)</c>
        ''' </summary>
        Public Shared Function ScalarDivide(scalar As Double, v As Double()) As Double()
            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim splat As New Vector(Of Double)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of Double)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Double)(splat, New Vector(Of Double)(v, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Double)(splat, New Vector(Of Double)(v, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = scalar / v(i)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 标量除以向量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function ScalarDivide(scalar As Single, v As Single()) As Single()
            v = CheckArgument(v, NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = NewArray(Of Single)(len)
            Dim count As Integer = Vector(Of Single).Count
            Dim splat As New Vector(Of Single)(scalar)
            Dim i As Integer = 0

            If CanVectorize(Of Single)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Divide(Of Single)(splat, New Vector(Of Single)(v, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Divide(Of Single)(splat, New Vector(Of Single)(v, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = scalar / v(i)
                i += 1
            Loop

            Return out
        End Function

#End Region
    End Class
End Namespace
