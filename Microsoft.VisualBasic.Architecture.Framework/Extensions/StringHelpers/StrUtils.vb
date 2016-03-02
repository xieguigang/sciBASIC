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

Public Module StrUtils

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
End Module

