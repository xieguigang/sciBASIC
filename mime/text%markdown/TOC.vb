#Region "Microsoft.VisualBasic::b7f8261ed9f4d89d1f1d8f67549e86e6, mime\text%markdown\TOC.vb"

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

    '   Total Lines: 64
    '    Code Lines: 42 (65.62%)
    ' Comment Lines: 17 (26.56%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (7.81%)
    '     File Size: 2.63 KB


    ' Module TOC
    ' 
    '     Function: AddToc, GenerateToc, Slug
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Public Module TOC

    Private ReadOnly atxHeader As New Regex("^(#{1,6})\s+(.*?)\s*#*\s*$", RegexOptions.Multiline)

    ''' <summary>
    ''' Generate a nested markdown table of contents from the ATX headers
    ''' (``#`` .. ``######``) found in the document.
    ''' </summary>
    ''' <param name="md">The markdown source text.</param>
    ''' <param name="numbering">Reserved for future numbered list output.</param>
    ''' <returns>A markdown list string representing the document outline.</returns>
    Public Function GenerateToc(md As String, Optional numbering As Boolean = False) As String
        Dim sb As New StringBuilder
        For Each m As Match In atxHeader.Matches(md)
            Dim level = m.Groups(1).Value.Length
            Dim text = m.Groups(2).Value.Trim()
            If text.StringEmpty Then
                Continue For
            End If
            Dim indent = New String(" "c, (level - 1) * 2)
            Dim anchor = Slug(text)
            sb.AppendLine($"{indent}- [{text}](#{anchor})")
        Next
        Return sb.ToString.Trim(vbCr, vbLf, " "c)
    End Function

    ''' <summary>
    ''' Make a url friendly anchor slug out of a header text.
    ''' </summary>
    Private Function Slug(text As String) As String
        Dim s = text.ToLower()
        Dim sb As New StringBuilder
        For Each c As Char In s
            If Char.IsLetterOrDigit(c) OrElse c = " "c OrElse c = "-"c Then
                sb.Append(If(c = " "c, "-"c, c))
            End If
        Next
        Return sb.ToString
    End Function

    ''' <summary>
    ''' Insert a generated table of contents at the top of the markdown document.
    ''' </summary>
    ''' <param name="md">The markdown source text.</param>
    ''' <param name="numbering">Reserved for future numbered list output.</param>
    ''' <param name="autoSave">Reserved for future file persistence.</param>
    ''' <returns>The markdown document with a TOC block prepended.</returns>
    Public Function AddToc(md As String, Optional numbering As Boolean = False, Optional autoSave As Boolean = False) As String
        Dim toc = GenerateToc(md, numbering)
        Dim out As New StringBuilder
        out.AppendLine("<!-- markdown-toc -->")
        out.AppendLine()
        out.AppendLine(toc)
        out.AppendLine()
        out.AppendLine("<!-- /markdown-toc -->")
        out.AppendLine()
        out.Append(md)
        Return out.ToString
    End Function
End Module

