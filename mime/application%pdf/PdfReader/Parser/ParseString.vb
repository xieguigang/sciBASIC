
Namespace PdfReader
    Public Class ParseString
        Inherits ParseObjectBase

        Public Sub New(ByVal token As TokenString)
            Me.Token = token
        End Sub

        Public ReadOnly Property Value As String
            Get
                Return Token.Resolved
            End Get
        End Property

        Public ReadOnly Property ValueAsBytes As Byte()
            Get
                Return Token.ResolvedAsBytes
            End Get
        End Property

        Public Function BytesToString(ByVal bytes As Byte()) As String
            Return Token.BytesToString(bytes)
        End Function

        Private Property Token As TokenString
    End Class
End Namespace
