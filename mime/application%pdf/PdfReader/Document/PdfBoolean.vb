
Namespace PdfReader
    Public Class PdfBoolean
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject, ByVal [boolean] As ParseBoolean)
            MyBase.New(parent, [boolean])
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseBoolean As ParseBoolean
            Get
                Return TryCast(ParseObject, ParseBoolean)
            End Get
        End Property

        Public ReadOnly Property Value As Boolean
            Get
                Return ParseBoolean.Value
            End Get
        End Property

        Public Shared Narrowing Operator CType(b As PdfBoolean) As Boolean
            Return b.Value
        End Operator
    End Class
End Namespace
