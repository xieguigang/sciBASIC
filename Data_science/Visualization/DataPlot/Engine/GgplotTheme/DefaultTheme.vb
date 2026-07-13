#Region "Microsoft.VisualBasic::c3082f74dfacf565db9ee904126dc0d1, Data_science\Visualization\DataPlot\Engine\GgplotTheme\DefaultTheme.vb"

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

    '   Total Lines: 489
    '    Code Lines: 273 (55.83%)
    ' Comment Lines: 124 (25.36%)
    '    - Xml Docs: 23.39%
    ' 
    '   Blank Lines: 92 (18.81%)
    '     File Size: 20.43 KB


    '     Class DefaultTheme
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Classic], Bw, Dark, Grey, Light
    '                   Minimal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Drawing

Namespace GgplotTheme

    '==========================================================================
    ' DefaultTheme - Factory for default ggplot2 themes
    '==========================================================================
    ''' <summary>
    ''' Provides factory methods for creating default themes equivalent to
    ''' ggplot2's built-in themes: theme_grey, theme_bw, theme_classic,
    ''' theme_minimal, theme_light, and theme_dark.
    ''' </summary>
    Public NotInheritable Class DefaultTheme

        Private Sub New()
        End Sub

        '==================================================================
        ' theme_grey - The default ggplot2 theme
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_grey().
        ''' This is the default ggplot2 theme with a grey panel background.
        ''' </summary>
        Public Shared Function Grey() As Theme
            Dim t As New Theme()

            '--- Canvas defaults ---
            t.Canvas = New CanvasContext(800, 600, 96, 11.0, 1.2)

            '--- Root: line ---
            Dim lineElem As New ElementLine()
            lineElem.Colour = Color.FromArgb(&H33, &H33, &H33)
            lineElem.Linewidth = New Unit(0.5, UnitType.Pt)
            lineElem.Linetype = "solid"
            lineElem.Lineend = "butt"
            lineElem.IsSet = True
            t.SetElement(ThemeElementNames.Line, lineElem)

            '--- Root: rect ---
            Dim rectElem As New ElementRect()
            rectElem.Fill = Color.FromArgb(&HEB, &HEB, &HEB)
            rectElem.Colour = Color.Empty ' NA
            rectElem.Linewidth = New Unit(0.5, UnitType.Pt)
            rectElem.Linetype = "solid"
            rectElem.IsSet = True
            t.SetElement(ThemeElementNames.Rect, rectElem)

            '--- Root: text ---
            Dim textElem As New ElementText()
            textElem.Family = "sans-serif"
            textElem.Face = "plain"
            textElem.Size = New Unit(11, UnitType.Pt)
            textElem.Colour = Color.FromArgb(&H33, &H33, &H33)
            textElem.Hjust = 0.5
            textElem.Vjust = 0.5
            textElem.Angle = 0.0
            textElem.Lineheight = 1.2
            textElem.Margin = New Margin(0, 0, 0, 0, UnitType.Pt)
            textElem.IsSet = True
            t.SetElement(ThemeElementNames.Text, textElem)

            '--- Root: title (inherits from text, overrides face) ---
            Dim titleElem As New ElementText()
            titleElem.Face = "plain"
            titleElem.IsSet = True
            t.SetElement(ThemeElementNames.Title, titleElem)

            '--- Root: spacing ---
            t.SetUnit(ThemeElementNames.Spacing, New Unit(0.5, UnitType.Lines))

            '--- Root: margins ---
            t.SetMargin(ThemeElementNames.Margins, New Margin(0, 0, 0, 0, UnitType.Pt))

            '--- Axis title ---
            Dim axisTitle As New ElementText()
            axisTitle.Face = "plain"
            axisTitle.IsSet = True
            t.SetElement(ThemeElementNames.AxisTitle, axisTitle)

            '--- Axis text ---
            Dim axisText As New ElementText()
            axisText.Size = New Unit(9, UnitType.Pt)
            axisText.IsSet = True
            t.SetElement(ThemeElementNames.AxisText, axisText)

            '--- Axis ticks ---
            Dim axisTicks As New ElementLine()
            axisTicks.Colour = Color.FromArgb(&H33, &H33, &H33)
            axisTicks.Linewidth = New Unit(0.5, UnitType.Pt)
            axisTicks.IsSet = True
            t.SetElement(ThemeElementNames.AxisTicks, axisTicks)

            '--- Axis ticks length ---
            t.SetUnit(ThemeElementNames.AxisTicksLength, New Unit(0.2, UnitType.Cm))

            '--- Axis minor ticks length (relative to major) ---
            t.SetUnit(ThemeElementNames.AxisMinorTicksLength, New Unit(0.5, UnitType.Rel))

            '--- Axis line ---
            Dim axisLine As New ElementLine()
            axisLine.Colour = Color.Empty ' NA - not drawn by default in theme_grey
            axisLine.Linewidth = New Unit(0.5, UnitType.Pt)
            axisLine.IsSet = True
            t.SetElement(ThemeElementNames.AxisLine, axisLine)

            '--- Panel background ---
            Dim panelBg As New ElementRect()
            panelBg.Fill = Color.FromArgb(&HEB, &HEB, &HEB)
            panelBg.Colour = Color.Empty ' NA
            panelBg.IsSet = True
            t.SetElement(ThemeElementNames.PanelBackground, panelBg)

            '--- Panel border ---
            Dim panelBorder As New ElementRect()
            panelBorder.Fill = Color.Empty ' NA
            panelBorder.Colour = Color.FromArgb(&H33, &H33, &H33)
            panelBorder.Linewidth = New Unit(0.5, UnitType.Pt)
            panelBorder.IsSet = True
            t.SetElement(ThemeElementNames.PanelBorder, panelBorder)

            '--- Panel grid ---
            Dim panelGrid As New ElementLine()
            panelGrid.Colour = Color.White
            panelGrid.Linewidth = New Unit(0.5, UnitType.Pt)
            panelGrid.IsSet = True
            t.SetElement(ThemeElementNames.PanelGrid, panelGrid)

            '--- Panel grid major ---
            Dim panelGridMajor As New ElementLine()
            panelGridMajor.Colour = Color.White
            panelGridMajor.Linewidth = New Unit(0.5, UnitType.Pt)
            panelGridMajor.IsSet = True
            t.SetElement(ThemeElementNames.PanelGridMajor, panelGridMajor)

            '--- Panel grid minor ---
            Dim panelGridMinor As New ElementLine()
            panelGridMinor.Colour = Color.White
            panelGridMinor.Linewidth = New Unit(0.25, UnitType.Pt)
            panelGridMinor.IsSet = True
            t.SetElement(ThemeElementNames.PanelGridMinor, panelGridMinor)

            '--- Panel spacing ---
            t.SetUnit(ThemeElementNames.PanelSpacing, New Unit(0.25, UnitType.Lines))

            '--- Plot background ---
            Dim plotBg As New ElementRect()
            plotBg.Fill = Color.White
            plotBg.Colour = Color.Empty ' NA
            plotBg.IsSet = True
            t.SetElement(ThemeElementNames.PlotBackground, plotBg)

            '--- Plot title ---
            Dim plotTitle As New ElementText()
            plotTitle.Size = New Unit(13.2, UnitType.Pt) ' rel(1.2)
            plotTitle.Hjust = 0.0
            plotTitle.Vjust = 1.0
            plotTitle.Face = "plain"
            plotTitle.Margin = New Margin(0, 0, 5.5, 0, UnitType.Pt)
            plotTitle.IsSet = True
            t.SetElement(ThemeElementNames.PlotTitle, plotTitle)

            '--- Plot subtitle ---
            Dim plotSubtitle As New ElementText()
            plotSubtitle.Size = New Unit(11, UnitType.Pt)
            plotSubtitle.Hjust = 0.0
            plotSubtitle.Vjust = 1.0
            plotSubtitle.Face = "plain"
            plotSubtitle.Margin = New Margin(0, 0, 5.5, 0, UnitType.Pt)
            plotSubtitle.IsSet = True
            t.SetElement(ThemeElementNames.PlotSubtitle, plotSubtitle)

            '--- Plot caption ---
            Dim plotCaption As New ElementText()
            plotCaption.Size = New Unit(9, UnitType.Pt)
            plotCaption.Hjust = 1.0
            plotCaption.Vjust = 1.0
            plotCaption.Face = "plain"
            plotCaption.Margin = New Margin(0, 0, 0, 0, UnitType.Pt)
            plotCaption.IsSet = True
            t.SetElement(ThemeElementNames.PlotCaption, plotCaption)

            '--- Plot tag ---
            Dim plotTag As New ElementText()
            plotTag.Size = New Unit(13.2, UnitType.Pt) ' rel(1.2)
            plotTag.Hjust = 0.0
            plotTag.Vjust = 0.0
            plotTag.Face = "plain"
            plotTag.IsSet = True
            t.SetElement(ThemeElementNames.PlotTag, plotTag)

            '--- Plot margin ---
            t.SetMargin(ThemeElementNames.PlotMargin, New Margin(5.5, 5.5, 5.5, 5.5, UnitType.Pt))

            '--- Legend background ---
            Dim legendBg As New ElementRect()
            legendBg.Fill = Color.Empty ' NA
            legendBg.Colour = Color.Empty ' NA
            legendBg.IsSet = True
            t.SetElement(ThemeElementNames.LegendBackground, legendBg)

            '--- Legend key ---
            Dim legendKey As New ElementRect()
            legendKey.Fill = Color.White
            legendKey.Colour = Color.Empty ' NA
            legendKey.IsSet = True
            t.SetElement(ThemeElementNames.LegendKey, legendKey)

            '--- Legend key size ---
            t.SetUnit(ThemeElementNames.LegendKeySize, New Unit(0.4, UnitType.Cm))

            '--- Legend spacing ---
            t.SetUnit(ThemeElementNames.LegendSpacing, New Unit(0.4, UnitType.Cm))

            '--- Legend key spacing ---
            t.SetUnit(ThemeElementNames.LegendKeySpacing, New Unit(0.2, UnitType.Cm))

            '--- Legend ticks length ---
            t.SetUnit(ThemeElementNames.LegendTicksLength, New Unit(0.2, UnitType.Cm))

            '--- Legend box spacing ---
            t.SetUnit(ThemeElementNames.LegendBoxSpacing, New Unit(0.4, UnitType.Cm))

            '--- Legend box background ---
            Dim legendBoxBg As New ElementRect()
            legendBoxBg.Fill = Color.Empty ' NA
            legendBoxBg.Colour = Color.Empty ' NA
            legendBoxBg.IsSet = True
            t.SetElement(ThemeElementNames.LegendBoxBackground, legendBoxBg)

            '--- Legend box margin ---
            t.SetMargin(ThemeElementNames.LegendBoxMargin, New Margin(0.6, 0.6, 0.6, 0.6, UnitType.Cm))

            '--- Legend margin ---
            t.SetMargin(ThemeElementNames.LegendMargin, New Margin(0.2, 0.2, 0.2, 0.2, UnitType.Cm))

            '--- Legend text ---
            Dim legendText As New ElementText()
            legendText.Size = New Unit(9, UnitType.Pt)
            legendText.IsSet = True
            t.SetElement(ThemeElementNames.LegendText, legendText)

            '--- Legend title ---
            Dim legendTitle As New ElementText()
            legendTitle.Size = New Unit(9, UnitType.Pt)
            legendTitle.Hjust = 0.0
            legendTitle.IsSet = True
            t.SetElement(ThemeElementNames.LegendTitle, legendTitle)

            '--- Legend position ---
            t.SetScalar(ThemeElementNames.LegendPosition, "right")

            '--- Legend direction ---
            t.SetScalar(ThemeElementNames.LegendDirection, "vertical")

            '--- Legend justification ---
            t.SetScalar(ThemeElementNames.LegendJustification, "center")

            '--- Legend box ---
            t.SetScalar(ThemeElementNames.LegendBox, "vertical")

            '--- Legend box just ---
            t.SetScalar(ThemeElementNames.LegendBoxJust, "center")

            '--- Legend location ---
            t.SetScalar(ThemeElementNames.LegendLocation, "panel")

            '--- Strip background ---
            Dim stripBg As New ElementRect()
            stripBg.Fill = Color.FromArgb(&HEB, &HEB, &HEB)
            stripBg.Colour = Color.Empty ' NA
            stripBg.IsSet = True
            t.SetElement(ThemeElementNames.StripBackground, stripBg)

            '--- Strip text ---
            Dim stripText As New ElementText()
            stripText.Size = New Unit(9, UnitType.Pt)
            stripText.Face = "plain"
            stripText.Margin = New Margin(0, 0, 0, 0, UnitType.Pt)
            stripText.IsSet = True
            t.SetElement(ThemeElementNames.StripText, stripText)

            '--- Strip switch pad grid ---
            t.SetUnit(ThemeElementNames.StripSwitchPadGrid, New Unit(0.1, UnitType.Cm))

            '--- Strip switch pad wrap ---
            t.SetUnit(ThemeElementNames.StripSwitchPadWrap, New Unit(0.1, UnitType.Cm))

            '--- Plot title position ---
            t.SetScalar(ThemeElementNames.PlotTitlePosition, "panel")

            '--- Plot caption position ---
            t.SetScalar(ThemeElementNames.PlotCaptionPosition, "panel")

            '--- Plot tag location ---
            t.SetScalar(ThemeElementNames.PlotTagLocation, "margin")

            '--- Strip placement ---
            t.SetScalar(ThemeElementNames.StripPlacement, "inside")

            '--- Strip clip ---
            t.SetScalar(ThemeElementNames.StripClip, "inherit")

            '--- Panel ontop ---
            t.SetScalar(ThemeElementNames.PanelOntop, False)

            Return t
        End Function

        '==================================================================
        ' theme_bw - Black and white theme
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_bw().
        ''' White panel background with black border.
        ''' </summary>
        Public Shared Function Bw() As Theme
            Dim t As Theme = Grey()

            ' Panel background: white
            Dim panelBg As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.PanelBackground)
            panelBg.Fill = Color.White

            ' Panel border: black
            Dim panelBorder As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.PanelBorder)
            panelBorder.Fill = Color.Empty ' NA
            panelBorder.Colour = Color.Black

            ' Panel grid: grey
            Dim panelGridMajor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMajor)
            panelGridMajor.Colour = Color.FromArgb(&HE5, &HE5, &HE5)

            Dim panelGridMinor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMinor)
            panelGridMinor.Colour = Color.FromArgb(&HF5, &HF5, &HF5)

            ' Strip background: white with grey border
            Dim stripBg As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.StripBackground)
            stripBg.Fill = Color.White
            stripBg.Colour = Color.FromArgb(&HE5, &HE5, &HE5)

            Return t
        End Function

        '==================================================================
        ' theme_classic - Classic theme (no grid, no background)
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_classic().
        ''' No grid lines, no background, only axis lines.
        ''' </summary>
        Public Shared Function [Classic]() As Theme
            Dim t As Theme = Grey()

            ' Panel background: blank
            Dim panelBg As New ElementRect()
            panelBg.IsBlank = True
            panelBg.IsSet = True
            t.SetElement(ThemeElementNames.PanelBackground, panelBg)

            ' Panel border: blank
            Dim panelBorder As New ElementRect()
            panelBorder.IsBlank = True
            panelBorder.IsSet = True
            t.SetElement(ThemeElementNames.PanelBorder, panelBorder)

            ' Panel grid: blank
            Dim panelGrid As New ElementLine()
            panelGrid.IsBlank = True
            panelGrid.IsSet = True
            t.SetElement(ThemeElementNames.PanelGrid, panelGrid)

            ' Axis line: black
            Dim axisLine As New ElementLine()
            axisLine.Colour = Color.Black
            axisLine.Linewidth = New Unit(0.5, UnitType.Pt)
            axisLine.IsSet = True
            t.SetElement(ThemeElementNames.AxisLine, axisLine)

            ' Strip background: blank
            Dim stripBg As New ElementRect()
            stripBg.IsBlank = True
            stripBg.IsSet = True
            t.SetElement(ThemeElementNames.StripBackground, stripBg)

            Return t
        End Function

        '==================================================================
        ' theme_minimal - Minimal theme
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_minimal().
        ''' No background, no border, no axis lines, subtle grid.
        ''' </summary>
        Public Shared Function Minimal() As Theme
            Dim t As Theme = Grey()

            ' Panel background: blank
            Dim panelBg As New ElementRect()
            panelBg.IsBlank = True
            panelBg.IsSet = True
            t.SetElement(ThemeElementNames.PanelBackground, panelBg)

            ' Panel border: blank
            Dim panelBorder As New ElementRect()
            panelBorder.IsBlank = True
            panelBorder.IsSet = True
            t.SetElement(ThemeElementNames.PanelBorder, panelBorder)

            ' Axis line: blank
            Dim axisLine As New ElementLine()
            axisLine.IsBlank = True
            axisLine.IsSet = True
            t.SetElement(ThemeElementNames.AxisLine, axisLine)

            ' Axis ticks: blank
            Dim axisTicks As New ElementLine()
            axisTicks.IsBlank = True
            axisTicks.IsSet = True
            t.SetElement(ThemeElementNames.AxisTicks, axisTicks)

            ' Strip background: blank
            Dim stripBg As New ElementRect()
            stripBg.IsBlank = True
            stripBg.IsSet = True
            t.SetElement(ThemeElementNames.StripBackground, stripBg)

            Return t
        End Function

        '==================================================================
        ' theme_light - Light theme
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_light().
        ''' Light grey background and grid lines.
        ''' </summary>
        Public Shared Function Light() As Theme
            Dim t As Theme = Grey()

            ' Panel background: white
            Dim panelBg As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.PanelBackground)
            panelBg.Fill = Color.White

            ' Panel border: light grey
            Dim panelBorder As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.PanelBorder)
            panelBorder.Fill = Color.Empty
            panelBorder.Colour = Color.FromArgb(&HE5, &HE5, &HE5)

            ' Panel grid major: light grey
            Dim panelGridMajor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMajor)
            panelGridMajor.Colour = Color.FromArgb(&HF0, &HF0, &HF0)

            ' Panel grid minor: lighter grey
            Dim panelGridMinor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMinor)
            panelGridMinor.Colour = Color.FromArgb(&HF5, &HF5, &HF5)

            Return t
        End Function

        '==================================================================
        ' theme_dark - Dark theme
        '==================================================================
        ''' <summary>
        ''' Creates a theme equivalent to ggplot2's theme_dark().
        ''' Dark panel background with light grid lines.
        ''' </summary>
        Public Shared Function Dark() As Theme
            Dim t As Theme = Grey()

            ' Panel background: dark grey
            Dim panelBg As ElementRect = t.GetElement(Of ElementRect)(ThemeElementNames.PanelBackground)
            panelBg.Fill = Color.FromArgb(&H4D, &H4D, &H4D)

            ' Panel grid major: lighter grey
            Dim panelGridMajor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMajor)
            panelGridMajor.Colour = Color.FromArgb(&H80, &H80, &H80)

            ' Panel grid minor: slightly lighter
            Dim panelGridMinor As ElementLine = t.GetElement(Of ElementLine)(ThemeElementNames.PanelGridMinor)
            panelGridMinor.Colour = Color.FromArgb(&H66, &H66, &H66)

            Return t
        End Function

    End Class

End Namespace

