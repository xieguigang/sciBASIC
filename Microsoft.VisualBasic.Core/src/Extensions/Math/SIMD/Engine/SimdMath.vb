#Region "Microsoft.VisualBasic::5ab9db360a5ece14b5fcb326fbbdfc37, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Engine\SimdMath.vb"

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

    '   Total Lines: 353
    '    Code Lines: 192 (54.39%)
    ' Comment Lines: 85 (24.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 76 (21.53%)
    '     File Size: 13.44 KB


    '     Class SimdMath
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Abs, Clamp, (+2 Overloads) Exp, (+2 Overloads) Log, Negate
    '                   Pow, PowScalar, (+2 Overloads) Reciprocal, (+2 Overloads) Sqrt, Square
    '                   Unary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 单目（逐元素一元）向量化的数学函数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>设计说明</b>：只有存在对应硬件指令的运算才会被向量化。
    ''' <see cref="Sqrt"/>/<see cref="Abs"/>/<see cref="Negate"/>/<see cref="Square"/>/<see cref="Clamp"/>/
    ''' <see cref="Reciprocal"/> 都可以直接映射到 <see cref="System.Numerics.Vector(Of T)"/> 原语；
    ''' 而 <see cref="Exp"/> 与 <see cref="Log"/> 在主流 x86/ARM 处理器上**没有**逐元素硬件指令
    ''' （仅 AVX-512 的 ER/PF 子集提供近似指令，且精度与可用性都不足以作为默认实现），
    ''' 因此这两个函数保持与 <see cref="System.Math"/> 完全一致的标量实现，以保证数值结果的
    ''' 确定性；对它们做“看似向量化”的多项式逼近会静默牺牲精度，这里不做这种取舍。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdMath

        Private Sub New()
        End Sub

#Region "generic unary"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Unary(Of T As Structure)(v As T(), op As SimdEngine.VectorUnaryOp(Of T)) As T()
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = SimdEngine.NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If SimdEngine.CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    op(New Vector(Of T)(v, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    op(New Vector(Of T)(v, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = op(New Vector(Of T)(v(i))).GetElement(0)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素绝对值：<c>out(i) = |v(i)|</c>
        ''' </summary>
        Public Shared Function Abs(Of T As Structure)(v As T()) As T()
            Return Unary(Of T)(v, Function(a) Vector.Abs(Of T)(a))
        End Function

        ''' <summary>
        ''' 逐元素取负：<c>out(i) = -v(i)</c>
        ''' </summary>
        Public Shared Function Negate(Of T As Structure)(v As T()) As T()
            Return Unary(Of T)(v, Function(a) Vector.Negate(Of T)(a))
        End Function

        ''' <summary>
        ''' 逐元素平方：<c>out(i) = v(i) ^ 2</c>
        ''' </summary>
        Public Shared Function Square(Of T As Structure)(v As T()) As T()
            Return Unary(Of T)(v, Function(a) Vector.Multiply(Of T)(a, a))
        End Function

#End Region

#Region "float unary"

        ''' <summary>
        ''' 逐元素平方根（<see cref="Double"/>）。
        ''' </summary>
        Public Shared Function Sqrt(v As Double()) As Double()
            Return Unary(Of Double)(v, Function(a) Vector.SquareRoot(Of Double)(a))
        End Function

        ''' <summary>
        ''' 逐元素平方根（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Sqrt(v As Single()) As Single()
            Return Unary(Of Single)(v, Function(a) Vector.SquareRoot(Of Single)(a))
        End Function

        ''' <summary>
        ''' 逐元素倒数（<see cref="Double"/>）：<c>out(i) = 1 / v(i)</c>
        ''' </summary>
        Public Shared Function Reciprocal(v As Double()) As Double()
            Dim ones As New Vector(Of Double)(1.0)

            Return Unary(Of Double)(v, Function(a) Vector.Divide(Of Double)(ones, a))
        End Function

        ''' <summary>
        ''' 逐元素倒数（<see cref="Single"/>）：<c>out(i) = 1 / v(i)</c>
        ''' </summary>
        Public Shared Function Reciprocal(v As Single()) As Single()
            Dim ones As New Vector(Of Single)(1.0F)

            Return Unary(Of Single)(v, Function(a) Vector.Divide(Of Single)(ones, a))
        End Function

        ''' <summary>
        ''' 逐元素区间钳制：<c>out(i) = Min(Max(v(i), min), max)</c>
        ''' </summary>
        ''' <remarks>
        ''' 这个函数是手写展开的（而不是复用 <see cref="Unary(Of T)"/>），
        ''' 以避免为了捕获 <paramref name="min"/> / <paramref name="max"/> 而产生闭包对象的堆分配。
        ''' </remarks>
        Public Shared Function Clamp(Of T As Structure)(v As T(), min As T, max As T) As T()
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = SimdEngine.NewArray(Of T)(len)
            Dim count As Integer = Vector(Of T).Count
            Dim lo As New Vector(Of T)(min)
            Dim hi As New Vector(Of T)(max)
            Dim i As Integer = 0

            If SimdEngine.CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Vector.Min(Of T)(Vector.Max(Of T)(New Vector(Of T)(v, i), lo), hi).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Vector.Min(Of T)(Vector.Max(Of T)(New Vector(Of T)(v, last), lo), hi).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = Vector.Min(Of T)(Vector.Max(Of T)(New Vector(Of T)(v(i)), lo), hi).GetElement(0)
                i += 1
            Loop

            Return out
        End Function

#End Region

#Region "power"

        ''' <summary>
        ''' 逐元素幂运算：<c>out(i) = v1(i) ^ v2(i)</c>
        ''' </summary>
        ''' <remarks>
        ''' 幂运算没有逐元素硬件指令（VB 的 <c>^</c> 会编译成
        ''' <see cref="System.Math.Pow(Double, Double)"/>），因此这里保持标量实现。
        ''' 当指数是编译期可知的常量时，请优先使用 <see cref="PowScalar"/> —— 它能够把
        ''' 常见指数（2/3/4/0.5）特化为向量化的乘法与开方。
        ''' </remarks>
        Public Shared Function Pow(v1 As Double(), v2 As Double()) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)

            For i As Integer = 0 To len - 1
                out(i) = v1(i) ^ v2(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' 以固定指数求幂：<c>out(i) = v(i) ^ exponent</c>
        ''' </summary>
        ''' <remarks>
        ''' 常见指数会被特化为向量化的乘法/开方：
        ''' <list type="bullet">
        ''' <item><description><c>0</c> → 常量 1</description></item>
        ''' <item><description><c>1</c> → 直接拷贝</description></item>
        ''' <item><description><c>2</c> → <see cref="Square(Of T)"/></description></item>
        ''' <item><description><c>3</c> → <c>v * v * v</c></description></item>
        ''' <item><description><c>4</c> → 连续两次 <see cref="Square(Of T)"/></description></item>
        ''' <item><description><c>0.5</c> → <see cref="Sqrt(Double())"/></description></item>
        ''' </list>
        ''' 其余指数退回标量 <c>^</c>（与 VB 运算符语义完全一致）。
        ''' </remarks>
        Public Shared Function PowScalar(v As Double(), exponent As Double) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            If exponent = 2.0 Then Return Square(Of Double)(v)
            If exponent = 4.0 Then Return Square(Of Double)(Square(Of Double)(v))
            If exponent = 0.5 Then Return Sqrt(v)
            If exponent = 1.0 Then Return CType(v.Clone(), Double())
            If exponent = 0.0 Then
                Dim ones As Double() = SimdEngine.NewArray(Of Double)(len)

                For i As Integer = 0 To len - 1
                    ones(i) = 1.0
                Next

                Return ones
            End If
            If exponent = 3.0 Then
                Dim cube As Double() = SimdEngine.NewArray(Of Double)(len)
                Dim count As Integer = Vector(Of Double).Count
                Dim i As Integer = 0

                If SimdEngine.CanVectorize(Of Double)(len) Then
                    Dim last As Integer = len - count

                    Do While i <= last
                        Dim x As Vector(Of Double) = New Vector(Of Double)(v, i)

                        Vector.Multiply(Of Double)(Vector.Multiply(Of Double)(x, x), x).CopyTo(cube, i)
                        i += count
                    Loop
                    If i < len Then
                        Dim x As Vector(Of Double) = New Vector(Of Double)(v, last)

                        Vector.Multiply(Of Double)(Vector.Multiply(Of Double)(x, x), x).CopyTo(cube, last)
                    End If

                    Return cube
                End If

                For k As Integer = 0 To len - 1
                    cube(k) = v(k) * v(k) * v(k)
                Next

                Return cube
            End If

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)

            For i As Integer = 0 To len - 1
                out(i) = v(i) ^ exponent
            Next

            Return out
        End Function

#End Region

#Region "transcendental"

        ''' <summary>
        ''' 逐元素自然指数：<c>out(i) = Exp(v(i))</c>
        ''' </summary>
        ''' <remarks>
        ''' 保持与 <see cref="System.Math.Exp(Double)"/> 逐位一致的标量实现，
        ''' 原因见类型备注（无可用硬件指令）。
        ''' </remarks>
        Public Shared Function Exp(v As Double()) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)

            For i As Integer = 0 To len - 1
                out(i) = std.Exp(v(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 逐元素自然指数（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Exp(v As Single()) As Single()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = SimdEngine.NewArray(Of Single)(len)

            For i As Integer = 0 To len - 1
                out(i) = std.Exp(v(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 逐元素自然对数：<c>out(i) = Log(v(i))</c>
        ''' </summary>
        ''' <remarks>
        ''' 保持与 <see cref="System.Math.Log(Double)"/> 逐位一致的标量实现。
        ''' </remarks>
        Public Shared Function Log(v As Double()) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)

            For i As Integer = 0 To len - 1
                out(i) = std.Log(v(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 逐元素自然对数（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Log(v As Single()) As Single()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = SimdEngine.NewArray(Of Single)(len)

            For i As Integer = 0 To len - 1
                out(i) = std.Log(v(i))
            Next

            Return out
        End Function

#End Region
    End Class
End Namespace
