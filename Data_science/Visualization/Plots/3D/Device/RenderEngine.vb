#Region "Microsoft.VisualBasic::94a448882461c33dbc45413a67901d75, Data_science\Visualization\Plots\3D\Device\RenderEngine.vb"

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

    '   Total Lines: 138
    '    Code Lines: 102 (73.91%)
    ' Comment Lines: 20 (14.49%)
    '    - Xml Docs: 65.00%
    ' 
    '   Blank Lines: 16 (11.59%)
    '     File Size: 5.94 KB


    '     Module RenderEngine
    ' 
    '         Sub: drawLabels, RenderAs3DChart
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
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
#End If

Namespace Plot3D.Device

    ''' <summary>
    ''' 将生成的绘图元素在这个引擎模块之中进行排序操作，然后进行图表的绘制
    ''' </summary>
    Public Module RenderEngine

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="elements"></param>
        ''' <param name="canvas"></param>
        ''' <param name="camera"></param>
        ''' <param name="region"></param>
        <Extension>
        Public Sub RenderAs3DChart(elements As IEnumerable(Of Element3D),
                                   canvas As IGraphics,
                                   camera As Camera,
                                   region As GraphicsRegion,
                                   theme As Theme)

            ' 首先对模型执行rotate和project，然后再进行Z排序
            Dim models As Element3D() = elements.ToArray

            For Each element As Element3D In models
                Call element.Transform(camera)
            Next

            ' 进行投影之后只需要直接取出XY即可得到二维的坐标
            ' 然后生成多边形，进行画布的居中处理
            Dim plotRect As Rectangle = region.PlotRegion
            Dim polygon As PointF() = models _
                .Select(Function(element) element.EnumeratePath) _
                .IteratesALL _
                .Select(Function(pt) pt.PointXY(plotRect.Size)) _
                .ToArray
            Dim scaleX = d3js.scale.linear.domain(polygon.Select(Function(a) a.X)).range(values:=New Double() {plotRect.Left, plotRect.Right})
            Dim scaleY = d3js.scale.linear.domain(polygon.Select(Function(a) a.Y)).range(values:=New Double() {plotRect.Top, plotRect.Bottom})
            Dim orders = PainterAlgorithm _
                .OrderProvider(models, Function(e) e.Location.Z) _
                .ToArray
            Dim labels As New List(Of d3js.Layout.Label)
            Dim anchors As New List(Of d3js.Layout.Anchor)
            Dim location As PointF
            Dim labelSize As SizeF
            Dim css As CSSEnvirnment = canvas.LoadEnvironment
            Dim labelFont As Font = css.GetFont(CSSFont.TryParse(theme.tagCSS))

            For i As Integer = 0 To models.Length - 1
                Dim index As Integer = orders(i)
                Dim model As Element3D = models(index)

                Call model.Draw(canvas, region, scaleX, scaleY)

                If TypeOf model Is ShapePoint Then
                    ' 如果是一个数据点，则还需要将标签数据模型拿出来
                    ' 准备进行当前数据点的文本标签的绘制
                    With DirectCast(model, ShapePoint)
                        If Not .Label.StringEmpty Then
                            location = .GetPosition(plotRect.Size)
                            location = New PointF(scaleX(location.X), scaleY(location.Y))
                            labelSize = canvas.MeasureString(.Label, labelFont)
                            labels += New d3js.Layout.Label(
                                label:= .Label,
                                pos:=location,
                                size:=labelSize
                            )
                            anchors += New d3js.Layout.Anchor(location, 2)
                        End If
                    End With
                End If
            Next

            If theme.drawLabels AndAlso labels > 0 Then
                Call labels.ToArray _
                    .drawLabels(
                        anchors:=anchors,
                        canvas:=canvas,
                        graphicsRegion:=region,
                        Font:=labelFont,
                        labelerItr:=100,
                        labelColor:=theme.tagColor.GetBrush
                    )
            End If
        End Sub

        ''' <summary>
        ''' layout and then draw labels
        ''' </summary>
        ''' <param name="labels"></param>
        <Extension>
        Private Sub drawLabels(labels As d3js.Layout.Label(), anchors As d3js.Layout.Anchor(),
                               canvas As IGraphics,
                               graphicsRegion As GraphicsRegion,
                               Font As Font,
                               labelerItr%,
                               labelColor As SolidBrush)

            Dim label As d3js.Layout.Label
            Dim anchor As d3js.Layout.Anchor
            Dim labelAnchor As Point

            If labelerItr > 0 Then
                ' Generate label layouts
                Call d3js.labeler() _
                    .Labels(labels) _
                    .Anchors(anchors) _
                    .Width(canvas.Size.Width) _
                    .Height(canvas.Size.Height) _
                    .Start(labelerItr%, showProgress:=False)
            End If

            For i As Integer = 0 To labels.Length - 1
                label = labels(i)
                anchor = anchors(i)
                labelAnchor = label.GetTextAnchor(anchor)

                Call canvas.DrawString(label.text, Font, labelColor, label.location)

                If labelerItr > 0 Then
                    Call canvas.DrawLine(Pens.LightGray, labelAnchor, anchor)
                End If
            Next
        End Sub
    End Module
End Namespace
