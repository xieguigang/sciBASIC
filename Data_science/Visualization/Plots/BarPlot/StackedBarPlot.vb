#Region "Microsoft.VisualBasic::ba1566cdca57b99f7a2d559eaaaae073, Data_science\Visualization\Plots\BarPlot\StackedBarPlot.vb"

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

    '   Total Lines: 91
    '    Code Lines: 65 (71.43%)
    ' Comment Lines: 16 (17.58%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (10.99%)
    '     File Size: 3.73 KB


    '     Module StackedBarPlot
    ' 
    '         Function: BarWidth, loadBrushes, Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace BarPlot

    Public Module StackedBarPlot

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BarWidth(regionWidth%, n%, interval#) As Single
            Return (regionWidth - (n - 1) * interval) / n
        End Function

        <Extension>
        Friend Iterator Function loadBrushes(data As BarDataGroup) As IEnumerable(Of NamedValue(Of SolidBrush))
            For Each s In data.Serials
#Disable Warning CA1416 ' 验证平台兼容性
                Yield New NamedValue(Of SolidBrush) With {
                    .Name = s.Name,
                    .Value = New SolidBrush(s.Value)
                }
#Enable Warning CA1416 ' 验证平台兼容性
            Next
        End Function

        ''' <summary>
        ''' 绘制百分比堆积的条形图
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="percentStacked!"></param>
        ''' <param name="YaxisTitle$"></param>
        ''' <param name="interval!"></param>
        ''' <param name="columnCount%"></param>
        ''' <param name="legendLabelFontCSS$"></param>
        ''' <param name="tickFontCSS$"></param>
        ''' <param name="groupLabelFontCSS$"></param>
        ''' <param name="axisLabelFontCSS$"></param>
        ''' <returns></returns>
        Public Function Plot(data As BarDataGroup,
                             Optional size$ = "3000,2700",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional percentStacked! = no,
                             Optional YaxisTitle$ = "Value",
                             Optional interval! = 5,
                             Optional boxSeperator! = 5,
                             Optional columnCount% = 8,
                             Optional legendLabelFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional tickFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional groupLabelFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional axisLabelFontCSS$ = CSSFont.Win7Large) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .axisLabelCSS = axisLabelFontCSS,
                .axisTickCSS = tickFontCSS,
                .legendLabelCSS = legendLabelFontCSS,
                .legendTitleCSS = groupLabelFontCSS
            }
            Dim app As New StackedPercentageBarPlot(data, theme) With {
                .ylabel = YaxisTitle,
                .interval = interval,
                .columnCount = columnCount,
                .boxSeperator = boxSeperator
            }

            If percentStacked = no Then

            Else

            End If

            Return app.Plot(size)
        End Function
    End Module
End Namespace
