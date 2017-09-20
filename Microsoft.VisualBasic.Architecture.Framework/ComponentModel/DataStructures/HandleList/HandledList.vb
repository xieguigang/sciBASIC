#Region "Microsoft.VisualBasic::063c77e2a70d7e8002804cbecd30a84c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\HandleList\HandledList.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">
    ''' Class object that can be dispose by the system automatically and the class object that should 
    ''' have a handle property to specific its position in this list class. 
    ''' (能够被系统所自动销毁的对象类型，并且该类型的对象必须含有一个Handle属性来指明其在本列表中的位置)
    ''' </typeparam>
    ''' <remarks></remarks>
    Public Class HandledList(Of T As IAddressOf) : Implements IEnumerable(Of T)

        ''' <summary>
        ''' Object instances data physical storage position, element may be null after 
        ''' remove a specify object handle. 
        ''' (列表中的元素对象实例的实际存储位置，当对象元素从列表之中被移除了之后，其将会被销毁)
        ''' </summary>
        ''' <remarks>
        ''' 即与只读属性'ListData'相比，这个字段的列表中可能含有空引用的元素对象.
        ''' </remarks>
        Dim list As New List(Of T)
        Dim isNothing As Assert(Of T) = Function(x) x Is Nothing

        Public ReadOnly Property Count As Integer
            Get
                Return list.Where(Function(x) Not isNothing(x)).Count
            End Get
        End Property

        Public ReadOnly Property EmptySlots As Integer()
            Get
                Return list _
                    .SeqIterator _
                    .Where(Function(x) isNothing(x.value)) _
                    .Select(Function(x) x.i) _
                    .ToArray
            End Get
        End Property

        Public Function GetAvailablePos() As Integer
            With list _
                .SeqIterator _
                .Where(Function(x) isNothing(x.value)) _
                .FirstOrDefault

                If list.Count > 0 And .i = 0 Then
                    If list(0) Is Nothing Then
                        Return 0
                    Else
                        Return list.Count
                    End If
                Else
                    Return .i
                End If
            End With
        End Function

        ''' <summary>
        ''' 与迭代器<see cref="GetEnumerator()"/>函数所不同的是，迭代器函数返回的都是非空元素，而这个读写属性则是可以直接接触到内部的
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Property Item(index%) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If index > list.Count - 1 Then
                    Return Nothing
                End If

                If index < 0 Then
                    index = list.Count + index
                End If

                Return list(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                If index < 0 Then
                    index = list.Count + index
                End If

                list(index) = value
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(source As IEnumerable(Of T))
            Call Add(source)
        End Sub

        Public Function Contains(x As T) As Boolean
            Return Not Me(x.Address) Is Nothing
        End Function

        Public Sub Clear()
            Call list.Clear()
        End Sub

        Public Sub Remove(x As T)
            list(x.Address) = Nothing
        End Sub

        Public Sub Remove(index%)
            list(index) = Nothing
        End Sub

        Public Sub Add(source As IEnumerable(Of T))
            For Each x As T In source.SafeQuery
                Call Add(x)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(x As T)
            If list.Count <= x.Address Then
                For i As Integer = 0 To x.Address - list.Count
                    list.Add(Nothing)
                Next
            End If

            list(x.Address) = x
        End Sub

        Shared Narrowing Operator CType(src As HandledList(Of T)) As T()
            Return src _
                .list _
                .Where(Function(x) Not src.isNothing(x)) _
                .ToArray
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In list.Where(Function(o) Not isNothing(o))
                Yield x
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
