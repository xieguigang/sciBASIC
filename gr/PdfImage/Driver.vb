Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Module Driver

    Sub New()
        ImageDriver.pdfDriver = AddressOf OpenDevice
        ImageDriver.getPdfImage = AddressOf CreateImage
    End Sub

    Private Function CreateImage(g As IGraphics, size As Size, padding As Padding) As GraphicsData
        Return New PdfImage(g, size, padding)
    End Function

    Public Function OpenDevice(size As Size) As PdfGraphics
        Dim buffer As New MemoryStream
        Dim stream As New PdfDocument(size.Width, size.Height, 1, buffer)
        Dim info = PdfInfo.CreatePdfInfo(stream)
        Dim localTime = DateTime.Now

        info.Title("Article Example")
        info.Author(Environment.UserName)
        info.Keywords("PDF, .NET, C#, Library, Document Creator")
        info.Subject("PDF File Writer C# Class Library (Version 1.15.0)")
        info.CreationDate(localTime)
        info.ModDate(localTime)
        info.Creator("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)
        info.Producer("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)

        Dim page As New PdfPage(stream)
        Dim g As New PdfGraphics(size, page, buffer)

        Return g
    End Function

    Public Sub Init()
        Call Console.WriteLine()
    End Sub

End Module
