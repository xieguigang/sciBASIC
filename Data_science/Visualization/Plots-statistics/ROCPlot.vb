#Region "Microsoft.VisualBasic::86e65c13723b511bfb3602fed4862819, Data_science\Visualization\Plots-statistics\ROCPlot.vb"

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

    '   Total Lines: 143
    '    Code Lines: 105 (73.43%)
    ' Comment Lines: 24 (16.78%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 14 (9.79%)
    '     File Size: 5.54 KB


    ' Module ROCPlot
    ' 
    '     Function: (+2 Overloads) CreateSerial, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Module ROCPlot

    ''' <summary>
    ''' x = <see cref="Validation.Specificity"/>;
    ''' y = <see cref="Validation.Sensibility"/>;
    ''' </summary>
    ''' <param name="test"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateSerial(test As IEnumerable(Of Validation)) As SerialData
        Dim points As New List(Of PointData)
        Dim testData As Validation() = test _
            .Where(Function(p)
                       Return Not p.Specificity.IsNaNImaginary AndAlso Not p.Sensibility.IsNaNImaginary
                   End Function) _
            .ToArray
        Dim AUC As Double = Validation.AUC(testData)

        points += New PointData(0, 0)
        points += testData _
            .Select(Function(pct)
                        Dim x! = (100 - pct.Specificity) / 100
                        Dim y! = pct.Sensibility / 100

                        Return New PointData(x, y)
                    End Function)
        points += New PointData(1, 1)

        Return New SerialData With {
            .color = Color.Black,
            .lineType = DashStyle.Solid,
            .pointSize = 5,
            .shape = LegendStyles.Triangle,
            .pts = points _
                .OrderBy(Function(p) p.pt.X) _
                .ToArray,
            .title = AUC.ToString("F2")
        }
    End Function

    ''' <summary>
    ''' This file should contains at least two fields: <see cref="Validation.Specificity"/> and <see cref="Validation.Sensibility"/>
    ''' </summary>
    ''' <param name="test"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateSerial(test As IEnumerable(Of DataSet)) As SerialData
        Dim data As DataSet() = test.ToArray
        Dim specificity = data(Scan0).Properties.Keys.First(Function(key) key.TextEquals(NameOf(Validation.Specificity)))
        Dim sensibility = data(Scan0).Properties.Keys.First(Function(key) key.TextEquals(NameOf(Validation.Sensibility)))

        Return data _
            .Select(Function(d)
                        Return New Validation With {
                            .Specificity = d(specificity),
                            .Sensibility = d(sensibility),
                            .Threshold = Val(d.ID)
                        }
                    End Function) _
            .CreateSerial
    End Function

    ''' <summary>
    ''' 这个函数所绘制出来的ROC曲线的AUC的值应该是在调用这个函数之前就完成计算,保存于 <see cref="SerialData.title"/> 之中了的
    ''' </summary>
    ''' <param name="roc"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="lineWidth!"></param>
    ''' <param name="fillAUC"></param>
    ''' <param name="referenceLineColor$"></param>
    ''' <param name="showReference"></param>
    ''' <returns></returns>
    Public Function Plot(roc As SerialData,
                         Optional size$ = "2300,2100",
                         Optional margin$ = g.DefaultUltraLargePadding,
                         Optional bg$ = "white",
                         Optional lineWidth! = 10,
                         Optional fillAUC As Boolean = True,
                         Optional referenceLineColor$ = "skyblue",
                         Optional showReference As Boolean = False,
                         Optional labelFontStyle$ = CSSFont.PlotTitleNormal,
                         Optional titleFontCSS$ = CSSFont.Win7VeryLarge,
                         Optional tickFontStyle$ = CSSFont.Win7LargerBold,
                         Optional dpi As Integer = 100,
                         Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim reference As New SerialData With {
            .color = referenceLineColor.TranslateColor,
            .lineType = DashStyle.Dash,
            .pointSize = 5,
            .width = lineWidth,
            .pts = {New PointData(0, 0), New PointData(1, 1)},
            .shape = LegendStyles.Circle
        }

        roc.width = lineWidth
        ' roc.color = AUCfillColor.TranslateColor
        roc.pts = roc.pts.OrderBy(Function(p) p.pt.Y).ToArray

        Dim input As SerialData()

        If showReference Then
            input = {reference, roc}
        Else
            input = {roc}
        End If

        Dim img As GraphicsData = Scatter.Plot(
            input,
            size:=size,
            padding:=margin,
            bg:=bg,
            interplot:=Splines.B_Spline,
            xlim:=1, ylim:=1,
            showLegend:=False,
            fill:=fillAUC,
            Xlabel:="1 - Specificity",
            Ylabel:="Sensibility",
            drawAxis:=True,
            htmlLabel:=False,
            title:=$"ROC (AUC={roc.title})",
            labelFontStyle:=labelFontStyle,
            tickFontStyle:=tickFontStyle,
            dpi:=dpi,
            titleFontCSS:=titleFontCSS,
            driver:=driver
        )

        Return img
    End Function
End Module
