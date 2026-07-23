#Region "Microsoft.VisualBasic::a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6, mime\application%json\LenientParser\LenientJsonParser.vb"

    ' Author:
    '
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) 2026 GPL3 Licensed
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



    ' /*******************************************************************************/

    ' Summaries:

    ' Code Statistics:
    '
    '   Total Lines: ~650
    '    Code Lines: ~550
    ' Comment Lines: ~60
    '
    '   Class LenientJsonParser
    '
    '       Constructor: (+1 Overload) Sub New
    '       Function: Parse, ParseJSON, Open, OpenStream
    '                 parse_value, parse_object, parse_array, parse_string
    '                 parse_number, parse_keyword, parse_key
    '                 skip_whitespace_and_comments, skip_line_comment
    '                 skip_block_comment, check_infinity, is_hex_digit
    '                 is_likely_closing_quote
    '
    ' /*******************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace LenientJson

    ''' <summary>
    ''' A lenient (fault-tolerant) JSON parser designed to automatically repair
    ''' and parse JSON output produced by Large Language Models (LLMs).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This parser implements a "Lenient Recursive Descent Parser" strategy
    ''' inspired by the json-repair library. Instead of parsing first and then
    ''' repairing, it tolerates errors during the parsing process itself by
    ''' scanning the input character-by-character with a cursor.
    ''' </para>
    ''' <para>
    ''' The following 15 repair strategies are implemented:
    ''' </para>
    ''' <list type="number">
    ''' <item><description>Skip comments (// line and /* block */ comments)</description></item>
    ''' <item><description>Ignore extra content after the first JSON value</description></item>
    ''' <item><description>Accept single-quoted strings ('...')</description></item>
    ''' <item><description>Convert Infinity to null</description></item>
    ''' <item><description>Skip unrecognized characters</description></item>
    ''' <item><description>Auto-close unclosed objects (truncation repair)</description></item>
    ''' <item><description>Skip leading/trailing commas</description></item>
    ''' <item><description>Tolerate missing colons in objects</description></item>
    ''' <item><description>Accept unquoted keys (JavaScript-style)</description></item>
    ''' <item><description>Accept unescaped control characters in strings</description></item>
    ''' <item><description>Auto-close unclosed strings (truncation repair)</description></item>
    ''' <item><description>Accept leading + sign in numbers</description></item>
    ''' <item><description>Convert NaN to null</description></item>
    ''' <item><description>Partial keyword matching (tru -> true, etc.)</description></item>
    ''' <item><description>Smart quote closure detection — when a quote is encountered inside a string, lookahead to check if the next non-whitespace character is a structural character (, } ] : or EOF) before treating it as the closing quote; otherwise keep it as an internal unescaped quote</description></item>
    ''' </list>
    ''' </remarks>
    Public Class LenientJsonParser

#Region "Private fields"

        ''' <summary>
        ''' The input JSON text (possibly malformed) to be parsed.
        ''' </summary>
        Private ReadOnly m_input As String

        ''' <summary>
        ''' Current cursor position in the input string.
        ''' </summary>
        Private m_index As Integer

        ''' <summary>
        ''' Total length of the input string.
        ''' </summary>
        Private ReadOnly m_length As Integer

#End Region

#Region "Constructor"

        ''' <summary>
        ''' Create a new lenient JSON parser for the given text.
        ''' </summary>
        ''' <param name="jsonText">
        ''' The JSON text (possibly malformed) to parse. If Nothing or empty,
        ''' the parser will return Nothing from <see cref="Parse"/>.
        ''' </param>
        Public Sub New(jsonText As String)
            If jsonText Is Nothing Then
                m_input = ""
            Else
                m_input = jsonText
            End If
            m_length = m_input.Length
            m_index = 0
        End Sub

#End Region

#Region "Public API"

        ''' <summary>
        ''' Parse the JSON text and return the root <see cref="JsonElement"/>.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="JsonElement"/> representing the parsed JSON value,
        ''' or <c>Nothing</c> if the input is empty or contains no recognizable
        ''' JSON value.
        ''' </returns>
        Public Function Parse() As JsonElement
            ' Strategy 2: Ignore extra content after the first JSON value.
            ' LLMs often append explanatory text after the JSON, e.g.:
            '   "Here is the JSON: {...} Hope this helps!"
            ' We parse the first complete JSON value and ignore the rest.
            Return parse_value(stop_at_structural:=False)
        End Function

        ''' <summary>
        ''' Convenience shared method: parse a JSON string leniently.
        ''' </summary>
        ''' <param name="json">The JSON text to parse</param>
        ''' <returns>The parsed <see cref="JsonElement"/>, or Nothing if input is empty.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseJSON(json As String) As JsonElement
            If String.IsNullOrEmpty(json) Then
                Return Nothing
            End If
            Return New LenientJsonParser(json).Parse()
        End Function

        ''' <summary>
        ''' Open and parse a JSON file leniently.
        ''' </summary>
        ''' <param name="file">Path to the JSON file</param>
        ''' <returns>The parsed <see cref="JsonElement"/>, or Nothing if file is empty.</returns>
        Public Shared Function Open(file As String) As JsonElement
            If String.IsNullOrEmpty(file) OrElse Not System.IO.File.Exists(file) Then
                Return Nothing
            End If
            Dim text As String = System.IO.File.ReadAllText(file)
            Return ParseJSON(text)
        End Function

        ''' <summary>
        ''' Parse JSON text from a stream leniently.
        ''' </summary>
        ''' <param name="stream">The stream containing JSON text</param>
        ''' <returns>The parsed <see cref="JsonElement"/>, or Nothing if stream is empty.</returns>
        Public Shared Function OpenStream(stream As Stream) As JsonElement
            If stream Is Nothing Then
                Return Nothing
            End If
            Using reader As New StreamReader(stream)
                Dim text As String = reader.ReadToEnd()
                Return ParseJSON(text)
            End Using
        End Function

#End Region

#Region "Cursor helpers"

        ''' <summary>
        ''' Returns True if the cursor has reached the end of input.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function at_end() As Boolean
            Return m_index >= m_length
        End Function

        ''' <summary>
        ''' Returns the character at the current cursor position without advancing.
        ''' Returns ChrW(0) if at end of input (caller should check at_end() first).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function peek() As Char
            If m_index < m_length Then
                Return m_input(m_index)
            End If
            Return ChrW(0)
        End Function

        ''' <summary>
        ''' Returns the character at the given offset from the current cursor.
        ''' Returns ChrW(0) if the offset is out of range.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function peek(offset As Integer) As Char
            Dim pos As Integer = m_index + offset
            If pos >= 0 AndAlso pos < m_length Then
                Return m_input(pos)
            End If
            Return ChrW(0)
        End Function

#End Region

#Region "Skip methods (Strategy 1: comments, Strategy 5: unrecognized chars)"

        ''' <summary>
        ''' Skip whitespace characters and comments (// line comments and
        ''' /* block comments */). This implements Strategy 1.
        ''' </summary>
        Private Sub skip_whitespace_and_comments()
            Do While Not at_end()
                Dim c As Char = peek()

                If Char.IsWhiteSpace(c) Then
                    ' Skip whitespace (space, tab, newline, etc.)
                    m_index += 1
                ElseIf c = "/"c AndAlso peek(1) = "/"c Then
                    ' Strategy 1: Skip single-line comment //
                    m_index += 2
                    skip_line_comment()
                ElseIf c = "/"c AndAlso peek(1) = "*"c Then
                    ' Strategy 1: Skip block comment /* ... */
                    m_index += 2
                    skip_block_comment()
                ElseIf c = "#"c Then
                    ' Bonus: Skip Python-style # comment (some LLMs use this)
                    m_index += 1
                    skip_line_comment()
                Else
                    Exit Do
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Skip to the end of the current line (for // and # comments).
        ''' </summary>
        Private Sub skip_line_comment()
            Do While Not at_end()
                Dim c As Char = peek()
                If c = ControlChars.Lf OrElse c = ControlChars.Cr Then
                    Exit Do
                End If
                m_index += 1
            Loop
        End Sub

        ''' <summary>
        ''' Skip a block comment /* ... */. If the block comment is unclosed
        ''' (truncated), skip to the end of input. This implements the
        ''' "unclosed block comment" part of Strategy 1.
        ''' </summary>
        Private Sub skip_block_comment()
            Do While Not at_end()
                Dim c As Char = peek()
                m_index += 1
                If c = "*"c AndAlso Not at_end() AndAlso peek() = "/"c Then
                    ' Found closing */
                    m_index += 1
                    Exit Do
                End If
            Loop
            ' If we reach here without finding */, the comment was truncated.
            ' We simply skip to EOF, which is the lenient behavior.
        End Sub

#End Region

#Region "Parse value (main dispatcher)"

        ''' <summary>
        ''' Parse a JSON value (object, array, string, number, boolean, or null).
        ''' This is the main dispatcher that examines the current character and
        ''' delegates to the appropriate sub-parser.
        ''' </summary>
        ''' <param name="stop_at_structural">
        ''' If True, stop and return null when encountering a structural character
        ''' (comma, closing brace, closing bracket). This is used inside objects
        ''' and arrays to handle missing values gracefully.
        ''' </param>
        ''' <returns>The parsed <see cref="JsonElement"/>, or Nothing if no value found.</returns>
        Private Function parse_value(stop_at_structural As Boolean) As JsonElement
            Do
                ' Skip whitespace and comments before each attempt
                skip_whitespace_and_comments()

                If at_end() Then
                    ' Reached end of input without finding a value
                    Return Nothing
                End If

                Dim c As Char = peek()

                ' If we're inside an object/array and hit a structural char,
                ' the value is missing — return null.
                If stop_at_structural AndAlso (c = ","c OrElse c = "}"c OrElse c = "]"c) Then
                    Return JsonValue.NULL
                End If

                If c = "{"c Then
                    ' Object
                    Return parse_object()
                ElseIf c = "["c Then
                    ' Array
                    Return parse_array()
                ElseIf c = """"c OrElse c = "'"c Then
                    ' Strategy 3: Accept both double-quoted and single-quoted strings
                    Dim s As String = parse_string()
                    Return New JsonValue(s, alreadyDecoded:=True)
                ElseIf c = "-"c OrElse c = "+"c Then
                    ' Strategy 12: Accept leading + sign
                    ' Strategy 4: Convert Infinity/-Infinity/+Infinity to null
                    If check_infinity() Then
                        Return JsonValue.NULL
                    End If
                    Return parse_number()
                ElseIf Char.IsDigit(c) Then
                    ' Number
                    Return parse_number()
                ElseIf Char.IsLetter(c) Then
                    ' Could be a keyword (true, false, null, NaN, Infinity)
                    ' or just unrecognized text
                    Dim result As JsonElement = parse_keyword()
                    If result IsNot Nothing Then
                        Return result
                    End If
                    ' parse_keyword returned Nothing: the text was not a keyword.
                    ' parse_keyword already consumed all consecutive letters.
                    ' Continue the loop to skip remaining unrecognized chars.
                Else
                    ' Strategy 5: Skip unrecognized character and continue
                    m_index += 1
                End If
            Loop
        End Function

#End Region

#Region "Parse object (Strategy 6, 7, 8, 9)"

        ''' <summary>
        ''' Parse a JSON object { "key": value, ... }.
        ''' Implements:
        ''' - Strategy 6: Auto-close unclosed objects at EOF (truncation repair)
        ''' - Strategy 7: Skip trailing commas
        ''' - Strategy 8: Tolerate missing colons
        ''' - Strategy 9: Accept unquoted keys
        ''' </summary>
        Private Function parse_object() As JsonElement
            Dim obj As New JsonObject()

            ' Consume opening {
            m_index += 1

            Do
                skip_whitespace_and_comments()

                If at_end() Then
                    ' Strategy 6: Object not closed (truncated) — auto-close at EOF
                    Exit Do
                End If

                Dim c As Char = peek()

                If c = "}"c Then
                    ' Normal end of object
                    m_index += 1
                    Exit Do
                End If

                ' Strategy 7: Skip leading/trailing commas
                If c = ","c Then
                    m_index += 1
                    Continue Do
                End If

                ' Parse the key (Strategy 9: may be unquoted)
                Dim key As String = parse_key()

                skip_whitespace_and_comments()

                If at_end() Then
                    ' Strategy 6: Truncated after key — add key with null value
                    obj(key) = JsonValue.NULL
                    Exit Do
                End If

                ' Strategy 8: Tolerate missing colon
                ' If there's a colon, consume it. If not, just continue.
                c = peek()
                If c = ":"c Then
                    m_index += 1
                End If

                ' Parse the value (stop at structural chars to handle missing values)
                Dim value As JsonElement = parse_value(stop_at_structural:=True)

                If value Is Nothing Then
                    value = JsonValue.NULL
                End If

                ' Add key-value pair (duplicate keys silently overwrite)
                obj(key) = value

                skip_whitespace_and_comments()

                If at_end() Then
                    ' Strategy 6: Truncated after value — auto-close
                    Exit Do
                End If

                c = peek()

                If c = ","c Then
                    ' Strategy 7: Skip comma (may be trailing, loop will handle it)
                    m_index += 1
                ElseIf c = "}"c Then
                    ' Normal end of object
                    m_index += 1
                    Exit Do
                Else
                    ' Unexpected character after value — skip it and continue
                    ' (lenient behavior: try to recover)
                    m_index += 1
                End If
            Loop

            Return obj
        End Function

#End Region

#Region "Parse array (Strategy 6, 7)"

        ''' <summary>
        ''' Parse a JSON array [ value, value, ... ].
        ''' Implements:
        ''' - Strategy 6: Auto-close unclosed arrays at EOF (truncation repair)
        ''' - Strategy 7: Skip trailing commas
        ''' </summary>
        Private Function parse_array() As JsonElement
            Dim arr As New JsonArray()

            ' Consume opening [
            m_index += 1

            Do
                skip_whitespace_and_comments()

                If at_end() Then
                    ' Strategy 6: Array not closed (truncated) — auto-close at EOF
                    Exit Do
                End If

                Dim c As Char = peek()

                If c = "]"c Then
                    ' Normal end of array
                    m_index += 1
                    Exit Do
                End If

                ' Strategy 7: Skip leading/trailing commas
                If c = ","c Then
                    m_index += 1
                    Continue Do
                End If

                ' Parse the value (stop at structural chars to handle missing values)
                Dim value As JsonElement = parse_value(stop_at_structural:=True)

                If value Is Nothing Then
                    value = JsonValue.NULL
                End If

                ' Add element to array
                arr.Add(value)

                skip_whitespace_and_comments()

                If at_end() Then
                    ' Strategy 6: Truncated after value — auto-close
                    Exit Do
                End If

                c = peek()

                If c = ","c Then
                    ' Strategy 7: Skip comma (may be trailing, loop will handle it)
                    m_index += 1
                ElseIf c = "]"c Then
                    ' Normal end of array
                    m_index += 1
                    Exit Do
                Else
                    ' Unexpected character after value — skip it and continue
                    m_index += 1
                End If
            Loop

            Return arr
        End Function

#End Region

#Region "Parse string (Strategy 3, 10, 11, 15)"

        ''' <summary>
        ''' Parse a JSON string. The opening quote (either " or ') should be
        ''' at the current cursor position.
        ''' Implements:
        ''' - Strategy 3: Accept both single-quoted and double-quoted strings
        ''' - Strategy 10: Accept unescaped control characters (raw newlines, etc.)
        ''' - Strategy 11: Auto-close unclosed strings at EOF (truncation repair)
        ''' - Strategy 15: Smart quote closure detection — when a quote is
        '''   encountered inside the string, lookahead to check whether the next
        '''   non-whitespace character is a structural character (, } ] : or EOF).
        '''   If so, treat the quote as the real closing quote; otherwise keep it
        '''   as an internal unescaped quote.
        ''' </summary>
        ''' <returns>The decoded string value.</returns>
        Private Function parse_string() As String
            ' Strategy 3: Record the opening quote character (either " or ')
            Dim quote As Char = peek()
            m_index += 1  ' Consume opening quote

            Dim sb As New StringBuilder()

            Do
                If at_end() Then
                    ' Strategy 11: String not closed (truncated) — auto-close at EOF
                    Exit Do
                End If

                Dim c As Char = m_input(m_index)
                m_index += 1

                If c = quote Then
                    ' Strategy 15: Smart quote closure detection.
                    ' Before treating this quote as the closing quote, lookahead
                    ' past any whitespace to find the next non-whitespace character.
                    ' If it is a structural character (, } ] : or EOF), this is the
                    ' real closing quote. Otherwise, this quote is an internal
                    ' unescaped quote and should be kept in the string content.
                    If is_likely_closing_quote() Then
                        ' Real closing quote — end of string
                        Exit Do
                    Else
                        ' Internal unescaped quote — keep it in the string content
                        sb.Append(c)
                    End If
                ElseIf c = "\"c Then
                    ' Escape sequence
                    If at_end() Then
                        ' Trailing backslash at EOF — just stop
                        Exit Do
                    End If

                    Dim esc As Char = m_input(m_index)
                    m_index += 1

                    Select Case esc
                        Case """"c
                            sb.Append(""""c)
                        Case "'"c
                            sb.Append("'"c)
                        Case "\"c
                            sb.Append("\"c)
                        Case "/"c
                            sb.Append("/"c)
                        Case "b"c
                            sb.Append(ControlChars.Back)
                        Case "f"c
                            sb.Append(ControlChars.FormFeed)
                        Case "n"c
                            sb.Append(ControlChars.Lf)
                        Case "r"c
                            sb.Append(ControlChars.Cr)
                        Case "t"c
                            sb.Append(ControlChars.Tab)
                        Case "u"c
                            ' Unicode escape: \uXXXX
                            parse_unicode_escape(sb)
                        Case Else
                            ' Unknown escape sequence: keep the backslash and char
                            sb.Append("\"c)
                            sb.Append(esc)
                    End Select
                Else
                    ' Strategy 10: Accept unescaped control characters
                    ' (including raw newlines, tabs, etc.) without error
                    sb.Append(c)
                End If
            Loop

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Parse a \uXXXX unicode escape sequence and append the decoded
        ''' character to the StringBuilder. Handles surrogate pairs
        ''' (\uD800-\uDBFF followed by \uDC00-\uDFFF).
        ''' The cursor should be positioned right after the "\u" prefix.
        ''' </summary>
        Private Sub parse_unicode_escape(sb As StringBuilder)
            ' Read 4 hex digits for the main code point
            Dim hex As New StringBuilder(4)
            For i As Integer = 0 To 3
                If at_end() Then Exit For
                Dim hc As Char = peek()
                If is_hex_digit(hc) Then
                    hex.Append(hc)
                    m_index += 1
                Else
                    Exit For
                End If
            Next

            If hex.Length = 0 Then
                Exit Sub
            End If

            Dim codePoint As Integer
            If Not Integer.TryParse(hex.ToString(), NumberStyles.HexNumber,
                                 CultureInfo.InvariantCulture, codePoint) Then
                Exit Sub
            End If

            ' Check for high surrogate (D800-DBFF) followed by low surrogate (DC00-DFFF)
            If codePoint >= &HD800 AndAlso codePoint <= &HDBFF Then
                ' Look for \uXXXX following (low surrogate)
                If m_index + 1 < m_length AndAlso
               m_input(m_index) = "\"c AndAlso m_input(m_index + 1) = "u"c Then
                    m_index += 2  ' Skip \u

                    Dim lowHex As New StringBuilder(4)
                    For i As Integer = 0 To 3
                        If at_end() Then Exit For
                        Dim hc As Char = peek()
                        If is_hex_digit(hc) Then
                            lowHex.Append(hc)
                            m_index += 1
                        Else
                            Exit For
                        End If
                    Next

                    If lowHex.Length > 0 Then
                        Dim lowPoint As Integer
                        If Integer.TryParse(lowHex.ToString(), NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture, lowPoint) AndAlso
                       lowPoint >= &HDC00 AndAlso lowPoint <= &HDFFF Then
                            ' Valid surrogate pair — combine into full code point
                            Dim fullCode As Integer = &H10000 + ((codePoint - &HD800) << 10) + (lowPoint - &HDC00)
                            sb.Append(Char.ConvertFromUtf32(fullCode))
                            Exit Sub
                        End If
                    End If

                    ' Invalid low surrogate — append high surrogate as-is
                    sb.Append(Char.ConvertFromUtf32(codePoint))
                Else
                    ' No low surrogate following — append high surrogate as-is
                    sb.Append(Char.ConvertFromUtf32(codePoint))
                End If
            Else
                ' Normal (non-surrogate) code point
                sb.Append(Char.ConvertFromUtf32(codePoint))
            End If
        End Sub

#End Region

#Region "Parse number (Strategy 12)"

        ''' <summary>
        ''' Parse a JSON number (integer or floating-point).
        ''' Implements:
        ''' - Strategy 12: Accept leading + sign
        ''' - Bonus: Accept hex numbers (0x...)
        ''' </summary>
        Private Function parse_number() As JsonElement
            Dim sb As New StringBuilder()

            ' Strategy 12: Accept leading - or + sign
            If Not at_end() AndAlso (peek() = "-"c OrElse peek() = "+"c) Then
                sb.Append(peek())
                m_index += 1
            End If

            ' Check for hex number (0x or 0X prefix)
            If m_index + 1 < m_length AndAlso
           m_input(m_index) = "0"c AndAlso
           (m_input(m_index + 1) = "x"c OrElse m_input(m_index + 1) = "X"c) Then

                sb.Append(m_input(m_index))
                sb.Append(m_input(m_index + 1))
                m_index += 2

                ' Read hex digits
                Do While Not at_end()
                    Dim c As Char = peek()
                    If is_hex_digit(c) Then
                        sb.Append(c)
                        m_index += 1
                    Else
                        Exit Do
                    End If
                Loop

                ' Parse hex string (remove 0x prefix)
                Dim hexStr As String = sb.ToString().Substring(2)
                Dim hexVal As Long
                If Long.TryParse(hexStr, NumberStyles.HexNumber,
                             CultureInfo.InvariantCulture, hexVal) Then
                    Return New JsonValue(hexVal)
                Else
                    Return New JsonValue(0)
                End If
            End If

            ' Read decimal number: digits, optional decimal point, optional exponent
            Dim hasDecimal As Boolean = False
            Dim hasExponent As Boolean = False

            Do While Not at_end()
                Dim c As Char = peek()

                If Char.IsDigit(c) Then
                    sb.Append(c)
                    m_index += 1
                ElseIf c = "."c AndAlso Not hasDecimal AndAlso Not hasExponent Then
                    hasDecimal = True
                    sb.Append(c)
                    m_index += 1
                ElseIf (c = "e"c OrElse c = "E"c) AndAlso Not hasExponent Then
                    hasExponent = True
                    sb.Append(c)
                    m_index += 1

                    ' Read optional exponent sign
                    If Not at_end() AndAlso (peek() = "+"c OrElse peek() = "-"c) Then
                        sb.Append(peek())
                        m_index += 1
                    End If
                Else
                    Exit Do
                End If
            Loop

            Dim numStr As String = sb.ToString()

            ' Handle edge cases: just "-" or "+" or empty
            If numStr = "" OrElse numStr = "-" OrElse numStr = "+" Then
                Return New JsonValue(0)
            End If

            ' Try to parse as Long (integer) first
            If Not hasDecimal AndAlso Not hasExponent Then
                Dim intVal As Long
                If Long.TryParse(numStr, NumberStyles.Integer,
                             CultureInfo.InvariantCulture, intVal) Then
                    Return New JsonValue(intVal)
                End If
            End If

            ' Try to parse as Double (floating-point)
            Dim dblVal As Double
            If Double.TryParse(numStr, NumberStyles.Float Or NumberStyles.AllowLeadingSign,
                           CultureInfo.InvariantCulture, dblVal) Then
                Return New JsonValue(dblVal)
            End If

            ' If all parsing fails, return 0 (lenient fallback)
            Return New JsonValue(0)
        End Function

#End Region

#Region "Parse keyword (Strategy 4, 13, 14)"

        ''' <summary>
        ''' Parse a JSON keyword (true, false, null) or special value
        ''' (NaN, Infinity). Reads all consecutive letters and attempts to match.
        ''' Implements:
        ''' - Strategy 4: Convert Infinity to null
        ''' - Strategy 13: Convert NaN to null
        ''' - Strategy 14: Partial keyword matching (tru -> true, etc.)
        ''' </summary>
        ''' <returns>
        ''' The parsed <see cref="JsonElement"/>, or Nothing if the text does
        ''' not match any keyword (caller should skip and continue).
        ''' </returns>
        Private Function parse_keyword() As JsonElement
            Dim sb As New StringBuilder()

            ' Read all consecutive letters
            Do While Not at_end()
                Dim c As Char = peek()
                If Char.IsLetter(c) Then
                    sb.Append(c)
                    m_index += 1
                Else
                    Exit Do
                End If
            Loop

            Dim kw As String = sb.ToString()

            If kw = "" Then
                Return Nothing
            End If

            ' Exact match (case-insensitive)
            If kw.Equals("true", StringComparison.OrdinalIgnoreCase) Then
                Return New JsonValue(True)
            ElseIf kw.Equals("false", StringComparison.OrdinalIgnoreCase) Then
                Return New JsonValue(False)
            ElseIf kw.Equals("null", StringComparison.OrdinalIgnoreCase) Then
                Return JsonValue.NULL
            ElseIf kw.Equals("NaN", StringComparison.OrdinalIgnoreCase) Then
                ' Strategy 13: NaN -> null
                Return JsonValue.NULL
            ElseIf kw.Equals("Infinity", StringComparison.OrdinalIgnoreCase) Then
                ' Strategy 4: Infinity -> null
                Return JsonValue.NULL
            End If

            ' Strategy 14: Partial keyword matching
            ' If the read text is a prefix of a known keyword, return the
            ' corresponding value. This handles truncated keywords like
            ' "tru" instead of "true" (common in LLM output truncation).
            If "true".StartsWith(kw, StringComparison.OrdinalIgnoreCase) Then
                Return New JsonValue(True)
            ElseIf "false".StartsWith(kw, StringComparison.OrdinalIgnoreCase) Then
                Return New JsonValue(False)
            ElseIf "null".StartsWith(kw, StringComparison.OrdinalIgnoreCase) Then
                Return JsonValue.NULL
            ElseIf "NaN".StartsWith(kw, StringComparison.OrdinalIgnoreCase) Then
                Return JsonValue.NULL
            ElseIf "Infinity".StartsWith(kw, StringComparison.OrdinalIgnoreCase) Then
                Return JsonValue.NULL
            End If

            ' Not a keyword — return Nothing so caller can skip and continue
            Return Nothing
        End Function

#End Region

#Region "Parse key (Strategy 3, 9)"

        ''' <summary>
        ''' Parse an object key. The key may be:
        ''' - A double-quoted string ("key")
        ''' - A single-quoted string ('key') — Strategy 3
        ''' - An unquoted identifier (key) — Strategy 9
        ''' </summary>
        ''' <returns>The key string.</returns>
        Private Function parse_key() As String
            skip_whitespace_and_comments()

            If at_end() Then
                Return ""
            End If

            Dim c As Char = peek()

            ' Strategy 3: Quoted key (double or single quote)
            If c = """"c OrElse c = "'"c Then
                Return parse_string()
            End If

            ' Strategy 9: Unquoted key (JavaScript-style)
            ' Read until we encounter :, ,, }, or whitespace
            Dim sb As New StringBuilder()

            Do While Not at_end()
                c = peek()
                If c = ":"c OrElse c = ","c OrElse c = "}"c OrElse c = "]"c OrElse Char.IsWhiteSpace(c) Then
                    Exit Do
                End If
                sb.Append(c)
                m_index += 1
            Loop

            Return sb.ToString()
        End Function

#End Region

#Region "Infinity check (Strategy 4)"

        ''' <summary>
        ''' Check if the current position (after an optional - or + sign)
        ''' spells "Infinity". If so, advance the cursor past it and return True.
        ''' This implements Strategy 4: Infinity/-Infinity/+Infinity -> null.
        ''' </summary>
        Private Function check_infinity() As Boolean
            ' Cursor is at - or +, so Infinity starts at the next position
            Dim start As Integer = m_index + 1
            Const INFINITY As String = "Infinity"

            If start + INFINITY.Length > m_length Then
                Return False
            End If

            For i As Integer = 0 To INFINITY.Length - 1
                If Char.ToLowerInvariant(m_input(start + i)) <> Char.ToLowerInvariant(INFINITY(i)) Then
                    Return False
                End If
            Next

            ' Match found — advance cursor past the sign and "Infinity"
            m_index = start + INFINITY.Length
            Return True
        End Function

#End Region

#Region "Hex digit check"

        ''' <summary>
        ''' Check if a character is a hexadecimal digit (0-9, a-f, A-F).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function is_hex_digit(c As Char) As Boolean
            Return (c >= "0"c AndAlso c <= "9"c) OrElse
               (c >= "a"c AndAlso c <= "f"c) OrElse
               (c >= "A"c AndAlso c <= "F"c)
        End Function

#End Region

#Region "Smart quote closure detection (Strategy 15)"

        ''' <summary>
        ''' Determine whether the quote character just consumed at
        ''' <c>m_index - 1</c> is likely the real closing quote of a string.
        ''' </summary>
        ''' <para>
        ''' Strategy 15: Smart Quote Closure Detection.
        ''' </para>
        ''' <para>
        ''' When the parser encounters a quote character inside a string, it does
        ''' not immediately treat it as the closing quote. Instead, it looks ahead
        ''' past any whitespace to find the next non-whitespace character:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>If the next non-whitespace character is one of
        ''' <c>,</c>, <c>}</c>, <c>]</c>, <c>:</c>, or EOF, this quote is treated
        ''' as the real closing quote (these are the only legal characters that can
        ''' follow a string value in valid JSON).</description></item>
        ''' <item><description>Otherwise (the next character is a letter, digit, or
        ''' any other non-structural character), this quote is treated as an
        ''' internal unescaped quote and is kept in the string content.</description></item>
        ''' </list>
        ''' <para>
        ''' This handles the common LLM error of embedding unescaped quotes inside
        ''' string values, e.g.:
        ''' </para>
        ''' <example>
        ''' <code>
        ''' "evidence": "Multiple papers: "HIF-1 induces GLUT1" and "PDK1" in cells."
        ''' </code>
        ''' <para>
        ''' The internal quotes around "HIF-1 induces GLUT1" and "PDK1" are followed
        ''' by letters/spaces, so they are kept. Only the final quote (followed by
        ''' <c>}</c> or <c>,</c>) is treated as the closing quote.
        ''' </para>
        ''' </example>
        ''' <returns>
        ''' <c>True</c> if the quote is likely the real closing quote;
        ''' <c>False</c> if it is likely an internal unescaped quote.
        ''' </returns>
        Private Function is_likely_closing_quote() As Boolean
            ' The cursor is currently positioned just after the quote character
            ' that was consumed. Look ahead from the current position.
            Dim i As Integer = m_index

            ' Skip whitespace (including newlines, tabs, spaces, carriage returns)
            ' to find the next non-whitespace character.
            Do While i < m_length
                Dim c As Char = m_input(i)
                If c = " "c OrElse c = ControlChars.Tab OrElse
               c = ControlChars.Lf OrElse c = ControlChars.Cr Then
                    i += 1
                Else
                    Exit Do
                End If
            Loop

            ' If we reached EOF, this is the closing quote (truncation case).
            If i >= m_length Then
                Return True
            End If

            ' Check the next non-whitespace character.
            ' In valid JSON, the only characters that can legally follow a string
            ' value's closing quote are:
            '   ,  (separator before next key-value pair or array element)
            '   }  (closing an object)
            '   ]  (closing an array)
            '   :  (after a key string, before its value)
            Dim nextChar As Char = m_input(i)

            Return nextChar = ","c OrElse
               nextChar = "}"c OrElse
               nextChar = "]"c OrElse
               nextChar = ":"c
        End Function

#End Region

    End Class
End Namespace