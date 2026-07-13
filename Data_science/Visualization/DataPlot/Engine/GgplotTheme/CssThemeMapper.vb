#Region "Microsoft.VisualBasic::9259d885ce51935b77a94fa463739841, Data_science\Visualization\DataPlot\Engine\GgplotTheme\CssThemeMapper.vb"

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

    '   Total Lines: 672
    '    Code Lines: 443 (65.92%)
    ' Comment Lines: 142 (21.13%)
    '    - Xml Docs: 46.48%
    ' 
    '   Blank Lines: 87 (12.95%)
    '     File Size: 28.46 KB


    '     Class CssRule
    ' 
    '         Properties: Properties, Selector
    ' 
    '         Function: ToString
    ' 
    '     Class CssThemeMapper
    ' 
    '         Function: ParseCss, ParseCssFile, ParseCssUnitValue, ParseDouble, ParseRules
    '                   ParseScalarValue, ScalarToCss, SerializeCss, SkipWhitespace, SkipWhitespaceAndComments
    ' 
    '         Sub: AppendElementRule, ParseAtRule, ParseElementRule, ParseMarginElement, ParseScalarElement
    '              ParseThemeElement, ParseUnitElement, SerializeCssFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace GgplotTheme

    '==========================================================================
    ' CssRule - Represents a single CSS rule (selector + properties)
    '==========================================================================
    ''' <summary>
    ''' Represents a single CSS rule with a selector and a set of properties.
    ''' </summary>
    Public Class CssRule
        Public Property Selector As String = ""
        Public Property Properties As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder()
            sb.Append(Selector).Append(" {").AppendLine()
            For Each kvp As KeyValuePair(Of String, String) In Properties
                sb.Append("    ").Append(kvp.Key).Append(": ").Append(kvp.Value).Append(";").AppendLine()
            Next
            sb.Append("}").AppendLine()
            Return sb.ToString()
        End Function
    End Class

    '==========================================================================
    ' CssThemeMapper - CSS parser and serializer for Theme objects
    '==========================================================================
    ''' <summary>
    ''' The CSS theme mapping module. Provides two public functions:
    ''' - ParseCss: Parses a CSS string/file into a Theme object
    ''' - SerializeCss: Serializes a Theme object to a CSS string/file
    '''
    ''' CSS Format:
    '''   /* Comments are supported */
    '''   
    '''   @canvas {
    '''       width: 800px;
    '''       height: 600px;
    '''       ppi: 96;
    '''       base-font-size: 11pt;
    '''   }
    '''   
    '''   text {
    '''       family: "sans-serif";
    '''       size: 11pt;
    '''       color: #333333;
    '''       face: plain;
    '''       hjust: 0.5;
    '''       vjust: 0.5;
    '''       angle: 0;
    '''       lineheight: 1.2;
    '''       margin: 2pt 2pt 2pt 2pt;
    '''   }
    '''   
    '''   axis.title {
    '''       face: bold;
    '''   }
    '''   
    '''   panel.grid.minor.y {
    '''       blank: true;
    '''   }
    '''   
    '''   axis.ticks.length {
    '''       length: 0.25cm;
    '''   }
    '''   
    '''   plot.margin {
    '''       margin: 5pt 5pt 5pt 5pt;
    '''   }
    '''   
    '''   legend.position {
    '''       value: right;
    '''   }
    ''' </summary>
    Public Class CssThemeMapper

        '==================================================================
        ' PUBLIC FUNCTION 1: Parse CSS string into Theme object
        '==================================================================

        ''' <summary>
        ''' Parses a CSS string into a Theme object.
        ''' This is one of the two main public functions of the CSS mapping module.
        ''' </summary>
        ''' <param name="css">The CSS string to parse.</param>
        ''' <returns>A Theme object populated from the CSS.</returns>
        Public Shared Function ParseCss(css As String) As Theme
            If String.IsNullOrWhiteSpace(css) Then Return New Theme()

            Dim theme As New Theme()
            Dim rules As List(Of CssRule) = ParseRules(css)

            For Each rule As CssRule In rules
                If rule.Selector.StartsWith("@") Then
                    ' At-rule (e.g., @canvas)
                    ParseAtRule(theme, rule)
                Else
                    ' Normal element rule
                    ParseElementRule(theme, rule)
                End If
            Next

            Return theme
        End Function

        ''' <summary>
        ''' Parses a CSS file into a Theme object.
        ''' </summary>
        ''' <param name="filePath">Path to the CSS file.</param>
        ''' <returns>A Theme object populated from the CSS file.</returns>
        Public Shared Function ParseCssFile(filePath As String) As Theme
            If Not File.Exists(filePath) Then
                Throw New FileNotFoundException($"CSS file not found: {filePath}")
            End If
            Dim css As String = File.ReadAllText(filePath)
            Return ParseCss(css)
        End Function

        '==================================================================
        ' PUBLIC FUNCTION 2: Serialize Theme object to CSS string
        '==================================================================

        ''' <summary>
        ''' Serializes a Theme object to a CSS string.
        ''' This is one of the two main public functions of the CSS mapping module.
        ''' </summary>
        ''' <param name="theme">The Theme object to serialize.</param>
        ''' <param name="includeDefaults">If True, includes default values for root elements.</param>
        ''' <returns>A CSS string representing the theme.</returns>
        Public Shared Function SerializeCss(theme As Theme, Optional includeDefaults As Boolean = False) As String
            If theme Is Nothing Then Return ""

            Dim sb As New StringBuilder()

            ' Header comment
            sb.AppendLine("/*")
            sb.AppendLine(" * Ggplot Theme CSS")
            sb.AppendLine(" * Generated by CssThemeMapper")
            sb.AppendLine(" */")
            sb.AppendLine()

            ' Canvas context
            If theme.Canvas IsNot Nothing Then
                sb.AppendLine("@canvas {")
                sb.AppendLine($"    width: {theme.Canvas.WidthPx}px;")
                sb.AppendLine($"    height: {theme.Canvas.HeightPx}px;")
                sb.AppendLine($"    ppi: {theme.Canvas.Ppi};")
                sb.AppendLine($"    base-font-size: {theme.Canvas.BaseFontSizePt}pt;")
                sb.AppendLine("}")
                sb.AppendLine()
            End If

            ' Get all element names and sort them by category for readability
            Dim allNames As List(Of String) = InheritanceTree.GetAllElementNames()
            allNames.Sort()

            ' Group elements by type for organized output
            Dim elementLines As New List(Of String)()
            Dim elementRects As New List(Of String)()
            Dim elementTexts As New List(Of String)()
            Dim elementPoints As New List(Of String)()
            Dim elementPolygons As New List(Of String)()
            Dim elementGeoms As New List(Of String)()
            Dim unitElements As New List(Of String)()
            Dim marginElements As New List(Of String)()
            Dim scalarElements As New List(Of String)()

            For Each name As String In allNames
                Dim elemType As ThemeElementType = InheritanceTree.GetElementType(name)
                Select Case elemType
                    Case ThemeElementType.ElementLine
                        If theme.HasElement(name) Then elementLines.Add(name)
                    Case ThemeElementType.ElementRect
                        If theme.HasElement(name) Then elementRects.Add(name)
                    Case ThemeElementType.ElementText
                        If theme.HasElement(name) Then elementTexts.Add(name)
                    Case ThemeElementType.ElementPoint
                        If theme.HasElement(name) Then elementPoints.Add(name)
                    Case ThemeElementType.ElementPolygon
                        If theme.HasElement(name) Then elementPolygons.Add(name)
                    Case ThemeElementType.ElementGeom
                        If theme.HasElement(name) Then elementGeoms.Add(name)
                    Case ThemeElementType.Unit
                        If theme.HasUnit(name) Then unitElements.Add(name)
                    Case ThemeElementType.Margin
                        If theme.HasMargin(name) Then marginElements.Add(name)
                    Case ThemeElementType.Scalar
                        If theme.HasScalar(name) Then scalarElements.Add(name)
                End Select
            Next

            ' Output line elements
            If elementLines.Count > 0 Then
                sb.AppendLine("/* ===== Line Elements ===== */")
                For Each name As String In elementLines
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output rect elements
            If elementRects.Count > 0 Then
                sb.AppendLine("/* ===== Rect Elements ===== */")
                For Each name As String In elementRects
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output text elements
            If elementTexts.Count > 0 Then
                sb.AppendLine("/* ===== Text Elements ===== */")
                For Each name As String In elementTexts
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output point elements
            If elementPoints.Count > 0 Then
                sb.AppendLine("/* ===== Point Elements ===== */")
                For Each name As String In elementPoints
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output polygon elements
            If elementPolygons.Count > 0 Then
                sb.AppendLine("/* ===== Polygon Elements ===== */")
                For Each name As String In elementPolygons
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output geom elements
            If elementGeoms.Count > 0 Then
                sb.AppendLine("/* ===== Geom Elements ===== */")
                For Each name As String In elementGeoms
                    AppendElementRule(sb, name, theme.GetElement(name))
                Next
                sb.AppendLine()
            End If

            ' Output unit elements
            If unitElements.Count > 0 Then
                sb.AppendLine("/* ===== Unit Elements (spacing, lengths) ===== */")
                For Each name As String In unitElements
                    Dim u As Unit = theme.GetUnit(name)
                    If u IsNot Nothing Then
                        sb.Append(name).Append(" {").AppendLine()
                        sb.Append("    length: ").Append(u.ToString()).Append(";").AppendLine()
                        sb.Append("}").AppendLine()
                    End If
                Next
                sb.AppendLine()
            End If

            ' Output margin elements
            If marginElements.Count > 0 Then
                sb.AppendLine("/* ===== Margin Elements ===== */")
                For Each name As String In marginElements
                    Dim m As Margin = theme.GetMargin(name)
                    If m IsNot Nothing Then
                        sb.Append(name).Append(" {").AppendLine()
                        sb.Append("    margin: ")
                        sb.Append(m.Top.ToString()).Append(" ")
                        sb.Append(m.Right.ToString()).Append(" ")
                        sb.Append(m.Bottom.ToString()).Append(" ")
                        sb.Append(m.Left.ToString()).Append(";").AppendLine()
                        sb.Append("}").AppendLine()
                    End If
                Next
                sb.AppendLine()
            End If

            ' Output scalar elements
            If scalarElements.Count > 0 Then
                sb.AppendLine("/* ===== Scalar Properties ===== */")
                For Each name As String In scalarElements
                    Dim val As Object = theme.GetScalar(name)
                    If val IsNot Nothing Then
                        sb.Append(name).Append(" {").AppendLine()
                        sb.Append("    value: ").Append(ScalarToCss(val)).Append(";").AppendLine()
                        sb.Append("}").AppendLine()
                    End If
                Next
                sb.AppendLine()
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Serializes a Theme object to a CSS file.
        ''' </summary>
        ''' <param name="theme">The Theme object to serialize.</param>
        ''' <param name="filePath">Path to the output CSS file.</param>
        ''' <param name="includeDefaults">If True, includes default values for root elements.</param>
        Public Shared Sub SerializeCssFile(theme As Theme, filePath As String, Optional includeDefaults As Boolean = False)
            Dim css As String = SerializeCss(theme, includeDefaults)
            File.WriteAllText(filePath, css)
        End Sub

        '==================================================================
        ' Private: CSS Parsing
        '==================================================================

        Private Shared Function ParseRules(css As String) As List(Of CssRule)
            Dim rules As New List(Of CssRule)()
            Dim pos As Integer = 0
            Dim len As Integer = css.Length

            While pos < len
                ' Skip whitespace and comments
                pos = SkipWhitespaceAndComments(css, pos)
                If pos >= len Then Exit While

                ' Read selector (everything until '{')
                Dim selectorStart As Integer = pos
                While pos < len AndAlso css(pos) <> "{"c
                    pos += 1
                End While

                If pos >= len Then Exit While

                Dim selector As String = css.Substring(selectorStart, pos - selectorStart).Trim()
                pos += 1 ' skip '{'

                ' Skip whitespace
                pos = SkipWhitespaceAndComments(css, pos)

                ' Read properties until '}'
                Dim rule As New CssRule()
                rule.Selector = selector

                While pos < len AndAlso css(pos) <> "}"c
                    ' Skip whitespace and comments
                    pos = SkipWhitespaceAndComments(css, pos)
                    If pos >= len OrElse css(pos) = "}"c Then Exit While

                    ' Read property name (until ':')
                    Dim nameStart As Integer = pos
                    While pos < len AndAlso css(pos) <> ":"c AndAlso css(pos) <> "}"c
                        pos += 1
                    End While

                    If pos >= len OrElse css(pos) = "}"c Then Exit While

                    Dim propName As String = css.Substring(nameStart, pos - nameStart).Trim()
                    pos += 1 ' skip ':'

                    ' Skip whitespace
                    pos = SkipWhitespace(css, pos)

                    ' Read property value (until ';' or '}')
                    ' Need to handle quoted strings carefully
                    Dim valueSb As New StringBuilder()
                    Dim inQuotes As Boolean = False
                    Dim quoteChar As Char = """"c

                    While pos < len
                        Dim c As Char = css(pos)
                        If inQuotes Then
                            valueSb.Append(c)
                            If c = quoteChar Then
                                inQuotes = False
                            End If
                        Else
                            If c = """"c OrElse c = "'"c Then
                                inQuotes = True
                                quoteChar = c
                                valueSb.Append(c)
                            ElseIf c = ";"c OrElse c = "}"c Then
                                Exit While
                            Else
                                valueSb.Append(c)
                            End If
                        End If
                        pos += 1
                    End While

                    Dim propValue As String = valueSb.ToString().Trim()

                    If propName.Length > 0 Then
                        rule.Properties(propName) = propValue
                    End If

                    ' Skip ';'
                    If pos < len AndAlso css(pos) = ";"c Then
                        pos += 1
                    End If

                    ' Skip whitespace
                    pos = SkipWhitespaceAndComments(css, pos)
                End While

                ' Skip '}'
                If pos < len AndAlso css(pos) = "}"c Then
                    pos += 1
                End If

                If rule.Selector.Length > 0 Then
                    rules.Add(rule)
                End If
            End While

            Return rules
        End Function

        Private Shared Function SkipWhitespaceAndComments(s As String, pos As Integer) As Integer
            Dim len As Integer = s.Length
            While pos < len
                Dim c As Char = s(pos)
                If Char.IsWhiteSpace(c) Then
                    pos += 1
                ElseIf pos + 1 < len AndAlso c = "/"c AndAlso s(pos + 1) = "*"c Then
                    ' Skip comment
                    pos += 2
                    While pos + 1 < len AndAlso Not (s(pos) = "*"c AndAlso s(pos + 1) = "/"c)
                        pos += 1
                    End While
                    pos += 2
                ElseIf c = "#"c AndAlso pos + 1 < len AndAlso s(pos + 1) = "!"c Then
                    ' Skip single-line comment (#! style)
                    While pos < len AndAlso s(pos) <> vbLf AndAlso s(pos) <> vbCr
                        pos += 1
                    End While
                Else
                    Exit While
                End If
            End While
            Return pos
        End Function

        Private Shared Function SkipWhitespace(s As String, pos As Integer) As Integer
            Dim len As Integer = s.Length
            While pos < len AndAlso Char.IsWhiteSpace(s(pos))
                pos += 1
            End While
            Return pos
        End Function

        '==================================================================
        ' Private: Parse at-rules (@canvas)
        '==================================================================

        Private Shared Sub ParseAtRule(theme As Theme, rule As CssRule)
            Dim atType As String = rule.Selector.ToLowerInvariant()

            If atType = "@canvas" Then
                Dim canvas As New CanvasContext()

                For Each kvp As KeyValuePair(Of String, String) In rule.Properties
                    Select Case kvp.Key.ToLowerInvariant()
                        Case "width"
                            canvas.WidthPx = ParseCssUnitValue(kvp.Value)
                        Case "height"
                            canvas.HeightPx = ParseCssUnitValue(kvp.Value)
                        Case "ppi", "dpi"
                            canvas.Ppi = ParseDouble(kvp.Value)
                        Case "base-font-size"
                            Dim u As Unit = Unit.Parse(kvp.Value)
                            If u.Type = UnitType.Pt Then
                                canvas.BaseFontSizePt = u.Value
                            ElseIf u.Type = UnitType.Px Then
                                canvas.BaseFontSizePt = u.Value * 72.0 / canvas.Ppi
                            End If
                    End Select
                Next

                theme.Canvas = canvas
            End If
        End Sub

        '==================================================================
        ' Private: Parse element rules
        '==================================================================

        Private Shared Sub ParseElementRule(theme As Theme, rule As CssRule)
            Dim name As String = rule.Selector.Trim()

            ' Handle multiple selectors separated by commas
            Dim names() As String = name.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
            For Each singleName As String In names
                singleName = singleName.Trim()
                If singleName.Length = 0 Then Continue For

                If Not InheritanceTree.IsKnownElement(singleName) Then
                    ' Unknown element name - skip or could throw
                    Continue For
                End If

                Dim elemType As ThemeElementType = InheritanceTree.GetElementType(singleName)

                Select Case elemType
                    Case ThemeElementType.ElementLine, ThemeElementType.ElementRect,
                         ThemeElementType.ElementText, ThemeElementType.ElementPoint,
                         ThemeElementType.ElementPolygon, ThemeElementType.ElementGeom
                        ParseThemeElement(theme, singleName, elemType, rule.Properties)

                    Case ThemeElementType.Unit
                        ParseUnitElement(theme, singleName, rule.Properties)

                    Case ThemeElementType.Margin
                        ParseMarginElement(theme, singleName, rule.Properties)

                    Case ThemeElementType.Scalar
                        ParseScalarElement(theme, singleName, rule.Properties)
                End Select
            Next
        End Sub

        Private Shared Sub ParseThemeElement(theme As Theme, name As String,
                                             elemType As ThemeElementType,
                                             props As Dictionary(Of String, String))
            ' Get existing element or create new
            Dim elem As ThemeElement = theme.GetElement(name)
            If elem Is Nothing Then
                Select Case elemType
                    Case ThemeElementType.ElementLine : elem = New ElementLine()
                    Case ThemeElementType.ElementRect : elem = New ElementRect()
                    Case ThemeElementType.ElementText : elem = New ElementText()
                    Case ThemeElementType.ElementPoint : elem = New ElementPoint()
                    Case ThemeElementType.ElementPolygon : elem = New ElementPolygon()
                    Case ThemeElementType.ElementGeom : elem = New ElementGeom()
                End Select
            End If

            If elem Is Nothing Then Return

            ' Apply properties
            For Each kvp As KeyValuePair(Of String, String) In props
                elem.SetPropertyFromCss(kvp.Key, kvp.Value)
            Next

            elem.IsSet = True
            theme.SetElement(name, elem)
        End Sub

        Private Shared Sub ParseUnitElement(theme As Theme, name As String,
                                            props As Dictionary(Of String, String))
            For Each kvp As KeyValuePair(Of String, String) In props
                If kvp.Key.ToLowerInvariant() = "length" OrElse kvp.Key.ToLowerInvariant() = "value" Then
                    Dim u As Unit = Unit.Parse(kvp.Value)
                    theme.SetUnit(name, u)
                    Exit For
                End If
            Next
        End Sub

        Private Shared Sub ParseMarginElement(theme As Theme, name As String,
                                              props As Dictionary(Of String, String))
            For Each kvp As KeyValuePair(Of String, String) In props
                If kvp.Key.ToLowerInvariant() = "margin" OrElse kvp.Key.ToLowerInvariant() = "value" Then
                    Dim m As Margin = Margin.Parse(kvp.Value)
                    theme.SetMargin(name, m)
                    Exit For
                End If
            Next
        End Sub

        Private Shared Sub ParseScalarElement(theme As Theme, name As String,
                                              props As Dictionary(Of String, String))
            For Each kvp As KeyValuePair(Of String, String) In props
                If kvp.Key.ToLowerInvariant() = "value" Then
                    Dim val As Object = ParseScalarValue(kvp.Value)
                    theme.SetScalar(name, val)
                    Exit For
                End If
            Next
        End Sub

        '==================================================================
        ' Private: Value parsing helpers
        '==================================================================

        Private Shared Function ParseCssUnitValue(s As String) As Double
            ' Parse values like "800px", "600px", "96"
            s = s.Trim()
            ' Remove trailing unit if present
            Dim numEnd As Integer = 0
            While numEnd < s.Length AndAlso (Char.IsDigit(s(numEnd)) OrElse s(numEnd) = "."c OrElse s(numEnd) = "-"c)
                numEnd += 1
            End While
            Dim numStr As String = s.Substring(0, numEnd)
            Dim result As Double
            If Double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, result) Then
                Return result
            End If
            Return 0
        End Function

        Private Shared Function ParseDouble(s As String) As Double
            If String.IsNullOrWhiteSpace(s) Then Return 0
            Dim result As Double
            If Double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, result) Then
                Return result
            End If
            Return 0
        End Function

        Private Shared Function ParseScalarValue(s As String) As Object
            If String.IsNullOrWhiteSpace(s) Then Return Nothing
            s = s.Trim()

            ' Check for quoted string
            If s.StartsWith("""") AndAlso s.EndsWith("""") Then
                Return s.Substring(1, s.Length - 2)
            End If
            If s.StartsWith("'") AndAlso s.EndsWith("'") Then
                Return s.Substring(1, s.Length - 2)
            End If

            ' Check for special values
            Dim lower As String = s.ToLowerInvariant()
            If lower = "null" OrElse lower = "na" Then Return Nothing
            If lower = "true" Then Return True
            If lower = "false" Then Return False

            ' Try to parse as double
            Dim d As Double
            If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                Return d
            End If

            ' Return as string
            Return s
        End Function

        Private Shared Function ScalarToCss(val As Object) As String
            If val Is Nothing Then Return "null"
            If TypeOf val Is Boolean Then
                Return If(CBool(val), "true", "false")
            End If
            If TypeOf val Is Double Then
                Return CStr(val)
            End If
            If TypeOf val Is Integer Then
                Return CStr(val)
            End If
            ' String - check if it needs quoting
            Dim s As String = val.ToString()
            If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, Nothing) Then
                Return s
            End If
            ' Quote non-numeric strings
            Return """" & s.Replace("""", "\""") & """"
        End Function

        '==================================================================
        ' Private: CSS serialization helpers
        '==================================================================

        Private Shared Sub AppendElementRule(sb As StringBuilder, name As String, elem As ThemeElement)
            If elem Is Nothing Then Return
            Dim body As String = elem.ToCssBody()
            If String.IsNullOrWhiteSpace(body) Then Return

            sb.Append(name).Append(" {").AppendLine()
            sb.Append(body)
            sb.Append("}").AppendLine()
        End Sub
    End Class

End Namespace
