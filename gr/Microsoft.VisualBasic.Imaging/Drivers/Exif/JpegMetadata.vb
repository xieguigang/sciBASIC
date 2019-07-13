#Region "Microsoft.VisualBasic::68a7f9f29425dbed9adb4c9a67fa4a6e, gr\Microsoft.VisualBasic.Imaging\Drivers\Exif\JpegMetadata.vb"

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

    '     Class JpegMetadata
    ' 
    '         Properties: Author, Comments, Keywords, Rating, Subject
    '                     Title
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Driver

    Public Class JpegMetadata

        Public Property Title As String
        Public Property Subject As String
        Public Property Author As List(Of String)
        Public Property Rating As Integer
        Public Property Keywords As List(Of String)
        Public Property Comments As String

        Friend Sub New()
        End Sub
    End Class
End Namespace
