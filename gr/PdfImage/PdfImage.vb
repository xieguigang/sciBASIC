Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Public Class PdfImage : Inherits GraphicsData

    ReadOnly image As PdfGraphics
    ReadOnly pdf As MemoryStream

    Public Overrides ReadOnly Property Driver As Drivers
        Get
            Return Drivers.PDF
        End Get
    End Property

    Public Sub New(img As Object, size As Size, padding As Padding)
        MyBase.New(img, size, padding)

        image = DirectCast(img, PdfGraphics)
        pdf = image.buffer

        Call image.page.Document.CreateFile()
        Call pdf.Seek(Scan0, SeekOrigin.Begin)
    End Sub

    Public Overrides Function GetDataURI() As DataURI
        Using buffer As New MemoryStream
            Call Save(buffer)
            Return New DataURI(buffer, "application/pdf")
        End Using
    End Function

    Public Overrides Function Save(path As String) As Boolean
        Using file As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Return Save(file)
        End Using
    End Function

    Public Overrides Function Save(out As Stream) As Boolean
        Call pdf.CopyTo(out)
        Call pdf.Seek(Scan0, SeekOrigin.Begin)
        Call out.Flush()

        Return True
    End Function
End Class
