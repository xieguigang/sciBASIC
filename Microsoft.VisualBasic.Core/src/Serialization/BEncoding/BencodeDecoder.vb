#Region "Microsoft.VisualBasic::3c3fafb58c18003f3a50ef9407b792eb, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BencodeDecoder.vb"

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

    '   Total Lines: 179
    '    Code Lines: 115 (64.25%)
    ' Comment Lines: 26 (14.53%)
    '    - Xml Docs: 30.77%
    ' 
    '   Blank Lines: 38 (21.23%)
    '     File Size: 5.67 KB


    '     Module BencodeDecoder
    ' 
    '         Function: (+2 Overloads) [Error], Decode, DecodeObject, ReadDictionary, ReadElement
    '                   ReadInteger, ReadList, ReadString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ***
'  Encoding usage:
'  
'  new BDictionary()
'  {
'   {"Some Key", "Some Value"},
'   {"Another Key", 42}
'  }.ToBencodedString();
'  
'  Decoding usage:
'  
'  BencodeDecoder.Decode("d8:Some Key10:Some Value13:Another Valuei42ee");
'  
'  Feel free to use it.
'  More info about Bencoding at http://wiki.theory.org/BitTorrentSpecification#bencoding
'  
'  Originally posted at http://snipplr.com/view/37790/ by SuprDewd.
'  

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A class used for decoding Bencoding.
    ''' </summary>
    Public Module BencodeDecoder

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DecodeObject(bencodedString As String) As BDictionary
            Return DirectCast(Decode(bencodedString).First, BDictionary)
        End Function

        ''' <summary>
        ''' Decodes the string.
        ''' </summary>
        ''' <param name="bencodedString">The bencoded string.</param>
        ''' <returns>An array of root elements.</returns>
        Public Function Decode(bencodedString As String) As BElement()
            Dim index = 0

            If bencodedString.StringEmpty Then
                Return Nothing
            Else
                bencodedString = bencodedString.Trim(" "c, ASCII.CR, ASCII.LF, ASCII.TAB)
            End If

            Try
                Dim rootElements As New List(Of BElement)()

                While bencodedString.Length > index
                    rootElements.Add(ReadElement(bencodedString, index))
                End While

                Return rootElements.ToArray()
            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function ReadElement(ByRef bencodedString As String, ByRef index As Integer) As BElement
            Select Case bencodedString(index)
                Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c
                    Return ReadString(bencodedString, index)
                Case "i"c
                    Return ReadInteger(bencodedString, index)
                Case "l"c
                    Return ReadList(bencodedString, index)
                Case "d"c
                    Return ReadDictionary(bencodedString, index)
                Case Else
                    Throw [Error]()
            End Select
        End Function

        Private Function ReadDictionary(ByRef bencodedString As String, ByRef index As Integer) As BDictionary
            Dim dict As New BDictionary()

            index += 1

            Try

                While bencodedString(index) <> "e"c
                    Dim K = ReadString(bencodedString, index)
                    Dim V = ReadElement(bencodedString, index)

                    Call dict.Add(K, V)
                End While

            Catch unusedBencodingException As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1

            Return dict
        End Function

        Private Function ReadList(ByRef bencodedString As String, ByRef index As Integer) As BList
            Dim lst As New BList()

            index += 1

            Try

                While bencodedString(index) <> "e"c
                    lst.Add(ReadElement(bencodedString, index))
                End While

            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1

            Return lst
        End Function

        Private Function ReadInteger(ByRef bencodedString As String, ByRef index As Integer) As BInteger
            Dim [end] As Integer

            index += 1
            [end] = bencodedString.IndexOf("e"c, index)

            If [end] = -1 Then
                Throw [Error]()
            End If

            Dim [integer] As Long

            Try
                [integer] = Convert.ToInt64(bencodedString.Substring(index, [end] - index))
                index = [end] + 1
            Catch e As Exception
                Throw [Error](e)
            End Try

            Return New BInteger([integer])
        End Function

        Private Function ReadString(ByRef bencodedString As String, ByRef index As Integer) As BString
            Dim length, colon As Integer

            Try
                colon = bencodedString.IndexOf(":"c, index)
                If colon = -1 Then Throw [Error]()
                length = Convert.ToInt32(bencodedString.Substring(index, colon - index))
            Catch e As Exception
                Throw [Error](e)
            End Try

            index = colon + 1
            Dim tmpIndex = index
            index += length

            Try
                Return New BString(bencodedString.Substring(tmpIndex, length))
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function [Error](e As Exception) As Exception
            Return New BencodingException("Bencoded string invalid.", e)
        End Function

        Private Function [Error]() As Exception
            Return New BencodingException("Bencoded string invalid.")
        End Function
    End Module

End Namespace
