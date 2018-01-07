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
