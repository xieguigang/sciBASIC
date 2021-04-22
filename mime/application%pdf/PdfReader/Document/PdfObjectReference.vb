
Namespace PdfReader
    Public Class PdfObjectReference
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject, ByVal reference As ParseObjectReference)
            MyBase.New(parent, reference)
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseObjectReference As ParseObjectReference
            Get
                Return TryCast(ParseObject, ParseObjectReference)
            End Get
        End Property

        Public ReadOnly Property Id As Integer
            Get
                Return ParseObjectReference.Id
            End Get
        End Property

        Public ReadOnly Property Gen As Integer
            Get
                Return ParseObjectReference.Gen
            End Get
        End Property
    End Class
End Namespace
