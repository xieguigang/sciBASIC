#Region "Microsoft.VisualBasic::e8686c99c1559d0ca38c4da673250427, ..\sciBASIC#\Data\DataFrame\Extensions\DocumentExtensions.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Module DocumentExtensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="cols"><see cref="DocumentStream.File.Columns"/> filtering results.</param>
    ''' <returns></returns>
    <Extension>
    Public Function JoinColumns(cols As IEnumerable(Of String())) As DocumentStream.File
        Dim array$()() = cols.ToArray
        Dim out As New DocumentStream.File

        For i As Integer = 0 To array.First.Length - 1
            Dim ind As Integer = i
            out += New RowObject(array.Select(Function(x) x(ind)))
        Next

        Return out
    End Function

    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row._innerColumns.Count - 1
            row._innerColumns(i) = action(row._innerColumns(i))
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
    Public Function MergeTable(EXPORT$, files As IEnumerable(Of String)) As Boolean
        Dim data As New List(Of GenericTable)

        For Each path$ In files
            ' List(Of T) 对象的 + 语法有冲突，所以在这里需要先进行转换
            data += DirectCast(path.LoadCsv(Of GenericTable), IEnumerable(Of GenericTable))
        Next

        Return data.SaveTo(EXPORT)
    End Function

    <Extension>
    Public Function SaveTsv(csv As DocumentStream.File, path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean
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

    <Extension>
    Public Function LoadData(path$) As NamedValue(Of Double())()
        Dim data As DocumentStream.File =
            DocumentStream.File.Load(path)
        Dim out As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <=
 _
            From column As String()
            In data.Columns
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
        Return [Imports](Of T)(path, delimiter:=ASCII.TAB, encoding:=encoding.GetEncodings)
    End Function
End Module
