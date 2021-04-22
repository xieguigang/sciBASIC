Namespace PdfReader
    Public Class TokenXRefEntry
        Inherits TokenObject

        Private _Id As Integer, _Gen As Integer, _Offset As Long, _Used As Boolean

        Public Sub New(ByVal id As Integer, ByVal gen As Integer, ByVal offset As Long, ByVal used As Boolean)
            Me.Id = id
            Me.Gen = gen
            Me.Offset = offset
            Me.Used = used
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

        Public Property Offset As Long
            Get
                Return _Offset
            End Get
            Private Set(ByVal value As Long)
                _Offset = value
            End Set
        End Property

        Public Property Used As Boolean
            Get
                Return _Used
            End Get
            Private Set(ByVal value As Boolean)
                _Used = value
            End Set
        End Property
    End Class
End Namespace
