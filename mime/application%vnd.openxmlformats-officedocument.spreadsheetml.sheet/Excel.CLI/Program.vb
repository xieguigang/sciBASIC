Imports Microsoft.VisualBasic.MIME.Office

Module Program

    Public Function Main() As Integer
        ' Call test()

        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function

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
