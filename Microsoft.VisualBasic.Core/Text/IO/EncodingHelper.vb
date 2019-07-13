#Region "Microsoft.VisualBasic::c7deacbacbe45ff501aee967090fc664, Microsoft.VisualBasic.Core\Text\IO\EncodingHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class EncodingHelper
    ' 
    '         Properties: TextEncoding
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBytes, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Text

    ''' <summary>
    ''' Helper for <see cref="Encodings"/>
    ''' </summary>
    Public Class EncodingHelper

        ''' <summary>
        ''' <see cref="Encoding"/> instance
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TextEncoding As Encoding

        ''' <summary>
        ''' Helper for <see cref="Encodings"/>
        ''' </summary>
        ''' <param name="encoding"></param>
        Sub New(encoding As Encodings)
            TextEncoding = encoding.CodePage
        End Sub

        '
        ' Summary:
        '     When overridden in a derived class, encodes all the characters in the specified
        '     string into a sequence of bytes.
        '
        ' Parameters:
        '   s:
        '     The string containing the characters to encode.
        '
        ' Returns:
        '     A byte array containing the results of encoding the specified set of characters.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     s is null.
        '
        '   T:System.Text.EncoderFallbackException:
        '     A fallback occurred (see Character Encoding in the .NET Framework for complete
        '     explanation)-and-System.Text.Encoding.EncoderFallback is set to System.Text.EncoderExceptionFallback.

        ''' <summary>
        ''' When overridden in a derived class, encodes all the characters in the specified
        ''' string into a sequence of bytes.
        ''' </summary>
        ''' <param name="s">The string containing the characters to encode.</param>
        ''' <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        Public Function GetBytes(s As String) As Byte()
            Return TextEncoding.GetBytes(s)
        End Function

        '
        ' Summary:
        '     When overridden in a derived class, decodes all the bytes in the specified byte
        '     array into a string.
        '
        ' Parameters:
        '   bytes:
        '     The byte array containing the sequence of bytes to decode.
        '
        ' Returns:
        '     A string that contains the results of decoding the specified sequence of bytes.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     The byte array contains invalid Unicode code points.
        '
        '   T:System.ArgumentNullException:
        '     bytes is null.
        '
        '   T:System.Text.DecoderFallbackException:
        '     A fallback occurred (see Character Encoding in the .NET Framework for complete
        '     explanation)-and-System.Text.Encoding.DecoderFallback is set to System.Text.DecoderExceptionFallback.

        ''' <summary>
        ''' When overridden in a derived class, decodes all the bytes in the specified byte
        ''' array into a string.
        ''' </summary>
        ''' <param name="byts">The byte array containing the sequence of bytes to decode.</param>
        ''' <returns>A string that contains the results of decoding the specified sequence of bytes.</returns>
        Public Overloads Function ToString(byts As Byte()) As String
            Return TextEncoding.GetString(byts)
        End Function

        ''' <summary>
        ''' <see cref="Encoding"/> instance
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return TextEncoding.ToString
        End Function
    End Class
End Namespace
