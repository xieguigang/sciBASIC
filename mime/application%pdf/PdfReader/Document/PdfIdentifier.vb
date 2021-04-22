
Namespace PdfReader
    Public Class PdfIdentifier
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject, ByVal name As ParseIdentifier)
            MyBase.New(parent, name)
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseIdentifier As ParseIdentifier
            Get
                Return TryCast(ParseObject, ParseIdentifier)
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return ParseIdentifier.Value
            End Get
        End Property
    End Class
End Namespace
