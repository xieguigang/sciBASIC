#Region "Microsoft.VisualBasic::HalfSimd, Data_science\Mathematica\Math\Math\Numerics\HalfSimd.vb"

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

    '     Module HalfSimd
    ' 
    '         Function: Add, AddScalar, Divide, DivideScalar, Dot, Max, Min, Multiply,
    '                   MultiplyAdd, MultiplyScalar, Norm, Subtract, SubtractScalar,
    '                   Sum, SumOfSquares, ToHalf, ToSingle
    ' 
    '         Sub: ConvertToHalf, ConvertToSingle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Runtime.Intrinsics
Imports Microsoft.VisualBasic.Math.SIMD

Namespace Numerics

    ''' <summary>
    ''' 半精度浮点数（<see cref="System.Half"/>）的 SIMD 计算内核（8 路）
    ''' </summary>
    ''' <remarks>
    ''' ## 为什么需要自己写
    ''' 
    ''' net10.0 的 BCL 并没有暴露半精度的 SIMD 转换指令：
    ''' 
    ''' + ``System.Runtime.Intrinsics.X86`` 命名空间中不存在 ``F16C`` 类（也不存在 ``Avx512FP16``）；
    ''' + ``Vector128.ConvertToSingle`` 只有 ``Vector128(Of Integer)`` / ``Vector128(Of UInteger)`` 两个重载；
    ''' + 既有的 SimdEngine 基于 ``System.Numerics.Vector(Of T)``，而 ``Vector(Of Half)`` 是不受支持的类型。
    ''' 
    ''' 但是 <see cref="System.Half"/> 两个方向的转换在 BCL 中都是**无分支**实现
    ''' （只由 AND/OR/ADD/SUB/SHL 与一次浮点加/减构成），因此可以把每一条 32 位运算
    ''' 直接替换为 <see cref="Vector128"/> 的 lane 运算，从而得到 8 路转换内核。
    ''' 
    ''' ## 语义保证（bit-exact）
    ''' 
    ''' 本模块实现的转换与 BCL 的 ``CSng(h)`` / ``CType(f, Half)`` **逐位一致**：
    ''' 包含 round-to-nearest-even 舍入、次正规数、正负零、NaN（含 payload）以及溢出到无穷的全部语义。
    ''' 又因为 BCL 的 ``Half.op_Addition`` 本身就是 ``CType(CSng(a) + CSng(b), Half)``，
    ''' 所以本模块的融合算子与标量的 ``a + b`` 同样逐位一致，可以直接做差分回归测试。
    ''' 
    ''' ## 回退路径
    ''' 
    ''' 当 ``SIMDEnvironment.IsEnabled`` 为 False（例如基准测试里把
    ''' ``SIMDEnvironment.config`` 设为 ``SIMDConfiguration.disable``）或者向量长度不足 8 时，
    ''' 全部走 <see cref="System.Half"/> 的标量路径，结果与向量路径一致。
    ''' </remarks>
    Public Module HalfSimd

        ''' <summary>
        ''' 一个 128 位向量一次可以处理的 <see cref="System.Half"/> 元素个数
        ''' </summary>
        Public Const Lanes As Integer = 8

#Region "constants"

        ' 以下常数与 dotnet/runtime 的 System\Half.cs 中 explicit operator Single(Half) /
        ' explicit operator Half(float) 使用的常数完全一致，改动任何一个都会破坏 bit-exact 性。

        ''' <summary>Single 的符号位掩码（写成 Integer.MinValue 是因为 &amp;H80000000 在 VB 里是 Long）</summary>
        Private Const SignMask32 As Integer = Integer.MinValue
        ''' <summary>Single 的绝对值掩码</summary>
        Private Const AbsMask32 As Integer = &H7FFFFFFF
        ''' <summary>Single 的指数位掩码</summary>
        Private Const SingleExpMask32 As Integer = &H7F800000
        ''' <summary>(Half)Single 转换里的指数位移量 #2（指数 +13）</summary>
        Private Const Exponent13 As Integer = &H6800000
        ''' <summary>(Half)Single 转换里的指数位移量 #1（指数 -126）</summary>
        Private Const Exponent126 As Integer = &H3F000000
        ''' <summary>(Single)Half 转换里的指数偏置：SingleToUInt32Bits(1.0F) - (HalfToUInt16Bits(1.0H) &lt;&lt; 13)</summary>
        Private Const ExponentOffset32 As Integer = &H38000000
        ''' <summary>Half 最小正规数 2^-14 的 Single 位型，同时用于次正规修正</summary>
        Private Const ExponentLowerBound32 As Integer = &H38800000
        ''' <summary>(Single)Half 转换里对齐指数/尾数边界用的掩码</summary>
        Private Const HalfToSingleBitsMask32 As Integer = &HFFFE000
        ''' <summary>Half 的指数位掩码</summary>
        Private Const HalfExponentMask32 As Integer = &H7C00
        ''' <summary>会向下舍入到 Half.MaxValue 的最大无穷精细值</summary>
        Private Const MaxHalfValueBelowInfinity As Single = 65520.0F

        ' 注意：这里刻意**不**预先构造 Vector128 常量字段。VB Module 的字段是 Shared ReadOnly，
        ' JIT 不会把它们折叠成立即数，每次读取都是一次内存加载；内核里统一使用内联的
        ' Vector128.Create(常量) 由 JIT 广播成一条 movdqa（实测两者相差约 7 倍）。

#End Region

#Region "helpers"

        ''' <summary>
        ''' 当前数据长度是否值得走 8 路向量路径
        ''' </summary>
        ''' <param name="length">向量长度</param>
        ''' <returns>True 表示可以向量化</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CanVectorize(length As Integer) As Boolean
            Return SIMDEnvironment.IsEnabled AndAlso length >= Lanes
        End Function

        ''' <summary>
        ''' 分配结果数组，跳过运行时的零初始化（后续循环会完整覆盖每一个下标）
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="length"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function NewArray(Of T)(length As Integer) As T()
            Return GC.AllocateUninitializedArray(Of T)(length)
        End Function

        ''' <summary>
        ''' 读取 8 个 <see cref="System.Half"/> 并拆分到两个 4 路 Integer lane
        ''' </summary>
        ''' <remarks>
        ''' 关键点：``Vector128(Of Half)`` 在运行时**不受支持**（读它的 Count 会抛
        ''' NotSupportedException，因为 Half 不是向量基元类型），所以不能直接
        ''' ``Vector128.Create(Of Half)(...)``。这里用 <see cref="MemoryMarshal"/> 把
        ''' ``Half()`` 按位重解释成 ``Short()``（两者都是 2 字节且不含引用），再走 Short 通道。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub LoadLanes(source As Half(), start As Integer,
                              ByRef lower As Vector128(Of Integer),
                              ByRef upper As Vector128(Of Integer))
            ' 注意：
            ' 1) VB 不支持声明 Span/ReadOnlySpan 局部变量（ref struct），所以必须内联写；
            ' 2) 必须显式给出 (Of Short)，否则 VB 会把 Span 本身当成广播标量去匹配 Create(Of T)(T)
            Dim bits As Vector128(Of Short) = Vector128.Create(Of Short)(
                MemoryMarshal.Cast(Of Half, Short)(source.AsSpan(start, Lanes)))

            lower = Vector128.WidenLower(bits)
            upper = Vector128.WidenUpper(bits)
        End Sub

        ''' <summary>
        ''' 把两个 4 路 Integer lane（低 16 位有效）合并写回 8 个 <see cref="System.Half"/>
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub StoreLanes(lower As Vector128(Of Integer), upper As Vector128(Of Integer),
                               target As Half(), start As Integer)
            Dim packed As Vector128(Of Short) = Vector128.Narrow(lower, upper)

            packed.CopyTo(MemoryMarshal.Cast(Of Half, Short)(target.AsSpan(start, Lanes)))
        End Sub

#End Region

#Region "lane kernels"

        ''' <summary>
        ''' 4 路 lane：Half 的位型（符号扩展后的 32 位）转换为 Single
        ''' </summary>
        ''' <param name="x">4 个符号扩展后的 Half 位型</param>
        ''' <returns>4 个 Single</returns>
        ''' <remarks>
        ''' 逐 lane 复刻 BCL <c>explicit operator Single(Half)</c> 的无分支算法：
        ''' 符号位隔离 → 次正规掩码 → 左移 13 位对齐指数/尾数边界 → 加偏置
        ''' （Inf/NaN 车道偏置再翻一倍）→ 一次浮点减法完成次正规修正 → 拼回符号位。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function LanesToSingle(x As Vector128(Of Integer)) As Vector128(Of Single)
            ' 性能要点：这里的掩码常量必须写成内联的 Vector128.Create(常量)，
            ' 不能读 Module 级的 Shared ReadOnly 向量字段——后者每次都是一次内存加载，
            ' 实测会让整个内核慢 7 倍（0.11 ms -> 0.77 ms / 60k 元素）。
            Dim sign As Vector128(Of Integer) = Vector128.BitwiseAnd(x, Vector128.Create(SignMask32))
            Dim expBits As Vector128(Of Integer) = Vector128.BitwiseAnd(x, Vector128.Create(HalfExponentMask32))
            ' 指数为 0 表示次正规（含零）：此时需要 2^-14 的修正量
            Dim subnormal As Vector128(Of Integer) = Vector128.BitwiseAnd(
                Vector128.Equals(expBits, Vector128.Create(0)), Vector128.Create(ExponentLowerBound32))
            ' 指数全 1 表示 Infinity / NaN
            Dim infNan As Vector128(Of Integer) = Vector128.Equals(expBits, Vector128.Create(HalfExponentMask32))

            Dim offset As Vector128(Of Integer) = Vector128.BitwiseOr(Vector128.Create(ExponentOffset32), subnormal)
            ' offset &lt;&lt;= (Inf/NaN ? 1 : 0)，等价于 offset + (offset And mask)
            offset = Vector128.Add(offset, Vector128.BitwiseAnd(offset, infNan))

            Dim bits As Vector128(Of Integer) = Vector128.BitwiseAnd(
                Vector128.ShiftLeft(x, 13), Vector128.Create(HalfToSingleBitsMask32))
            bits = Vector128.Add(bits, offset)

            Dim corrected As Vector128(Of Single) = Vector128.Subtract(
                bits.AsSingle(), subnormal.AsSingle())

            Return Vector128.BitwiseOr(corrected.AsInt32(), sign).AsSingle()
        End Function

        ''' <summary>
        ''' 4 路 lane：Single 转换为 Half 的位型（低 16 位有效）
        ''' </summary>
        ''' <param name="v">4 个 Single</param>
        ''' <returns>4 个 Half 位型（低 16 位有效，高位是会被 Narrow 丢掉的垃圾位）</returns>
        ''' <remarks>
        ''' 逐 lane 复刻 BCL <c>explicit operator Half(float)</c> 的无分支算法：
        ''' 符号位隔离 → NaN 掩码（<c>v = v</c> 在非 NaN 时得到全 1）→ 绝对值 →
        ''' 与 65520 取小 → 与 2^-14 取大 → 构造偏置指数并**用一次浮点加法触发 RNE 舍入** →
        ''' 整数减法/移位/掩码 → 拼回符号与 NaN 指数。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function LanesToHalf(v As Vector128(Of Single)) As Vector128(Of Integer)
            Dim raw As Vector128(Of Integer) = v.AsInt32()
            Dim sign As Vector128(Of Integer) = Vector128.ShiftRightLogical(
                Vector128.BitwiseAnd(raw, Vector128.Create(SignMask32)), 16)
            ' 非 NaN 车道为全 1；NaN 车道为 0
            Dim notNaN As Vector128(Of Integer) = Vector128.Equals(v, v).AsInt32()

            ' 钳到 65520，避免溢出成 Infinity
            Dim abs As Vector128(Of Single) = Vector128.BitwiseAnd(raw, Vector128.Create(AbsMask32)).AsSingle()
            abs = Vector128.Min(abs, Vector128.Create(MaxHalfValueBelowInfinity))

            ' 关键：指数偏置只由 Max(|x|, 2^-14) 推导，**参与加法的仍然是钳完上界的原值**。
            ' 如果把 Max 的结果赋回 abs，那么 0 / 次正规数会被抬到 2^-14，结果整体偏移一个 exponent（0x0400）。
            ' 2^-14 直接由位型 &amp;H38800000 重解释而来，避免读静态字段
            Dim clampedForExp As Vector128(Of Single) = Vector128.Max(abs, Vector128.Create(ExponentLowerBound32).AsSingle())

            Dim expOffset As Vector128(Of Integer) = Vector128.Add(
                Vector128.BitwiseAnd(clampedForExp.AsInt32(), Vector128.Create(SingleExpMask32)), Vector128.Create(Exponent13))
            Dim rounded As Vector128(Of Integer) = Vector128.Add(abs, expOffset.AsSingle()).AsInt32()

            Dim bits As Vector128(Of Integer) = Vector128.Subtract(rounded, Vector128.Create(Exponent126))
            ' NaN 车道需要补上全 1 的指数位
            Dim nanExp As Vector128(Of Integer) = Vector128.AndNot(Vector128.Create(HalfExponentMask32), notNaN)
            Dim newExp As Vector128(Of Integer) = Vector128.ShiftRightLogical(bits, 13)

            bits = Vector128.BitwiseAnd(bits, notNaN)
            bits = Vector128.Add(bits, newExp)
            bits = Vector128.AndNot(bits, nanExp)

            Return Vector128.BitwiseOr(bits, Vector128.BitwiseOr(nanExp, sign))
        End Function

#End Region

#Region "batch convert"

        ''' <summary>
        ''' 批量把 <see cref="System.Half"/> 数组转换为 Single 数组（8 路内核 + 标量尾部）
        ''' </summary>
        ''' <param name="source">源数组</param>
        ''' <param name="target">目标数组，长度必须不小于 <paramref name="source"/></param>
        Public Sub ConvertToSingle(source As Half(), target As Single())
            Dim n As Integer = source.Length
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lower, upper As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(source, i, lower, upper)
                    Vector128.CopyTo(Of Single)(LanesToSingle(lower), target, i)
                    Vector128.CopyTo(Of Single)(LanesToSingle(upper), target, i + 4)
                    i += Lanes
                Loop
            End If

            Do While i < n
                target(i) = CSng(source(i))
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' 把 <see cref="System.Half"/> 数组转换为 Single 数组
        ''' </summary>
        ''' <param name="source">源数组</param>
        ''' <returns>与 <paramref name="source"/> 等长的新数组</returns>
        Public Function ToSingle(source As Half()) As Single()
            Dim target As Single() = NewArray(Of Single)(source.Length)

            ConvertToSingle(source, target)

            Return target
        End Function

        ''' <summary>
        ''' 批量把 Single 数组转换为 <see cref="System.Half"/> 数组（8 路内核 + 标量尾部）
        ''' </summary>
        ''' <param name="source">源数组</param>
        ''' <param name="target">目标数组，长度必须不小于 <paramref name="source"/></param>
        Public Sub ConvertToHalf(source As Single(), target As Half())
            Dim n As Integer = source.Length
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lower, upper As Vector128(Of Integer)

                Do While i <= last
                    lower = LanesToHalf(Vector128.Create(source, i))
                    upper = LanesToHalf(Vector128.Create(source, i + 4))
                    StoreLanes(lower, upper, target, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                target(i) = CType(source(i), Half)
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' 把 Single 数组转换为 <see cref="System.Half"/> 数组
        ''' </summary>
        ''' <param name="source">源数组</param>
        ''' <returns>与 <paramref name="source"/> 等长的新数组</returns>
        Public Function ToHalf(source As Single()) As Half()
            Dim target As Half() = NewArray(Of Half)(source.Length)

            ConvertToHalf(source, target)

            Return target
        End Function

        ''' <summary>
        ''' 把 Double 数组转换为 <see cref="System.Half"/> 数组
        ''' </summary>
        ''' <param name="source">源数组</param>
        ''' <returns>与 <paramref name="source"/> 等长的新数组</returns>
        ''' <remarks>
        ''' 这里刻意保持标量实现：先塌到 Single 再转 Half 会产生二次舍入，
        ''' 因此必须走 <see cref="System.Half"/> 的 Double 直转路径。
        ''' </remarks>
        Public Function ToHalf(source As Double()) As Half()
            Dim target As Half() = NewArray(Of Half)(source.Length)

            For i As Integer = 0 To source.Length - 1
                target(i) = CType(source(i), Half)
            Next

            Return target
        End Function

#End Region

#Region "elementwise (half in, half out)"

        ''' <summary>
        ''' 逐元素相加：<c>out(i) = CType(CSng(a(i)) + CSng(b(i)), Half)</c>
        ''' </summary>
        ''' <param name="a">第一个向量</param>
        ''' <param name="b">第二个向量</param>
        ''' <returns>与输入等长的新数组</returns>
        Public Function Add(a As Half(), b As Half()) As Half()
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    StoreLanes(
                        LanesToHalf(Vector128.Add(LanesToSingle(alo), LanesToSingle(blo))),
                        LanesToHalf(Vector128.Add(LanesToSingle(ahi), LanesToSingle(bhi))),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) + CSng(b(i)), Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相减：<c>out(i) = CType(CSng(a(i)) - CSng(b(i)), Half)</c>
        ''' </summary>
        ''' <param name="a">被减数</param>
        ''' <param name="b">减数</param>
        ''' <returns>与输入等长的新数组</returns>
        Public Function Subtract(a As Half(), b As Half()) As Half()
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    StoreLanes(
                        LanesToHalf(Vector128.Subtract(LanesToSingle(alo), LanesToSingle(blo))),
                        LanesToHalf(Vector128.Subtract(LanesToSingle(ahi), LanesToSingle(bhi))),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) - CSng(b(i)), Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相乘：<c>out(i) = CType(CSng(a(i)) * CSng(b(i)), Half)</c>
        ''' </summary>
        ''' <param name="a">被乘数</param>
        ''' <param name="b">乘数</param>
        ''' <returns>与输入等长的新数组</returns>
        Public Function Multiply(a As Half(), b As Half()) As Half()
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    StoreLanes(
                        LanesToHalf(Vector128.Multiply(LanesToSingle(alo), LanesToSingle(blo))),
                        LanesToHalf(Vector128.Multiply(LanesToSingle(ahi), LanesToSingle(bhi))),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) * CSng(b(i)), Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素相除：<c>out(i) = CType(CSng(a(i)) / CSng(b(i)), Half)</c>
        ''' </summary>
        ''' <param name="a">被除数</param>
        ''' <param name="b">除数</param>
        ''' <returns>与输入等长的新数组</returns>
        Public Function Divide(a As Half(), b As Half()) As Half()
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    StoreLanes(
                        LanesToHalf(Vector128.Divide(LanesToSingle(alo), LanesToSingle(blo))),
                        LanesToHalf(Vector128.Divide(LanesToSingle(ahi), LanesToSingle(bhi))),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) / CSng(b(i)), Half)
                i += 1
            Loop

            Return out
        End Function

#End Region

#Region "elementwise (half in, scalar operand)"

        ''' <summary>
        ''' 逐元素加标量：<c>out(i) = CType(CSng(a(i)) + scalar, Half)</c>
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <param name="scalar">加数</param>
        ''' <returns>与 <paramref name="a"/> 等长的新数组</returns>
        Public Function AddScalar(a As Half(), scalar As Single) As Half()
            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0
            Dim broadcast As Vector128(Of Single) = Vector128.Create(scalar)

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    StoreLanes(
                        LanesToHalf(Vector128.Add(LanesToSingle(lo), broadcast)),
                        LanesToHalf(Vector128.Add(LanesToSingle(hi), broadcast)),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) + scalar, Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素减标量：<c>out(i) = CType(CSng(a(i)) - scalar, Half)</c>
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <param name="scalar">减数</param>
        ''' <returns>与 <paramref name="a"/> 等长的新数组</returns>
        Public Function SubtractScalar(a As Half(), scalar As Single) As Half()
            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0
            Dim broadcast As Vector128(Of Single) = Vector128.Create(scalar)

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    StoreLanes(
                        LanesToHalf(Vector128.Subtract(LanesToSingle(lo), broadcast)),
                        LanesToHalf(Vector128.Subtract(LanesToSingle(hi), broadcast)),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) - scalar, Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素乘标量：<c>out(i) = CType(CSng(a(i)) * scalar, Half)</c>
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <param name="scalar">乘数</param>
        ''' <returns>与 <paramref name="a"/> 等长的新数组</returns>
        Public Function MultiplyScalar(a As Half(), scalar As Single) As Half()
            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0
            Dim broadcast As Vector128(Of Single) = Vector128.Create(scalar)

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    StoreLanes(
                        LanesToHalf(Vector128.Multiply(LanesToSingle(lo), broadcast)),
                        LanesToHalf(Vector128.Multiply(LanesToSingle(hi), broadcast)),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) * scalar, Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 逐元素除标量：<c>out(i) = CType(CSng(a(i)) / scalar, Half)</c>
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <param name="scalar">除数</param>
        ''' <returns>与 <paramref name="a"/> 等长的新数组</returns>
        Public Function DivideScalar(a As Half(), scalar As Single) As Half()
            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0
            Dim broadcast As Vector128(Of Single) = Vector128.Create(scalar)

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    StoreLanes(
                        LanesToHalf(Vector128.Divide(LanesToSingle(lo), broadcast)),
                        LanesToHalf(Vector128.Divide(LanesToSingle(hi), broadcast)),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) / scalar, Half)
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 融合乘加：<c>out(i) = CType(CSng(a(i)) * CSng(b(i)) + addend, Half)</c>
        ''' </summary>
        ''' <param name="a">第一个向量</param>
        ''' <param name="b">第二个向量</param>
        ''' <param name="addend">加数</param>
        ''' <returns>与输入等长的新数组</returns>
        ''' <remarks>
        ''' 与"先乘再加"的区别在于中间结果不落回 Half，因此全程只舍入一次；
        ''' 逐位等价于 <c>CType(CSng(a) * CSng(b) + addend, Half)</c>，
        ''' 而不是 <c>CType(CSng(CType(CSng(a) * CSng(b), Half)) + addend, Half)</c>。
        ''' </remarks>
        Public Function MultiplyAdd(a As Half(), b As Half(), addend As Single) As Half()
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim out As Half() = NewArray(Of Half)(n)
            Dim i As Integer = 0
            Dim addendV As Vector128(Of Single) = Vector128.Create(addend)

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    StoreLanes(
                        LanesToHalf(Vector128.Add(Vector128.Multiply(LanesToSingle(alo), LanesToSingle(blo)), addendV)),
                        LanesToHalf(Vector128.Add(Vector128.Multiply(LanesToSingle(ahi), LanesToSingle(bhi)), addendV)),
                        out, i)
                    i += Lanes
                Loop
            End If

            Do While i < n
                out(i) = CType(CSng(a(i)) * CSng(b(i)) + addend, Half)
                i += 1
            Loop

            Return out
        End Function

#End Region

#Region "reduce"

        ''' <summary>
        ''' 求和。累加在 Double 中进行，与既有 <c>HalfVector.Sum</c> 的语义保持一致。
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <returns>Double 精度的累加和</returns>
        ''' <remarks>
        ''' 向量路径先按 4 路 Single lane 求和，再把块和加到 Double 累加器上，
        ''' 因此与逐元素 Double 累加相比**归约顺序发生了改变**，
        ''' 最后几个二进制位可能不同，但累加仍然发生在 Double 精度上，误差量级相同。
        ''' </remarks>
        Public Function Sum(a As Half()) As Double
            Dim n As Integer = a.Length
            Dim total As Double = 0R
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)
                Dim acc0 As Vector128(Of Double) = Vector128(Of Double).Zero
                Dim acc1 As Vector128(Of Double) = Vector128(Of Double).Zero

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    Accumulate(LanesToSingle(lo), acc0, acc1)
                    Accumulate(LanesToSingle(hi), acc0, acc1)
                    i += Lanes
                Loop

                total += ReduceDouble(acc0) + ReduceDouble(acc1)
            End If

            Do While i < n
                total += CSng(a(i))
                i += 1
            Loop

            Return total
        End Function

        ''' <summary>
        ''' 把 4 路 Single lane 先加宽到 Double 再累加到 2 个 Double 累加器上
        ''' </summary>
        ''' <remarks>
        ''' 这里**不能**直接用 <c>Vector128.Sum(Of Single)</c>：那样是在 Single 精度内求和，
        ''' 对 Dot（乘积可达 1e9 量级）会引入 0.1 量级的误差。先加宽再累加才能保持 Double 精度。
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Accumulate(block As Vector128(Of Single),
                               ByRef acc0 As Vector128(Of Double),
                               ByRef acc1 As Vector128(Of Double))
            acc0 = Vector128.Add(acc0, Vector128.WidenLower(block))
            acc1 = Vector128.Add(acc1, Vector128.WidenUpper(block))
        End Sub

        ''' <summary>
        ''' 把一个 2 路 Double 向量归约为标量
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function ReduceDouble(v As Vector128(Of Double)) As Double
            Return Vector128.GetElement(v, 0) + Vector128.GetElement(v, 1)
        End Function

        ''' <summary>
        ''' 点积：<c>Sum(CSng(a(i)) * CSng(b(i)))</c>，累加在 Double 中进行
        ''' </summary>
        ''' <param name="a">第一个向量</param>
        ''' <param name="b">第二个向量</param>
        ''' <returns>Double 精度的点积</returns>
        Public Function Dot(a As Half(), b As Half()) As Double
            CheckSameLength(a, b)

            Dim n As Integer = a.Length
            Dim total As Double = 0R
            Dim i As Integer = 0

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim alo, ahi, blo, bhi As Vector128(Of Integer)
                Dim acc0 As Vector128(Of Double) = Vector128(Of Double).Zero
                Dim acc1 As Vector128(Of Double) = Vector128(Of Double).Zero

                Do While i <= last
                    LoadLanes(a, i, alo, ahi)
                    LoadLanes(b, i, blo, bhi)
                    Accumulate(Vector128.Multiply(LanesToSingle(alo), LanesToSingle(blo)), acc0, acc1)
                    Accumulate(Vector128.Multiply(LanesToSingle(ahi), LanesToSingle(bhi)), acc0, acc1)
                    i += Lanes
                Loop

                total += ReduceDouble(acc0) + ReduceDouble(acc1)
            End If

            Do While i < n
                total += CSng(a(i)) * CSng(b(i))
                i += 1
            Loop

            Return total
        End Function

        ''' <summary>
        ''' 平方和：<c>Sum(CSng(a(i)) ^ 2)</c>，累加在 Double 中进行
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <returns>Double 精度的平方和</returns>
        Public Function SumOfSquares(a As Half()) As Double
            Return Dot(a, a)
        End Function

        ''' <summary>
        ''' 欧几里得范数（L2 范数）
        ''' </summary>
        ''' <param name="a">输入向量</param>
        ''' <returns>Double 精度的 L2 范数</returns>
        Public Function Norm(a As Half()) As Double
            ' 注意：本工程的 RootNamespace 就是 Microsoft.VisualBasic.Math，
            ' 裸写 Math.Xxx 会被解析到本命名空间而不是 System.Math，必须全限定
            Return System.Math.Sqrt(SumOfSquares(a))
        End Function

        ''' <summary>
        ''' 最小值。比较在 Single 精度上进行，结果再舍入回 <see cref="System.Half"/>。
        ''' </summary>
        ''' <param name="a">非空输入向量</param>
        ''' <returns>向量中的最小值</returns>
        Public Function Min(a As Half()) As Half
            Return CType(MinMaxSingle(a, findMin:=True), Half)
        End Function

        ''' <summary>
        ''' 最大值。比较在 Single 精度上进行，结果再舍入回 <see cref="System.Half"/>。
        ''' </summary>
        ''' <param name="a">非空输入向量</param>
        ''' <returns>向量中的最大值</returns>
        Public Function Max(a As Half()) As Half
            Return CType(MinMaxSingle(a, findMin:=False), Half)
        End Function

        ''' <summary>
        ''' 在 Single 精度上做极值归约：向量路径用 4 路 lane 归约，尾部走标量
        ''' </summary>
        Private Function MinMaxSingle(a As Half(), findMin As Boolean) As Single
            If a Is Nothing OrElse a.Length = 0 Then
                Throw New ArgumentException("the input vector can not be empty!", NameOf(a))
            End If

            Dim n As Integer = a.Length
            Dim best As Single = CSng(a(0))
            Dim i As Integer = 1

            If CanVectorize(n) Then
                Dim last As Integer = n - Lanes
                Dim lo, hi As Vector128(Of Integer)

                Do While i <= last
                    LoadLanes(a, i, lo, hi)
                    best = ReduceBlock(LanesToSingle(lo), best, findMin)
                    best = ReduceBlock(LanesToSingle(hi), best, findMin)
                    i += Lanes
                Loop
            End If

            Do While i < n
                Dim x As Single = CSng(a(i))

                If Single.IsNaN(x) Then
                    Return Single.NaN
                ElseIf findMin Then
                    If x < best Then best = x
                Else
                    If x > best Then best = x
                End If

                i += 1
            Loop

            Return best
        End Function

        ''' <summary>
        ''' 把一个 4 路 lane 归约为单个 Single，再与已有的极值合并
        ''' </summary>
        Private Function ReduceBlock(v As Vector128(Of Single), best As Single, findMin As Boolean) As Single
            ' NaN 传播：与 Enumerable.Min/Max 对浮点序列的行为保持一致
            For lane As Integer = 0 To 3
                Dim xi As Single = Vector128.GetElement(v, lane)

                If Single.IsNaN(xi) Then
                    Return Single.NaN
                End If
            Next

            Dim block As Single = Vector128.GetElement(v, 0)

            For lane As Integer = 1 To 3
                Dim x As Single = Vector128.GetElement(v, lane)

                If findMin Then
                    If x < block Then block = x
                Else
                    If x > block Then block = x
                End If
            Next

            Return If(findMin, System.Math.Min(best, block), System.Math.Max(best, block))
        End Function

#End Region

#Region "argument checks"

        ''' <summary>
        ''' 校验两个输入向量非空且等长
        ''' </summary>
        Private Sub CheckSameLength(a As Half(), b As Half())
            If a Is Nothing Then
                Throw New ArgumentNullException(NameOf(a))
            End If
            If b Is Nothing Then
                Throw New ArgumentNullException(NameOf(b))
            End If
            If a.Length <> b.Length Then
                Throw New ArgumentException(
                    $"the dimension size between two vector should be equals! ({a.Length} <> {b.Length})",
                    NameOf(b))
            End If
        End Sub

#End Region

    End Module
End Namespace
