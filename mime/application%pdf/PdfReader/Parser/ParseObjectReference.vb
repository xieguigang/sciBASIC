
Namespace PdfReader
    Public Class ParseObjectReference
        Inherits ParseObjectBase

        Private _Id As Integer, _Gen As Integer

        Public Sub New(ByVal id As TokenInteger, ByVal gen As TokenInteger)
            Me.Id = id.Value
            Me.Gen = gen.Value
        End Sub

        Public Property Id As Integer
            Get
                Return _Id
            End Get
            Private Set(ByVal value As Integer)
                _Id = value
            End Set
        End Property

        Public Property Gen As Integer
            Get
                Return _Gen
            End Get
            Private Set(ByVal value As Integer)
                _Gen = value
            End Set
        End Property
    End Class
End Namespace
