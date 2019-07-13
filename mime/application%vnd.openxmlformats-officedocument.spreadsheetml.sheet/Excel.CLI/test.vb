#Region "Microsoft.VisualBasic::139f0dbfcf4da3f97d04233ce967e2cf, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\test.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module test
    ' 
    '     Sub: IOtest, Main, styleModelTest, stylingTest, test
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Office
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File

Module test

    Sub Main()
        stylingTest()
        styleModelTest()

        Call IOtest()
        ' Call test()
    End Sub

    Sub stylingTest()
        Dim file = Excel.CreateNew
        Dim style4 = Styling.ColorScale("black", "red", "green", "blue", "yellow")

        Call file.SetColorScaleStyles("Sheet1", "A1:A100", style4)

        Call file.SaveTo("D:\test.xlsx")

        Pause()
    End Sub

    Sub styleModelTest()
        Dim font = Styling.StyleFont(FontFace.BookmanOldStyle, 55, "yellow", True, True)

        font.GetXml.__DEBUG_ECHO

        Pause()
    End Sub

    Sub IOtest()
        Dim file = Excel.File.Open("E:\GCModeller\src\runtime\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\test\test.xlsx")
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
