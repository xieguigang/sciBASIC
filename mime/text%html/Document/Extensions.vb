#Region "Microsoft.VisualBasic::0850d2b24cf5b46da074378116c059e4, mime\text%html\Document\Extensions.vb"

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

    '   Total Lines: 155
    '    Code Lines: 73 (47.10%)
    ' Comment Lines: 63 (40.65%)
    '    - Xml Docs: 36.51%
    ' 
    '   Blank Lines: 19 (12.26%)
    '     File Size: 8.18 KB


    '     Module Extensions
    ' 
    '         Function: Pagebreak, StripHTMLDirectly, StripHTMLSafely
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Document

    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' Since Markdown accepts plain HTML and CSS, simply add this line 
        ''' wherever you want to force page break.
        '''
        ''' ```html
        ''' &lt;div style="page-break-after: always;">&lt;/div>
        ''' ```
        ''' 
        ''' If your Markdown editor have trouble exporting PDF correctly, 
        ''' first Try To export As HTML, Then open With your browser And 
        ''' print As PDF.
        ''' 
        ''' > https://stackoverflow.com/questions/22601053/pagebreak-in-markdown-while-creating-pdf#29642392
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Pagebreak() As String
            Return (<div style="page-break-after: always;"></div>).ToString
        End Function

        ''' <summary>
        ''' Strip out HTML tags while preserving the basic formatting
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension> Public Function StripHTMLSafely(source As String) As String
            Try
                If source.StringEmpty Then
                    Return ""
                Else
                    Return StripHTMLDirectly(source)
                End If
            Catch ex As Exception
                Call App.LogException(New Exception(source, ex))
                Return source
            End Try
        End Function

        ''' <summary>
        ''' Strip out HTML tags while preserving the basic formatting
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>http://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text</remarks>
        Public Function StripHTMLDirectly(source As String) As String
            Dim result As String

            ' Remove HTML Development formatting
            ' Replace line breaks with space
            ' because browsers inserts space
            result = source.Replace(vbCr, " ")
            ' Replace line breaks with space
            ' because browsers inserts space
            result = result.Replace(vbLf, " ")
            ' Remove step-formatting
            result = result.Replace(vbTab, String.Empty)
            ' Remove repeating spaces because browsers ignore them
            result = Regex.Replace(result, "( )+", " ")

            ' Remove the header (prepare first by clearing attributes)
            result = Regex.Replace(result, "<( )*head([^>])*>", "<head>", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<( )*(/)( )*head( )*>)", "</head>", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<head>).*(</head>)", String.Empty, RegexOptions.IgnoreCase)

            ' remove all scripts (prepare first by clearing attributes)
            result = Regex.Replace(result, "<( )*script([^>])*>", "<script>", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<( )*(/)( )*script( )*>)", "</script>", RegexOptions.IgnoreCase)
            'result = Regex.Replace(result,
            '        "(<script>)([^(<script>\.</script>)])*(</script>)",
            '         string.Empty,
            '         RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<script>).*(</script>)", String.Empty, RegexOptions.IgnoreCase)

            ' remove all styles (prepare first by clearing attributes)
            result = Regex.Replace(result, "<( )*style([^>])*>", "<style>", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<( )*(/)( )*style( )*>)", "</style>", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(<style>).*(</style>)", String.Empty, RegexOptions.IgnoreCase)

            ' insert tabs in spaces of <td> tags
            result = Regex.Replace(result, "<( )*td([^>])*>", vbTab, RegexOptions.IgnoreCase)

            ' insert line breaks in places of <BR> and <LI> tags
            result = Regex.Replace(result, "<( )*br( )*>", vbCr, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "<( )*li( )*>", vbCr, RegexOptions.IgnoreCase)

            ' insert line paragraphs (double line breaks) in place
            ' if <P>, <DIV> and <TR> tags
            result = Regex.Replace(result, "<( )*div([^>])*>", vbCr & vbCr, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "<( )*tr([^>])*>", vbCr & vbCr, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "<( )*p([^>])*>", vbCr & vbCr, RegexOptions.IgnoreCase)

            ' Remove remaining tags like <a>, links, images,
            ' comments etc - anything that's enclosed inside < >
            result = Regex.Replace(result, "<[^>]*>", String.Empty, RegexOptions.IgnoreCase)

            ' replace special characters:
            result = Regex.Replace(result, " ", " ", RegexOptions.IgnoreCase)

            result = Regex.Replace(result, "&bull;", " * ", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&lsaquo;", "<", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&rsaquo;", ">", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&trade;", "(tm)", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&frasl;", "/", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&lt;", "<", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&gt;", ">", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&copy;", "(c)", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "&reg;", "(r)", RegexOptions.IgnoreCase)
            ' Remove all others. More can be added, see
            ' http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = Regex.Replace(result, "&(.{2,6});", String.Empty, RegexOptions.IgnoreCase)

            ' for testing
            'Regex.Replace(result,
            '       this.txtRegex.Text,string.Empty,
            '       RegexOptions.IgnoreCase)

            ' make line breaking consistent
            result = result.Replace(vbLf, vbCr)

            ' Remove extra line breaks and tabs:
            ' replace over 2 breaks with 2 and over 4 tabs with 4.
            ' Prepare first to remove any whitespaces in between
            ' the escaped characters and remove redundant tabs in between line breaks
            result = Regex.Replace(result, "(" & vbCr & ")( )+(" & vbCr & ")", vbCr & vbCr, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(" & vbTab & ")( )+(" & vbTab & ")", vbTab & vbTab, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(" & vbTab & ")( )+(" & vbCr & ")", vbTab & vbCr, RegexOptions.IgnoreCase)
            result = Regex.Replace(result, "(" & vbCr & ")( )+(" & vbTab & ")", vbCr & vbTab, RegexOptions.IgnoreCase)
            ' Remove redundant tabs
            result = Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+(" & vbCr & ")", vbCr & vbCr, RegexOptions.IgnoreCase)
            ' Remove multiple tabs following a line break with just one tab
            result = Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+", vbCr & vbTab, RegexOptions.IgnoreCase)
            ' Initial replacement target string for line breaks
            Dim breaks As String = vbCr & vbCr & vbCr
            ' Initial replacement target string for tabs
            Dim tabs As String = vbTab & vbTab & vbTab & vbTab & vbTab
            For index As Integer = 0 To result.Length - 1
                result = result.Replace(breaks, vbCr & vbCr)
                result = result.Replace(tabs, vbTab & vbTab & vbTab & vbTab)
                breaks = breaks & vbCr
                tabs = tabs & vbTab
            Next

            ' That's it.
            Return result
        End Function
    End Module
End Namespace
