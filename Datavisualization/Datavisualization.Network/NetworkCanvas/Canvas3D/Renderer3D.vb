#Region "Microsoft.VisualBasic::80411bef400ebf3870673bf625f5ae8b, ..\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\NetworkCanvas\Canvas3D\Renderer3D.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Transformation

Public Class Renderer3D : Inherits Renderer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="canvas"></param>
    ''' <param name="regionProvider"></param>
    ''' <param name="iForceDirected"><see cref="ForceDirected3D"/></param>
    Public Sub New(canvas As Func(Of Graphics), regionProvider As Func(Of Rectangle), iForceDirected As IForceDirected)
        Call MyBase.New(canvas, regionProvider, iForceDirected)
    End Sub

    Public Property rotate As Double = Math.PI / 3

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = __regionProvider()
        Dim pos1 As Point = SpaceToGrid(iPosition1.x, iPosition1.y, iPosition1.z, rotate)
        pos1 = GraphToScreen(pos1, rect)
        Dim pos2 As Point = SpaceToGrid(iPosition2.x, iPosition2.y, iPosition2.z, rotate)
        pos2 = GraphToScreen(pos2, rect)
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
        Dim pos As Point = SpaceToGrid(iPosition.x, iPosition.y, iPosition.z, rotate)
        Dim canvas As Graphics = __graphicsProvider()

        pos = GraphToScreen(pos, __regionProvider())

        SyncLock canvas
            Dim r As Single = radiushash(n)
            Dim pt As New Point(CInt(pos.X - r / 2), CInt(pos.Y - r / 2))
            Dim rect As New Rectangle(pt, New Size(CInt(r), CInt(r)))

            Call canvas.FillPie(n.Data.Color, rect, 0, 360)
        End SyncLock
    End Sub
End Class
