#Region "Microsoft.VisualBasic::93244ff1d08f3061b31f69ec21893d4c, sciBASIC#\mime\text%markdown\MarkdownParser.vb"

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

    '   Total Lines: 141
    '    Code Lines: 64
    ' Comment Lines: 62
    '   Blank Lines: 15
    '     File Size: 4.54 KB


    ' Module MarkdownParser
    ' 
    '     Function: MarkdownParser, SyntaxParser
    '     Delegate Function
    ' 
    '         Function: BlockParser, IsHeader
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

''' <summary>
''' 在markdown里面有两张类型的标记语法：
''' 
''' + 一种是和普通的文本混合在一起的
''' + 一种是自己占有一整行文本或者一整个文本块的
''' 
''' </summary>
Public Module MarkdownParser

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path">The file path to the markdown text document.</param>
    ''' <returns></returns>
    Public Function MarkdownParser(path As String) As Markup
        Return path.ReadAllText.SyntaxParser
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="md">The markdown file text content, not file path</param>
    ''' <returns></returns>
    ''' <remarks>在这个函数之中只是解析区块的文本数据，段落型的格式则是在另外的一个模块之中解析的</remarks>
    <Extension>
    Public Function SyntaxParser(md As String) As Markup
        Dim lines As String() = md.LineTokens
        Dim result As New List(Of PlantText)

        For i As Integer = 0 To lines.Length - 1
            Dim s As String = lines(i)
            Dim block As PlantText = BlockParser(s, lines, i)

            If Not block Is Nothing Then
                result += block
            Else
                result += ParagraphParser(s, lines, i)
            End If
        Next

        Dim model As New Markup With {
            .nodes = result
        }
        Return model
    End Function

    Public Delegate Function IParser(s As String, lines As String(), ByRef i As Integer) As PlantText

    Private Function BlockParser(s As String, lines As String(), ByRef i As Integer) As PlantText
        Dim syn As PlantText = IsHeader(s, lines, i)

        If Not syn Is Nothing Then
            Return syn
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="lines"></param>
    ''' <param name="i"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <![CDATA[
    ''' <h3 id = "header" > Headers</h3>
    '''
    ''' Markdown supports two styles Of headers, [Setext] [1] And [atx] [2].
    '''
    ''' Setext-style headers are "underlined" using equal signs (for first-level
    ''' headers) And dashes (for second-level headers). For example:
    '''
    '''     This Is an H1
    '''     =============
    '''
    '''     This Is an H2
    '''     -------------
    '''
    ''' Any number Of underlining `=`'s or `-`'s will work.
    '''
    ''' Atx-style headers use 1-6 hash characters at the start of the line,
    ''' corresponding to header levels 1-6. For example:
    '''
    '''     # This Is an H1
    '''
    '''     ## This Is an H2
    '''
    '''     ###### This Is an H6
    '''
    ''' Optionally, you may "close" atx-style headers. This Is purely
    ''' cosmetic -- you can use this if you think it looks better. The
    ''' closing hashes don't even need to match the number of hashes
    ''' used to open the header. (The number of opening hashes
    ''' determines the header level.) 
    '''
    '''     # This Is an H1 #
    ''' 
    '''     ## This Is an H2 ##
    '''
    '''     ### This Is an H3 ######
    ''' ]]>
    Private Function IsHeader(s As String, lines As String(), ByRef i As Integer) As Header
        Dim m As String = Regex.Match(s, "^#+\s", RegexOptions.Multiline).Value

        If Not String.IsNullOrEmpty(m) Then
            Dim level As Integer = m.Count("#"c)
            If level > 6 Then
                level = 6
            End If
            s = Regex.Replace(s, "^#+", "", RegexOptions.Multiline)
            s = Regex.Replace(s, "\s#+$", "", RegexOptions.Multiline)
            s = s.Trim

            Return New Header With {
                .Level = level,
                .Text = s
            }
        Else
            If i + 1 = lines.Length Then
                Return Nothing
            End If

            Dim sNext As String = lines(i + 1)

            If Regex.Match(sNext, "^[=-]+$", RegexOptions.Multiline).Success Then
                i += 1
                Return New Header With {
                    .Level = 1,
                    .Text = s
                }
            Else
                Return Nothing
            End If
        End If
    End Function
End Module
