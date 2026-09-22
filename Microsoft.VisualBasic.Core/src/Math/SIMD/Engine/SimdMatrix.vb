#Region "Microsoft.VisualBasic::a17c2f0d4b8e44f2a6d9e1c73b5f820c, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Engine\SimdMatrix.vb"

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

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 矩阵级（row-major <c>Double()()</c>）批量 SIMD 内核门面。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <see cref="SimdEngine"/> / <see cref="SimdReduce"/> / <see cref="SimdMath"/> /
    ''' <see cref="SimdParallel"/> 都是面向一维连续数组的内核，而
    ''' <c>Microsoft.VisualBasic.Math</c> 中的矩阵对象内部数据是
    ''' “行数组的数组”（每行是一段连续内存）。这里把矩阵操作**按行**适配到
    ''' 一维内核上，避免在每个调用点重复书写 <c>For Each row</c> + 内核分派的样板代码。
    ''' </para>
    ''' <para>
    ''' <b>为什么按行而不是按列</b>：行是连续内存，可以直接交给
    ''' <c>New Vector(Of T)(array, offset)</c> 批量装载；列方向是 strided access，
    ''' 没有可用的向量化原语。需要列数据时先通过 <see cref="ExtractColumn(Double()(), Integer)"/>
    ''' 把它变成连续数组。
    ''' </para>
    ''' <para>
    ''' <b>形状契约</b>：与 <see cref="SimdEngine"/> 的逐元素内核不同，这里**会**校验
    ''' 两个矩阵的行数与每行长度都一致，不一致时抛出 <see cref="ArgumentException"/>；
    ''' 因为逐元素内核假设长度一致，缺少这层校验只会导致难以定位的越界访问。
    ''' </para>
    ''' <para>
    ''' <b>回退</b>：所有内核最终都会经过 <see cref="SimdEngine.CanVectorize(Of T)(Integer)"/>
    ''' 检查 <see cref="SIMDEnvironment.IsEnabled"/>，因此
    ''' <c>SIMDConfiguration.disable</c> 仍然是全局有效的逃生开关。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SimdMatrix

        Private Sub New()
        End Sub

#Region "helpers"

        ''' <summary>
        ''' 矩阵行数（<c>Nothing</c> 视作 0 行）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function RowCount(m As Double()()) As Integer
            Return If(m Is Nothing, 0, m.Length)
        End Function

        ''' <summary>
        ''' 矩阵列数（以第一行的长度为基准）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ColumnCount(m As Double()()) As Integer
            If m Is Nothing OrElse m.Length = 0 Then Return 0

            Dim first As Double() = m(0)

            Return If(first Is Nothing, 0, first.Length)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function RowLength(m As Double()(), i As Integer) As Integer
            Dim row As Double() = m(i)

            Return If(row Is Nothing, 0, row.Length)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function CheckNull(m As Double()(), name As String) As Double()()
            If m Is Nothing Then
                Throw New ArgumentNullException(name, "the input matrix can not be NULL!")
            End If

            Return m
        End Function

        ''' <summary>
        ''' 校验两个矩阵的行数与每行长度都一致。
        ''' </summary>
        Private Shared Sub CheckAgree(a As Double()(), b As Double()(), nameA As String, nameB As String)
            Call CheckNull(a, nameA)
            Call CheckNull(b, nameB)

            If a.Length <> b.Length Then
                Throw New ArgumentException($"the row dimension of the two matrix not agree: {a.Length} vs {b.Length}!")
            End If

            For i As Integer = 0 To a.Length - 1
                Dim la As Integer = RowLength(a, i)
                Dim lb As Integer = RowLength(b, i)

                If la <> lb Then
                    Throw New ArgumentException($"the column dimension of the two matrix not agree on row {i}: {la} vs {lb}!")
                End If
            Next
        End Sub

        ''' <summary>
        ''' 分配 <c>[rows, cols]</c> 的矩形容器（每行独立分配，内容未初始化）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function NewMatrix(rows As Integer, cols As Integer) As Double()()
            Dim m As Double()() = SimdEngine.NewArray(Of Double())(rows)

            For i As Integer = 0 To rows - 1
                m(i) = SimdEngine.NewArray(Of Double)(cols)
            Next

            Return m
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Binary(a As Double()(), b As Double()(),
                                       op As Func(Of Double(), Double(), Double())) As Double()()
            Call CheckAgree(a, b, NameOf(a), NameOf(b))

            Dim out As Double()() = SimdEngine.NewArray(Of Double())(a.Length)

            For i As Integer = 0 To a.Length - 1
                out(i) = op(a(i), b(i))
            Next

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function BinaryInPlace(a As Double()(), b As Double()(),
                                              op As Func(Of Double(), Double(), Double())) As Double()()
            Call CheckAgree(a, b, NameOf(a), NameOf(b))

            For i As Integer = 0 To a.Length - 1
                a(i) = op(a(i), b(i))
            Next

            Return a
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function ScalarOp(a As Double()(), scalar As Double,
                                         op As Func(Of Double(), Double, Double())) As Double()()
            Call CheckNull(a, NameOf(a))

            Dim out As Double()() = SimdEngine.NewArray(Of Double())(a.Length)

            For i As Integer = 0 To a.Length - 1
                out(i) = op(a(i), scalar)
            Next

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Unary(a As Double()(), op As Func(Of Double(), Double())) As Double()()
            Call CheckNull(a, NameOf(a))

            Dim out As Double()() = SimdEngine.NewArray(Of Double())(a.Length)

            For i As Integer = 0 To a.Length - 1
                out(i) = op(a(i))
            Next

            Return out
        End Function

#End Region

#Region "elementwise binary"

        ''' <summary>
        ''' 逐元素相加：<c>out(i)(j) = a(i)(j) + b(i)(j)</c>
        ''' </summary>
        Public Shared Function Add(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, Function(x, y) SimdEngine.Add(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 逐元素相减：<c>out(i)(j) = a(i)(j) - b(i)(j)</c>
        ''' </summary>
        Public Shared Function Subtract(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, Function(x, y) SimdEngine.Subtract(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 逐元素相乘：<c>out(i)(j) = a(i)(j) * b(i)(j)</c>
        ''' </summary>
        Public Shared Function Multiply(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, Function(x, y) SimdEngine.Multiply(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 逐元素标准相除：<c>out(i)(j) = a(i)(j) / b(i)(j)</c>（<c>0/0 = NaN</c>）。
        ''' </summary>
        Public Shared Function Divide(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, AddressOf SimdEngine.Divide)
        End Function

        ''' <summary>
        ''' 逐元素相除，且分子为零时结果置零（避免 <c>0/0</c> 产生 <see cref="Double.NaN"/>）。
        ''' </summary>
        Public Shared Function DivideZeroSafe(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, AddressOf SimdEngine.DivideZeroSafe)
        End Function

        ''' <summary>
        ''' 就地逐元素相加。
        ''' </summary>
        Public Shared Function AddInPlace(a As Double()(), b As Double()()) As Double()()
            Return BinaryInPlace(a, b, Function(x, y) SimdEngine.AddInPlace(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 就地逐元素相减。
        ''' </summary>
        Public Shared Function SubtractInPlace(a As Double()(), b As Double()()) As Double()()
            Return BinaryInPlace(a, b, Function(x, y) SimdEngine.SubtractInPlace(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 就地逐元素相乘。
        ''' </summary>
        Public Shared Function MultiplyInPlace(a As Double()(), b As Double()()) As Double()()
            Return BinaryInPlace(a, b, Function(x, y) SimdEngine.MultiplyInPlace(Of Double)(x, y))
        End Function

        ''' <summary>
        ''' 就地逐元素相除。
        ''' </summary>
        Public Shared Function DivideInPlace(a As Double()(), b As Double()()) As Double()()
            Return BinaryInPlace(a, b, AddressOf SimdEngine.DivideInPlace)
        End Function

        ''' <summary>
        ''' 就地逐元素相除（分子为零则结果置零）。
        ''' </summary>
        Public Shared Function DivideZeroSafeInPlace(a As Double()(), b As Double()()) As Double()()
            Return BinaryInPlace(a, b, AddressOf SimdEngine.DivideZeroSafeInPlace)
        End Function

#End Region

#Region "scalar broadcast"

        ''' <summary>
        ''' 矩阵加标量：<c>out(i)(j) = a(i)(j) + scalar</c>
        ''' </summary>
        Public Shared Function AddScalar(a As Double()(), scalar As Double) As Double()()
            Return ScalarOp(a, scalar, Function(x, s) SimdEngine.AddScalar(Of Double)(x, s))
        End Function

        ''' <summary>
        ''' 矩阵减标量：<c>out(i)(j) = a(i)(j) - scalar</c>
        ''' </summary>
        Public Shared Function SubtractScalar(a As Double()(), scalar As Double) As Double()()
            Return ScalarOp(a, scalar, Function(x, s) SimdEngine.SubtractScalar(Of Double)(x, s))
        End Function

        ''' <summary>
        ''' 标量减矩阵：<c>out(i)(j) = scalar - a(i)(j)</c>
        ''' </summary>
        Public Shared Function ScalarSubtract(scalar As Double, a As Double()()) As Double()()
            Return ScalarOp(a, scalar, Function(x, s) SimdEngine.ScalarSubtract(Of Double)(s, x))
        End Function

        ''' <summary>
        ''' 数乘：<c>out(i)(j) = scalar * a(i)(j)</c>
        ''' </summary>
        Public Shared Function MultiplyScalar(a As Double()(), scalar As Double) As Double()()
            Return ScalarOp(a, scalar, Function(x, s) SimdEngine.MultiplyScalar(Of Double)(s, x))
        End Function

        ''' <summary>
        ''' 矩阵除以标量：<c>out(i)(j) = a(i)(j) / scalar</c>
        ''' </summary>
        Public Shared Function DivideScalar(a As Double()(), scalar As Double) As Double()()
            Return ScalarOp(a, scalar, AddressOf SimdEngine.DivideScalar)
        End Function

        ''' <summary>
        ''' 标量除以矩阵：<c>out(i)(j) = scalar / a(i)(j)</c>
        ''' </summary>
        Public Shared Function ScalarDivide(scalar As Double, a As Double()()) As Double()()
            Return ScalarOp(a, scalar, Function(x, s) SimdEngine.ScalarDivide(s, x))
        End Function

        ''' <summary>
        ''' 就地数乘：<c>a(i)(j) = scalar * a(i)(j)</c>
        ''' </summary>
        Public Shared Function MultiplyScalarInPlace(a As Double()(), scalar As Double) As Double()()
            Call CheckNull(a, NameOf(a))

            For i As Integer = 0 To a.Length - 1
                a(i) = SimdEngine.MultiplyScalarInPlace(Of Double)(a(i), scalar)
            Next

            Return a
        End Function

#End Region

#Region "unary"

        ''' <summary>
        ''' 逐元素取负。
        ''' </summary>
        Public Shared Function Negate(a As Double()()) As Double()()
            Return Unary(a, Function(x) SimdMath.Negate(Of Double)(x))
        End Function

        ''' <summary>
        ''' 逐元素绝对值。
        ''' </summary>
        Public Shared Function Abs(a As Double()()) As Double()()
            Return Unary(a, Function(x) SimdMath.Abs(Of Double)(x))
        End Function

        ''' <summary>
        ''' 逐元素平方根。
        ''' </summary>
        Public Shared Function Sqrt(a As Double()()) As Double()()
            Return Unary(a, Function(x) SimdMath.Sqrt(x))
        End Function

        ''' <summary>
        ''' 逐元素平方。
        ''' </summary>
        Public Shared Function Square(a As Double()()) As Double()()
            Return Unary(a, Function(x) SimdMath.Square(Of Double)(x))
        End Function

        ''' <summary>
        ''' 逐元素固定次幂：<c>out(i)(j) = a(i)(j) ^ exponent</c>
        ''' </summary>
        ''' <remarks>
        ''' 指数为 2 / 3 / 4 / 0.5 时会走到向量化的快速路径，其余指数退回标量 <c>^</c>。
        ''' </remarks>
        Public Shared Function PowScalar(a As Double()(), exponent As Double) As Double()()
            Return Unary(a, Function(x) SimdMath.PowScalar(x, exponent))
        End Function

        ''' <summary>
        ''' 逐元素幂运算：<c>out(i)(j) = a(i)(j) ^ b(i)(j)</c>
        ''' </summary>
        Public Shared Function Pow(a As Double()(), b As Double()()) As Double()()
            Return Binary(a, b, AddressOf SimdMath.Pow)
        End Function

        ''' <summary>
        ''' 逐元素以 <paramref name="base"/> 为底的对数。
        ''' </summary>
        Public Shared Function Log(a As Double()(), Optional base As Double = std.E) As Double()()
            Return Unary(a, Function(x) SimdMath.Log(x, base))
        End Function

        ''' <summary>
        ''' 逐元素自然指数（无硬件指令，保持标量内核）。
        ''' </summary>
        Public Shared Function Exp(a As Double()()) As Double()()
            Return Unary(a, Function(x) SimdMath.Exp(x))
        End Function

#End Region

#Region "structure"

        ''' <summary>
        ''' 矩阵转置。
        ''' </summary>
        ''' <remarks>
        ''' 使用 32x32 分块，把跨行的离散访问收拢成块内的顺序访问以降低 cache miss。
        ''' 转置本身是纯数据搬运，没有可用的算术指令，因此不使用逐元素内核。
        ''' </remarks>
        Public Shared Function Transpose(a As Double()()) As Double()()
            Call CheckNull(a, NameOf(a))

            Dim rows As Integer = a.Length
            If rows = 0 Then Return Array.Empty(Of Double())()

            Dim cols As Integer = RowLength(a, 0)
            If cols = 0 Then Return Array.Empty(Of Double())()

            Dim t As Double()() = NewMatrix(cols, rows)

            Const BLOCK As Integer = 32

            For i0 As Integer = 0 To rows - 1 Step BLOCK
                Dim i1 As Integer = std.Min(i0 + BLOCK, rows) - 1

                For j0 As Integer = 0 To cols - 1 Step BLOCK
                    Dim j1 As Integer = std.Min(j0 + BLOCK, cols) - 1

                    For i As Integer = i0 To i1
                        Dim row As Double() = a(i)

                        For j As Integer = j0 To j1
                            t(j)(i) = row(j)
                        Next
                    Next
                Next
            Next

            Return t
        End Function

        ''' <summary>
        ''' 抽取指定列为一个连续的一维数组。
        ''' </summary>
        ''' <remarks>
        ''' 跨行读取是 strided access，没有高效的向量化 gather 原语，因此保持顺序拷贝；
        ''' 它的价值在于把列数据变成连续内存，让后续的列内积/列范数可以走向量化路径。
        ''' </remarks>
        Public Shared Function ExtractColumn(a As Double()(), col As Integer) As Double()
            Call CheckNull(a, NameOf(a))

            Dim rows As Integer = a.Length
            If rows = 0 Then Return Array.Empty(Of Double)()

            Dim v As Double() = SimdEngine.NewArray(Of Double)(rows)

            For i As Integer = 0 To rows - 1
                v(i) = a(i)(col)
            Next

            Return v
        End Function

        ''' <summary>
        ''' 按行缩放：<c>out(i)(j) = factors(i) * a(i)(j)</c>
        ''' </summary>
        ''' <param name="factors">长度必须等于矩阵的行数</param>
        Public Shared Function MultiplyRows(a As Double()(), factors As Double()) As Double()()
            Call CheckNull(a, NameOf(a))
            If factors Is Nothing Then Throw New ArgumentNullException(NameOf(factors))

            Dim rows As Integer = a.Length
            If rows <> factors.Length Then
                Throw New ArgumentException($"the size of the factors({factors.Length}) should be equals to the row dimension({rows})!")
            End If

            Dim out As Double()() = SimdEngine.NewArray(Of Double())(rows)

            For i As Integer = 0 To rows - 1
                out(i) = SimdEngine.MultiplyScalar(Of Double)(factors(i), a(i))
            Next

            Return out
        End Function

#End Region

#Region "reduce"

        ''' <summary>
        ''' 1-范数：所有列绝对值之和中的最大值。
        ''' </summary>
        ''' <remarks>
        ''' 逐行累加绝对值，列方向仍然按 <c>[0, rows)</c> 的顺序累加，
        ''' 因此结果与原实现逐位一致（不存在归约重排）；
        ''' 累加过程使用就地内核，不需要为每一行生成绝对值临时数组。
        ''' </remarks>
        Public Shared Function Norm1(a As Double()()) As Double
            Call CheckNull(a, NameOf(a))

            Dim rows As Integer = a.Length
            If rows = 0 Then Return 0.0

            Dim cols As Integer = RowLength(a, 0)
            If cols = 0 Then Return 0.0

            Dim acc As Double() = SimdEngine.NewArray(Of Double)(cols)

            For i As Integer = 0 To rows - 1
                Call SimdEngine.AddAbsInPlace(acc, a(i))
            Next

            Return SimdReduce.Max(acc)
        End Function

        ''' <summary>
        ''' 无穷范数：所有行绝对值之和中的最大值。
        ''' </summary>
        Public Shared Function NormInf(a As Double()()) As Double
            Call CheckNull(a, NameOf(a))

            Dim f As Double = 0

            For i As Integer = 0 To a.Length - 1
                f = std.Max(f, SimdReduce.L1Norm(a(i)))
            Next

            Return f
        End Function

        ''' <summary>
        ''' Frobenius 范数：<c>SQRT(SUM(a(i)(j) ^ 2))</c>。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 与原先 <c>Hypot</c> 逐步缩放实现一样具备抗上溢能力：先用
        ''' <see cref="SimdReduce.MaxAbs(Double(), Integer, Integer)"/> 探测矩阵的最大绝对值，
        ''' 只有当它大到平方和可能上溢时（<c>&gt;= 1E+150</c>）才切换到缩放路径。
        ''' </para>
        ''' <para>
        ''' 常规量级的数据直接累加平方和，因此热路径上没有任何逐行临时数组；
        ''' 这也让常见的 <c>NormF</c> 调用不再是「每行两次分配」的形态。
        ''' </para>
        ''' </remarks>
        Public Shared Function NormF(a As Double()()) As Double
            Call CheckNull(a, NameOf(a))

            Dim scale As Double = 0

            For i As Integer = 0 To a.Length - 1
                If RowLength(a, i) = 0 Then Continue For

                scale = std.Max(scale, SimdReduce.MaxAbs(a(i)))
            Next

            If scale = 0.0 Then Return 0.0
            If Double.IsInfinity(scale) Then Return Double.PositiveInfinity
            If Double.IsNaN(scale) Then Return Double.NaN

            ' 平方和的上溢保护阈值（Double.MaxValue 的平方根量级）
            Const overflowGuard As Double = 1.0E+150

            Dim sumSq As Double = 0

            If scale < overflowGuard Then
                For i As Integer = 0 To a.Length - 1
                    sumSq += SimdReduce.SumSquares(a(i))
                Next

                Return std.Sqrt(sumSq)
            End If

            ' 极端量级：先缩放再累加，避免中间结果上溢
            For i As Integer = 0 To a.Length - 1
                sumSq += SimdReduce.SumSquares(SimdEngine.DivideScalar(a(i), scale))
            Next

            Return scale * std.Sqrt(sumSq)
        End Function

        ''' <summary>
        ''' 矩阵的迹：主对角线元素之和。
        ''' </summary>
        Public Shared Function Trace(a As Double()()) As Double
            Call CheckNull(a, NameOf(a))

            Dim n As Integer = std.Min(RowCount(a), ColumnCount(a))
            Dim t As Double = 0

            For i As Integer = 0 To n - 1
                t += a(i)(i)
            Next

            Return t
        End Function

        ''' <summary>
        ''' 全矩阵最大值及其位置。
        ''' </summary>
        ''' <remarks>
        ''' 只在**严格大于**当前最大值时更新位置，因此多个相同最大值时返回第一个出现的位置，
        ''' 与原逐行逐列扫描的语义一致。空矩阵返回 <see cref="Double.MinValue"/> 且位置保持 <c>(0, 0)</c>。
        ''' </remarks>
        Public Shared Function MaxIndex(a As Double()(), ByRef row As Integer, ByRef col As Integer) As Double
            Call CheckNull(a, NameOf(a))

            row = 0
            col = 0

            Dim maxVal As Double = Double.MinValue

            For i As Integer = 0 To a.Length - 1
                Dim r As Double() = a(i)
                If r Is Nothing OrElse r.Length = 0 Then Continue For

                Dim rowMax As Double = SimdReduce.Max(r)

                If rowMax > maxVal Then
                    maxVal = rowMax
                    row = i
                    col = Array.IndexOf(r, rowMax)
                End If
            Next

            Return maxVal
        End Function

        ''' <summary>
        ''' 全矩阵最小值及其位置。
        ''' </summary>
        Public Shared Function MinIndex(a As Double()(), ByRef row As Integer, ByRef col As Integer) As Double
            Call CheckNull(a, NameOf(a))

            row = 0
            col = 0

            Dim minVal As Double = Double.MaxValue

            For i As Integer = 0 To a.Length - 1
                Dim r As Double() = a(i)
                If r Is Nothing OrElse r.Length = 0 Then Continue For

                Dim rowMin As Double = SimdReduce.Min(r)

                If rowMin < minVal Then
                    minVal = rowMin
                    row = i
                    col = Array.IndexOf(r, rowMin)
                End If
            Next

            Return minVal
        End Function

#End Region

#Region "blas"

        ''' <summary>
        ''' 矩阵乘积 <c>A * B</c>：右矩阵预转置 + 行方向并行 + 行内 FMA 点积。
        ''' </summary>
        ''' <param name="a">左矩阵，形状 <c>m x n</c></param>
        ''' <param name="b">右矩阵，形状 <c>n x p</c></param>
        Public Shared Function Dot(a As Double()(), b As Double()()) As Double()()
            Call CheckNull(a, NameOf(a))
            Call CheckNull(b, NameOf(b))

            Return SimdParallel.MatrixDot(a, b)
        End Function

        ''' <summary>
        ''' 矩阵向量乘积：<c>out(i) = SUM(a(i)(j) * x(j))</c>，使用 FMA 点积。
        ''' </summary>
        Public Shared Function MatrixVector(a As Double()(), x As Double()) As Double()
            Call CheckNull(a, NameOf(a))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

            Dim rows As Integer = a.Length
            If rows = 0 Then Return Array.Empty(Of Double)()

            Dim cols As Integer = RowLength(a, 0)
            If x.Length <> cols Then
                Throw New ArgumentException($"the size of the vector({x.Length}) should be equals to the column dimension({cols})!")
            End If

            Dim out As Double() = SimdEngine.NewArray(Of Double)(rows)

            For i As Integer = 0 To rows - 1
                out(i) = SIMDIntrinsics.DotFma(a(i), x)
            Next

            Return out
        End Function

        ''' <summary>
        ''' 秩一更新：<c>a(i)(j) += alpha * x(i) * y(j)</c>，使用 FMA/AXPY 就地更新。
        ''' </summary>
        ''' <param name="a">目标矩阵，形状 <c>m x n</c></param>
        ''' <param name="x">长度 <c>m</c> 的列因子</param>
        ''' <param name="y">长度 <c>n</c> 的行因子</param>
        ''' <param name="alpha">缩放因子；需要做减法时传入负值即可</param>
        Public Shared Sub Rank1Update(a As Double()(), x As Double(), y As Double(), alpha As Double)
            Call CheckNull(a, NameOf(a))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))

            Dim rows As Integer = a.Length
            If rows = 0 Then Return
            If x.Length <> rows Then
                Throw New ArgumentException($"the size of the factor x({x.Length}) should be equals to the row dimension({rows})!")
            End If

            Dim cols As Integer = RowLength(a, 0)
            If y.Length <> cols Then
                Throw New ArgumentException($"the size of the factor y({y.Length}) should be equals to the column dimension({cols})!")
            End If

            For i As Integer = 0 To rows - 1
                Call SIMDIntrinsics.AxpyInPlace(alpha * x(i), y, a(i))
            Next
        End Sub

        ''' <summary>
        ''' 就地 AXPY：<c>y(i) += alpha * x(i)</c>，使用 FMA。
        ''' </summary>
        Public Shared Sub AxpyInPlace(alpha As Double, x As Double(), y As Double())
            Call SIMDIntrinsics.AxpyInPlace(alpha, x, y)
        End Sub

#End Region
    End Class
End Namespace
