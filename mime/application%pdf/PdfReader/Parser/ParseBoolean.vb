
Namespace PdfReader
    Public Class ParseBoolean
        Inherits ParseObjectBase

        Private _Value As Boolean

        Public Sub New(ByVal value As Boolean)
            Me.Value = value
        End Sub

        Public Property Value As Boolean
            Get
                Return _Value
            End Get
            Private Set(ByVal value As Boolean)
                _Value = value
            End Set
        End Property
    End Class
End Namespace
