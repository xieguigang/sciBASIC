#Region "Microsoft.VisualBasic::734a0cdd91baadfce6c0eb4f3452034e, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Extensions.vb"

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

    '   Total Lines: 81
    '    Code Lines: 55 (67.90%)
    ' Comment Lines: 15 (18.52%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 11 (13.58%)
    '     File Size: 3.00 KB


    '     Module Extensions
    ' 
    '         Properties: Sheet1
    ' 
    '         Function: EnumerateTables, FirstSheet, GetSheetNames, ReadTableAuto
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
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
        Public Sub WriteSheetTable(sheet As Worksheet, data As csv)

        End Sub
    End Module
End Namespace
