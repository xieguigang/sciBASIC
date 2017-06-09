#Region "Microsoft.VisualBasic::d48e5ccc8da20468163e0990dccb8988, ..\sciBASIC#\Data\DataFrame\Extensions\DocumentExtensions.vb"

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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

''' <summary>
''' The csv document extensions API
''' </summary>
Public Module DocumentExtensions

    ''' <summary>
    ''' 将列数据合并为一个csv文件对象
    ''' </summary>
    ''' <param name="cols"><see cref="io.File.Columns"/> filtering results.</param>
    ''' <returns></returns>
    <Extension>
    Public Function JoinColumns(cols As IEnumerable(Of String())) As IO.File
        Dim array$()() = cols.ToArray
        Dim out As New IO.File

        For i As Integer = 0 To array.First.Length - 1
            Dim ind As Integer = i
            out += New RowObject(array.Select(Function(x) x(ind)))
        Next

        Return out
    End Function

    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row.buffer.Count - 1
            row.buffer(i) = action(row.buffer(i))
        Next

        Return row
    End Function

    Private Class GenericTable
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Data.GetJson
        End Function
    End Class

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, String)), path$) As Boolean
        Dim table As GenericTable() = d _
            .Select(Function(x) New GenericTable With {.Data = x}) _
            .ToArray
        Return table.SaveTo(path)
    End Function

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, Double)), path$) As Boolean
        Return d _
            .Select(
            Function(x) x.ToDictionary(
            Function(k) k.Key,
            Function(v) v.Value.ToString)) _
            .SaveAsDataFrame(path)
    End Function

    <Extension>
    Public Function MergeTable(EXPORT$, files As IEnumerable(Of String)) As Boolean
        Dim data As New List(Of GenericTable)

        For Each path$ In files
            ' List(Of T) 对象的 + 语法有冲突，所以在这里需要先进行转换
            data += DirectCast(path.LoadCsv(Of GenericTable), IEnumerable(Of GenericTable))
        Next

        Return data.SaveTo(EXPORT)
    End Function

    <Extension>
    Public Function SaveTsv(csv As IO.File, path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Using file As StreamWriter = path.OpenWriter(encoding)
            For Each line In csv
                Call file.WriteLine(line.TsvLine)
            Next

            Return True
        End Using
    End Function

    <Extension>
    Public Function TsvLine(row As RowObject) As String
        Dim ls As New List(Of String)

        For Each c As String In row
            If c.Contains(ASCII.TAB) Then
                c = $"""{c}"""
            End If

            ls.Add(c)
        Next

        Return ls.JoinBy(ASCII.TAB)
    End Function

    ''' <summary>
    ''' 文件之中的每一列都是数据
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="skipFirstColumn">假若第一列是固定的时间序列的话，是否需要跳过这第一列？？</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadData(path$, Optional skipFirstColumn As Boolean = False) As NamedValue(Of Double())()
        Dim data As IO.File = IO.File.Load(path)
        Dim source As IEnumerable(Of String())

        If skipFirstColumn Then
            source = data.Columns.Skip(1)
        Else
            source = data.Columns
        End If

        Dim out = LinqAPI.Exec(Of NamedValue(Of Double())) <=
 _
            From column As String()
            In source
            Let name As String = column(Scan0)
            Let values As Double() = column.Skip(1).ToArray(AddressOf Val)
            Select New NamedValue(Of Double()) With {
                .Name = name,
                .Value = values
            }

        Return out
    End Function

    <Extension>
    Public Function LoadTsv(Of T As Class)(path$, Optional encoding As Encodings = Encodings.Default) As T()
        Return [Imports](Of T)(path, delimiter:=ASCII.TAB, encoding:=encoding.CodePage)
    End Function

    <Extension>
    Public Iterator Function LoadMappings(path$, key$, mapTo$) As IEnumerable(Of Map(Of String, String))
        Dim header As RowObject = RowObject.TryParse(path.ReadFirstLine)
        Dim keyIndex% = header.IndexOf(key)
        Dim mapIndex% = header.IndexOf(mapTo)

        If keyIndex = -1 OrElse mapIndex = -1 Then
            Dim msg$ =
                $"Mapping: {key} --> {mapTo} is missing in the target csv file! ({header.ToArray.GetJson})"
            Throw New KeyNotFoundException(msg)
        End If

        Dim skip1 As Boolean = True

        For Each line$ In path.IterateAllLines
            If Not skip1 Then
                Dim row As RowObject = RowObject.TryParse(line)

                Yield New Map(Of String, String) With {
                    .key = row(keyIndex),
                    .Maps = row(mapIndex)
                }
            Else
                skip1 = False
            End If
        Next
    End Function

    <Extension>
    Public Function GetColumnValues(csv As IO.File, column$) As IEnumerable(Of String)
        Dim index As Integer = csv.Headers.IndexOf(column)
        Dim out As New List(Of String)

        For Each r As RowObject In csv.Skip(1)
            Call out.Add(r(index))
        Next

        Return out
    End Function

    <Extension>
    Public Iterator Function GetColumnObjects(Of T)(csv As IO.File, column$, [ctype] As Func(Of String, T)) As IEnumerable(Of T)
        For Each row As String In csv.GetColumnValues(column)
            Yield [ctype](row)
        Next
    End Function

    <Extension>
    Public Function LoadCsv(path$, Optional encoding As Encodings = Encodings.ASCII) As IO.File
        Return IO.File.Load(path, encoding.CodePage)
    End Function
End Module
