#Region "Microsoft.VisualBasic::f50cfe15cd3094849ed8077ac75227ca, Microsoft.VisualBasic.Core\src\Math\SIMD\Engine\SimdMath.vb"

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

    '   Total Lines: 461
    '    Code Lines: 241 (52.28%)
    ' Comment Lines: 129 (27.98%)
    '    - Xml Docs: 99.22%
    ' 
    '   Blank Lines: 91 (19.74%)
    '     File Size: 18.54 KB


    '     Class SimdMath
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Abs, (+2 Overloads) Ceiling, Clamp, (+2 Overloads) Exp, (+2 Overloads) Floor
    '                   (+3 Overloads) Log, Negate, Pow, PowScalar, (+2 Overloads) Reciprocal
    '                   (+2 Overloads) Sign, SignBlock, SignBlockSingle, (+2 Overloads) Sqrt, Square
    '                   (+2 Overloads) Truncate, Unary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
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
        ''' 逐元素向下取整（<see cref="Double"/>）：<c>out(i) = Floor(v(i))</c>
        ''' </summary>
        ''' <remarks>
        ''' <c>Vector.Floor</c> 会由 JIT 映射到 SSE4.1 的 <c>roundpd</c> / AVX 的
        ''' <c>vroundpd</c>，或者 ARM64 的 <c>frintm</c>，语义与
        ''' <see cref="System.Math.Floor(Double)"/> 完全一致。
        ''' </remarks>
        Public Shared Function Floor(v As Double()) As Double()
            Return Unary(Of Double)(v, Function(a) Vector.Floor(a))
        End Function

        ''' <summary>
        ''' 逐元素向下取整（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Floor(v As Single()) As Single()
            Return Unary(Of Single)(v, Function(a) Vector.Floor(a))
        End Function

        ''' <summary>
        ''' 逐元素向上取整（<see cref="Double"/>）：<c>out(i) = Ceiling(v(i))</c>
        ''' </summary>
        Public Shared Function Ceiling(v As Double()) As Double()
            Return Unary(Of Double)(v, Function(a) Vector.Ceiling(a))
        End Function

        ''' <summary>
        ''' 逐元素向上取整（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Ceiling(v As Single()) As Single()
            Return Unary(Of Single)(v, Function(a) Vector.Ceiling(a))
        End Function

        ''' <summary>
        ''' 逐元素截断取整（<see cref="Double"/>）：<c>out(i) = Truncate(v(i))</c>
        ''' </summary>
        Public Shared Function Truncate(v As Double()) As Double()
            Return Unary(Of Double)(v, Function(a) Vector.Truncate(a))
        End Function

        ''' <summary>
        ''' 逐元素截断取整（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Truncate(v As Single()) As Single()
            Return Unary(Of Single)(v, Function(a) Vector.Truncate(a))
        End Function

        ''' <summary>
        ''' 逐元素符号函数（<see cref="Double"/>）：
        ''' <c>out(i) = If(v(i) &gt; 0, 1, If(v(i) &lt; 0, -1, v(i)))</c>
        ''' </summary>
        ''' <remarks>
        ''' “既不大于零也不小于零”的通道直接输出原始值，因此 <c>+0.0</c> / <c>-0.0</c>
        ''' 与 <see cref="Double.NaN"/> 的返回值与 <see cref="System.Math.Sign(Double)"/>
        ''' 保持一致（<c>NaN</c> 输入得到 <c>NaN</c>，而不是被静默变成 <c>0</c>）。
        ''' </remarks>
        Public Shared Function Sign(v As Double()) As Double()
            Return Unary(Of Double)(v, AddressOf SignBlock)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function SignBlock(a As Vector(Of Double)) As Vector(Of Double)
            Dim zero As Vector(Of Double) = Vector(Of Double).Zero
            Dim positive As Vector(Of Double) = Vector.GreaterThan(Of Double)(a, zero)
            Dim negative As Vector(Of Double) = Vector.LessThan(Of Double)(a, zero)

            Return Vector.ConditionalSelect(Of Double)(
                positive,
                New Vector(Of Double)(1.0),
                Vector.ConditionalSelect(Of Double)(negative, New Vector(Of Double)(-1.0), a))
        End Function

        ''' <summary>
        ''' 逐元素符号函数（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function Sign(v As Single()) As Single()
            Return Unary(Of Single)(v, AddressOf SignBlockSingle)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function SignBlockSingle(a As Vector(Of Single)) As Vector(Of Single)
            Dim zero As Vector(Of Single) = Vector(Of Single).Zero
            Dim positive As Vector(Of Single) = Vector.GreaterThan(Of Single)(a, zero)
            Dim negative As Vector(Of Single) = Vector.LessThan(Of Single)(a, zero)

            Return Vector.ConditionalSelect(Of Single)(
                positive,
                New Vector(Of Single)(1.0F),
                Vector.ConditionalSelect(Of Single)(negative, New Vector(Of Single)(-1.0F), a))
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
        ''' <c>ln(2)</c>，用于指数/对数的区间约减。
        ''' </summary>
        Public Const Ln2 As Double = 0.6931471805599453R
        ''' <summary>
        ''' <c>1 / ln(2)</c>。
        ''' </summary>
        Public Const InvLn2 As Double = 1.4426950408889634R

        ''' <summary>
        ''' 指数多项式展开的最高阶乘项下标（<c>1 / 13!</c>）。
        ''' </summary>
        Private Const EXP_DEGREE As Integer = 13

        ''' <summary>
        ''' 用于把浮点数舍入成整数的 magic number：<c>1.5 × 2^52</c>。
        ''' </summary>
        Private Const MagicRound As Double = 6755399441055744.0R
        ''' <summary>
        ''' <see cref="MagicRound"/> 的整数位模式。
        ''' </summary>
        Private Const MagicRoundBits As Long = &H4338000000000000L
        ''' <summary>
        ''' <c>2^52</c>，用于把整数位模式还原为浮点数（指数拆分）。
        ''' </summary>
        Private Const TwoPow52 As Double = 4503599627370496.0R
        ''' <summary>
        ''' <c>2^52</c> 的指数域位模式（<c>0x433</c> 指数）。
        ''' </summary>
        Private Const TwoPow52Bits As Long = &H4330000000000000L

        ''' <summary>
        ''' 计算 <c>1 / n!</c>，用于展开 <c>exp(r)</c>。
        ''' </summary>
        Private Shared ReadOnly ExpCoefficients As Double() = makeExpCoefficients()

        Private Shared Function makeExpCoefficients() As Double()
            Dim c As Double() = New Double(EXP_DEGREE) {}
            Dim f As Double = 1

            For n As Integer = 0 To EXP_DEGREE
                If n > 0 Then
                    f *= n
                End If

                c(n) = 1.0R / f
            Next

            Return c
        End Function

        ''' <summary>
        ''' 判断当前数组是否可以走多项式路径（存在 NaN/Inf 的时候回退到标量实现，保证语义一致）。
        ''' </summary>
        Private Shared Function allFinite(v As Double(), count As Integer) As Boolean
            For i As Integer = 0 To count - 1
                If Double.IsNaN(v(i)) OrElse Double.IsInfinity(v(i)) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 逐元素自然指数（原地计算，真正的 SIMD 实现）。
        ''' </summary>
        ''' <param name="v">输入输出数组</param>
        ''' <returns>返回 <paramref name="v"/> 自身，方便链式调用</returns>
        ''' <remarks>
        ''' <para>
        ''' 实现方式与 Cephes/SLEEF 一致：区间约减 <c>x = k·ln2 + r</c>（<c>|r| ≤ ln2/2</c>），
        ''' 用 13 阶泰勒多项式计算 <c>exp(r)</c>，再左移 <c>k</c> 位构造 <c>2^k</c> 缩放回去。
        ''' 截断误差约 <c>1e-18</c>，远小于 <see cref="Double"/> 的有效精度。
        ''' </para>
        ''' <para>
        ''' 输入被夹在 <c>[-700, 700]</c> 之内（与 <c>S-system</c> 的溢出保护一致）；
        ''' 当数组中含有 NaN/Inf，或者 <see cref="SIMDEnvironment.IsEnabled"/> 为假时，
        ''' 自动回退到逐元素的 <see cref="System.Math.Exp(Double)"/>，保证数值语义不变。
        ''' </para>
        ''' </remarks>
        Public Shared Function ExpInPlace(v As Double()) As Double()
            Return ExpInPlace(v, v.Length)
        End Function

        ''' <summary>
        ''' 逐元素自然指数（原地计算，只处理前 <paramref name="count"/> 个元素）。
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Public Shared Function ExpInPlace(v As Double(), count As Integer) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
            If count <= 0 Then Return v
            If count > v.Length Then count = v.Length

            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled AndAlso Avx2.IsSupported AndAlso Fma.IsSupported AndAlso allFinite(v, count) Then
                Dim width As Integer = Vector256(Of Double).Count
                Dim last As Integer = count - width

                Do While i <= last
                    Dim x As Vector256(Of Double) = Vector256.LoadUnsafe(Of Double)(v(i))

                    Vector256.StoreUnsafe(exp256(x), v(i))
                    i += width
                Loop
            End If

            For j As Integer = i To count - 1
                v(j) = std.Exp(v(j))
            Next

            Return v
        End Function

        ''' <summary>
        ''' 逐元素自然对数（原地计算，真正的 SIMD 实现）。
        ''' </summary>
        ''' <param name="v">输入输出数组，要求所有元素为正数</param>
        ''' <returns>返回 <paramref name="v"/> 自身</returns>
        ''' <remarks>
        ''' 分解 <c>x = m·2^e</c>（<c>m ∈ [sqrt(0.5), sqrt(2))</c>），
        ''' 再对 <c>ln(m) = 2·atanh((m-1)/(m+1))</c> 做奇次多项式展开；
        ''' 输入中存在非正数或者非有限值时自动回退到标量实现。
        ''' </remarks>
        Public Shared Function LogInPlace(v As Double()) As Double()
            Return LogInPlace(v, v.Length)
        End Function

        ''' <summary>
        ''' 逐元素自然对数（原地计算，只处理前 <paramref name="count"/> 个元素）。
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Public Shared Function LogInPlace(v As Double(), count As Integer) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
            If count <= 0 Then Return v
            If count > v.Length Then count = v.Length

            Dim i As Integer = 0
            Dim positive As Boolean = True

            For k As Integer = 0 To count - 1
                If v(k) <= 0 OrElse Double.IsNaN(v(k)) OrElse Double.IsInfinity(v(k)) Then
                    positive = False
                    Exit For
                End If
            Next

            If SIMDEnvironment.IsEnabled AndAlso Avx2.IsSupported AndAlso Fma.IsSupported AndAlso positive Then
                Dim width As Integer = Vector256(Of Double).Count
                Dim last As Integer = count - width

                Do While i <= last
                    Dim x As Vector256(Of Double) = Vector256.LoadUnsafe(Of Double)(v(i))

                    Vector256.StoreUnsafe(log256(x), v(i))
                    i += width
                Loop
            End If

            For j As Integer = i To count - 1
                v(j) = std.Log(v(j))
            Next

            Return v
        End Function

        ''' <summary>
        ''' 4 通道向量的 <c>exp</c>：区间约减 + FMA 霍纳多项式 + 位运算构造 <c>2^k</c>。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这里刻意使用 <c>Avx/Avx2/Fma</c> 而不是泛型 <c>Vector(Of T)</c>：
        ''' 指数缩放需要 <c>double → int64</c> 的位运算，泛型 <see cref="Vector(Of T)"/>
        ''' 对 64 位整数的移位/位运算在 .NET 上会退化为软件展开，
        ''' 实测反而比标量 <see cref="System.Math.Exp(Double)"/> 慢 5 倍以上。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function exp256(x As Vector256(Of Double)) As Vector256(Of Double)
            ' 溢出保护（与 S-system 的 clampExp 一致）
            x = Avx.Max(x, Vector256.Create(-700.0R))
            x = Avx.Min(x, Vector256.Create(700.0R))

            ' k = round(x / ln2)，用 magic number 舍入（不产生 double→int 的转换指令）
            Dim t As Vector256(Of Double) = Fma.MultiplyAdd(x, Vector256.Create(InvLn2), Vector256.Create(MagicRound))
            Dim kd As Vector256(Of Double) = Avx.Subtract(t, Vector256.Create(MagicRound))
            Dim ki As Vector256(Of Int64) = Avx2.Subtract(Vector256.AsInt64(t), Vector256.Create(MagicRoundBits))
            Dim r As Vector256(Of Double) = Fma.MultiplyAddNegated(kd, Vector256.Create(Ln2), x)

            ' exp(r) = 1 + r + r^2/2! + ... + r^13/13!，|r| ≤ ln2/2
            Dim p As Vector256(Of Double) = Vector256.Create(1.6059043836821613E-10R)
            p = Fma.MultiplyAdd(p, r, Vector256.Create(2.08767569878681E-9R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(2.505210838544172E-8R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(2.755731922398589E-7R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(2.7557319223985893E-6R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(2.48015873015873E-5R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(1.984126984126984E-4R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(1.388888888888889E-3R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(8.333333333333333E-3R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(4.1666666666666664E-2R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(1.6666666666666666E-1R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(0.5R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(1.0R))
            p = Fma.MultiplyAdd(p, r, Vector256.Create(1.0R))

            ' 2^k = bitcast((k + 1023) << 52)
            Dim scale As Vector256(Of Double) = Vector256.AsDouble(
                Avx2.ShiftLeftLogical(Avx2.Add(ki, Vector256.Create(1023L)), 52))

            Return Avx.Multiply(p, scale)
        End Function

        ''' <summary>
        ''' 4 通道向量的 <c>log</c>：尾数/指数拆分 + <c>atanh</c> 奇次多项式。
        ''' </summary>
        ''' <param name="x">必须为正数</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function log256(x As Vector256(Of Double)) As Vector256(Of Double)
            Dim one As Vector256(Of Double) = Vector256.Create(1.0R)
            Dim bits As Vector256(Of Int64) = Vector256.AsInt64(x)
            Dim eBits As Vector256(Of Int64) = Avx2.ShiftRightLogical(bits, 52)
            Dim mant As Vector256(Of Int64) = Avx2.Or(
                Avx2.And(bits, Vector256.Create(&HFFFFFFFFFFFFFL)),
                Vector256.Create(1023L << 52))
            Dim m As Vector256(Of Double) = Vector256.AsDouble(mant)

            ' 把尾数规整到 [sqrt(0.5), sqrt(2))，让 atanh 级数收敛最快
            Dim hi As Vector256(Of Double) = Avx.Compare(m, Vector256.Create(1.4142135623730951R),
                                                         FloatComparisonMode.OrderedGreaterThanNonSignaling)
            Dim adj As Vector256(Of Double) = Avx.BlendVariable(Vector256(Of Double).Zero, one, hi)

            m = Avx.BlendVariable(m, Avx.Multiply(m, Vector256.Create(0.5R)), hi)

            ' 指数（0..2047）→ double：塞进 2^52 的尾数再减回去
            Dim eDbl As Vector256(Of Double) = Avx.Subtract(
                Vector256.AsDouble(Avx2.Or(eBits, Vector256.Create(TwoPow52Bits))),
                Vector256.Create(TwoPow52))
            Dim e As Vector256(Of Double) = Avx.Add(Avx.Subtract(eDbl, Vector256.Create(1023.0R)), adj)

            Dim s As Vector256(Of Double) = Avx.Divide(Avx.Subtract(m, one), Avx.Add(m, one))
            Dim s2 As Vector256(Of Double) = Avx.Multiply(s, s)
            Dim p As Vector256(Of Double) = Vector256.Create(1.0R / 13.0R)

            p = Fma.MultiplyAdd(p, s2, Vector256.Create(1.0R / 11.0R))
            p = Fma.MultiplyAdd(p, s2, Vector256.Create(1.0R / 9.0R))
            p = Fma.MultiplyAdd(p, s2, Vector256.Create(1.0R / 7.0R))
            p = Fma.MultiplyAdd(p, s2, Vector256.Create(1.0R / 5.0R))
            p = Fma.MultiplyAdd(p, s2, Vector256.Create(1.0R / 3.0R))
            p = Fma.MultiplyAdd(p, s2, one)

            Return Avx.Add(Avx.Multiply(e, Vector256.Create(Ln2)),
                           Avx.Multiply(Vector256.Create(2.0R), Avx.Multiply(s, p)))
        End Function

        ''' <summary>
        ''' 逐元素夹取（原地计算）。
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        Public Shared Function ClampInPlace(v As Double(), min As Double, max As Double) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))
            If v.Length = 0 Then Return v

            Dim i As Integer = 0

            If SIMDEnvironment.IsEnabled Then
                Dim width As Integer = Vector(Of Double).Count
                Dim last As Integer = v.Length - width
                Dim vmax As New Vector(Of Double)(max)
                Dim vmin As New Vector(Of Double)(min)

                Do While i <= last
                    Dim x As Vector(Of Double) = New Vector(Of Double)(v, i)

                    Vector.Min(Vector.Max(x, vmin), vmax).CopyTo(v, i)
                    i += width
                Loop
            End If

            For j As Integer = i To v.Length - 1
                v(j) = std.Max(std.Min(v(j), max), min)
            Next

            Return v
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

        ''' <summary>
        ''' 逐元素以 <paramref name="base"/> 为底的对数：<c>out(i) = Log(v(i), base)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 等价于 <c>Log(v(i)) / Log(base)</c>，与
        ''' <see cref="System.Math.Log(Double, Double)"/> 的实现方式一致。
        ''' 其中除法部分可以向量化，对数的预算仍然是标量（无硬件指令）。
        ''' </remarks>
        Public Shared Function Log(v As Double(), base As Double) As Double()
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            ' 底数为 E 时可直接跳过这次除法（与 Math.Log(x, E) 的结果一致）
            If base = std.E Then Return Log(v)

            Return SimdEngine.DivideScalar(Log(v), std.Log(base))
        End Function

#End Region
    End Class
End Namespace
