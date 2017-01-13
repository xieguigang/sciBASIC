#Region "Microsoft.VisualBasic::5e52cedb6eb3bf192cd460c895e6d898, ..\sciBASIC#\gr\Datavisualization.Network\NetworkCanvas\Canvas3D\Renderer3D.vb"

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

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Transformation

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

    Public Property rotate As Double = Math.PI / 3

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = __regionProvider()
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
        Dim canvas As Graphics = __graphicsProvider()

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
        Dim r As Single = If(dynamicsRadius, n.Data.radius, radiushash(n))

        If r < 0.6 OrElse Single.IsNaN(r) OrElse r > 500 Then
            Return
        End If

        Dim client As Rectangle = __regionProvider()
        Dim pos As Point = New Point3D(iPosition.x, iPosition.y, iPosition.z) _
            .RotateX(rotate) _
            .RotateY(rotate) _
            .RotateZ(rotate) _
            .Project(client.Width, client.Height, 256, ViewDistance).PointXY
        Dim canvas As Graphics = __graphicsProvider()

        '   pos = GraphToScreen(pos, __regionProvider())

        SyncLock canvas
            Dim pt As New PointF(pos.X - r / 2, pos.Y - r / 2)
            Dim rect As New RectangleF(pt, New SizeF(r, r))

            Call canvas.FillPie(
                n.Data.Color,
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