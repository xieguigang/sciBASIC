
Namespace PdfReader
    Public Class ParseInteger
        Inherits ParseObjectBase

        Private _Value As Integer

        Public Sub New(ByVal token As TokenInteger)
            Value = token.Value
        End Sub

        Public Property Value As Integer
            Get
                Return _Value
            End Get
            Private Set(ByVal value As Integer)
                _Value = value
            End Set
        End Property
    End Class
End Namespace
