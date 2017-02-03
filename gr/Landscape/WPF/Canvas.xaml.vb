Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape.Vendor_3mf
Imports Vertex = System.Windows.Media.Media3D.Point3D
Imports Microsoft.VisualBasic.Language

Public Class Canvas

    Dim __geometry As New List(Of GeometryModel3D)
    Dim __drag As Boolean
    Dim __lastPos As Point

    Public Sub New()
        Call InitializeComponent()
    End Sub

    Public Sub BuildSolid(project As Project)
        For Each obj In project.model.resources.objects
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

        'For Each surface As Surface In project.GetSurfaces(True)

        '    ' Define 3D mesh object
        '    Dim mesh As New MeshGeometry3D()

        '    mesh.Positions.Add(New Vertex(-0.5, -0.5, 1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, 1));
        '    mesh.Positions.Add(New Vertex(0.5, -0.5, 1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, 1));
        '    mesh.Positions.Add(New Vertex(0.5, 0.5, 1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, 1));
        '    mesh.Positions.Add(New Vertex(-0.5, 0.5, 1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, 1));

        '    mesh.Positions.Add(New Vertex(-1, -1, -1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, -1));
        '    mesh.Positions.Add(New Vertex(1, -1, -1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, -1));
        '    mesh.Positions.Add(New Vertex(1, 1, -1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, -1));
        '    mesh.Positions.Add(New Vertex(-1, 1, -1))
        '    'mesh.Normals.Add(new Vector3D(0, 0, -1));

        '    ' Front face
        '    mesh.TriangleIndices.Add(0)
        '    mesh.TriangleIndices.Add(1)
        '    mesh.TriangleIndices.Add(2)
        '    mesh.TriangleIndices.Add(2)
        '    mesh.TriangleIndices.Add(3)
        '    mesh.TriangleIndices.Add(0)

        '    ' Back face
        '    mesh.TriangleIndices.Add(6)
        '    mesh.TriangleIndices.Add(5)
        '    mesh.TriangleIndices.Add(4)
        '    mesh.TriangleIndices.Add(4)
        '    mesh.TriangleIndices.Add(7)
        '    mesh.TriangleIndices.Add(6)

        '    ' Right face
        '    mesh.TriangleIndices.Add(1)
        '    mesh.TriangleIndices.Add(5)
        '    mesh.TriangleIndices.Add(2)
        '    mesh.TriangleIndices.Add(5)
        '    mesh.TriangleIndices.Add(6)
        '    mesh.TriangleIndices.Add(2)

        '    ' Top face
        '    mesh.TriangleIndices.Add(2)
        '    mesh.TriangleIndices.Add(6)
        '    mesh.TriangleIndices.Add(3)
        '    mesh.TriangleIndices.Add(3)
        '    mesh.TriangleIndices.Add(6)
        '    mesh.TriangleIndices.Add(7)

        '    ' Bottom face
        '    mesh.TriangleIndices.Add(5)
        '    mesh.TriangleIndices.Add(1)
        '    mesh.TriangleIndices.Add(0)
        '    mesh.TriangleIndices.Add(0)
        '    mesh.TriangleIndices.Add(4)
        '    mesh.TriangleIndices.Add(5)

        '    ' Right face
        '    mesh.TriangleIndices.Add(4)
        '    mesh.TriangleIndices.Add(0)
        '    mesh.TriangleIndices.Add(3)
        '    mesh.TriangleIndices.Add(3)
        '    mesh.TriangleIndices.Add(7)
        '    mesh.TriangleIndices.Add(4)

        '    ' Geometry creation
        '    mGeometry = New GeometryModel3D(mesh, New DiffuseMaterial(Brushes.YellowGreen))
        '    mGeometry.Transform = New Transform3DGroup()
        '    group.Children.Add(mGeometry)
        'Next
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
        If __drag Then
            Dim pos As Point = Mouse.GetPosition(viewport)
            Dim actualPos As New Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y)
            Dim dx As Double = actualPos.X - __lastPos.X, dy As Double = actualPos.Y - __lastPos.Y

            Dim mouseAngle As Double = 0
            If dx <> 0 AndAlso dy <> 0 Then
                mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)))
                If dx < 0 AndAlso dy > 0 Then
                    mouseAngle += Math.PI / 2
                ElseIf dx < 0 AndAlso dy < 0 Then
                    mouseAngle += Math.PI
                ElseIf dx > 0 AndAlso dy < 0 Then
                    mouseAngle += Math.PI * 1.5
                End If
            ElseIf dx = 0 AndAlso dy <> 0 Then
                mouseAngle = If(Math.Sign(dy) > 0, Math.PI / 2, Math.PI * 1.5)
            ElseIf dx <> 0 AndAlso dy = 0 Then
                mouseAngle = If(Math.Sign(dx) > 0, 0, Math.PI)
            End If

            Dim axisAngle As Double = mouseAngle + Math.PI / 2
            Dim axis As New Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0)
            Dim rotation As Double = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2))
            Dim r As New QuaternionRotation3D(New Quaternion(axis, rotation * 180 / Math.PI))

            For Each geometry As GeometryModel3D In __geometry
                With TryCast(geometry.Transform, Transform3DGroup)
                    .Children.Add(New RotateTransform3D(r))
                End With
            Next

            __lastPos = actualPos
        End If
    End Sub

    Private Sub Grid_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton <> MouseButtonState.Pressed Then
            Return
        End If
        __drag = True
        Dim pos As Point = Mouse.GetPosition(viewport)
        __lastPos = New Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y)
    End Sub

    Private Sub Grid_MouseUp(sender As Object, e As MouseButtonEventArgs)
        __drag = False
    End Sub
End Class
