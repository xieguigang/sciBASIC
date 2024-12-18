#Region "Microsoft.VisualBasic::b02acc0cf5846eb63357dce6d8fb7e68, Data\DataFrame\IO\csv\RowObject.vb"

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

    '   Total Lines: 502
    '    Code Lines: 300 (59.76%)
    ' Comment Lines: 141 (28.09%)
    '    - Xml Docs: 97.87%
    ' 
    '   Blank Lines: 61 (12.15%)
    '     File Size: 18.97 KB


    '     Class RowObject
    ' 
    '         Properties: AsLine, DirectGet, IsReadOnly, NumbersOfColumn, Width
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: AddRange, AppendItem, (+2 Overloads) Contains, CreateObject, GetALLNonEmptys
    '                   GetColumn, GetEnumerator, GetEnumerator1, IndexAny, IndexOf
    '                   InsertAt, LocateKeyWord, PopLine, Remove, (+2 Overloads) Takes
    '                   (+2 Overloads) ToString, TryParse
    ' 
    '         Sub: Add, Clear, CopyTo, Insert, RemoveAt
    '              Trim
    ' 
    '         Operators: -, (+4 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Text
Imports any = Microsoft.VisualBasic.Scripting

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
        Protected Friend buffer As New List(Of String)

        Sub New(Optional columns As IEnumerable(Of String) = Nothing)
            If Not columns Is Nothing Then
                Me.buffer = columns.AsList
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ParamArray columns$())
            buffer = columns.AsList
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="objs">using <see cref="any.ToString"/> to converts the objects into a string array.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(objs As IEnumerable(Of Object))
            Call Me.New(objs.Select(Function(x) Scripting.ToString(x)))
        End Sub

        ''' <summary>
        ''' 这个构造函数会使用<see cref="Tokenizer.CharsParser"/>解析所输入的字符串为列数据的集合
        ''' </summary>
        ''' <param name="rawString">A raw string line which read from the Csv text file.</param>
        Sub New(rawString$, Optional tsv As Boolean = False)
            Try
                If rawString.StringEmpty Then
                    buffer = New List(Of String)
                Else
                    buffer = Tokenizer.CharsParser(rawString, delimiter:=","c Or ASCII.TAB.When(tsv)).AsList
                End If
            Catch ex As Exception
                Throw New Exception(rawString, ex)
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
                Return buffer(index)
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

                If Index < buffer.Count Then
                    Return buffer(Index)
                Else
                    Return ""
                End If
            End Get
            Set(value As String)
                If Index < buffer.Count Then
                    buffer(Index) = value
                Else
                    Dim d As Integer = Index - buffer.Count

                    '当前行的数目少于指定的索引号的时候，进行填充
                    For i As Integer = 0 To d - 1
                        buffer.Add("")
                    Next

                    Call buffer.Add(value)
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
                                                In buffer
                                                Where String.IsNullOrEmpty(c)
                                                Select 1 '
                Return buffer.Count - LQuery.Length
            End Get
        End Property

        ''' <summary>
        ''' 返回本行中的非空白数据
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetALLNonEmptys() As String()
            Dim LQuery = LinqAPI.Exec(Of String) <=
                                                   _
                From s As String
                In buffer
                Where Not String.IsNullOrEmpty(s)
                Select s '

            Return LQuery
        End Function

        ''' <summary>
        ''' insert the data into a spercific column  
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="column">
        ''' 假若列的位置超过了当前的行所具有的列的数量，那么这个方法还会自动补齐空格
        ''' </param>
        ''' <returns>仅为LINQ查询使用的一个无意义的值</returns>
        ''' <remarks></remarks>
        Public Function InsertAt(value As String, column As Integer) As Integer
            Dim d As Integer = column - buffer.Count - 1
            If d > 0 Then
                For i As Integer = 0 To d
                    Call buffer.Add("")
                Next
            End If
            Call buffer.Insert(column, value)
            Return 0
        End Function

        ''' <summary>
        ''' Displaying in IDE
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return Me.AsLine
        End Function

        ''' <summary>
        ''' Takes the data in the specific number of columns, if columns is not exists in this row object, 
        ''' then a part of returned data will be the empty string. 
        ''' </summary>
        ''' <param name="Count"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(Count As Integer) As String()
            Dim d As Integer = Count - buffer.Count

            If d < 0 Then
                Return buffer.Take(Count).ToArray
            Else
                Dim List As List(Of String) = New List(Of String)
                List.AddRange(buffer)
                For i As Integer = 0 To d
                    List.Add("")
                Next

                Return List.ToArray
            End If
        End Function

        ''' <summary>
        ''' Takes the data in the specific column index collection, if the column is not exists in the row object, 
        ''' then a part of the returned data will be the empty string.
        ''' </summary>
        ''' <param name="Cols"></param>
        ''' <param name="retNullable">(当不存在数据的时候是否返回空字符串，默认返回空字符串)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(Cols As Integer(), Optional retNullable As Boolean = True) As String()
            Dim Items As String() = New String(Cols.Count - 1) {}
            For i As Integer = 0 To Cols.Length - 1
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
            If Idx > Me.buffer.Count - 1 Then
                retStr = Nothing
                Return True
            Else
                retStr = buffer(Idx)
                Return False
            End If
        End Function

        Public Function AddRange(values As IEnumerable(Of String)) As Integer
            Call buffer.AddRange(values)
            Return buffer.Count
        End Function

        ''' <summary>
        ''' 查询某一个关键词在本行中的哪一个单元格，返回-1表示没有查询到本关键词
        ''' </summary>
        ''' <param name="KeyWord">进行查找的目标单元格文本的一部分字符串</param>
        ''' <param name="CaseSensitive">是否是大小写不敏感的</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LocateKeyWord(KeyWord As String, Optional CaseSensitive As Boolean = True) As Integer
            Dim compare As CompareMethod = CompareMethod.Text Or CompareMethod.Binary.When(CaseSensitive)
            Dim i As Integer = 0

            For Each cell As String In buffer
                If InStr(cell, KeyWord, compare) > 0 Then
                    Return i
                Else
                    i += 1
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' Generate a line of the string data in the csv document.
        ''' (将当前的行对象转换为文件中的一行字符串)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AsLine(Optional delimiter As String = ",") As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ToString(buffer, delimiter)
            End Get
        End Property

        ''' <summary>
        ''' create content row string in a csv table file.
        ''' </summary>
        ''' <param name="content"></param>
        ''' <param name="delimiter"></param>
        ''' <returns></returns>
        Public Overloads Shared Function ToString(content As IEnumerable(Of String), Optional delimiter As String = ",") As String
            Dim array As String() = content _
                .Select(Function(cell)
                            Return cell.doDelimiterMask(delimiter)
                        End Function) _
                .ToArray
            Dim line As String = String.Join(delimiter, array)

            Return line
        End Function

        Public Sub Trim(lefts%)
            buffer = New List(Of String)(buffer.Take(lefts))
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
            Return New RowObject(Tokenizer.CharsParser(Line))
        End Operator

        ''' <summary>
        ''' Parse a text line as csv table row via <see cref="Tokenizer.CharsParser"/>
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="tsv"></param>
        ''' <returns></returns>
        Public Shared Function TryParse(line As String, Optional tsv As Boolean = False) As RowObject
            Return New RowObject(line, tsv)
        End Function

        Public Shared Widening Operator CType(tokens As String()) As RowObject
            Return New RowObject With {
                .buffer = tokens.AsList
            }
        End Operator

        Public Shared Widening Operator CType(tokens As List(Of String)) As RowObject
            Return New RowObject With {
                .buffer = tokens
            }
        End Operator

        Public Shared Function CreateObject(tokens As IEnumerable(Of String)) As RowObject
            Return New RowObject With {
                .buffer = tokens.AsList
            }
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For i As Integer = 0 To buffer.Count - 1
                Yield buffer(i)
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
                If String.IsNullOrEmpty(Row.buffer(i)) Then
                    Continue For '目标行的空元素被看作为相同元素
                End If
                If Not String.Equals(buffer(i), Row.buffer(i)) Then
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
                Call buffer.Add("")
                Return
            ElseIf columnValue.First = """"c AndAlso columnValue.Last = """"c Then
                columnValue = Mid(columnValue, 2, Len(columnValue) - 2)
            End If
            Call buffer.Add(columnValue)
        End Sub

        Public Sub Clear() Implements ICollection(Of String).Clear
            Call buffer.Clear()
        End Sub

        Public Overloads Function Contains(item As String) As Boolean Implements ICollection(Of String).Contains
            Return buffer.Contains(item)
        End Function

        Public Sub CopyTo(array() As String, arrayIndex As Integer) Implements ICollection(Of String).CopyTo
            Call buffer.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' 当前数据行的列的数目
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumbersOfColumn As Integer Implements ICollection(Of String).Count
            Get
                Return buffer.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of String).IsReadOnly
            Get
                Return False
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Remove(item As String) As Boolean Implements ICollection(Of String).Remove
            Return buffer.Remove(item)
        End Function

        ''' <summary>
        ''' 查找目标在行之中的索引号，也就是列的编号
        ''' </summary>
        ''' <param name="item">如果字符串是空的话或者查找失败，则返回-1</param>
        ''' <returns></returns>
        Public Function IndexOf(item As String) As Integer Implements IList(Of String).IndexOf
            If item.StringEmpty Then
                Return -1
            Else
                Return buffer.IndexOf(item)
            End If
        End Function

        Public Function IndexAny(ParamArray any As String()) As Integer
            For Each item As String In any
                Dim ordinal As Integer = IndexOf(item)

                If ordinal > -1 Then
                    Return ordinal
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 直接使用list对象的Insert方法插入目标值，这个方法不像<see cref="RowObject.InsertAt(String, Integer)"/>，这个方法不会自动补齐空格的
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="item"></param>
        Public Sub Insert(index As Integer, item As String) Implements IList(Of String).Insert
            Call buffer.Insert(index, item)
        End Sub

        Public Sub RemoveAt(index As Integer) Implements IList(Of String).RemoveAt
            Call buffer.RemoveAt(index)
        End Sub
#End Region

        ''' <summary>
        ''' Pop all cells string from the internal buffer list and then returns the generated text line.
        ''' </summary>
        ''' <param name="delimiter"></param>
        ''' <returns></returns>
        Public Function PopLine(Optional delimiter As String = ",") As String
            Return ToString(buffer.PopAll, delimiter)
        End Function

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
            Call row.buffer.Add(col)
            Return row
        End Operator

        Public Shared Operator +(row As RowObject, col As IEnumerable(Of String)) As RowObject
            Call row.buffer.AddRange(col.ToArray)
            Return row
        End Operator
#End Region
    End Class
End Namespace
