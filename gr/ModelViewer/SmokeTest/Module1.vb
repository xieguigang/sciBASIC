Imports System.Drawing
Imports ModelViewer

Module Module1

    Sub Main()
        Dim modelPath = "g:\pixelArtist\src\framework\gr\Landscape\COLLADA\samples\cube.dae"
        If Not System.IO.File.Exists(modelPath) Then
            ' 退回到其它候选路径
            Dim candidates = {
                "g:\pixelArtist\src\framework\gr\Landscape\COLLADA\samples\house.dae",
                "g:\pixelArtist\src\framework\gr\3DEngineTest\3mf\example.3mf"
            }
            For Each c In candidates
                If System.IO.File.Exists(c) Then modelPath = c : Exit For
            Next
        End If

        Console.WriteLine("Loading model: " & modelPath)
        Dim r = New SceneRenderer()
        r.LoadModel(modelPath)
        r.FitView()
        r.Camera.Screen = New Size(800, 600)

        Dim modes = New RenderMode() {RenderMode.Surface, RenderMode.Mesh, RenderMode.PointCloud}
        For Each mode In modes
            r.Mode = mode
            Using bmp As New Bitmap(800, 600)
                Using g = Graphics.FromImage(bmp)
                    r.Draw(g, New Size(800, 600))
                End Using
                Dim out = $"smoke_{mode}.png"
                bmp.Save(out, Imaging.ImageFormat.Png)

                ' 校验不是全空白：统计非背景像素
                Dim nonBg = 0
                For y = 0 To bmp.Height - 1 Step 7
                    For x = 0 To bmp.Width - 1 Step 7
                        Dim c = bmp.GetPixel(x, y)
                        If Math.Abs(c.R - r.BackgroundColor.R) > 8 OrElse
                           Math.Abs(c.G - r.BackgroundColor.G) > 8 OrElse
                           Math.Abs(c.B - r.BackgroundColor.B) > 8 Then
                            nonBg += 1
                        End If
                    Next
                Next
                Console.WriteLine($"Rendered {mode}: nonBgPixels={nonBg}, saved {out}")
                If nonBg = 0 Then
                    Console.WriteLine($"WARNING: {mode} produced a blank image (possible render regression)")
                End If
            End Using
        Next

        Console.WriteLine("SMOKE TEST PASSED (no exceptions)")
    End Sub

End Module
