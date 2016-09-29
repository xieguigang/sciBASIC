
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Class ColorBrewer

        Public Property c3 As String()
        Public Property c4 As String()
        Public Property c5 As String()
        Public Property c6 As String()
        Public Property c7 As String()
        Public Property c8 As String()
        Public Property c9 As String()
        Public Property c10 As String()
        Public Property c11 As String()
        Public Property c12 As String()

        Public Property type As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace