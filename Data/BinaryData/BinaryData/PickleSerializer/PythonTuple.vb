#Region "Microsoft.VisualBasic::a2f935d07df15abbb2f5387513001836, Data\BinaryData\BinaryData\PickleSerializer\PythonTuple.vb"

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

    '   Total Lines: 74
    '    Code Lines: 52 (70.27%)
    ' Comment Lines: 8 (10.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (18.92%)
    '     File Size: 2.76 KB


    '     Class PythonTuple
    ' 
    '         Properties: Items, Length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Equals, GetEnumerator, GetEnumerator1, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Pickle


    ''' <summary>
    ''' 表示 Python 的 tuple 类型。.NET 没有原生的不可变元组类型，
    ''' 因此使用此包装类来保持 Python 语义上的不可变性和有序性。
    ''' 元组在 Python 中常用于函数返回多值、字典键等场景。
    ''' </summary>
    Public Class PythonTuple
        Implements IEnumerable(Of Object)
        Implements IEquatable(Of PythonTuple)

        Private ReadOnly _items As Object()

        Public Sub New(items As Object())
            _items = If(items, Array.Empty(Of Object)())
        End Sub

        ''' <summary>按索引访问元组元素</summary>
        Default Public ReadOnly Property Item(index As Integer) As Object
            Get
                Return _items(index)
            End Get
        End Property

        ''' <summary>元组元素数量</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return _items.Length
            End Get
        End Property

        ''' <summary>获取元素数组的副本（防止外部修改）</summary>
        Public ReadOnly Property Items As Object()
            Get
                Return DirectCast(_items.Clone(), Object())
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of Object) Implements IEnumerable(Of Object).GetEnumerator
            Return DirectCast(_items, IEnumerable(Of Object)).GetEnumerator()
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Overrides Function ToString() As String
            Return "(" & String.Join(", ", _items.Select(Function(o) If(o Is Nothing, "None", o.ToString()))) & ")"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Return Equals(TryCast(obj, PythonTuple))
        End Function

        Public Overloads Function Equals(other As PythonTuple) As Boolean Implements IEquatable(Of PythonTuple).Equals
            If other Is Nothing Then Return False
            If _items.Length <> other._items.Length Then Return False
            For i = 0 To _items.Length - 1
                If Not Object.Equals(_items(i), other._items(i)) Then Return False
            Next
            Return True
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim hash = 17
            For Each item As Object In _items
                hash = hash * 31 + If(item?.GetHashCode(), 0)
            Next
            Return hash
        End Function
    End Class

End Namespace
