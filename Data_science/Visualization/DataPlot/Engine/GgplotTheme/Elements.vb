#Region "Microsoft.VisualBasic::20f4daaaf1608ac991ae6abaabd3adad, Data_science\Visualization\DataPlot\Engine\GgplotTheme\Elements.vb"

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

    '   Total Lines: 1075
    '    Code Lines: 864 (80.37%)
    ' Comment Lines: 113 (10.51%)
    '    - Xml Docs: 73.45%
    ' 
    '   Blank Lines: 98 (9.12%)
    '     File Size: 43.04 KB


    '     Class ArrowSpec
    ' 
    '         Properties: Angle, Ends, Length, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone, ToString
    ' 
    '     Class ThemeElement
    ' 
    '         Properties: IsBlank, IsSet
    ' 
    '     Class ElementLine
    ' 
    '         Properties: Arrow, Colour, Lineend, Linetype, Linewidth
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Class ElementRect
    ' 
    '         Properties: Colour, Fill, Linetype, Linewidth
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Class ElementText
    ' 
    '         Properties: Angle, Colour, Face, Family, Hjust
    '                     Lineheight, Margin, Size, Vjust
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Class ElementPoint
    ' 
    '         Properties: Colour, Fill, Shape, Size, Stroke
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Class ElementPolygon
    ' 
    '         Properties: Colour, Fill, Group, Linetype, Linewidth
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Class ElementGeom
    ' 
    '         Properties: Accent, Ink, PaletteColourContinuous, PaletteColourDiscrete, PaletteFillContinuous
    '                     PaletteFillDiscrete, Paper
    ' 
    '         Function: Clone, ToCssBody
    ' 
    '         Sub: MergeFrom, SetPropertyFromCss
    ' 
    '     Module ElementHelpers
    ' 
    '         Function: ColorsToCss, ColorToCss, ParseBool, ParseColor, ParseColorList
    '                   ParseDouble, QuoteString, UnquoteString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Drawing
Imports System.Globalization
Imports System.Text

Namespace GgplotTheme

    '==========================================================================
    ' ArrowSpec Class
    '==========================================================================
    ''' <summary>
    ''' Arrow specification for line elements, equivalent to ggplot2's arrow().
    ''' </summary>
    <Serializable>
    Public Class ArrowSpec
        Implements ICloneable

        Private _angle As Double = 30.0
        Private _length As Unit
        Private _ends As String = "last"
        Private _type As String = "open"

        Public Sub New()
            _length = New Unit(0.25, UnitType.Inch)
        End Sub

        ''' <summary>Angle of arrow head in degrees (0-180). Default 30.</summary>
        Public Property Angle As Double
            Get
                Return _angle
            End Get
            Set(value As Double)
                _angle = value
            End Set
        End Property

        ''' <summary>Length of arrow head edges.</summary>
        Public Property Length As Unit
            Get
                Return _length
            End Get
            Set(value As Unit)
                _length = value
            End Set
        End Property

        ''' <summary>Which end(s) to draw arrows: "first", "last", or "both".</summary>
        Public Property Ends As String
            Get
                Return _ends
            End Get
            Set(value As String)
                _ends = value
            End Set
        End Property

        ''' <summary>Arrow head type: "open" or "closed".</summary>
        Public Property Type As String
            Get
                Return _type
            End Get
            Set(value As String)
                _type = value
            End Set
        End Property

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New ArrowSpec() With {
                .Angle = _angle,
                .Length = If(_length?.Clone(), Nothing),
                .Ends = _ends,
                .Type = _type
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"arrow(angle={_angle}, length={_length}, ends={_ends}, type={_type})"
        End Function
    End Class

    '==========================================================================
    ' ThemeElement Base Class
    '==========================================================================
    ''' <summary>
    ''' Base class for all theme elements. Each property can be Nothing/null
    ''' meaning "inherit from parent element". This enables property-level
    ''' inheritance matching ggplot2's theme inheritance system.
    ''' </summary>
    <Serializable>
    Public MustInherit Class ThemeElement
        Implements ICloneable

        ''' <summary>
        ''' If True, this element is "blank" (element_blank) and should not be rendered.
        ''' </summary>
        Public Property IsBlank As Boolean = False

        ''' <summary>True if this element has been explicitly set in the theme.</summary>
        Public Property IsSet As Boolean = False

        Public MustOverride Function Clone() As Object Implements ICloneable.Clone

        ''' <summary>
        ''' Merges unset properties from a parent element into this element.
        ''' Only properties that are Nothing (unset) in this element are filled
        ''' from the parent. This implements property-level inheritance.
        ''' </summary>
        Public MustOverride Sub MergeFrom(parent As ThemeElement)

        ''' <summary>Returns the CSS rule body (properties) for this element.</summary>
        Public MustOverride Function ToCssBody() As String

        ''' <summary>
        ''' Sets a property by name from a CSS value string.
        ''' Used by the CSS parser.
        ''' </summary>
        Public MustOverride Sub SetPropertyFromCss(propName As String, value As String)
    End Class

    '==========================================================================
    ' ElementLine
    '==========================================================================
    ''' <summary>
    ''' Line element, equivalent to ggplot2's element_line().
    ''' Properties: colour, linewidth, linetype, lineend, arrow.
    ''' Inherits from the 'line' root element.
    ''' </summary>
    <Serializable>
    Public Class ElementLine
        Inherits ThemeElement

        Private _colour As Color? = Nothing
        Private _linewidth As Unit = Nothing
        Private _linetype As String = Nothing
        Private _lineend As String = Nothing
        Private _arrow As ArrowSpec = Nothing

        ''' <summary>Line color. Nothing=inherit, Color.Empty=NA(transparent), other=specific color.</summary>
        Public Property Colour As Color?
            Get
                Return _colour
            End Get
            Set(value As Color?)
                _colour = value
            End Set
        End Property

        ''' <summary>Line width as a Unit. Nothing=inherit.</summary>
        Public Property Linewidth As Unit
            Get
                Return _linewidth
            End Get
            Set(value As Unit)
                _linewidth = value
            End Set
        End Property

        ''' <summary>Line type: "solid", "dashed", "dotted", "dotdash", "longdash", "twodash", or "blank".</summary>
        Public Property Linetype As String
            Get
                Return _linetype
            End Get
            Set(value As String)
                _linetype = value
            End Set
        End Property

        ''' <summary>Line end cap style: "butt", "round", or "square".</summary>
        Public Property Lineend As String
            Get
                Return _lineend
            End Get
            Set(value As String)
                _lineend = value
            End Set
        End Property

        ''' <summary>Arrow specification. Nothing=no arrow.</summary>
        Public Property Arrow As ArrowSpec
            Get
                Return _arrow
            End Get
            Set(value As ArrowSpec)
                _arrow = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementLine()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._colour = Me._colour
            c._linewidth = If(Me._linewidth?.Clone(), Nothing)
            c._linetype = Me._linetype
            c._lineend = Me._lineend
            c._arrow = If(Me._arrow?.Clone(), Nothing)
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementLine = TryCast(parent, ElementLine)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Not Me._colour.HasValue AndAlso p._colour.HasValue Then Me._colour = p._colour
            If Me._linewidth Is Nothing AndAlso p._linewidth IsNot Nothing Then
                Me._linewidth = CType(p._linewidth.Clone(), Unit)
            ElseIf Me._linewidth IsNot Nothing AndAlso Me._linewidth.IsRelative() AndAlso p._linewidth IsNot Nothing Then
                Me._linewidth = Me._linewidth.ResolveRelative(p._linewidth)
            End If
            If Me._linetype Is Nothing Then Me._linetype = p._linetype
            If Me._lineend Is Nothing Then Me._lineend = p._lineend
            If Me._arrow Is Nothing AndAlso p._arrow IsNot Nothing Then
                Me._arrow = CType(p._arrow.Clone(), ArrowSpec)
            End If
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _colour.HasValue Then
                sb.Append("    color: " & ColorToCss(_colour.Value) & ";" & vbCrLf)
            End If
            If _linewidth IsNot Nothing Then
                sb.Append("    linewidth: " & _linewidth.ToString() & ";" & vbCrLf)
            End If
            If _linetype IsNot Nothing Then
                sb.Append("    linetype: " & _linetype & ";" & vbCrLf)
            End If
            If _lineend IsNot Nothing Then
                sb.Append("    lineend: " & _lineend & ";" & vbCrLf)
            End If
            If _arrow IsNot Nothing Then
                sb.Append("    arrow: " & _arrow.ToString() & ";" & vbCrLf)
            End If
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "colour", "color"
                    _colour = ParseColor(value)
                Case "linewidth", "size"
                    _linewidth = Unit.Parse(value)
                Case "linetype"
                    _linetype = value.Trim().Trim(""""c, "'"c)
                Case "lineend"
                    _lineend = value.Trim().Trim(""""c, "'"c)
                Case "arrow"
                    ' Arrow parsing is complex; store as-is for now
                    If value.Trim().ToLowerInvariant() = "none" OrElse value.Trim().ToLowerInvariant() = "null" Then
                        _arrow = Nothing
                    End If
            End Select
        End Sub
    End Class

    '==========================================================================
    ' ElementRect
    '==========================================================================
    ''' <summary>
    ''' Rectangle element, equivalent to ggplot2's element_rect().
    ''' Properties: fill, colour, linewidth, linetype.
    ''' Inherits from the 'rect' root element.
    ''' </summary>
    <Serializable>
    Public Class ElementRect
        Inherits ThemeElement

        Private _fill As Color? = Nothing
        Private _colour As Color? = Nothing
        Private _linewidth As Unit = Nothing
        Private _linetype As String = Nothing

        ''' <summary>Fill color. Nothing=inherit, Color.Empty=NA(transparent), other=specific color.</summary>
        Public Property Fill As Color?
            Get
                Return _fill
            End Get
            Set(value As Color?)
                _fill = value
            End Set
        End Property

        ''' <summary>Border color. Nothing=inherit, Color.Empty=NA(no border), other=specific color.</summary>
        Public Property Colour As Color?
            Get
                Return _colour
            End Get
            Set(value As Color?)
                _colour = value
            End Set
        End Property

        ''' <summary>Border width as a Unit. Nothing=inherit.</summary>
        Public Property Linewidth As Unit
            Get
                Return _linewidth
            End Get
            Set(value As Unit)
                _linewidth = value
            End Set
        End Property

        ''' <summary>Border line type.</summary>
        Public Property Linetype As String
            Get
                Return _linetype
            End Get
            Set(value As String)
                _linetype = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementRect()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._fill = Me._fill
            c._colour = Me._colour
            c._linewidth = If(Me._linewidth?.Clone(), Nothing)
            c._linetype = Me._linetype
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementRect = TryCast(parent, ElementRect)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Not Me._fill.HasValue AndAlso p._fill.HasValue Then Me._fill = p._fill
            If Not Me._colour.HasValue AndAlso p._colour.HasValue Then Me._colour = p._colour
            If Me._linewidth Is Nothing AndAlso p._linewidth IsNot Nothing Then
                Me._linewidth = CType(p._linewidth.Clone(), Unit)
            ElseIf Me._linewidth IsNot Nothing AndAlso Me._linewidth.IsRelative() AndAlso p._linewidth IsNot Nothing Then
                Me._linewidth = Me._linewidth.ResolveRelative(p._linewidth)
            End If
            If Me._linetype Is Nothing Then Me._linetype = p._linetype
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _fill.HasValue Then
                sb.Append("    fill: " & ColorToCss(_fill.Value) & ";" & vbCrLf)
            End If
            If _colour.HasValue Then
                sb.Append("    color: " & ColorToCss(_colour.Value) & ";" & vbCrLf)
            End If
            If _linewidth IsNot Nothing Then
                sb.Append("    linewidth: " & _linewidth.ToString() & ";" & vbCrLf)
            End If
            If _linetype IsNot Nothing Then
                sb.Append("    linetype: " & _linetype & ";" & vbCrLf)
            End If
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "fill"
                    _fill = ParseColor(value)
                Case "colour", "color"
                    _colour = ParseColor(value)
                Case "linewidth", "size"
                    _linewidth = Unit.Parse(value)
                Case "linetype"
                    _linetype = value.Trim().Trim(""""c, "'"c)
            End Select
        End Sub
    End Class

    '==========================================================================
    ' ElementText
    '==========================================================================
    ''' <summary>
    ''' Text element, equivalent to ggplot2's element_text().
    ''' Properties: family, face, size, colour, hjust, vjust, angle, lineheight, margin.
    ''' Inherits from the 'text' root element (via 'title' for title elements).
    ''' </summary>
    <Serializable>
    Public Class ElementText
        Inherits ThemeElement

        Private _family As String = Nothing
        Private _face As String = Nothing
        Private _size As Unit = Nothing
        Private _colour As Color? = Nothing
        Private _hjust As Double? = Nothing
        Private _vjust As Double? = Nothing
        Private _angle As Double? = Nothing
        Private _lineheight As Double? = Nothing
        Private _margin As Margin = Nothing

        ''' <summary>Font family name. Nothing=inherit.</summary>
        Public Property Family As String
            Get
                Return _family
            End Get
            Set(value As String)
                _family = value
            End Set
        End Property

        ''' <summary>Font face: "plain", "bold", "italic", or "bold.italic".</summary>
        Public Property Face As String
            Get
                Return _face
            End Get
            Set(value As String)
                _face = value
            End Set
        End Property

        ''' <summary>Font size as a Unit (typically points). Nothing=inherit.</summary>
        Public Property Size As Unit
            Get
                Return _size
            End Get
            Set(value As Unit)
                _size = value
            End Set
        End Property

        ''' <summary>Text color. Nothing=inherit.</summary>
        Public Property Colour As Color?
            Get
                Return _colour
            End Get
            Set(value As Color?)
                _colour = value
            End Set
        End Property

        ''' <summary>Horizontal justification (0=left, 0.5=center, 1=right). Nothing=inherit.</summary>
        Public Property Hjust As Double?
            Get
                Return _hjust
            End Get
            Set(value As Double?)
                _hjust = value
            End Set
        End Property

        ''' <summary>Vertical justification (0=bottom, 0.5=middle, 1=top). Nothing=inherit.</summary>
        Public Property Vjust As Double?
            Get
                Return _vjust
            End Get
            Set(value As Double?)
                _vjust = value
            End Set
        End Property

        ''' <summary>Text angle in degrees. Nothing=inherit.</summary>
        Public Property Angle As Double?
            Get
                Return _angle
            End Get
            Set(value As Double?)
                _angle = value
            End Set
        End Property

        ''' <summary>Line height multiplier. Nothing=inherit.</summary>
        Public Property Lineheight As Double?
            Get
                Return _lineheight
            End Get
            Set(value As Double?)
                _lineheight = value
            End Set
        End Property

        ''' <summary>Margin around text. Nothing=inherit.</summary>
        Public Property Margin As Margin
            Get
                Return _margin
            End Get
            Set(value As Margin)
                _margin = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementText()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._family = Me._family
            c._face = Me._face
            c._size = If(Me._size?.Clone(), Nothing)
            c._colour = Me._colour
            c._hjust = Me._hjust
            c._vjust = Me._vjust
            c._angle = Me._angle
            c._lineheight = Me._lineheight
            c._margin = If(Me._margin?.Clone(), Nothing)
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementText = TryCast(parent, ElementText)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Me._family Is Nothing Then Me._family = p._family
            If Me._face Is Nothing Then Me._face = p._face
            If Me._size Is Nothing AndAlso p._size IsNot Nothing Then
                Me._size = CType(p._size.Clone(), Unit)
            ElseIf Me._size IsNot Nothing AndAlso Me._size.IsRelative() AndAlso p._size IsNot Nothing Then
                Me._size = Me._size.ResolveRelative(p._size)
            End If
            If Not Me._colour.HasValue AndAlso p._colour.HasValue Then Me._colour = p._colour
            If Not Me._hjust.HasValue AndAlso p._hjust.HasValue Then Me._hjust = p._hjust
            If Not Me._vjust.HasValue AndAlso p._vjust.HasValue Then Me._vjust = p._vjust
            If Not Me._angle.HasValue AndAlso p._angle.HasValue Then Me._angle = p._angle
            If Not Me._lineheight.HasValue AndAlso p._lineheight.HasValue Then Me._lineheight = p._lineheight
            If Me._margin Is Nothing AndAlso p._margin IsNot Nothing Then
                Me._margin = CType(p._margin.Clone(), Margin)
            End If
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _family IsNot Nothing Then
                sb.Append("    family: " & QuoteString(_family) & ";" & vbCrLf)
            End If
            If _face IsNot Nothing Then
                sb.Append("    face: " & _face & ";" & vbCrLf)
            End If
            If _size IsNot Nothing Then
                sb.Append("    size: " & _size.ToString() & ";" & vbCrLf)
            End If
            If _colour.HasValue Then
                sb.Append("    color: " & ColorToCss(_colour.Value) & ";" & vbCrLf)
            End If
            If _hjust.HasValue Then
                sb.Append("    hjust: " & _hjust.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            End If
            If _vjust.HasValue Then
                sb.Append("    vjust: " & _vjust.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            End If
            If _angle.HasValue Then
                sb.Append("    angle: " & _angle.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            End If
            If _lineheight.HasValue Then
                sb.Append("    lineheight: " & _lineheight.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            End If
            If _margin IsNot Nothing Then
                sb.Append("    margin: " & _margin.ToString() & ";" & vbCrLf)
            End If
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "family"
                    _family = UnquoteString(value)
                Case "face"
                    _face = UnquoteString(value)
                Case "size"
                    _size = Unit.Parse(value)
                Case "colour", "color"
                    _colour = ParseColor(value)
                Case "hjust"
                    _hjust = ParseDouble(value)
                Case "vjust"
                    _vjust = ParseDouble(value)
                Case "angle"
                    _angle = ParseDouble(value)
                Case "lineheight"
                    _lineheight = ParseDouble(value)
                Case "margin"
                    _margin = Margin.Parse(value)
            End Select
        End Sub
    End Class

    '==========================================================================
    ' ElementPoint
    '==========================================================================
    ''' <summary>
    ''' Point element, equivalent to ggplot2's element_point().
    ''' Inherits from the 'point' root element.
    ''' </summary>
    <Serializable>
    Public Class ElementPoint
        Inherits ThemeElement

        Private _colour As Color? = Nothing
        Private _fill As Color? = Nothing
        Private _size As Double? = Nothing
        Private _shape As Integer? = Nothing
        Private _stroke As Double? = Nothing

        Public Property Colour As Color?
            Get
                Return _colour
            End Get
            Set(value As Color?)
                _colour = value
            End Set
        End Property

        Public Property Fill As Color?
            Get
                Return _fill
            End Get
            Set(value As Color?)
                _fill = value
            End Set
        End Property

        Public Property Size As Double?
            Get
                Return _size
            End Get
            Set(value As Double?)
                _size = value
            End Set
        End Property

        Public Property Shape As Integer?
            Get
                Return _shape
            End Get
            Set(value As Integer?)
                _shape = value
            End Set
        End Property

        Public Property Stroke As Double?
            Get
                Return _stroke
            End Get
            Set(value As Double?)
                _stroke = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementPoint()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._colour = Me._colour
            c._fill = Me._fill
            c._size = Me._size
            c._shape = Me._shape
            c._stroke = Me._stroke
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementPoint = TryCast(parent, ElementPoint)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Not Me._colour.HasValue AndAlso p._colour.HasValue Then Me._colour = p._colour
            If Not Me._fill.HasValue AndAlso p._fill.HasValue Then Me._fill = p._fill
            If Not Me._size.HasValue AndAlso p._size.HasValue Then Me._size = p._size
            If Not Me._shape.HasValue AndAlso p._shape.HasValue Then Me._shape = p._shape
            If Not Me._stroke.HasValue AndAlso p._stroke.HasValue Then Me._stroke = p._stroke
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _colour.HasValue Then sb.Append("    color: " & ColorToCss(_colour.Value) & ";" & vbCrLf)
            If _fill.HasValue Then sb.Append("    fill: " & ColorToCss(_fill.Value) & ";" & vbCrLf)
            If _size.HasValue Then sb.Append("    size: " & _size.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            If _shape.HasValue Then sb.Append("    shape: " & _shape.Value.ToString() & ";" & vbCrLf)
            If _stroke.HasValue Then sb.Append("    stroke: " & _stroke.Value.ToString("G", CultureInfo.InvariantCulture) & ";" & vbCrLf)
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "colour", "color"
                    _colour = ParseColor(value)
                Case "fill"
                    _fill = ParseColor(value)
                Case "size"
                    _size = ParseDouble(value)
                Case "shape"
                    _shape = Integer.Parse(value.Trim(), CultureInfo.InvariantCulture)
                Case "stroke"
                    _stroke = ParseDouble(value)
            End Select
        End Sub
    End Class

    '==========================================================================
    ' ElementPolygon
    '==========================================================================
    ''' <summary>
    ''' Polygon element, equivalent to ggplot2's element_polygon().
    ''' Inherits from the 'polygon' root element.
    ''' </summary>
    <Serializable>
    Public Class ElementPolygon
        Inherits ThemeElement

        Private _fill As Color? = Nothing
        Private _colour As Color? = Nothing
        Private _linewidth As Unit = Nothing
        Private _linetype As String = Nothing
        Private _group As Integer? = Nothing

        Public Property Fill As Color?
            Get
                Return _fill
            End Get
            Set(value As Color?)
                _fill = value
            End Set
        End Property

        Public Property Colour As Color?
            Get
                Return _colour
            End Get
            Set(value As Color?)
                _colour = value
            End Set
        End Property

        Public Property Linewidth As Unit
            Get
                Return _linewidth
            End Get
            Set(value As Unit)
                _linewidth = value
            End Set
        End Property

        Public Property Linetype As String
            Get
                Return _linetype
            End Get
            Set(value As String)
                _linetype = value
            End Set
        End Property

        Public Property Group As Integer?
            Get
                Return _group
            End Get
            Set(value As Integer?)
                _group = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementPolygon()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._fill = Me._fill
            c._colour = Me._colour
            c._linewidth = If(Me._linewidth?.Clone(), Nothing)
            c._linetype = Me._linetype
            c._group = Me._group
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementPolygon = TryCast(parent, ElementPolygon)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Not Me._fill.HasValue AndAlso p._fill.HasValue Then Me._fill = p._fill
            If Not Me._colour.HasValue AndAlso p._colour.HasValue Then Me._colour = p._colour
            If Me._linewidth Is Nothing AndAlso p._linewidth IsNot Nothing Then
                Me._linewidth = CType(p._linewidth.Clone(), Unit)
            End If
            If Me._linetype Is Nothing Then Me._linetype = p._linetype
            If Not Me._group.HasValue AndAlso p._group.HasValue Then Me._group = p._group
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _fill.HasValue Then sb.Append("    fill: " & ColorToCss(_fill.Value) & ";" & vbCrLf)
            If _colour.HasValue Then sb.Append("    color: " & ColorToCss(_colour.Value) & ";" & vbCrLf)
            If _linewidth IsNot Nothing Then sb.Append("    linewidth: " & _linewidth.ToString() & ";" & vbCrLf)
            If _linetype IsNot Nothing Then sb.Append("    linetype: " & _linetype & ";" & vbCrLf)
            If _group.HasValue Then sb.Append("    group: " & _group.Value.ToString() & ";" & vbCrLf)
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "fill"
                    _fill = ParseColor(value)
                Case "colour", "color"
                    _colour = ParseColor(value)
                Case "linewidth", "size"
                    _linewidth = Unit.Parse(value)
                Case "linetype"
                    _linetype = UnquoteString(value)
                Case "group"
                    _group = Integer.Parse(value.Trim(), CultureInfo.InvariantCulture)
            End Select
        End Sub
    End Class

    '==========================================================================
    ' ElementGeom
    '==========================================================================
    ''' <summary>
    ''' Geom defaults element, equivalent to ggplot2's element_geom().
    ''' Defines default colors and palettes for geometric objects.
    ''' Inherits from the 'geom' root element.
    ''' </summary>
    <Serializable>
    Public Class ElementGeom
        Inherits ThemeElement

        Private _ink As Color? = Nothing
        Private _accent As Color? = Nothing
        Private _paper As Color? = Nothing
        Private _paletteColourContinuous As New List(Of Color)()
        Private _paletteColourDiscrete As New List(Of Color)()
        Private _paletteFillContinuous As New List(Of Color)()
        Private _paletteFillDiscrete As New List(Of Color)()

        ''' <summary>Default ink (primary) color for geoms.</summary>
        Public Property Ink As Color?
            Get
                Return _ink
            End Get
            Set(value As Color?)
                _ink = value
            End Set
        End Property

        ''' <summary>Accent color for geoms.</summary>
        Public Property Accent As Color?
            Get
                Return _accent
            End Get
            Set(value As Color?)
                _accent = value
            End Set
        End Property

        ''' <summary>Paper (background) color for geoms.</summary>
        Public Property Paper As Color?
            Get
                Return _paper
            End Get
            Set(value As Color?)
                _paper = value
            End Set
        End Property

        Public Property PaletteColourContinuous As List(Of Color)
            Get
                Return _paletteColourContinuous
            End Get
            Set(value As List(Of Color))
                _paletteColourContinuous = value
            End Set
        End Property

        Public Property PaletteColourDiscrete As List(Of Color)
            Get
                Return _paletteColourDiscrete
            End Get
            Set(value As List(Of Color))
                _paletteColourDiscrete = value
            End Set
        End Property

        Public Property PaletteFillContinuous As List(Of Color)
            Get
                Return _paletteFillContinuous
            End Get
            Set(value As List(Of Color))
                _paletteFillContinuous = value
            End Set
        End Property

        Public Property PaletteFillDiscrete As List(Of Color)
            Get
                Return _paletteFillDiscrete
            End Get
            Set(value As List(Of Color))
                _paletteFillDiscrete = value
            End Set
        End Property

        Public Overrides Function Clone() As Object
            Dim c As New ElementGeom()
            c.IsBlank = Me.IsBlank
            c.IsSet = Me.IsSet
            c._ink = Me._ink
            c._accent = Me._accent
            c._paper = Me._paper
            c._paletteColourContinuous = New List(Of Color)(Me._paletteColourContinuous)
            c._paletteColourDiscrete = New List(Of Color)(Me._paletteColourDiscrete)
            c._paletteFillContinuous = New List(Of Color)(Me._paletteFillContinuous)
            c._paletteFillDiscrete = New List(Of Color)(Me._paletteFillDiscrete)
            Return c
        End Function

        Public Overrides Sub MergeFrom(parent As ThemeElement)
            If parent Is Nothing Then Return
            Dim p As ElementGeom = TryCast(parent, ElementGeom)
            If p Is Nothing Then Return
            If p.IsBlank Then Me.IsBlank = True
            If Not Me._ink.HasValue AndAlso p._ink.HasValue Then Me._ink = p._ink
            If Not Me._accent.HasValue AndAlso p._accent.HasValue Then Me._accent = p._accent
            If Not Me._paper.HasValue AndAlso p._paper.HasValue Then Me._paper = p._paper
            If Me._paletteColourContinuous.Count = 0 Then Me._paletteColourContinuous = New List(Of Color)(p._paletteColourContinuous)
            If Me._paletteColourDiscrete.Count = 0 Then Me._paletteColourDiscrete = New List(Of Color)(p._paletteColourDiscrete)
            If Me._paletteFillContinuous.Count = 0 Then Me._paletteFillContinuous = New List(Of Color)(p._paletteFillContinuous)
            If Me._paletteFillDiscrete.Count = 0 Then Me._paletteFillDiscrete = New List(Of Color)(p._paletteFillDiscrete)
        End Sub

        Public Overrides Function ToCssBody() As String
            If IsBlank Then Return "    blank: true;" & vbCrLf
            Dim sb As New StringBuilder()
            If _ink.HasValue Then sb.Append("    ink: " & ColorToCss(_ink.Value) & ";" & vbCrLf)
            If _accent.HasValue Then sb.Append("    accent: " & ColorToCss(_accent.Value) & ";" & vbCrLf)
            If _paper.HasValue Then sb.Append("    paper: " & ColorToCss(_paper.Value) & ";" & vbCrLf)
            If _paletteColourContinuous.Count > 0 Then
                sb.Append("    palette.colour.continuous: " & ColorsToCss(_paletteColourContinuous) & ";" & vbCrLf)
            End If
            If _paletteColourDiscrete.Count > 0 Then
                sb.Append("    palette.colour.discrete: " & ColorsToCss(_paletteColourDiscrete) & ";" & vbCrLf)
            End If
            If _paletteFillContinuous.Count > 0 Then
                sb.Append("    palette.fill.continuous: " & ColorsToCss(_paletteFillContinuous) & ";" & vbCrLf)
            End If
            If _paletteFillDiscrete.Count > 0 Then
                sb.Append("    palette.fill.discrete: " & ColorsToCss(_paletteFillDiscrete) & ";" & vbCrLf)
            End If
            Return sb.ToString()
        End Function

        Public Overrides Sub SetPropertyFromCss(propName As String, value As String)
            Select Case propName.ToLowerInvariant()
                Case "blank"
                    IsBlank = ParseBool(value)
                Case "ink"
                    _ink = ParseColor(value)
                Case "accent"
                    _accent = ParseColor(value)
                Case "paper"
                    _paper = ParseColor(value)
                Case "palette.colour.continuous", "palette.color.continuous"
                    _paletteColourContinuous = ParseColorList(value)
                Case "palette.colour.discrete", "palette.color.discrete"
                    _paletteColourDiscrete = ParseColorList(value)
                Case "palette.fill.continuous"
                    _paletteFillContinuous = ParseColorList(value)
                Case "palette.fill.discrete"
                    _paletteFillDiscrete = ParseColorList(value)
            End Select
        End Sub
    End Class

    '==========================================================================
    ' Helper Functions for Color and Value Parsing
    '==========================================================================
    Public Module ElementHelpers

        ''' <summary>Converts a Color to CSS string representation.</summary>
        Public Function ColorToCss(c As Color) As String
            If c.IsEmpty Then Return "NA"
            If c.IsNamedColor Then Return c.Name
            If c.A < 255 Then
                Return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}"
            End If
            Return $"#{c.R:X2}{c.G:X2}{c.B:X2}"
        End Function

        ''' <summary>Parses a color from CSS string. Supports #RGB, #RRGGBB, #AARRGGBB, named colors, and "NA".</summary>
        Public Function ParseColor(s As String) As Color?
            If String.IsNullOrWhiteSpace(s) Then Return Nothing
            s = s.Trim()
            If s.Equals("NA", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("null", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("none", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("transparent", StringComparison.OrdinalIgnoreCase) Then
                Return Color.Empty
            End If
            If s.Equals("inherit", StringComparison.OrdinalIgnoreCase) Then Return Nothing
            Try
                Return ColorTranslator.FromHtml(s)
            Catch
                Return Color.Empty
            End Try
        End Function

        ''' <summary>Parses a list of colors from a space or comma-separated string.</summary>
        Public Function ParseColorList(s As String) As List(Of Color)
            Dim result As New List(Of Color)()
            If String.IsNullOrWhiteSpace(s) Then Return result
            ' Remove surrounding brackets if present
            s = s.Trim().TrimStart("{"c, "("c).TrimEnd("}"c, ")"c)
            ' Split by comma, but handle hex colors carefully
            Dim parts() As String = s.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
            For Each part As String In parts
                part = part.Trim().Trim(""""c, "'"c)
                Dim c? As Color = ParseColor(part)
                If c.HasValue Then result.Add(c.Value)
            Next
            Return result
        End Function

        ''' <summary>Converts a list of colors to CSS string.</summary>
        Public Function ColorsToCss(colors As List(Of Color)) As String
            Dim parts As New List(Of String)()
            For Each c As Color In colors
                parts.Add(ColorToCss(c))
            Next
            Return String.Join(", ", parts)
        End Function

        ''' <summary>Parses a double from a string.</summary>
        Public Function ParseDouble(s As String) As Double?
            If String.IsNullOrWhiteSpace(s) Then Return Nothing
            s = s.Trim()
            If s.Equals("NA", StringComparison.OrdinalIgnoreCase) OrElse
               s.Equals("null", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If
            Dim result As Double
            If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, result) Then
                Return result
            End If
            Return Nothing
        End Function

        ''' <summary>Parses a boolean from a string.</summary>
        Public Function ParseBool(s As String) As Boolean
            If String.IsNullOrWhiteSpace(s) Then Return False
            s = s.Trim().ToLowerInvariant()
            Return s = "true" OrElse s = "1" OrElse s = "yes" OrElse s = "on"
        End Function

        ''' <summary>Quotes a string for CSS output.</summary>
        Public Function QuoteString(s As String) As String
            If s Is Nothing Then Return """"""
            Return """" & s.Replace("""", "\""") & """"
        End Function

        ''' <summary>Removes quotes from a string.</summary>
        Public Function UnquoteString(s As String) As String
            If s Is Nothing Then Return Nothing
            s = s.Trim()
            If s.Length >= 2 Then
                If (s(0) = """"c AndAlso s(s.Length - 1) = """"c) OrElse
                   (s(0) = "'"c AndAlso s(s.Length - 1) = "'"c) Then
                    Return s.Substring(1, s.Length - 2).Replace("\""", """")
                End If
            End If
            Return s
        End Function
    End Module

End Namespace

