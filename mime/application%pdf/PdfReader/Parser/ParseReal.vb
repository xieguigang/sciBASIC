
Namespace PdfReader
    Public Class ParseReal
        Inherits ParseObjectBase

        Private _Value As Single

        Public Sub New(ByVal token As TokenReal)
            Me.New(token.Value)
        End Sub

        Public Sub New(ByVal value As Single)
            Me.Value = value
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
