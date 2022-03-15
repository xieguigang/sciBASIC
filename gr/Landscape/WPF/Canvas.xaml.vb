#Region "Microsoft.VisualBasic::a7b2ff8821b7ceed5e2583048ebacbb5, sciBASIC#\gr\Landscape\WPF\Canvas.xaml.vb"

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

    '   Total Lines: 112
    '    Code Lines: 89
    ' Comment Lines: 2
    '   Blank Lines: 21
    '     File Size: 4.28 KB


    ' Class Canvas
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: BuildSolid, Button_Click, Grid_MouseDown, Grid_MouseMove, Grid_MouseUp
    '          Grid_MouseWheel
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape.Vendor_3mf
Imports Microsoft.VisualBasic.Language
Imports Vertex = System.Windows.Media.Media3D.Point3D
Imports stdNum = System.Math

Public Class Canvas

    Dim __geometry As New List(Of GeometryModel3D)
    Dim __drag As Boolean
    Dim __lastPos As Point

    Public Sub New()
        Call InitializeComponent()
        Call BuildSolid(Vendor_3mf.IO.Open("G:\sciBASIC\gr\build_3DEngine\models\WOW_rthas the Lich King1.3mf"))
    End Sub

    Public Sub BuildSolid(project As Project)
        For Each obj In project.model.resources.objects.Where(Function(o) Not o.mesh Is Nothing)
            ' Define 3D mesh object
            Dim mesh As New MeshGeometry3D()

            For Each v In obj.mesh.vertices
                Call mesh.Positions.Add(New Vertex(v.X, v.Y, v.Z))
            Next

            For Each t In obj.mesh.triangles
                Call mesh.TriangleIndices.Add(t.v1)
                Call mesh.TriangleIndices.Add(t.v2)
                Call mesh.TriangleIndices.Add(t.v3)
            Next

            ' Geometry creation
            Dim geometry As New GeometryModel3D(
                mesh,
                New DiffuseMaterial(Brushes.YellowGreen)) With {
                .Transform = New Transform3DGroup()
            }

            Call __geometry.Add(geometry)
            Call group.Children.Add(geometry)
        Next
    End Sub

    Private Sub Grid_MouseWheel(sender As Object, e As MouseWheelEventArgs)
        camera.Position = New Vertex(camera.Position.X, camera.Position.Y, camera.Position.Z - e.Delta / 250.0)
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        camera.Position = New Vertex(camera.Position.X, camera.Position.Y, 5)

        For Each g As GeometryModel3D In __geometry
            g.Transform = New Transform3DGroup()
        Next
    End Sub

    Private Sub Grid_MouseMove(sender As Object, e As MouseEventArgs)
        If Not __drag Then Return

        Dim pos As Point = Mouse.GetPosition(viewport)
        Dim actualPos As New Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y)
        Dim dx As Double = actualPos.X - __lastPos.X, dy As Double = actualPos.Y - __lastPos.Y
        Dim mouseAngle As Double = 0

        If dx <> 0 AndAlso dy <> 0 Then
            mouseAngle = stdNum.Asin(stdNum.Abs(dy) / stdNum.Sqrt(stdNum.Pow(dx, 2) + stdNum.Pow(dy, 2)))

            If dx < 0 AndAlso dy > 0 Then
                mouseAngle += stdNum.PI / 2
            ElseIf dx < 0 AndAlso dy < 0 Then
                mouseAngle += stdNum.PI
            ElseIf dx > 0 AndAlso dy < 0 Then
                mouseAngle += stdNum.PI * 1.5
            End If
        ElseIf dx = 0 AndAlso dy <> 0 Then
            mouseAngle = If(stdNum.Sign(dy) > 0, stdNum.PI / 2, stdNum.PI * 1.5)
        ElseIf dx <> 0 AndAlso dy = 0 Then
            mouseAngle = If(stdNum.Sign(dx) > 0, 0, stdNum.PI)
        End If

        Dim axisAngle As Double = mouseAngle + stdNum.PI / 2
        Dim axis As New Vector3D(stdNum.Cos(axisAngle) * 4, stdNum.Sin(axisAngle) * 4, 0)
        Dim rotation As Double = 0.01 * stdNum.Sqrt(stdNum.Pow(dx, 2) + stdNum.Pow(dy, 2))
        Dim r As New QuaternionRotation3D(New Quaternion(axis, rotation * 180 / stdNum.PI))

        For Each geometry As GeometryModel3D In __geometry
            With TryCast(geometry.Transform, Transform3DGroup)
                .Children.Add(New RotateTransform3D(r))
            End With
        Next

        __lastPos = actualPos
    End Sub

    Private Sub Grid_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton <> MouseButtonState.Pressed Then
            Return
        Else
            __drag = True
        End If

        Dim pos As Point = Mouse.GetPosition(viewport)
        __lastPos = New Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y)
    End Sub

    Private Sub Grid_MouseUp(sender As Object, e As MouseButtonEventArgs)
        __drag = False
    End Sub
End Class
