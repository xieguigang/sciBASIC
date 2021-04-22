
Namespace PdfReader
    Public Class PdfReal
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject, ByVal real As ParseReal)
            MyBase.New(parent, real)
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseReal As ParseReal
            Get
                Return TryCast(ParseObject, ParseReal)
            End Get
        End Property

        Public ReadOnly Property Value As Single
            Get
                Return ParseReal.Value
            End Get
        End Property

        Public Shared Narrowing Operator CType(f As PdfReal) As Single
            Return f.Value
        End Operator
    End Class
End Namespace
