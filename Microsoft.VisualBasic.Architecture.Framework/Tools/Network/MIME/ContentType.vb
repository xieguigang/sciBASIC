Imports System.Data.Linq.Mapping

Namespace Net.Protocols.ContentTypes

    ''' <summary>
    ''' MIME types / Internet Media Types
    ''' </summary>
    Public Class ContentType
        Public Property Name As String
        <Column(Name:="MIME Type / Internet Media Type")> Public Property MIMEType As String
        <Column(Name:="File Extension")>
        Public Property FileExt As String
        <Column(Name:="More Details")> Public Property Details As String

        Public Overrides Function ToString() As String
            Return $"{MIMEType} (*{FileExt})"
        End Function

        Friend Shared Function __createObject(line As String) As ContentType
            Dim tokens As String() = Strings.Split(line, vbTab)
            Dim mime As New ContentType With {
                .Name = tokens(Scan0),
                .MIMEType = tokens(1),
                .FileExt = tokens(2),
                .Details = tokens(3)
            }
            Return mime
        End Function
    End Class
End Namespace