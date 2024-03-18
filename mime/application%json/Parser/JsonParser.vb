﻿#Region "Microsoft.VisualBasic::180411b12eaddeb1a5d4a195281c225b, sciBASIC#\mime\application%json\Parser\JsonParser.vb"

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

Imports System.Data
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
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
    Dim comments As New Dictionary(Of String, String)
    Dim comment_key As String = ""

    Dim lastToken As Token
    Dim line As Integer = 0
    Dim lineText As String()

    ''' <summary>
    ''' set this option to value false will enable syntax of array like: [1 2 3], 
    ''' without comma symbol as the element value delimiter!
    ''' </summary>
    Dim strictVectorSyntax As Boolean = True

    ''' <summary>
    ''' The root node in json file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property JSONvalue As JsonElement

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="json_str">the json text content</param>
    Sub New(json_str As String, Optional strictVectorSyntax As Boolean = True)
        Me.strictVectorSyntax = strictVectorSyntax
        Me.buffer = ""
        Me.lineText = Strings _
            .Trim(json_str) _
            .Trim(ASCII.TAB, ASCII.CR, ASCII.LF, " "c) _
            .LineTokens
        Me.json_str = lineText.JoinBy(vbLf)
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
    Public Shared Function Open(file As String, Optional strictVectorSyntax As Boolean = True) As JsonElement
        Using sr As New StreamReader(file)
            Return New JsonParser(sr.ReadToEnd, strictVectorSyntax:=strictVectorSyntax).OpenJSON()
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
    Public Shared Function Parse(json As String, Optional strictVectorSyntax As Boolean = True) As JsonElement
        If json.StringEmpty Then
            Return Nothing
        Else
            Return New JsonParser(json, strictVectorSyntax:=strictVectorSyntax).OpenJSON()
        End If
    End Function

    ''' <summary>
    ''' parse string and create JSON object
    ''' </summary>
    ''' <returns></returns>
    Private Function _parse() As JsonElement
        Dim tokens As IEnumerator(Of Token) = GetTokenSequence() _
            .ToArray _
            .AsEnumerable _
            .GetEnumerator

        If Not tokens.MoveNext Then
            ' empty collection 
            Return Nothing
        End If

        If tokens.Current.IsJsonValue Then
            Dim scalar As JsonValue = tokens.Current.GetValue

            If tokens.MoveNext Then
                Throw New InvalidExpressionException("the json literal value should be a scalar token value!")
            Else
                Return scalar
            End If
        Else
            Return PullJson(tokens)
        End If
    End Function

    Private Function PullJson(pull As IEnumerator(Of Token)) As JsonElement
        Dim t As Token = pull.Current

        If t Is Nothing Then
            Return Nothing
        End If

        Select Case t.name
            Case Token.JSONElements.Open
                If t.text = "{" Then
                    Return PullObject(pull)
                Else
                    Return PullArray(pull)
                End If
            Case Else
                If t.IsJsonValue Then
                    Return t.GetValue
                Else
                    Throw New InvalidProgramException($"invalid json syntax: the required token should be literal, object open or array open! (json_document_line: {t.span.line})")
                End If
        End Select
    End Function

    Private Function PullObject(pull As IEnumerator(Of Token)) As JsonObject
        Dim obj As New JsonObject
        Dim t As Token
        Dim key As String
        Dim val As JsonElement

        Do While pull.MoveNext
            ' get key
            t = pull.Current

            If t Is Nothing Then
                Throw New InvalidDataException("key should not be nothing")
            ElseIf t = (Token.JSONElements.Close, "}") Then
                ' empty json object {}
                Exit Do
            Else
                key = t.text
            End If

            t = pull.Next

            If t Is Nothing Then
                Throw New InvalidDataException($"in-complete json object document! (json_document_line: {t.span.line})")
            ElseIf t.name <> Token.JSONElements.Colon Then
                Throw New InvalidDataException($"missing colon symbol for key:value pair in json object document! (json_document_line: {t.span.line})")
            Else
                pull.MoveNext()
            End If

            val = PullJson(pull)
            obj.Add(key, val)
            t = pull.Next

            If t.name <> Token.JSONElements.Delimiter Then
                If t = (Token.JSONElements.Close, "}") Then
                    Exit Do
                Else
                    Throw New InvalidDataException($"a comma delimiter or json object close symbol should be follow the end of key:value tuple! (json_document_line: {t.span.line})")
                End If
            End If
        Loop

        Return obj
    End Function

    Private Function PullArray(pull As IEnumerator(Of Token)) As JsonArray
        Dim array As New JsonArray
        Dim t As Token
        Dim back As Boolean = False

        Do While If(back, True, pull.MoveNext())
            t = pull.Current
            back = False

            If t Is Nothing Then
                Throw New InvalidDataException($"in-complete json array! (json_document_line: {t.span.line})")
            ElseIf t = (Token.JSONElements.Close, "]") Then
                ' empty json array []
                Exit Do
            End If

            array.Add(PullJson(pull))
            pull.MoveNext()
            t = pull.Current

            If t Is Nothing Then
                Throw New InvalidDataException($"in-complete json array! (json_document_line: {t.span.line})")
            ElseIf t.name <> Token.JSONElements.Delimiter Then
                If t = (Token.JSONElements.Close, "]") Then
                    ' end of current vector
                    Exit Do
                ElseIf strictVectorSyntax Then
                    Throw New SyntaxErrorException($"the json element value should be follow a comma delimiter or close symbol of the array! (json_document_line: {t.span.line})")
                ElseIf t.name = Token.JSONElements.Open OrElse t.IsJsonValue Then
                    ' strict off mode will continute
                    ' try to parse next element
                    '
                    ' do nothing at here
                    Dim message As String = $"possible json syntax error on parse json array at line {t.span.line}."

                    ' stop iterator move to next in next loop
                    back = True

                    Call message.Warning
                    Call Debug.WriteLine(message)
                End If
            End If
        Loop

        Return array
    End Function

    Private Iterator Function GetTokenSequence() As IEnumerable(Of Token)
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
            Else
                Throw New Exception("unknow parser error at the end of the json document stream!")
            End If
        End If
    End Function

    Private Iterator Function walkChar(c As Char) As IEnumerable(Of Token)
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
