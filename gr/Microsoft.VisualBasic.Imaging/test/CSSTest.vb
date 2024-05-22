#Region "Microsoft.VisualBasic::3959bdc358d376150280df54f20af0fb, gr\Microsoft.VisualBasic.Imaging\test\CSSTest.vb"

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

    '   Total Lines: 122
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 104 (85.25%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (14.75%)
    '     File Size: 7.70 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::52390e14d028aef0509124471e112aa5, gr\Microsoft.VisualBasic.Imaging\test\CSSTest.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    ' Module CSSTest
'    ' 
'    '     Sub: Main, SetPrpertyValue
'    ' 
'    ' /********************************************************************************/

'#End Region

'Module CSSTest
'    Sub Main()
'        '        Dim html$ = "<html>
'        '<style>

'        '#font {

'        'font-size: 55px;
'        'color: red;
'        '}

'        '</style>

'        '<div id=""font"">12345</div>

'        '</html>"


'        '        Using g = New Size(100, 100).CreateGDIDevice
'        '            Call g.Graphics.Render(html, New Point(0, 0), 10)
'        '        End Using

'        Call SetPrpertyValue()
'    End Sub
'    Private Sub SetPrpertyValue()
'        Dim csskey As String() = {
'            "font-weight", "border-radius", "color-stop", "alignment-adjust", "alignment-baseline", "animation",
'            "animation-delay", "animation-direction", "animation-duration", "animation-iteration-count", "animation-name", "animation-play-state",
'            "animation-timing-function", "appearance", "azimuth", "backface-visibility", "background", "background-attachment",
'            "background-break", "background-clip", "background-color", "background-image", "background-origin", "background-position",
'            "background-repeat", "background-size", "baseline-shift", "binding", "bleed", "bookmark-label",
'            "bookmark-level", "bookmark-state", "bookmark-target", "border", "border-bottom", "border-bottom-color",
'            "border-bottom-left-radius", "border-bottom-right-radius", "border-bottom-style", "border-bottom-width", "border-collapse", "border-color",
'            "border-image", "border-image-outset", "border-image-repeat", "border-image-slice", "border-image-source", "border-image-width",
'            "border-left", "border-left-color", "border-left-style", "border-left-width", "border-right", "border-right-color",
'            "border-right-style", "border-right-width", "border-spacing", "border-style", "border-top", "border-top-color",
'            "border-top-left-radius", "border-top-right-radius", "border-top-style", "border-top-width", "border-width", "bottom",
'            "box-align", "box-decoration-break", "box-direction", "box-flex", "box-flex-group", "box-lines",
'            "box-ordinal-group", "box-orient", "box-pack", "box-shadow", "box-sizing", "break-after",
'            "break-before", "break-inside", "caption-side", "clear", "clip", "color",
'            "color-profile", "column-count", "column-fill", "column-gap", "column-rule", "column-rule-color",
'            "column-rule-style", "column-rule-width", "column-span", "column-width", "columns", "content",
'            "counter-increment", "counter-reset", "crop", "cue", "cue-after", "cue-before",
'            "cursor", "direction", "display", "dominant-baseline", "drop-initial-after-adjust", "drop-initial-after-align",
'            "drop-initial-before-adjust", "drop-initial-before-align", "drop-initial-size", "drop-initial-value", "elevation", "empty-cells",
'            "filter", "fit", "fit-position", "float-offset", "font", "font-effect",
'            "font-emphasize", "font-family", "font-size", "font-size-adjust", "font-stretch", "font-style",
'            "font-variant", "grid-columns", "grid-rows", "hanging-punctuation", "height", "hyphenate-after",
'            "hyphenate-before", "hyphenate-character", "hyphenate-lines", "hyphenate-resource", "hyphens", "icon",
'            "image-orientation", "image-rendering", "image-resolution", "inline-box-align", "left", "letter-spacing",
'            "line-height", "line-stacking", "line-stacking-ruby", "line-stacking-shift", "line-stacking-strategy", "list-style",
'            "list-style-image", "list-style-position", "list-style-type", "margin", "margin-bottom", "margin-left",
'            "margin-right", "margin-top", "mark", "mark-after", "mark-before", "marker-offset",
'            "marks", "marquee-direction", "marquee-play-count", "marquee-speed", "marquee-style", "max-height",
'            "max-width", "min-height", "min-width", "move-to", "nav-down", "nav-index",
'            "nav-left", "nav-right", "nav-up", "opacity", "orphans", "outline",
'            "outline-color", "outline-offset", "outline-style", "outline-width", "overflow", "overflow-style",
'            "overflow-x", "overflow-y", "padding", "padding-bottom", "padding-left", "padding-right",
'            "padding-top", "page", "page-break-after", "page-break-before", "page-break-inside", "page-policy",
'            "pause", "pause-after", "pause-before", "perspective", "perspective-origin", "phonemes",
'            "pitch", "pitch-range", "play-during", "position", "presentation-level", "punctuation-trim",
'            "quotes", "rendering-intent", "resize", "rest", "rest-after", "rest-before",
'            "richness", "right", "rotation", "rotation-point", "ruby-align", "ruby-overhang",
'            "ruby-position", "ruby-span", "size", "speak", "speak-header", "speak-numeral",
'            "speak-punctuation", "speech-rate", "stress", "string-set", "table-layout", "target",
'            "target-name", "target-new", "target-position", "text-align", "text-align-last", "text-decoration",
'            "text-emphasis", "text-height", "text-indent", "text-justify", "text-outline", "text-overflow",
'            "text-shadow", "text-transform", "text-wrap", "top", "transform", "transform-origin",
'            "transform-style", "transition", "transition-delay", "transition-duration", "transition-property", "transition-timing-function",
'            "unicode-bidi", "vertical-align", "visibility", "voice-balance", "voice-duration", "voice-family",
'            "voice-pitch", "voice-pitch-range", "voice-rate", "voice-stress", "voice-volume", "volume",
'            "white-space", "white-space-collapse", "widows", "width", "word-break", "word-spacing",
'            "word-wrap", "fixed", "linear-gradient", "color-dodge", "center", "content-box",
'            "-webkit-flex", "flex", "row-reverse", "space-around", "first", "justify",
'            "inter-word", "uppercase", "lowercase", "capitalize", "nowrap", "break-all",
'            "break-word", "overline", "line-through", "wavy", "myFirstFont", "sensation"}

'        Call csskey.EnumCodeHelper("CssProperty",, pascalStyle:=False).SaveTo("./CssProperty.vb")
'    End Sub

'End Module
