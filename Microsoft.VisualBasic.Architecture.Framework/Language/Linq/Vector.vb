#Region "Microsoft.VisualBasic::ba0350705be0bc07ae8d0229ffdc1888, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Linq\Vector.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

Imports System.Dynamic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports CollectionSet = Microsoft.VisualBasic.ComponentModel.DataStructures.Set

Namespace Language

    ''' <summary>
    ''' VB.NET object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Vector(Of T) : Inherits DynamicObject
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' Array that hold the .NET object in this collection
        ''' </summary>
        Protected buffer As T()

        ''' <summary>
        ''' Gets the element counts in this vector collection
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Length
            End Get
        End Property

#Region ""
        ''' <summary>
        ''' The last elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Last As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(Length - 1)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Length - 1) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The first elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property First As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(0)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Scan0) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="args">同时支持boolean和integer</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(args As Object) As List(Of T)
            Get
                Dim index = Indexer.Indexing(args)
                Return Me(index)
            End Get
            Set
                Dim index = Indexer.Indexing(args)
                Me(index) = Value
            End Set
        End Property

        ''' <summary>
        ''' This indexer property is using for the ODEs-system computing.
        ''' (这个是为了ODEs计算模块所准备的一个数据接口)
        ''' </summary>
        ''' <param name="address"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(address As IAddress(Of Integer)) As T
            Get
                Return buffer(address.Address)
            End Get
            Set(value As T)
                buffer(address.Address) = value
            End Set
        End Property

#Region "2017-7-22 -1索引好像对向量的意义不大，而且会降低代码性能，所以在这里去除了这个索引属性"

        '''' <summary>
        '''' Can accept negative number as the index value, negative value means ``<see cref="Count"/> - n``, 
        '''' example as ``list(-1)``: means the last element in this list: ``list(list.Count -1)``
        '''' </summary>
        '''' <param name="index%"></param>
        '''' <returns></returns>
        'Default Public Overloads Property Item(index%) As T
        '    Get
        '        If index < 0 Then
        '            index = Count + index  ' -1 -> count -1
        '        End If
        '        Return buffer(index)
        '    End Get
        '    Set(value As T)
        '        If index < 0 Then
        '            index = Count + index  ' -1 -> count -1
        '        End If

        '        buffer(index) = value
        '    End Set
        'End Property

        ''' <summary>
        ''' Direct get the element in the array by its index.
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index%) As T
            Get
                Return buffer(index)
            End Get
            Set(value As T)
                buffer(index) = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Using a index vector expression to select/update many elements from this list collection.
        ''' </summary>
        ''' <param name="exp$">
        ''' + ``1``, index=1
        ''' + ``1:8``, index=1, count=8
        ''' + ``1->8``, index from 1 to 8
        ''' + ``8->1``, index from 8 to 1
        ''' + ``1,2,3,4``, index=1 or  2 or 3 or 4
        ''' </param>
        ''' <returns></returns>
        Default Public Overloads Property Item(exp$) As List(Of T)
            Get
                Dim list As New List(Of T)

                For Each i% In exp.TranslateIndex
                    list += buffer(i)
                Next

                Return list
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In exp.TranslateIndex.SeqIterator
                    buffer(+i) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Get subset of the collection by using a continues index
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(range As IntRange) As List(Of T)
            Get
                Return New List(Of T)(Me.Skip(range.Min).Take(range.Length))
            End Get
            Set(value As List(Of T))
                Dim indices As Integer() = range.ToArray

                For i As Integer = 0 To indices.Length - 1
                    buffer(indices(i)) = value(i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Gets subset of the collection by using a discontinues index
        ''' </summary>
        ''' <param name="indices"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(indices As IEnumerable(Of Integer)) As List(Of T)
            Get
                Return New List(Of T)(indices.Select(Function(i) buffer(i)))
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In indices.SeqIterator
                    buffer(+i) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Select all of the elements from this list collection is any of them match the condition expression: <paramref name="where"/>
        ''' </summary>
        ''' <param name="[where]"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item([where] As Predicate(Of T)) As T()
            Get
                Return buffer.Where(Function(o) where(o)).ToArray
            End Get
        End Property

        ''' <summary>
        ''' Select elements by logical condiction result.
        ''' </summary>
        ''' <param name="booleans"></param>
        ''' <returns></returns>
        Default Public Overridable Overloads Property Item(booleans As IEnumerable(Of Boolean)) As Vector(Of T)
            Get
                Return New Vector(Of T)(Me(indices:=Which.IsTrue(booleans)))
            End Get
            Set(value As Vector(Of T))
                For Each i In booleans.SeqIterator
                    If i.value Then
                        buffer(i) = value(i)
                    End If
                Next
            End Set
        End Property
#End Region

#Region "Constructor"
        Public Sub New()
        End Sub

        Sub New(capacity%)
            buffer = New T(capacity - 1) {}
        End Sub

        Sub New(data As IEnumerable(Of T))
            buffer = data.ToArray
        End Sub
#End Region

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x In buffer
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' 没用？？？
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(v As Vector(Of T)) As T()
            Return v.ToArray
        End Operator

        ''' <summary>
        ''' Union two collection directly without <see cref="Enumerable.Distinct"/> operation.
        ''' (请注意，使用<see cref="CollectionSet"/>集合对象的Union功能会去除重复，而这个操作符则是直接进行合并取``并集``而不去重)
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator &(a As Vector(Of T), b As Vector(Of T)) As Vector(Of T)
            Return New Vector(Of T)(a.buffer.AsList + b.buffer)
        End Operator
    End Class
End Namespace
