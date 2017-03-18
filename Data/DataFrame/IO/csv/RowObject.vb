#Region "Microsoft.VisualBasic::856348c6fc0dfa7791e535f48030b096, ..\sciBASIC#\Data\DataFrame\IO\RowObject.vb"

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

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language

Namespace IO

    ''' <summary>
    ''' A line of data in the csv file.(Csv表格文件之中的一行)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RowObject : Implements IEnumerable(Of String)
        Implements IList(Of String)

        ''' <summary>
        ''' 本行对象中的所有的单元格的数据集合
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend _innerColumns As New List(Of String)

        Sub New(Optional Columns As IEnumerable(Of String) = Nothing)
            If Not Columns Is Nothing Then
                Me._innerColumns = Columns.ToList
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw">using <see cref="Scripting.Tostring"/> to converts the objects into a string array.</param>
        Sub New(raw As IEnumerable(Of Object))
            Call Me.New(raw.ToArray(Function(x) Scripting.ToString(x)))
        End Sub

        ''' <summary>
        ''' 这个构造函数会使用<see cref="Tokenizer.CharsParser"/>解析所输入的字符串为列数据的集合
        ''' </summary>
        ''' <param name="raw">A raw string line which read from the Csv text file.</param>
        Sub New(raw As String)
            Try
                _innerColumns = Tokenizer.CharsParser(raw)
            Catch ex As Exception
                ex = New Exception(raw)
                Throw ex
            End Try
        End Sub

        ''' <summary>
        ''' Unsafety method, <see cref="Column"/> method is safely.
        ''' (不做任何处理直接获取数据)
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public ReadOnly Property DirectGet(index As Integer) As String
            Get
                Return _innerColumns(index)
            End Get
        End Property

        ''' <summary>
        ''' Get the cell data in a specific column number. if the column is not exists in this row then will return a empty string.
        ''' (获取某一列中的数据，若该列不存在则返回空字符串)
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public Property Column(Index As Integer) As String Implements IList(Of String).Item
            Get
                If Index < 0 Then
                    Return ""
                End If

                If Index < _innerColumns.Count Then
                    Return _innerColumns(Index)
                Else
                    Return ""
                End If
            End Get
            Set(value As String)
                If Index < _innerColumns.Count Then
                    _innerColumns(Index) = value
                Else
                    Dim d As Integer = Index - _innerColumns.Count  '当前行的数目少于指定的索引号的时候，进行填充
                    For i As Integer = 0 To d - 1
                        _innerColumns.Add("")
                    Next
                    Call _innerColumns.Add(value)
                End If
            End Set
        End Property

        ''' <summary>
        ''' 非空白单元格的数目
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Width As Integer
            Get
                Dim LQuery As Integer() =
                    LinqAPI.Exec(Of Integer) <= From c As String
                                                In _innerColumns
                                                Where String.IsNullOrEmpty(c)
                                                Select 1 '
                Return _innerColumns.Count - LQuery.Length
            End Get
        End Property

        ''' <summary>
        ''' 返回本行中的非空白数据
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NotNullColumns As String()
            Get
                Dim LQuery As String() =
                    LinqAPI.Exec(Of String) <= From s As String
                                               In _innerColumns
                                               Where Not String.IsNullOrEmpty(s)
                                               Select s '
                Return LQuery
            End Get
        End Property

        ''' <summary>
        ''' is this row object contains any data?
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsNullOrEmpty As Boolean
            Get
                If _innerColumns.Count = 0 Then Return True
                Dim LQuery As Integer =
                    LinqAPI.DefaultFirst(Of Integer) <= From colum As String
                                                        In _innerColumns
                                                        Where Len(Strings.Trim(colum)) > 0
                                                        Select 100 '
                Return Not LQuery > 50
            End Get
        End Property

        ''' <summary>
        ''' insert the data into a spercific column  
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="column"></param>
        ''' <returns>仅为LINQ查询使用的一个无意义的值</returns>
        ''' <remarks></remarks>
        Public Function InsertAt(value As String, column As Integer) As Integer
            Dim d As Integer = column - _innerColumns.Count - 1
            If d > 0 Then
                For i As Integer = 0 To d
                    Call _innerColumns.Add("")
                Next
            End If
            Call _innerColumns.Insert(column, value)
            Return 0
        End Function

        ''' <summary>
        ''' Displaying in IDE
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Join(", ", _innerColumns.ToArray(Of String)(Function(col) $"[{col}]"))
        End Function

        ''' <summary>
        ''' Takes the data in the specific number of columns, if columns is not exists in this row object, then a part of returned data will be the empty string. 
        ''' </summary>
        ''' <param name="Count"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(Count As Integer) As String()
            Dim d As Integer = Count - _innerColumns.Count

            If d < 0 Then
                Return _innerColumns.Take(Count).ToArray
            Else
                Dim List As List(Of String) = New List(Of String)
                List.AddRange(_innerColumns)
                For i As Integer = 0 To d
                    List.Add("")
                Next

                Return List.ToArray
            End If
        End Function

        ''' <summary>
        ''' Takes the data in the specific column index collection, if the column is not exists in the row object, then a part of the returned data will be the empty string.
        ''' </summary>
        ''' <param name="Cols"></param>
        ''' <param name="retNullable">(当不存在数据的时候是否返回空字符串，默认返回空字符串)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(Cols As Integer(), Optional retNullable As Boolean = True) As String()
            Dim Items As String() = New String(Cols.Count - 1) {}
            For i As Integer = 0 To Cols.Count - 1
                If retNullable Then
                    Items(i) = Me.Column(Cols(i))
                Else
                    Dim Null As Boolean = GetColumn(Cols(i), Items(i))
                    If Null Then Return Nothing
                End If
            Next
            Return Items
        End Function

        ''' <summary>
        ''' 返回一个指示：是否为空？
        ''' </summary>
        ''' <param name="Idx"></param>
        ''' <param name="retStr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetColumn(Idx As Integer, ByRef retStr As String) As Boolean
            If Idx > Me._innerColumns.Count - 1 Then
                retStr = Nothing
                Return True
            Else
                retStr = _innerColumns(Idx)
                Return False
            End If
        End Function

        Public Function AddRange(values As IEnumerable(Of String)) As Integer
            Call _innerColumns.AddRange(values)
            Return _innerColumns.Count
        End Function

        ''' <summary>
        ''' 查询某一个关键词在本行中的哪一个单元格，返回-1表示没有查询到本关键词
        ''' </summary>
        ''' <param name="KeyWord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LocateKeyWord(KeyWord As String, Optional CaseSensitive As Boolean = True) As Integer
            Dim cpMethod As CompareMethod = If(
                CaseSensitive,
                CompareMethod.Binary,
                CompareMethod.Text)
            Dim LQuery As String = LinqAPI.DefaultFirst(Of String) <=
 _
                From str As String
                In _innerColumns.AsParallel
                Where InStr(str, KeyWord, cpMethod) > 0
                Select str

            If Not String.IsNullOrEmpty(LQuery) Then
                Return _innerColumns.IndexOf(LQuery)
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' Generate a line of the string data in the csv document.(将当前的行对象转换为文件中的一行字符串)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AsLine(Optional delimiter$ = ",") As String
            Get
                Dim array$() = _innerColumns.ToArray(AddressOf __mask)
                Dim line As String = String.Join(delimiter, array)
                Return line
            End Get
        End Property

        Private Shared Function __mask(s As String) As String
            If String.IsNullOrEmpty(s) Then
                Return ""
            Else
                s = s.Replace("""", """""")
            End If

            If s.IndexOf(","c) > -1 Then
                ' If s.IndexOf(" "c) > -1 OrElse s.IndexOf(","c) > -1 Then
                Return $"""{s}"""
            Else
                Return s
            End If
        End Function

        Public Sub Trim(lefts%)
            _innerColumns = New List(Of String)(_innerColumns.Take(lefts))
        End Sub

        ''' <summary>
        ''' Write to file.
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Narrowing Operator CType(row As RowObject) As String
            If row.IsNullOrEmpty Then
                Return ""
            Else
                Return row.AsLine
            End If
        End Operator

        ''' <summary>
        ''' Row parsing into column tokens
        ''' </summary>
        ''' <param name="Line"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Widening Operator CType(Line As String) As RowObject
            Dim row As List(Of String) = Tokenizer.CharsParser(Line)
            Return New RowObject(row)
        End Operator

        Public Shared Function TryParse(Line As String) As RowObject
            Return CType(Line, RowObject)
        End Function

        Public Shared Widening Operator CType(Tokens As String()) As RowObject
            Return New RowObject With {
                ._innerColumns = Tokens.ToList
            }
        End Operator

        Public Shared Widening Operator CType(Tokens As List(Of String)) As RowObject
            Return New RowObject With {
                ._innerColumns = Tokens
            }
        End Operator

        Public Shared Function CreateObject(DataTokens As IEnumerable(Of String)) As RowObject
            Return New RowObject With {
                ._innerColumns = DataTokens.ToList
            }
        End Function

        ''' <summary>
        ''' 去除行集合中的重复的数据行
        ''' </summary>
        ''' <param name="rowList"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Iterator Function Distinct(rowList As IEnumerable(Of RowObject)) As IEnumerable(Of RowObject)
            Dim source As IEnumerable(Of String) = From row In rowList
                                                   Let rowLine As String = CType(row, String)
                                                   Select rowLine
                                                   Distinct
                                                   Order By rowLine Ascending
            For Each line As String In source
                Yield New RowObject(line)
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For i As Integer = 0 To _innerColumns.Count - 1
                Yield _innerColumns(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' 查看目标行是否被包含在本行之中，即是否对应元素相等
        ''' </summary>
        ''' <param name="Row"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Contains(Row As RowObject) As Boolean
            If Row.IsNullOrEmpty Then
                Return True '空行肯定会被包含在本行中
            End If
            If Row.Count > Me.Count Then
                Return False '目标行中的元素数目多余本对象，则不包含
            End If

            For i As Integer = 0 To Row.Count - 1
                If String.IsNullOrEmpty(Row._innerColumns(i)) Then
                    Continue For '目标行的空元素被看作为相同元素
                End If
                If Not String.Equals(_innerColumns(i), Row._innerColumns(i)) Then
                    Return False '相对应的位置有不同的元素，则认为不包含
                End If
            Next
            Return True
        End Function

        Public Function AppendItem(columnValue As String) As RowObject
            Call Add(columnValue)
            Return Me
        End Function

        Public Overloads Shared Operator +(x As RowObject) As RowObject
            Return x
        End Operator

#Region "Implements of Generic.IList(Of System.String) interface"

        Public Sub Add(columnValue As String) Implements ICollection(Of String).Add
            If String.IsNullOrEmpty(columnValue) Then
                Call _innerColumns.Add("")
                Return
            ElseIf columnValue.First = """"c AndAlso columnValue.Last = """"c Then
                columnValue = Mid(columnValue, 2, Len(columnValue) - 2)
            End If
            Call _innerColumns.Add(columnValue)
        End Sub

        Public Sub Clear() Implements ICollection(Of String).Clear
            Call _innerColumns.Clear()
        End Sub

        Public Overloads Function Contains(item As String) As Boolean Implements ICollection(Of String).Contains
            Return _innerColumns.Contains(item)
        End Function

        Public Sub CopyTo(array() As String, arrayIndex As Integer) Implements ICollection(Of String).CopyTo
            Call _innerColumns.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' 当前数据行的列的数目
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumbersOfColumn As Integer Implements ICollection(Of String).Count
            Get
                Return _innerColumns.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of String).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As String) As Boolean Implements ICollection(Of String).Remove
            Return _innerColumns.Remove(item)
        End Function

        Public Function IndexOf(item As String) As Integer Implements IList(Of String).IndexOf
            Return _innerColumns.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As String) Implements IList(Of String).Insert
            Call _innerColumns.Insert(index, item)
        End Sub

        Public Sub RemoveAt(index As Integer) Implements IList(Of String).RemoveAt
            Call _innerColumns.RemoveAt(index)
        End Sub
#End Region

#Region "List Operations"
        Public Overloads Shared Operator +(dataframe As File, x As RowObject) As File
            Call dataframe.Add(x)
            Return dataframe
        End Operator

        Public Overloads Shared Operator -(dataframe As File, x As RowObject) As File
            Call dataframe.Remove(x)
            Return dataframe
        End Operator

        Public Shared Operator +(row As RowObject, col As String) As RowObject
            Call row._innerColumns.Add(col)
            Return row
        End Operator

        Public Shared Operator +(row As RowObject, col As IEnumerable(Of String)) As RowObject
            Call row._innerColumns.AddRange(col.ToArray)
            Return row
        End Operator
#End Region
    End Class
End Namespace
