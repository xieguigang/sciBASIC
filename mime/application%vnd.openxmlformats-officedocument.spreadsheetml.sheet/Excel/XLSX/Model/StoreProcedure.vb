#Region "Microsoft.VisualBasic::46738daeaf8739828721c95f7440b879, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Model\StoreProcedure.vb"

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

    '   Total Lines: 145
    '    Code Lines: 111 (76.55%)
    ' Comment Lines: 13 (8.97%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 21 (14.48%)
    '     File Size: 5.08 KB


    '     Module StoreProcedure
    ' 
    '         Function: CreateColumn, CreateRow, CreateWorksheet, ToRowData, ToTableFrame
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.Excel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl.worksheets
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Namespace XLSX.Model

    ''' <summary>
    ''' 读取工作表的数据或者生成工作表数据的过程
    ''' </summary>
    Public Module StoreProcedure

        <Extension>
        Public Function ToTableFrame(worksheet As worksheets.worksheet, strings As sharedStrings) As csv
            Dim getValues As IEnumerable(Of RowObject) = worksheet.sheetData.rows _
                .Select(Function(r)
                            Return r.ToRowData(strings)
                        End Function)
            Dim csv As New csv(getValues)
            Return csv
        End Function

        <Extension>
        Private Function ToRowData(row As row, strings As sharedStrings) As RowObject
            Dim csv As New RowObject
            Dim string$
            Dim colIndex%
            Dim cellIndex As Point

            If row.columns.IsNullOrEmpty Then
                Return csv
            ElseIf row.columns.All(Function(c) c.r.StringEmpty) Then
                ' 顺序填充
                For i As Integer = 0 To row.columns.Length - 1
                    Dim col = row.columns(i)

                    [string] = col.GetValueString
                    csv(i) = [string]
                Next
            Else
                For Each col As XML.xl.worksheets.c In row.columns
                    If col.r.StringEmpty Then
                        Continue For
                    End If

                    ' 因为这里都是同一行的数据，所以只取列下标即可
                    cellIndex = Coordinates.Index(col.r)
                    colIndex = cellIndex.Y

                    If col.sharedStringsRef = -1 Then
                        [string] = col.GetValueString
                    Else
                        [string] = strings.strings(col.sharedStringsRef).ToString
                    End If

                    csv(colIndex - 1) = [string]
                Next
            End If

            Return csv
        End Function

        ''' <summary>
        ''' 创建新的表格对象
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="strings"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateWorksheet(table As csv, strings As sharedStrings) As worksheet
            Dim stringTable = strings.ToHashTable
            Dim rows As row() = table _
                .SeqIterator _
                .Select(Function(i) StoreProcedure.CreateRow(i.i + 1, i, stringTable)) _
                .ToArray
            Dim worksheet As New worksheet With {
                .sheetData = New sheetData With {
                    .rows = rows
                },
                .Dimension = New dimension With {
                    .ref = table.Dimension
                }
            }

            strings += stringTable

            Return worksheet
        End Function

        Private Function CreateRow(i%, data As RowObject, strings As Dictionary(Of String, Integer)) As row
            Dim spans$
            Dim cols As XML.xl.worksheets.c() = data _
                .SeqIterator _
                .Where(Function(s) Not s.value.StringEmpty) _
                .Select(Function(x)
                            Return strings.CreateColumn(i, x)
                        End Function) _
                .ToArray

            With data.Spans
                spans = $"{ .start},{ .ends}"
            End With

            Return New row With {
                .r = i,
                .spans = spans,
                .columns = cols
            }
        End Function

        <Extension>
        Private Function CreateColumn(strings As Dictionary(Of String, Integer), i%, x As SeqValue(Of String)) As XML.xl.worksheets.c
            Dim s$ = x
            Dim t$ = Nothing

            If strings.ContainsKey(s) Then
                ' 使用共享引用以减少所生成的文件的大小
                t = "s"
                s = strings(s)
            ElseIf Not s.IsNumeric Then

                ' 非数值类型的要添加进入共享字符串列表
                SyncLock strings
                    With strings
                        Call .Add(s, .Count)
                    End With
                End SyncLock

                t = "s"
                s = strings(s)
            End If

            Return New XML.xl.worksheets.c With {
                .r = x.i.ColumnIndex & i,
                .v = s,
                .t = t,
                .s = If(t Is Nothing, Nothing, "1")
            }
        End Function
    End Module
End Namespace
