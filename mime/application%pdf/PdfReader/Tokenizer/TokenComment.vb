Namespace PdfReader
    Public Class TokenComment
        Inherits TokenObject

        Private _Value As String

        Public Sub New(ByVal comment As String)
            Value = comment
        End Sub

        Public Property Value As String
            Get
                Return _Value
            End Get
            Private Set(ByVal value As String)
                _Value = value
            End Set
        End Property
    End Class
End Namespace
