
Namespace PdfReader

    Public Class PdfName : Inherits PdfObject

        Public ReadOnly Property StrVal As String
            Get
                Return Value
            End Get
        End Property

        Public Sub New(ByVal parent As PdfObject, ByVal name As ParseName)
            MyBase.New(parent, name)
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseName As ParseName
            Get
                Return TryCast(ParseObject, ParseName)
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return ParseName.Value
            End Get
        End Property
    End Class
End Namespace
