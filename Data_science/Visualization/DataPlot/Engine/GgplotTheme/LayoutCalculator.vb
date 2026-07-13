#Region "Microsoft.VisualBasic::e976885da846d6cbf70a635b80d74eb9, Data_science\Visualization\DataPlot\Engine\GgplotTheme\LayoutCalculator.vb"

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

    '   Total Lines: 637
    '    Code Lines: 375 (58.87%)
    ' Comment Lines: 173 (27.16%)
    '    - Xml Docs: 63.58%
    ' 
    '   Blank Lines: 89 (13.97%)
    '     File Size: 30.21 KB


    '     Class ResolvedElementText
    ' 
    '         Properties: Angle, Colour, Face, Family, Hjust
    '                     IsBlank, Lineheight, MarginBottomPx, MarginLeftPx, MarginRightPx
    '                     MarginTopPx, SizePx, Vjust
    ' 
    '         Function: ToFont
    ' 
    '     Class ResolvedElementLine
    ' 
    '         Properties: Colour, IsBlank, Lineend, Linetype, LinewidthPx
    ' 
    '         Function: ToPen
    ' 
    '     Class ResolvedElementRect
    ' 
    '         Properties: Colour, Fill, IsBlank, Linetype, LinewidthPx
    ' 
    '         Function: ToBorderPen, ToFillBrush
    ' 
    '     Class PlotLayout
    ' 
    '         Properties: CanvasRect, LegendKeySizePx, LegendSpacingPx, MarginBottomPx, MarginLeftPx
    '                     MarginRightPx, MarginTopPx, PanelAreaRect, PanelSpacingXPx, PanelSpacingYPx
    '                     PlotAreaRect, TickLengthXBottomPx, TickLengthXTopPx, TickLengthYLeftPx, TickLengthYRightPx
    ' 
    '     Class LayoutCalculator
    ' 
    '         Properties: Canvas, Converter, Theme
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputeLegendPosition, ComputePlotLayout, DumpLayout, GetAspectRatio, GetFontSizePx
    '                   GetLegendKeySizePx, GetLegendSpacingPx, GetLineWidthPx, GetPanelSpacingPx, GetTickLengthPx
    '                   ResolveLine, ResolveMarginToPixels, ResolveRect, ResolveText, ResolveUnitToPixels
    '                   ResolveUnitToPixelsX, ResolveUnitToPixelsY
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.Imaging

Namespace GgplotTheme

    '==========================================================================
    ' ResolvedElementText - Text element with all values resolved to pixels
    '==========================================================================
    ''' <summary>
    ''' A text element with all properties resolved to pixel values.
    ''' Produced by LayoutCalculator for rendering.
    ''' </summary>
    Public Class ResolvedElementText
        Public Property Family As String = "sans-serif"
        Public Property Face As String = "plain"
        Public Property SizePx As Double = 11.0
        Public Property Colour As Color = Color.Black
        Public Property Hjust As Double = 0.5
        Public Property Vjust As Double = 0.5
        Public Property Angle As Double = 0.0
        Public Property Lineheight As Double = 1.2
        Public Property MarginTopPx As Double = 0.0
        Public Property MarginRightPx As Double = 0.0
        Public Property MarginBottomPx As Double = 0.0
        Public Property MarginLeftPx As Double = 0.0
        Public Property IsBlank As Boolean = False

        ''' <summary>Creates a System.Drawing.Font from the resolved properties.</summary>
        Public Function ToFont() As Font
            Dim fontStyle As FontStyle = FontStyle.Regular
            Select Case Face.ToLowerInvariant()
                Case "bold"
                    fontStyle = FontStyle.Bold
                Case "italic"
                    fontStyle = FontStyle.Italic
                Case "bold.italic"
                    fontStyle = FontStyle.Bold Or FontStyle.Italic
            End Select
            Dim sizePt As Single = CSng(SizePx * 72.0F / 96.0F)
            If sizePt < 1 Then sizePt = 1
            Return New Font(Family, sizePt, fontStyle, GraphicsUnit.Point)
        End Function
    End Class

    '==========================================================================
    ' ResolvedElementLine - Line element with all values resolved to pixels
    '==========================================================================
    ''' <summary>
    ''' A line element with all properties resolved to pixel values.
    ''' </summary>
    Public Class ResolvedElementLine
        Public Property Colour As Color = Color.Black
        Public Property LinewidthPx As Double = 0.5
        Public Property Linetype As String = "solid"
        Public Property Lineend As String = "butt"
        Public Property IsBlank As Boolean = False

        ''' <summary>Creates a System.Drawing.Pen from the resolved properties.</summary>
        Public Function ToPen() As Pen
            Dim pen As New Pen(Colour, CSng(LinewidthPx))
            Select Case Lineend.ToLowerInvariant()
                Case "round"
                    pen.EndCap = LineCap.Round
                    pen.StartCap = LineCap.Round
                Case "square"
                    pen.EndCap = LineCap.Square
                    pen.StartCap = LineCap.Square
                Case Else
                    pen.EndCap = LineCap.Flat
                    pen.StartCap = LineCap.Flat
            End Select
            ' Apply dash style
            Select Case Linetype.ToLowerInvariant()
                Case "dashed", "22"
                    pen.DashStyle = DashStyle.Dash
                Case "dotted", "13"
                    pen.DashStyle = DashStyle.Dot
                Case "dotdash", "1343"
                    pen.DashStyle = DashStyle.DashDot
                Case "longdash", "44"
                    pen.DashStyle = DashStyle.Dash
                Case "twodash", "2822"
                    pen.DashStyle = DashStyle.DashDot
                Case "blank", "0"
                    pen.Color = Color.Transparent
            End Select
            Return pen
        End Function
    End Class

    '==========================================================================
    ' ResolvedElementRect - Rect element with all values resolved to pixels
    '==========================================================================
    ''' <summary>
    ''' A rectangle element with all properties resolved to pixel values.
    ''' </summary>
    Public Class ResolvedElementRect
        Public Property Fill As Color = Color.White
        Public Property Colour As Color = Color.Black
        Public Property LinewidthPx As Double = 0.5
        Public Property Linetype As String = "solid"
        Public Property IsBlank As Boolean = False

        ''' <summary>Creates a System.Drawing.Brush for the fill.</summary>
        Public Function ToFillBrush() As Brush
            Return New SolidBrush(Fill)
        End Function

        ''' <summary>Creates a System.Drawing.Pen for the border.</summary>
        Public Function ToBorderPen() As Pen
            Dim pen As New Pen(Colour, CSng(LinewidthPx))
            Return pen
        End Function
    End Class

    '==========================================================================
    ' PlotLayout - Computed layout positions for the plot
    '==========================================================================
    ''' <summary>
    ''' Contains computed pixel positions for the main plot regions.
    ''' All values are in pixels relative to the canvas origin (top-left).
    ''' </summary>
    Public Class PlotLayout
        ''' <summary>The full canvas rectangle.</summary>
        Public Property CanvasRect As RectangleF

        ''' <summary>The plot area (inside margins, where panels are drawn).</summary>
        Public Property PlotAreaRect As RectangleF

        ''' <summary>The panel area (inside plot area, where data is plotted).</summary>
        Public Property PanelAreaRect As RectangleF

        ''' <summary>Top margin in pixels.</summary>
        Public Property MarginTopPx As Double

        ''' <summary>Right margin in pixels.</summary>
        Public Property MarginRightPx As Double

        ''' <summary>Bottom margin in pixels.</summary>
        Public Property MarginBottomPx As Double

        ''' <summary>Left margin in pixels.</summary>
        Public Property MarginLeftPx As Double

        ''' <summary>Tick length for x-axis (bottom) in pixels.</summary>
        Public Property TickLengthXBottomPx As Double

        ''' <summary>Tick length for x-axis (top) in pixels.</summary>
        Public Property TickLengthXTopPx As Double

        ''' <summary>Tick length for y-axis (left) in pixels.</summary>
        Public Property TickLengthYLeftPx As Double

        ''' <summary>Tick length for y-axis (right) in pixels.</summary>
        Public Property TickLengthYRightPx As Double

        ''' <summary>Panel spacing in x direction (pixels).</summary>
        Public Property PanelSpacingXPx As Double

        ''' <summary>Panel spacing in y direction (pixels).</summary>
        Public Property PanelSpacingYPx As Double

        ''' <summary>Legend key size in pixels.</summary>
        Public Property LegendKeySizePx As Double

        ''' <summary>Legend spacing in pixels.</summary>
        Public Property LegendSpacingPx As Double
    End Class

    '==========================================================================
    ' LayoutCalculator - Converts theme units to pixel values
    '==========================================================================
    ''' <summary>
    ''' The automatic layout calculation system. Converts all theme units
    ''' (cm, mm, pt, lines, char, null, npc, inch, px) to pixel values
    ''' based on the canvas context (canvas size, PPI).
    '''
    ''' This class provides methods to:
    ''' - Resolve theme elements with inheritance applied
    ''' - Convert all units to pixels
    ''' - Compute layout positions for plot regions
    ''' - Get pixel values for line widths, font sizes, tick lengths, etc.
    ''' </summary>
    Public Class LayoutCalculator

        Private _theme As Theme
        Private _canvas As CanvasContext
        Private _converter As UnitConverter

        ''' <summary>
        ''' Creates a LayoutCalculator for the given theme and canvas context.
        ''' </summary>
        ''' <param name="theme">The theme to calculate layout for.</param>
        ''' <param name="canvas">The canvas context (dimensions, PPI). If Nothing, uses theme's canvas or a default 800x600@96dpi.</param>
        Public Sub New(theme As Theme, canvas As CanvasContext)
            If theme Is Nothing Then Throw New ArgumentNullException(NameOf(theme))
            _theme = theme
            _canvas = If(canvas, If(theme.Canvas, New CanvasContext(800, 600, 96)))
            _converter = New UnitConverter(_canvas)
        End Sub

        ''' <summary>The canvas context used for calculations.</summary>
        Public ReadOnly Property Canvas As CanvasContext
            Get
                Return _canvas
            End Get
        End Property

        ''' <summary>The unit converter.</summary>
        Public ReadOnly Property Converter As UnitConverter
            Get
                Return _converter
            End Get
        End Property

        ''' <summary>The theme being calculated.</summary>
        Public ReadOnly Property Theme As Theme
            Get
                Return _theme
            End Get
        End Property

        '==================================================================
        ' Resolved element accessors (with inheritance applied, in pixels)
        '==================================================================

        ''' <summary>
        ''' Resolves a text element to pixel values, applying inheritance.
        ''' Returns a ResolvedElementText with all properties filled in.
        ''' </summary>
        Public Function ResolveText(name As String) As ResolvedElementText
            Dim resolved As New ResolvedElementText()

            ' Get the resolved element (with inheritance applied)
            Dim elem As ElementText = TryCast(_theme.ResolveElement(name), ElementText)
            If elem Is Nothing OrElse elem.IsBlank Then
                resolved.IsBlank = True
                Return resolved
            End If

            ' Apply resolved values
            If elem.Family IsNot Nothing Then resolved.Family = elem.Family
            If elem.Face IsNot Nothing Then resolved.Face = elem.Face

            ' Font size: convert to pixels
            If elem.Size IsNot Nothing AndAlso Not elem.Size.IsNull() Then
                resolved.SizePx = _converter.ToPixels(elem.Size)
            End If

            ' Color
            If elem.Colour.HasValue AndAlso Not elem.Colour.Value.IsEmpty Then
                resolved.Colour = elem.Colour.Value
            End If

            ' Justification
            If elem.Hjust.HasValue Then resolved.Hjust = elem.Hjust.Value
            If elem.Vjust.HasValue Then resolved.Vjust = elem.Vjust.Value

            ' Angle
            If elem.Angle.HasValue Then resolved.Angle = elem.Angle.Value

            ' Line height
            If elem.Lineheight.HasValue Then resolved.Lineheight = elem.Lineheight.Value

            ' Margin: convert to pixels
            If elem.Margin IsNot Nothing Then
                Dim margin As (t As Double, r As Double, b As Double, l As Double) = _converter.MarginToPixels(elem.Margin, resolved.SizePx * 72.0 / 96.0)
                resolved.MarginTopPx = margin.t
                resolved.MarginRightPx = margin.r
                resolved.MarginBottomPx = margin.b
                resolved.MarginLeftPx = margin.l
            End If

            resolved.IsBlank = elem.IsBlank
            Return resolved
        End Function

        ''' <summary>
        ''' Resolves a line element to pixel values, applying inheritance.
        ''' </summary>
        Public Function ResolveLine(name As String) As ResolvedElementLine
            Dim resolved As New ResolvedElementLine()

            Dim elem As ElementLine = TryCast(_theme.ResolveElement(name), ElementLine)
            If elem Is Nothing OrElse elem.IsBlank Then
                resolved.IsBlank = True
                Return resolved
            End If

            If elem.Colour.HasValue AndAlso Not elem.Colour.Value.IsEmpty Then
                resolved.Colour = elem.Colour.Value
            End If

            If elem.Linewidth IsNot Nothing AndAlso Not elem.Linewidth.IsNull() Then
                resolved.LinewidthPx = _converter.ToPixels(elem.Linewidth)
            End If

            If elem.Linetype IsNot Nothing Then resolved.Linetype = elem.Linetype
            If elem.Lineend IsNot Nothing Then resolved.Lineend = elem.Lineend
            resolved.IsBlank = elem.IsBlank

            Return resolved
        End Function

        ''' <summary>
        ''' Resolves a rect element to pixel values, applying inheritance.
        ''' </summary>
        Public Function ResolveRect(name As String) As ResolvedElementRect
            Dim resolved As New ResolvedElementRect()

            Dim elem As ElementRect = TryCast(_theme.ResolveElement(name), ElementRect)
            If elem Is Nothing OrElse elem.IsBlank Then
                resolved.IsBlank = True
                Return resolved
            End If

            If elem.Fill.HasValue AndAlso Not elem.Fill.Value.IsEmpty Then
                resolved.Fill = elem.Fill.Value
            End If

            If elem.Colour.HasValue AndAlso Not elem.Colour.Value.IsEmpty Then
                resolved.Colour = elem.Colour.Value
            End If

            If elem.Linewidth IsNot Nothing AndAlso Not elem.Linewidth.IsNull() Then
                resolved.LinewidthPx = _converter.ToPixels(elem.Linewidth)
            End If

            If elem.Linetype IsNot Nothing Then resolved.Linetype = elem.Linetype
            resolved.IsBlank = elem.IsBlank

            Return resolved
        End Function

        '==================================================================
        ' Unit value accessors (in pixels)
        '==================================================================

        ''' <summary>
        ''' Resolves a unit value to pixels, applying inheritance.
        ''' For NPC units, uses the canvas dimensions as the parent.
        ''' </summary>
        Public Function ResolveUnitToPixels(name As String,
                                            Optional fontSizePt As Double = -1,
                                            Optional parentWidthPx As Double = -1,
                                            Optional parentHeightPx As Double = -1) As Double
            Dim u As Unit = _theme.ResolveUnit(name)
            If u Is Nothing OrElse u.IsNull() Then Return 0.0
            Return _converter.ToPixels(u, fontSizePt, parentWidthPx, parentHeightPx)
        End Function

        ''' <summary>
        ''' Resolves a unit value to pixels using width context (for horizontal measurements).
        ''' </summary>
        Public Function ResolveUnitToPixelsX(name As String,
                                             Optional fontSizePt As Double = -1,
                                             Optional parentWidthPx As Double = -1) As Double
            Dim u As Unit = _theme.ResolveUnit(name)
            If u Is Nothing OrElse u.IsNull() Then Return 0.0
            Return _converter.ToPixelsX(u, fontSizePt, parentWidthPx)
        End Function

        ''' <summary>
        ''' Resolves a unit value to pixels using height context (for vertical measurements).
        ''' </summary>
        Public Function ResolveUnitToPixelsY(name As String,
                                             Optional fontSizePt As Double = -1,
                                             Optional parentHeightPx As Double = -1) As Double
            Dim u As Unit = _theme.ResolveUnit(name)
            If u Is Nothing OrElse u.IsNull() Then Return 0.0
            Return _converter.ToPixelsY(u, fontSizePt, parentHeightPx)
        End Function

        ''' <summary>
        ''' Resolves a margin to pixel values (top, right, bottom, left).
        ''' </summary>
        Public Function ResolveMarginToPixels(name As String,
                                              Optional fontSizePt As Double = -1,
                                              Optional parentWidthPx As Double = -1,
                                              Optional parentHeightPx As Double = -1) As (Top As Double, Right As Double, Bottom As Double, Left As Double)
            Dim m As Margin = _theme.ResolveMargin(name)
            If m Is Nothing Then Return (0, 0, 0, 0)
            Return _converter.MarginToPixels(m, fontSizePt, parentWidthPx, parentHeightPx)
        End Function

        '==================================================================
        ' Convenience methods for common layout values
        '==================================================================

        ''' <summary>
        ''' Gets the font size in pixels for a text element.
        ''' </summary>
        Public Function GetFontSizePx(name As String) As Double
            Dim elem As ElementText = TryCast(_theme.ResolveElement(name), ElementText)
            If elem Is Nothing OrElse elem.Size Is Nothing OrElse elem.Size.IsNull() Then
                Return _converter.PtToPx(_canvas.BaseFontSizePt)
            End If
            Return _converter.ToPixels(elem.Size)
        End Function

        ''' <summary>
        ''' Gets the line width in pixels for a line element.
        ''' </summary>
        Public Function GetLineWidthPx(name As String) As Double
            Dim elem As ElementLine = TryCast(_theme.ResolveElement(name), ElementLine)
            If elem Is Nothing OrElse elem.Linewidth Is Nothing OrElse elem.Linewidth.IsNull() Then
                Return 0.5
            End If
            Return _converter.ToPixels(elem.Linewidth)
        End Function

        ''' <summary>
        ''' Gets the tick length in pixels for the specified axis.
        ''' </summary>
        Public Function GetTickLengthPx(axis As String) As Double
            ' axis can be "x.top", "x.bottom", "y.left", "y.right", "theta", "r"
            Dim name As String = "axis.ticks.length." & axis
            If Not InheritanceTree.IsKnownElement(name) Then
                name = "axis.ticks.length"
            End If
            Return ResolveUnitToPixels(name)
        End Function

        ''' <summary>
        ''' Gets the panel spacing in pixels for the specified direction.
        ''' </summary>
        Public Function GetPanelSpacingPx(direction As String) As Double
            ' direction can be "x" or "y"
            Dim name As String = "panel.spacing." & direction
            If Not InheritanceTree.IsKnownElement(name) Then
                name = "panel.spacing"
            End If
            Return ResolveUnitToPixels(name)
        End Function

        ''' <summary>
        ''' Gets the legend key size in pixels.
        ''' </summary>
        Public Function GetLegendKeySizePx() As Double
            Return ResolveUnitToPixels("legend.key.size")
        End Function

        ''' <summary>
        ''' Gets the legend spacing in pixels.
        ''' </summary>
        Public Function GetLegendSpacingPx() As Double
            Return ResolveUnitToPixels("legend.spacing")
        End Function

        '==================================================================
        ' Full plot layout computation
        '==================================================================

        ''' <summary>
        ''' Computes the full plot layout, including canvas rect, plot area
        ''' (inside margins), and panel area. All values are in pixels.
        ''' </summary>
        ''' <param name="canvasWidth">Canvas width in pixels. If <= 0, uses canvas context width.</param>
        ''' <param name="canvasHeight">Canvas height in pixels. If <= 0, uses canvas context height.</param>
        Public Function ComputePlotLayout(Optional canvasWidth As Double = -1,
                                          Optional canvasHeight As Double = -1) As PlotLayout
            Dim layout As New PlotLayout()

            Dim cw As Double = If(canvasWidth > 0, canvasWidth, _canvas.WidthPx)
            Dim ch As Double = If(canvasHeight > 0, canvasHeight, _canvas.HeightPx)

            ' Canvas rect
            layout.CanvasRect = New RectangleF(0, 0, CSng(cw), CSng(ch))

            ' Plot margins
            Dim margin As (mt As Double, mr As Double, mb As Double, ml As Double) = ResolveMarginToPixels("plot.margin")
            layout.MarginTopPx = margin.mt
            layout.MarginRightPx = margin.mr
            layout.MarginBottomPx = margin.mb
            layout.MarginLeftPx = margin.ml

            ' Plot area (inside margins)
            layout.PlotAreaRect = New RectangleF(
                CSng(margin.ml),
                CSng(margin.mt),
                CSng(cw - margin.ml - margin.mr),
                CSng(ch - margin.mt - margin.mb)
            )

            ' Tick lengths
            layout.TickLengthXBottomPx = GetTickLengthPx("x.bottom")
            layout.TickLengthXTopPx = GetTickLengthPx("x.top")
            layout.TickLengthYLeftPx = GetTickLengthPx("y.left")
            layout.TickLengthYRightPx = GetTickLengthPx("y.right")

            ' Panel spacing
            layout.PanelSpacingXPx = GetPanelSpacingPx("x")
            layout.PanelSpacingYPx = GetPanelSpacingPx("y")

            ' Legend key size and spacing
            layout.LegendKeySizePx = GetLegendKeySizePx()
            layout.LegendSpacingPx = GetLegendSpacingPx()

            ' Panel area (simplified: same as plot area for single-panel plots)
            ' In a real implementation, this would account for axis labels, titles, etc.
            layout.PanelAreaRect = layout.PlotAreaRect

            Return layout
        End Function

        '==================================================================
        ' Legend position computation
        '==================================================================

        ''' <summary>
        ''' Computes the legend position based on the theme's legend.position
        ''' and legend.position.inside settings.
        ''' </summary>
        ''' <returns>A tuple of (x, y, isInside) where x,y are in pixels or NPC.</returns>
        Public Function ComputeLegendPosition(plotArea As RectangleF) As (X As Double, Y As Double, IsInside As Boolean, Position As String)
            Dim pos As Object = _theme.ResolveScalar("legend.position")
            Dim posStr As String = If(pos?.ToString(), "right").ToLowerInvariant()

            Select Case posStr
                Case "none"
                    Return (-1, -1, False, "none")
                Case "left"
                    Return (0, plotArea.Top + plotArea.Height / 2.0, False, "left")
                Case "right"
                    Return (plotArea.Right, plotArea.Top + plotArea.Height / 2.0, False, "right")
                Case "top"
                    Return (plotArea.Left + plotArea.Width / 2.0, 0, False, "top")
                Case "bottom"
                    Return (plotArea.Left + plotArea.Width / 2.0, plotArea.Bottom, False, "bottom")
                Case "inside"
                    ' Use legend.position.inside for coordinates
                    Dim insidePos As Object = _theme.ResolveScalar("legend.position.inside")
                    If insidePos IsNot Nothing Then
                        Dim posStr2 As String = insidePos.ToString()
                        Dim parts() As String = posStr2.Split(New Char() {" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
                        If parts.Length = 2 Then
                            Dim xNpc, yNpc As Double
                            If Double.TryParse(parts(0), NumberStyles.Float, CultureInfo.InvariantCulture, xNpc) AndAlso
                               Double.TryParse(parts(1), NumberStyles.Float, CultureInfo.InvariantCulture, yNpc) Then
                                Dim x As Double = plotArea.Left + xNpc * plotArea.Width
                                Dim y As Double = plotArea.Top + yNpc * plotArea.Height
                                Return (x, y, True, "inside")
                            End If
                        End If
                    End If
                    ' Default inside position: top-right
                    Return (plotArea.Right - 10, plotArea.Top + 10, True, "inside")
                Case Else
                    ' Try to parse as coordinate pair
                    Dim parts() As String = posStr.Split(New Char() {" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
                    If parts.Length = 2 Then
                        Dim xNpc, yNpc As Double
                        If Double.TryParse(parts(0), NumberStyles.Float, CultureInfo.InvariantCulture, xNpc) AndAlso
                           Double.TryParse(parts(1), NumberStyles.Float, CultureInfo.InvariantCulture, yNpc) Then
                            Dim x As Double = plotArea.Left + xNpc * plotArea.Width
                            Dim y As Double = plotArea.Top + yNpc * plotArea.Height
                            Return (x, y, True, "inside")
                        End If
                    End If
                    Return (plotArea.Right, plotArea.Top + plotArea.Height / 2.0, False, "right")
            End Select
        End Function

        '==================================================================
        ' Aspect ratio
        '==================================================================

        ''' <summary>
        ''' Gets the aspect ratio (plot width / plot height) from the theme.
        ''' Returns Nothing if not set (no constraint).
        ''' </summary>
        Public Function GetAspectRatio() As Double?
            Dim val As Object = _theme.ResolveScalar("aspect.ratio")
            If val Is Nothing Then Return Nothing
            If TypeOf val Is Double Then Return CDbl(val)
            Dim d As Double
            If Double.TryParse(val.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
                Return d
            End If
            Return Nothing
        End Function

        '==================================================================
        ' Summary: dump all resolved values (for debugging)
        '==================================================================

        ''' <summary>
        ''' Returns a string with all resolved layout values for debugging.
        ''' </summary>
        Public Function DumpLayout() As String
            Dim sb As New StringBuilder()
            sb.AppendLine("=== Layout Dump ===")
            sb.AppendLine($"Canvas: {_canvas.WidthPx}x{_canvas.HeightPx} px, PPI={_canvas.Ppi}")
            sb.AppendLine($"Base font size: {_canvas.BaseFontSizePt}pt ({_converter.PtToPx(_canvas.BaseFontSizePt):F1}px)")
            sb.AppendLine()

            ' Plot layout
            Dim layout As PlotLayout = ComputePlotLayout()
            sb.AppendLine("-- Plot Layout --")
            sb.AppendLine($"Canvas rect: {layout.CanvasRect}")
            sb.AppendLine($"Plot area: {layout.PlotAreaRect}")
            sb.AppendLine($"Margins (T,R,B,L): {layout.MarginTopPx:F1}, {layout.MarginRightPx:F1}, {layout.MarginBottomPx:F1}, {layout.MarginLeftPx:F1}px")
            sb.AppendLine($"Tick lengths (xB,xT,yL,yR): {layout.TickLengthXBottomPx:F1}, {layout.TickLengthXTopPx:F1}, {layout.TickLengthYLeftPx:F1}, {layout.TickLengthYRightPx:F1}px")
            sb.AppendLine($"Panel spacing (X,Y): {layout.PanelSpacingXPx:F1}, {layout.PanelSpacingYPx:F1}px")
            sb.AppendLine($"Legend key size: {layout.LegendKeySizePx:F1}px, spacing: {layout.LegendSpacingPx:F1}px")
            sb.AppendLine()

            ' Text elements
            sb.AppendLine("-- Text Elements (resolved) --")
            For Each name As String In {"text", "title", "axis.title", "axis.title.x.bottom", "axis.text", "axis.text.x.bottom", "plot.title", "plot.subtitle", "plot.caption", "legend.title", "legend.text", "strip.text"}
                Dim rt As ResolvedElementText = ResolveText(name)
                sb.AppendLine($"  {name}: blank={rt.IsBlank}, family={rt.Family}, face={rt.Face}, size={rt.SizePx:F1}px, color={rt.Colour.Name}, hjust={rt.Hjust}, vjust={rt.Vjust}, angle={rt.Angle}")
            Next
            sb.AppendLine()

            ' Line elements
            sb.AppendLine("-- Line Elements (resolved) --")
            For Each name As String In {"line", "axis.ticks", "axis.ticks.x.bottom", "axis.line", "axis.line.x.bottom", "panel.grid.major", "panel.grid.minor"}
                Dim rl As ResolvedElementLine = ResolveLine(name)
                sb.AppendLine($"  {name}: blank={rl.IsBlank}, color={rl.Colour.Name}, width={rl.LinewidthPx:F2}px, type={rl.Linetype}, end={rl.Lineend}")
            Next
            sb.AppendLine()

            ' Rect elements
            sb.AppendLine("-- Rect Elements (resolved) --")
            For Each name As String In {"rect", "panel.background", "panel.border", "plot.background", "legend.background", "legend.key", "strip.background"}
                Dim rr As ResolvedElementRect = ResolveRect(name)
                sb.AppendLine($"  {name}: blank={rr.IsBlank}, fill={rr.Fill.Name}, color={rr.Colour.Name}, width={rr.LinewidthPx:F2}px, type={rr.Linetype}")
            Next

            Return sb.ToString()
        End Function
    End Class

End Namespace

