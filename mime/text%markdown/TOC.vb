#Region "Microsoft.VisualBasic::a04620205deebab5edc82eb2b99947e0, mime\text%markdown\TOC.vb"

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

    '   Total Lines: 119
    '    Code Lines: 83 (69.75%)
    ' Comment Lines: 15 (12.61%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 21 (17.65%)
    '     File Size: 3.83 KB


    ' Module TOC
    ' 
    '     Function: AddToc, GetHeaders, ReplaceHeaders
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' TOC generator for markdown
''' </summary>
Public Module TOC

    ''' <summary>
    ''' 为makrdown文本添加目录
    ''' </summary>
    ''' <param name="md$"></param>
    ''' <returns></returns>
    Public Function AddToc(md As StringBuilder, Optional numbering As Boolean = True, Optional autoSave As Boolean = True) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine("<!-- vb.net-markdown-toc -->")
        Call sb.AppendLine()
        Call sb.AppendLine(md.ReplaceHeaders)
        Call sb.AppendLine()
        Call sb.AppendLine("<!-- vb.net-markdown-toc-config
	numbering=true
	autoSave=true
	/vb.net-markdown-toc-config -->
<!-- /vb.net-markdown-toc -->")

        Call sb.AppendLine(md.ToString)

        Return sb.ToString
    End Function

    <Extension> Private Function ReplaceHeaders(ByRef md As StringBuilder) As String
        Dim headers = GetHeaders(md.ToString)
        Dim i%() = {1, 1, 1, 1, 1, 1}
        Dim TOC As New List(Of String)

        For Each head In headers
            Dim parts = head.GetTagValue(" ", trim:=True)
            Dim level$ = parts.Name
            Dim indent$ = "   ".Repeats(level.Length - 1).JoinBy("")

            If level.Length > 4 Then
                Continue For
            End If

            If level.Length = 1 Then
                TOC += $"{i}. " & parts.Value
            End If

            level = $"<h{level.Length}>"

        Next

        Throw New NotImplementedException
    End Function

    Const regex_headerSetext$ = "
              ^(.+?)
              [ ]*
              \n
              (=+|-+)     # $1 = string of ='s or -'s
              [ ]*
              \n+"
    Const regex_headerAtx$ = "
              ^(\#{1,6})  # $1 = string of #'s
              [ ]*
              (.+?)       # $2 = Header text
              [ ]*
              \#*         # optional closing #'s (not counted)
              \n+"

    ReadOnly _headerSetext As New Regex(regex_headerSetext, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)
    ReadOnly _headerAtx As New Regex(regex_headerAtx, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' 按header在markdown文档之中出现的顺序进行返回
    ''' </summary>
    ''' <param name="md$"></param>
    ''' <returns></returns>
    Public Function GetHeaders(md$) As String()
        Dim headers As New List(Of String)

        headers += _headerSetext.Matches(md).ToArray
        headers += _headerAtx _
            .Matches(md) _
            .ToArray(Function(s) s.TrimNewLine.Trim)

        Dim orders As New List(Of SeqValue(Of String))
        Dim pos%

        For Each headerGroup As IGrouping(Of String, String) In headers.GroupBy(Function(s) s)
            ' start 参数必须要大于零
            pos = 1

            Do While True
                pos = InStr(pos, md, headerGroup.Key)

                If pos > 0 Then
                    orders += New SeqValue(Of String) With {
                        .i = pos,
                        .value = headerGroup.Key
                    }
                    ' 必须要往前位移一个字符，否则会出现死循环
                    pos += 1
                Else
                    Exit Do
                End If
            Loop
        Next

        Return orders _
            .OrderBy(Function(i) i.i) _
            .Select(Function(s) s.value) _
            .ToArray
    End Function
End Module
