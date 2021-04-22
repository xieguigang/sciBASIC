Namespace PdfReader
    Public Class TokenReal
        Inherits TokenObject

        Private _Value As Single

        Public Sub New(ByVal real As Single)
            Value = real
        End Sub

        Public Property Value As Single
            Get
                Return _Value
            End Get
            Private Set(ByVal value As Single)
                _Value = value
            End Set
        End Property
    End Class
End Namespace
