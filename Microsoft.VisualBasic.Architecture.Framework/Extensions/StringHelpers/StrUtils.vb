#Region "Microsoft.VisualBasic::4adae42acc2268152d948f5c86dd25df, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\StrUtils.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

'
' System.Web.Util.StrUtils
'
' Author(s):
'  Gonzalo Paniagua Javier (gonzalo@ximian.com)
'
' (C) 2005 Novell, Inc, (http://www.novell.com)
'

'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'

Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions

Public Module StrUtils

    ''' <summary>
    ''' 32-126
    ''' </summary>
    ''' <param name="len%"></param>
    ''' <returns></returns>
    Public Function RandomASCIIString(len%) As String
        Dim rnd As New Random
        Dim s As New List(Of Char)

        For i As Integer = 0 To len
            s.Add(Chr(rnd.Next(32, 127)))
        Next

        Return New String(s.ToArray)
    End Function

    ''' <summary>
    ''' <see cref="CultureInfo.InvariantCulture"/>, Gets the System.Globalization.CultureInfo object that is culture-independent
    ''' (invariant).
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property InvariantCulture As CultureInfo = CultureInfo.InvariantCulture

    Public Function StartsWith(str1 As String, str2 As String) As Boolean
        Return StartsWith(str1, str2, False)
    End Function

    Public Function StartsWith(str1 As String, str2 As String, ignore_case As Boolean) As Boolean
        Dim l2 As Integer = str2.Length
        If l2 = 0 Then
            Return True
        End If

        Dim l1 As Integer = str1.Length
        If l2 > l1 Then
            Return False
        End If

        Return (0 = String.Compare(str1, 0, str2, 0, l2, ignore_case, StrUtils.InvariantCulture))
    End Function

    Public Function EndsWith(str1 As String, str2 As String) As Boolean
        Return EndsWith(str1, str2, False)
    End Function

    Public Function EndsWith(str1 As String, str2 As String, ignore_case As Boolean) As Boolean
        Dim l2 As Integer = str2.Length
        If l2 = 0 Then
            Return True
        End If

        Dim l1 As Integer = str1.Length
        If l2 > l1 Then
            Return False
        End If

        Return (0 = String.Compare(str1, l1 - l2, str2, 0, l2, ignore_case, StrUtils.InvariantCulture))
    End Function

    Public Function EscapeQuotesAndBackslashes(attributeValue As String) As String
        Dim sb As StringBuilder = Nothing
        For i As Integer = 0 To attributeValue.Length - 1
            Dim ch As Char = attributeValue(i)
            If ch = "'"c OrElse ch = """"c OrElse ch = "\"c Then
                If sb Is Nothing Then
                    sb = New StringBuilder()
                    sb.Append(attributeValue.Substring(0, i))
                End If
                sb.Append("\"c)
                sb.Append(ch)
            Else
                If sb IsNot Nothing Then
                    sb.Append(ch)
                End If
            End If
        Next
        If sb IsNot Nothing Then
            Return sb.ToString()
        End If
        Return attributeValue
    End Function

    Public Function SplitRemoveEmptyEntries(value As String, separator As Char()) As String()
        Return value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
    End Function

    ''' <summary>
    ''' Split text with a separator char
    ''' </summary>
    ''' <param name="text">The text.</param>
    ''' <param name="sep">The separator.</param>
    ''' <returns></returns>
    Public Function SplitWithSeparator(text As String, sep As Char) As String()
        If String.IsNullOrEmpty(text) Then
            Return Nothing
        End If

        Dim items As String() = New String(1) {}

        Dim index As Integer = text.IndexOf(sep)
        If index >= 0 Then
            items(0) = text.Substring(0, index)
            items(1) = text.Substring(index + 1)
        Else
            items(0) = text
            items(1) = Nothing
        End If

        Return items
    End Function

    ''' <summary>
    ''' Split text with a separator char
    ''' </summary>
    ''' <param name="text">The text.</param>
    ''' <param name="sep">The separator.</param>
    ''' <returns></returns>
    Public Function SplitWithSeparatorFromRight(text As String, sep As Char) As String()
        If String.IsNullOrEmpty(text) Then
            Return Nothing
        End If

        Dim items As String() = New String(1) {}

        Dim index As Integer = text.LastIndexOf(sep)
        If index >= 0 Then
            items(0) = text.Substring(0, index)
            items(1) = text.Substring(index + 1)
        Else
            items(0) = text
            items(1) = Nothing
        End If

        Return items
    End Function

    ''' <summary>
    ''' Splits the text with spaces.
    ''' </summary>
    ''' <param name="text">The text.</param>
    ''' <returns></returns>
    Public Function SplitWithSpaces(text As String) As String()
        Dim pattern As String = "[^ ]+"
        Dim rgx As New Regex(pattern)
        Dim mc As MatchCollection = rgx.Matches(text)
        Dim items As String() = New String(mc.Count - 1) {}
        For i As Integer = 0 To items.Length - 1
            items(i) = mc(i).Value
        Next
        Return items
    End Function

    ''' <summary>
    ''' Splits the text into lines.
    ''' </summary>
    ''' <param name="text">The text.</param>
    ''' <returns></returns>
    Public Function SplitIntoLines(text As String) As String()
        Dim lines As New List(Of String)()
        Dim line As New StringBuilder()
        For Each ch As Char In text
            Select Case ch
                Case ControlChars.Cr

                Case ControlChars.Lf
                    lines.Add(line.ToString())
                    line.Length = 0

                Case Else
                    line.Append(ch)

            End Select
        Next
        If line.Length > 0 Then
            lines.Add(line.ToString())
        End If
        Return lines.ToArray()
    End Function

    ''' <summary>
    ''' Concats two strings with a delimiter.
    ''' </summary>
    ''' <param name="s1">string 1</param>
    ''' <param name="delim">delimiter</param>
    ''' <param name="s2">string 2</param>
    ''' <returns></returns>
    Public Function AddWithDelim(s1 As String, delim As String, s2 As String) As String
        If String.IsNullOrEmpty(s1) Then
            Return s2
        Else
            Return s1 & delim & s2
        End If
    End Function
    ''' <summary>
    ''' Contacts the with delim.
    ''' </summary>
    ''' <param name="str1">The STR1.</param>
    ''' <param name="delim">The delim.</param>
    ''' <param name="str2">The STR2.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(str1 As String, delim As String, str2 As String) As String
        If String.IsNullOrEmpty(str1) Then
            Return str2
        ElseIf String.IsNullOrEmpty(str2) Then
            Return str1
        Else
            Return str1 & delim & str2
        End If
    End Function

    ''' <summary>
    ''' Contact with delim, delim is used after the first not Empty item
    ''' </summary>
    ''' <param name="items"></param>
    ''' <param name="delim"></param>
    ''' <returns></returns>
    Public Function ContactWithDelimSkipEmpty(items As IEnumerable(Of String), delim As String) As String
        Return ContactWithDelimSkipSome(items, delim, String.Empty)
    End Function

    ''' <summary>
    ''' Contact with delim, delim is used after the first not null item
    ''' </summary>
    ''' <param name="items"></param>
    ''' <param name="delim"></param>
    ''' <returns></returns>
    Public Function ContactWithDelimSkipNull(items As IEnumerable(Of String), delim As String) As String
        Return ContactWithDelimSkipSome(items, delim, Nothing)
    End Function

    ''' <summary>
    ''' Contacts the items with delim skip some.
    ''' </summary>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <param name="skip">The skip.</param>
    ''' <returns></returns>
    Public Function ContactWithDelimSkipSome(items As IEnumerable(Of String), delim As String, skip As String) As String
        Dim text As New StringBuilder()
        Dim isFirst As Boolean = True
        For Each item As String In items
            If item = skip Then
                Continue For
            End If
            If isFirst Then
                isFirst = False
            Else
                text.Append(delim)
            End If
            text.Append(item)
        Next
        Return text.ToString()
    End Function

    ''' <summary>
    ''' Contacts the items with delim.
    ''' </summary>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(items As IEnumerable(Of String), delim As String) As String
        Return ContactWithDelim(items, delim, Nothing, Nothing)
    End Function
    ''' <summary>
    ''' Contacts the items with delim.
    ''' </summary>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <param name="initialValue">The initial value.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(items As IEnumerable(Of String), delim As String, initialValue As String) As String
        Return ContactWithDelim(items, delim, initialValue, Nothing)
    End Function
    ''' <summary>
    ''' Contacts the items with delim.
    ''' </summary>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <param name="initialValue">The initial value.</param>
    ''' <param name="endValue">The end value.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(items As IEnumerable(Of String), delim As String, initialValue As String, endValue As String) As String
        Dim text As New StringBuilder(initialValue)
        Dim isFirst As Boolean = True
        For Each item As String In items
            If isFirst Then
                isFirst = False
            Else
                text.Append(delim)
            End If
            text.Append(item)
        Next
        text.Append(endValue)
        Return text.ToString()
    End Function

    ''' <summary>
    ''' Contacts the items with delim.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(Of T)(items As IEnumerable(Of T), delim As String) As String
        Return ContactWithDelim(Of T)(items, delim, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Contacts the items with delim.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="items">The items.</param>
    ''' <param name="delim">The delim.</param>
    ''' <param name="initialValue">The initial value.</param>
    ''' <param name="endValue">The end value.</param>
    ''' <returns></returns>
    Public Function ContactWithDelim(Of T)(items As IEnumerable(Of T), delim As String, initialValue As String, endValue As String) As String
        Dim text As New StringBuilder()
        text.Append(initialValue)
        Dim isFirst As Boolean = True
        For Each item As T In items
            If isFirst Then
                isFirst = False
            Else
                text.Append(delim)
            End If
            text.Append(item.ToString())
        Next
        text.Append(endValue)
        Return text.ToString()
    End Function

    ''' <summary>
    ''' Gets the header.
    ''' </summary>
    ''' <param name="text">The text.</param>
    ''' <param name="length">The length.</param>
    ''' <returns></returns>
    Public Function GetHeader(text As String, length As Integer) As String
        If text.Length <= length Then
            Return text
        Else
            Return text.Substring(0, length)
        End If
    End Function

    ''' <summary>
    ''' Get the sub string between 'ket' and 'bra'.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="bra"></param>
    ''' <param name="ket"></param>
    ''' <returns></returns>
    Public Function GetSubStringBetween(text As String, bra As Char, ket As Char) As String
        If text Is Nothing Then
            Return Nothing
        End If
        Dim braIndex As Integer = text.IndexOf(bra)
        If braIndex > -1 Then
            Dim ketIndex As Integer = text.IndexOf(ket)
            If ketIndex > braIndex Then
                Return text.Substring(braIndex + 1, ketIndex - braIndex - 1)
            End If
        End If
        Return String.Empty
    End Function

    ''' <summary>
    ''' Get the sub string between 'ket' and 'bra'.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="bra"></param>
    ''' <param name="ket"></param>
    ''' <returns></returns>
    Public Function GetLastSubStringBetween(text As String, bra As Char, ket As Char) As String
        If text Is Nothing Then
            Return Nothing
        End If
        Dim braIndex As Integer = text.LastIndexOf(bra)
        If braIndex > -1 Then
            Dim ketIndex As Integer = text.LastIndexOf(ket)
            If ketIndex > braIndex Then
                Return text.Substring(braIndex + 1, ketIndex - braIndex - 1)
            End If
        End If
        Return String.Empty
    End Function

    ''' <summary>
    ''' Starts with upper case.
    ''' </summary>
    ''' <param name="name">The name.</param>
    ''' <returns></returns>
    Public Function StartWithUpperCase(name As String) As Boolean
        Return name.Length >= 1 AndAlso Char.IsUpper(name(0))
    End Function

    ''' <summary>
    ''' Uppers the case of the first char.
    ''' </summary>
    ''' <param name="name">The name.</param>
    ''' <returns></returns>
    Public Function UpperCaseFirstChar(name As String) As String
        If name.Length >= 1 AndAlso Char.IsLower(name(0)) Then
            Dim chars As Char() = name.ToCharArray()
            chars(0) = [Char].ToUpper(chars(0))
            Return New String(chars)
        End If
        Return name
    End Function

    ''' <summary>
    ''' Lowers the case of the first char.
    ''' </summary>
    ''' <param name="name">The name.</param>
    ''' <returns></returns>
    Public Function LowerCaseFirstChar(name As String) As String
        If name.Length >= 1 AndAlso Char.IsUpper(name(0)) Then
            Dim chars As Char() = name.ToCharArray()
            chars(0) = [Char].ToLower(chars(0))
            Return New String(chars)
        End If
        Return name
    End Function

    ''' <summary>
    ''' split text into words by space and newline chars, multiple spaces are treated as a single space.
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function GetWords(text As String) As String()
        Dim tokens As New List(Of String)()
        Dim token As New List(Of Char)()

        For Each ch As Char In text
            Select Case ch
                Case " "c, ControlChars.Cr, ControlChars.Lf
                    If token.Count > 0 Then
                        tokens.Add(New String(token.ToArray()))
                        token.Clear()
                    End If
                Case Else
                    token.Add(ch)
            End Select
        Next

        If token.Count > 0 Then
            tokens.Add(New String(token.ToArray()))
        End If

        Return tokens.ToArray()
    End Function

    Public Function CountWordFrequency(article As String, delimiters As String) As Dictionary(Of String, Integer)
        If article Is Nothing Then
            Throw New ArgumentNullException("article")
        End If
        If delimiters Is Nothing Then
            Throw New ArgumentNullException("delimiters")
        End If
        Dim words As New List(Of String)()
        Dim buffer As New List(Of Char)()
        For Each c As Char In article
            If delimiters.IndexOf(c) = -1 Then
                buffer.Add(c)
            Else
                If buffer.Count > 0 Then
                    words.Add(New String(buffer.ToArray()))
                    buffer.Clear()
                End If
            End If
        Next
        If buffer.Count > 0 Then
            words.Add(New String(buffer.ToArray()))
        End If

        Dim table As New Dictionary(Of String, Integer)()
        For Each word As String In words
            If table.ContainsKey(word) Then
                table(word) += 1
            Else
                table.Add(word, 1)
            End If
        Next
        Return table
    End Function
End Module
