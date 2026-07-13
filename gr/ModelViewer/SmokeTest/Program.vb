Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Imaging.Landscape.Ply

Module Program

    Sub Main()
        Dim outDir = Path.Combine(Directory.GetCurrentDirectory(), "smoke_out")
        Directory.CreateDirectory(outDir)

        TestModel(Path.Combine(outDir, "cube.obj"), Path.Combine(outDir, "cube.png"))
        TestPointCloud(Path.Combine(outDir, "cloud.ply"), Path.Combine(outDir, "cloud.png"))

        System.Console.WriteLine("SMOKE_TEST_DONE")
    End Sub

    Private Sub TestModel(objPath As String, pngPath As String)
        Dim obj =
            "v -1 -1 -1" & vbCrLf &
            "v  1 -1 -1" & vbCrLf &
            "v  1  1 -1" & vbCrLf &
            "v -1  1 -1" & vbCrLf &
            "v -1 -1  1" & vbCrLf &
            "v  1 -1  1" & vbCrLf &
            "v  1  1  1" & vbCrLf &
            "v -1  1  1" & vbCrLf &
            "f 1 2 3" & vbCrLf & "f 1 3 4" & vbCrLf &
            "f 5 6 7" & vbCrLf & "f 5 7 8" & vbCrLf &
            "f 1 5 8" & vbCrLf & "f 1 8 4" & vbCrLf &
            "f 2 6 7" & vbCrLf & "f 2 7 3" & vbCrLf &
            "f 1 2 6" & vbCrLf & "f 1 6 5" & vbCrLf &
            "f 4 3 7" & vbCrLf & "f 4 7 8" & vbCrLf
        File.WriteAllText(objPath, obj)

        Dim scene = ModelLoader.LoadModel(objPath)
        System.Console.WriteLine($"Model surfaces: {If(scene.Surfaces, New Microsoft.VisualBasic.Imaging.Landscape.Data.Surface() {}).Length}")

        Dim surfaces = scene.Surfaces.Select(Function(s) s.CreateObject()).ToArray()

        Dim bmp As New Bitmap(600, 600)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            Dim cam As New Camera()
            cam.Screen = New Size(600, 600)
            cam.FieldOfView = 256
            cam.ViewDistance = 350
            cam.AngleX = 25
            cam.AngleY = 35
            cam.Draw(g, surfaces, drawPath:=False)
        End Using
        bmp.Save(pngPath, ImageFormat.Png)

        Dim nonWhite = CountNonWhite(bmp)
        System.Console.WriteLine($"Model render non-white pixels: {nonWhite}")
        If nonWhite < 100 Then Throw New Exception("Model rendering produced almost no output")
    End Sub

    Private Sub TestPointCloud(plyPath As String, pngPath As String)
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("ply")
        sb.AppendLine("format ascii 1.0")
        sb.AppendLine("element vertex 400")
        sb.AppendLine("property float x")
        sb.AppendLine("property float y")
        sb.AppendLine("property float z")
        sb.AppendLine("property float intensity")
        sb.AppendLine("end_header")
        Dim rnd = New System.Random(42)
        For i = 1 To 400
            Dim x = (rnd.NextDouble() - 0.5) * 100
            Dim y = (rnd.NextDouble() - 0.5) * 100
            Dim z = (rnd.NextDouble() - 0.5) * 100
            Dim intensity = x + y + 200
            sb.AppendLine($"{x:F3} {y:F3} {z:F3} {intensity:F3}")
        Next
        File.WriteAllText(plyPath, sb.ToString())

        Dim cloud = PlyReader.ReadFile(plyPath)
        System.Console.WriteLine($"PointCloud points: {cloud.Length}")

        Dim colors = Designer.GetColors("viridis", 256, 255)
        Dim imin = cloud.Min(Function(c) c.intensity)
        Dim imax = cloud.Max(Function(c) c.intensity)

        Dim bmp As New Bitmap(600, 600)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            Dim cam As New Camera()
            cam.Screen = New Size(600, 600)
            cam.FieldOfView = 256
            cam.ViewDistance = 350
            cam.AngleX = 25
            cam.AngleY = 35

            Dim pts = cloud.Select(Function(c) New Point3D(c.x, c.y, c.z)).ToArray()
            Dim rotated = cam.Rotate(pts).ToArray()
            Dim projected = cam.Project(rotated).ToArray()
            For i = 0 To cloud.Length - 1
                Dim xy = projected(i).PointXY(cam.Screen)
                Dim t = (cloud(i).intensity - imin) / (imax - imin)
                Dim cidx = CInt(t * 255)
                g.FillRectangle(New SolidBrush(colors(cidx)), xy.X, xy.Y, 2, 2)
            Next
        End Using
        bmp.Save(pngPath, ImageFormat.Png)

        Dim nonWhite = CountNonWhite(bmp)
        System.Console.WriteLine($"PointCloud render non-white pixels: {nonWhite}")
        If nonWhite < 100 Then Throw New Exception("PointCloud rendering produced almost no output")
    End Sub

    Private Function CountNonWhite(bmp As Bitmap) As Integer
        Dim white = Color.White.ToArgb()
        Dim count = 0
        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                If bmp.GetPixel(x, y).ToArgb() <> white Then count += 1
            Next
        Next
        Return count
    End Function

End Module
