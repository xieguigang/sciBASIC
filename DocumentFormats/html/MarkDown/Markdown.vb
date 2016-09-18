#Region "Microsoft.VisualBasic::f33e21df5a4bfc69cc6c2d977918d405, ..\visualbasic_App\DocumentFormats\VB_HTML\VB_HTML\MarkDown\Markdown.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace MarkDown

    ''' <summary>
    ''' Markdown is a text-to-HTML conversion tool for web writers. 
    ''' Markdown allows you to write using an easy-to-read, easy-to-write plain text format, 
    ''' then convert it to structurally valid XHTML (or HTML).
    ''' </summary>
    Public Class MarkdownHTML

#Region "Constructors and Options"

        ''' <summary>
        ''' Create a new Markdown instance and set the options from the MarkdownOptions object.
        ''' </summary>
        Public Sub New(options As MarkdownOptions)
            If Not [String].IsNullOrEmpty(options.EmptyElementSuffix) Then
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

        Private Const _markerUL As String = "[*+-]"
        Private Const _markerOL As String = "\d+[.]"

        Private ReadOnly _urls As New Dictionary(Of String, String)()
        Private ReadOnly _titles As New Dictionary(Of String, String)()
        Private ReadOnly _htmlBlocks As New Dictionary(Of String, String)()

        Private _listLevel As Integer
        Private Shared AutoLinkPreventionMarker As String = ChrW(26) & "P"

        ''' <summary>
        ''' Transforms the provided Markdown-formatted text to HTML;  
        ''' see http://en.wikipedia.org/wiki/Markdown
        ''' (好像这个并不支持代码高亮的格式化)
        ''' </summary>
        ''' <remarks>
        ''' The order in which other subs are called here is
        ''' essential. Link and image substitutions need to happen before
        ''' EscapeSpecialChars(), so that any *'s or _'s in the a
        ''' and img tags get encoded.
        ''' </remarks>
        Public Function Transform(text As String) As String
            If [String].IsNullOrEmpty(text) Then
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

            If Not _DisableHeaders Then
                text = DoHeaders(text)
            End If

            If Not _DisableHr Then
                text = DoHorizontalRules(text)
            End If

            text = DoLists(text)
            text = DoCodeBlocks(text)
            text = DoBlockQuotes(text)

            ' We already ran HashHTMLBlocks() before, in Markdown(), but that
            ' was to escape raw HTML in the original Markdown source. This time,
            ' we're escaping the markup we've just created, so that we don't wrap
            ' <p> tags around block-level tags.
            text = HashHTMLBlocks(text)

            text = FormParagraphs(text, unhash:=unhash, createParagraphs:=createParagraphs)

            Return text
        End Function


        Private _inlineExtensions As New List(Of ExtensionTransform)()

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

            Return text
        End Function

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
                _nestedParensPattern = RepeatString(vbCr & vbLf & "                    (?>              # Atomic matching" & vbCr & vbLf & "                       [^()\s]+      # Anything other than parens or whitespace" & vbCr & vbLf & "                     |" & vbCr & vbLf & "                       \(" & vbCr & vbLf & "                           ", _nestDepth) & RepeatString(" \)" & vbCr & vbLf & "                    )*", _nestDepth)
            End If
            Return _nestedParensPattern
        End Function

        Private Shared _linkDef As New Regex(String.Format(vbCr & vbLf & "                        ^[ ]{{0,{0}}}\[([^\[\]]+)\]:  # id = $1" & vbCr & vbLf & "                          [ ]*" & vbCr & vbLf & "                          \n?                   # maybe *one* newline" & vbCr & vbLf & "                          [ ]*" & vbCr & vbLf & "                        <?(\S+?)>?              # url = $2" & vbCr & vbLf & "                          [ ]*" & vbCr & vbLf & "                          \n?                   # maybe one newline" & vbCr & vbLf & "                          [ ]*" & vbCr & vbLf & "                        (?:" & vbCr & vbLf & "                            (?<=\s)             # lookbehind for whitespace" & vbCr & vbLf & "                            [""(]" & vbCr & vbLf & "                            (.+?)               # title = $3" & vbCr & vbLf & "                            ["")]" & vbCr & vbLf & "                            [ ]*" & vbCr & vbLf & "                        )?                      # title is optional" & vbCr & vbLf & "                        (?:\n+|\Z)", _tabWidth - 1), RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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

        ' compiling this monster regex results in worse performance. trust me.
        Private Shared _blocksHtml As New Regex(GetBlockPattern(), RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)


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

            ' First, look for nested blocks, e.g.:
            ' 	<div>
            ' 		<div>
            ' 		tags for inner block must be indented.
            ' 		</div>
            ' 	</div>
            '
            ' The outermost tags must start at the left margin for this to match, and
            ' the inner nested divs must be indented.
            ' We need to do this before the next, more liberal match, because the next
            ' match will start at the first `<div>` and stop at the first `</div>`.
            Dim pattern As String = vbCr & vbLf & "            (?>" & vbCr & vbLf & "                  (?>" & vbCr & vbLf & "                    (?<=\n)     # Starting at the beginning of a line" & vbCr & vbLf & "                    |           # or" & vbCr & vbLf & "                    \A\n?       # the beginning of the doc" & vbCr & vbLf & "                  )" & vbCr & vbLf & "                  (             # save in $1" & vbCr & vbLf & vbCr & vbLf & "                    # Match from `\n<tag>` to `</tag>\n`, handling nested tags " & vbCr & vbLf & "                    # in between." & vbCr & vbLf & "                      " & vbCr & vbLf & "                        <($block_tags_b_re)   # start tag = $2" & vbCr & vbLf & "                        $attr>                # attributes followed by > and \n" & vbCr & vbLf & "                        $content              # content, support nesting" & vbCr & vbLf & "                        </\2>                 # the matching end tag" & vbCr & vbLf & "                        [ ]*                  # trailing spaces" & vbCr & vbLf & "                        (?=\n+|\Z)            # followed by a newline or end of document" & vbCr & vbLf & vbCr & vbLf & "                  | # Special version for tags of group a." & vbCr & vbLf & vbCr & vbLf & "                        <($block_tags_a_re)   # start tag = $3" & vbCr & vbLf & "                        $attr>[ ]*\n          # attributes followed by >" & vbCr & vbLf & "                        $content2             # content, support nesting" & vbCr & vbLf & "                        </\3>                 # the matching end tag" & vbCr & vbLf & "                        [ ]*                  # trailing spaces" & vbCr & vbLf & "                        (?=\n+|\Z)            # followed by a newline or end of document" & vbCr & vbLf & "                      " & vbCr & vbLf & "                  | # Special case just for <hr />. It was easier to make a special " & vbCr & vbLf & "                    # case than to make the other regex more complicated." & vbCr & vbLf & "                  " & vbCr & vbLf & "                        [ ]{0,$less_than_tab}" & vbCr & vbLf & "                        <hr" & vbCr & vbLf & "                        $attr                 # attributes" & vbCr & vbLf & "                        /?>                   # the matching end tag" & vbCr & vbLf & "                        [ ]*" & vbCr & vbLf & "                        (?=\n{2,}|\Z)         # followed by a blank line or end of document" & vbCr & vbLf & "                  " & vbCr & vbLf & "                  | # Special case for standalone HTML comments:" & vbCr & vbLf & "                  " & vbCr & vbLf & "                      (?<=\n\n|\A)            # preceded by a blank line or start of document" & vbCr & vbLf & "                      [ ]{0,$less_than_tab}" & vbCr & vbLf & "                      (?s:" & vbCr & vbLf & "                        <!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->" & vbCr & vbLf & "                      )" & vbCr & vbLf & "                      [ ]*" & vbCr & vbLf & "                      (?=\n{2,}|\Z)            # followed by a blank line or end of document" & vbCr & vbLf & "                  " & vbCr & vbLf & "                  | # PHP and ASP-style processor instructions (<? and <%)" & vbCr & vbLf & "                  " & vbCr & vbLf & "                      [ ]{0,$less_than_tab}" & vbCr & vbLf & "                      (?s:" & vbCr & vbLf & "                        <([?%])                # $4" & vbCr & vbLf & "                        .*?" & vbCr & vbLf & "                        \4>" & vbCr & vbLf & "                      )" & vbCr & vbLf & "                      [ ]*" & vbCr & vbLf & "                      (?=\n{2,}|\Z)            # followed by a blank line or end of document" & vbCr & vbLf & "                      " & vbCr & vbLf & "                  )" & vbCr & vbLf & "            )"

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


        Private Shared _anchorRef As New Regex(String.Format(vbCr & vbLf & "            (                               # wrap whole match in $1" & vbCr & vbLf & "                \[" & vbCr & vbLf & "                    ({0})                   # link text = $2" & vbCr & vbLf & "                \]" & vbCr & vbLf & vbCr & vbLf & "                [ ]?                        # one optional space" & vbCr & vbLf & "                (?:\n[ ]*)?                 # one optional newline followed by spaces" & vbCr & vbLf & vbCr & vbLf & "                \[" & vbCr & vbLf & "                    (.*?)                   # id = $3" & vbCr & vbLf & "                \]" & vbCr & vbLf & "            )", GetNestedBracketsPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        Private Shared _anchorInline As New Regex(String.Format(vbCr & vbLf & "                (                           # wrap whole match in $1" & vbCr & vbLf & "                    \[" & vbCr & vbLf & "                        ({0})               # link text = $2" & vbCr & vbLf & "                    \]" & vbCr & vbLf & "                    \(                      # literal paren" & vbCr & vbLf & "                        [ ]*" & vbCr & vbLf & "                        ({1})               # href = $3" & vbCr & vbLf & "                        [ ]*" & vbCr & vbLf & "                        (                   # $4" & vbCr & vbLf & "                        (['""])           # quote char = $5" & vbCr & vbLf & "                        (.*?)               # title = $6" & vbCr & vbLf & "                        \5                  # matching quote" & vbCr & vbLf & "                        [ ]*                # ignore any spaces between closing quote and )" & vbCr & vbLf & "                        )?                  # title is optional" & vbCr & vbLf & "                    \)" & vbCr & vbLf & "                )", GetNestedBracketsPattern(), GetNestedParensPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        Private Shared _anchorRefShortcut As New Regex(vbCr & vbLf & "            (                               # wrap whole match in $1" & vbCr & vbLf & "              \[" & vbCr & vbLf & "                 ([^\[\]]+)                 # link text = $2; can't contain [ or ]" & vbCr & vbLf & "              \]" & vbCr & vbLf & "            )", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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

                If [String].IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
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

                If [String].IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
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

            If Not [String].IsNullOrEmpty(title) Then
                title = AttributeEncode(title)
                title = EscapeBoldItalic(title)
                result += String.Format(" title=""{0}""", title)
            End If

            If [String].IsNullOrEmpty(linkText) AndAlso Not _AllowEmptyLinkText Then
                linkText = url
            End If

            result += String.Format(">{0}</a>", linkText)

            Return result
        End Function

        Private Shared _imagesRef As New Regex(vbCr & vbLf & "                    (               # wrap whole match in $1" & vbCr & vbLf & "                    !\[" & vbCr & vbLf & "                        (.*?)       # alt text = $2" & vbCr & vbLf & "                    \]" & vbCr & vbLf & vbCr & vbLf & "                    [ ]?            # one optional space" & vbCr & vbLf & "                    (?:\n[ ]*)?     # one optional newline followed by spaces" & vbCr & vbLf & vbCr & vbLf & "                    \[" & vbCr & vbLf & "                        (.*?)       # id = $3" & vbCr & vbLf & "                    \]" & vbCr & vbLf & vbCr & vbLf & "                    )", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        Private Shared _imagesInline As New Regex([String].Format(vbCr & vbLf & "              (                     # wrap whole match in $1" & vbCr & vbLf & "                !\[" & vbCr & vbLf & "                    (.*?)           # alt text = $2" & vbCr & vbLf & "                \]" & vbCr & vbLf & "                \s?                 # one optional whitespace character" & vbCr & vbLf & "                \(                  # literal paren" & vbCr & vbLf & "                    [ ]*" & vbCr & vbLf & "                    ({0})           # href = $3" & vbCr & vbLf & "                    [ ]*" & vbCr & vbLf & "                    (               # $4" & vbCr & vbLf & "                    (['""])       # quote char = $5" & vbCr & vbLf & "                    (.*?)           # title = $6" & vbCr & vbLf & "                    \5              # matching quote" & vbCr & vbLf & "                    [ ]*" & vbCr & vbLf & "                    )?              # title is optional" & vbCr & vbLf & "                \)" & vbCr & vbLf & "              )", GetNestedParensPattern()), RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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
            If Not [String].IsNullOrEmpty(title) Then
                title = AttributeEncode(EscapeBoldItalic(title))
                result += String.Format(" title=""{0}""", title)
            End If
            result += _EmptyElementSuffix
            Return result
        End Function

        Private Shared _headerSetext As New Regex(vbCr & vbLf & "                ^(.+?)" & vbCr & vbLf & "                [ ]*" & vbCr & vbLf & "                \n" & vbCr & vbLf & "                (=+|-+)     # $1 = string of ='s or -'s" & vbCr & vbLf & "                [ ]*" & vbCr & vbLf & "                \n+", RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        Private Shared _headerAtx As New Regex(vbCr & vbLf & "                ^(\#{1,6})  # $1 = string of #'s" & vbCr & vbLf & "                [ ]*" & vbCr & vbLf & "                (.+?)       # $2 = Header text" & vbCr & vbLf & "                [ ]*" & vbCr & vbLf & "                \#*         # optional closing #'s (not counted)" & vbCr & vbLf & "                \n+", RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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


        Private Shared _horizontalRules As New Regex(vbCr & vbLf & "            ^[ ]{0,3}         # Leading space" & vbCr & vbLf & "                ([-*_])       # $1: First marker" & vbCr & vbLf & "                (?>           # Repeated marker group" & vbCr & vbLf & "                    [ ]{0,2}  # Zero, one, or two spaces." & vbCr & vbLf & "                    \1        # Marker character" & vbCr & vbLf & "                ){2,}         # Group repeated at least twice" & vbCr & vbLf & "                [ ]*          # Trailing spaces" & vbCr & vbLf & "                $             # End of line." & vbCr & vbLf & "            ", RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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

        Private Shared _wholeList As String = String.Format(vbCr & vbLf & "            (                               # $1 = whole list" & vbCr & vbLf & "              (                             # $2" & vbCr & vbLf & "                [ ]{{0,{1}}}" & vbCr & vbLf & "                ({0})                       # $3 = first list item marker" & vbCr & vbLf & "                [ ]+" & vbCr & vbLf & "              )" & vbCr & vbLf & "              (?s:.+?)" & vbCr & vbLf & "              (                             # $4" & vbCr & vbLf & "                  \z" & vbCr & vbLf & "                |" & vbCr & vbLf & "                  \n{{2,}}" & vbCr & vbLf & "                  (?=\S)" & vbCr & vbLf & "                  (?!                       # Negative lookahead for another list item marker" & vbCr & vbLf & "                    [ ]*" & vbCr & vbLf & "                    {0}[ ]+" & vbCr & vbLf & "                  )" & vbCr & vbLf & "              )" & vbCr & vbLf & "            )", String.Format("(?:{0}|{1})", _markerUL, _markerOL), _tabWidth - 1)

        Private Shared _listNested As New Regex("^" & _wholeList, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        Private Shared _listTopLevel As New Regex("(?:(?<=\n\n)|\A\n?)" & _wholeList, RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

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

        Private Function ListEvaluator(match As Match) As String
            Dim list As String = match.Groups(1).Value
            Dim marker As String = match.Groups(3).Value
            Dim listType As String = If(Regex.IsMatch(marker, _markerUL), "ul", "ol")
            Dim result As String
            Dim start As String = ""
            If listType = "ol" Then
                Dim firstNumber = Integer.Parse(marker.Substring(0, marker.Length - 1))
                If firstNumber <> 1 AndAlso firstNumber <> 0 Then
                    start = " start=""" & firstNumber & """"
                End If
            End If

            result = ProcessListItems(list, If(listType = "ul", _markerUL, _markerOL))

            result = String.Format("<{0}{1}>" & vbLf & "{2}</{0}>" & vbLf, listType, start, result)
            Return result

        End Function

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

            Dim pattern As String = String.Format("(^[ ]*)                    # leading whitespace = $1" & vbCr & vbLf & "                ({0}) [ ]+                 # list marker = $2" & vbCr & vbLf & "                ((?s:.+?)                  # list item text = $3" & vbCr & vbLf & "                (\n+))      " & vbCr & vbLf & "                (?= (\z | \1 ({0}) [ ]+))", marker)

            Dim lastItemHadADoubleNewline As Boolean = False

            ' has to be a closure, so subsequent invocations can share the bool
            Dim ListItemEvaluator As MatchEvaluator = Function(match As Match)
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

        Private Shared _codeBlock As New Regex(String.Format(vbCr & vbLf & "                    (?:\n\n|\A\n?)" & vbCr & vbLf & "                    (                        # $1 = the code block -- one or more lines, starting with a space" & vbCr & vbLf & "                    (?:" & vbCr & vbLf & "                        (?:[ ]{{{0}}})       # Lines must start with a tab-width of spaces" & vbCr & vbLf & "                        .*\n+" & vbCr & vbLf & "                    )+" & vbCr & vbLf & "                    )" & vbCr & vbLf & "                    ((?=^[ ]{{0,{0}}}[^ \t\n])|\Z) # Lookahead for non-space at line-start, or end of doc", _tabWidth), RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Compiled)

        ''' <summary>
        ''' /// Turn Markdown 4-space indented code into HTML pre code blocks
        ''' </summary>
        Private Function DoCodeBlocks(text As String) As String
            text = _codeBlock.Replace(text, New MatchEvaluator(AddressOf CodeBlockEvaluator))
            Return text
        End Function

        Private Function CodeBlockEvaluator(match As Match) As String
            Dim codeBlock As String = match.Groups(1).Value

            codeBlock = EncodeCode(Outdent(codeBlock))
            codeBlock = _newlinesLeadingTrailing.Replace(codeBlock, "")

            Return String.Concat(vbLf & vbLf & "<pre><code>", codeBlock, vbLf & "</code></pre>" & vbLf & vbLf)
        End Function

        Private Shared _codeSpan As New Regex(vbCr & vbLf & "                    (?<![\\`])   # Character before opening ` can't be a backslash or backtick" & vbCr & vbLf & "                    (`+)      # $1 = Opening run of `" & vbCr & vbLf & "                    (?!`)     # and no more backticks -- match the full run" & vbCr & vbLf & "                    (.+?)     # $2 = The code block" & vbCr & vbLf & "                    (?<!`)" & vbCr & vbLf & "                    \1" & vbCr & vbLf & "                    (?!`)", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Singleline Or RegexOptions.Compiled)

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

        Private Shared _blockquote As New Regex(vbCr & vbLf & "            (                           # Wrap whole match in $1" & vbCr & vbLf & "                (" & vbCr & vbLf & "                ^[ ]*>[ ]?              # '>' at the start of a line" & vbCr & vbLf & "                    .+\n                # rest of the first line" & vbCr & vbLf & "                (.+\n)*                 # subsequent consecutive lines" & vbCr & vbLf & "                \n*                     # blanks" & vbCr & vbLf & "                )+" & vbCr & vbLf & "            )", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline Or RegexOptions.Compiled)

        Private Shared _blockquoteSingleLine As New Regex(vbCr & vbLf & "            (                           # Wrap whole match in $1" & vbCr & vbLf & "                (" & vbCr & vbLf & "                ^[ ]*>[ ]?              # '>' at the start of a line" & vbCr & vbLf & "                    .+                # rest of the first line" & vbCr & vbLf & "                )+" & vbCr & vbLf & "            )", RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline Or RegexOptions.Compiled)

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

        Private Shared _autolinkBare As New Regex("(<|="")?\b(https?|ftp)(://" & _charInsideUrl & "*" & _charEndingUrl & ")(?=$|\W)", RegexOptions.IgnoreCase Or RegexOptions.Compiled)

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
                ' Email addresses: address@domain.foo
                Dim pattern As String = "(?:mailto:)?" & vbCr & vbLf & "                      (" & vbCr & vbLf & "                        [-.\w]+" & vbCr & vbLf & "                        \@" & vbCr & vbLf & "                        [-a-z0-9]+(\.[-a-z0-9]+)*\.[a-z]+" & vbCr & vbLf & "                      )"
                text = Regex.Replace(text, pattern, New MatchEvaluator(AddressOf EmailEvaluator), RegexOptions.IgnoreCase Or RegexOptions.IgnorePatternWhitespace)
            End If

            Return text
        End Function

        Private Function HyperlinkEvaluator(match As Match) As String
            Dim link As String = match.Groups(1).Value
            Dim url As String = AttributeSafeUrl(link)
            Return String.Format("<a href=""{0}"">{1}</a>", url, link)
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


        ''' <summary>
        ''' encodes email address randomly  
        ''' roughly 10% raw, 45% hex, 45% dec 
        ''' note that @ is always encoded and : never is
        ''' </summary>
        Private Function EncodeEmailAddress(addr As String) As String
            Dim sb = New StringBuilder(addr.Length * 5)
            Dim rand = New Random()
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

        Private Shared _codeEncoder As New Regex("&|<|>|\\|\*|_|\{|\}|\[|\]", RegexOptions.Compiled)

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
            Dim tokens As List(Of Token(Of TokenType)) = TokenizeHTML(text)

            ' now, rebuild text from the tokens
            Dim sb = New StringBuilder(text.Length)

            For Each token As Token(Of TokenType) In tokens
                Dim value As String = token.TokenValue

                If token.Type = TokenType.Tag Then
                    value = value.Replace("\", _escapeTable("\"))

                    If _AutoHyperlink AndAlso value.StartsWith("<!") Then
                        ' escape slashes in comments to prevent autolinking there -- http://meta.stackexchange.com/questions/95987/html-comment-containing-url-breaks-if-followed-by-another-html-comment
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
End Namespace
