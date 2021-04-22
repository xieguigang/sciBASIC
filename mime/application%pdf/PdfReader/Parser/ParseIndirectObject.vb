
Namespace PdfReader
    Public Class ParseIndirectObject
        Inherits ParseObjectBase

        Private _Id As Integer, _Gen As Integer, _Object As PdfReader.ParseObjectBase

        Public Sub New(ByVal id As TokenInteger, ByVal gen As TokenInteger, ByVal obj As ParseObjectBase)
            Me.Id = id.Value
            Me.Gen = gen.Value
            [Object] = obj
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

        Public Property [Object] As ParseObjectBase
            Get
                Return _Object
            End Get
            Private Set(ByVal value As ParseObjectBase)
                _Object = value
            End Set
        End Property
    End Class
End Namespace
