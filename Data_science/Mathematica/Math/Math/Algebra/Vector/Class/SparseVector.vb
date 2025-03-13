#Region "Microsoft.VisualBasic::17f4ead36d2c9a1193769353dde1d244, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\SparseVector.vb"

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

    '   Total Lines: 305
    '    Code Lines: 188 (61.64%)
    ' Comment Lines: 75 (24.59%)
    '    - Xml Docs: 92.00%
    ' 
    '   Blank Lines: 42 (13.77%)
    '     File Size: 11.45 KB


    '     Class SparseVector
    ' 
    '         Properties: [Dim], Array, Data, Length, Precision
    '                     Values
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: Equals, GetEnumerator, Max, Min, Sum
    ' 
    '         Sub: SetValue
    ' 
    '         Operators: -, (+2 Overloads) *, /, (+2 Overloads) ^, (+2 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports std = System.Math

Namespace LinearAlgebra

    ''' <summary>
    ''' 稀疏的向量
    ''' </summary>
    ''' <remarks>
    ''' 在这个向量中存在大量的零，主要适用于节省计算内存
    ''' 因为有<see cref="buffer"/>索引的存在，所以假若零数值比较少的话，
    ''' 使用这个稀疏向量来存储数据反而会导致内存被过度占用
    ''' </remarks>
    ''' 
    <FrameworkConfig(SparseVector.PrecisionEnvironmentConfigName)>
    Public Class SparseVector : Inherits Vector

        ''' <summary>
        ''' 非零值的索引号和对应的值
        ''' </summary>
        Shadows ReadOnly buffer As Dictionary(Of Integer, Double)

        ''' <summary>
        ''' 这个向量的虚拟长度
        ''' </summary>
        Dim dimension As Integer

        ''' <summary>
        ''' 返回这个向量的虚拟长度
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 请注意，由于buffer对象记录的是非零元素的数据记录集合，所以其元素计数并不是当前
        ''' 的这个向量对象的真实长度，<see cref="dimension"/>的值是真实的长度
        ''' </remarks>
        Public Overrides ReadOnly Property Length As Integer
            Get
                Return dimension
            End Get
        End Property

        ''' <summary>
        ''' 返回这个向量的虚拟长度
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property [Dim] As Integer
            Get
                Return dimension
            End Get
        End Property

        ''' <summary>
        ''' 返回所有的非零元素
        ''' </summary>
        ''' <returns></returns>
        Protected ReadOnly Property Values As IEnumerable(Of Double)
            Get
                Return buffer.Values.AsEnumerable
            End Get
        End Property

        ''' <summary>
        ''' returns all data values in current vector object.
        ''' </summary>
        ''' <returns></returns>
        Protected Overrides Property Data As Double()
            Get
                Return Me.ToArray
            End Get
            Set(value As Double())
                Throw New Exception("read only!")
            End Set
        End Property

        Public Overrides ReadOnly Property Array As Double()
            Get
                Return Me.ToArray
            End Get
        End Property

#Region "Index properties"

        Default Public Overrides Property Item(booleans As IEnumerable(Of Boolean)) As Vector(Of Double)
            Get
                Return New Vector(Of Double)(Linq.which.IsTrue(booleans).Select(Function(index) Me(index)))
            End Get
            Set(value As Vector(Of Double))
                For Each index As SeqValue(Of Integer) In Linq.which.IsTrue(booleans).SeqIterator
                    Call SetValue(index.value, value(index))
                Next
            End Set
        End Property

        ''' <summary>
        ''' Get/Set value by index access
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overrides Property Item(index As Integer) As Double
            Get
                If buffer.ContainsKey(index) Then
                    Return buffer.Item(key:=index)
                Else
                    Return 0
                End If
            End Get
            Set
                Call SetValue(index, Value)
            End Set
        End Property

        Default Public Overloads Property Item(range As IEnumerable(Of Integer)) As Vector
            Get
                Return New SparseVector(range.Select(Function(index) Me(index)))
            End Get
            Set
                For Each index As SeqValue(Of Integer) In range.SeqIterator
                    Call SetValue(index.value, Value(index))
                Next
            End Set
        End Property

        Default Public Overloads Property Item(range As IntRange) As SparseVector
            Get
                Return Me(range.ToArray)
            End Get
            Set
                Me(range.ToArray) = Value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' All of the element its ABS value less than this precision threshold will be treated as ZERO value
        ''' So the larger of this threshold value, the lower precision precision of all of the math algorithm
        ''' that related to this <see cref="SparseVector"/>.
        ''' 
        ''' (当元素的绝对值小于这个值之后就会被当作为零，可以根据情况来设置这个公用属性来控制稀疏向量的计算精度)
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property Precision As Double = 0.00001

        Public Const PrecisionEnvironmentConfigName$ = "sparse_vector.zero_precision"

        Shared Sub New()
            Dim precision$ = App.GetVariable(PrecisionEnvironmentConfigName)

            If Not precision.StringEmpty Then
                SparseVector.Precision = precision.ParseDouble

                Call $"The precision controls config of the sparse vector was set to {precision}".__INFO_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">非零元素值列表</param>
        ''' <param name="index">非零元素对应的索引编号</param>
        ''' <param name="length">
        ''' 因为存在大量零，数组中并不是存储真实的数据，而是非零值，所以在这里必须要有一个长度来标记出当前的这个向量的长度
        ''' </param>
        Private Sub New(data As IEnumerable(Of Double), index As IEnumerable(Of Integer), length%)
            Call MyBase.New(data)

            buffer = New Dictionary(Of Integer, Double)

            For Each i As SeqValue(Of Integer) In index.SeqIterator
                buffer.Add(i.value, MyBase.buffer(i))
            Next

            dimension = length
        End Sub

        ''' <summary>
        ''' create a new sparse compact numeric vector from the given raw data
        ''' </summary>
        ''' <param name="data">
        ''' the given raw data
        ''' </param>
        Sub New(data As IEnumerable(Of Double))
            Call MyBase.New(New Double() {})

            Dim dimension As Integer = 0
            Dim buffer As New Dictionary(Of Integer, Double)

            For Each x As Double In data
                If std.Abs(x) < Precision Then
                    ' 0.0
                Else
                    ' has a non-ZERO value at current index
                    buffer.Add(dimension, x)
                End If

                dimension += 1
            Next

            Me.buffer = buffer
            Me.dimension = dimension
        End Sub

        Public Sub SetValue(index%, value#)
            If value = 0.0 OrElse std.Abs(value) < Precision Then
                buffer.Remove(key:=index)
            Else
                buffer.Remove(key:=index)
                buffer.Add(index, value)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Min() As Double
            Return std.Min(0.0, Values.Min)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Max() As Double
            Return std.Max(0.0, Values.Max)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 忽略掉所有的零值
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Sum() As Double
            Return Values.Sum
        End Function

        Public Overloads Shared Function Equals(a#, b#) As Boolean
            Return std.Abs(a - b) <= Precision
        End Function

        ''' <summary>
        ''' 这个枚举器函数会枚举出所有的元素，包括非零元素以及值等于零的元素
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Iterator Function GetEnumerator() As IEnumerator(Of Double)
            For i As Integer = 0 To dimension - 1
                If buffer.ContainsKey(i) Then
                    Yield buffer(key:=i)
                Else
                    Yield 0.0
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator *(v As SparseVector, multipl As Double) As SparseVector
            Return New SparseVector(From x As Double In v Select x * multipl)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator *(v As SparseVector, multipl As Vector) As SparseVector
            If v.Dim <> multipl.Dim Then
                Throw New InvalidConstraintException
            Else
                Return New SparseVector(From i As Integer In v.Sequence Select v(i) ^ multipl(i))
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator /(v As SparseVector, div As Double) As SparseVector
            Return New SparseVector(From x As Double In v Select x / div)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(v As SparseVector, minus As Double) As SparseVector
            Return New SparseVector(From x As Double In v Select x - minus)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(v As SparseVector, add As Double) As SparseVector
            Return New SparseVector(From x As Double In v Select x + add)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator +(add As Double, v As SparseVector) As SparseVector
            Return v + add
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator ^(v As SparseVector, p As Double) As SparseVector
            Return New SparseVector(From x As Double In v Select x ^ p)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator ^(x As SparseVector, y As Vector) As SparseVector
            If x.Dim <> y.Dim Then
                Throw New InvalidConstraintException()
            Else
                Return New SparseVector(From i As Integer In x.Sequence Select x(i) ^ y(i))
            End If
        End Operator
    End Class
End Namespace
