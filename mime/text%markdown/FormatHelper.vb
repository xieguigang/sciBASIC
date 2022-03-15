#Region "Microsoft.VisualBasic::3bf7fefc7fc4153069a0e2a3c9126923, sciBASIC#\mime\text%markdown\FormatHelper.vb"

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

    '   Total Lines: 125
    '    Code Lines: 83
    ' Comment Lines: 23
    '   Blank Lines: 19
    '     File Size: 5.20 KB


    ' Module FormatHelper
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AttributeEncode, AttributeSafeUrl, GetHashKey, handleTrailingParens, Outdent
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports stdNum = System.Math

Module FormatHelper

    Public ReadOnly _escapeTable As Dictionary(Of String, String)
    Public ReadOnly _invertedEscapeTable As Dictionary(Of String, String)
    Public ReadOnly _backslashEscapeTable As Dictionary(Of String, String)

    Public _unescapes As New Regex(ChrW(26) & "E\d+E", RegexOptions.Compiled)

    Public _backslashEscapes As Regex

    ' temporarily replaces "://" where auto-linking shouldn't happen

    ''' <summary>
    ''' In the static constuctor we'll initialize what stays the same across all transforms.
    ''' </summary>
    Sub New()
        ' Table of hash values for escaped characters:
        _escapeTable = New Dictionary(Of String, String)()
        _invertedEscapeTable = New Dictionary(Of String, String)()
        ' Table of hash value for backslash escaped characters:
        _backslashEscapeTable = New Dictionary(Of String, String)()

        Dim backslashPattern As String = ""

        For Each c As Char In "\`*_{}[]()>#+-.!/:"
            Dim key As String = c.ToString()
            Dim hash As String = GetHashKey(key, isHtmlBlock:=False)
            _escapeTable.Add(key, hash)
            _invertedEscapeTable.Add(hash, key)
            _backslashEscapeTable.Add("\" & key, hash)
            backslashPattern += Regex.Escape("\" & key) & "|"
        Next

        _backslashEscapes = New Regex(backslashPattern.Substring(0, backslashPattern.Length - 1), RegexOptions.Compiled)
    End Sub

    ''' <summary>
    ''' Tabs are automatically converted to spaces as part of the transform  
    ''' this constant determines how "wide" those tabs become in spaces  
    ''' </summary>
    Public Const _tabWidth As Integer = 4

    Dim _outDent As New Regex("^[ ]{1," & _tabWidth & "}", RegexOptions.Multiline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Remove one level of line-leading spaces
    ''' </summary>
    Public Function Outdent(block As String) As String
        Return _outDent.Replace(block, "")
    End Function

    Public Function GetHashKey(s As String, isHtmlBlock As Boolean) As String
        Dim delim = If(isHtmlBlock, "H"c, "E"c)
        Return ChrW(26) & delim & stdNum.Abs(s.GetHashCode()).ToString() & delim
    End Function

    Public Function AttributeEncode(s As String) As String
        Return s.Replace(">", "&gt;").Replace("<", "&lt;").Replace("""", "&quot;").Replace("'", "&#39;")
    End Function

    Public Function AttributeSafeUrl(s As String) As String
        s = AttributeEncode(s)
        For Each c As Char In "*_:()[]"
            s = s.Replace(c.ToString(), _escapeTable(c.ToString()))
        Next
        Return s
    End Function

    Public Const _charInsideUrl As String = "[-A-Z0-9+&@#/%?=~_|\[\]\(\)!:,\.;" & ChrW(26) & "]"
    Public Const _charEndingUrl As String = "[-A-Z0-9+&@#/%=~_|\[\])]"

    Dim _endCharRegex As New Regex(_charEndingUrl, RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    ''' <summary>
    ''' The first group is essentially a negative lookbehind -- if there's a &lt; or a =", we don't touch this.
    ''' We're not using a *real* lookbehind, because of links with in links, like 
    ''' &lt;a href="http://web.archive.org/web/20121130000728/http://www.google.com/">
    ''' With a real lookbehind, the full link would never be matched, and thus the http://www.google.com *would* be matched.
    ''' With the simulated lookbehind, the full link *is* matched (just not handled, because of this early return), causing
    ''' the google link to not be matched again.
    ''' </summary>
    ''' <param name="match"></param>
    ''' <returns></returns>
    Public Function handleTrailingParens(match As Match) As String
        If match.Groups(1).Success Then
            Return match.Value
        End If

        Dim protocol = match.Groups(2).Value
        Dim link = match.Groups(3).Value
        If Not link.EndsWith(")") Then
            Return "<" & protocol & link & ">"
        End If
        Dim level = 0
        For Each c As Match In Regex.Matches(link, "[()]")
            If c.Value = "(" Then
                If level <= 0 Then
                    level = 1
                Else
                    level += 1
                End If
            Else
                level -= 1
            End If
        Next
        Dim tail = ""
        If level < 0 Then
            link = Regex.Replace(link, "\){1," & (-level) & "}$", Function(m)
                                                                      tail = m.Value
                                                                      Return ""
                                                                  End Function)
        End If
        If tail.Length > 0 Then
            Dim lastChar = link(link.Length - 1)
            If Not _endCharRegex.IsMatch(lastChar.ToString()) Then
                tail = lastChar & tail
                link = link.Substring(0, link.Length - 1)
            End If
        End If
        Return "<" & protocol & link & ">" & tail
    End Function
End Module
