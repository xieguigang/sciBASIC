#Region "Microsoft.VisualBasic::c7deacbacbe45ff501aee967090fc664, Microsoft.VisualBasic.Core\src\Text\IO\EncodingHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 95
    '    Code Lines: 18
    ' Comment Lines: 68
    '   Blank Lines: 9
    '     File Size: 3.62 KB


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
