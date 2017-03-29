#Region "Microsoft.VisualBasic::c3c36d4f6ecc503bb5c027a1977c3134, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\NamedCollection(Of T).vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The value object collection that have a name string.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedCollection(Of T) : Implements INamedValue
        Implements IKeyValuePairObject(Of String, T())
        Implements Value(Of T()).IValueOf
        Implements IEnumerable(Of T)
        Implements IGrouping(Of String, T)
        Implements IList(Of T)

        ''' <summary>
        ''' 这个集合对象的标识符名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Name As String Implements _
            IKeyedEntity(Of String).Key,
            IKeyValuePairObject(Of String, T()).Key,
            IGrouping(Of String, T).Key

        Dim __list As List(Of T)

        ''' <summary>
        ''' 目标集合对象
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As T() Implements IKeyValuePairObject(Of String, T()).Value, Value(Of T()).IValueOf.value
            Get
                Return __list.ToArray
            End Get
            Set(value As T())
                __list = New List(Of T)(value)
            End Set
        End Property

        ''' <summary>
        ''' 目标集合对象的描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Property Description As String

        ''' <summary>
        ''' 当前的这个命名的目标集合对象是否是空对象？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Name Is Nothing AndAlso Value Is Nothing
            End Get
        End Property

        Default Public Property Item(index As Integer) As T Implements IList(Of T).Item
            Get
                Return __list(index)
            End Get
            Set(value As T)
                __list(index) = value
            End Set
        End Property

        Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
            Get
                Return __list.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">名称属性<see cref="NamedValue(Of T).Name"/></param>必须是相同的
        Sub New(source As IEnumerable(Of NamedValue(Of T)))
            Dim array = source.ToArray

            Name = array(Scan0).Name
            Value = array.Select(Function(x) x.Value).ToArray
            Description = array _
                .Select(Function(x) x.Description) _
                .Where(Function(s) Not s.StringEmpty) _
                .Distinct _
                .JoinBy("; ")
        End Sub

        Sub New(name$, data As IEnumerable(Of T), Optional description$ = Nothing)
            Me.Name = name
            Me.Description = description
            Me.Value = data.ToArray
        End Sub

        Public Function GetValues() As NamedValue(Of T)()
            Dim name$ = Me.Name
            Dim describ$ = Description

            Return Value.ToArray(
                Function(v) New NamedValue(Of T) With {
                    .Name = name,
                    .Description = describ,
                    .Value = v
                })
        End Function

        Public Overrides Function ToString() As String
            Return Name & " --> " & Value.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In __list.SafeQuery
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Private Function IndexOf(item As T) As Integer Implements IList(Of T).IndexOf
            Return __list.IndexOf(item)
        End Function

        Private Sub Insert(index As Integer, item As T) Implements IList(Of T).Insert
            Call __list.Insert(index, item)
        End Sub

        Private Sub RemoveAt(index As Integer) Implements IList(Of T).RemoveAt
            Call __list.RemoveAt(index)
        End Sub

        Private Sub Add(item As T) Implements ICollection(Of T).Add
            Call __list.Add(item)
        End Sub

        Private Sub Clear() Implements ICollection(Of T).Clear
            Call __list.Clear()
        End Sub

        Private Function Contains(item As T) As Boolean Implements ICollection(Of T).Contains
            Return __list.Contains(item)
        End Function

        Private Sub CopyTo(array() As T, arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            Call __list.CopyTo(array, arrayIndex)
        End Sub

        Private Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
            Return __list.Remove(item)
        End Function
    End Structure
End Namespace
