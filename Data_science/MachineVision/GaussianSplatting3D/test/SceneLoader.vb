' ---------------------------------------------------------------------------
' 测试场景的加载与初始化（相机 / 图像 / 初始高斯）
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports std = System.Math

Module SceneLoader

    ''' <summary>测试数据目录（相机 JSON + 多视角 PNG）</summary>
    Public ReadOnly TestDataDir As String =
        "G:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineVision\GaussianSplatting3D\test_data"

    ''' <summary>输出目录</summary>
    Public ReadOnly OutputDir As String =
        "G:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineVision\GaussianSplatting3D\output"

    Public Function LoadCameras() As (Intrinsics As CameraIntrinsics, Cameras As List(Of Camera))
        Return Camera.LoadFromJson(Path.Combine(TestDataDir, "cameras.json"))
    End Function

    Public Function LoadImages(count As Integer) As List(Of Tensor)
        Return ImageLoader.LoadAll(TestDataDir, count)
    End Function

    ''' <summary>
    ''' 从第一个视图的像素反投影建立初始点云，不足则随机补充。
    ''' </summary>
    Public Function InitializeModel(intrinsics As CameraIntrinsics, cameras As List(Of Camera),
                                    images As List(Of Tensor), targetCount As Integer) As GaussianModel
        Dim rng As New Random(42)
        Dim positions As New List(Of Double())()
        Dim colors As New List(Of Double())()

        Dim cam0 = cameras(0)
        Dim img0 = images(0)
        Dim W = intrinsics.Width
        Dim H = intrinsics.Height

        Dim matR = cam0.Extrinsics.RFlat
        Dim t = cam0.Extrinsics.T

        For y = 0 To H - 1 Step 5
            For x = 0 To W - 1 Step 5
                Dim o3 = (y * W + x) * 3
                Dim r = img0.Data(o3)
                Dim g = img0.Data(o3 + 1)
                Dim b = img0.Data(o3 + 2)

                ' 跳过背景（黑色）
                If r + g + b < 0.1 Then Continue For

                Dim depth = 1.5 + rng.NextDouble() * 2.5
                Dim xc = (x - intrinsics.Cx) / intrinsics.Fx * depth
                Dim yc = (y - intrinsics.Cy) / intrinsics.Fy * depth
                Dim zc = depth

                ' P_world = Rᵀ·(P_cam − t)
                Dim xt = xc - t(0)
                Dim yt = yc - t(1)
                Dim zt = zc - t(2)

                Dim wx = matR(0) * xt + matR(3) * yt + matR(6) * zt
                Dim wy = matR(1) * xt + matR(4) * yt + matR(7) * zt
                Dim wz = matR(2) * xt + matR(5) * yt + matR(8) * zt

                positions.Add({wx, wy, wz})
                colors.Add({r, g, b})
            Next
        Next

        While positions.Count < targetCount
            positions.Add({(rng.NextDouble() - 0.5) * 4, (rng.NextDouble() - 0.5) * 2 + 0.5, (rng.NextDouble() - 0.5) * 4})
            colors.Add({rng.NextDouble(), rng.NextDouble(), rng.NextDouble()})
        End While

        Dim n = std.Min(targetCount, positions.Count)
        Dim pos(n * 3 - 1) As Double
        Dim col(n * 3 - 1) As Double

        For i = 0 To n - 1
            Dim o = i * 3

            pos(o) = positions(i)(0)
            pos(o + 1) = positions(i)(1)
            pos(o + 2) = positions(i)(2)

            col(o) = colors(i)(0)
            col(o + 1) = colors(i)(1)
            col(o + 2) = colors(i)(2)
        Next

        Return GaussianModel.FromPointCloud(pos, col, n, 0.05F)
    End Function

    ''' <summary>
    ''' 由现有模型复制出一个独立模型（用于对拍时保护原模型）。
    ''' </summary>
    Public Function CloneModel(source As GaussianModel) As GaussianModel
        Dim clone As New GaussianModel(source.Count)

        Call Copy(source.PositionsData, clone.PositionsData)
        Call Copy(source.ScalesData, clone.ScalesData)
        Call Copy(source.RotationsData, clone.RotationsData)
        Call Copy(source.ColorsData, clone.ColorsData)
        Call Copy(source.OpacitiesData, clone.OpacitiesData)

        Call clone.MarkModified()

        Return clone
    End Function

    Private Sub Copy(source As Double(), target As Double())
        Call Array.Copy(source, target, source.Length)
    End Sub

    Public Sub EnsureOutput()
        If Not Directory.Exists(OutputDir) Then Call Directory.CreateDirectory(OutputDir)
    End Sub

    Public Sub SaveRenders(model As GaussianModel, cameras As List(Of Camera), subDir As String, tag As String)
        Dim dir = Path.Combine(OutputDir, subDir)

        If Not Directory.Exists(dir) Then Call Directory.CreateDirectory(dir)

        For v = 0 To cameras.Count - 1
            Dim rendered = SplatRenderer.Render(model, cameras(v))
            Call ImageLoader.Save(rendered, Path.Combine(dir, $"{tag}_{v:00}.png"))
        Next
    End Sub

End Module
