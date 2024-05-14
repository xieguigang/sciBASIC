#Region "Microsoft.VisualBasic::acae0abdd944a5121213c982ed94334d, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\Vector.vb"

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

    '   Total Lines: 1100
    '    Code Lines: 486
    ' Comment Lines: 455
    '   Blank Lines: 159
    '     File Size: 39.72 KB


    '     Class Vector
    ' 
    '         Properties: [Mod], Data, Inf, IsNaN, IsNumeric
    '                     NaN, Range, SumMagnitude, Unit, Zero
    ' 
    '         Constructor: (+12 Overloads) Sub New
    ' 
    '         Function: Abs, AsDiagonal, AsSparse, CumSum, (+3 Overloads) DotProduct
    '                   norm, Ones, Order, Product, (+2 Overloads) rand
    '                   Scalar, ScaleToRange, seq, slice, SumMagnitudes
    '                   (+2 Overloads) ToString
    ' 
    '         Sub: (+3 Overloads) CopyTo
    ' 
    '         Operators: (+4 Overloads) -, (+7 Overloads) *, (+3 Overloads) /, (+3 Overloads) ^, (+4 Overloads) +
    '                    <, (+3 Overloads) <=, (+2 Overloads) <>, (+2 Overloads) =, >
    '                    (+3 Overloads) >=, (+2 Overloads) Or, (+2 Overloads) Xor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Scripting.Rscript
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports numpy = Microsoft.VisualBasic.Language.Python
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace LinearAlgebra

    ''' <summary>
    ''' A numeric vector
    ''' </summary>
    ''' <remarks>
    ''' this numeric vector is based on the <see cref="Vector(Of Double)"/>
    ''' </remarks>
    Public Class Vector : Inherits GenericVector(Of Double)
        Implements IVector

#Region "Properties"

#Region "Default values"
        ''' <summary>
        ''' <see cref="System.Double.NaN"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property NaN As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector({Double.NaN})
            End Get
        End Property

        ''' <summary>
        ''' <see cref="System.Double.PositiveInfinity"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Inf As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector({Double.PositiveInfinity})
            End Get
        End Property

        ''' <summary>
        ''' Only one number in the vector and its value is ZERO
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Zero(Optional [Dim] As Integer = 1) As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector(Linq.Extensions.Repeats(0R, [Dim]))
            End Get
        End Property
#End Region

        ''' <summary>
        ''' <see cref="[Dim]"/>为1?即当前的向量对象是否是只包含有一个数字？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNumeric As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return [Dim] = 1
            End Get
        End Property

        Public ReadOnly Property IsNaN As BooleanVector
            Get
                Return New BooleanVector(From xi As Double In buffer Select xi.IsNaNImaginary)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rangeExpression"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' syntax helper
        ''' </remarks>
        Default Public Overloads Property Item(rangeExpression As String) As Vector
            Get
                Return MyBase.Item(rangeExpression).AsVector
            End Get
            Set(value As Vector)
                MyBase.Item(rangeExpression) = value.AsList
            End Set
        End Property

        ''' <summary>
        ''' ``norm2()``
        ''' 
        ''' 向量模的平方，``||x||``是向量``x=(x1，x2，…，xp)``的欧几里得范数
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' SquaredNorm 平方绝对值的总和
        ''' </remarks>
        Public ReadOnly Property [Mod] As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Me ^ 2).Sum
            End Get
        End Property

        Public Shared Function seq(from As Integer, [to] As Integer, Optional [by] As Double = 1) As Vector
            Return seq2(from, [to], by:=by)
        End Function

        ''' <summary>
        ''' ``norm()``
        ''' 
        ''' http://math.stackexchange.com/questions/440320/what-is-magnitude-of-sum-of-two-vector
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SumMagnitude As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return std.Sqrt(Me.Mod)
            End Get
        End Property

        ''' <summary>
        ''' normalize
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Unit As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' function $norm(a: Number[]) {
                '    return Math.sqrt(a[0] * a[0] + a[1] * a[1]);
                ' }

                ' function $normalize(a: Number[]) {
                '    var n = $norm(a);
                '    return $mult(a, 1 / n);
                ' }
                Return Me / SumMagnitude
            End Get
        End Property

        ''' <summary>
        ''' ``[min, max]``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Range As DoubleRange
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New DoubleRange(Me.AsEnumerable)
            End Get
        End Property

        Protected Overridable Property Data As Double() Implements IVector.Data
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer
            End Get
            Set(value As Double())
                buffer = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Creates vector with <paramref name="m"/> element and init value set to zero
        ''' It creates a double-precision vector of the specified length with each element equal to 0.
        ''' </summary>
        ''' <param name="m"></param>
        ''' <remarks>
        ''' equality to R expression
        ''' 
        ''' ```R
        ''' numeric(<paramref name="m"/>)
        ''' ```
        ''' </remarks>
        Public Sub New(m As Integer)
            Call Me.New(0R, m)
        End Sub

        Sub New(f As Double)
            Call Me.New({f})
        End Sub

        Sub New(f As Single)
            Call Me.New({CDbl(f)})
        End Sub

        ''' <summary>
        ''' 创建一个空的向量，包含有零个元素
        ''' </summary>
        Public Sub New()
            Call MyBase.New()
        End Sub

        ''' <summary>
        ''' Creates vector with a specific value sequence.
        ''' </summary>
        ''' <param name="data">
        ''' a sequence of numeric value that will fill the vector's 
        ''' data <see cref="buffer"/>.
        ''' </param>
        Sub New(data As IEnumerable(Of Double))
            Call MyBase.New(data)
        End Sub

        ''' <summary>
        ''' Creates vector with a specific value sequence
        ''' </summary>
        ''' <param name="shorts"></param>
        Sub New(shorts As IEnumerable(Of Single))
            Call Me.New(shorts.Select(Function(x) CDbl(x)))
        End Sub

        ''' <summary>
        ''' Init with transform
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="apply"></param>
        Sub New(x As IEnumerable(Of Double), apply As Func(Of Double, Double))
            Me.New(x.Select(apply))
        End Sub

        Sub New(from#, to#, Optional by# = 0.01)
            Me.New(VBMath.seq(from, [to], by))
        End Sub

        ''' <summary>
        ''' Creates vector with a specific value sequence
        ''' </summary>
        ''' <param name="integers"></param>
        Sub New(integers As IEnumerable(Of Integer))
            Me.New(integers.Select(Function(n) CDbl(n)))
        End Sub

        ''' <summary>
        ''' Creates vector with m element and init value specific by init parameter.
        ''' </summary>
        ''' <param name="init"></param>
        ''' <param name="m"></param>
        Sub New(init#, m%)
            Call MyBase.New(capacity:=m)

            For i As Integer = 0 To m - 1
                buffer(i) = init
            Next
        End Sub

        ''' <summary>
        ''' Creates a vector from a specified array starting at a specified index position.
        ''' </summary>
        ''' <param name="values">
        ''' The values to add to the vector, as an array of objects of type T. 
        ''' The array must contain at least Count elements from the specified 
        ''' index and only the first Count elements are used.
        ''' </param>
        ''' <param name="index">
        ''' The starting index position from which to create the vector.
        ''' </param>
        Sub New(values As Single(), index As Integer)
            Call Me.New(values.Skip(index).Select(Function(sng) CDbl(sng)))
        End Sub

        ''' <summary>
        ''' Creates a vector from a specified array starting at a specified index position.
        ''' </summary>
        ''' <param name="values">
        ''' The values to add to the vector, as an array of objects of type T. 
        ''' The array must contain at least Count elements from the specified 
        ''' index and only the first Count elements are used.
        ''' </param>
        ''' <param name="index">
        ''' The starting index position from which to create the vector.
        ''' </param>
        Sub New(values As Double(), index As Integer, Optional count As Integer = 8)
            Call Me.New(values.Skip(index).Take(count))
        End Sub

        Public Function AsDiagonal() As NumericMatrix
            Dim rows As New List(Of Double())
            Dim r As Double()
            Dim size As Integer = buffer.Length

            For i As Integer = 0 To size - 1
                r = New Double(size - 1) {}
                r(i) = buffer(i)
                rows.Add(r)
            Next

            Return New NumericMatrix(rows)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsSparse() As SparseVector
            Return New SparseVector(Me)
        End Function

        ''' <summary>
        ''' Copies the vector instance to a specified destination array starting at a specified index position.
        ''' </summary>
        ''' <param name="destination">The array to receive a copy of the vector values.</param>
        ''' <param name="startIndex">The starting index in destination at which to begin the copy operation.</param>
        Public Sub CopyTo(ByRef destination As Double(), startIndex As Integer)
            For id As Integer = 0 To buffer.Length - 1
                destination(id + startIndex) = buffer(id)
            Next
        End Sub

        ''' <summary>
        ''' Copies the vector instance to a specified destination array starting at a specified index position.
        ''' </summary>
        ''' <param name="destination">The array to receive a copy of the vector values.</param>
        ''' <param name="startIndex">The starting index in destination at which to begin the copy operation.</param>
        Public Sub CopyTo(destination As Integer(), startIndex As Integer)
            For id As Integer = 0 To buffer.Length - 1
                destination(id + startIndex) = buffer(id)
            Next
        End Sub

        ''' <summary>
        ''' Copies the vector instance to a specified destination array starting at a specified index position.
        ''' </summary>
        ''' <param name="destination">The array to receive a copy of the vector values.</param>
        ''' <param name="startIndex">The starting index in destination at which to begin the copy operation.</param>
        Public Sub CopyTo(destination As Single(), startIndex As Integer)
            For id As Integer = 0 To buffer.Length - 1
                destination(id + startIndex) = buffer(id)
            Next
        End Sub

#Region "Operators"
        ''' <summary>
        ''' 两个向量加法算符重载，分量分别相加
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(v1 As Vector, v2 As Vector) As Vector
            Dim output As Double()

            If v1 Is Nothing Then
                Return New Vector(v2.ToArray)
            ElseIf v2 Is Nothing Then
                Return New Vector(v1.ToArray)
            End If

            If v1.Length = 1 Then
                output = SIMD.Add.f64_op_add_f64_scalar(v2.buffer, v1(Scan0))
            ElseIf v2.Length = 1 Then
                output = SIMD.Add.f64_op_add_f64_scalar(v1.buffer, v2(Scan0))
            Else
                output = SIMD.Add.f64_op_add_f64(v1.buffer, v2.buffer)
            End If

            Return New Vector(output)
        End Operator

        ''' <summary>
        ''' 向量减法算符重载，分量分别相减
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator -(v1 As Vector, v2 As Vector) As Vector
            'Dim N0 As Integer = v1.[Dim]    ' 获取变量维数
            'Dim v3 As New Vector(N0)

            'For j As Integer = 0 To N0 - 1
            '    v3(j) = v1(j) - v2(j)
            'Next
            'Return v3

            Return New Vector(SIMD.Subtract.f64_op_subtract_f64(v1.buffer, v2.buffer))
        End Operator

        Public Overloads Shared Operator *(x As IVector, y As Vector) As Vector
            Return New Vector(SIMD.Multiply.f64_op_multiply_f64(x.Data, y.Array))
        End Operator

        Public Overloads Shared Operator *(data As IEnumerable(Of Double), x As Vector) As Vector
            Dim N0 As Integer = x.[Dim]
            Dim v3 As New Vector(N0)
            Dim i As Integer = Scan0

            ' 0 * Inf = NaN
            ' 零乘上任意数应该都是零的?
            For Each a As Double In data
                If (a = 0R OrElse x(i) = 0R) Then
                    v3(i) = 0
                Else
                    v3(i) = a * x(i)
                End If

                i += 1
            Next

            Return v3
        End Operator

        ''' <summary>
        ''' 向量乘法算符重载，分量分别相乘，相当于MATLAB中的``.*``算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator *(v1 As Vector, v2#()) As Vector
            Dim N0 As Integer = v1.[Dim]
            Dim v3 As New Vector(N0)

            ' 0 * Inf = NaN
            ' 零乘上任意数应该都是零的?

            For j As Integer = 0 To N0 - 1
                If (v1(j) = 0R OrElse v2(j) = 0R) Then
                    v3(j) = 0
                Else
                    v3(j) = v1(j) * v2(j)
                End If
            Next

            Return v3
        End Operator

        ''' <summary>
        ''' 向量乘法算符重载，分量分别相乘，相当于MATLAB中的``.*``算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator *(v1 As Vector, v2 As Vector) As Vector
            Return New Vector(SIMD.Multiply.f64_op_multiply_f64(v1.buffer, v2.buffer))
        End Operator

        ''' <summary>
        ''' 向量除法算符重载，分量分别相除，相当于MATLAB中的``./``算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(v1 As Vector, v2 As Vector) As Vector
            'Dim N0 As Integer = v1.[Dim]         ' 获取变量维数
            'Dim v3 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v3(j) = v1(j) / v2(j)
            'Next
            'Return v3
            Return New Vector(SIMD.Divide.f64_op_divide_f64(v1.buffer, v2.buffer))
        End Operator

        ''' <summary>
        ''' 向量减加实数，各分量分别加实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(v1 As Vector, a#) As Vector
            ''向量数加算符重载
            'Dim N0 As Integer = v1.[Dim]         ' 获取变量维数
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = v1(j) + a
            'Next
            'Return v2
            Return New Vector(SIMD.Add.f64_op_add_f64_scalar(v1.buffer, a))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(v1 As Vector, a As Int16) As Vector
            Return v1 + CDbl(a)
        End Operator

        ''' <summary>
        ''' 向量减实数，各分量分别减实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(v1 As Vector, a#) As Vector
            ''向量数加算符重载
            'Dim N0 As Integer = v1.[Dim] ' 获取变量维数
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = v1(j) - a
            'Next

            'Return v2
            Return New Vector(SIMD.Subtract.f64_op_subtract_f64_scalar(v1.buffer, a))
        End Operator

        ''' <summary>
        ''' 向量 数乘，各分量分别乘以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator *(v1 As Vector, a#) As Vector
            'Dim N0 As Integer = v1.[Dim] ' 获取变量维数
            'Dim v2 As New Vector(N0)

            'For j As Integer = 0 To N0 - 1
            '    v2(j) = v1(j) * a
            'Next

            'Return v2
            Return New Vector(SIMD.Multiply.f64_scalar_op_multiply_f64(a, v1.buffer))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator *(v1 As Vector, a!) As Vector
            Return v1 * CDbl(a)
        End Operator

        ''' <summary>
        ''' 向量 数除，各分量分别除以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v1 As Vector, a#) As Vector
            'Dim N0 As Integer = v1.[Dim]         '获取变量维数
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = v1(j) / a
            'Next

            'Return v2
            Return New Vector(SIMD.Divide.f64_op_divide_f64_scalar(v1.buffer, a))
        End Operator

        Public Shared Operator /(x As Double, v As Vector) As Vector
            'Dim N0 As Integer = v.[Dim]         '获取变量维数
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = x / v(j)
            'Next

            'Return v2
            Return New Vector(SIMD.Divide.f64_scalar_op_divide_f64(x, v.buffer))
        End Operator

        ''' <summary>
        ''' 实数加向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(a As Double, v1 As Vector) As Vector
            ' 向量数加算符重载
            ' 获取变量维数
            'Dim N0 As Integer = v1.[Dim]
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = v1(j) + a
            'Next

            'Return v2
            Return New Vector(SIMD.Add.f64_op_add_f64_scalar(v1.buffer, a))
        End Operator

        ''' <summary>
        ''' 实数减向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator -(a As Double, v1 As Vector) As Vector
            ''向量数加算符重载
            'Dim N0 As Integer = v1.[Dim]         '获取变量维数
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = a - v1(j)
            'Next
            'Return v2
            Return New Vector(SIMD.Subtract.f64_scalar_op_subtract_f64(a, v1.buffer))
        End Operator

        ''' <summary>
        ''' 向量 数乘
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator *(a As Double, v1 As Vector) As Vector
            'Dim N0 As Integer = v1.[Dim]
            'Dim v2 As New Vector(N0)

            'For j = 0 To N0 - 1
            '    v2(j) = v1(j) * a
            'Next

            'Return v2
            Return New Vector(SIMD.Multiply.f64_scalar_op_multiply_f64(a, v1.buffer))
        End Operator

        ''' <summary>
        ''' 向量内积
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Or(v1 As Vector, v2 As Vector) As Double
            ''获取变量维数
            'Dim N0 = v1.[Dim]
            'Dim M0 = v2.[Dim]

            'If N0 <> M0 Then
            '    ' 如果向量维数不匹配，给出告警信息
            '    Throw New ArgumentException("Inner vector dimensions must agree！")
            'End If

            'Dim sum As Double

            'For j = 0 To N0 - 1
            '    sum = sum + v1(j) * v2(j)
            'Next
            'Return sum
            Dim dot As Double() = SIMD.Multiply.f64_op_multiply_f64(v1.buffer, v2.buffer)
            Dim sum As Double = dot.Sum

            Return sum
        End Operator

        ''' <summary>
        ''' 向量外积（相当于列向量，乘以横向量）
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Xor(v1 As Vector, v2 As Vector) As GeneralMatrix
            '获取变量维数
            Dim N0 = v1.[Dim]
            Dim M0 = v2.[Dim]

            If N0 <> M0 Then
                ' 如果向量维数不匹配，给出告警信息
                Throw New ArgumentException("Inner vector dimensions must agree！")
            End If

            Dim vvmat As New NumericMatrix(N0, N0)

            For i As Integer = 0 To N0 - 1
                For j As Integer = 0 To N0 - 1
                    vvmat(i, j) = v1(i) * v2(j)
                Next
            Next

            '返回外积矩阵
            Return vvmat
        End Operator

        ''' <summary>
        ''' 负向量 
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(v1 As Vector) As Vector
            'Dim N0 As Integer = v1.[Dim]
            'Dim v2 As New Vector(N0)

            'For i As Integer = 0 To N0 - 1
            '    v2(i) = -v1(i)
            'Next

            'Return v2
            Return New Vector(SIMD.Subtract.f64_scalar_op_subtract_f64(0, v1.buffer))
        End Operator

        ''' <summary>
        ''' <paramref name="x"/>向量之中的每一个元素是否都等于<paramref name="n"/>?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(x As Vector, n As Integer) As BooleanVector
            Return x = CDbl(n)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(x As Vector, n As Integer) As BooleanVector
            Return Not x = CDbl(n)
        End Operator

        ''' <summary>
        ''' <paramref name="x"/>向量之中的每一个元素是否都等于<paramref name="n"/>?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(x As Vector, n As Double) As BooleanVector
            ' 不可以缺少这一对括号，否则会被当作为属性d，而非值比较
            Dim asserts = From d As Double In x Select (d = n)
            Dim b As New BooleanVector(asserts)

            Return b
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(x As Vector, n As Double) As BooleanVector
            Return Not x = n
        End Operator

        ''' <summary>
        ''' Power: <see cref="std.Pow(Double, Double)"/>
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator ^(v As Vector, n As Double) As Vector
            Return New Vector(From d As Double In v Select d ^ n)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator ^(n As Double, v As Vector) As Vector
            Return New Vector(From p As Double In v Select n ^ p)
        End Operator

        Public Overloads Shared Operator ^(x As Vector, p As Vector) As Vector
            'Dim N0 = x.[Dim]
            'Dim M0 = p.[Dim]

            'If N0 <> M0 Then
            '    ' 如果向量维数不匹配，给出告警信息
            '    Throw New ArgumentException("Inner vector dimensions must agree！")
            'End If

            'Dim v2 As New Vector(N0)

            'For j As Integer = 0 To N0 - 1
            '    v2(j) = x(j) ^ p(j)
            'Next

            'Return v2
            Return New Vector(SIMD.Exponent.f64_op_exponent_f64(x.buffer, p.buffer))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator >(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d > n)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d < n)
        End Operator

        ''' <summary>
        ''' 返回一个逻辑向量，用来指示向量对象的每一个分量与目标比较的逻辑值结果
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >=(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d >= n)
        End Operator

        ''' <summary>
        ''' x &lt;= n
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <=(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d <= n)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >=(x As Vector, y As Vector) As BooleanVector
            Return New BooleanVector(From a In x.SeqIterator Select a.value >= y.buffer(a))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <=(x As Vector, y As Vector) As BooleanVector
            Return New BooleanVector(From a In x.SeqIterator Select a.value <= y.buffer(a))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <=(x#, y As Vector) As BooleanVector
            Return New BooleanVector(From a In y Select x <= a)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >=(x#, y As Vector) As BooleanVector
            Return Not x <= y
        End Operator

#Region "CType"

#Region "Syntax support for: Dim v As Vector = {1, 2, 3, 4, 5, 6}"
        Public Shared Widening Operator CType(v As Double()) As Vector
            Return New Vector(v)
        End Operator

        Public Shared Widening Operator CType(v%()) As Vector
            Return New Vector(v.Select(Function(x) CDbl(x)))
        End Operator

        Public Shared Widening Operator CType(v&()) As Vector
            Return New Vector(v.Select(Function(x) CDbl(x)))
        End Operator

        Public Shared Widening Operator CType(v!()) As Vector
            Return New Vector(v.Select(Function(x) CDbl(x)))
        End Operator
#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(v As Vector) As Double()
            Return v.ToArray
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(v As Vector) As DoubleRange
            Return New DoubleRange(v.AsEnumerable)
        End Operator

        ''' <summary>
        ''' [1,2,3,4,5,6,...]
        ''' </summary>
        ''' <param name="vector$"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(vector As String) As Vector
            Dim exp As String = Trim(vector)

            If exp.StringEmpty Then
                Return New Vector

            ElseIf exp.First = "["c AndAlso exp.Last = "]"c Then

                Dim array#() = exp _
                    .GetStackValue("[", "]") _
                    .Split(","c) _
                    .Select(AddressOf Trim) _
                    .Select(AddressOf ParseNumeric) _
                    .ToArray

                Return New Vector(array)

            Else

                Dim array#() = Expressions _
                    .TranslateIndex(exp) _
                    .Select(Function(int) CDbl(int)) _
                    .ToArray
                Return New Vector(array)

            End If
        End Operator

        Public Shared Widening Operator CType(x As Double) As Vector
            Return New Vector() From {x}
        End Operator

        Public Shared Widening Operator CType(x As Integer) As Vector
            Return New Vector() From {CDbl(x)}
        End Operator

        Public Shared Widening Operator CType(x As Long) As Vector
            Return New Vector() From {CDbl(x)}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(args As DefaultString) As Vector
            Return CType(args.DefaultValue, Vector)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(vector As VectorShadows(Of Double)) As Vector
            Return vector.As(Of Double).AsVector
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(list As List(Of Double)) As Vector
            Return New Vector(list)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(v As Vector) As List(Of Double)
            Return v.buffer.AsList
        End Operator
#End Region
#End Region

        ''' <summary>
        ''' 进行序列切片操作
        ''' </summary>
        ''' <param name="start%"></param>
        ''' <param name="stop%"></param>
        ''' <param name="steps%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function slice(Optional start% = 0, Optional stop% = -1, Optional steps% = 1) As Vector
            Return numpy.slice(buffer, start, [stop], steps).AsVector
        End Function

        ''' <summary>
        ''' dot
        ''' 
        ''' + http://mathworld.wolfram.com/DotProduct.html
        ''' + http://www.mathsisfun.com/algebra/vectors-dot-product.html
        ''' </summary>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DotProduct(v2 As Vector) As Double
            Return DotProduct(buffer, v2.buffer)
        End Function

        Public Shared Function DotProduct(lhs As Double(), rhs As Double()) As Double
            'Dim sum#

            'For i As Integer = 0 To Me.Count - 1
            '    sum += Me(i) * v2(i)
            'Next

            'Return sum
            Dim dot As Double() = SIMD.Multiply.f64_op_multiply_f64(lhs, rhs)
            Dim sum As Double = dot.Sum

            Return sum
        End Function

        Public Shared Function DotProduct(lhs As Single(), rhs As Single()) As Double
            Dim dot As Single() = SIMD.Multiply.f32_op_multiply_f32(lhs, rhs)
            Dim sum As Double = dot.Sum

            Return sum
        End Function

        ''' <summary>
        ''' 返回这个向量之中的所有的元素的乘积
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Product() As Double
            Return Me.ProductALL
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CumSum() As Vector
            Return Math.CumSum(Me)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs() As Vector
            Return Abs(Me)
        End Function

        ''' <summary>
        ''' scale elements in current vector to another value <paramref name="range"/>.
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns>
        ''' a new data <see cref="Vector"/> which its element value is 
        ''' in range of a given <paramref name="range"/>.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ScaleToRange(range As DoubleRange) As Vector
            Return Me.RangeTransform(range)
        End Function

        ''' <summary>
        ''' Display member's data as json array
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"<dims: {[Dim]}> [" & Me.ToString("G6").Take(20).JoinBy(", ") & "...]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function ToString(format$) As String()
            Return Me _
                .Select(Function(xi) xi.ToString(format)) _
                .ToArray
        End Function

        ''' <summary>
        ''' ``order()``的返回值是对应``排名``的元素所在向量中的位置。
        ''' 
        ''' ```R
        ''' x &lt;- c(97, 93, 85, 74, 32, 100, 99, 67);
        ''' 
        ''' sort(x)
        ''' # [1] 32  67  74  85  93  97  99 100
        ''' order(x)
        ''' # [1] 5 8 4 3 2 1 7 6
        ''' rank(x)
        ''' # [1] 6 5 4 3 1 8 7 2
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Order() As Vector
            Return Vector.Order(Me)
        End Function

        ''' <summary>
        ''' http://math.stackexchange.com/questions/440320/what-is-magnitude-of-sum-of-two-vector
        ''' </summary>
        ''' <param name="x1"></param>
        ''' <param name="x2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SumMagnitudes(x1 As Vector, x2 As Vector) As Double
            Return (x1 + x2).Mod
        End Function

        ''' <summary>
        ''' Returns a numeric vector with all elements is value ``1``
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Function Ones(n As Integer) As Vector
            Dim result As New Vector(n)

            For i As Integer = 0 To result.Count - 1
                result(i) = 1.0
            Next

            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Scalar(x As Double) As Vector
            Return New Vector(data:={x})
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size%"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this method can be affected by the <see cref="randf2.SetSeed(Integer)"/> method.
        ''' </remarks> 
        '''
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function rand(size%) As Vector
            Return Extensions.rand(size)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function rand(min#, max#, size%) As Vector
            Return Extensions.rand(size, {min, max})
        End Function

        Public Shared Function norm(size As Integer, Optional mu As Double = 0.0, Optional sigma As Double = 1.0) As Vector
            Dim v As Double() = New Double(size - 1) {}

            For i As Integer = 0 To v.Length - 1
                v(i) = randf2.NextGaussian(mu, sigma)
            Next

            Return New Vector(v)
        End Function
    End Class
End Namespace
