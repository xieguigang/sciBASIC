Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
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

        Friend Shared ReadOnly solidStroke As New Stroke(1.0F)

        Private _model As Cluster
        Private component As ClusterComponent

        Private scaleTickLabelPadding As Integer = 4

        Private xModelOrigin As Double = 0.0
        Private yModelOrigin As Double = 0.0
        Private wModel As Double = 0.0
        Private hModel As Double = 0.0

        Public Property ShowDistanceValues As Boolean = True
        Public Property ShowScale As Boolean = True
        Public Property ScalePadding As Integer = 10
        Public Property ScaleTickLength As Integer = 4
        Public Property ScaleValueInterval As Double
        Public Property ScaleValueDecimals As Integer

        Public Property BorderTop As Integer = 20
        Public Property BorderLeft As Integer = 20
        Public Property BorderRight As Integer = 20
        Public Property BorderBottom As Integer = 20

        Public Property LineColor As Color = Color.Black

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

        Private Function createComponent(cluster As Cluster, initCoord As PointF, clusterHeight As Double) As ClusterComponent
            Dim comp As ClusterComponent = Nothing

            If cluster IsNot Nothing Then

                comp = New ClusterComponent(cluster, cluster.Leaf, initCoord)
                Dim leafHeight As Double = clusterHeight / cluster.CountLeafs()
                Dim yChild As Double = initCoord.Y - (clusterHeight / 2)
                Dim distance As Double = cluster.DistanceValue

                For Each child As Cluster In cluster.Children
                    Dim childLeafCount As Integer = child.CountLeafs()
                    Dim childHeight As Double = childLeafCount * leafHeight
                    Dim childDistance As Double = child.DistanceValue
                    Dim childInitCoord As New PointF(initCoord.X + (distance - childDistance), yChild + childHeight / 2.0)
                    yChild += childHeight

                    ' Traverse cluster node tree 
                    Dim childComp As ClusterComponent = createComponent(child, childInitCoord, childHeight)

                    childComp.LinkPoint = initCoord
                    comp.Children.Add(childComp)
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

        Public Sub paint(g2 As Graphics2D)
            Dim size As Size = g2.Size
            Dim wDisplay As Integer = Size.Width - BorderLeft - BorderRight
            Dim hDisplay As Integer = Size.Height - BorderTop - BorderBottom
            Dim xDisplayOrigin As Integer = BorderLeft
            Dim yDisplayOrigin As Integer = BorderBottom

            g2.Stroke = solidStroke

            If component IsNot Nothing Then

                Dim nameGutterWidth As Integer = component.getMaxNameWidth(g2, False) + component.NamePadding
                wDisplay -= nameGutterWidth

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
                component.paint(g2, xOffset, yOffset, xFactor, yFactor, ShowDistanceValues)

                If ShowScale Then
                    Dim x1 As Integer = xDisplayOrigin
                    Dim y1 As Integer = yDisplayOrigin - ScalePadding
                    Dim x2 As Integer = x1 + wDisplay
                    Dim y2 As Integer = y1
                    g2.DrawLine(x1, y1, x2, y2)

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
            Else

                ' No data available 
                Dim str As String = "No data"
                Dim rect As RectangleF = g2.FontMetrics.GetStringBounds(str, g2.Graphics)
                Dim xt As Integer = CInt(Fix(wDisplay / 2.0 - rect.Width / 2.0))
                Dim yt As Integer = CInt(Fix(hDisplay / 2.0 - rect.Height / 2.0))
                g2.DrawString(str, xt, yt)
            End If
        End Sub
    End Class

End Namespace