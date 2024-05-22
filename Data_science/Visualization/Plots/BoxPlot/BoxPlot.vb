#Region "Microsoft.VisualBasic::eb53ee37a98bf2d2ddcbbac5c46e71e8, Data_science\Visualization\Plots\BoxPlot\BoxPlot.vb"

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

    '   Total Lines: 69
    '    Code Lines: 55 (79.71%)
    ' Comment Lines: 8 (11.59%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (8.70%)
    '     File Size: 2.95 KB


    '     Module BoxPlot
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BoxPlot

    ''' <summary>
    ''' ```
    ''' min, q1, q2, q3, max
    '''       _________
    '''  +----|   |   |----+
    '''       ---------
    ''' ```
    ''' </summary>
    Public Module BoxPlot

        Friend ReadOnly Zero As [Default](Of Double()) = LanguageAPI.[Default]({0R}, Function(x) TryCast(x, Double()).IsNullOrEmpty)

        <Extension>
        Public Function Plot(data As BoxData,
                             Optional size$ = "3000,2700",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional schema$ = ColorBrewer.QualitativeSchemes.Set1_9,
                             Optional YaxisLabel$ = "value",
                             Optional groupLabelCSSFont$ = CSSFont.Win7Large,
                             Optional YAxisLabelFontCSS$ = CSSFont.Win7Large,
                             Optional tickFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional regionStroke$ = Stroke.AxisStroke,
                             Optional interval# = 100,
                             Optional dotSize! = 10,
                             Optional lineWidth% = 2,
                             Optional rangeScale# = 1.25,
                             Optional showDataPoints As Boolean = True,
                             Optional showOutliers As Boolean = True,
                             Optional fillBox As Boolean = True,
                             Optional ppi As Integer = 100,
                             Optional driver As Drivers = Drivers.GDI) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .colorSet = schema,
                .lineStroke = regionStroke,
                .axisTickCSS = tickFontCSS,
                .axisLabelCSS = YAxisLabelFontCSS
            }
            Dim app As New Box(data, theme) With {
                .ylabel = YaxisLabel,
                .interval = interval,
                .fillBox = fillBox,
                .rangeScale = rangeScale,
                .lineWidth = lineWidth,
                .dotSize = dotSize,
                .showDataPoints = showDataPoints,
                .showOutliers = showOutliers
            }

            Return app.Plot(size.SizeParser, ppi, driver)
        End Function
    End Module
End Namespace
