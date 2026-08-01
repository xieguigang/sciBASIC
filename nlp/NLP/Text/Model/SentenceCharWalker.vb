#Region "Microsoft.VisualBasic::0aa245627d6277b33bcd0c5c078aacab, Data\Trinity\Model\SentenceCharWalker.vb"

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

    '   Total Lines: 60
    '    Code Lines: 46 (76.67%)
    ' Comment Lines: 3 (5.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (18.33%)
    '     File Size: 1.86 KB


    '     Class SentenceCharWalker
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens, Trim, WalkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Model

    ''' <summary>
    ''' Parse a line of sentence to a set of the word tokens
    ''' </summary>
    Public Class SentenceCharWalker

        ReadOnly buf As New CharBuffer
        ReadOnly str As CharPtr
        ReadOnly url_protocols As New Regex("[a-zA-Z0-9]+[:]//")

        Sub New(line As String)
            str = line
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of String)
            Dim token As New Value(Of String)

            Do While Not str
                If Not (token = WalkChar(++str)) Is Nothing Then
                    Yield Trim(CStr(token))
                End If
            Loop

            If buf > 0 Then
                Yield Trim(New String(buf.PopAllChars))
            End If
        End Function

        Private Shared Function Trim(si As String) As String
            Return si.Trim("."c, ","c, "-"c, """"c, "'"c, " "c, vbTab)
        End Function

        Private Function WalkChar(c As Char) As String
            If c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.CR OrElse c = ASCII.LF Then
                If buf > 0 Then
                    Return New String(buf.PopAllChars)
                End If
            ElseIf Char.IsSeparator(c) AndAlso c <> "-"c Then
                If buf.StartsWith(url_protocols) Then
                    buf.Add(c)
                Else
                    If buf > 0 Then
                        Return New String(buf.PopAllChars)
                    End If
                End If
            Else
                buf.Add(c)
            End If

            Return Nothing
        End Function

    End Class
End Namespace
