Namespace Serialization.Bencoding

    ''' <summary>
    ''' A class with extension methods for use with Bencoding.
    ''' </summary>
    Public Module BencodingExtensions
        ''' <summary>
        ''' Decode the current instance.
        ''' </summary>
        ''' <paramname="s">The current instance.</param>
        ''' <returns>The root elements of the decoded string.</returns>
        <Extension()>
        Public Function BDecode(ByVal s As String) As BElement()
            Return Decode(s)
        End Function
    End Module
End Namespace