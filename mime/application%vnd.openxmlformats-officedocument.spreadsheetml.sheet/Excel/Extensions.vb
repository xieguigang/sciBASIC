#Region "Microsoft.VisualBasic::b14f4cc57a9e172c25435ab74af6f917, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Extensions.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File

Public Module Extensions

    ''' <summary>
    ''' Create a new empty excel xlsx file.
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateNew() As Xlsx
        With App.GetAppSysTempFile(".xlsx", App.PID)
            Call My.Resources._New.FlushStream(.ByRef)
            Return File.Open(path:= .ByRef)
        End With
    End Function

    ''' <summary>
    ''' 枚举出当前的这个Excel文件之中的所有的表格数据
    ''' </summary>
    ''' <param name="xlsx"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function EnumerateTables(xlsx As Xlsx) As IEnumerable(Of NamedValue(Of csv))
        Dim names$() = xlsx.xl _
            .workbook _
            .sheets _
            .Select(Function(s) s.name) _
            .ToArray

        For Each name As String In names
            Yield New NamedValue(Of csv) With {
                .Name = name,
                .Value = xlsx.GetTable(sheetName:=name)
            }
        Next
    End Function
End Module
