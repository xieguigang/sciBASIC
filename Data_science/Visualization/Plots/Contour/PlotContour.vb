#Region "Microsoft.VisualBasic::994b710994ae9569743c9357302bcee2, Data_science\Visualization\Plots\Contour\PlotContour.vb"

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

    '   Total Lines: 71
    '    Code Lines: 63 (88.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (11.27%)
    '     File Size: 3.23 KB


    '     Module PlotContour
    ' 
    '         Function: (+2 Overloads) Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Contour

    Public Module PlotContour

        <Extension>
        Public Function Plot(sample As IEnumerable(Of MeasureData),
                             Optional size$ = "3600,2400",
                             Optional padding$ = "padding:100px 400px 100px 100px;",
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional legendTitle$ = "Contour Levels",
                             Optional legendTitleCSS$ = CSSFont.Win7LargeBold,
                             Optional tickCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickAxisStroke$ = Stroke.ScatterLineStroke,
                             Optional interpolateFill As Boolean = True,
                             Optional ppi% = 300) As GraphicsData

            Dim contours As GeneralPath() = ContourLayer.GetContours(sample, interpolateFill:=interpolateFill).ToArray
            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .colorSet = colorSet,
                .legendTitleCSS = legendTitleCSS,
                .legendTickCSS = tickCSS,
                .legendTickAxisStroke = tickAxisStroke
            }
            Dim plotApp As New ContourPlot(contours, theme) With {
                .legendTitle = legendTitle
            }

            Return plotApp.Plot(size, ppi)
        End Function

        <Extension>
        Public Function Plot(sample As IEnumerable(Of ContourLayer),
                             Optional size$ = "2700,2000",
                             Optional padding$ = "padding:100px 400px 100px 100px;",
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional legendTitle$ = "Contour Levels",
                             Optional legendTitleCSS$ = CSSFont.Win7LargeBold,
                             Optional tickCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickAxisStroke$ = Stroke.ScatterLineStroke,
                             Optional xlim As Double = Double.NaN,
                             Optional ylim As Double = Double.NaN,
                             Optional ppi% = 300) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .colorSet = colorSet,
                .legendTitleCSS = legendTitleCSS,
                .legendTickCSS = tickCSS,
                .legendTickAxisStroke = tickAxisStroke
            }
            Dim plotApp As New ContourPlot(sample, theme) With {
                .legendTitle = legendTitle,
                .xlim = xlim,
                .ylim = ylim
            }

            Return plotApp.Plot(size, ppi)
        End Function
    End Module
End Namespace
