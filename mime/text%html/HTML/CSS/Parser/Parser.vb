Imports System.Text.RegularExpressions

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
Public Class CssParser

    Public Class NULL_TAG : Inherits Exception

        Public Sub New()
            MyBase.New("Tag name is null.")
        End Sub
    End Class

    Public Class NULL_PRORERTY_VALUE : Inherits Exception

        Public Sub New()
            MyBase.New("Property value is null.")
        End Sub
    End Class

    Dim PrpertyValue As Dictionary(Of CssProperty, String)
    Dim tag As Dictionary(Of HtmlTags, String)

    ReadOnly TagWithCSSList As List(Of TagWithCSS)

    Private Function GetTagWithCSS(input As String) As List(Of TagWithCSS)
        Dim TagWithCSSList As New List(Of TagWithCSS)()
        Dim IndivisualTag As List(Of String) = IndivisualTags(input)
        For Each tag As String In IndivisualTag
            Dim tagname As String() = Regex.Split(tag, "[{]")
            Dim TWCSS As New TagWithCSS()
            If RemoveWhitespace(tagname(0)) <> "" Then
                TWCSS.TagName = RemoveWitespaceFormStartAndEnd(tagname(0))
                TWCSS.Properties = GetProperty(GetBetween(tag, "{", "}"))
                TagWithCSSList.Add(TWCSS)
            End If
        Next
        Return TagWithCSSList
    End Function

    Const IndivisualTagsPattern$ = "(?<selector>(?:(?:[^,{]+),?)*?)\{(?:(?<name>[^}:]+):?(?<value>[^};]+);?)*?\}"

    Private Function IndivisualTags(input As String) As List(Of String)
        Dim b As New List(Of String)()

        For Each m As Match In Regex.Matches(input, IndivisualTagsPattern)
            b.Add(m.Value)
        Next

        Return b
    End Function

    Private Function GetProperty(input As String) As List(Of [Property])
        Dim p As New List(Of [Property])()
        Dim s As String() = Regex.Split(input, "[;]")
        Dim i As Integer = 0
        For Each b As String In s
            If b <> "" Then
                Dim t As String() = Regex.Split(s(i), "[:]")
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
    '
    '
    '  Public 
    '
    '
    Public Overrides Function ToString() As String
        Dim ToRreturn As String = String.Empty
        For Each T As TagWithCSS In TagWithCSSList
#If DEBUG Then
            ToRreturn += "//---------------------------" & T.TagName & "-----------------------------" & vbLf
#End If
            ToRreturn += T.TagName
            ToRreturn += vbLf & "{" & vbLf
            For Each p As [Property] In T.Properties
                ToRreturn += vbTab & p.PropertyName & ":" & p.PropertyValue & ";" & vbLf
            Next

            ToRreturn += "}" & vbLf
        Next
        Return ToRreturn
    End Function

    ''' <summary>
    ''' Remove property form given tag.
    ''' </summary>
    ''' <param name="Tag">Tage to remove property.</param>
    ''' <param name="Proverty">Property to remove.</param>
    ''' <returns>True if removed.</returns>
    Public Function RemoveProperty(Tag As String, Proverty As CssProperty) As Boolean
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        Dim pointinTagWithCSSList As Integer = 0
        Dim pointinProperties As Integer = 0
        Dim ProvertyName As String = PrpertyValue(Proverty)
        For Each T As TagWithCSS In TagWithCSSList
            If T.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                For Each p As [Property] In T.Properties
                    If p.PropertyName.Equals(ProvertyName, StringComparison.InvariantCultureIgnoreCase) Then
                        TagWithCSSList(pointinTagWithCSSList).Properties.RemoveAt(pointinProperties)
                        Return True
                    End If
                    pointinProperties += 1

                Next
            End If
            pointinTagWithCSSList += 1
        Next
        Return False
    End Function
    ''' <summary>
    ''' Remove property form given tag.
    ''' </summary>
    ''' <param name="Tag">Tage to remove property.</param>
    ''' <param name="Proverty">Property to remove.</param>
    ''' <returns>True if removed.</returns>
    Public Function RemoveProperty(tag As HtmlTags, Proverty As CssProperty) As Boolean
        Return RemoveProperty(Me.tag(tag), Proverty)
    End Function
    ''' <summary>
    ''' Remove Tag and it's properties.
    ''' </summary>
    ''' <param name="Tag">Tag to remove.</param>
    ''' <returns>True if removed.</returns>
    Public Function RemoveTag(Tag As String) As Boolean
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        Dim pointinTagWithCSSList As Integer = 0

        For Each T As TagWithCSS In TagWithCSSList
            If T.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                TagWithCSSList.RemoveAt(pointinTagWithCSSList)
                Return True
            End If
            pointinTagWithCSSList += 1
        Next
        Return False
    End Function
    ''' <summary>
    ''' Remove Tag and it's properties.
    ''' </summary>
    ''' <param name="Tag">Tag to remove.</param>
    ''' <returns>True if removed.</returns>
    Public Function RemoveTag(tag As HtmlTags) As Boolean
        Return RemoveTag(Me.tag(tag))
    End Function
    ''' <summary>
    ''' Get list of properties on tag.
    ''' </summary>
    ''' <param name="Tag">Tag to get properties</param>
    ''' <returns>Property List</returns>
    Public Function GetProperties(Tag As String) As List(Of [Property])
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        Dim pointinTagWithCSSList As Integer = 0

        For Each T As TagWithCSS In TagWithCSSList
            If T.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                Return T.Properties
            End If
            pointinTagWithCSSList += 1
        Next
        Return New List(Of [Property])()
    End Function
    ''' <summary>
    ''' Get list of properties on tag.
    ''' </summary>
    ''' <param name="Tag">Tag to get properties</param>
    ''' <returns>Property List</returns>
    Public Function GetProperties(tag As HtmlTags) As List(Of [Property])
        Return GetProperties(Me.tag(tag))
    End Function
    ''' <summary>
    ''' Get property.
    ''' </summary>
    ''' <param name="Tag">Name of tag.</param>
    ''' <param name="Property">Property to value.</param>
    ''' <returns>Property</returns>
    Public Function GetPropertie(Tag As String, [Property] As CssProperty) As [Property]
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        Dim pointinTagWithCSSList As Integer = 0

        For Each T As TagWithCSS In TagWithCSSList
            If T.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                For Each p As [Property] In T.Properties
                    If p.PropertyName.Equals(PrpertyValue([Property]), StringComparison.InvariantCultureIgnoreCase) Then
                        Return p
                    End If
                Next
            End If
            pointinTagWithCSSList += 1
        Next
        Return New [Property]()
    End Function
    ''' <summary>
    ''' Get property.
    ''' </summary>
    ''' <param name="Tag">Name of tag.</param>
    ''' <param name="Property">Property to value.</param>
    ''' <returns>Property</returns>
    Public Function GetPropertie(tag As HtmlTags, [Property] As CssProperty) As [Property]
        Return GetPropertie(Me.tag(tag), [Property])
    End Function
    ''' <summary>
    ''' Add/Overwrite property or property's value of tag.  
    ''' </summary>
    ''' <param name="Tag">Tag name.</param>
    ''' <param name="property">Property to add/overwrite</param>
    ''' <param name="PropertValue">Property's valueto add/overwrite</param>
    ''' <param name="Type">Type of tag (HTML tag ,Class or Id).Deffalt HTML Tag.</param>
    ''' <returns>Added/Overwrited or not.</returns>
    Public Function AddPropery(Tag As String, [property] As CssProperty, PropertValue As String, Optional Type As CSSTagTypes = CSSTagTypes.tag) As Boolean
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        If PropertValue = "" OrElse PropertValue = String.Empty OrElse PropertValue = "" Then
            Throw New NULL_PRORERTY_VALUE()
        End If
        Dim pointinTagWithCSSList As Integer = 0
        Dim pointinProperties As Integer = 0
        Dim notfound As Boolean = False
        Dim added As Boolean = False

        Dim tagcannotexist As Boolean = True
        Dim tagExist As Boolean = False

        If Type = CSSTagTypes.[class] Then
            Tag = "." & Tag
        End If
        If Type = CSSTagTypes.id Then
            Tag = "#" & Tag
        End If

        Dim ProvertyName As String = PrpertyValue([property])

        For Each T__1 As TagWithCSS In TagWithCSSList
            If T__1.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                For Each p As [Property] In T__1.Properties
                    If p.PropertyName.Equals(ProvertyName, StringComparison.InvariantCultureIgnoreCase) Then
                        Dim np As New [Property]()
                        np.PropertyName = ProvertyName
                        np.PropertyValue = PropertValue
                        TagWithCSSList(pointinTagWithCSSList).Properties(pointinProperties) = np
                        added = True
                        'break;
                        Return True
                    End If
                    notfound = True
                    pointinProperties += 1
                Next

                If notfound AndAlso Not added Then
                    Dim np As New [Property]()
                    np.PropertyName = ProvertyName
                    np.PropertyValue = PropertValue
                    TagWithCSSList(pointinTagWithCSSList).Properties.Add(np)
                    added = True
                    'break;                 
                    Return True
                End If
                tagExist = True
                tagcannotexist = False
            Else
                tagExist = False
                tagcannotexist = True
            End If
            pointinTagWithCSSList += 1
        Next
        If tagcannotexist AndAlso Not tagExist Then
            Dim t__2 As New TagWithCSS()
            t__2.TagName = Tag
            Dim p As New [Property]()
            p.PropertyName = ProvertyName
            p.PropertyValue = PropertValue
            Dim pl As New List(Of [Property])()
            pl.Add(p)
            t__2.Properties = pl
            TagWithCSSList.Add(t__2)
            'break;
            Return True
        End If
        Return False
    End Function
    ''' <summary>
    ''' Add/Overwrite property or property's value of tag.  
    ''' </summary>
    ''' <param name="Tag">Tag name.</param>
    ''' <param name="property">Property to add/overwrite</param>
    ''' <param name="PropertValue">Property's valueto add/overwrite</param>
    ''' <returns>Added/Overwrited or not.</returns>
    Public Function AddPropery(tag As HtmlTags, [property] As CssProperty, PropertValue As String) As Boolean
        Return AddPropery(Me.tag(tag), [property], PropertValue)
    End Function
    ''' <summary>
    ''' Initilise the pasrser with Css code.
    ''' </summary>
    ''' <param name="input">CSS code.</param>
    Public Sub New(input As String)
        setHTML()
        SetPrpertyValue()
        TagWithCSSList = GetTagWithCSS(input)
    End Sub

    ''' <summary>
    ''' Check the existance of tag.
    ''' </summary>
    ''' <param name="Tag">Name of tag.</param>
    ''' <returns>True if exist.</returns>
    Public Function TagExist(Tag As String) As Boolean
        If Tag = "" OrElse Tag = String.Empty OrElse Tag = "" Then
            Throw New NULL_TAG()
        End If
        For Each T As TagWithCSS In TagWithCSSList
            If T.TagName.Equals(Tag, StringComparison.InvariantCultureIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function
    ''' <summary>
    ''' Check the existance of tag.
    ''' </summary>
    ''' <param name="Tag">Name of tag.</param>
    ''' <returns>True if exist.</returns>
    Public Function TagExist(tag As HtmlTags) As Boolean
        Return TagExist(Me.tag(tag))
    End Function
End Class
