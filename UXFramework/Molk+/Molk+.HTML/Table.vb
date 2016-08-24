#Region "Microsoft.VisualBasic::99525b94a90829e6d467e7578006ea40, ..\visualbasic_App\UXFramework\Molk+\Molk+.HTML\Table.vb"

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

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Linq.Extensions

Public Class Table(Of T As Class) : Inherits Designer
    Implements System.Collections.Generic.IList(Of T)
    Implements System.Collections.Generic.IEnumerable(Of T)

#Region "Properties"

    Dim _headers As Dictionary(Of String, Title)

    ''' <summary>
    ''' 表头
    ''' </summary>
    ''' <returns></returns>
    Public Property Headers As Title()
        Get
            Return _headers.Values.ToArray
        End Get
        Protected Set()
            _headers = Value.ToDictionary(Function(obj) obj.Text)
        End Set
    End Property

    ''' <summary>
    ''' 表中的数据
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ListData As List(Of T)

    Default Public Property Item(index As Integer) As T Implements IList(Of T).Item
        Get
            Return ListData(index)
        End Get
        Set(value As T)
            ListData(index) = value
        End Set
    End Property

    Public ReadOnly Property Length As Integer Implements ICollection(Of T).Count
        Get
            Return ListData.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
        Get
            Return False
        End Get
    End Property
#End Region

    Sub New(preLoad As Generic.IEnumerable(Of T))
        Call MyBase.New
        ListData = preLoad.ToList
        Headers = Title.GetTitles(GetType(T))
        Call GroupBy(Headers(Scan0).Text)
    End Sub

    Public Overrides Function ToString() As String
        Return GetType(T).FullName
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="propName">请使用NameOf操作符来获取</param>
    Public Sub GroupBy(propName As String)
        _groupKey = _headers(propName).BindProperty
        __buildHTML()
    End Sub

    Dim _groupKey As PropertyInfo

    Protected Overrides Function __buildHTML() As String
        Dim html As New StringBuilder(My.Resources.list)
        Dim groupData As GroupKey(Of T)() = GroupKey(Of T).Groups(Me, _groupKey)

        Call html.Replace("[Headers]", __buildHeaders)
        Call html.Replace("[Rows]", __buildRows(groupData))

        Me.HTML = html.ToString
        Return Me.HTML
    End Function

    ''' <summary>
    ''' 重新生成HTML文档
    ''' </summary>
    Public Sub Refresh()
        Call __buildHTML()
        Call __refreshHandle()
    End Sub

    Private Function __buildRows(groupData As GroupKey(Of T)()) As String
        Dim idx As Integer = 0
        Dim rbr As StringBuilder = New StringBuilder

        For Each group As GroupKey(Of T) In groupData
            Call rbr.AppendLine(group.ToHTML(idx))
        Next

        Return rbr.ToString
    End Function

    Private Function __buildHeaders() As String
        Dim hbr As String = String.Join("", Headers.ToArray(Function(h) $"<th style=""text-align:center""><a href=""/groupBy?{h.Text}"">{h.Text}</a></th>"))
        Return $"<tr><th></th>{hbr}</tr>"
    End Function

#Region "List Operations"

    Public Function IndexOf(item As T) As Integer Implements IList(Of T).IndexOf
        Return ListData.IndexOf(item)
    End Function

    Public Sub Insert(index As Integer, item As T) Implements IList(Of T).Insert
        Call ListData.Insert(index, item)
        Call Refresh()
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList(Of T).RemoveAt
        Call ListData.RemoveAt(index)
        Call Refresh()
    End Sub

    Public Sub Add(item As T) Implements ICollection(Of T).Add
        Call ListData.Add(item)
        Call Refresh()
    End Sub

    Public Sub Clear() Implements ICollection(Of T).Clear
        Call ListData.Clear()
        Call Refresh()
    End Sub

    Public Function Contains(item As T) As Boolean Implements ICollection(Of T).Contains
        Return ListData.Contains(item)
    End Function

    Public Sub CopyTo(array() As T, arrayIndex As Integer) Implements ICollection(Of T).CopyTo
        Call ListData.CopyTo(array, arrayIndex)
    End Sub

    Public Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
        Dim rtvl = ListData.Remove(item)
        Call Refresh()
        Return rtvl
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        For Each Item As T In ListData
            Yield Item
        Next
    End Function
#End Region
End Class

Public Class GroupKey(Of T As Class)
    Public Property Id As String

    Public ReadOnly Property RowData As T()

    Dim _numberOfFields As Integer
    Dim _schema As PropertyInfo()

    Public Overrides Function ToString() As String
        Return $"{GetType(T).FullName} ===> {Id}"
    End Function

    ''' <summary>
    ''' 对表之中的数据进行分组操作
    ''' </summary>
    ''' <param name="Table"></param>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Public Shared Function Groups(Table As Table(Of T), key As PropertyInfo) As GroupKey(Of T)()
        Dim LQuery = (From obj As T In Table.ListData.AsParallel
                      Let keyValue As String = __getKey(Scripting.ToString(key.GetValue(obj)))
                      Select keyValue, obj
                      Group By keyChr = AscW(keyValue) Into Group).ToArray
        Dim result = (From group In LQuery
                      Select data = New GroupKey(Of T) With {
                          .Id = group.Group.First.keyValue,
                          ._RowData = group.Group.ToArray(Function(obj) obj.obj),
                          ._numberOfFields = Table.Headers.Length,
                          ._schema = Table.Headers.ToArray(Function(field) field.BindProperty)}
                      Order By data.Id Ascending).ToArray
        Return result
    End Function

    Private Shared Function __getKey(value As String) As Char
        If String.IsNullOrEmpty(value) Then
            Return " "c
        Else
            Return value.First
        End If
    End Function

    Public Function ToHTML(ByRef idx As Integer) As String
        Dim sbr As StringBuilder = New StringBuilder($"<tr>
                                                         <th scope=""row""></th>
                                                         <td id=""{Id}"">{Id}</td>
{_numberOfFields.Sequence.ToArray(Function(null) "<td></td>")}</tr>")
        Dim caches = (From obj As T In RowData.AsParallel
                      Select values = _schema.ToArray(Function(field) Scripting.ToString(field.GetValue(obj)))).ToArray

        For Each values In caches
            idx += 1
            Call sbr.AppendLine($"<tr>
          <th scope = ""row"">{idx}</th>
{String.Join("", values.ToArray(Function(value) $"<td>{value}</td>"))}</tr>")
        Next

        Return sbr.ToString
    End Function
End Class

<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
Public Class Title : Inherits Attribute

    Public ReadOnly Property Text As String
    Public ReadOnly Property BindProperty As PropertyInfo

    Sub New(title As String)
        Text = title
    End Sub

    Public Overrides Function ToString() As String
        Return Text
    End Function

    Private Function Bind(prop As PropertyInfo) As Title
        _BindProperty = prop
        Return Me
    End Function

    Public Shared Function GetTitles(typeDef As Type) As Title()
        Dim LQuery = (From prop As PropertyInfo
                      In typeDef.GetProperties(BindingFlags.Public + BindingFlags.Instance)
                      Let attrs As Object() = prop.GetCustomAttributes(GetType(Title))
                      Where Not attrs.IsNullOrEmpty
                      Let title As Title = DirectCast(attrs(Scan0), Title)
                      Select title.Bind(prop)).ToArray
        Return LQuery
    End Function
End Class
