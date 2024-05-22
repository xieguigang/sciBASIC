#Region "Microsoft.VisualBasic::213f0f7bf9ac6df3a53829f31c80cfdf, mime\application%pdf\PdfReader\Tokenizer\Tokenizer.vb"

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

    '   Total Lines: 680
    '    Code Lines: 488 (71.76%)
    ' Comment Lines: 57 (8.38%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 135 (19.85%)
    '     File Size: 26.70 KB


    '     Class Tokenizer
    ' 
    '         Properties: AllowIdentifiers, IgnoreComments, Position, Reader
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ConvertDecimalToInteger, ConvertDecimalToLong, GetAnyToken, GetBytes, GetComment
    '                   GetDictionaryClose, GetDictionaryOpenOrHexString, GetKeywordOrIdentifier, GetName, GetNumber
    '                   GetStringLiteral, GetStringLiteralUTF16, GetToken, GetXRefEntry, GetXRefOffset
    '                   GotoNextLine
    ' 
    '         Sub: (+2 Overloads) Dispose, PushToken, SkipWhitespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports stdNum = System.Math

Namespace PdfReader
    Public Class Tokenizer
        Private Const EOF_SCAN_LENGTH As Integer = 1024

        ' Lookup arrays are fast and small because the source is ASCII characters, so limimted to a possible 256 values
        Private Shared _lookupWhitespace As Boolean()
        Private Shared _lookupDelimiter As Boolean()
        Private Shared _lookupDelimiterWhitespace As Boolean()
        Private Shared _lookupHexToDecimal As Integer()
        Private Shared _lookupHexadecimal As Boolean()
        Private Shared _lookupHexadecimalWhitespace As Boolean()
        Private Shared _lookupIsNumeric As Boolean()
        Private Shared _lookupIsNumericStart As Boolean()
        Private Shared _lookupKeyword As Boolean()
        Private Shared ReadOnly _whitespace As Byte() = New Byte() {0, 9, 10, 12, 13, 32}
        '                                                       \0  \t \n  \f  \r  (SPACE)

        Private Shared ReadOnly _delimiter As Byte() = New Byte() {40, 41, 60, 62, 91, 93, 123, 125, 47, 37}
        '                                                       (   )   <   >   [   ]   {    }    /   %

        Private Shared ReadOnly _hexadecimal As Byte() = New Byte() {48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70, 97, 98, 99, 100, 101, 102}
        '                                                         0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F   a   b   c   d    e    f

        Private Shared ReadOnly _isNumeric As Byte() = New Byte() {48, 49, 50, 51, 52, 53, 54, 55, 56, 57}
        '                                                       0   1   2   3   4   5   6   7   8   9

        Private Shared ReadOnly _isNumericStart As Byte() = New Byte() {48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 43, 45, 46}
        '                                                            0   1   2   3   4   5   6   7   8   9   +   -   .

        Private Shared ReadOnly EOF_COMMENT As Byte() = New Byte() {70, 79, 69, 37, 37}
        '                                                        F   O   E   %   %

        Private _index As Integer
        Private _length As Integer
        Private _position As Long
        Private _line As Byte()
        Private _disposed As Boolean
        Private _reader As TokenReader
        Private _cachedReader As TokenReader
        Private _stack As Stack(Of TokenObject) = New Stack(Of TokenObject)()

        Shared Sub New()
            _lookupWhitespace = New Boolean(255) {}

            For Each code In _whitespace
                _lookupWhitespace(code) = True
            Next

            _lookupDelimiter = New Boolean(255) {}

            For Each code In _delimiter
                _lookupDelimiter(code) = True
            Next

            _lookupHexadecimal = New Boolean(255) {}

            For Each code In _hexadecimal
                _lookupHexadecimal(code) = True
            Next

            _lookupIsNumeric = New Boolean(255) {}

            For Each code In _isNumeric
                _lookupIsNumeric(code) = True
            Next

            _lookupIsNumericStart = New Boolean(255) {}

            For Each code In _isNumericStart
                _lookupIsNumericStart(code) = True
            Next

            _lookupHexadecimalWhitespace = New Boolean(255) {}

            For Each code In _whitespace
                _lookupHexadecimalWhitespace(code) = True
            Next

            For Each code In _hexadecimal
                _lookupHexadecimalWhitespace(code) = True
            Next

            _lookupDelimiterWhitespace = New Boolean(255) {}

            For Each code In _whitespace
                _lookupDelimiterWhitespace(code) = True
            Next

            For Each code In _delimiter
                _lookupDelimiterWhitespace(code) = True
            Next

            _lookupHexToDecimal = New Integer(255) {}

            For i = 0 To 10 - 1
                _lookupHexToDecimal(48 + i) = i
            Next    ' '0' + i

            For i = 0 To 6 - 1
                _lookupHexToDecimal(65 + i) = i    ' 'a' + i
                _lookupHexToDecimal(97 + i) = i    ' 'A' + i
            Next

            _lookupKeyword = New Boolean(255) {}

            For i = 65 To 90  ' 'a' -> 'z'
                _lookupKeyword(i) = True
            Next

            For i = 97 To 122 ' 'A' -> 'Z'
                _lookupKeyword(i) = True
            Next
        End Sub

        Public Sub New(stream As Stream)
            ' Must have an actual stream reference
            If stream Is Nothing Then
                Throw New ArgumentNullException("stream")
            Else
                Me.Stream = stream
            End If

            ' Stream is no use if we cannot read from it!
            If Not stream.CanRead Then Throw New ApplicationException("Cannot read from stream.")

            ' Stream must be able to be randomly positioned
            If Not stream.CanSeek Then Throw New ApplicationException("Cannot seek within stream.")
        End Sub

        Public Sub Dispose()
            Dispose(True)
        End Sub

        Public Property AllowIdentifiers As Boolean = False
        Public Property IgnoreComments As Boolean = True

        Public Property Position As Long
            Get
                ' The Reader class keeps track of the actual cursor position when buffering data
                If Reader IsNot Nothing Then
                    Return Reader.Position
                Else
                    Return Stream.Position
                End If
            End Get
            Set(value As Long)
                ' Clear any cached bytes
                _reader = Nothing
                _line = Nothing

                ' Move stream to desired offset
                Stream.Position = value
            End Set
        End Property

        Public Sub PushToken(token As TokenObject)
            _stack.Push(token)
        End Sub

        Public Function GetToken() As TokenObject
            Dim t As TokenObject = Nothing

            If _stack.Count > 0 Then
                t = _stack.Pop()
            Else
                t = GetAnyToken()
            End If

            If IgnoreComments Then
                While TypeOf t Is TokenComment

                    If _stack.Count > 0 Then
                        t = _stack.Pop()
                    Else
                        t = GetAnyToken()
                    End If
                End While
            End If

            Return t
        End Function

        Public Function GotoNextLine() As Boolean
            _position = Reader.Position
            Dim splice As TokenByteSplice = Reader.ReadLine()
            _line = splice.Bytes

            If _line IsNot Nothing Then
                _length = splice.Start + splice.Length
                _index = splice.Start
                Return True
            Else
                Return False
            End If
        End Function

        Public Function GetBytes(length As Integer) As Byte()
            Return Reader.GetBytes(length)
        End Function

        Public Function GetXRefEntry(id As Integer) As TokenObject
            ' Ignore zero or more whitespace characters
            SkipWhitespace()
            If _length - _index < 18 Then Return New TokenError(_position, $"Cross-reference entry data is {_length - _index} bytes instead of the expected 18.")
            Dim ret As TokenXRefEntry = New TokenXRefEntry(id, ConvertDecimalToInteger(11, 5), ConvertDecimalToInteger(0, 10), _line(_index + 17) = 110)       ' 'n' = used, otherwise free
            _index = _length
            Return ret
        End Function

        Public Function GetXRefOffset() As Long
            ' We expect the stream to be at least a certain minimal size
            If Stream.Length < EOF_SCAN_LENGTH Then Throw New ApplicationException($"Stream must be at least {EOF_SCAN_LENGTH} bytes.")

            ' Load bytes from the end, enough to discover the location of the 'xref' section
            Dim bytes = New Byte(1023) {}
            Stream.Position = Stream.Length - bytes.Length
            If Stream.Read(bytes, 0, bytes.Length) <> bytes.Length Then Throw New ApplicationException($"Failed to read in last {EOF_SCAN_LENGTH} bytes of stream.")

            ' Start scanning backwards from the end
            Dim index = bytes.Length - 1

            ' Find the %%EOF comment
            Dim match = 0

            While index > 0

                If EOF_COMMENT(match) = bytes(index) Then
                    match += 1

                    If match = EOF_COMMENT.Length Then
                        index -= 1
                        Exit While
                    End If
                ElseIf EOF_COMMENT(0) = bytes(index) Then
                    match = 1
                Else
                    match = 0
                End If

                index -= 1
            End While

            If index = 0 Then Throw New ApplicationException($"Could not find %%EOF comment at end of the stream.")

            ' Skip any whitespace
            While _lookupWhitespace(bytes(index))
                index -= 1
            End While

            If index = 0 Then Throw New ApplicationException($"Could not find offset of the cross-reference table.")
            Dim [end] = index

            While _lookupIsNumericStart(bytes([end]))
                [end] -= 1
            End While

            If index = 0 Then Throw New ApplicationException($"Could not find offset of the cross-reference table.")
            Return ConvertDecimalToLong(bytes, [end] + 1, index - [end])
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    _reader = Nothing
                    _cachedReader = Nothing
                    Stream.Dispose()
                    Stream = Nothing
                End If

                _disposed = True
            End If
        End Sub

        Private ReadOnly Property Reader As TokenReader
            Get

                If _reader Is Nothing Then
                    If _cachedReader Is Nothing Then
                        _reader = New TokenReader(Stream)
                        _cachedReader = _reader
                    Else
                        _reader = _cachedReader
                        _reader.Reset()
                    End If
                End If

                Return _reader
            End Get
        End Property

        Private Function ConvertDecimalToInteger(start As Integer, length As Integer) As Integer
            Dim ret = 0
            Dim index = _index + start

            For i = 0 To length - 1
                ret *= 10
                ret += _line(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) - 48 ' '0'
            Next

            Return ret
        End Function

        Private Function ConvertDecimalToLong(bytes As Byte(), start As Integer, length As Integer) As Long
            Dim ret As Long = 0
            Dim index = start

            For i = 0 To length - 1
                ret *= 10
                ret += bytes(stdNum.Min(Threading.Interlocked.Increment(index), index - 1)) - 48 ' '0'
            Next

            Return ret
        End Function

        Private Sub SkipWhitespace()
            While True
                ' Do we need to fetch the next line of characters?
                If _line Is Nothing OrElse _index = _length Then
                    _position = Reader.Position
                    Dim splice As TokenByteSplice = Reader.ReadLine()
                    _line = splice.Bytes

                    If _line IsNot Nothing Then
                        _length = splice.Start + splice.Length
                        _index = splice.Start
                    Else
                        ' No more lines, finished
                        Exit While
                    End If
                End If

                ' Skip all whitespace characters
                While _index < _length

                    If _lookupWhitespace(_line(_index)) Then
                        _index += 1
                    Else
                        Return
                    End If
                End While
            End While
        End Sub

        Private Function GetAnyToken() As TokenObject
            ' Ignore zero or more whitespace characters
            SkipWhitespace()

            ' Have we run out of content?
            If _line Is Nothing OrElse _index = _length Then
                Return TokenObject.Empty
            Else

                If _lookupIsNumericStart(_line(_index)) Then
                    Return GetNumber()
                ElseIf Not _lookupDelimiter(_line(_index)) Then
                    Return GetKeywordOrIdentifier()
                Else

                    Select Case _line(_index)
                        Case 37 ' '%'
                            Return GetComment()
                        Case 47 ' '/'
                            Return GetName()
                        Case 60 ' '<'
                            Return GetDictionaryOpenOrHexString()
                        Case 62 ' '>'
                            Return GetDictionaryClose()
                        Case 40 '  '('
                            Return GetStringLiteral()
                        Case 91 ' '['
                            _index += 1
                            Return TokenObject.ArrayOpen
                        Case 93 ' ']'
                            _index += 1
                            Return TokenObject.ArrayClose
                    End Select

                    ' Found invalid character for this position
                    Return New TokenError(_position + _index, $"Cannot parse '{_line(_index)}' as a delimiter or regular character.")
                End If
            End If
        End Function

        Private Function GetNumber() As TokenObject
            Dim position = _position + _index
            Dim positive = True
            Dim start = _index
            Dim current = _line(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1))

            ' Check for sign
            If current = 43 Then ' '+'
                If _index < _length Then
                    current = _line(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1))
                Else
                    Return New TokenError(position, $"Cannot parse number because unexpected end-of-line encountered after '+'.")
                End If
            ElseIf current = 45 Then ' '-'
                positive = False

                If _index < _length Then
                    current = _line(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1))
                Else
                    Return New TokenError(position, $"Cannot parse number because unexpected end-of-line encountered after '-'.")
                End If
            End If

            ' Convert whole number part
            Dim whole = 0

            While True

                If _lookupIsNumeric(current) Then
                    whole = whole * 10 + (current - 48)

                    If _index < _length Then
                        current = _line(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1))
                    Else
                        Return New TokenInteger(If(positive, whole, -whole))
                    End If
                Else
                    Exit While
                End If
            End While

            ' Is there is no fractional part then it must be an integer
            If current <> 46 Then
                _index -= 1
                Return New TokenInteger(If(positive, whole, -whole))
            End If

            If _index < _length Then
                current = _line(stdNum.Min(Threading.Interlocked.Increment(_index), _index - 1))
            Else
                Return New TokenReal(If(positive, whole, -whole))
            End If

            ' Find end of the fractional part
            While _index < _length AndAlso _lookupIsNumeric(_line(_index))
                _index += 1
            End While

            Dim text = Encoding.ASCII.GetString(_line, start, _index - start)
            Dim convert As Single = Nothing
            If Single.TryParse(text, convert) Then Return New TokenReal(convert)

            ' String is not a recognized number format
            Return New TokenError(position, $"Cannot parse text as a real number.")
        End Function

        Private Function GetKeywordOrIdentifier() As TokenObject
            Dim position = _position + _index

            ' Scan looking for the end of the keyword characters
            Dim key = _index

            While key < _length AndAlso _lookupKeyword(_line(key))
                key += 1
            End While

            Dim text = Encoding.ASCII.GetString(_line, _index, key - _index)
            Dim token = TokenKeyword.GetToken(text)

            If token IsNot Nothing Then
                _index = key
                Return token
            End If

            If AllowIdentifiers Then
                ' Scan looking for the end of the identifier
                key = _index

                While key < _length AndAlso Not _lookupDelimiterWhitespace(_line(key))
                    key += 1
                End While

                text = Encoding.ASCII.GetString(_line, _index, key - _index)
                _index = key
                Return New TokenIdentifier(text)
            Else
                Return New TokenError(position, $"Cannot parse '{text}' as a keyword.")
            End If
        End Function

        Private Function GetComment() As TokenObject
            Dim position = _position + _index

            ' Everything till end of the line is the comment
            Dim comment = Encoding.ASCII.GetString(_line, _index, _length - _index)

            ' Continue processing at start of the next line
            _line = Nothing
            Return New TokenComment(comment)
        End Function

        Private Function GetName() As TokenObject
            Dim position = _position + _index

            ' Find the run of regular characters
            Dim [end] = _index + 1

            While [end] < _length AndAlso Not _lookupDelimiterWhitespace(_line([end]))
                [end] += 1
            End While

            Dim name = Encoding.ASCII.GetString(_line, _index + 1, [end] - _index - 1)
            _index = [end]

            ' Convert any escape sequences
            While True
                Dim escape = name.IndexOf("#"c)
                If escape < 0 Then Exit While

                ' Check there are two digits after it
                If escape > name.Length - 3 OrElse Not _lookupHexadecimal(Microsoft.VisualBasic.AscW(name(escape + 1))) OrElse Not _lookupHexadecimal(Microsoft.VisualBasic.AscW(name(escape + 2))) Then Return New TokenError(position + escape, $"Escaped character inside name is not followed by two hex digits.")
                Dim val As Char = Microsoft.VisualBasic.ChrW(_lookupHexToDecimal(Microsoft.VisualBasic.AscW(name(escape + 1))) * 16 + _lookupHexToDecimal(Microsoft.VisualBasic.AscW(name(escape + 2))))
                name = name.Replace(name.Substring(escape, 3), $"{val}")
            End While

            Return TokenName.GetToken(name)
        End Function

        Private Function GetDictionaryOpenOrHexString() As TokenObject
            Dim position = _position + _index
            _index += 1
            If _index >= _length Then Return New TokenError(position, $"Unexpected end of line after '<'.")

            Const byteLt As Byte = Asc("<")
            Const byteGt As Byte = Asc(">")

            ' Is the next character another '<'
            If _line(_index) = byteLt Then
                _index += 1
                Return TokenObject.DictionaryOpen
            Else
                ' Find the run of hexadecimal characters and whitespace
                Dim [end] = _index

                While [end] < _length AndAlso _lookupHexadecimalWhitespace(_line([end]))
                    [end] += 1
                End While

                If [end] = _length Then Return New TokenError(position, $"Missing '>' at end of hexadecimal string.")
                If _line([end]) <> byteGt Then Return New TokenError(position, $"Invalid character '{_line([end])}' found in hexadecimal string.")
                Dim str = Encoding.ASCII.GetString(_line, _index, [end] - _index)
                _index = [end] + 1
                Return New TokenStringHex(str)
            End If
        End Function

        Private Function GetDictionaryClose() As TokenObject
            Dim position = _position + _index

            ' Check the next character is also a '>'
            If _index + 1 < _length AndAlso _line(_index + 1) = 62 Then
                _index += 2
                Return TokenObject.DictionaryClose
            Else
                _index += 1
                Return New TokenError(position, $"Missing '>' after the initial '>'.")
            End If
        End Function

        Private Function GetStringLiteral() As TokenObject
            Dim position = _position + _index

            ' Move past the '(' start literal string marker
            _index += 1
            Dim nesting = 0
            Dim first = _index
            Dim scanned = 0
            Dim continuation = False
            Dim checkedBOM = False
            Dim sb As StringBuilder = New StringBuilder()

            ' Keep scanning until we get to the end of literal string ')' marker
            While True
                ' Scan rest of the current line
                While _index < _length
                    ' Is this the start of an escape sequence
                    If _line(_index) = 92 Then    ' '\'
                        ' If the last character, then '\' indicates that no newline should be appended into the literal string
                        If _index >= _length - 1 Then
                            continuation = True
                        Else
                            ' Skip over the following escaped character for first digit of escaped number
                            _index += 1
                        End If
                    ElseIf _line(_index) = 41 Then   ' ')'
                        ' If the balancing end marker then we are finished
                        If nesting = 0 Then
                            sb.Append(Encoding.ASCII.GetString(_line, first, _index - first))

                            ' Move past the ')' marker
                            _index += 1
                            Return New TokenStringLiteral(sb.ToString())
                        Else
                            nesting -= 1
                        End If
                    ElseIf _line(_index) = 40 Then   ' '('
                        nesting += 1
                    End If

                    _index += 1
                    scanned += 1

                    If Not checkedBOM AndAlso scanned = 2 Then
                        ' Check for the UTF16 Byte Order Mark (little endian or big endian versions)
                        If _line(_index - 2) = &HFE AndAlso _line(_index - 1) = &HFF Then
                            Return GetStringLiteralUTF16(position, True)
                        ElseIf _line(_index - 2) = &HFF AndAlso _line(_index - 1) = &HFE Then
                            Return GetStringLiteralUTF16(position, False)
                        End If
                    End If
                End While

                checkedBOM = True

                If continuation Then
                    ' Append everything from the first character
                    sb.Append(Encoding.ASCII.GetString(_line, first, _index - first - 1))
                Else
                    ' Append everything from the first character but excluding the continuation marker
                    sb.Append(Encoding.ASCII.GetString(_line, first, _index - first))
                    sb.Append(vbLf)
                End If

                Dim splice As TokenByteSplice = Reader.ReadLine()
                _line = splice.Bytes

                If _line IsNot Nothing Then
                    _length = splice.Start + splice.Length
                    _index = splice.Start
                    first = _index
                    continuation = False
                Else
                    ' End of content before end of string literal
                    Return New TokenError(position, $"End of content before end of literal string character ')'.")
                End If
            End While
        End Function

        Private Function GetStringLiteralUTF16(position As Long, bigEndian As Boolean) As TokenObject
            Dim first = _index
            Dim temp As Byte

            ' Scan rest of the current line
            While _index < _length
                ' Is this the endof the literal
                If _line(_index) = 41 Then    ' ')'
                    Dim literal = Encoding.Unicode.GetString(_line, first, _index - first)

                    ' Move past the ')' marker
                    _index += 1
                    Return New TokenStringLiteral(literal)
                End If

                _index += 2

                If bigEndian AndAlso _index < _length Then
                    ' Switch byte order of each character pair
                    temp = _line(_index - 1)
                    _line(_index - 1) = _line(_index - 2)
                    _line(_index - 2) = temp
                End If
            End While

            ' End of content before end of string literal
            Return New TokenError(position, $"End of content before end of UTF16 literal string character.")
        End Function

        Private Stream As Stream

    End Class
End Namespace
