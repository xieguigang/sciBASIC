#Region "Microsoft.VisualBasic::9a5e2355fb3d961a2b5d43b4d7fbcbc0, sciBASIC#\mime\text%markdown\DocumentHelper.vb"

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

    '   Total Lines: 122
    '    Code Lines: 80
    ' Comment Lines: 19
    '   Blank Lines: 23
    '     File Size: 4.61 KB


    ' Class DocumentToken
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Module DocumentHelper
    ' 
    ' 
    '     Enum TokenType
    ' 
    '         Tag, Text
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: Normalize, TokenizeHTML
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Friend Class DocumentToken : Inherits CodeToken(Of TokenType)

    Sub New(name As TokenType, text$)
        Call MyBase.New(name, text)
    End Sub
End Class

Module DocumentHelper

    ''' <summary>
    ''' maximum nested depth of [] and () supported by the transform; implementation detail
    ''' </summary>
    Public Const _nestDepth As Integer = 6

    ReadOnly internalRepeats$ = RepeatString($" {vbCrLf}            (<[A-Za-z\/!$](?:[^<>]|", _nestDepth) & RepeatString(")*>)", _nestDepth)
    ReadOnly htmlTokensRegexp As String = "

                (<!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->)|        # match <!-- foo -->
                (<\?.*?\?>)|                                         # match <?foo?> " & vbCrLf &
            internalRepeats & "                                  # match <tag> and </tag>"

    Dim _htmlTokens As New Regex(htmlTokensRegexp, RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.ExplicitCapture Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    Public Enum TokenType
        Text
        Tag
    End Enum

    ''' <summary>
    ''' returns an array of HTML tokens comprising the input string. Each token is 
    ''' either a tag (possibly with nested, tags contained therein, such 
    ''' as &lt;a href="&lt;MTFoo&gt;"&gt;, or a run of text between tags. Each element of the 
    ''' array is a two-element array; the first is either 'tag' or 'text'; the second is 
    ''' the actual value.
    ''' </summary>
    Public Function TokenizeHTML(text As String) As List(Of DocumentToken)
        Dim pos As Integer = 0
        Dim tagStart As Integer = 0
        Dim tokens As New List(Of DocumentToken)()

        ' this regex is derived from the _tokenize() subroutine in Brad Choate's MTRegex plugin.
        ' http://www.bradchoate.com/past/mtregex.php
        For Each m As Match In _htmlTokens.Matches(text)
            tagStart = m.Index

            If pos < tagStart Then
                tokens += New DocumentToken(TokenType.Text, text.Substring(pos, tagStart - pos))
            End If

            tokens += New DocumentToken(TokenType.Tag, m.Value)
            pos = tagStart + m.Length
        Next

        If pos < text.Length Then
            tokens += New DocumentToken(TokenType.Text, text.Substring(pos, text.Length - pos))
        End If

        Return tokens
    End Function

    ''' <summary>
    ''' convert all tabs to _tabWidth spaces; 
    ''' standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
    ''' makes sure text ends with a couple of newlines; 
    ''' removes any blank lines (only spaces) in the text
    ''' </summary>
    Public Function Normalize(text As String) As String
        Dim output = New StringBuilder(text.Length)
        Dim line = New StringBuilder()
        Dim valid As Boolean = False

        For i As Integer = 0 To text.Length - 1
            Select Case text(i)
                Case ControlChars.Lf
                    If valid Then
                        output.Append(line)
                    End If
                    output.Append(ControlChars.Lf)
                    line.Length = 0
                    valid = False

                Case ControlChars.Cr
                    If (i < text.Length - 1) AndAlso (text(i + 1) <> ControlChars.Lf) Then
                        If valid Then
                            output.Append(line)
                        End If
                        output.Append(ControlChars.Lf)
                        line.Length = 0
                        valid = False
                    End If

                Case ControlChars.Tab
                    Dim width As Integer = (_tabWidth - line.Length Mod _tabWidth)
                    For k As Integer = 0 To width - 1
                        line.Append(" "c)
                    Next

                Case ChrW(26)

                Case Else
                    If Not valid AndAlso text(i) <> " "c Then
                        valid = True
                    End If
                    line.Append(text(i))

            End Select
        Next

        If valid Then
            output.Append(line)
        End If
        output.Append(ControlChars.Lf)

        ' add two newlines to the end before return
        Return output.Append(vbLf & vbLf).ToString()
    End Function
End Module
