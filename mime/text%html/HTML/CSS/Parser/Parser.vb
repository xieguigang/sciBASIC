Imports System.Text
Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

''' <summary>
''' Parser css code to add/remove or manage it.
''' </summary>
''' <remarks>
''' #### Jo-CSS-Parser
''' 
''' https://github.com/rizwan3d/Jo-CSS-Parser
''' 
''' > Complete Css Parser Writen in C#
''' </remarks>
Public Module CssParser

    Public Class EmptyTagNameException : Inherits Exception

        Public Sub New()
            Call MyBase.New("Tag name is null.")
        End Sub
    End Class

    Public Class EmptyPropertyValueException : Inherits Exception

        Public Sub New()
            MyBase.New("Property value is null.")
        End Sub
    End Class

    Dim PrpertyValue As Dictionary(Of CssProperty, String)
    Dim tag As Dictionary(Of HtmlTags, String)

    ''' <summary>
    ''' 主要的CSS解析函数
    ''' </summary>
    ''' <param name="CSS"></param>
    ''' <returns></returns>
    Public Function GetTagWithCSS(CSS As String) As TagWithCSS()
        Dim TagWithCSSList As New List(Of TagWithCSS)()
        Dim IndivisualTag As List(Of String) = IndivisualTags(CSS)

        For Each tag As String In IndivisualTag
            Dim tagname As String() = r.Split(tag, "[{]")
            Dim TWCSS As New TagWithCSS()
            If RemoveWhitespace(tagname(0)) <> "" Then
                TWCSS.TagName = RemoveWitespaceFormStartAndEnd(tagname(0))
                TWCSS.Properties = GetProperty(GetBetween(tag, "{", "}"))
                TagWithCSSList.Add(TWCSS)
            End If
        Next

        Return TagWithCSSList.ToArray
    End Function

    Const IndivisualTagsPattern$ = "(?<selector>(?:(?:[^,{]+),?)*?)\{(?:(?<name>[^}:]+):?(?<value>[^};]+);?)*?\}"

    ''' <summary>
    ''' CSS的注释总是以/*起始，以*/结束的
    ''' </summary>
    Const CommentBlock$ = "/\*.+?\*/"

    ''' <summary>
    ''' ###### 2017-10-1
    ''' 
    ''' 原来的这个解析函数还没有考虑到注释的问题
    ''' 当CSS之中存在注释的时候就无法正常工作了
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Private Function IndivisualTags(input As String) As List(Of String)
        Dim b As New List(Of String)()
        Dim s As New StringBuilder(input)

        ' 首先需要移除掉CSS的注释文本
        For Each block As Match In r.Matches(input, CommentBlock, RegexICSng)
            Call s.Replace(block.Value, "")
        Next

        For Each m As Match In r.Matches(s.ToString, IndivisualTagsPattern)
            b.Add(m.Value.StripBlank)
        Next

        Return b
    End Function

    Private Function GetProperty(input As String) As List(Of [Property])
        Dim p As New List(Of [Property])()
        Dim s As String() = r.Split(input, "[;]")
        Dim i As Integer = 0
        For Each b As String In s
            If b <> "" Then
                Dim t As String() = r.Split(s(i), "[:]")
                Dim g As New [Property]()
                If t.Length = 2 Then
                    If t(0) <> "" Then
                        g.PropertyName = RemoveWhitespace(t(0))
                    End If
                    If t(1) <> "" Then
                        g.PropertyValue = RemoveWitespaceFormStartAndEnd(t(1))
                    End If
                    p.Add(g)
                End If
            End If
            i += 1
        Next
        Return p
    End Function

    Private Function RemoveWhitespace(input As String) As String
        Return New String(input.ToCharArray().Where(Function(c) Not [Char].IsWhiteSpace(c)).ToArray())
    End Function

    Private Function RemoveWitespaceFormStartAndEnd(input As String) As String
        input = input.Trim()
        input = input.TrimEnd()
        Return input
    End Function

    Private Sub SetPrpertyValue()
        PrpertyValue = New Dictionary(Of CssProperty, String)()
        Dim csskey As String() = {"font-weight", "border-radius", "color-stop", "alignment-adjust", "alignment-baseline", "animation",
            "animation-delay", "animation-direction", "animation-duration", "animation-iteration-count", "animation-name", "animation-play-state",
            "animation-timing-function", "appearance", "azimuth", "backface-visibility", "background", "background-attachment",
            "background-break", "background-clip", "background-color", "background-image", "background-origin", "background-position",
            "background-repeat", "background-size", "baseline-shift", "binding", "bleed", "bookmark-label",
            "bookmark-level", "bookmark-state", "bookmark-target", "border", "border-bottom", "border-bottom-color",
            "border-bottom-left-radius", "border-bottom-right-radius", "border-bottom-style", "border-bottom-width", "border-collapse", "border-color",
            "border-image", "border-image-outset", "border-image-repeat", "border-image-slice", "border-image-source", "border-image-width",
            "border-left", "border-left-color", "border-left-style", "border-left-width", "border-right", "border-right-color",
            "border-right-style", "border-right-width", "border-spacing", "border-style", "border-top", "border-top-color",
            "border-top-left-radius", "border-top-right-radius", "border-top-style", "border-top-width", "border-width", "bottom",
            "box-align", "box-decoration-break", "box-direction", "box-flex", "box-flex-group", "box-lines",
            "box-ordinal-group", "box-orient", "box-pack", "box-shadow", "box-sizing", "break-after",
            "break-before", "break-inside", "caption-side", "clear", "clip", "color",
            "color-profile", "column-count", "column-fill", "column-gap", "column-rule", "column-rule-color",
            "column-rule-style", "column-rule-width", "column-span", "column-width", "columns", "content",
            "counter-increment", "counter-reset", "crop", "cue", "cue-after", "cue-before",
            "cursor", "direction", "display", "dominant-baseline", "drop-initial-after-adjust", "drop-initial-after-align",
            "drop-initial-before-adjust", "drop-initial-before-align", "drop-initial-size", "drop-initial-value", "elevation", "empty-cells",
            "filter", "fit", "fit-position", "float-offset", "font", "font-effect",
            "font-emphasize", "font-family", "font-size", "font-size-adjust", "font-stretch", "font-style",
            "font-variant", "grid-columns", "grid-rows", "hanging-punctuation", "height", "hyphenate-after",
            "hyphenate-before", "hyphenate-character", "hyphenate-lines", "hyphenate-resource", "hyphens", "icon",
            "image-orientation", "image-rendering", "image-resolution", "inline-box-align", "left", "letter-spacing",
            "line-height", "line-stacking", "line-stacking-ruby", "line-stacking-shift", "line-stacking-strategy", "list-style",
            "list-style-image", "list-style-position", "list-style-type", "margin", "margin-bottom", "margin-left",
            "margin-right", "margin-top", "mark", "mark-after", "mark-before", "marker-offset",
            "marks", "marquee-direction", "marquee-play-count", "marquee-speed", "marquee-style", "max-height",
            "max-width", "min-height", "min-width", "move-to", "nav-down", "nav-index",
            "nav-left", "nav-right", "nav-up", "opacity", "orphans", "outline",
            "outline-color", "outline-offset", "outline-style", "outline-width", "overflow", "overflow-style",
            "overflow-x", "overflow-y", "padding", "padding-bottom", "padding-left", "padding-right",
            "padding-top", "page", "page-break-after", "page-break-before", "page-break-inside", "page-policy",
            "pause", "pause-after", "pause-before", "perspective", "perspective-origin", "phonemes",
            "pitch", "pitch-range", "play-during", "position", "presentation-level", "punctuation-trim",
            "quotes", "rendering-intent", "resize", "rest", "rest-after", "rest-before",
            "richness", "right", "rotation", "rotation-point", "ruby-align", "ruby-overhang",
            "ruby-position", "ruby-span", "size", "speak", "speak-header", "speak-numeral",
            "speak-punctuation", "speech-rate", "stress", "string-set", "table-layout", "target",
            "target-name", "target-new", "target-position", "text-align", "text-align-last", "text-decoration",
            "text-emphasis", "text-height", "text-indent", "text-justify", "text-outline", "text-overflow",
            "text-shadow", "text-transform", "text-wrap", "top", "transform", "transform-origin",
            "transform-style", "transition", "transition-delay", "transition-duration", "transition-property", "transition-timing-function",
            "unicode-bidi", "vertical-align", "visibility", "voice-balance", "voice-duration", "voice-family",
            "voice-pitch", "voice-pitch-range", "voice-rate", "voice-stress", "voice-volume", "volume",
            "white-space", "white-space-collapse", "widows", "width", "word-break", "word-spacing",
            "word-wrap", "fixed", "linear-gradient", "color-dodge", "center", "content-box",
            "-webkit-flex", "flex", "row-reverse", "space-around", "first", "justify",
            "inter-word", "uppercase", "lowercase", "capitalize", "nowrap", "break-all",
            "break-word", "overline", "line-through", "wavy", "myFirstFont", "sensation"}
        Dim i As Integer = 0
        For Each T As String In csskey
            PrpertyValue.Add(CType(i, CssProperty), T)
            i += 1
        Next
    End Sub

    Private Sub setHTML()
        tag = New Dictionary(Of HtmlTags, String)()
        Dim tags As String() = {"h1", "h2", "h3", "h4", "h5", "h6",
            "body", "a", "img", "ol", " ul", "li",
            "table", "tr", "th", "nav", "heder", "footer",
            "form", "option", "select", "button", "textarea", "input",
            "audio", "video", "iframe", "hr", "em", "div",
            "pre", "p", "span"}
        Dim i As Integer = 0
        For Each T As String In tags
            tag.Add(CType(i, HtmlTags), T)
            i += 1
        Next
    End Sub

    ''' <summary>
    ''' Initilise the pasrser with Css code.
    ''' </summary>
    Sub New()
        setHTML()
        SetPrpertyValue()
    End Sub
End Module
