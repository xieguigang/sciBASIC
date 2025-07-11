#Region "Microsoft.VisualBasic::52a8eb08a6038a1c64c46a690d3d8e24, mime\application%json\Parser\TokenIcer.vb"

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

    '   Total Lines: 226
    '    Code Lines: 172 (76.11%)
    ' Comment Lines: 27 (11.95%)
    '    - Xml Docs: 62.96%
    ' 
    '   Blank Lines: 27 (11.95%)
    '     File Size: 7.30 KB


    ' Class TokenIcer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: MeasureToken, walkChar
    ' 
    ' Class StringTokenIcer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CheckError, GetTokenSequence
    ' 
    ' Class StreamTokenIcer
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CheckError, GetTokenSequence
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Public MustInherit Class TokenIcer

    Protected buffer As CharBuffer
    Protected escape As Char = ASCII.NUL
    Protected lastToken As Token
    Protected line As Integer = 0

    ''' <summary>
    ''' single line comment
    ''' </summary>
    Protected comment_escape As Boolean
    Protected comments As New Dictionary(Of String, String)
    Protected comment_key As String = ""

    ''' <summary>
    ''' set this option to value false will enable syntax of array like: [1 2 3], 
    ''' without comma symbol as the element value delimiter!
    ''' </summary>
    Protected strictVectorSyntax As Boolean = True

    Sub New(strictVectorSyntax As Boolean)
        Me.buffer = ""
    End Sub

    Public MustOverride Iterator Function GetTokenSequence() As IEnumerable(Of Token)
    Public MustOverride Function CheckError() As Boolean

    Protected Iterator Function walkChar(c As Char) As IEnumerable(Of Token)
        If c = ASCII.LF Then
            line += 1
        End If

        If comment_escape Then
            If c = ASCII.LF Then
                comment_escape = False
                comments(comment_key) = New String(buffer.PopAllChars)
                comment_key = ""
            Else
                Call buffer.Add(c)
            End If
        ElseIf escape <> ASCII.NUL Then
            ' is string escape
            If c = escape Then
                If buffer.StartEscaping Then
                    ' continute
                    buffer += c
                Else
                    ' end of the string escape
                    escape = ASCII.NUL
                    Yield New Token(Token.JSONElements.String, buffer.PopAllChars)
                End If
            Else
                buffer += c
            End If
        ElseIf c = "/"c Then
            ' check hjson single comment line
            If buffer > 0 Then
                If buffer = "/" Then
                    ' is single comment line
                    buffer.Pop()
                    comment_escape = True
                Else
                    Yield MeasureToken()
                    buffer.Add("/"c)
                End If
            Else
                buffer.Add(c)
            End If
        ElseIf c = "'"c OrElse c = """"c Then
            escape = c
            Return
        ElseIf c = ":"c Then
            ' end previous token
            ' key: value
            If buffer > 0 Then
                Yield New Token(Token.JSONElements.Key, buffer.PopAllChars)
            End If

            Yield New Token(Token.JSONElements.Colon, ":")
        ElseIf c = "," Then
            ' end previous token
            Yield MeasureToken()
            Yield New Token(Token.JSONElements.Delimiter, ",")
        ElseIf c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.LF Then
            Yield MeasureToken()
        ElseIf c = "{"c OrElse c = "["c Then
            ' end previous token
            Yield MeasureToken()
            Yield New Token(Token.JSONElements.Open, CStr(c))
        ElseIf c = "}"c OrElse c = "]"c Then
            ' end previous token
            Yield MeasureToken()
            Yield New Token(Token.JSONElements.Close, CStr(c))
        Else
            buffer += c
        End If
    End Function

    ''' <summary>
    ''' the entire <see cref="buffer"/> will be clear in this function
    ''' </summary>
    ''' <returns></returns>
    Protected Function MeasureToken() As Token
        If buffer = 0 Then
            Return Nothing
        End If

        Dim str As New String(buffer.PopAllChars)

        Static [boolean] As Index(Of String) = {"true", "false"}

        If str.IsInteger Then
            Return New Token(Token.JSONElements.Integer, str)
        ElseIf str.IsNumeric Then
            Return New Token(Token.JSONElements.Double, str)
        ElseIf str.ToLower Like [boolean] Then
            Return New Token(Token.JSONElements.Boolean, str)
        Else
            Return New Token(Token.JSONElements.String, str)
        End If
    End Function

End Class

Public Class StringTokenIcer : Inherits TokenIcer

    ''' <summary>
    ''' a clean json string
    ''' </summary>
    Dim json_str As CharPtr

    Sub New(s As String, strictVectorSyntax As Boolean)
        Call MyBase.New(strictVectorSyntax)
        json_str = s
    End Sub

    Public Overrides Iterator Function GetTokenSequence() As IEnumerable(Of Token)
        Do While Not json_str.EndRead
            For Each t As Token In walkChar(++json_str)
                If Not t Is Nothing Then
                    lastToken = t.SetLine(line)
                    Yield t
                End If
            Next
        Loop

        If buffer > 0 Then
            If comment_escape Then
                comments(comment_key) = New String(buffer.PopAllChars)
            ElseIf Not strictVectorSyntax Then
                Yield MeasureToken()
            Else
                Throw New Exception("unknow parser error at the end of the json document stream!")
            End If
        End If
    End Function

    Public Overrides Function CheckError() As Boolean
        If json_str Is Nothing Then
            Return True
        ElseIf json_str Like "null" Then
            Return True
        ElseIf json_str = "" Then
            Return True
        Else
            Return False
        End If
    End Function
End Class

Public Class StreamTokenIcer : Inherits TokenIcer

    ''' <summary>
    ''' a clean json string
    ''' </summary>
    Dim json_str As CharStream

    Sub New(s As Stream, strictVectorSyntax As Boolean, Optional tqdm As Boolean = True)
        Call MyBase.New(strictVectorSyntax)
        json_str = New CharStream(New StreamReader(s), tqdm)
    End Sub

    Sub New(s As StreamReader, strictVectorSyntax As Boolean, Optional tqdm As Boolean = True)
        Call MyBase.New(strictVectorSyntax)
        json_str = New CharStream(s, tqdm)
    End Sub

    Public Overrides Iterator Function GetTokenSequence() As IEnumerable(Of Token)
        Do While Not json_str.EndRead
            For Each t As Token In walkChar(++json_str)
                If Not t Is Nothing Then
                    lastToken = t.SetLine(line)
                    Yield t
                End If
            Next
        Loop

        If buffer > 0 Then
            If comment_escape Then
                comments(comment_key) = New String(buffer.PopAllChars)
            ElseIf Not strictVectorSyntax Then
                Yield MeasureToken()
            Else
                Throw New Exception("unknow parser error at the end of the json document stream!")
            End If
        End If
    End Function

    Public Overrides Function CheckError() As Boolean
        If json_str Is Nothing Then
            Return True
        ElseIf json_str Like "null" Then
            Return True
        ElseIf json_str = "" Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
