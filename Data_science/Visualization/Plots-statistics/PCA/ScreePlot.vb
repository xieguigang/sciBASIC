#Region "Microsoft.VisualBasic::f8f869da9afd2a09f7f94c9d7397a67d, Data_science\Visualization\Plots-statistics\PCA\ScreePlot.vb"

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

    '   Total Lines: 67
    '    Code Lines: 58 (86.57%)
    ' Comment Lines: 3 (4.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (8.96%)
    '     File Size: 3.17 KB


    '     Module ScreePlot
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace PCA

    ''' <summary>
    ''' 碎石图
    ''' </summary>
    Public Module ScreePlot

        <Extension>
        Public Function Plot(pca As MultivariateAnalysisResult,
                             Optional size$ = "3300,2700",
                             Optional margin$ = g.DefaultUltraLargePadding,
                             Optional bg$ = "white",
                             Optional lineStroke$ = Stroke.HighlightStroke,
                             Optional pointSize! = 10,
                             Optional title$ = "PCA ScreePlot",
                             Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                             Optional tickFontStyle$ = CSSFont.Win7Large,
                             Optional labelFontStyle$ = CSSFont.Win7VeryLarge,
                             Optional axisStrokeCSS$ = Stroke.AxisStroke) As GraphicsData

            Dim cv As Vector = pca.Contributions.AsVector
            Dim X$() = cv.Dim _
                .SeqIterator(offset:=1) _
                .Select(Function(i) $"Comp.{i}") _
                .ToArray
            Dim Y As Vector = cv.CreateAxisTicks
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim rect As Rectangle = region.PlotRegion
                    Dim Xscaler = d3js.scale.ordinal.domain(tags:=X).range(integers:={rect.Left, rect.Right})
                    Dim Yscaler = d3js.scale.linear.domain(values:=Y).range(integers:={rect.Top, rect.Bottom})
                    Dim scaler As New TermScaler With {
                        .AxisTicks = (X, Y),
                        .X = Xscaler,
                        .Y = Yscaler,
                        .region = rect
                    }
                    Dim labelColor As Brush = CSSFont.TryParse(labelFontStyle).color.GetBrush
                    Dim tickColor As Brush = CSSFont.TryParse(tickFontStyle).color.GetBrush
                    Dim css As CSSEnvirnment = g.LoadEnvironment

                    Call g.DrawY(css.GetPen(Stroke.TryParse(axisStrokeCSS)), "Variances", scaler, -1, Y, YAxisLayoutStyles.Left, Nothing, labelFontStyle, labelColor,
                                 css.GetFont(CSSFont.TryParse(tickFontStyle)), tickColor,
                                 htmlLabel:=False,
                                 tickFormat:="F2")
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, margin, bg,
                plotInternal
            )
        End Function
    End Module
End Namespace
