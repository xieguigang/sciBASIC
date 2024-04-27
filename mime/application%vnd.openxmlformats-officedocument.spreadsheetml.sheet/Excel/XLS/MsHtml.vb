﻿#Region "Microsoft.VisualBasic::9cbceee9b6bb35d9c119b11702acb973, G:/GCModeller/src/runtime/sciBASIC#/mime/application%vnd.openxmlformats-officedocument.spreadsheetml.sheet/Excel//XLS/MsHtml.vb"

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

    '   Total Lines: 56
    '    Code Lines: 42
    ' Comment Lines: 6
    '   Blank Lines: 8
    '     File Size: 2.13 KB


    '     Module MsHtml
    ' 
    '         Function: (+2 Overloads) ToExcel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DATA
Imports Table = Microsoft.VisualBasic.Data.csv.IO.File

Namespace XLS

    Public Module MsHtml

        Public Function ToExcel(Of T As Class)(seq As IEnumerable(Of T), Optional sheetName$ = "Sheet1") As String
            Return seq.ToCsvDoc.ToExcel(sheetName)
        End Function

        ''' <summary>
        ''' excel table in html format
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="sheetName"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToExcel(file As Table, sheetName As String, Optional width As Dictionary(Of String, String) = Nothing) As String
            Dim html As New StringBuilder()

            html.AppendLine("<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">")
            html.AppendLine("<head>")
            html.AppendLine("<meta name=""ProgId"" content=""Excel.Sheet"">")
            html.AppendLine("<meta name=""Generator"" content=""Microsoft Excel 11"">")
            html.AppendLine("<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">")

            html.AppendLine($"    
<!--[if gte mso 9]>
	<xml>
		<x:ExcelWorkbook>
            <x:ExcelWorksheets>
                <x:ExcelWorksheet>
                    <x:Name>{sheetName}</x:Name>
                    <x:WorksheetOptions>
                        <x:DisplayGridlines/>
                    </x:WorksheetOptions>
                </x:ExcelWorksheet>
            </x:ExcelWorksheets>
		</x:ExcelWorkbook>
    </xml>
<![endif]-->")

            html.AppendLine("</head>")
            html.AppendLine("<body>")
            html.AppendLine(New HTMLWriter(thwidth:=width).html(file))
            html.AppendLine("</body>")
            html.AppendLine("</html>")

            Return html.ToString()
        End Function
    End Module
End Namespace
