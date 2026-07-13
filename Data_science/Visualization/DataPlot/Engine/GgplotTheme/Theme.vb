#Region "Microsoft.VisualBasic::53bc139600f4a0d77e92f9dea66c2284, Data_science\Visualization\DataPlot\Engine\GgplotTheme\Theme.vb"

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

    '   Total Lines: 1270
    '    Code Lines: 976 (76.85%)
    ' Comment Lines: 183 (14.41%)
    '    - Xml Docs: 54.10%
    ' 
    '   Blank Lines: 111 (8.74%)
    '     File Size: 62.49 KB


    '     Enum ThemeElementType
    ' 
    '         ElementGeom, ElementLine, ElementPoint, ElementPolygon, ElementRect
    '         ElementText, Margin, Scalar, Unit
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ThemeElementNames
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class InheritanceTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetAllElementNames, GetAncestry, GetElementType, GetParent, IsKnownElement
    ' 
    '     Class Theme
    ' 
    '         Properties: AspectRatio, AxisLine, AxisText, AxisTicks, AxisTitle
    '                     Canvas, Geom, LegendBackground, LegendPosition, LegendText
    '                     LegendTitle, Line, PanelBackground, PanelBorder, PanelGridMajor
    '                     PanelGridMinor, PlotBackground, PlotCaption, PlotMargin, PlotSubtitle
    '                     PlotTag, PlotTitle, Point, Polygon, Rect
    '                     StripBackground, StripText, Text, Title
    ' 
    '         Function: Clone, CreateElement, (+2 Overloads) GetElement, GetElementNames, GetMargin
    '                   (+2 Overloads) GetScalar, GetScalarNames, GetUnit, HasElement, HasMargin
    '                   HasScalar, HasUnit, ResolveElement, ResolveMargin, ResolveScalar
    '                   ResolveUnit
    ' 
    '         Sub: Merge, RemoveElement, SetElement, SetMargin, SetScalar
    '              SetUnit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace GgplotTheme

    '==========================================================================
    ' ThemeElementType Enumeration
    '==========================================================================
    ''' <summary>
    ''' Identifies the type of a theme element, used for CSS parsing and
    ''' inheritance resolution.
    ''' </summary>
    Public Enum ThemeElementType
        ''' <summary>Line element (element_line)</summary>
        ElementLine
        ''' <summary>Rectangle element (element_rect)</summary>
        ElementRect
        ''' <summary>Text element (element_text)</summary>
        ElementText
        ''' <summary>Point element (element_point)</summary>
        ElementPoint
        ''' <summary>Polygon element (element_polygon)</summary>
        ElementPolygon
        ''' <summary>Geom defaults element (element_geom)</summary>
        ElementGeom
        ''' <summary>Single Unit value (e.g., axis.ticks.length)</summary>
        Unit
        ''' <summary>Margin value (e.g., plot.margin)</summary>
        Margin
        ''' <summary>Scalar value: string, number, or boolean</summary>
        Scalar
    End Enum

    '==========================================================================
    ' ThemeElementNames - All ggplot theme element name constants
    '==========================================================================
    ''' <summary>
    ''' Contains string constants for all ggplot2 theme element names.
    ''' These match the parameter names of ggplot2's theme() function exactly.
    ''' </summary>
    Public NotInheritable Class ThemeElementNames

        '--- Root elements ---
        Public Const Line As String = "line"
        Public Const Rect As String = "rect"
        Public Const Text As String = "text"
        Public Const Title As String = "title"
        Public Const Point As String = "point"
        Public Const Polygon As String = "polygon"
        Public Const Geom As String = "geom"
        Public Const Spacing As String = "spacing"
        Public Const Margins As String = "margins"

        '--- Aspect ratio ---
        Public Const AspectRatio As String = "aspect.ratio"

        '--- Axis title ---
        Public Const AxisTitle As String = "axis.title"
        Public Const AxisTitleX As String = "axis.title.x"
        Public Const AxisTitleXTop As String = "axis.title.x.top"
        Public Const AxisTitleXBottom As String = "axis.title.x.bottom"
        Public Const AxisTitleY As String = "axis.title.y"
        Public Const AxisTitleYLeft As String = "axis.title.y.left"
        Public Const AxisTitleYRight As String = "axis.title.y.right"

        '--- Axis text ---
        Public Const AxisText As String = "axis.text"
        Public Const AxisTextX As String = "axis.text.x"
        Public Const AxisTextXTop As String = "axis.text.x.top"
        Public Const AxisTextXBottom As String = "axis.text.x.bottom"
        Public Const AxisTextY As String = "axis.text.y"
        Public Const AxisTextYLeft As String = "axis.text.y.left"
        Public Const AxisTextYRight As String = "axis.text.y.right"
        Public Const AxisTextTheta As String = "axis.text.theta"
        Public Const AxisTextR As String = "axis.text.r"

        '--- Axis ticks (line elements) ---
        Public Const AxisTicks As String = "axis.ticks"
        Public Const AxisTicksX As String = "axis.ticks.x"
        Public Const AxisTicksXTop As String = "axis.ticks.x.top"
        Public Const AxisTicksXBottom As String = "axis.ticks.x.bottom"
        Public Const AxisTicksY As String = "axis.ticks.y"
        Public Const AxisTicksYLeft As String = "axis.ticks.y.left"
        Public Const AxisTicksYRight As String = "axis.ticks.y.right"
        Public Const AxisTicksTheta As String = "axis.ticks.theta"
        Public Const AxisTicksR As String = "axis.ticks.r"

        '--- Axis minor ticks ---
        Public Const AxisMinorTicksXTop As String = "axis.minor.ticks.x.top"
        Public Const AxisMinorTicksXBottom As String = "axis.minor.ticks.x.bottom"
        Public Const AxisMinorTicksYLeft As String = "axis.minor.ticks.y.left"
        Public Const AxisMinorTicksYRight As String = "axis.minor.ticks.y.right"
        Public Const AxisMinorTicksTheta As String = "axis.minor.ticks.theta"
        Public Const AxisMinorTicksR As String = "axis.minor.ticks.r"

        '--- Axis ticks length (unit elements) ---
        Public Const AxisTicksLength As String = "axis.ticks.length"
        Public Const AxisTicksLengthX As String = "axis.ticks.length.x"
        Public Const AxisTicksLengthXTop As String = "axis.ticks.length.x.top"
        Public Const AxisTicksLengthXBottom As String = "axis.ticks.length.x.bottom"
        Public Const AxisTicksLengthY As String = "axis.ticks.length.y"
        Public Const AxisTicksLengthYLeft As String = "axis.ticks.length.y.left"
        Public Const AxisTicksLengthYRight As String = "axis.ticks.length.y.right"
        Public Const AxisTicksLengthTheta As String = "axis.ticks.length.theta"
        Public Const AxisTicksLengthR As String = "axis.ticks.length.r"

        '--- Axis minor ticks length ---
        Public Const AxisMinorTicksLength As String = "axis.minor.ticks.length"
        Public Const AxisMinorTicksLengthX As String = "axis.minor.ticks.length.x"
        Public Const AxisMinorTicksLengthXTop As String = "axis.minor.ticks.length.x.top"
        Public Const AxisMinorTicksLengthXBottom As String = "axis.minor.ticks.length.x.bottom"
        Public Const AxisMinorTicksLengthY As String = "axis.minor.ticks.length.y"
        Public Const AxisMinorTicksLengthYLeft As String = "axis.minor.ticks.length.y.left"
        Public Const AxisMinorTicksLengthYRight As String = "axis.minor.ticks.length.y.right"
        Public Const AxisMinorTicksLengthTheta As String = "axis.minor.ticks.length.theta"
        Public Const AxisMinorTicksLengthR As String = "axis.minor.ticks.length.r"

        '--- Axis line ---
        Public Const AxisLine As String = "axis.line"
        Public Const AxisLineX As String = "axis.line.x"
        Public Const AxisLineXTop As String = "axis.line.x.top"
        Public Const AxisLineXBottom As String = "axis.line.x.bottom"
        Public Const AxisLineY As String = "axis.line.y"
        Public Const AxisLineYLeft As String = "axis.line.y.left"
        Public Const AxisLineYRight As String = "axis.line.y.right"
        Public Const AxisLineTheta As String = "axis.line.theta"
        Public Const AxisLineR As String = "axis.line.r"

        '--- Legend ---
        Public Const LegendBackground As String = "legend.background"
        Public Const LegendMargin As String = "legend.margin"
        Public Const LegendSpacing As String = "legend.spacing"
        Public Const LegendSpacingX As String = "legend.spacing.x"
        Public Const LegendSpacingY As String = "legend.spacing.y"
        Public Const LegendKey As String = "legend.key"
        Public Const LegendKeySize As String = "legend.key.size"
        Public Const LegendKeyHeight As String = "legend.key.height"
        Public Const LegendKeyWidth As String = "legend.key.width"
        Public Const LegendKeySpacing As String = "legend.key.spacing"
        Public Const LegendKeySpacingX As String = "legend.key.spacing.x"
        Public Const LegendKeySpacingY As String = "legend.key.spacing.y"
        Public Const LegendKeyJustification As String = "legend.key.justification"
        Public Const LegendFrame As String = "legend.frame"
        Public Const LegendTicks As String = "legend.ticks"
        Public Const LegendTicksLength As String = "legend.ticks.length"
        Public Const LegendAxisLine As String = "legend.axis.line"
        Public Const LegendText As String = "legend.text"
        Public Const LegendTextPosition As String = "legend.text.position"
        Public Const LegendTitle As String = "legend.title"
        Public Const LegendTitlePosition As String = "legend.title.position"
        Public Const LegendPosition As String = "legend.position"
        Public Const LegendPositionInside As String = "legend.position.inside"
        Public Const LegendDirection As String = "legend.direction"
        Public Const LegendByrow As String = "legend.byrow"
        Public Const LegendJustification As String = "legend.justification"
        Public Const LegendJustificationTop As String = "legend.justification.top"
        Public Const LegendJustificationBottom As String = "legend.justification.bottom"
        Public Const LegendJustificationLeft As String = "legend.justification.left"
        Public Const LegendJustificationRight As String = "legend.justification.right"
        Public Const LegendJustificationInside As String = "legend.justification.inside"
        Public Const LegendLocation As String = "legend.location"
        Public Const LegendBox As String = "legend.box"
        Public Const LegendBoxJust As String = "legend.box.just"
        Public Const LegendBoxMargin As String = "legend.box.margin"
        Public Const LegendBoxBackground As String = "legend.box.background"
        Public Const LegendBoxSpacing As String = "legend.box.spacing"

        '--- Panel ---
        Public Const PanelBackground As String = "panel.background"
        Public Const PanelBorder As String = "panel.border"
        Public Const PanelSpacing As String = "panel.spacing"
        Public Const PanelSpacingX As String = "panel.spacing.x"
        Public Const PanelSpacingY As String = "panel.spacing.y"
        Public Const PanelGrid As String = "panel.grid"
        Public Const PanelGridMajor As String = "panel.grid.major"
        Public Const PanelGridMinor As String = "panel.grid.minor"
        Public Const PanelGridMajorX As String = "panel.grid.major.x"
        Public Const PanelGridMajorY As String = "panel.grid.major.y"
        Public Const PanelGridMinorX As String = "panel.grid.minor.x"
        Public Const PanelGridMinorY As String = "panel.grid.minor.y"
        Public Const PanelOntop As String = "panel.ontop"
        Public Const PanelWidths As String = "panel.widths"
        Public Const PanelHeights As String = "panel.heights"

        '--- Plot ---
        Public Const PlotBackground As String = "plot.background"
        Public Const PlotTitle As String = "plot.title"
        Public Const PlotTitlePosition As String = "plot.title.position"
        Public Const PlotSubtitle As String = "plot.subtitle"
        Public Const PlotCaption As String = "plot.caption"
        Public Const PlotCaptionPosition As String = "plot.caption.position"
        Public Const PlotTag As String = "plot.tag"
        Public Const PlotTagPosition As String = "plot.tag.position"
        Public Const PlotTagLocation As String = "plot.tag.location"
        Public Const PlotMargin As String = "plot.margin"

        '--- Strip ---
        Public Const StripBackground As String = "strip.background"
        Public Const StripBackgroundX As String = "strip.background.x"
        Public Const StripBackgroundY As String = "strip.background.y"
        Public Const StripClip As String = "strip.clip"
        Public Const StripPlacement As String = "strip.placement"
        Public Const StripText As String = "strip.text"
        Public Const StripTextX As String = "strip.text.x"
        Public Const StripTextXBottom As String = "strip.text.x.bottom"
        Public Const StripTextXTop As String = "strip.text.x.top"
        Public Const StripTextY As String = "strip.text.y"
        Public Const StripTextYLeft As String = "strip.text.y.left"
        Public Const StripTextYRight As String = "strip.text.y.right"
        Public Const StripSwitchPadGrid As String = "strip.switch.pad.grid"
        Public Const StripSwitchPadWrap As String = "strip.switch.pad.wrap"

        '--- Complete/Validate ---
        Public Const Complete As String = "complete"
        Public Const Validate As String = "validate"

        Private Sub New()
        End Sub
    End Class

    '==========================================================================
    ' InheritanceTree - Defines the ggplot2 theme inheritance hierarchy
    '==========================================================================
    ''' <summary>
    ''' Defines the inheritance relationships between theme elements and
    ''' the element type for each named element. This mirrors the
    ''' inheritance tree documented in ggplot2's theme() function.
    '''
    ''' Inheritance works as follows:
    ''' - Each element has a parent element (or is a root)
    ''' - When resolving an element, unset properties are filled from the parent
    ''' - This continues up the tree until all properties are resolved
    ''' - Root elements (line, rect, text, etc.) provide base defaults
    ''' </summary>
    Public NotInheritable Class InheritanceTree

        ' Maps element name -> parent element name (Nothing = root)
        Private Shared _parents As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                                                                                                   _ '--- Root elements (no parent) ---
            {"line", Nothing},
            {"rect", Nothing},
            {"text", Nothing},
            {"title", "text"},
            {"point", Nothing},
            {"polygon", Nothing},
            {"geom", Nothing},
            {"spacing", Nothing},
            {"margins", Nothing},
                                 _
                                 _ '--- Axis title (inherits from title -> text) ---
            {"axis.title", "title"},
            {"axis.title.x", "axis.title"},
            {"axis.title.x.top", "axis.title.x"},
            {"axis.title.x.bottom", "axis.title.x"},
            {"axis.title.y", "axis.title"},
            {"axis.title.y.left", "axis.title.y"},
            {"axis.title.y.right", "axis.title.y"},
                                                   _
                                                   _   '--- Axis text (inherits from text) ---
            {"axis.text", "text"},
            {"axis.text.x", "axis.text"},
            {"axis.text.x.top", "axis.text.x"},
            {"axis.text.x.bottom", "axis.text.x"},
            {"axis.text.y", "axis.text"},
            {"axis.text.y.left", "axis.text.y"},
            {"axis.text.y.right", "axis.text.y"},
            {"axis.text.theta", "axis.text"},
            {"axis.text.r", "axis.text"},
                                         _
                                         _  '--- Axis ticks (line elements, inherit from line) ---
            {"axis.ticks", "line"},
            {"axis.ticks.x", "axis.ticks"},
            {"axis.ticks.x.top", "axis.ticks.x"},
            {"axis.ticks.x.bottom", "axis.ticks.x"},
            {"axis.ticks.y", "axis.ticks"},
            {"axis.ticks.y.left", "axis.ticks.y"},
            {"axis.ticks.y.right", "axis.ticks.y"},
            {"axis.ticks.theta", "axis.ticks"},
            {"axis.ticks.r", "axis.ticks"},
                                           _
                                           _  '--- Axis minor ticks (inherit from axis.ticks) ---
            {"axis.minor.ticks.x.top", "axis.ticks.x.top"},
            {"axis.minor.ticks.x.bottom", "axis.ticks.x.bottom"},
            {"axis.minor.ticks.y.left", "axis.ticks.y.left"},
            {"axis.minor.ticks.y.right", "axis.ticks.y.right"},
            {"axis.minor.ticks.theta", "axis.ticks.theta"},
            {"axis.minor.ticks.r", "axis.ticks.r"},
                                                   _
                                                   _  '--- Axis ticks length (unit elements, inherit from spacing) ---
            {"axis.ticks.length", "spacing"},
            {"axis.ticks.length.x", "axis.ticks.length"},
            {"axis.ticks.length.x.top", "axis.ticks.length.x"},
            {"axis.ticks.length.x.bottom", "axis.ticks.length.x"},
            {"axis.ticks.length.y", "axis.ticks.length"},
            {"axis.ticks.length.y.left", "axis.ticks.length.y"},
            {"axis.ticks.length.y.right", "axis.ticks.length.y"},
            {"axis.ticks.length.theta", "axis.ticks.length"},
            {"axis.ticks.length.r", "axis.ticks.length"},
                                                         _
                                                         _   '--- Axis minor ticks length (inherit from axis.ticks.length) ---
            {"axis.minor.ticks.length", "axis.ticks.length"},
            {"axis.minor.ticks.length.x", "axis.minor.ticks.length"},
            {"axis.minor.ticks.length.x.top", "axis.minor.ticks.length.x"},
            {"axis.minor.ticks.length.x.bottom", "axis.minor.ticks.length.x"},
            {"axis.minor.ticks.length.y", "axis.minor.ticks.length"},
            {"axis.minor.ticks.length.y.left", "axis.minor.ticks.length.y"},
            {"axis.minor.ticks.length.y.right", "axis.minor.ticks.length.y"},
            {"axis.minor.ticks.length.theta", "axis.minor.ticks.length"},
            {"axis.minor.ticks.length.r", "axis.minor.ticks.length"},
                                                                     _
                                                                     _  '--- Axis line (line elements, inherit from line) ---
            {"axis.line", "line"},
            {"axis.line.x", "axis.line"},
            {"axis.line.x.top", "axis.line.x"},
            {"axis.line.x.bottom", "axis.line.x"},
            {"axis.line.y", "axis.line"},
            {"axis.line.y.left", "axis.line.y"},
            {"axis.line.y.right", "axis.line.y"},
            {"axis.line.theta", "axis.line"},
            {"axis.line.r", "axis.line"},
                                         _
                                         _  '--- Legend (rect, text, line, unit, margin, scalar elements) ---
            {"legend.background", "rect"},
            {"legend.margin", "margins"},
            {"legend.spacing", "spacing"},
            {"legend.spacing.x", "legend.spacing"},
            {"legend.spacing.y", "legend.spacing"},
            {"legend.key", "rect"},
            {"legend.key.size", "spacing"},
            {"legend.key.height", "legend.key.size"},
            {"legend.key.width", "legend.key.size"},
            {"legend.key.spacing", "spacing"},
            {"legend.key.spacing.x", "legend.key.spacing"},
            {"legend.key.spacing.y", "legend.key.spacing"},
            {"legend.key.justification", Nothing},
            {"legend.frame", "rect"},
            {"legend.ticks", "line"},
            {"legend.ticks.length", "legend.key.size"},
            {"legend.axis.line", "axis.line"},
            {"legend.text", "text"},
            {"legend.text.position", Nothing},
            {"legend.title", "title"},
            {"legend.title.position", Nothing},
            {"legend.position", Nothing},
            {"legend.position.inside", Nothing},
            {"legend.direction", Nothing},
            {"legend.byrow", Nothing},
            {"legend.justification", Nothing},
            {"legend.justification.top", "legend.justification"},
            {"legend.justification.bottom", "legend.justification"},
            {"legend.justification.left", "legend.justification"},
            {"legend.justification.right", "legend.justification"},
            {"legend.justification.inside", "legend.justification"},
            {"legend.location", Nothing},
            {"legend.box", Nothing},
            {"legend.box.just", Nothing},
            {"legend.box.margin", "margins"},
            {"legend.box.background", "rect"},
            {"legend.box.spacing", "spacing"},
                                              _
                                              _  '--- Panel ---
            {"panel.background", "rect"},
            {"panel.border", "rect"},
            {"panel.spacing", "spacing"},
            {"panel.spacing.x", "panel.spacing"},
            {"panel.spacing.y", "panel.spacing"},
            {"panel.grid", "line"},
            {"panel.grid.major", "panel.grid"},
            {"panel.grid.minor", "panel.grid"},
            {"panel.grid.major.x", "panel.grid.major"},
            {"panel.grid.major.y", "panel.grid.major"},
            {"panel.grid.minor.x", "panel.grid.minor"},
            {"panel.grid.minor.y", "panel.grid.minor"},
            {"panel.ontop", Nothing},
            {"panel.widths", Nothing},
            {"panel.heights", Nothing},
                                       _
                                       _  '--- Plot ---
            {"plot.background", "rect"},
            {"plot.title", "title"},
            {"plot.title.position", Nothing},
            {"plot.subtitle", "title"},
            {"plot.caption", "title"},
            {"plot.caption.position", Nothing},
            {"plot.tag", "title"},
            {"plot.tag.position", Nothing},
            {"plot.tag.location", Nothing},
            {"plot.margin", "margins"},
                                       _
                                       _  '--- Strip ---
            {"strip.background", "rect"},
            {"strip.background.x", "strip.background"},
            {"strip.background.y", "strip.background"},
            {"strip.clip", Nothing},
            {"strip.placement", Nothing},
            {"strip.text", "text"},
            {"strip.text.x", "strip.text"},
            {"strip.text.x.bottom", "strip.text.x"},
            {"strip.text.x.top", "strip.text.x"},
            {"strip.text.y", "strip.text"},
            {"strip.text.y.left", "strip.text.y"},
            {"strip.text.y.right", "strip.text.y"},
            {"strip.switch.pad.grid", "spacing"},
            {"strip.switch.pad.wrap", "spacing"},
                                                 _
                                                 _   '--- Scalar properties ---
            {"aspect.ratio", Nothing},
            {"complete", Nothing},
            {"validate", Nothing}
        }

        ' Maps element name -> element type
        Private Shared _types As New Dictionary(Of String, ThemeElementType)(StringComparer.Ordinal) From {
                                                                                                           _  '--- Root elements ---
            {"line", ThemeElementType.ElementLine},
            {"rect", ThemeElementType.ElementRect},
            {"text", ThemeElementType.ElementText},
            {"title", ThemeElementType.ElementText},
            {"point", ThemeElementType.ElementPoint},
            {"polygon", ThemeElementType.ElementPolygon},
            {"geom", ThemeElementType.ElementGeom},
            {"spacing", ThemeElementType.Unit},
            {"margins", ThemeElementType.Margin},
                                                 _
                                                 _   '--- Axis title ---
            {"axis.title", ThemeElementType.ElementText},
            {"axis.title.x", ThemeElementType.ElementText},
            {"axis.title.x.top", ThemeElementType.ElementText},
            {"axis.title.x.bottom", ThemeElementType.ElementText},
            {"axis.title.y", ThemeElementType.ElementText},
            {"axis.title.y.left", ThemeElementType.ElementText},
            {"axis.title.y.right", ThemeElementType.ElementText},
                                                                 _
                                                                 _  '--- Axis text ---
            {"axis.text", ThemeElementType.ElementText},
            {"axis.text.x", ThemeElementType.ElementText},
            {"axis.text.x.top", ThemeElementType.ElementText},
            {"axis.text.x.bottom", ThemeElementType.ElementText},
            {"axis.text.y", ThemeElementType.ElementText},
            {"axis.text.y.left", ThemeElementType.ElementText},
            {"axis.text.y.right", ThemeElementType.ElementText},
            {"axis.text.theta", ThemeElementType.ElementText},
            {"axis.text.r", ThemeElementType.ElementText},
                                                          _
                                                          _  '--- Axis ticks ---
            {"axis.ticks", ThemeElementType.ElementLine},
            {"axis.ticks.x", ThemeElementType.ElementLine},
            {"axis.ticks.x.top", ThemeElementType.ElementLine},
            {"axis.ticks.x.bottom", ThemeElementType.ElementLine},
            {"axis.ticks.y", ThemeElementType.ElementLine},
            {"axis.ticks.y.left", ThemeElementType.ElementLine},
            {"axis.ticks.y.right", ThemeElementType.ElementLine},
            {"axis.ticks.theta", ThemeElementType.ElementLine},
            {"axis.ticks.r", ThemeElementType.ElementLine},
                                                           _
            {"axis.minor.ticks.x.top", ThemeElementType.ElementLine},
            {"axis.minor.ticks.x.bottom", ThemeElementType.ElementLine},
            {"axis.minor.ticks.y.left", ThemeElementType.ElementLine},
            {"axis.minor.ticks.y.right", ThemeElementType.ElementLine},
            {"axis.minor.ticks.theta", ThemeElementType.ElementLine},
            {"axis.minor.ticks.r", ThemeElementType.ElementLine},
                                                                 _
                                                                 _  '--- Axis ticks length ---
            {"axis.ticks.length", ThemeElementType.Unit},
            {"axis.ticks.length.x", ThemeElementType.Unit},
            {"axis.ticks.length.x.top", ThemeElementType.Unit},
            {"axis.ticks.length.x.bottom", ThemeElementType.Unit},
            {"axis.ticks.length.y", ThemeElementType.Unit},
            {"axis.ticks.length.y.left", ThemeElementType.Unit},
            {"axis.ticks.length.y.right", ThemeElementType.Unit},
            {"axis.ticks.length.theta", ThemeElementType.Unit},
            {"axis.ticks.length.r", ThemeElementType.Unit},
                                                           _
            {"axis.minor.ticks.length", ThemeElementType.Unit},
            {"axis.minor.ticks.length.x", ThemeElementType.Unit},
            {"axis.minor.ticks.length.x.top", ThemeElementType.Unit},
            {"axis.minor.ticks.length.x.bottom", ThemeElementType.Unit},
            {"axis.minor.ticks.length.y", ThemeElementType.Unit},
            {"axis.minor.ticks.length.y.left", ThemeElementType.Unit},
            {"axis.minor.ticks.length.y.right", ThemeElementType.Unit},
            {"axis.minor.ticks.length.theta", ThemeElementType.Unit},
            {"axis.minor.ticks.length.r", ThemeElementType.Unit},
                                                                 _
                                                                 _ '--- Axis line ---
            {"axis.line", ThemeElementType.ElementLine},
            {"axis.line.x", ThemeElementType.ElementLine},
            {"axis.line.x.top", ThemeElementType.ElementLine},
            {"axis.line.x.bottom", ThemeElementType.ElementLine},
            {"axis.line.y", ThemeElementType.ElementLine},
            {"axis.line.y.left", ThemeElementType.ElementLine},
            {"axis.line.y.right", ThemeElementType.ElementLine},
            {"axis.line.theta", ThemeElementType.ElementLine},
            {"axis.line.r", ThemeElementType.ElementLine},
                                                          _
                                                          _  '--- Legend ---
            {"legend.background", ThemeElementType.ElementRect},
            {"legend.margin", ThemeElementType.Margin},
            {"legend.spacing", ThemeElementType.Unit},
            {"legend.spacing.x", ThemeElementType.Unit},
            {"legend.spacing.y", ThemeElementType.Unit},
            {"legend.key", ThemeElementType.ElementRect},
            {"legend.key.size", ThemeElementType.Unit},
            {"legend.key.height", ThemeElementType.Unit},
            {"legend.key.width", ThemeElementType.Unit},
            {"legend.key.spacing", ThemeElementType.Unit},
            {"legend.key.spacing.x", ThemeElementType.Unit},
            {"legend.key.spacing.y", ThemeElementType.Unit},
            {"legend.key.justification", ThemeElementType.Scalar},
            {"legend.frame", ThemeElementType.ElementRect},
            {"legend.ticks", ThemeElementType.ElementLine},
            {"legend.ticks.length", ThemeElementType.Unit},
            {"legend.axis.line", ThemeElementType.ElementLine},
            {"legend.text", ThemeElementType.ElementText},
            {"legend.text.position", ThemeElementType.Scalar},
            {"legend.title", ThemeElementType.ElementText},
            {"legend.title.position", ThemeElementType.Scalar},
            {"legend.position", ThemeElementType.Scalar},
            {"legend.position.inside", ThemeElementType.Scalar},
            {"legend.direction", ThemeElementType.Scalar},
            {"legend.byrow", ThemeElementType.Scalar},
            {"legend.justification", ThemeElementType.Scalar},
            {"legend.justification.top", ThemeElementType.Scalar},
            {"legend.justification.bottom", ThemeElementType.Scalar},
            {"legend.justification.left", ThemeElementType.Scalar},
            {"legend.justification.right", ThemeElementType.Scalar},
            {"legend.justification.inside", ThemeElementType.Scalar},
            {"legend.location", ThemeElementType.Scalar},
            {"legend.box", ThemeElementType.Scalar},
            {"legend.box.just", ThemeElementType.Scalar},
            {"legend.box.margin", ThemeElementType.Margin},
            {"legend.box.background", ThemeElementType.ElementRect},
            {"legend.box.spacing", ThemeElementType.Unit},
                                                          _
                                                          _  '--- Panel ---
            {"panel.background", ThemeElementType.ElementRect},
            {"panel.border", ThemeElementType.ElementRect},
            {"panel.spacing", ThemeElementType.Unit},
            {"panel.spacing.x", ThemeElementType.Unit},
            {"panel.spacing.y", ThemeElementType.Unit},
            {"panel.grid", ThemeElementType.ElementLine},
            {"panel.grid.major", ThemeElementType.ElementLine},
            {"panel.grid.minor", ThemeElementType.ElementLine},
            {"panel.grid.major.x", ThemeElementType.ElementLine},
            {"panel.grid.major.y", ThemeElementType.ElementLine},
            {"panel.grid.minor.x", ThemeElementType.ElementLine},
            {"panel.grid.minor.y", ThemeElementType.ElementLine},
            {"panel.ontop", ThemeElementType.Scalar},
            {"panel.widths", ThemeElementType.Unit},
            {"panel.heights", ThemeElementType.Unit},
                                                     _
                                                     _  '--- Plot ---
            {"plot.background", ThemeElementType.ElementRect},
            {"plot.title", ThemeElementType.ElementText},
            {"plot.title.position", ThemeElementType.Scalar},
            {"plot.subtitle", ThemeElementType.ElementText},
            {"plot.caption", ThemeElementType.ElementText},
            {"plot.caption.position", ThemeElementType.Scalar},
            {"plot.tag", ThemeElementType.ElementText},
            {"plot.tag.position", ThemeElementType.Scalar},
            {"plot.tag.location", ThemeElementType.Scalar},
            {"plot.margin", ThemeElementType.Margin},
                                                     _
                                                     _  '--- Strip ---
            {"strip.background", ThemeElementType.ElementRect},
            {"strip.background.x", ThemeElementType.ElementRect},
            {"strip.background.y", ThemeElementType.ElementRect},
            {"strip.clip", ThemeElementType.Scalar},
            {"strip.placement", ThemeElementType.Scalar},
            {"strip.text", ThemeElementType.ElementText},
            {"strip.text.x", ThemeElementType.ElementText},
            {"strip.text.x.bottom", ThemeElementType.ElementText},
            {"strip.text.x.top", ThemeElementType.ElementText},
            {"strip.text.y", ThemeElementType.ElementText},
            {"strip.text.y.left", ThemeElementType.ElementText},
            {"strip.text.y.right", ThemeElementType.ElementText},
            {"strip.switch.pad.grid", ThemeElementType.Unit},
            {"strip.switch.pad.wrap", ThemeElementType.Unit},
                                                             _
                                                             _  '--- Scalars ---
            {"aspect.ratio", ThemeElementType.Scalar},
            {"complete", ThemeElementType.Scalar},
            {"validate", ThemeElementType.Scalar}
        }

        ''' <summary>Returns the parent element name for the given element, or Nothing if it's a root.</summary>
        Public Shared Function GetParent(name As String) As String
            If name Is Nothing Then Return Nothing
            If _parents.ContainsKey(name) Then Return _parents(name)
            Return Nothing
        End Function

        ''' <summary>Returns the element type for the given element name.</summary>
        Public Shared Function GetElementType(name As String) As ThemeElementType
            If name Is Nothing Then Return ThemeElementType.Scalar
            If _types.ContainsKey(name) Then Return _types(name)
            Return ThemeElementType.Scalar
        End Function

        ''' <summary>Returns True if the element name is known.</summary>
        Public Shared Function IsKnownElement(name As String) As Boolean
            Return name IsNot Nothing AndAlso _types.ContainsKey(name)
        End Function

        ''' <summary>Returns the inheritance chain from root to the given element.</summary>
        Public Shared Function GetAncestry(name As String) As List(Of String)
            Dim chain As New List(Of String)()
            Dim current As String = name
            Dim visited As New HashSet(Of String)(StringComparer.Ordinal)
            While current IsNot Nothing AndAlso Not visited.Contains(current)
                chain.Insert(0, current)
                visited.Add(current)
                current = GetParent(current)
            End While
            Return chain
        End Function

        ''' <summary>Returns all known element names.</summary>
        Public Shared Function GetAllElementNames() As List(Of String)
            Return New List(Of String)(_types.Keys)
        End Function

        Private Sub New()
        End Sub
    End Class

    '==========================================================================
    ' Theme Class
    '==========================================================================
    ''' <summary>
    ''' The main theme class that holds all ggplot2 theme elements and scalar
    ''' properties. Elements are stored in a dictionary keyed by their ggplot
    ''' name (e.g., "axis.title.x.bottom").
    '''
    ''' The theme supports:
    ''' - Root elements (line, rect, text, etc.) that provide base defaults
    ''' - Child elements that inherit unset properties from their parent
    ''' - Property-level inheritance (individual properties can be overridden)
    ''' - element_blank support (IsBlank = True)
    ''' - Scalar properties (aspect.ratio, legend.position, etc.)
    ''' </summary>
    <Serializable>
    Public Class Theme
        Implements ICloneable

        ' Stores theme elements (ElementLine, ElementRect, ElementText, etc.) keyed by name
        Private _elements As New Dictionary(Of String, ThemeElement)(StringComparer.Ordinal)

        ' Stores Unit values (axis.ticks.length, panel.spacing, etc.) keyed by name
        Private _units As New Dictionary(Of String, Unit)(StringComparer.Ordinal)

        ' Stores Margin values (plot.margin, legend.margin, etc.) keyed by name
        Private _margins As New Dictionary(Of String, Margin)(StringComparer.Ordinal)

        ' Stores scalar values (aspect.ratio, legend.position, etc.) keyed by name
        Private _scalars As New Dictionary(Of String, Object)(StringComparer.Ordinal)

        ' Canvas context (optional, can be set for layout calculation)
        Private _canvas As CanvasContext = Nothing

        ''' <summary>Canvas context associated with this theme (optional).</summary>
        Public Property Canvas As CanvasContext
            Get
                Return _canvas
            End Get
            Set(value As CanvasContext)
                _canvas = value
            End Set
        End Property

        '==================================================================
        ' Element accessors (ElementLine, ElementRect, ElementText, etc.)
        '==================================================================

        ''' <summary>
        ''' Gets a theme element by name. Returns Nothing if not set.
        ''' </summary>
        Public Function GetElement(name As String) As ThemeElement
            If name Is Nothing Then Return Nothing
            If _elements.ContainsKey(name) Then Return _elements(name)
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets a theme element by name, typed. Returns Nothing if not set or wrong type.
        ''' </summary>
        Public Function GetElement(Of T As ThemeElement)(name As String) As T
            Dim elem As ThemeElement = GetElement(name)
            If elem Is Nothing Then Return Nothing
            Return TryCast(elem, T)
        End Function

        ''' <summary>
        ''' Sets a theme element by name.
        ''' </summary>
        Public Sub SetElement(name As String, element As ThemeElement)
            If name Is Nothing Then Return
            If element IsNot Nothing Then element.IsSet = True
            _elements(name) = element
        End Sub

        ''' <summary>
        ''' Returns True if an element with the given name has been explicitly set.
        ''' </summary>
        Public Function HasElement(name As String) As Boolean
            Return name IsNot Nothing AndAlso _elements.ContainsKey(name)
        End Function

        ''' <summary>
        ''' Removes an element from the theme.
        ''' </summary>
        Public Sub RemoveElement(name As String)
            If name IsNot Nothing AndAlso _elements.ContainsKey(name) Then
                _elements.Remove(name)
            End If
        End Sub

        ''' <summary>Returns all element names that have been set.</summary>
        Public Function GetElementNames() As List(Of String)
            Return New List(Of String)(_elements.Keys)
        End Function

        '==================================================================
        ' Unit accessors (for spacing, lengths, etc.)
        '==================================================================

        ''' <summary>Gets a Unit value by name. Returns Nothing if not set.</summary>
        Public Function GetUnit(name As String) As Unit
            If name Is Nothing Then Return Nothing
            If _units.ContainsKey(name) Then Return _units(name)
            Return Nothing
        End Function

        ''' <summary>Sets a Unit value by name.</summary>
        Public Sub SetUnit(name As String, value As Unit)
            If name Is Nothing Then Return
            _units(name) = value
        End Sub

        ''' <summary>Returns True if a Unit with the given name has been set.</summary>
        Public Function HasUnit(name As String) As Boolean
            Return name IsNot Nothing AndAlso _units.ContainsKey(name)
        End Function

        '==================================================================
        ' Margin accessors
        '==================================================================

        ''' <summary>Gets a Margin value by name. Returns Nothing if not set.</summary>
        Public Function GetMargin(name As String) As Margin
            If name Is Nothing Then Return Nothing
            If _margins.ContainsKey(name) Then Return _margins(name)
            Return Nothing
        End Function

        ''' <summary>Sets a Margin value by name.</summary>
        Public Sub SetMargin(name As String, value As Margin)
            If name Is Nothing Then Return
            _margins(name) = value
        End Sub

        ''' <summary>Returns True if a Margin with the given name has been set.</summary>
        Public Function HasMargin(name As String) As Boolean
            Return name IsNot Nothing AndAlso _margins.ContainsKey(name)
        End Function

        '==================================================================
        ' Scalar accessors
        '==================================================================

        ''' <summary>Gets a scalar value by name. Returns Nothing if not set.</summary>
        Public Function GetScalar(name As String) As Object
            If name Is Nothing Then Return Nothing
            If _scalars.ContainsKey(name) Then Return _scalars(name)
            Return Nothing
        End Function

        ''' <summary>Gets a scalar value by name, typed. Returns default if not set.</summary>
        Public Function GetScalar(Of T)(name As String) As T
            Dim obj As Object = GetScalar(name)
            If obj Is Nothing Then Return Nothing
            If TypeOf obj Is T Then Return CType(obj, T)
            Try
                Return CType(Convert.ChangeType(obj, GetType(T)), T)
            Catch
                Return Nothing
            End Try
        End Function

        ''' <summary>Sets a scalar value by name.</summary>
        Public Sub SetScalar(name As String, value As Object)
            If name Is Nothing Then Return
            _scalars(name) = value
        End Sub

        ''' <summary>Returns True if a scalar with the given name has been set.</summary>
        Public Function HasScalar(name As String) As Boolean
            Return name IsNot Nothing AndAlso _scalars.ContainsKey(name)
        End Function

        ''' <summary>Returns all scalar names that have been set.</summary>
        Public Function GetScalarNames() As List(Of String)
            Return New List(Of String)(_scalars.Keys)
        End Function

        '==================================================================
        ' Typed convenience properties for root elements
        '==================================================================

        ''' <summary>Root line element. All line-type elements inherit from this.</summary>
        Public Property Line As ElementLine
            Get
                Return GetElement(Of ElementLine)(ThemeElementNames.Line)
            End Get
            Set(value As ElementLine)
                SetElement(ThemeElementNames.Line, value)
            End Set
        End Property

        ''' <summary>Root rect element. All rect-type elements inherit from this.</summary>
        Public Property Rect As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.Rect)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.Rect, value)
            End Set
        End Property

        ''' <summary>Root text element. All text-type elements inherit from this.</summary>
        Public Property Text As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.Text)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.Text, value)
            End Set
        End Property

        ''' <summary>Title element. Inherits from text. Plot titles inherit from this.</summary>
        Public Property Title As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.Title)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.Title, value)
            End Set
        End Property

        ''' <summary>Root point element.</summary>
        Public Property Point As ElementPoint
            Get
                Return GetElement(Of ElementPoint)(ThemeElementNames.Point)
            End Get
            Set(value As ElementPoint)
                SetElement(ThemeElementNames.Point, value)
            End Set
        End Property

        ''' <summary>Root polygon element.</summary>
        Public Property Polygon As ElementPolygon
            Get
                Return GetElement(Of ElementPolygon)(ThemeElementNames.Polygon)
            End Get
            Set(value As ElementPolygon)
                SetElement(ThemeElementNames.Polygon, value)
            End Set
        End Property

        ''' <summary>Root geom element. Defines default geom colors and palettes.</summary>
        Public Property Geom As ElementGeom
            Get
                Return GetElement(Of ElementGeom)(ThemeElementNames.Geom)
            End Get
            Set(value As ElementGeom)
                SetElement(ThemeElementNames.Geom, value)
            End Set
        End Property

        '==================================================================
        ' Typed convenience properties for common elements
        '==================================================================

        Public Property AxisTitle As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.AxisTitle)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.AxisTitle, value)
            End Set
        End Property

        Public Property AxisText As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.AxisText)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.AxisText, value)
            End Set
        End Property

        Public Property AxisTicks As ElementLine
            Get
                Return GetElement(Of ElementLine)(ThemeElementNames.AxisTicks)
            End Get
            Set(value As ElementLine)
                SetElement(ThemeElementNames.AxisTicks, value)
            End Set
        End Property

        Public Property AxisLine As ElementLine
            Get
                Return GetElement(Of ElementLine)(ThemeElementNames.AxisLine)
            End Get
            Set(value As ElementLine)
                SetElement(ThemeElementNames.AxisLine, value)
            End Set
        End Property

        Public Property PanelBackground As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.PanelBackground)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.PanelBackground, value)
            End Set
        End Property

        Public Property PanelBorder As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.PanelBorder)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.PanelBorder, value)
            End Set
        End Property

        Public Property PanelGridMajor As ElementLine
            Get
                Return GetElement(Of ElementLine)(ThemeElementNames.PanelGridMajor)
            End Get
            Set(value As ElementLine)
                SetElement(ThemeElementNames.PanelGridMajor, value)
            End Set
        End Property

        Public Property PanelGridMinor As ElementLine
            Get
                Return GetElement(Of ElementLine)(ThemeElementNames.PanelGridMinor)
            End Get
            Set(value As ElementLine)
                SetElement(ThemeElementNames.PanelGridMinor, value)
            End Set
        End Property

        Public Property PlotBackground As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.PlotBackground)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.PlotBackground, value)
            End Set
        End Property

        Public Property PlotTitle As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.PlotTitle)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.PlotTitle, value)
            End Set
        End Property

        Public Property PlotSubtitle As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.PlotSubtitle)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.PlotSubtitle, value)
            End Set
        End Property

        Public Property PlotCaption As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.PlotCaption)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.PlotCaption, value)
            End Set
        End Property

        Public Property PlotTag As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.PlotTag)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.PlotTag, value)
            End Set
        End Property

        Public Property PlotMargin As Margin
            Get
                Return GetMargin(ThemeElementNames.PlotMargin)
            End Get
            Set(value As Margin)
                SetMargin(ThemeElementNames.PlotMargin, value)
            End Set
        End Property

        Public Property LegendBackground As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.LegendBackground)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.LegendBackground, value)
            End Set
        End Property

        Public Property LegendText As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.LegendText)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.LegendText, value)
            End Set
        End Property

        Public Property LegendTitle As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.LegendTitle)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.LegendTitle, value)
            End Set
        End Property

        Public Property StripBackground As ElementRect
            Get
                Return GetElement(Of ElementRect)(ThemeElementNames.StripBackground)
            End Get
            Set(value As ElementRect)
                SetElement(ThemeElementNames.StripBackground, value)
            End Set
        End Property

        Public Property StripText As ElementText
            Get
                Return GetElement(Of ElementText)(ThemeElementNames.StripText)
            End Get
            Set(value As ElementText)
                SetElement(ThemeElementNames.StripText, value)
            End Set
        End Property

        ''' <summary>Aspect ratio scalar (plot width / plot height). Null = no constraint.</summary>
        Public Property AspectRatio As Double?
            Get
                Return GetScalar(Of Double?)(ThemeElementNames.AspectRatio)
            End Get
            Set(value As Double?)
                SetScalar(ThemeElementNames.AspectRatio, value)
            End Set
        End Property

        ''' <summary>Legend position: "none", "left", "right", "bottom", "top", or "inside".</summary>
        Public Property LegendPosition As String
            Get
                Return GetScalar(Of String)(ThemeElementNames.LegendPosition)
            End Get
            Set(value As String)
                SetScalar(ThemeElementNames.LegendPosition, value)
            End Set
        End Property

        '==================================================================
        ' Resolution: applies inheritance to produce fully-resolved elements
        '==================================================================

        ''' <summary>
        ''' Resolves a theme element by applying inheritance from its ancestors.
        ''' Returns a new element with all properties filled in from the
        ''' inheritance chain. If the element is not set, returns Nothing.
        ''' </summary>
        Public Function ResolveElement(name As String) As ThemeElement
            If Not InheritanceTree.IsKnownElement(name) Then Return Nothing

            Dim elemType As ThemeElementType = InheritanceTree.GetElementType(name)

            ' Handle element types (ElementLine, ElementRect, etc.)
            If elemType = ThemeElementType.ElementLine OrElse
               elemType = ThemeElementType.ElementRect OrElse
               elemType = ThemeElementType.ElementText OrElse
               elemType = ThemeElementType.ElementPoint OrElse
               elemType = ThemeElementType.ElementPolygon OrElse
               elemType = ThemeElementType.ElementGeom Then

                ' Get the ancestry chain from root to this element
                Dim ancestry As List(Of String) = InheritanceTree.GetAncestry(name)

                ' Start with a fresh element of the right type
                Dim resolved As ThemeElement = CreateElement(elemType)

                ' Merge from root down, so each level overrides the previous
                For Each ancestorName As String In ancestry
                    Dim ancestor As ThemeElement = GetElement(ancestorName)
                    If ancestor IsNot Nothing Then
                        resolved.MergeFrom(ancestor)
                    End If
                Next

                ' Finally, if this element itself is set, merge its own properties
                Dim ownElem As ThemeElement = GetElement(name)
                If ownElem IsNot Nothing Then
                    ' The own element's properties should take priority
                    ' We need to merge own into resolved, but only for set properties
                    ' Since MergeFrom fills in unset properties, we need to reverse:
                    ' create a copy of own, then merge resolved (which has inherited values) into it
                    Dim result As ThemeElement = CType(ownElem.Clone(), ThemeElement)
                    result.MergeFrom(resolved)
                    Return result
                End If

                Return resolved
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Resolves a Unit value by applying inheritance.
        ''' If the unit is relative (rel), resolves against the parent unit.
        ''' </summary>
        Public Function ResolveUnit(name As String) As Unit
            If Not InheritanceTree.IsKnownElement(name) Then Return Nothing
            If InheritanceTree.GetElementType(name) <> ThemeElementType.Unit Then Return Nothing

            ' Walk up the inheritance chain to find the first set unit
            Dim ancestry As List(Of String) = InheritanceTree.GetAncestry(name)
            ' ancestry is from root to this element; we want to find the first set value
            ' going from this element up to root
            For i As Integer = ancestry.Count - 1 To 0 Step -1
                Dim ancestorName As String = ancestry(i)
                Dim u As Unit = GetUnit(ancestorName)
                If u IsNot Nothing AndAlso Not u.IsNull() Then
                    ' If this is a relative unit and there's a parent value, resolve it
                    If u.IsRelative() AndAlso i > 0 Then
                        ' Find parent value
                        For j As Integer = i - 1 To 0 Step -1
                            Dim parentU As Unit = GetUnit(ancestry(j))
                            If parentU IsNot Nothing AndAlso Not parentU.IsNull() Then
                                Return u.ResolveRelative(parentU)
                            End If
                        Next
                    End If
                    Return u
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Resolves a Margin value by applying inheritance.
        ''' </summary>
        Public Function ResolveMargin(name As String) As Margin
            If Not InheritanceTree.IsKnownElement(name) Then Return Nothing
            If InheritanceTree.GetElementType(name) <> ThemeElementType.Margin Then Return Nothing

            Dim ancestry As List(Of String) = InheritanceTree.GetAncestry(name)
            For i As Integer = ancestry.Count - 1 To 0 Step -1
                Dim m As Margin = GetMargin(ancestry(i))
                If m IsNot Nothing Then Return m
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Resolves a scalar value by applying inheritance.
        ''' </summary>
        Public Function ResolveScalar(name As String) As Object
            If Not InheritanceTree.IsKnownElement(name) Then Return Nothing
            If InheritanceTree.GetElementType(name) <> ThemeElementType.Scalar Then Return Nothing

            Dim ancestry As List(Of String) = InheritanceTree.GetAncestry(name)
            For i As Integer = ancestry.Count - 1 To 0 Step -1
                If HasScalar(ancestry(i)) Then
                    Return GetScalar(ancestry(i))
                End If
            Next

            Return Nothing
        End Function

        '==================================================================
        ' Cloning
        '==================================================================

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim t As New Theme()
            ' Clone elements
            For Each kvp As KeyValuePair(Of String, ThemeElement) In _elements
                If kvp.Value IsNot Nothing Then
                    t._elements(kvp.Key) = CType(kvp.Value.Clone(), ThemeElement)
                Else
                    t._elements(kvp.Key) = Nothing
                End If
            Next
            ' Clone units
            For Each kvp As KeyValuePair(Of String, Unit) In _units
                t._units(kvp.Key) = If(kvp.Value?.Clone(), Nothing)
            Next
            ' Clone margins
            For Each kvp As KeyValuePair(Of String, Margin) In _margins
                t._margins(kvp.Key) = If(kvp.Value?.Clone(), Nothing)
            Next
            ' Copy scalars (value types are copied, reference types may need cloning)
            For Each kvp As KeyValuePair(Of String, Object) In _scalars
                t._scalars(kvp.Key) = kvp.Value
            Next
            ' Clone canvas
            t._canvas = If(_canvas?.Clone(), Nothing)
            Return t
        End Function

        '==================================================================
        ' Merge: merges another theme into this one (other overrides this)
        '==================================================================

        ''' <summary>
        ''' Merges another theme into this one. Elements from the other theme
        ''' override corresponding elements in this theme.
        ''' </summary>
        Public Sub Merge(other As Theme)
            If other Is Nothing Then Return
            For Each kvp As KeyValuePair(Of String, ThemeElement) In other._elements
                _elements(kvp.Key) = If(kvp.Value?.Clone(), Nothing)
            Next
            For Each kvp As KeyValuePair(Of String, Unit) In other._units
                _units(kvp.Key) = If(kvp.Value?.Clone(), Nothing)
            Next
            For Each kvp As KeyValuePair(Of String, Margin) In other._margins
                _margins(kvp.Key) = If(kvp.Value?.Clone(), Nothing)
            Next
            For Each kvp As KeyValuePair(Of String, Object) In other._scalars
                _scalars(kvp.Key) = kvp.Value
            Next
            If other._canvas IsNot Nothing Then
                _canvas = CType(other._canvas.Clone(), CanvasContext)
            End If
        End Sub

        '==================================================================
        ' Private helper: create element by type
        '==================================================================

        Private Function CreateElement(elemType As ThemeElementType) As ThemeElement
            Select Case elemType
                Case ThemeElementType.ElementLine : Return New ElementLine()
                Case ThemeElementType.ElementRect : Return New ElementRect()
                Case ThemeElementType.ElementText : Return New ElementText()
                Case ThemeElementType.ElementPoint : Return New ElementPoint()
                Case ThemeElementType.ElementPolygon : Return New ElementPolygon()
                Case ThemeElementType.ElementGeom : Return New ElementGeom()
                Case Else : Return Nothing
            End Select
        End Function
    End Class

End Namespace
