
Namespace PdfReader
    Public Class PdfInteger
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject, ByVal [integer] As ParseInteger)
            MyBase.New(parent, [integer])
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseInteger As ParseInteger
            Get
                Return TryCast(ParseObject, ParseInteger)
            End Get
        End Property

        Public ReadOnly Property Value As Integer
            Get
                Return ParseInteger.Value
            End Get
        End Property

        Public Shared Narrowing Operator CType(i As PdfInteger) As Integer
            Return i.Value
        End Operator
    End Class
End Namespace
