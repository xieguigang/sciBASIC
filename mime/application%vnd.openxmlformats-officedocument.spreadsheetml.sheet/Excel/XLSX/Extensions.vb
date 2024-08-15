#Region "Microsoft.VisualBasic::1a72ca3f0c697ac02850b78eb69e5a60, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Extensions.vb"

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

    '   Total Lines: 94
    '    Code Lines: 65 (69.15%)
    ' Comment Lines: 15 (15.96%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 14 (14.89%)
    '     File Size: 3.43 KB


    '     Module Extensions
    ' 
    '         Properties: Sheet1
    ' 
    '         Function: EnumerateTables, FirstSheet, GetSheetNames, ReadTableAuto
    ' 
    '         Sub: WriteSheetTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports XlsxFile = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File

Namespace XLSX

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' ``Sheet1`` is the default sheet name in excel file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Sheet1 As [Default](Of String)
            Get
                Return NameOf(Sheet1)
            End Get
        End Property

        ''' <summary>
        ''' 枚举出当前的这个Excel文件之中的所有的表格数据
        ''' </summary>
        ''' <param name="xlsx"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function EnumerateTables(xlsx As XlsxFile) As IEnumerable(Of NamedValue(Of csv))
            Dim names$() = xlsx.SheetNames.ToArray

            For Each name As String In names
                Yield New NamedValue(Of csv) With {
                .Name = name,
                .Value = xlsx.GetTable(sheetName:=name)
            }
            Next
        End Function

        Public Function GetSheetNames(path As String) As String()
            Dim xlsx As XlsxFile = File.Open(path)
            Dim names$() = xlsx.SheetNames.ToArray

            Return names
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path$">*.csv/*.xlsx</param>
        ''' <param name="sheetName$">如果传入的名称是空值的话，则总是会返回第一张表</param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadTableAuto(path$, Optional sheetName$ = "Sheet1") As csv
            With path.ExtensionSuffix.ToLower
                If .Equals("csv") Then
                    Return csv.Load(path)
                ElseIf .Equals("xlsx") Then
                    Dim Xlsx As XlsxFile = File.Open(path)

                    If sheetName.StringEmpty Then
                        sheetName = Xlsx.SheetNames.FirstOrDefault
                    End If
                    If sheetName.StringEmpty Then
                        Throw New NullReferenceException($"[{path}] didn't contains any sheet table!")
                    End If

                    Return Xlsx.GetTable(sheetName)
                Else
                    Throw New NotImplementedException
                End If
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FirstSheet(xlsx As XlsxFile) As csv
            Return xlsx.GetTable(sheetName:=xlsx.SheetNames.FirstOrDefault)
        End Function

        <Extension>
        Public Sub WriteSheetTable(workbook As Workbook, data As csv)
            Dim sheet As Worksheet = workbook.CurrentWorksheet

            For Each row As RowObject In data.AsEnumerable
                For Each col As String In row
                    Call sheet.AddNextCell(col)
                Next

                Call sheet.GoToNextRow()
            Next
        End Sub
    End Module
End Namespace
