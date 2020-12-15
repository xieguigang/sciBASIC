#Region "Microsoft.VisualBasic::39916c9fb2973927f3d6a905a2990c73, Data_science\Visualization\Plots\3D\Device\RenderEngine.vb"

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

'     Module RenderEngine
' 
'         Sub: drawLabels, RenderAs3DChart
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language

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
        ''' <param name="labelFont">Font style that will draw of the elements' label content.</param>
        <Extension>
        Public Sub RenderAs3DChart(elements As IEnumerable(Of Element3D),
                                   canvas As IGraphics,
                                   camera As Camera,
                                   region As GraphicsRegion,
                                   labelFont As Font,
                                   labelerItr%,
                                   showLabel As Boolean)

            ' 首先对模型执行rotate和project，然后再进行Z排序
            Dim models As Element3D() = elements.ToArray

            For Each element As Element3D In models
                Call element.Transform(camera)
            Next

            ' 进行投影之后只需要直接取出XY即可得到二维的坐标
            ' 然后生成多边形，进行画布的居中处理
            Dim polygon As Point() = models _
                .Select(Function(element) element.GetPosition(canvas)) _
                .ToArray
            Dim centra As PointF = polygon.CentralOffset(canvas.Size)
            Dim orders = PainterAlgorithm _
                .OrderProvider(models, Function(e) e.Location.Z) _
                .ToArray
            Dim labels As New List(Of d3js.Layout.Label)
            Dim anchors As New List(Of d3js.Layout.Anchor)
            Dim location As Point
            Dim labelSize As SizeF

            For i As Integer = 0 To models.Length - 1
                Dim index As Integer = orders(i)
                Dim model As Element3D = models(index)

                Call model.Draw(canvas, centra)

                If TypeOf model Is ShapePoint Then
                    With DirectCast(model, ShapePoint)
                        If Not .Label.StringEmpty Then
                            location = .Point2D(canvas).OffSet2D(centra)
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

            If showLabel AndAlso labels > 0 Then
                Call labels.ToArray.drawLabels(
                    anchors:=anchors,
                    canvas:=canvas,
                    graphicsRegion:=region,
                    Font:=labelFont,
                    labelerItr:=labelerItr
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
                               labelerItr%)

            Dim label As d3js.Layout.Label
            Dim anchor As d3js.Layout.Anchor
            Dim labelAnchor As Point

            ' Generate label layouts
            Call d3js.labeler() _
                .Labels(labels) _
                .Anchors(anchors) _
                .Width(canvas.Size.Width) _
                .Height(canvas.Size.Height) _
                .Start(labelerItr%, showProgress:=False)

            For i As Integer = 0 To labels.Length - 1
                label = labels(i)
                anchor = anchors(i)
                labelAnchor = label.GetTextAnchor(anchor)

                Call canvas.DrawString(label.text, Font, Brushes.Black, label.location)
                Call canvas.DrawLine(Pens.LightGray, labelAnchor, anchor)
            Next
        End Sub
    End Module
End Namespace
