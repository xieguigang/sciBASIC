#Region "Microsoft.VisualBasic::2a464be949d7e1383005582755b78afc, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\List(Of T).vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace Language

    ''' <summary>
    ''' Represents a strongly typed list of objects that can be accessed by index. Provides
    ''' methods to search, sort, and manipulate lists.To browse the .NET Framework source
    ''' code for this type, see the Reference Source.
    ''' (加强版的<see cref="System.Collections.Generic.List(Of T)"/>)
    ''' </summary>
    ''' <typeparam name="T">The type of elements in the list.</typeparam>
    Public Class List(Of T) : Inherits Generic.List(Of T)
        ' Implements IEnumerable(Of Value(Of T))

        Dim __index As Pointer

#Region "Improvements Index"

        Default Public Overloads Property Item(index%) As T
            Get
                If index < 0 Then
                    index = Count + index  ' -1 -> count -1
                End If
                Return MyBase.Item(index)
            End Get
            Set(value As T)
                If index < 0 Then
                    index = Count + index  ' -1 -> count -1
                End If
                MyBase.Item(index) = value
            End Set
        End Property

        ''' <summary>
        ''' 
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
                    list += Item(index:=i)
                Next

                Return list
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In exp.TranslateIndex.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(range As IntRange) As List(Of T)
            Get
                Return New List(Of T)(Me.Skip(range.Min).Take(range.Length))
            End Get
            Set(value As List(Of T))
                Dim indices As Integer() = range.ToArray

                For i As Integer = 0 To indices.Length - 1
                    Item(index:=indices(i)) = value(i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(indices As IEnumerable(Of Integer)) As List(Of T)
            Get
                Return New List(Of T)(indices.Select(Function(i) Item(index:=i)))
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In indices.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        Default Public Overloads ReadOnly Property Item([where] As Predicate(Of T)) As T()
            Get
                Return MyBase.Where(Function(o) where(o)).ToArray
            End Get
        End Property
#End Region

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List"/>`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' (这是一个安全的构造函数，假若输入的参数为空值，则只会创建一个空的列表，而不会抛出错误)
        ''' </summary>
        ''' <param name="source">The collection whose elements are copied to the new list.</param>
        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(If(source Is Nothing, {}, source.ToArray))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the List`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="x">The collection whose elements are copied to the new list.</param>
        Sub New(ParamArray x As T())
            Call MyBase.New(x)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the List`1 class that
        ''' is empty and has the default initial capacity.
        ''' </summary>
        Public Sub New()
            Call MyBase.New
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the List`1 class that
        ''' is empty and has the specified initial capacity.
        ''' </summary>
        ''' <param name="capacity">The number of elements that the new list can initially store.</param>
        Public Sub New(capacity As Integer)
            Call MyBase.New(capacity)
        End Sub

        Public Sub New(size As Integer, fill As T)
            For i As Integer = 0 To size - 1
                Call Add(fill)
            Next
        End Sub

        ''' <summary>
        ''' Pop all of the elements value in to array from the list object and then clear all of the list data.
        ''' </summary>
        ''' <returns></returns>
        Public Function PopAll() As T()
            Dim array As T() = ToArray()
            Call Clear()
            Return array
        End Function

        ''' <summary>
        ''' Move Next
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(list As List(Of T)) As T
            Return list(+list.__index)
        End Operator

        ''' <summary>
        ''' Adds an object to the end of the List`1.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">The object to be added to the end of the List`1. The
        ''' value can be null for reference types.</param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), x As T) As List(Of T)
            If list Is Nothing Then
                Return New List(Of T) From {x}
            Else
                Call list.Add(x)
                Return list
            End If
        End Operator

        ''' <summary>
        ''' Adds an object to the end of the List`1.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">The object to be added to the end of the List`1. The
        ''' value can be null for reference types.</param>
        ''' <returns></returns>
        Public Shared Operator +(x As T, list As List(Of T)) As List(Of T)
            If list Is Nothing Then
                Return New List(Of T) From {x}
            Else
                Call list.Insert(Scan0, x)
                Return list
            End If
        End Operator

        Public Shared Operator *(list As List(Of T), n%) As List(Of T)
            Select Case n
                Case <= 0
                    Return New List(Of T)
                Case 1
                    Return New List(Of T)(list)
                Case > 1
                    Dim out As New List(Of T)

                    For i As Integer = 1 To n
                        out.AddRange(list)
                    Next

                    Return out
                Case Else
                    Throw New NotImplementedException
            End Select
        End Operator

        ''' <summary>
        ''' Removes the first occurrence of a specific object from the List`1.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">The object to remove from the List`1. The value can
        ''' be null for reference types.</param>
        ''' <returns></returns>
        Public Shared Operator -(list As List(Of T), x As T) As List(Of T)
            Call list.Remove(x)
            Return list
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the System.Collections.Generic.List`1.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="vals"></param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), vals As IEnumerable(Of T)) As List(Of T)
            If vals Is Nothing Then
                Return list
            End If
            Call list.AddRange(vals.ToArray)
            Return list
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the System.Collections.Generic.List`1.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="vals"></param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), vals As IEnumerable(Of IEnumerable(Of T))) As List(Of T)
            If vals Is Nothing Then
                Return list
            End If
            Call list.AddRange(vals.IteratesALL)
            Return list
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the System.Collections.Generic.List`1.
        ''' </summary>
        ''' <param name="vals"></param>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Operator +(vals As IEnumerable(Of T), list As List(Of T)) As List(Of T)
            Dim all As List(Of T) = vals.ToList
            Call all.AddRange(list)
            Return all
        End Operator

        ' 请注意，由于下面的代码是和Csv文件操作模块有冲突的，所以代码在这里被注释掉了
        'Public Shared Operator +(vals As IEnumerable(Of IEnumerable(Of T)), list As List(Of T)) As List(Of T)
        '    Call list.AddRange(vals.MatrixAsIterator)
        '    Return list
        'End Operator

        Public Shared Operator -(list As List(Of T), vals As IEnumerable(Of T)) As List(Of T)
            If Not vals Is Nothing Then
                For Each x As T In vals
                    Call list.Remove(x)
                Next
            End If
            Return list
        End Operator

        Public Overloads Shared Operator -(list As List(Of T), all As Func(Of T, Boolean)) As List(Of T)
            Call list.RemoveAll(Function(x) all(x))
            Return list
        End Operator

        ''' <summary>
        ''' <see cref="List(Of T).RemoveAt(Integer)"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Shared Operator -(list As List(Of T), index As Integer) As List(Of T)
            Call list.RemoveAt(index)
            Return list
        End Operator

        ''' <summary>
        ''' 将这个列表对象隐式转换为向量数组
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(list As List(Of T)) As T()
            If list Is Nothing Then
                Return {}
            Else
                Return list.ToArray
            End If
        End Operator

        ' 因为这个隐式会使得数组被默认转换为本List对象，会导致 + 运算符重载失败，所以在这里将这个隐式转换取消掉
        'Public Shared Widening Operator CType(array As T()) As List(Of T)
        '    Return New List(Of T)(array)
        'End Operator

        ''' <summary>
        ''' Find a item in the list
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="find"></param>
        ''' <returns></returns>
        Public Shared Operator ^(list As List(Of T), find As Func(Of T, Boolean)) As T
            Dim LQuery = LinqAPI.DefaultFirst(Of T) <=
 _
                From x As T
                In list.AsParallel
                Where True = find(x)
                Select x

            Return LQuery
        End Operator

        ''' <summary>
        ''' Dump this collection data to the file system.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Shared Operator >(source As List(Of T), path As String) As Boolean
            Return CollectionIO.DefaultHandle()(source, path, System.Text.Encoding.UTF8)
        End Operator

        Public Shared Operator >>(source As List(Of T), path As Integer) As Boolean
            Dim file As FileHandle = __getHandle(path)
            Return source > file.FileName
        End Operator

        Public Shared Operator <(source As List(Of T), path As String) As Boolean
            Throw New NotImplementedException
        End Operator

        Public Iterator Function ValuesEnumerator() As IEnumerable(Of Value(Of T))
            Dim o As New Value(Of T)

            For Each x As T In Me
                o.value = x
                Yield o
            Next
        End Function
    End Class
End Namespace
