#Region "Microsoft.VisualBasic::180411b12eaddeb1a5d4a195281c225b, sciBASIC#\mime\application%json\Parser\JsonParser.vb"

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

'   Total Lines: 516
'    Code Lines: 338
' Comment Lines: 114
'   Blank Lines: 64
'     File Size: 16.14 KB


' Class JsonParser
' 
'     Properties: JSONvalue
' 
'     Function: _parse, GetParserErrors, Open, OpenJSON, Parse
'               parseArray, parseBoolean, parseKey, parseNull, parseNumber
'               parseObject, parseString, parseValue, StripString
' 
'     Sub: ClearParserError, skipChar
' 
' /********************************************************************************/

#End Region

' rewrite from VBA JSON project
' author: Michael Glaser (vbjson@ediy.co.nz)
' source code: http://code.google.com/p/vba-json
' BSD Licensed

' translated into vb.net : pandasxd (qhgz2011@hotmail.com)
' version 1.0.0 beta [debugged]
' READ ONLY!! Output part is under construction

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

''' <summary>
''' A json text parser module
''' </summary>
Public Class JsonParser

    'Const INVALID_JSON As Integer = 1
    'Const INVALID_OBJECT As Integer = 2
    'Const INVALID_ARRAY As Integer = 3
    'Const INVALID_BOOLEAN As Integer = 4
    'Const INVALID_NULL As Integer = 5
    'Const INVALID_KEY As Integer = 6
    'Const INVALID_RPC_CALL As Integer = 7

    Dim psErrors As String
    ''' <summary>
    ''' a clean json string
    ''' </summary>
    Dim json_str As CharPtr
    Dim buffer As CharBuffer
    Dim escape As Char = ASCII.NUL
    ''' <summary>
    ''' single line comment
    ''' </summary>
    Dim comment_escape As Boolean

    ''' <summary>
    ''' The root node in json file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property JSONvalue As JsonElement

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="json_str">the json text content</param>
    Sub New(json_str As String)
        Me.buffer = ""
        Me.json_str = Strings _
            .Trim(json_str) _
            .Trim(ASCII.TAB, ASCII.CR, ASCII.LF, " "c)
    End Sub

    Public Function GetParserErrors() As String
        Return psErrors
    End Function

    Public Sub ClearParserError()
        psErrors = "*"
    End Sub

    ''' <summary>
    ''' parse a json file
    ''' </summary>
    ''' <param name="file">
    ''' a file path of the json data file
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Open(file As String) As JsonElement
        Using sr As New StreamReader(file)
            Return New JsonParser(sr.ReadToEnd).OpenJSON()
        End Using
    End Function

    ''' <summary>
    ''' parse json text content
    ''' </summary>
    ''' <returns>
    ''' this function will returns nothing if the given json string is empty string or "null" literal.
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function OpenJSON() As JsonElement
        If json_str Is Nothing Then
            Return Nothing
        ElseIf json_str Like "null" Then
            Return Nothing
        ElseIf json_str = "" Then
            Return Nothing
        Else
            _JSONvalue = _parse()
        End If

        Return JSONvalue
    End Function

    ''' <summary>
    ''' parse the text in json format
    ''' </summary>
    ''' <param name="json">
    ''' text data in json string format
    ''' </param>
    ''' <returns>
    ''' this function will returns nothing if the given json string is empty string or "null" literal.
    ''' </returns>
    Public Shared Function Parse(json As String) As JsonElement
        If json.StringEmpty Then
            Return Nothing
        Else
            Return New JsonParser(json).OpenJSON()
        End If
    End Function

    ''' <summary>
    ''' parse string and create JSON object
    ''' </summary>
    ''' <returns></returns>
    Private Function _parse() As JsonElement
        Dim tokens As New List(Of Token)

        Do While Not json_str.EndRead
            ' no hjson comment
            ' hjson comment will be skiped
            Call tokens.AddRange(walkChar(++json_str))
        Loop


    End Function

    Private Iterator Function walkChar(c As Char) As IEnumerable(Of Token)
        If comment_escape Then
            If c = ASCII.CR OrElse c = ASCII.LF Then
                comment_escape = False
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
        ElseIf c = "'"c OrElse c = """"c Then
            escape = c
            Return
        ElseIf c = ":"c Then
            ' end previous token
            ' key: value
            Yield New Token(Token.JSONElements.Key, buffer.PopAllChars)
            Yield New Token(Token.JSONElements.Colon, ":")
        ElseIf c = "," Then
            ' end previous token
            Yield MeasureToken()
            Yield New Token(Token.JSONElements.Delimiter, ",")
        ElseIf c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.CR OrElse c = ASCII.LF Then
            Yield MeasureToken()
        Else
            buffer += c
        End If
    End Function

    Private Function MeasureToken() As Token
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

    ''' <summary>
    ''' do string unescape
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="decodeMetaChar"></param>
    ''' <returns></returns>
    Public Shared Function StripString(ByRef str$, decodeMetaChar As Boolean) As String
        Dim index% = 0
        Dim chr As Char, code$
        Dim sb As New StringBuilder
        Dim str_len As Integer = Len(str)

        If str Is Nothing OrElse str = "" Then
            Return str
        End If

        While index < str_len
            chr = str(index)

            Select Case chr
                Case "\"c
                    index += 1

                    If decodeMetaChar Then
                        chr = str(index)

                        Select Case chr
                            Case """", "\", "/", """"
                                sb.Append(chr)
                                index += 1
                            Case "b"
                                sb.Append(vbBack)
                                index += 1
                            Case "f"
                                sb.Append(vbFormFeed)
                                index += 1
                            Case "n"
                                sb.Append(vbLf)
                                index += 1
                            Case "r"
                                sb.Append(vbCr)
                                index += 1
                            Case "t"
                                sb.Append(vbTab)
                                index += 1
                            Case "u"
                                index += 1
                                code = Mid(str, index, 4)
                                sb.Append(ChrW(Val("&h" & code)))
                                index += 4
                        End Select
                    Else
                        sb.Append(chr)
                    End If
                Case Else
                    sb.Append(chr)
                    index += 1
            End Select
        End While

        Return sb.ToString
    End Function
End Class
