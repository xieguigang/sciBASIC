#Region "Microsoft.VisualBasic::61cf7327f9c2a90624466d5c1cafcd3f, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\DendrogramPanel.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

' 
' *****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
' 
Namespace DendrogramVisualize

    Public Class DendrogramPanel

        Dim _model As Cluster
        ''' <summary>
        ''' Root node
        ''' </summary>
        Dim component As ClusterComponent

        Dim scaleTickLabelPadding% = 4

        Dim xModelOrigin As Double = 0.0
        Dim yModelOrigin As Double = 0.0
        Dim wModel As Double = 0.0
        Dim hModel As Double = 0.0

        Public Property ShowLeafLabel As Boolean = True
        Public Property ShowDistanceValues As Boolean = True
        Public Property ShowScale As Boolean = True
        Public Property ScalePadding As Integer = 10
        Public Property ScaleTickLength As Integer = 4
        Public Property ScaleValueInterval As Double
        Public Property ScaleValueDecimals As Integer
        Public Property LineColor As Color = Color.Black
        Public Property LinkDotRadius% = 5

        Public Property Model As Cluster
            Get
                Return _model
            End Get
            Set(model As Cluster)
                Me._model = model
                component = createComponent(model)
                updateModelMetrics()
            End Set
        End Property

        ''' <summary>
        ''' ``<see cref="Cluster.Label"/> --> Class Color``
        ''' </summary>
        ''' <returns></returns>
        Public Property ClassTable As Dictionary(Of String, String)

        Private Sub updateModelMetrics()
            Dim minX As Double = component.RectMinX
            Dim maxX As Double = component.RectMaxX
            Dim minY As Double = component.RectMinY
            Dim maxY As Double = component.RectMaxY

            xModelOrigin = minX
            yModelOrigin = minY
            wModel = maxX - minX
            hModel = maxY - minY
        End Sub

        ''' <summary>
        ''' 对整棵树进行递归生成layout信息
        ''' </summary>
        ''' <param name="cluster"></param>
        ''' <param name="pt0"></param>
        ''' <param name="clusterHeight"></param>
        ''' <returns></returns>
        Private Function createComponent(cluster As Cluster, pt0 As PointF, clusterHeight As Double) As ClusterComponent
            Dim comp As ClusterComponent = Nothing

            If cluster IsNot Nothing Then
                Dim leafHeight As Double = clusterHeight / cluster.CountLeafs()
                Dim yChild As Double = pt0.Y - (clusterHeight / 2)
                Dim distance As Double = cluster.DistanceValue

                comp = New ClusterComponent(cluster, cluster.IsLeaf, pt0)

                For Each child As Cluster In cluster.Childs
                    Dim childLeafCount As Integer = child.CountLeafs()
                    Dim childHeight As Double = childLeafCount * leafHeight
                    Dim childDistance As Double = child.DistanceValue
                    Dim childInitCoord As New PointF(pt0.X + (distance - childDistance), yChild + childHeight / 2.0)
                    yChild += childHeight

                    ' Traverse cluster node tree 
                    Dim childComp As ClusterComponent = createComponent(child, childInitCoord, childHeight)

                    childComp.LinkPoint = pt0
                    comp.Childs.Add(childComp)
                Next
            End If

            Return comp
        End Function

        Private Function createComponent(model As Cluster) As ClusterComponent
            Dim virtualModelHeight As Double = 1
            Dim initCoord As New PointF(0, virtualModelHeight / 2)
            Dim comp As ClusterComponent = createComponent(model, initCoord, virtualModelHeight)
            comp.LinkPoint = initCoord
            Return comp
        End Function

        ''' <summary>
        ''' Draw dendrogram tree visualize and returns the label orders
        ''' </summary>
        ''' <param name="g2"></param>
        ''' <param name="region"></param>
        ''' <param name="axisStrokeCSS$"></param>
        ''' <param name="branchStrokeCSS$"></param>
        ''' <param name="classLegendWidth%"></param>
        ''' <returns></returns>
        Public Function Paint(g2 As Graphics2D,
                              Optional region As Rectangle = Nothing,
                              Optional axisStrokeCSS$ = Stroke.AxisStroke,
                              Optional branchStrokeCSS$ = Stroke.AxisStroke,
                              Optional classLegendWidth% = 50) As NamedValue(Of PointF)()

            If region.IsEmpty Then
                region = g2.ImageResource.EntireImage
            End If

            Dim size As Size = region.Size
            ' 绘图区域的大小，width/height
            Dim wDisplay As Integer = size.Width
            Dim hDisplay As Integer = size.Height
            Dim xDisplayOrigin As Integer = region.Location.X
            Dim yDisplayOrigin As Integer = region.Location.Y

            ' 设置默认的笔对象
            g2.Stroke = Stroke.TryParse(axisStrokeCSS)

            ' 如果cluster的结果不为空
            If component IsNot Nothing Then
                Return __draw(g2,
                    wDisplay, hDisplay, xDisplayOrigin, yDisplayOrigin,
                    stroke:=Stroke.TryParse(branchStrokeCSS),
                    classLegendWidth:=classLegendWidth)
            Else
                ' No data available 
                Dim str As String = "No data"
                Dim rect As RectangleF = g2.FontMetrics.GetStringBounds(str, g2.Graphics)
                Dim xt As Integer = CInt(Fix(wDisplay / 2.0 - rect.Width / 2.0))
                Dim yt As Integer = CInt(Fix(hDisplay / 2.0 - rect.Height / 2.0))

                g2.DrawString(str, xt, yt)

                Return {}
            End If
        End Function

        Private Function __draw(g2 As Graphics2D, wDisplay%, hDisplay%, xDisplayOrigin%, yDisplayOrigin%, stroke As Stroke, classLegendWidth%) As NamedValue(Of PointF)()

            If ShowLeafLabel Then
                Dim nameGutterWidth% = component.GetMaxNameWidth(g2, False) + component.NamePadding
                wDisplay -= nameGutterWidth
            End If

            If ShowScale Then
                Dim rect As RectangleF = g2.FontMetrics.GetStringBounds("0", g2.Graphics)
                Dim scaleHeight As Integer = rect.Height + ScalePadding + ScaleTickLength + scaleTickLabelPadding
                hDisplay -= scaleHeight
                yDisplayOrigin += scaleHeight
            End If

            ' Calculate conversion factor and offset for display 
            Dim xFactor As Double = wDisplay / wModel
            Dim yFactor As Double = hDisplay / hModel
            Dim xOffset As Integer = CInt(Fix(xDisplayOrigin - xModelOrigin * xFactor))
            Dim yOffset As Integer = CInt(Fix(yDisplayOrigin - yModelOrigin * yFactor))
            Dim classHeight! = (1 / component.Cluster.CountLeafs) * yFactor
            Dim padding% = If(
                ClassTable.IsNullOrEmpty,
                -1,
                g2.MeasureString(ClassTable.Keys.MaxLengthString).Width + 10)
            Dim legendHeight% = hDisplay / If(ClassTable.IsNullOrEmpty, 1, ClassTable.Count - 1) ' 绘图区域的高度除以个数
            Dim args As New PainterArguments With {
                .xDisplayOffset = xOffset,
                .yDisplayOffset = yOffset,
                .xDisplayFactor = xFactor,
                .yDisplayFactor = yFactor,
                .decorated = ShowDistanceValues,
                .classHeight = classHeight,
                .classTable = ClassTable,
                .stroke = stroke,
                .classLegendSize = New Size(classLegendWidth, legendHeight),
                .classLegendPadding = padding,
                .ShowLabelName = ShowLeafLabel,
                .LinkDotRadius = LinkDotRadius
            }
            Dim labels As New List(Of NamedValue(Of PointF))

            ' 从这里开始进行递归的绘制出整个进化树
            Call component.paint(g2, args, labels)

            ' 在这里进行标尺的绘制
            If ShowScale Then
                Dim x1 As Integer = xDisplayOrigin
                Dim y1 As Integer = yDisplayOrigin - ScalePadding
                Dim x2 As Integer = x1 + wDisplay
                Dim y2 As Integer = y1

                Call g2.DrawLine(x1, y1, x2, y2)

                Dim totalDistance As Double = component.Cluster.TotalDistance
                Dim xModelInterval As Double

                If ScaleValueInterval <= 0 Then
                    xModelInterval = totalDistance / 10.0
                Else
                    xModelInterval = ScaleValueInterval
                End If

                Dim xTick As Integer = xDisplayOrigin + wDisplay
                y1 = yDisplayOrigin - ScalePadding
                y2 = yDisplayOrigin - ScalePadding - ScaleTickLength
                Dim distanceValue As Double = 0
                Dim xDisplayInterval As Double = xModelInterval * xFactor

                Do While xTick >= xDisplayOrigin

                    ' 绘制坐标轴的Tick竖线
                    Call g2.DrawLine(xTick, y1, xTick, y2)

                    Dim distanceValueStr As String = sprintf("%." & ScaleValueDecimals & "f", distanceValue)
                    Dim rect As RectangleF = g2.FontMetrics.GetStringBounds(distanceValueStr, g2.Graphics)
                    g2.DrawString(distanceValueStr, CInt(Fix(xTick - (rect.Width / 2))), y2 - scaleTickLabelPadding - rect.Height)
                    xTick -= xDisplayInterval
                    distanceValue += xModelInterval
                Loop
            End If

            Return labels
        End Function
    End Class
End Namespace
