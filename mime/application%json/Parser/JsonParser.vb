#Region "Microsoft.VisualBasic::22c56b260c4b5a9b9cf9a1824798f5c0, mime\application%json\Parser\JsonParser.vb"

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

    '   Total Lines: 486
    '    Code Lines: 333 (68.52%)
    ' Comment Lines: 94 (19.34%)
    '    - Xml Docs: 59.57%
    ' 
    '   Blank Lines: 59 (12.14%)
    '     File Size: 16.76 KB


    ' Class JsonParser
    ' 
    '     Properties: JSONvalue
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: _parse, GetParserErrors, GetTokenSequence, MeasureToken, Open
    '               OpenJSON, Parse, PullArray, PullJson, PullObject
    '               StripString, walkChar
    ' 
    '     Sub: ClearParserError
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
Imports Microsoft.VisualBasic.Text.Parser
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

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

    Dim ps As TokenIcer
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
        Me.lineText = Strings _
            .Trim(json_str) _
            .Trim(ASCII.TAB, ASCII.CR, ASCII.LF, " "c) _
            .LineTokens
        Me.ps = New StringTokenIcer(lineText.JoinBy(vbLf), strictVectorSyntax)
    End Sub

    Sub New(s As StreamReader, Optional strictVectorSyntax As Boolean = True)
        Me.strictVectorSyntax = strictVectorSyntax
        Me.ps = New StreamTokenIcer(s, strictVectorSyntax)
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
        If ps.CheckError Then
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
        Dim tokens As IEnumerator(Of Token) = ps.GetTokenSequence().GetEnumerator

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

            If t Is Nothing Then
                ' InvalidOperationException: Enumeration already finished.
                Dim message As String = "in-complete json object at the end of the document stream!"

                If strictVectorSyntax Then
                    Throw New Exception(message)
                Else
                    Call message.Warning
                    Call VBDebugger.EchoLine(message)
                End If

                Exit Do
            End If
            If t.name <> Token.JSONElements.Delimiter Then
                If t = (Token.JSONElements.Close, "}") Then
                    Exit Do
                Else
                    Throw New InvalidDataException($"a comma delimiter or json object close symbol should be follow the end of key:value tuple! (json_document_line: {t.span.line}, in-complete_json_obj: {obj.BuildJsonString})")
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

            Call array.Add(PullJson(pull))

            If pull.MoveNext() Then
                t = pull.Current
            Else
                ' InvalidOperationException: Enumeration already finished.
                t = Nothing
            End If

            If t Is Nothing Then
                If strictVectorSyntax Then
                    Throw New InvalidDataException($"in-complete json array! (json_document_line: {t.span.line})")
                Else
                    Dim message As String = $"in-complete json array: possible json syntax error on parse json array at line {t.span.line}."

                    Call message.Warning
                    Call Debug.WriteLine(message)

                    Exit Do
                End If
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
                                ' unescape the unicode characters
                                index += 1
                                code = Mid(str, index + 1, 4)
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
