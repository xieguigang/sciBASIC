#Region "Microsoft.VisualBasic::9b47f1004bf8bde6675cd0b0c22f9de1, gr\network-visualization\NetworkCanvas\Canvas3D\Renderer3D.vb"

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

    '   Total Lines: 118
    '    Code Lines: 91 (77.12%)
    ' Comment Lines: 6 (5.08%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 21 (17.80%)
    '     File Size: 4.66 KB


    ' Class Renderer3D
    ' 
    '     Properties: rotate, ViewDistance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: drawEdge, drawNode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Public Class Renderer3D : Inherits Renderer
    Implements IGraphicsEngine

    Public Property ViewDistance As Double = -220

    Dim dynamicsRadius As Boolean

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="canvas"></param>
    ''' <param name="regionProvider"></param>
    ''' <param name="iForceDirected"><see cref="ForceDirected3D"/></param>
    Public Sub New(canvas As Func(Of IGraphics),
                   regionProvider As Func(Of Rectangle),
                   iForceDirected As IForceDirected,
                   Optional dynamicsRadius As Boolean = False)

        Call MyBase.New(canvas, regionProvider, iForceDirected)
        Me.dynamicsRadius = dynamicsRadius
    End Sub

    Public Property rotate As Double = std.PI / 3

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = regionProvider()
        Dim pos1 As PointF = New Point3D(iPosition1.x, iPosition1.y, iPosition1.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(rect.Width, rect.Height, 256, ViewDistance) _
            .PointXY
        Dim pos2 As PointF = New Point3D(iPosition2.x, iPosition2.y, iPosition2.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(rect.Width, rect.Height, 256, ViewDistance) _
            .PointXY

        If Renderer.IsOffscreen(pos1, pos2, rect) Then
            Return
        End If

        Dim canvas As IGraphics = frameCanvas

        Try
            If View.IsHighlighted(iEdge.U) OrElse View.IsHighlighted(iEdge.V) Then
                Dim w As Single = edgeStyles(iEdge).Width + 1.5F
                Using hp As New Microsoft.VisualBasic.Imaging.Pen(System.Drawing.Color.Orange, w)
                    canvas.DrawLine(hp, pos1.X, pos1.Y, pos2.X, pos2.Y)
                End Using
            Else
                canvas.DrawLine(edgeStyles(iEdge), pos1.X, pos1.Y, pos2.X, pos2.Y)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Overrides Sub drawNode(n As Node, iPosition As AbstractVector)
        Dim r As Single = If(dynamicsRadius, n.data.size(0), radiushash(n))

        If r < 0.6 OrElse Single.IsNaN(r) OrElse r > 500 Then
            Return
        End If

        Dim client As Rectangle = regionProvider()
        Dim pos As PointF = New Point3D(iPosition.x, iPosition.y, iPosition.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(client.Width, client.Height, 256, ViewDistance) _
            .PointXY ' 调整FOV参数的效果不太好

        If Renderer.IsOffscreen(pos, pos, client) Then
            Return
        End If

        Dim canvas As IGraphics = frameCanvas

        Try
            If View.IsHighlighted(n) Then
                Dim rr As Single = r + 3.0F
                Dim ring = If(View.Hovered Is n AndAlso Not View.Selected.Contains(n), hoverBrush, selectBrush)
                canvas.FillPie(ring, pos.X - rr / 2.0F, pos.Y - rr / 2.0F, rr, rr, 0.0F, 360.0F)
            End If

            Dim pt As New PointF(pos.X - r / 2, pos.Y - r / 2)
            Dim rect As New RectangleF(pt, New SizeF(r, r))

            Call canvas.FillPie(
                n.data.color,
                rect.X, rect.Y, rect.Width, rect.Height,
                0.0F, 360.0F)

            If View.ShouldDrawLabels(forceDirected.graph.vertex.Count, 1.0F) Then
                Dim labeltext As String = n.data.label
                If Not String.IsNullOrEmpty(labeltext) Then
                    Dim sz As SizeF = canvas.MeasureString(labeltext, Font)
                    Dim center As PointF = New PointF(rect.Centre.X - sz.Width / 2, rect.Centre.Y - sz.Height / 2)
                    Call canvas.DrawString(labeltext, Font, Brushes.Gray, center)
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub
End Class
