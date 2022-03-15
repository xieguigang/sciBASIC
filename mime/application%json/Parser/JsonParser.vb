#Region "Microsoft.VisualBasic::7ec6e0c967db0d273610991962a4683d, sciBASIC#\mime\application%json\Parser\JsonParser.vb"

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

    '   Total Lines: 498
    '    Code Lines: 330
    ' Comment Lines: 107
    '   Blank Lines: 61
    '     File Size: 16.03 KB


    ' Class JsonParser
    ' 
    '     Properties: JSONvalue
    ' 
    '     Function: GetParserErrors, Open, OpenJSON, parse, parseArray
    '               parseBoolean, parseKey, parseNull, parseNumber, parseObject
    '               parseString, parseValue, StripString
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
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

''' <summary>
''' https://github.com/qhgz2013/VBUtil/blob/master/VBUtil/JsonParser.vb
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
    ''' The root node in json file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property JSONvalue As JsonElement

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
    Public Function Open(file As String) As JsonElement
        Using sr As New StreamReader(file)
            Return OpenJSON(sr.ReadToEnd)
        End Using
    End Function

    ''' <summary>
    ''' parse json text content
    ''' </summary>
    ''' <param name="jsonStr">
    ''' the json text content
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function OpenJSON(jsonStr As String) As JsonElement
        _JSONvalue = parse(jsonStr)
        Return JSONvalue
    End Function

    ''' <summary>
    ''' parse string and create JSON object
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    Private Function parse(ByRef str As String) As JsonElement
        Dim index As Long = 1

        psErrors = "*"
        skipChar(str, index)

        Select Case Mid(str, index, 1)
            Case "{"
                Return parseObject(str, index)
            Case "["
                Return parseArray(str, index)
            Case Else
                psErrors = "Invalid JSON"
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' parse collections of key/value
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseObject(ByRef str As String, ByRef index As Long) As JsonObject
        Dim ret As New JsonObject
        Dim sKey As String

        '"{"
        skipChar(str, index)
        If Mid(str, index, 1) <> "{" Then
            psErrors &= "Invalid Object at position " & index & ":" & Mid(str, index) & vbCrLf
            Return ret
        End If

        index += 1

        Dim c As String

        Do
            skipChar(str, index)

            c = Mid(str, index, 1)

            If c = "}" Then
                index += 1
                Exit Do
            ElseIf c = "," Then
                index += 1
                skipChar(str, index)
            ElseIf index > Len(str) Then
                psErrors &= "Missing '}': " & Right(str, 20) & vbCrLf
                Exit Do
            End If

            'add key/value pair
            sKey = parseKey(str, index)
            ret.Add(sKey, parseValue(str, index))
            If Err.Number <> 0 Then
                psErrors &= Err.Description & ": " & sKey & vbCrLf
                Exit Do
            End If
        Loop
eh:
        Return ret
    End Function

    ''' <summary>
    ''' parse list
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseArray(ByRef str As String, ByRef index As Long) As JsonArray
        Dim ret As New JsonArray

        '"{"
        skipChar(str, index)

        If Mid(str, index, 1) <> "[" Then
            psErrors &= "Invalid Object at position " & index & ":" & Mid(str, index) & vbCrLf
            Return ret
        End If

        index += 1

        Dim c As String

        Do
            skipChar(str, index)

            c = Mid(str, index, 1)

            If c = "]" Then
                index += 1
                Exit Do
            ElseIf c = "," Then
                index += 1
                skipChar(str, index)
            ElseIf index > Len(str) Then
                psErrors &= "Missing '}': " & Right(str, 20) & vbCrLf
                Exit Do
            End If

            'add value
            ret.Add(parseValue(str, index))

            If Err.Number <> 0 Then
                psErrors = psErrors & Err.Description & ": " & Mid(str, index, 20) & vbCrLf
                Exit Do
            End If
        Loop

        Return ret
    End Function

    ''' <summary>
    ''' parse string/number/object/array/boolean/null
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseValue(ByRef str As String, ByRef index As Long) As JsonElement
        Call skipChar(str, index)

        Dim c As String = Mid(str, index, 1)

        Select Case c
            Case "{"
                Return parseObject(str, index)
            Case "["
                Return parseArray(str, index)
            Case """", "'"
                Return parseString(str, index)
            Case "t", "f"
                Return parseBoolean(str, index)
            Case "n"
                Return parseNull(str, index)
            Case Else
                Return parseNumber(str, index)
        End Select
    End Function

    Public Shared Function StripString(str$) As String
        Dim index% = 1
        Dim chr$, code$
        Dim sb As New StringBuilder

        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            Select Case chr
                Case "\"
                    index += 1
                    chr = Mid(str, index, 1)
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
                Case Else
                    sb.Append(chr)
                    index += 1
            End Select
        End While

        Return sb.ToString
    End Function

    ''' <summary>
    ''' parse string
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseString(ByRef str As String, ByRef index As Long) As JsonValue
        Dim quote, chr, code As String
        Dim sb As New StringBuilder

        skipChar(str, index)
        quote = Mid(str, index, 1)
        index += 1

        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            Select Case chr
                Case "\"
                    index += 1
                    chr = Mid(str, index, 1)

                    sb.Append("\") ' !!!!!

                    Select Case chr
                        Case """", "\", "/", """"
                            sb.Append(chr)
                            index += 1
                        Case "b"
                            ' sb.Append(vbBack)
                            sb.Append(chr)
                            index += 1
                        Case "f"
                            ' sb.Append(vbFormFeed)
                            sb.Append(chr)
                            index += 1
                        Case "n"
                            ' sb.Append(vbLf)
                            sb.Append(chr)
                            index += 1
                        Case "r"
                            ' sb.Append(vbCr)
                            sb.Append(chr)
                            index += 1
                        Case "t"
                            ' sb.Append(vbTab)
                            sb.Append(chr)
                            index += 1
                        Case "u"
                            index += 1
                            code = Mid(str, index, 4)
                            ' sb.Append(ChrW(Val("&h" & code)))
                            sb.Append(chr)
                            sb.Append(code)
                            index += 4
                    End Select
                Case quote
                    index += 1

                    Return New JsonValue($"""{sb.ToString}""")
                Case Else
                    sb.Append(chr)
                    index += 1
            End Select
        End While

        Return New JsonValue($"""{sb.ToString}""")
    End Function

    ''' <summary>
    ''' parse number
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseNumber(ByRef str As String, ByRef index As Long) As JsonValue
        Dim value As String = ""
        Dim chr As String

        Call skipChar(str, index)

        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)

            If InStr("+-0123456789.eE", chr) Then
                value &= chr
                index += 1
            ElseIf value = "" Then
                Dim textAround As String = Mid(str, index - 5, 10)
                Dim msg$ = $"unsure empty string for parse numeric value, text around the current pointer is: ""{textAround}""."

                Throw New InvalidCastException(msg)
            Else
                Return New JsonValue(CDbl(value))
            End If
        End While

        Return New JsonValue(CDbl(value))
    End Function

    ''' <summary>
    ''' parse true/false
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseBoolean(ByRef str As String, ByRef index As Long) As JsonValue

        skipChar(str, index)
        If Mid(str, index, 4) = "true" Then
            index += 4
            Return New JsonValue(True)
        ElseIf Mid(str, index, 5) = "false" Then
            index += 5
            Return New JsonValue(False)
        Else
            psErrors *= "Invalid Boolean at position " & index & " : " & Mid(str, index) & vbCrLf
        End If
        Return New JsonValue(False)
    End Function

    ''' <summary>
    ''' parse null
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Private Function parseNull(ByRef str As String, ByRef index As Long) As JsonValue
        Call skipChar(str, index)

        If Mid(str, index, 4) = "null" Then
            index += 4
            Return New JsonValue(Nothing)
        Else
            psErrors &= "Invalid null value at position " & index & " : " & Mid(str, index) & vbCrLf
        End If

        Return New JsonValue(Nothing)
    End Function

    Private Function parseKey(ByRef str As String, ByRef index As Long) As String
        Dim dquote, squote As Boolean
        Dim ret As String = ""
        Dim chr As String
        skipChar(str, index)
        While index > 0 AndAlso index <= Len(str)
            chr = Mid(str, index, 1)
            Select Case chr
                Case """"
                    dquote = Not dquote
                    index += 1
                    If Not dquote Then
                        skipChar(str, index)
                        If Mid(str, index, 1) <> ":" Then
                            psErrors &= "Invalid Key at position " & index & " : " & ret & vbCrLf
                            Exit While
                        End If
                    End If
                'Case "'"
                '    squote = Not squote
                '    index += 1
                '    If Not squote Then
                '        skipChar(str, index)
                '        If Mid(str, index, 1) <> ":" Then
                '            psErrors &= "Invalid Key at position " & index & " : " & ret & vbCrLf
                '            Exit While
                '        End If
                '    End If
                Case ":"
                    index += 1
                    If Not dquote AndAlso Not squote Then
                        Exit While
                    Else
                        ret &= chr
                    End If
                Case Else
                    If InStr(vbCrLf & vbCr & vbLf & vbTab & "", chr) Then
                    Else
                        ret &= chr
                    End If
                    index += 1
            End Select
        End While
        Return ret
    End Function

    ''' <summary>
    ''' skip special character
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="index"></param>
    Private Sub skipChar(ByRef str As String, ByRef index As Long)
        Dim bComment, bStartComment, bLongComment As Boolean
        While index > 0 AndAlso index <= Len(str)
            Select Case Mid(str, index, 1)
                Case vbCr, vbLf
                    If Not bLongComment Then
                        bStartComment = False
                        bComment = False
                    End If
                Case vbTab, " ", "(", ")"

                Case "/"
                    If Not bLongComment Then
                        If bStartComment Then
                            bStartComment = False
                            bComment = True
                        Else
                            bStartComment = True
                            bComment = False
                            bLongComment = False
                        End If
                    Else
                        If bStartComment Then
                            bLongComment = False
                            bStartComment = False
                            bComment = False
                        End If
                    End If
                Case "*"
                    If bStartComment Then
                        bStartComment = False
                        bComment = True
                        bLongComment = True
                    Else
                        bStartComment = True
                    End If

                Case Else
                    If Not bComment Then
                        Exit While
                    End If
            End Select

            index += 1
        End While
    End Sub
End Class
