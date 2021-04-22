
Namespace PdfReader
    Public Class PdfIndirectObject
        Inherits PdfObject

        Private _Id As Integer, _Gen As Integer, _Offset As Long

        Public Sub New(ByVal parent As PdfObject, ByVal xref As TokenXRefEntry)
            MyBase.New(parent)
            Id = xref.Id
            Gen = xref.Gen
            Offset = xref.Offset
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
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

        Public Property Child As PdfObject
    End Class
End Namespace
