#Region "Microsoft.VisualBasic::94024f54b951cc01ce8fa295a5ae7f4b, Data_science\Visualization\Plots-statistics\Zscore\ZScores.vb"

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

    '   Total Lines: 42
    '    Code Lines: 34
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 1.89 KB


    ' Module ZScoresPlots
    ' 
    '     Function: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' Plot of the <see cref="Distributions.Z"/>
''' </summary>
Public Module ZScoresPlots

    <Extension>
    Public Function Plot(data As ZScores,
                         Optional size$ = "3300,3400",
                         Optional margin$ = Canvas.Resolution2K.PaddingWithRightLegendAndBottomTitle,
                         Optional bg$ = "white",
                         Optional title$ = "Z-scores",
                         Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional serialLabelFontCSS$ = CSSFont.Win7VeryLarge,
                         Optional legendLabelFontCSS$ = CSSFont.Win7VeryLarge,
                         Optional tickFontCSS$ = CSSFont.Win7LargeBold,
                         Optional pointWidth! = 50,
                         Optional axisStrokeCSS$ = Stroke.AxisStroke,
                         Optional legendBoxStroke$ = Stroke.AxisStroke,
                         Optional displayZERO As Boolean = True,
                         Optional ZEROStrokeCSS$ = Stroke.AxisGridStroke,
                         Optional ppi As Integer = 100) As GraphicsData



    End Function
End Module
