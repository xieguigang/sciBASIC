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
