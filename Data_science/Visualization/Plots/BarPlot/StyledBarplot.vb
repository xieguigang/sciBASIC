#Region "Microsoft.VisualBasic::dfc42d78b628a1b234b7c12333f7d741, Data_science\Visualization\Plots\BarPlot\StyledBarplot.vb"

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

    '   Total Lines: 165
    '    Code Lines: 123 (74.55%)
    ' Comment Lines: 26 (15.76%)
    '    - Xml Docs: 73.08%
    ' 
    '   Blank Lines: 16 (9.70%)
    '     File Size: 6.91 KB


    '     Module StyledBarplot
    ' 
    '         Function: ColorRendering, Plot
    ' 
    '         Sub: plotInternal
    '         Structure BarSerial
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.MIME.Html.Render


#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace BarPlot

    Public Module StyledBarplot

        Public Structure BarSerial
            Dim Label$
            Dim Value#
            ''' <summary>
            ''' 颜色表达式或者图片资源的文件路径
            ''' </summary>
            Dim Brush$

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure

        ''' <summary>
        ''' 进行更加复杂的样式的条形图的绘图操作
        ''' </summary>
        ''' <param name="data">这个数据集之中是没有同组比较数据的</param>
        ''' <param name="size"></param>
        ''' <param name="padding$"></param>
        ''' <param name="interval">The interval distance between the bars, percentage of the width</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As IEnumerable(Of BarSerial),
                             Optional size$ = "1024,800",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional interval# = 0.085,
                             Optional labelFont$ = CSSFont.Win10Normal,
                             Optional shadowOffset% = 4) As GraphicsData

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                bg,
                Sub(ByRef g, region)
                    Call data _
                        .ToArray _
                        .plotInternal(g, region,
                                        interval:=interval * region.PlotRegion(g.LoadEnvironment).Width,
                                        labelFont:=labelFont,
                                        shadowOffset:=shadowOffset)
                End Sub)
        End Function

        <Extension>
        Private Sub plotInternal(data As BarSerial(),
                                 g As IGraphics, region As GraphicsRegion,
                                 interval%,
                                 labelFont$,
                                 shadowOffset%)

            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim padding As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, region.Padding)
            Dim scaler As New Mapper(
                range:=New Scaling(data.Select(Function(o) o.Value), horizontal:=False),
                ignoreX:=True)
            Dim rect As Rectangle = region.PlotRegion(css)
            Dim bWidth% = (rect.Width - data.Length * interval) / data.Length
            Dim bTop%
            Dim hScaler As Func(Of Single, Single) = scaler.YScaler(region.Size, region.Padding)
            Dim bLeft% = padding.Left + interval / 2
            Dim bRECT As Rectangle   ' shadow rectangle
            Dim barRECT As Rectangle
            Dim label As Image
            Dim labelLeft%

            With region
                ' Call g.DrawAxis(.Size, .Padding, scaler, showGrid:=True)
            End With

            For Each s As BarSerial In data
                bTop = hScaler(s.Value)
                bRECT = BarPlotAPI.Rectangle(bTop, bLeft, bLeft + bWidth, rect.Bottom)
                barRECT = New Rectangle(bRECT.Location.OffSet2D(-2, -2), bRECT.Size)

                ' Draw shadows
                g.FillRectangle(Brushes.Gray, bRECT)
                ' Draw bar
                g.FillRectangle(s.Brush.GetBrush, barRECT)
                ' Draw label
                ' label = TextRender.DrawHtmlText(s.Label, cssFont:=labelFont)
                ' rotate -90
                ' label = label.RotateImage(-90)
                Throw New NotImplementedException
                labelLeft = bLeft + (bWidth - label.Width) / 2
                g.DrawImageUnscaled(label, New Point(labelLeft, region.Bottom(css) + 20))

                bLeft += interval + bWidth
            Next
        End Sub

        ''' <summary>
        ''' 使用这个函数进行条形图之中的系列的颜色的渲染
        ''' </summary>
        ''' <param name="data">
        ''' 系列数据，其中的<see cref="BarSerial.Brush"/>将会被填充为颜色谱之中的某一个颜色值
        ''' </param>
        ''' <param name="schema$"><see cref="Designer"/>的颜色谱名称</param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorRendering(data As IEnumerable(Of BarSerial), schema$) As BarSerial()
            Dim array As BarSerial() = data.ToArray
            Dim colors As Color() = Designer.GetColors(schema, array.Length)
            Dim out As BarSerial() = LinqAPI.Exec(Of BarSerial) <=
 _
                From ls As SeqValue(Of BarSerial)
                In array.SeqIterator
                Let color As String = colors(ls).ToHtmlColor
                Let r As BarSerial = (+ls)
                Select New BarSerial With {
                    .Brush = color,
                    .Label = r.Label,
                    .Value = r.Value
                }

            Return out
        End Function
    End Module
End Namespace
