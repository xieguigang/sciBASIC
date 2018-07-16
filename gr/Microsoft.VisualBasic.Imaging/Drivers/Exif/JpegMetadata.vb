Namespace Driver

    Public Class JpegMetadata

        Public Property Title As String
        Public Property Subject As String
        Public Property Rating As Integer
        Public Property Keywords As List(Of String)
        Public Property Comments As String

        Friend Sub New()
            Me.Keywords = New List(Of String)()
        End Sub
    End Class
End Namespace