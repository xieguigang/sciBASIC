Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Module Module1

    Sub Main()
        ' 1) 验证 SIMD 旋转与旧 RotatePoint 数学等价（X=90 -> (X,-Z,Y)；Y=90 -> (Z,Y,-X)）
        Dim cam As New Microsoft.VisualBasic.Imaging.Drawing3D.Camera()
        cam.AngleX = 90 : cam.AngleY = 0 : cam.AngleZ = 0
        Dim p1 = cam.Rotate(New Point3D(0, 0, 1))   ' 期望 (0,-1,0)
        Console.WriteLine($"Rotate(0,0,1) @X90 = ({p1.X:F3},{p1.Y:F3},{p1.Z:F3})")
        AssertClose(p1, 0, -1, 0, "X=90 (0,0,1)")

        cam.AngleX = 0 : cam.AngleY = 90 : cam.AngleZ = 0
        Dim p2 = cam.Rotate(New Point3D(1, 0, 0))   ' 期望 (0,0,-1)
        Console.WriteLine($"Rotate(1,0,0) @Y90 = ({p2.X:F3},{p2.Y:F3},{p2.Z:F3})")
        AssertClose(p2, 0, 0, -1, "Y=90 (1,0,0)")

        ' 2) 合成立方体，验证 Camera.Draw 的并行+SIMD 渲染不抛异常且无 NaN
        cam.AngleX = 20 : cam.AngleY = -30 : cam.AngleZ = 0
        cam.FieldOfView = 256 * 8
        cam.Screen = New Size(800, 600)
        cam.ViewDistance = 1000

        Dim cube = CubeTriangles()
        Dim surfaces As New System.Collections.Generic.List(Of Microsoft.VisualBasic.Imaging.Drawing3D.Surface)()
        For Each tri In cube
            surfaces.Add(New Microsoft.VisualBasic.Imaging.Drawing3D.Surface(tri, New System.Drawing.SolidBrush(Color.FromArgb(180, 60, 120, 200))))
        Next

        Using bmp As New Bitmap(800, 600)
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                cam.Draw(g, surfaces)
            End Using
            bmp.Save("smoke_camera_draw.png", Imaging.ImageFormat.Png)
        End Using
        Console.WriteLine("Camera.Draw (parallel + SIMD) OK, surfaces=" & surfaces.Count)

        ' 3) 批量旋转 SIMD 路径
        Dim pts = cube.SelectMany(Function(t) t).ToArray()
        Dim rotated = cam.Rotate(pts)
        If rotated.Length <> pts.Length Then Throw New Exception("Batch Rotate length mismatch")
        For Each r In rotated
            If Double.IsNaN(r.X) OrElse Double.IsNaN(r.Y) OrElse Double.IsNaN(r.Z) Then
                Throw New Exception("Batch Rotate produced NaN")
            End If
        Next
        Console.WriteLine("Batch Rotate (SIMD) count=" & rotated.Length & " finite=OK")

        Console.WriteLine("SMOKE TEST PASSED")
    End Sub

    Sub AssertClose(p As Point3D, ex As Double, ey As Double, ez As Double, tag As String)
        Dim tol = 1.0E-3
        If Math.Abs(p.X - ex) > tol OrElse Math.Abs(p.Y - ey) > tol OrElse Math.Abs(p.Z - ez) > tol Then
            Throw New Exception($"Rotation math regression [{tag}]: got ({p.X},{p.Y},{p.Z}) expected ({ex},{ey},{ez})")
        End If
    End Sub

    Function CubeTriangles() As System.Collections.Generic.List(Of Point3D())
        Dim v = {
            New Point3D(-1, -1, -1), New Point3D(1, -1, -1), New Point3D(1, 1, -1), New Point3D(-1, 1, -1),
            New Point3D(-1, -1, 1), New Point3D(1, -1, 1), New Point3D(1, 1, 1), New Point3D(-1, 1, 1)
        }
        Dim faces = {
            New Integer() {0, 1, 2, 3}, New Integer() {4, 5, 6, 7},
            New Integer() {0, 1, 5, 4}, New Integer() {2, 3, 7, 6},
            New Integer() {1, 2, 6, 5}, New Integer() {0, 3, 7, 4}
        }
        Dim tris = New System.Collections.Generic.List(Of Point3D())()
        For Each f In faces
            tris.Add({v(f(0)), v(f(1)), v(f(2))})
            tris.Add({v(f(0)), v(f(2)), v(f(3))})
        Next
        Return tris
    End Function

End Module
