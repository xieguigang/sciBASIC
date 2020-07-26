Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencoding exception.
    ''' </summary>
    Public Class BencodingException
        Inherits FormatException

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <param name="message">The message.</param>
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <param name="message">The message.</param>
        ''' <param name="inner">The inner exception.</param>
        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub
    End Class

End Namespace