#Region "Microsoft.VisualBasic::928b5849c2395d537c222244852be0cb, sciBASIC#\mime\text%markdown\Span\LinksParser.vb"

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

    '   Total Lines: 204
    '    Code Lines: 29
    ' Comment Lines: 169
    '   Blank Lines: 6
    '     File Size: 8.45 KB


    '     Module LinksParser
    ' 
    '         Function: InlineLink, InlineLinks
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text.Parser

Namespace Span

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <![CDATA[
    ''' <h2 id="span">Span Elements</h2>
    ''' <h3 id = "link" > Links</h3>
    '''
    ''' Markdown supports two style Of links: *inline* And *reference*.
    '''
    ''' In both styles, the link text Is delimited by [square brackets].
    '''
    ''' To create an inline link, use a set of regular parentheses immediately
    ''' after the link text's closing square bracket. Inside the parentheses,
    ''' put the URL where you want the link To point, along With an *Optional*
    ''' title for the link, surrounded in quotes. For example:
    '''
    '''     This Is [an example](http//example.com/ "Title") inline link.
    '''
    '''     [This link](http//example.net/) has no title attribute.
    '''
    ''' Will produce
    '''
    '''     <p> This Is <a href="http://example.com/" title="Title">
    '''     an example</a> inline link.</p>
    '''
    '''     <p> <a href = "http://example.net/" > This link</a> has no
    '''     title attribute.</p>
    '''
    ''' If you're referring to a local resource on the same server, you can
    ''' use relative paths:
    '''
    '''     See my [About](/about/) page For details.   
    '''
    ''' Reference-style links use a second set of square brackets, inside
    ''' which you place a label Of your choosing To identify the link:
    '''
    '''     This Is [an example][id] reference-style link.
    '''
    ''' You can optionally use a space To separate the sets Of brackets:
    '''
    '''     This Is [an example] [id] reference-style link.
    '''
    ''' Then, anywhere in the document, you define your link label Like this,
    ''' On a line by itself
    '''
    '''     [id]: http://example.com/  "Optional Title Here"
    '''
    ''' That Is
    '''
    ''' *   Square brackets containing the link identifier (optionally
    '''     indented from the left margin Using up To three spaces);
    ''' *   followed by a colon;
    ''' *   followed by one Or more spaces (Or tabs);
    ''' *   followed by the URL for the link;
    ''' *   optionally followed by a title attribute for the link, enclosed
    '''     in double Or single quotes, Or enclosed in parentheses.
    '''
    ''' The following three link definitions are equivalent
    '''
    '''	 [foo]: http://example.com/  "Optional Title Here"
    '''	 [foo]: http://example.com/  'Optional Title Here'
    '''	 [foo]: http://example.com/  (Optional Title Here)
    '''
    ''' **Note** There Is a known bug in Markdown.pl 1.0.1 which prevents
    ''' Single quotes from being used to delimit link titles.
    '''
    ''' The link URL may, optionally, be surrounded by angle brackets
    '''
    '''     [id]: <http: //example.com/>  "Optional Title Here"
    '''
    ''' You can put the title attribute On the Next line And use extra spaces
    ''' Or tabs for padding, which tends to look better with longer URLs
    '''
    '''     [id]: http://example.com/longish/path/to/resource/here
    '''         "Optional Title Here"
    '''
    ''' Link definitions are only used For creating links during Markdown
    ''' processing, And are stripped from your document in the HTML output.
    '''
    ''' Link definition names may consist Of letters, numbers, spaces, And
    ''' punctuation -- but they are *Not* case sensitive. E.g. these two
    ''' links:
    '''
    '''	 [link text][a]
    '''	 [link text][A]
    '''
    ''' are equivalent.
    '''
    ''' The *implicit link name* shortcut allows you to omit the name of the
    ''' link, in which case the link text itself Is used as the name.
    ''' Just use an empty Set Of square brackets -- e.g., To link the word
    ''' "Google" to the google.com web site, you could simply write:
    '''
    '''	 [Google][]
    '''
    ''' And then define the link
    '''
    '''	 [Google]: http://google.com/
    '''
    ''' Because link names may contain spaces, this shortcut even works For
    ''' multiple words In the link text:
    '''
    '''	 Visit [Daring Fireball][] for more information.
    '''
    ''' And then define the link
    '''
    ''' 	[Daring Fireball] http://daringfireball.net/
    '''
    ''' Link definitions can be placed anywhere In your Markdown document. I
    ''' tend to put them immediately after each paragraph in which they're
    ''' used, but if you want, you can put them all at the end of your
    ''' document, sort of Like footnotes.
    '''
    ''' Here's an example of reference links in action:
    '''
    '''     I get 10 times more traffic from [Google] [1] than from
    '''     [Yahoo] [2] Or [MSN] [3].
    '''
    '''       [1] http://google.com/        "Google"
    '''       [2]: http://search.yahoo.com/  "Yahoo Search"
    '''       [3]: http://search.msn.com/    "MSN Search"
    '''
    ''' Using the implicit link name shortcut, you could instead write:
    '''
    '''     I get 10 times more traffic from [Google][] than from
    '''     [Yahoo][] Or [MSN][].
    '''
    '''       [google]: http://google.com/        "Google"
    '''       [yahoo]:  http://search.yahoo.com/  "Yahoo Search"
    '''       [msn]:    http://search.msn.com/    "MSN Search"
    '''
    ''' Both of the above examples will produce the following HTML output
    '''
    '''     <p> I Get 10 times more traffic from <a href="http://google.com/"
    '''     title="Google">Google</a> than from
    '''     <a href = "http://search.yahoo.com/" title="Yahoo Search">Yahoo</a>
    '''     Or <a href="http://search.msn.com/" title="MSN Search">MSN</a>.</p>
    '''
    ''' For comparison, here Is the same paragraph written using
    ''' Markdown's inline link style:
    '''
    '''     I get 10 times more traffic from [Google](http//google.com/ "Google")
    '''     than from [Yahoo](http://search.yahoo.com/ "Yahoo Search") Or
    '''     [MSN](http://search.msn.com/ "MSN Search").
    '''
    ''' The point Of reference-style links Is Not that they're easier to
    ''' write. The point Is that with reference-style links, your document
    ''' source Is vastly more readable. Compare the above examples: Using
    ''' reference-style links, the paragraph itself Is only 81 characters
    ''' Long; With inline-style links, it's 176 characters; and as raw HTML,
    ''' it's 234 characters. In the raw HTML, there's more markup than there
    ''' Is text.
    '''
    ''' With Markdown's reference-style links, a source document much more
    ''' closely resembles the final output, As rendered In a browser. By
    ''' allowing you To move the markup-related metadata out Of the paragraph,
    ''' you can add links without interrupting the narrative flow Of your
    ''' prose.
    ''' ]]>
    Public Module LinksParser

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' This Is [an example](http//example.com/ "Title") inline link.
        '''
        ''' [This link](http//example.net/) has no title attribute.
        ''' </remarks>
        Public Iterator Function InlineLinks(s As String) As IEnumerable(Of ParserValue(Of Hyperlink))
            For Each x In Regex.Matches(s, "\[.*?\]\(.*?\)").EachValue(AddressOf InlineLink)
                Yield x
            Next
        End Function

        Public Function InlineLink(s As String) As ParserValue(Of Hyperlink)
            Dim text As String = Regex.Match(s, "^\[.*?\]", RegexOptions.Multiline).Value
            Dim link As String = s.Replace(text, "").GetStackValue("(", ")")
            Dim title As String = Regex.Match(link, "\s"".+?""").Value

            If Not String.IsNullOrEmpty(title) Then
                link = link.Replace(title, "")
                title = title.GetStackValue("""", """")
            End If
            text = text.GetStackValue("[", "]")

            Return New ParserValue(Of Hyperlink) With {
                .Raw = s,
                .value = New Hyperlink With {
                    .Links = link,
                    .Text = text,
                    .Title = title
                }
            }
        End Function
    End Module
End Namespace
