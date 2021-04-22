Namespace PdfReader
    Public Class TokenInteger
        Inherits TokenObject

        Private _Value As Integer

        Public Sub New(ByVal [integer] As Integer)
            Value = [integer]
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
