#Region "Microsoft.VisualBasic::26bccae598a2ac5cec3b4c552ffddbc5, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\test\Module1.vb"

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

    '   Total Lines: 27
    '    Code Lines: 19 (70.37%)
    ' Comment Lines: 1 (3.70%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (25.93%)
    '     File Size: 1006 B


    ' Module Module1
    ' 
    '     Sub: testWriter, zip_test
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

Module Module1

    Sub testWriter()
        Dim workbook As New Workbook("basic.xlsx", "SXheet124234") ' Create New workbook
        workbook.CurrentWorksheet.AddNextCell("Test") ' Add cell A1
        workbook.CurrentWorksheet.AddNextCell(55.2) ' Add cell B1
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        workbook.AddWorksheet("page_nooote")
        workbook.CurrentWorksheet.AddNextCell("Test22222") ' Add cell A1
        workbook.CurrentWorksheet.AddNextCell(4323355.2, New Style With {.CurrentFill = New Style.Fill With {.BackgroundColor = "FFFFBB66"}}) ' Add cell B1
        workbook.CurrentWorksheet.AddNextCell(DateTime.Now) ' Add cell C1

        workbook.Save()

        ' Pause()
    End Sub

    Sub zip_test()
        Dim xlsx As New ZipStream("basic.xlsx", is_readonly:=True)

        Pause()
    End Sub
End Module
