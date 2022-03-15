#Region "Microsoft.VisualBasic::0f0e5f9f9f4663de61fb6839b0e12b7c, sciBASIC#\mime\text%markdown\Markdown.vb"

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

    '   Total Lines: 1531
    '    Code Lines: 867
    ' Comment Lines: 387
    '   Blank Lines: 277
    '     File Size: 62.48 KB


    ' Class MarkdownHTML
    ' 
    '     Properties: AllowEmptyLinkText, AsteriskIntraWordEmphasis, AutoHyperlink, AutoNewLines, DisableHeaders
    '                 DisableHr, DisableImages, EmptyElementSuffix, LinkEmails, QuoteSingleLine
    '                 StrictBoldItalic
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: AnchorInlineEvaluator, AnchorRefEvaluator, AnchorRefShortcutEvaluator, AtxHeaderEvaluator, BlockQuoteEvaluator
    '               BlockQuoteEvaluator2, CheckboxEvaluator, CodeBlockEvaluator, CodeSpanEvaluator, DoAnchors
    '               DoAutoLinks, DoBlockQuotes, DoCheckbox, DoCodeBlocks, DoCodeSpans
    '               DoHardBreaks, DoHeaders, DoHorizontalRules, DoImages, DoItalicsAndBold
    '               DoLists, EmailEvaluator, EncodeAmpsAndAngles, EncodeCode, EncodeCodeEvaluator
    '               EncodeEmailAddress, EscapeBackslashes, EscapeBackslashesEvaluator, EscapeBoldItalic, EscapeImageAltText
    '               EscapeSpecialCharsWithinTagAttributes, FormParagraphs, GetBlockPattern, GetNestedBracketsPattern, GetNestedParensPattern
    '               HashHTMLBlocks, HtmlEvaluator, HyperlinkEvaluator, ImageInlineEvaluator, ImageReferenceEvaluator
    '               ImageTag, LinkEvaluator, ListEvaluator, markdownTable, ProcessListItems
    '               RunBlockGamut, RunSpanGamut, SaveFromAutoLinking, SetextHeaderEvaluator, StripLinkDefinitions
    '               SyntaxedCodeBlockEvaluator, Transform, Unescape, UnescapeEvaluator
    ' 
    '     Sub: AddExtension, Cleanup, Setup
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' Markdown is a text-to-HTML conversion tool for web writers. 
''' Markdown allows you to write using an easy-to-read, easy-to-write plain text format, 
''' then convert it to structurally valid XHTML (or HTML).
''' (Markdown to HTML格式转换引擎)
''' </summary>
Public Class MarkdownHTML

    Const MarkerUL As String = "[*+-]"
    Const MarkerOL As String = "\d+[.]"

    ReadOnly _urls As New Dictionary(Of String, String)()
    ReadOnly _titles As New Dictionary(Of String, String)()
    ReadOnly _htmlBlocks As New Dictionary(Of String, String)()

    Dim _listLevel As Integer

    Shared ReadOnly AutoLinkPreventionMarker As String = ChrW(26) & "P"

#Region "Options"

    Public Property AllowEmptyLinkText() As Boolean

    ''' <summary>
    ''' Disable hr parser
    ''' </summary>
    Public Property DisableHr() As Boolean

    ''' <summary>
    ''' Disable header parser
    ''' </summary>
    Public Property DisableHeaders() As Boolean

    ''' <summary>
    ''' Disable image parser
    ''' </summary>
    Public Property DisableImages() As Boolean

    ''' <summary>
    ''' Don't grab next lines
    ''' </summary>
    Public Property QuoteSingleLine() As Boolean

    ''' <summary>
    ''' use ">" for HTML output, or " />" for XHTML output
    ''' </summary>
    Public Property EmptyElementSuffix() As String = " />"

    ''' <summary>
    ''' when false, email addresses will never be auto-linked  
    ''' WARNING: this is a significant deviation from the markdown spec
    ''' </summary>
    Public Property LinkEmails() As Boolean = True

    ''' <summary>
    ''' when true, bold and italic require non-word characters on either side  
    ''' WARNING: this is a significant deviation from the markdown spec
    ''' </summary>
    Public Property StrictBoldItalic() As Boolean

    ''' <summary>
    ''' when true, asterisks may be used for intraword emphasis
    ''' this does nothing if StrictBoldItalic is false
    ''' </summary>
    Public Property AsteriskIntraWordEmphasis() As Boolean

    ''' <summary>
    ''' when true, RETURN becomes a literal newline  
    ''' WARNING: this is a significant deviation from the markdown spec
    ''' </summary>
    Public Property AutoNewLines() As Boolean

    ''' <summary>
    ''' when true, (most) bare plain URLs are auto-hyperlinked  
    ''' WARNING: this is a significant deviation from the markdown spec
    ''' </summary>
    Public Property AutoHyperlink() As Boolean

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Create a new Markdown instance and set the options from the MarkdownOptions object.
    ''' </summary>
    Public Sub New(options As MarkdownOptions)
        If Not String.IsNullOrEmpty(options.EmptyElementSuffix) Then
            _EmptyElementSuffix = options.EmptyElementSuffix
        End If
        _AllowEmptyLinkText = options.AllowEmptyLinkText
        _DisableHr = options.DisableHr
        _DisableHeaders = options.DisableHeaders
        _DisableImages = options.DisableImages
        _QuoteSingleLine = options.QuoteSingleLine
        _AutoHyperlink = options.AutoHyperlink
        _AutoNewLines = options.AutoNewlines
        _LinkEmails = options.LinkEmails
        _StrictBoldItalic = options.StrictBoldItalic
        _AsteriskIntraWordEmphasis = options.AsteriskIntraWordEmphasis
    End Sub

    Sub New()
        Call Me.New(MarkdownOptions.DefaultOption.DefaultValue)
    End Sub
#End Region

    ''' <summary>
    ''' Transforms the provided Markdown-formatted text to HTML;  
    ''' see http://en.wikipedia.org/wiki/Markdown
    ''' (好像这个并不支持代码高亮的格式化)
    ''' </summary>
    ''' <param name="text">
    ''' Markdown文本
    ''' </param>
    ''' <remarks>
    ''' The order in which other subs are called here is
    ''' essential. Link and image substitutions need to happen before
    ''' EscapeSpecialChars(), so that any *'s or _'s in the a
    ''' and img tags get encoded.
    ''' </remarks>
    Public Function Transform(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return ""
        End If

        Setup()

        text = Normalize(text)

        text = HashHTMLBlocks(text)
        text = StripLinkDefinitions(text)
        text = RunBlockGamut(text)
        text = Unescape(text)

        Cleanup()

        Return text
    End Function

    ''' <summary>
    ''' Perform transformations that form block-level tags like paragraphs, headers, and list items.
    ''' </summary>
    Private Function RunBlockGamut(text As String, Optional unhash As Boolean = True, Optional createParagraphs As Boolean = True) As String
        ' Apply extensions
        For Each extension As ExtensionTransform In _inlineExtensions
            text = extension(text)
        Next

        ' 因为R或者python语言之中的注释符号是markdown之中的header的标记
        ' 所以为了不将其转义，需要将codeblock优先于header进行转换
        text = DoCodeBlocks(text)

        If Not _DisableHeaders Then
            text = DoHeaders(text)
        End If

        If Not _DisableHr Then
            text = DoHorizontalRules(text)
        End If

        ' 因为checkbox这个和list有相似的前缀，所以需要优先于listbox进行处理
        text = DoCheckbox(text)
        text = DoLists(text)
        text = DoBlockQuotes(text)

        ' We already ran HashHTMLBlocks() before, in Markdown(), but that
        ' was to escape raw HTML in the original Markdown source. This time,
        ' we're escaping the markup we've just created, so that we don't wrap
        ' <p> tags around block-level tags.
        text = HashHTMLBlocks(text)

        text = FormParagraphs(text, unhash:=unhash, createParagraphs:=createParagraphs)

        Return text
    End Function

    ReadOnly _inlineExtensions As New List(Of ExtensionTransform)()

    ''' <summary>
    ''' Public <see cref="System.Delegate"/> Function ExtensionTransform(text As <see cref="String"/>) As <see cref="String"/>
    ''' </summary>
    ''' <param name="ext"></param>
    Public Sub AddExtension(ext As ExtensionTransform)
        _inlineExtensions.Add(ext)
    End Sub

    ''' <summary>
    ''' Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
    ''' </summary>
    Private Function RunSpanGamut(text As String) As String
        ' 2018-10-23
        ' 因为有些代码之中经常出现的符号可能会在markdown之中存在含义
        ' 所以会需要首先将代码段进行处理，避免这些符号产生影响
        text = DoCodeSpans(text)
        text = EscapeSpecialCharsWithinTagAttributes(text)
        text = EscapeBackslashes(text)

        ' Images must come first, because ![foo][f] looks like an anchor.
        If Not _DisableImages Then
            text = DoImages(text)
        End If

        text = DoAnchors(text)

        ' Must come after DoAnchors(), because you can use < and >
        ' delimiters in inline links like [this](<url>).
        text = DoAutoLinks(text)

        text = text.Replace(AutoLinkPreventionMarker, "://")

        text = EncodeAmpsAndAngles(text)
        text = DoItalicsAndBold(text)
        text = DoHardBreaks(text)

        ' 怎样处理table??
        text = markdownTable(text)

        Return text
    End Function

    Const tableThread$ = "^|(-+[|]?)+|(<br\s*/>)?$"

    ''' <summary>
    ''' 处理markdown table
    ''' </summary>
    ''' <param name="text$"></param>
    ''' <returns></returns>
    Private Shared Function markdownTable(text$) As String
        Dim lines$() = text.LineTokens

        If text.StringEmpty OrElse lines.Length < 2 Then
            Return text
        End If

        For Each line In lines
            If line.First <> "|"c Then
                Return text  ' 不是table格式的，则直接返回原始文本
            End If
        Next

        If Not Regex.Match(lines(1), tableThread, RegexOptions.Multiline).Success Then
            Return text
        End If

        Dim sb As New StringBuilder("<table>" & vbCrLf)

        ' 表头
        ' 假设在表头之中是没有任何特殊字符的，在这里直接分割转换
        Dim t As New List(Of String)(lines(0).Split("|"c))

        If br.Match(t.Last).Success Then
            Call t.RemoveAt(t.Count - 1)
        End If

        Call t.RemoveAt(Scan0) ' 第一个|是不需要的
        Call sb.Append("<thead>")
        Call sb.Append("<tr><th>")
        Call sb.Append(t.JoinBy("</th><th>"))
        Call sb.Append("</th></tr>")
        Call sb.Append("</thead>")
        Call sb.AppendLine()

        ' 处理表中的每一行
        Dim r As New Dictionary(Of String, String)

        For Each line In lines.Skip(2)
            Dim code = Regex.Matches(line, "<code>.+?</code>", RegexICSng).ToArray

            Call r.Clear()

            For Each c In code
                r(c) = c.Replace("|", "&line;")
                line = line.Replace(c, r(c))
            Next

            line = "<tr><td>" & Mid(line, 2)                           ' 处理第一个标记
            If line.Last <> "|"c AndAlso line.Last = ">"c Then  ' 假设这个是br标记，如果是其他的标记，那么我也没有办法了
                Dim brTags = br.Matches(line).ToArray

                If brTags.Length > 0 Then
                    Dim brTag = brTags.Last
                    line = Mid(line, 1, line.Length - brTag.Length)
                End If
            End If
            line = Mid(line, 1, line.Length - 1) & "</td></tr>"        ' 处理最后一个标记
            line = line.Replace("|", "</td><td>")                      ' 处理每一个标记

            For Each c In r
                line = line.Replace(c.Value, c.Key)
            Next

            Call sb.AppendLine(line)
        Next

        Call sb.Append("</table>")

        Return sb.ToString
    End Function

    Shared ReadOnly br As New Regex("<br\s*/>", RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    Private Shared _newlinesLeadingTrailing As New Regex("^\n+|\n+\z", RegexOptions.Compiled)
    Private Shared _newlinesMultiple As New Regex("\n{2,}", RegexOptions.Compiled)
    Private Shared _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Compiled)

    Private Shared _htmlBlockHash As New Regex(ChrW(26) & "H\d+H", RegexOptions.Compiled)

    ''' <summary>
    ''' splits on two or more newlines, to form "paragraphs";    
    ''' each paragraph is then unhashed (if it is a hash and unhashing isn't turned off) or wrapped in HTML p tag
    ''' </summary>
    Private Function FormParagraphs(text As String, Optional unhash As Boolean = True, Optional createParagraphs As Boolean = True) As String
        ' split on two or more newlines
        Dim grafs As String() = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text, ""))

        For i As Integer = 0 To grafs.Length - 1
            If grafs(i).Contains(ChrW(26) & "H") Then
                ' unhashify HTML blocks
                If unhash Then
                    Dim sanityCheck As Integer = 50
                    ' just for safety, guard against an infinite loop
                    Dim keepGoing As Boolean = True
                    ' as long as replacements where made, keep going
                    While keepGoing AndAlso sanityCheck > 0
                        keepGoing = False
                        grafs(i) = _htmlBlockHash.Replace(grafs(i), Function(match)
                                                                        keepGoing = True
                                                                        Return _htmlBlocks(match.Value)

                                                                    End Function)
                        sanityCheck -= 1
                        ' if (keepGoing)
                        '                        {
                        '                            // Logging of an infinite loop goes here.
                        '                            // If such a thing should happen, please open a new issue on http://code.google.com/p/markdownsharp/
                        '                            // with the input that caused it.
                        '                        }

                    End While
                End If
            Else
                ' do span level processing inside the block, then wrap result in <p> tags
                grafs(i) = _leadingWhitespace.Replace(RunSpanGamut(grafs(i)), If(createParagraphs, "<p>", "")) & (If(createParagraphs, "</p>", ""))
            End If
        Next

        Return String.Join(vbLf & vbLf, grafs)
    End Function


    Private Sub Setup()
        ' Clear the global hashes. If we don't clear these, you get conflicts
        ' from other articles when generating a page which contains more than
        ' one article (e.g. an index page that shows the N most recent
        ' articles):
        _urls.Clear()
        _titles.Clear()
        _htmlBlocks.Clear()
        _listLevel = 0
    End Sub

    Private Sub Cleanup()
        Setup()
    End Sub

    Private Shared _nestedBracketsPattern As String

    ''' <summary>
    ''' Reusable pattern to match balanced [brackets]. See Friedl's 
    ''' "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
    ''' </summary>
    Private Shared Function GetNestedBracketsPattern() As String
        ' in other words [this] and [this[also]] and [this[also[too]]]
        ' up to _nestDepth
        If _nestedBracketsPattern Is Nothing Then
            _nestedBracketsPattern = RepeatString(vbCr & vbLf & "                    (?>              # Atomic matching" & vbCr & vbLf & "                       [^\[\]]+      # Anything other than brackets" & vbCr & vbLf & "                     |" & vbCr & vbLf & "                       \[" & vbCr & vbLf & "                           ", _nestDepth) & RepeatString(" \]" & vbCr & vbLf & "                    )*", _nestDepth)
        End If
        Return _nestedBracketsPattern
    End Function

    Private Shared _nestedParensPattern As String

    ''' <summary>
    ''' Reusable pattern to match balanced (parens). See Friedl's 
    ''' "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
    ''' </summary>
    Private Shared Function GetNestedParensPattern() As String
        ' in other words (this) and (this(also)) and (this(also(too)))
        ' up to _nestDepth
        If _nestedParensPattern Is Nothing Then

            ' 2017-1-23
            ' 这里的原始表达式为：
            ' [^()\s]+     # Anything other than parens or whitespace
            ' 这个表达式不能够匹配含有空格的路径，现在将\s去除掉之后经过测试也没有发现太多问题
            _nestedParensPattern = RepeatString("

    (?>              # Atomic matching
       [^()]+        # Anything other than parens or whitespace
       |
       \(
        ", _nestDepth) & RepeatString(" \)
      )*", _nestDepth)

        End If
        Return _nestedParensPattern
    End Function

    Shared ReadOnly _linkDef As New Regex(String.Format("

        ^[ ]{{0,{0}}}\[([^\[\]]+)\]:  # id = $1
            [ ]*
                \n?                   # maybe *one* newline
            [ ]*
              <?(\S+?)>?              # url = $2
            [ ]*
                \n?                   # maybe one newline
            [ ]*
                (?:
                  (?<=\s)             # lookbehind for whitespace
                  [""(]
                  (.+?)               # title = $3
                  ["")]
            [ ]*
              )?                      # title is optional
            (?:\n+|\Z)", _tabWidth - 1), RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Strips link definitions from text, stores the URLs and titles in hash references.
    ''' </summary>
    ''' <remarks>
    ''' ^[id]: url "optional title"
    ''' </remarks>
    Private Function StripLinkDefinitions(text As String) As String
        Return _linkDef.Replace(text, New MatchEvaluator(AddressOf LinkEvaluator))
    End Function

    Private Function LinkEvaluator(match As Match) As String
        Dim linkID As String = match.Groups(1).Value.ToLowerInvariant()
        _urls(linkID) = EncodeAmpsAndAngles(match.Groups(2).Value)

        If match.Groups(3) IsNot Nothing AndAlso match.Groups(3).Length > 0 Then
            _titles(linkID) = match.Groups(3).Value.Replace("""", "&quot;")
        End If

        Return ""
    End Function

    ''' <summary>
    ''' compiling this monster regex results in worse performance. trust me.
    ''' </summary>
    Private Shared _blocksHtml As New Regex(GetBlockPattern(), RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)

    ''' <summary>
    ''' First, look for nested blocks, e.g.:
    '''     
    '''     &lt;div>
    ''' 	    &lt;div>
    ''' 		tags for inner block must be indented.
    ''' 		&lt;/div>
    ''' 	&lt;/div>
    '''
    ''' The outermost tags must start at the left margin for this to match, and
    ''' the inner nested divs must be indented.
    ''' We need to do this before the next, more liberal match, because the next
    ''' match will start at the first ``&lt;div>`` and stop at the first ``&lt;/div>``.
    ''' </summary>
    Const pattern$ = "
        (?>
        (?>
          (?<=\n)     # Starting at the beginning of a line
          |           # or
          \A\n?       # the beginning of the doc
        )
        (             # save in $1

                      # Match from `\n<tag>` to `</tag>\n`, handling nested tags 
                      # in between.

           <($block_tags_b_re)   # start tag = $2
           $attr>                # attributes followed by > and \n
           $content              # content, support nesting
           </\2>                 # the matching end tag
           [ ]*                  # trailing spaces
           (?=\n+|\Z)            # followed by a newline or end of document

                               | # Special version for tags of group a.

           <($block_tags_a_re)   # start tag = $3
           $attr>[ ]*\n          # attributes followed by >
           $content2             # content, support nesting
           </\3>                 # the matching end tag
           [ ]*                  # trailing spaces
           (?=\n+|\Z)            # followed by a newline or end of document

                               | # Special case just for <hr />. It was easier to make a special 
                                 # case than to make the other regex more complicated.

           [ ]{0,$less_than_tab}
           <hr
           $attr                 # attributes
           /?>                   # the matching end tag
           [ ]*
           (?=\n{2,}|\Z)         # followed by a blank line or end of document

                               | # Special case for standalone HTML comments:

           (?<=\n\n|\A)            # preceded by a blank line or start of document
           [ ]{0,$less_than_tab}
           (?s:
              <!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->
           )
           [ ]*
          (?=\n{2,}|\Z)            # followed by a blank line or end of document

                                 | # PHP and ASP-style processor instructions (<? and <%)

           [ ]{0,$less_than_tab}
          (?s:
            <([?%])                # $4
             .*?
            \4>
          )
           [ ]*
          (?=\n{2,}|\Z)            # followed by a blank line or end of document

          )
        )"

    ''' <summary>
    ''' derived pretty much verbatim from PHP Markdown
    ''' </summary>
    Private Shared Function GetBlockPattern() As String

        ' Hashify HTML blocks:
        ' We only want to do this for block-level HTML tags, such as headers,
        ' lists, and tables. That's because we still want to wrap <p>s around
        ' "paragraphs" that are wrapped in non-block-level tags, such as anchors,
        ' phrase emphasis, and spans. The list of tags we're looking for is
        ' hard-coded:
        '
        ' *  List "a" is made of tags which can be both inline or block-level.
        '    These will be treated block-level when the start tag is alone on 
        '    its line, otherwise they're not matched here and will be taken as 
        '    inline later.
        ' *  List "b" is made of tags which are always block-level;
        '
        Dim blockTagsA As String = "ins|del"
        Dim blockTagsB As String = "p|div|h[1-6]|blockquote|pre|table|dl|ol|ul|address|script|noscript|form|fieldset|iframe|math"

        ' Regular expression for the content of a block tag.
        Dim attr As String = vbCr & vbLf & "            (?>" & vbTab & vbTab & vbTab & vbTab & "            # optional tag attributes" & vbCr & vbLf & "              \s" & vbTab & vbTab & vbTab & "            # starts with whitespace" & vbCr & vbLf & "              (?>" & vbCr & vbLf & "                [^>""/]+" & vbTab & "            # text outside quotes" & vbCr & vbLf & "              |" & vbCr & vbLf & "                /+(?!>)" & vbTab & vbTab & "            # slash not followed by >" & vbCr & vbLf & "              |" & vbCr & vbLf & "                ""[^""]*""" & vbTab & vbTab & "        # text inside double quotes (tolerate >)" & vbCr & vbLf & "              |" & vbCr & vbLf & "                '[^']*'" & vbTab & "                # text inside single quotes (tolerate >)" & vbCr & vbLf & "              )*" & vbCr & vbLf & "            )?" & vbTab & vbCr & vbLf & "            "

        ' end of opening tag
        ' last level nested tag content
        Dim content As String = RepeatString(vbCr & vbLf & "                (?>" & vbCr & vbLf & "                  [^<]+" & vbTab & vbTab & vbTab & "        # content without tag" & vbCr & vbLf & "                |" & vbCr & vbLf & "                  <\2" & vbTab & vbTab & vbTab & "        # nested opening tag" & vbCr & vbLf & "                    " & attr & "       # attributes" & vbCr & vbLf & "                  (?>" & vbCr & vbLf & "                      />" & vbCr & vbLf & "                  |" & vbCr & vbLf & "                      >", _nestDepth) & ".*?" & RepeatString(vbCr & vbLf & "                      </\2\s*>" & vbTab & "        # closing nested tag" & vbCr & vbLf & "                  )" & vbCr & vbLf & "                  |" & vbTab & vbTab & vbTab & vbTab & vbCr & vbLf & "                  <(?!/\2\s*>           # other tags with a different name" & vbCr & vbLf & "                  )" & vbCr & vbLf & "                )*", _nestDepth)

        Dim content2 As String = content.Replace("\2", "\3")
        Dim pattern$ = MarkdownHTML.pattern

        pattern = pattern.Replace("$less_than_tab", (_tabWidth - 1).ToString())
        pattern = pattern.Replace("$block_tags_b_re", blockTagsB)
        pattern = pattern.Replace("$block_tags_a_re", blockTagsA)
        pattern = pattern.Replace("$attr", attr)
        pattern = pattern.Replace("$content2", content2)
        pattern = pattern.Replace("$content", content)

        Return pattern
    End Function

    ''' <summary>
    ''' replaces any block-level HTML blocks with hash entries
    ''' </summary>
    Private Function HashHTMLBlocks(text As String) As String
        Return _blocksHtml.Replace(text, New MatchEvaluator(AddressOf HtmlEvaluator))
    End Function

    Private Function HtmlEvaluator(match As Match) As String
        Dim text As String = match.Groups(1).Value
        Dim key As String = GetHashKey(text, isHtmlBlock:=True)
        _htmlBlocks(key) = text

        Return String.Concat(vbLf & vbLf, key, vbLf & vbLf)
    End Function


    Private Shared _anchorRef As New Regex(String.Format("
        (                               # wrap whole match in $1
            \[
                ({0})                   # link text = $2
            \]

            [ ]?                        # one optional space
            (?:\n[ ]*)?                 # one optional newline followed by spaces

            \[
                (.*?)                   # id = $3
            \]
        )", GetNestedBracketsPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    Private Shared _anchorInline As New Regex(String.Format("
        (                           # wrap whole match in $1
            \[
                ({0})               # link text = $2
            \]
            \(                      # literal paren
                [ ]*
                ({1})               # href = $3
                [ ]*
                (                   # $4
                  (['""])           # quote char = $5
                (.*?)               # title = $6
                \5                  # matching quote
                [ ]*                # ignore any spaces between closing quote and )
                )?                  # title is optional
            \)
        )", GetNestedBracketsPattern(), GetNestedParensPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    Private Shared _anchorRefShortcut As New Regex("
        (                               # wrap whole match in $1
             \[
             ([^\[\]]+)                 # link text = $2; can't contain [ or ]
             \]
        )", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown link shortcuts into HTML anchor tags
    ''' </summary>
    ''' <remarks>
    ''' [link text](url "title") 
    ''' [link text][id] 
    ''' [id] 
    ''' </remarks>
    Private Function DoAnchors(text As String) As String
        If Not text.Contains("[") Then
            Return text
        End If

        ' First, handle reference-style links: [link text] [id]
        text = _anchorRef.Replace(text, New MatchEvaluator(AddressOf AnchorRefEvaluator))

        ' Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
        text = _anchorInline.Replace(text, New MatchEvaluator(AddressOf AnchorInlineEvaluator))

        '  Last, handle reference-style shortcuts: [link text]
        '  These must come last in case you've also got [link test][1]
        '  or [link test](/foo)
        text = _anchorRefShortcut.Replace(text, New MatchEvaluator(AddressOf AnchorRefShortcutEvaluator))
        Return text
    End Function

    Private Function SaveFromAutoLinking(s As String) As String
        Return s.Replace("://", AutoLinkPreventionMarker)
    End Function

    Private Function AnchorRefEvaluator(match As Match) As String
        Dim wholeMatch As String = match.Groups(1).Value
        Dim linkText As String = SaveFromAutoLinking(match.Groups(2).Value)
        Dim linkID As String = match.Groups(3).Value.ToLowerInvariant()

        Dim result As String

        ' for shortcut links like [this][].
        If linkID = "" Then
            linkID = linkText.ToLowerInvariant()
        End If

        If _urls.ContainsKey(linkID) Then
            Dim url As String = _urls(linkID)

            url = AttributeSafeUrl(url)

            result = "<a href=""" & url & """"

            If _titles.ContainsKey(linkID) Then
                Dim title As String = AttributeEncode(_titles(linkID))
                title = AttributeEncode(EscapeBoldItalic(title))
                result += " title=""" & title & """"
            End If

            If String.IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
                linkText = url
            End If

            result += ">" & linkText & "</a>"
        Else
            result = wholeMatch
        End If

        Return result
    End Function

    Private Function AnchorRefShortcutEvaluator(match As Match) As String
        Dim wholeMatch As String = match.Groups(1).Value
        Dim linkText As String = SaveFromAutoLinking(match.Groups(2).Value)
        Dim linkID As String = Regex.Replace(linkText.ToLowerInvariant(), "[ ]*\n[ ]*", " ")
        ' lower case and remove newlines / extra spaces
        Dim result As String

        If _urls.ContainsKey(linkID) Then
            Dim url As String = _urls(linkID)

            url = AttributeSafeUrl(url)

            result = "<a href=""" & url & """"

            If _titles.ContainsKey(linkID) Then
                Dim title As String = AttributeEncode(_titles(linkID))
                title = EscapeBoldItalic(title)
                result += " title=""" & title & """"
            End If

            If String.IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
                linkText = url
            End If

            result += ">" & linkText & "</a>"
        Else
            result = wholeMatch
        End If

        Return result
    End Function


    Private Function AnchorInlineEvaluator(match As Match) As String
        Dim linkText As String = SaveFromAutoLinking(match.Groups(2).Value)
        Dim url As String = match.Groups(3).Value
        Dim title As String = match.Groups(6).Value
        Dim result As String

        If url.StartsWith("<") AndAlso url.EndsWith(">") Then
            url = url.Substring(1, url.Length - 2)
        End If
        ' remove <>'s surrounding URL, if present            
        url = AttributeSafeUrl(url)

        result = String.Format("<a href=""{0}""", url)

        If Not String.IsNullOrEmpty(title) Then
            title = AttributeEncode(title)
            title = EscapeBoldItalic(title)
            result += String.Format(" title=""{0}""", title)
        End If

        If String.IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
            linkText = url
        End If

        result += String.Format(">{0}</a>", linkText)

        Return result
    End Function

    Const imageRefRegexp$ = "

        (               # wrap whole match in $1
            !\[
            (.*?)       # alt text = $2
            \]

        [ ]?            # one optional space
        (?:\n[ ]*)?     # one optional newline followed by spaces

        \[
            (.*?)       # id = $3
        \]

        )"

    Const imageInlineRegexp$ = "

        (                     # wrap whole match in $1
            !\[
              (.*?)           # alt text = $2
            \]
          \s?                 # one optional whitespace character
          \(                  # literal paren
          [ ]*
              ({0})           # href = $3
          [ ]*
              (               # $4
                (['""])       # quote char = $5
              (.*?)           # title = $6
              \5              # matching quote
          [ ]*
              )?              # title is optional
          \)
        )"

    Private Shared _imagesRef As New Regex(imageRefRegexp, RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)
    Private Shared _imagesInline As New Regex(String.Format(imageInlineRegexp, GetNestedParensPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown image shortcuts into HTML img tags. 
    ''' </summary>
    ''' <remarks>
    ''' ![alt text][id]
    ''' ![alt text](url "optional title")
    ''' </remarks>
    Private Function DoImages(text As String) As String
        If Not text.Contains("![") Then
            Return text
        End If

        ' First, handle reference-style labeled images: ![alt text][id]
        text = _imagesRef.Replace(text, New MatchEvaluator(AddressOf ImageReferenceEvaluator))

        ' Next, handle inline images:  ![alt text](url "optional title")
        ' Don't forget: encode * and _
        text = _imagesInline.Replace(text, New MatchEvaluator(AddressOf ImageInlineEvaluator))

        Return text
    End Function

    ' This prevents the creation of horribly broken HTML when some syntax ambiguities
    ' collide. It likely still doesn't do what the user meant, but at least we're not
    ' outputting garbage.
    Private Function EscapeImageAltText(s As String) As String
        s = EscapeBoldItalic(s)
        s = Regex.Replace(s, "[\[\]()]", Function(m) _escapeTable(m.ToString()))
        Return s
    End Function

    Private Function ImageReferenceEvaluator(match As Match) As String
        Dim wholeMatch As String = match.Groups(1).Value
        Dim altText As String = match.Groups(2).Value
        Dim linkID As String = match.Groups(3).Value.ToLowerInvariant()

        ' for shortcut links like ![this][].
        If linkID = "" Then
            linkID = altText.ToLowerInvariant()
        End If

        If _urls.ContainsKey(linkID) Then
            Dim url As String = _urls(linkID)
            Dim title As String = Nothing

            If _titles.ContainsKey(linkID) Then
                title = _titles(linkID)
            End If

            Return ImageTag(url, altText, title)
        Else
            ' If there's no such link ID, leave intact:
            Return wholeMatch
        End If
    End Function

    Private Function ImageInlineEvaluator(match As Match) As String
        Dim alt As String = match.Groups(2).Value
        Dim url As String = match.Groups(3).Value
        Dim title As String = match.Groups(6).Value

#If DEBUG Then
        Call match.Value.__DEBUG_ECHO
#End If
        If url.StartsWith("<") AndAlso url.EndsWith(">") Then
            url = url.Substring(1, url.Length - 2)
        End If
        ' Remove <>'s surrounding URL, if present
        Return ImageTag(url, alt, title)
    End Function

    Private Function ImageTag(url As String, altText As String, title As String) As String
        altText = EscapeImageAltText(AttributeEncode(altText))
        url = AttributeSafeUrl(url)
        Dim result = String.Format("<img src=""{0}"" alt=""{1}""", url, altText)
        If Not String.IsNullOrEmpty(title) Then
            title = AttributeEncode(EscapeBoldItalic(title))
            result += String.Format(" title=""{0}""", title)
        End If
        result += _EmptyElementSuffix
        Return result
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

    Friend Shared ReadOnly _headerSetext As New Regex(regex_headerSetext, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)
    Friend Shared ReadOnly _headerAtx As New Regex(regex_headerAtx, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown headers into HTML header tags
    ''' </summary>
    ''' <remarks>
    ''' Header 1  
    ''' ========  
    ''' 
    ''' Header 2  
    ''' --------  
    ''' 
    ''' # Header 1  
    ''' ## Header 2  
    ''' ## Header 2 with closing hashes ##  
    ''' ...  
    ''' ###### Header 6  
    ''' </remarks>
    Private Function DoHeaders(text As String) As String
        text = _headerSetext.Replace(text, New MatchEvaluator(AddressOf SetextHeaderEvaluator))
        text = _headerAtx.Replace(text, New MatchEvaluator(AddressOf AtxHeaderEvaluator))
        Return text
    End Function

    Private Function SetextHeaderEvaluator(match As Match) As String
        Dim header As String = match.Groups(1).Value
        Dim level As Integer = If(match.Groups(2).Value.StartsWith("="), 1, 2)
        Return String.Format("<h{1}>{0}</h{1}>" & vbLf & vbLf, RunSpanGamut(header), level)
    End Function

    Private Function AtxHeaderEvaluator(match As Match) As String
        Dim header As String = match.Groups(2).Value
        Dim level As Integer = match.Groups(1).Value.Length
        Return String.Format("<h{1}>{0}</h{1}>" & vbLf & vbLf, RunSpanGamut(header), level)
    End Function

    Const regex_horizontalRules$ = "
              ^[ ]{0,3}         # Leading space
                  ([-*_])       # $1: First marker
                  (?>           # Repeated marker group
                      [ ]{0,2}  # Zero, one, or two spaces.
                      \1        # Marker character
                  ){2,}         # Group repeated at least twice
                  [ ]*          # Trailing spaces
                  $             # End of line."

    Shared ReadOnly _horizontalRules As New Regex(regex_horizontalRules, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown horizontal rules into HTML hr tags
    ''' </summary>
    ''' <remarks>
    ''' ***  
    ''' * * *  
    ''' ---
    ''' - - -
    ''' </remarks>
    Private Function DoHorizontalRules(text As String) As String
        Return _horizontalRules.Replace(text, "<hr" & _EmptyElementSuffix & vbLf)
    End Function

    Const regex_wholeList$ = "
              (                               # $1 = whole list
                (                             # $2
                  [ ]{{0,{1}}} 
                  ({0})                       # $3 = first list item marker
                  [ ]+
                )
                (?s:.+?)
                (                             # $4
                  \z
                  |
                  \n{{2,}}
                  (?=\S)
                    (?!                       # Negative lookahead for another list item marker
                      [ ]*
                      {0}[ ]+
                    )
                  )
                )"

    Shared ReadOnly _wholeList As String = String.Format(regex_wholeList, String.Format("(?:{0}|{1})", MarkerUL, MarkerOL), _tabWidth - 1)
    Shared _listNested As New Regex("^" & _wholeList, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)
    Shared _listTopLevel As New Regex("(?:(?<=\n\n)|\A\n?)" & _wholeList, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown lists into HTML ul and ol and li tags
    ''' </summary>
    Private Function DoLists(text As String) As String
        ' We use a different prefix before nested lists than top-level lists.
        ' See extended comment in _ProcessListItems().
        If _listLevel > 0 Then
            text = _listNested.Replace(text, New MatchEvaluator(AddressOf ListEvaluator))
        Else
            text = _listTopLevel.Replace(text, New MatchEvaluator(AddressOf ListEvaluator))
        End If

        Return text
    End Function

    Private Function DoCheckbox(text As String) As String
        Static checkbox As New Regex("^\-\s\[(x|\s)\]", RegexICMul)
        Return checkbox.Replace(text, New MatchEvaluator(AddressOf CheckboxEvaluator))
    End Function

    Private Function CheckboxEvaluator(match As Match) As String
        If (InStr(match.Value, "[ ]") > 0) Then
            Return "<input type=""checkbox"" disabled="""" class=""task-list-item-checkbox"" />"
        Else
            Return "<input type=""checkbox"" disabled="""" class=""task-list-item-checkbox"" checked=""checked"" />"
        End If
    End Function

    Private Function ListEvaluator(match As Match) As String
        Dim list As String = match.Groups(1).Value
        Dim marker As String = match.Groups(3).Value
        Dim listType As String = If(Regex.IsMatch(marker, MarkerUL), "ul", "ol")
        Dim result As String
        Dim start As String = ""
        If listType = "ol" Then
            Dim firstNumber = Integer.Parse(marker.Substring(0, marker.Length - 1))
            If firstNumber <> 1 AndAlso firstNumber <> 0 Then
                start = " start=""" & firstNumber & """"
            End If
        End If

        result = ProcessListItems(list, If(listType = "ul", MarkerUL, MarkerOL))
        result = String.Format("<{0}{1}>" & vbLf & "{2}</{0}>" & vbLf, listType, start, result)

        Return result
    End Function

    Const listPatternFormat$ = "

              (^[ ]*)                    # leading whitespace = $1
              ({0}) [ ]+                 # list marker = $2
              ((?s:.+?)                  # list item text = $3
              (\n+))      
              (?= (\z | \1 ({0}) [ ]+))"

    ''' <summary>
    ''' Process the contents of a single ordered or unordered list, splitting it
    ''' into individual list items.
    ''' </summary>
    Private Function ProcessListItems(list As String, marker As String) As String
        ' The listLevel global keeps track of when we're inside a list.
        ' Each time we enter a list, we increment it; when we leave a list,
        ' we decrement. If it's zero, we're not in a list anymore.

        ' We do this because when we're not inside a list, we want to treat
        ' something like this:

        '    I recommend upgrading to version
        '    8. Oops, now this line is treated
        '    as a sub-list.

        ' As a single paragraph, despite the fact that the second line starts
        ' with a digit-period-space sequence.

        ' Whereas when we're inside a list (or sub-list), that line will be
        ' treated as the start of a sub-list. What a kludge, huh? This is
        ' an aspect of Markdown's syntax that's hard to parse perfectly
        ' without resorting to mind-reading. Perhaps the solution is to
        ' change the syntax rules such that sub-lists must start with a
        ' starting cardinal number; e.g. "1." or "a.".

        _listLevel += 1

        ' Trim trailing blank lines:
        list = Regex.Replace(list, "\n{2,}\z", vbLf)

        Dim pattern$ = String.Format(listPatternFormat, marker)
        Dim lastItemHadADoubleNewline As Boolean = False

        ' has to be a closure, so subsequent invocations can share the bool
        Dim ListItemEvaluator As MatchEvaluator =
            Function(match As Match)
                Dim item As String = match.Groups(3).Value

                Dim endsWithDoubleNewline As Boolean = item.EndsWith(vbLf & vbLf)
                Dim containsDoubleNewline As Boolean = endsWithDoubleNewline OrElse item.Contains(vbLf & vbLf)

                Dim loose = containsDoubleNewline OrElse lastItemHadADoubleNewline
                    ' we could correct any bad indentation here..
                    item = RunBlockGamut(Outdent(item) & vbLf, unhash:=False, createParagraphs:=loose)

                lastItemHadADoubleNewline = endsWithDoubleNewline
                Return String.Format("<li>{0}</li>" & vbLf, item)
            End Function

        list = Regex.Replace(list, pattern, ListItemEvaluator, RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline)
        _listLevel -= 1

        Return list
    End Function

#Region "代码预览块的HTML文本处理"

    Const CodeBlockFlag$ = "[`]{3}"

    ''' <summary>
    ''' 带语言类型说明的代码块
    ''' </summary>
    Const SyntaxCodeBloackRegexp$ = CodeBlockFlag & ".+?" & CodeBlockFlag

    ''' <summary>
    ''' 使用\`符号作为代码块的标记，例如``代码块``
    ''' </summary>
    Shared ReadOnly __syntaxCodeBlock As New Regex(SyntaxCodeBloackRegexp, RegexICSng)

    ''' <summary>
    ''' 这里只是解析出4个空格的缩进的代码块
    ''' </summary>
    Const codeBlockRegexp$ = "

            (?:\n\n|\A\n?)
            (                        # $1 = the code block -- one or more lines, starting with a space
                (?:
                (?:[ ]{{{0}}})       # Lines must start with a tab-width of spaces
                .*\n+
                )+
            )
            ((?=^[ ]{{0,{0}}}[^ \t\n])|\Z) # Lookahead for non-space at line-start, or end of doc"

    ''' <summary>
    ''' 这种情况不是使用\`符号作为代码块，而是直接使用三行缩进来作为代码块的标记，例如：
    ''' 
    ''' ```
    ''' 在这里使用缩进来构建代码块
    ''' 组成结构为 空白行起始 + 三个空格缩进 + 空白行结束
    ''' 
    '''    代码
    '''    代码
    '''    
    ''' ```
    ''' </summary>
    Shared ReadOnly _codeBlock As New Regex(String.Format(codeBlockRegexp, _tabWidth), RegexPythonRawString)

    ''' <summary>
    ''' Turn Markdown 4-space indented code into HTML pre code blocks
    ''' </summary>
    Private Function DoCodeBlocks(text As String) As String
        text = __syntaxCodeBlock.Replace(text, New MatchEvaluator(AddressOf SyntaxedCodeBlockEvaluator))
        text = _codeBlock.Replace(text, New MatchEvaluator(AddressOf CodeBlockEvaluator))
        Return text
    End Function

    Private Function SyntaxedCodeBlockEvaluator(match As Match) As String
        Dim codeBlock As String = match.Value
        Dim lines = codeBlock.LineTokens
        Dim language$ = Mid(lines(Scan0), 4).Trim

        codeBlock = lines.Skip(1).Take(lines.Length - 2).JoinBy(vbLf)
        codeBlock = EncodeCode(Outdent(codeBlock))
        codeBlock = _newlinesLeadingTrailing.Replace(codeBlock, "")

        Return String.Concat(vbLf & vbLf & $"<pre><code class=""{language}"">", codeBlock, vbLf & "</code></pre>" & vbLf & vbLf)
    End Function

    Private Function CodeBlockEvaluator(match As Match) As String
        Dim codeBlock As String = match.Groups(1).Value

        codeBlock = EncodeCode(Outdent(codeBlock))
        codeBlock = _newlinesLeadingTrailing.Replace(codeBlock, "")

        Return String.Concat(vbLf & vbLf & "<pre><code>", codeBlock, vbLf & "</code></pre>" & vbLf & vbLf)
    End Function

    Const codeSpanRegexp$ = " 
        
            (?<![\\`])   # Character before opening ` can't be a backslash or backtick
               (`+)      # $1 = Opening run of `
               (?!`)     # and no more backticks -- match the full run
               (.+?)     # $2 = The code block
               (?<!`)
               \1
               (?!`)"

    Private Shared _codeSpan As New Regex(codeSpanRegexp, RegexOptions.IgnorePatternWhitespace Or RegexOptions.Singleline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown `code spans` into HTML code tags
    ''' </summary>
    Private Function DoCodeSpans(text As String) As String
        '    * You can use multiple backticks as the delimiters if you want to
        '        include literal backticks in the code span. So, this input:
        '
        '        Just type ``foo `bar` baz`` at the prompt.
        '
        '        Will translate to:
        '
        '          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
        '
        '        There's no arbitrary limit to the number of backticks you
        '        can use as delimters. If you need three consecutive backticks
        '        in your code, use four for delimiters, etc.
        '
        '    * You can use spaces to get literal backticks at the edges:
        '
        '          ... type `` `bar` `` ...
        '
        '        Turns to:
        '
        '          ... type <code>`bar`</code> ...         
        '

        Return _codeSpan.Replace(text, New MatchEvaluator(AddressOf CodeSpanEvaluator))
    End Function

    Private Function CodeSpanEvaluator(match As Match) As String
        Dim span As String = match.Groups(2).Value
        span = Regex.Replace(span, "^[ ]*", "")
        ' leading whitespace
        span = Regex.Replace(span, "[ ]*$", "")
        ' trailing whitespace
        span = EncodeCode(span)
        span = SaveFromAutoLinking(span)
        ' to prevent auto-linking. Not necessary in code *blocks*, but in code spans.
        Return String.Concat("<code>", span, "</code>")
    End Function
#End Region

    Private Shared _bold As New Regex("(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared _semiStrictBold As New Regex("(?=.[*_]|[*_])(^|(?=\W__|(?!\*)[\W_]\*\*|\w\*\*\w).)(\*\*|__)(?!\2)(?=\S)((?:|.*?(?!\2).)(?=\S_|\w|\S\*\*(?:[\W_]|$)).)(?=__(?:\W|$)|\*\*(?:[^*]|$))\2", RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared _strictBold As New Regex("(^|[\W_])(?:(?!\1)|(?=^))(\*|_)\2(?=\S)(.*?\S)\2\2(?!\2)(?=[\W_]|$)", RegexOptions.Singleline Or RegexOptions.Compiled)

    Private Shared _italic As New Regex("(\*|_) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared _semiStrictItalic As New Regex("(?=.[*_]|[*_])(^|(?=\W_|(?!\*)(?:[\W_]\*|\D\*(?=\w)\D)).)(\*|_)(?!\2\2\2)(?=\S)((?:(?!\2).)*?(?=[^\s_]_|(?=\w)\D\*\D|[^\s*]\*(?:[\W_]|$)).)(?=_(?:\W|$)|\*(?:[^*]|$))\2", RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared _strictItalic As New Regex("(^|[\W_])(?:(?!\1)|(?=^))(\*|_)(?=\S)((?:(?!\2).)*?\S)\2(?!\2)(?=[\W_]|$)", RegexOptions.Singleline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown *italics* and **bold** into HTML strong and em tags
    ''' </summary>
    Private Function DoItalicsAndBold(text As String) As String
        If Not (text.Contains("*") OrElse text.Contains("_")) Then
            Return text
        End If
        ' <strong> must go first, then <em>
        If _StrictBoldItalic Then
            If _AsteriskIntraWordEmphasis Then
                text = _semiStrictBold.Replace(text, "$1<strong>$3</strong>")

                text = _semiStrictItalic.Replace(text, "$1<em>$3</em>")
            Else
                text = _strictBold.Replace(text, "$1<strong>$3</strong>")

                text = _strictItalic.Replace(text, "$1<em>$3</em>")
            End If
        Else
            text = _bold.Replace(text, "<strong>$2</strong>")
            text = _italic.Replace(text, "<em>$2</em>")
        End If
        Return text
    End Function

    ''' <summary>
    ''' Turn markdown line breaks (two space at end of line) into HTML break tags
    ''' </summary>
    Private Function DoHardBreaks(text As String) As String
        If _AutoNewLines Then
            text = Regex.Replace(text, "\n", String.Format("<br{0}" & vbLf, _EmptyElementSuffix))
        Else
            text = Regex.Replace(text, " {2,}\n", String.Format("<br{0}" & vbLf, _EmptyElementSuffix))
        End If
        Return text
    End Function

    Const blockQuoteRegexp$ = "

        (                           # Wrap whole match in $1
            (
            ^[ ]*>[ ]?              # '>' at the start of a line
                .+\n                # rest of the first line
            (.+\n)*                 # subsequent consecutive lines
            \n*                     # blanks
            )+
        )"
    Const blockQuoteSingleLineRegexp$ = "

        (                           # Wrap whole match in $1
            (
            ^[ ]*>[ ]?              # '>' at the start of a line
                .+                  # rest of the first line
            )+
        )"

    Shared ReadOnly _blockquote As New Regex(blockQuoteRegexp, RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline Or RegexOptions.Compiled)
    Shared ReadOnly _blockquoteSingleLine As New Regex(blockQuoteSingleLineRegexp, RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Turn Markdown > quoted blocks into HTML blockquote blocks
    ''' </summary>
    Private Function DoBlockQuotes(text As String) As String
        If _QuoteSingleLine Then
            Return _blockquoteSingleLine.Replace(text, New MatchEvaluator(AddressOf BlockQuoteEvaluator))
        End If
        Return _blockquote.Replace(text, New MatchEvaluator(AddressOf BlockQuoteEvaluator))
    End Function

    Private Function BlockQuoteEvaluator(match As Match) As String
        Dim bq As String = match.Groups(1).Value

        bq = Regex.Replace(bq, "^[ ]*>[ ]?", "", RegexOptions.Multiline)
        ' trim one level of quoting
        bq = Regex.Replace(bq, "^[ ]+$", "", RegexOptions.Multiline)
        ' trim whitespace-only lines
        bq = RunBlockGamut(bq)
        ' recurse
        bq = Regex.Replace(bq, "^", "  ", RegexOptions.Multiline)

        ' These leading spaces screw with <pre> content, so we need to fix that:
        bq = Regex.Replace(bq, "(\s*<pre>.+?</pre>)", New MatchEvaluator(AddressOf BlockQuoteEvaluator2), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Singleline)

        bq = String.Format("<blockquote>" & vbLf & "{0}" & vbLf & "</blockquote>", bq)
        Dim key As String = GetHashKey(bq, isHtmlBlock:=True)
        _htmlBlocks(key) = bq

        Return vbLf & vbLf & key & vbLf & vbLf
    End Function

    Private Function BlockQuoteEvaluator2(match As Match) As String
        Return Regex.Replace(match.Groups(1).Value, "^  ", "", RegexOptions.Multiline)
    End Function

    Shared ReadOnly _autolinkBare As New Regex("(<|="")?\b(https?|ftp)(://" & _charInsideUrl & "*" & _charEndingUrl & ")(?=$|\W)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    ''' <summary>
    ''' Email addresses: address@domain.foo
    ''' </summary>
    Const EMailAddress$ = "
        
            (?:mailto:)?(
                [-.\w]+
                \@
                [-a-z0-9]+(\.[-a-z0-9]+)*\.[a-z]+
            )"

    ''' <summary>
    ''' Turn angle-delimited URLs into HTML anchor tags
    ''' </summary>
    ''' <remarks>
    ''' &lt;http://www.example.com&gt;
    ''' </remarks>
    Private Function DoAutoLinks(text As String) As String
        If _AutoHyperlink Then
            ' fixup arbitrary URLs by adding Markdown < > so they get linked as well
            ' note that at this point, all other URL in the text are already hyperlinked as <a href=""></a>
            ' *except* for the <http://www.foo.com> case
            text = _autolinkBare.Replace(text, AddressOf handleTrailingParens)
        End If

        ' Hyperlinks: <http://foo.com>
        text = Regex.Replace(text, "<((https?|ftp):[^'"">\s]+)>", New MatchEvaluator(AddressOf HyperlinkEvaluator))

        If _LinkEmails Then
            text = Regex.Replace(text, EMailAddress, New MatchEvaluator(AddressOf EmailEvaluator), RegexOptions.IgnoreCase Or RegexOptions.IgnorePatternWhitespace)
        End If

        Return text
    End Function

    Private Function HyperlinkEvaluator(match As Match) As String
        Dim link As String = match.Groups(1).Value
        Dim url As String = AttributeSafeUrl(link)

        Return $"<a href=""{url}"">{link}</a>"
    End Function

    Private Function EmailEvaluator(match As Match) As String
        Dim email As String = Unescape(match.Groups(1).Value)

        '
        '    Input: an email address, e.g. "foo@example.com"
        '
        '    Output: the email address as a mailto link, with each character
        '            of the address encoded as either a decimal or hex entity, in
        '            the hopes of foiling most address harvesting spam bots. E.g.:
        '
        '      <a href="&#x6D;&#97;&#105;&#108;&#x74;&#111;:&#102;&#111;&#111;&#64;&#101;
        '        x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;">&#102;&#111;&#111;
        '        &#64;&#101;x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;</a>
        '
        '    Based by a filter by Matthew Wickline, posted to the BBEdit-Talk
        '    mailing list: <http://tinyurl.com/yu7ue>
        '
        email = "mailto:" & email

        ' leave ':' alone (to spot mailto: later) 
        email = EncodeEmailAddress(email)

        email = String.Format("<a href=""{0}"">{0}</a>", email)

        ' strip the mailto: from the visible part
        email = Regex.Replace(email, """>.+?:", """>")

        Return email
    End Function

#Region "Encoding and Normalization"

    Shared ReadOnly rand As New Random()

    ''' <summary>
    ''' encodes email address randomly  
    ''' roughly 10% raw, 45% hex, 45% dec 
    ''' note that @ is always encoded and : never is
    ''' </summary>
    Private Function EncodeEmailAddress(addr As String) As String
        Dim sb = New StringBuilder(addr.Length * 5)
        Dim r As Integer

        For Each c As Char In addr
            r = rand.[Next](1, 100)
            If (r > 90 OrElse c = ":"c) AndAlso c <> "@"c Then
                sb.Append(c)
                ' m
            ElseIf r < 45 Then
                sb.AppendFormat("&#x{0:x};", AscW(c))
            Else
                ' &#x6D
                sb.AppendFormat("&#{0};", AscW(c))
                ' &#109
            End If
        Next

        Return sb.ToString()
    End Function

    Shared ReadOnly _codeEncoder As New Regex("&|<|>|\\|\*|_|\{|\}|\[|\]|[#]", RegexOptions.Compiled)

    ''' <summary>
    ''' Encode/escape certain Markdown characters inside code blocks and spans where they are literals
    ''' </summary>
    Private Function EncodeCode(code As String) As String
        Return _codeEncoder.Replace(code, AddressOf EncodeCodeEvaluator)
    End Function

    Private Function EncodeCodeEvaluator(match As Match) As String
        Select Case match.Value
            ' Encode all ampersands; HTML entities are not
            ' entities within a Markdown code span.
            Case "&"
                Return "&amp;"
            ' Do the angle bracket song and dance
            Case "<"
                Return "&lt;"
            Case ">"
                Return "&gt;"
            Case "#"
                Return "&#35;"

            Case Else
                ' escape characters that are magic in Markdown
                Return _escapeTable(match.Value)
        End Select
    End Function

    Private Shared _amps As New Regex("&(?!((#[0-9]+)|(#[xX][a-fA-F0-9]+)|([a-zA-Z][a-zA-Z0-9]*));)", RegexOptions.ExplicitCapture Or RegexOptions.Compiled)
    Private Shared _angles As New Regex("<(?![A-Za-z/?\$!])", RegexOptions.ExplicitCapture Or RegexOptions.Compiled)

    ''' <summary>
    ''' Encode any ampersands (that aren't part of an HTML entity) and left or right angle brackets
    ''' </summary>
    Private Function EncodeAmpsAndAngles(s As String) As String
        s = _amps.Replace(s, "&amp;")
        s = _angles.Replace(s, "&lt;")
        Return s
    End Function

    ''' <summary>
    ''' Encodes any escaped characters such as \`, \*, \[ etc
    ''' </summary>
    Private Function EscapeBackslashes(s As String) As String
        Return _backslashEscapes.Replace(s, New MatchEvaluator(AddressOf EscapeBackslashesEvaluator))
    End Function

    Private Function EscapeBackslashesEvaluator(match As Match) As String
        Return _backslashEscapeTable(match.Value)
    End Function

    ''' <summary>
    ''' swap back in all the special characters we've hidden
    ''' </summary>
    Private Function Unescape(s As String) As String
        Return _unescapes.Replace(s, New MatchEvaluator(AddressOf UnescapeEvaluator))
    End Function

    Private Function UnescapeEvaluator(match As Match) As String
        Return _invertedEscapeTable(match.Value)
    End Function

    ''' <summary>
    ''' escapes Bold [ * ] and Italic [ _ ] characters
    ''' </summary>
    Private Function EscapeBoldItalic(s As String) As String
        s = s.Replace("*", _escapeTable("*"))
        s = s.Replace("_", _escapeTable("_"))
        Return s
    End Function

    ''' <summary>
    ''' Within tags -- meaning between &lt; and &gt; -- encode [\ ` * _] so they 
    ''' don't conflict with their use in Markdown for code, italics and strong. 
    ''' We're replacing each such character with its corresponding hash 
    ''' value; this is likely overkill, but it should prevent us from colliding 
    ''' with the escape values by accident.
    ''' </summary>
    Private Function EscapeSpecialCharsWithinTagAttributes(text As String) As String
        Dim tokens As IEnumerable(Of DocumentToken) = TokenizeHTML(text)
        ' now, rebuild text from the tokens
        Dim sb = New StringBuilder(text.Length)
        Dim value As String

        For Each token As DocumentToken In tokens
            value = token.text

            If token.name = TokenType.Tag Then
                value = value.Replace("\", _escapeTable("\"))

                If _AutoHyperlink AndAlso value.StartsWith("<!") Then
                    ' escape slashes in comments to prevent autolinking there 
                    ' -- http://meta.stackexchange.com/questions/95987/html-comment-containing-url-breaks-if-followed-by-another-html-comment
                    value = value.Replace("/", _escapeTable("/"))
                End If

                value = Regex.Replace(value, "(?<=.)</?code>(?=.)", _escapeTable("`"))
                value = EscapeBoldItalic(value)
            End If

            sb.Append(value)
        Next

        Return sb.ToString()
    End Function
#End Region
End Class
