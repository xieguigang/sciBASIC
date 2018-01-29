#Region "Microsoft.VisualBasic::80ada0b68c01a4bacc04e39453290d42, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\test.vb"

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

Imports Microsoft.VisualBasic.MIME.Office
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Microsoft.VisualBasic.Data.csv.IO

Module test

    Sub Main()
        Call IOtest()
        ' Call test()
    End Sub

    Sub IOtest()
        Dim file = Xlsx.Open("E:\GCModeller\src\runtime\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\test\test.xlsx")
        Dim table As New csv

        table += New RowObject({"", "ddddddddd", "+++++++"})
        table += New RowObject({1, 2, 3, 4, 5})

        file.WriteSheetTable(table, "444444")

        Call file.WriteXlsx("./dddd.xlsx")

        Pause()
    End Sub

    Sub test()
        Call New Excel.XML.docProps.app With {
            .HeadingPairs = New Excel.XML.docProps.Vectors With {
                .vector = New Excel.XML.docProps.vector With {
                    .variants = {New Excel.XML.docProps.variant With {.i4 = 4444, .lpstr = "1234"}, New Excel.XML.docProps.variant With {.i4 = 4444, .lpstr = "ffff"}},
                    .baseType = "fffffffffff",
                    .size = "4453"
            }
            },
            .TitlesOfParts = New Excel.XML.docProps.Vectors With {.vector = New Excel.XML.docProps.vector With {.baseType = "test", .variants = {New Excel.XML.docProps.variant With {.i4 = "dddd", .lpstr = "1"}}}}
        }.GetXml.SaveTo("D:\rrrr.xml")

        Pause()
    End Sub
End Module

