
Namespace PdfReader
    Public Class PdfNull
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public Overrides Function ToString() As String
            Return "null"
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub
    End Class
End Namespace
