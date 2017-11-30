#Region "Microsoft.VisualBasic::b3096d44d16ecc12ddbecfa4f4020f2f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\IndexOf.vb"

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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Mappings of ``key As String -> index As Integer``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Index(Of T) : Implements IEnumerable(Of SeqValue(Of T))

        Dim maps As New Dictionary(Of T, Integer)
        Dim index As HashList(Of SeqValue(Of T))

        ''' <summary>
        ''' 获取包含在<see cref="System.Collections.Generic.Dictionary"/>中的键/值对的数目。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return maps.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets the input object keys that using for the construction of this index.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Objects As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return maps.Keys.ToArray
            End Get
        End Property

        ''' <summary>
        ''' 请注意，这里的数据源请尽量使用Distinct的，否则对于重复的数据，只会记录下第一个位置
        ''' </summary>
        ''' <param name="source"></param>
        Sub New(source As IEnumerable(Of T))
            If source Is Nothing Then
                source = {}
            End If

            For Each x As SeqValue(Of T) In source.SeqIterator
                If Not maps.ContainsKey(x) Then
                    Call maps.Add(+x, x.i)
                End If
            Next

            index = maps.Select(
                Function(s) New SeqValue(Of T) With {
                    .i = s.Value,
                    .value = s.Key
                }).AsHashList
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ParamArray vector As T())
            Call Me.New(source:=DirectCast(vector, IEnumerable(Of T)))
        End Sub

        ''' <summary>
        ''' 获取目标对象在本索引之中的位置编号，不存在则返回-1
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property IndexOf(x As T) As Integer
            Get
                If maps.ContainsKey(x) Then
                    Return maps(x)
                Else
                    Return -1
                End If
            End Get
        End Property

        ''' <summary>
        ''' 直接通过索引获取目标对象的值，请注意，如果<typeparamref name="T"/>泛型类型是<see cref="Integer"/>，
        ''' 则如果需要查找index的话，则必须要显式的指定参数名为``x:=``，否则调用的是当前的这个索引方法，
        ''' 得到错误的结果
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property IndexOf(index%) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.index(index).value
            End Get
        End Property

        ''' <summary>
        ''' For Linq ``where``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NotExists(x As T) As Boolean
            Return IndexOf(x) = -1
        End Function

        ''' <summary>
        ''' 这个函数是线程不安全的
        ''' </summary>
        ''' <param name="x"></param>
        Public Sub Add(x As T)
            If Not maps.ContainsKey(x) Then
                Call maps.Add(x, maps.Count)
                Call index.Add(
                    New SeqValue(Of T) With {
                        .i = maps.Count,
                        .value = x
                    })
            End If
        End Sub

        Public Sub Clear()
            Call maps.Clear()
            Call index.Clear()
        End Sub

        ''' <summary>
        ''' Display the input source sequence.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return maps.Keys _
                .Select(Function(x) x.ToString) _
                .ToArray _
                .GetJson
        End Function

        Public ReadOnly Property Map As Dictionary(Of T, Integer)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(index As Index(Of T)) As Dictionary(Of T, Integer)
            Return New Dictionary(Of T, Integer)(index.maps)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(objs As T()) As Index(Of T)
            Return New Index(Of T)(source:=objs)
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of SeqValue(Of T)) Implements IEnumerable(Of SeqValue(Of T)).GetEnumerator
            For Each o As SeqValue(Of T) In index
                Yield o
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
