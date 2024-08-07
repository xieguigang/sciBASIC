#Region "Microsoft.VisualBasic::79657930c96d9787bc7f776dd465df38, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\test\test.vb"

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

'   Total Lines: 69
'    Code Lines: 30 (43.48%)
' Comment Lines: 19 (27.54%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 20 (28.99%)
'     File Size: 2.18 KB


' Module test
' 
'     Sub: Main, styleModelTest, test
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Office
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model

Module test

    Sub Main()
        testWriter()
        zip_test()



        ' stylingTest()
        styleModelTest()

        ' Call IOtest()
        ' Call test()
    End Sub

    'Sub stylingTest()
    '    Dim file = Excel.XLSX.CreateNew
    '    Dim style4 = Styling.ColorScale("black", "red", "green", "blue", "yellow")

    '    Call file.SetColorScaleStyles("Sheet1", "A1:A100", style4)

    '    Call file.SaveTo("D:\test.xlsx")

    '    Pause()
    'End Sub

    Sub styleModelTest()
        Dim font = Styling.StyleFont(FontFace.BookmanOldStyle, 55, "yellow", True, True)

        font.GetXml.__DEBUG_ECHO

        Pause()
    End Sub

    'Sub IOtest()
    '    Dim file = Excel.XLSX.File.Open("E:\GCModeller\src\runtime\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\test\test.xlsx")
    '    Dim table As New csv

    '    table += New RowObject({"", "ddddddddd", "+++++++"})
    '    table += New RowObject({1, 2, 3, 4, 5})

    '    file.WriteSheetTable(table, "444444")

    '    Call file.WriteXlsx("./dddd.xlsx")

    '    Pause()
    'End Sub

    Sub test()
        Call New Excel.XLSX.XML.docProps.app With {
            .HeadingPairs = New Excel.XLSX.XML.docProps.Vectors With {
                .vector = New Excel.XLSX.XML.docProps.vector With {
                    .variants = {New Excel.XLSX.XML.docProps.variant With {.i4 = 4444, .lpstr = "1234"}, New Excel.XLSX.XML.docProps.variant With {.i4 = 4444, .lpstr = "ffff"}},
                    .baseType = "fffffffffff",
                    .size = "4453"
            }
            },
            .TitlesOfParts = New Excel.XLSX.XML.docProps.Vectors With {
            .vector = New Excel.XLSX.XML.docProps.vector With {.baseType = "test", .variants = {New Excel.XLSX.XML.docProps.variant With {.i4 = "dddd", .lpstr = "1"}}}}
        }.GetXml.SaveTo("D:\rrrr.xml")

        Pause()
    End Sub
End Module
