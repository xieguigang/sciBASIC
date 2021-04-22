Namespace PdfReader
    Public Class TokenError
        Inherits TokenObject

        Private _Position As Long, _Message As String

        Public Sub New(ByVal position As Long, ByVal message As String)
            Me.Position = position
            Me.Message = message
        End Sub

        Public Property Position As Long
            Get
                Return _Position
            End Get
            Private Set(ByVal value As Long)
                _Position = value
            End Set
        End Property

        Public Property Message As String
            Get
                Return _Message
            End Get
            Private Set(ByVal value As String)
                _Message = value
            End Set
        End Property
    End Class
End Namespace
