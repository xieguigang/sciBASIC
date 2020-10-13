#Region "Microsoft.VisualBasic::f7856822bfc7d5d655c0560775dfdf84, gr\network-visualization\NetworkCanvas\Canvas3D\Renderer3D.vb"

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
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports stdNum = System.Math

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
    Public Sub New(canvas As Func(Of Graphics),
                   regionProvider As Func(Of Rectangle),
                   iForceDirected As IForceDirected,
                   Optional dynamicsRadius As Boolean = False)

        Call MyBase.New(canvas, regionProvider, iForceDirected)
        Me.dynamicsRadius = dynamicsRadius
    End Sub

    Public Property rotate As Double = stdNum.PI / 3

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = regionProvider()
        Dim pos1 As Point = New Point3D(iPosition1.x, iPosition1.y, iPosition1.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(rect.Width, rect.Height, 256, ViewDistance).PointXY
        '   pos1 = GraphToScreen(pos1, rect)
        Dim pos2 As Point = New Point3D(iPosition2.x, iPosition2.y, iPosition2.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(rect.Width, rect.Height, 256, ViewDistance).PointXY
        '   pos2 = GraphToScreen(pos2, rect)
        Dim canvas As Graphics = graphicsProvider()

        SyncLock canvas
            Dim w As Single = widthHash(iEdge)
            Dim LineColor As New Pen(Color.Gray, w)

            Call canvas.DrawLine(
                LineColor,
                pos1.X,
                pos1.Y,
                pos2.X,
                pos2.Y)
        End SyncLock
    End Sub

    Protected Overrides Sub drawNode(n As Node, iPosition As AbstractVector)
        Dim r As Single = If(dynamicsRadius, n.data.size(0), radiushash(n))

        If r < 0.6 OrElse Single.IsNaN(r) OrElse r > 500 Then
            Return
        End If

        Dim client As Rectangle = regionProvider()
        Dim pos As Point = New Point3D(iPosition.x, iPosition.y, iPosition.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(client.Width, client.Height, 256, ViewDistance) _
            .PointXY ' 调整FOV参数的效果不太好
        Dim canvas As Graphics = graphicsProvider()

        '   pos = GraphToScreen(pos, __regionProvider())

        SyncLock canvas
            Dim pt As New PointF(pos.X - r / 2, pos.Y - r / 2)
            Dim rect As New RectangleF(pt, New SizeF(r, r))

            Call canvas.FillPie(
                n.data.color,
                rect.X, rect.Y, rect.Width, rect.Height,
                0!, 360.0!)

            If ShowLabels Then
                Dim center As PointF = rect.Centre
                Dim sz As SizeF = canvas.MeasureString(n.ID, Font)
                center = New PointF(
                    center.X - sz.Width / 2,
                    center.Y - sz.Height / 2)
                Call canvas.DrawString(n.ID, Font, Brushes.Gray, center)
            End If
        End SyncLock
    End Sub
End Class
