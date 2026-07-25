Imports System.IO

Public Module PDF

    Public Iterator Function GetText(file As Stream) As IEnumerable(Of String)
        Using reader As New PdfReader(file)
            Dim extractor As New TextExtractor(reader)
            Dim pages = reader.GetPages()

            For i As Integer = 0 To pages.Count - 1
                Yield extractor.ExtractFromPage(pages(i))
            Next
        End Using
    End Function
End Module
