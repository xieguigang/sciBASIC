Imports Microsoft.VisualBasic.Serialization.JSON

Namespace IL

    Public Class MethodReference

        Public Property func As String
        Public Property parameter As String()
        Public Property body As String()
        Public Property [return] As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace