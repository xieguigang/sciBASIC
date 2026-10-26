' ---------------------------------------------------------------------------
' bench：分阶段性能基准
'
' 目的不是给出漂亮的数字，而是让"时间花在哪"可观测：
' 投影 / 分箱 / 光栅化 / 反向 / 优化器 各自多少毫秒、占比多少。
' 另外用极小的中心差分样本外推"旧实现"的代价，直接量化算法级收益。
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports std = System.Math

Module BenchCommand

    Public Sub Run(args As String())
        Dim cli As New CommandLine(args)
        Dim splatCount = cli.GetInt("gaussians", 2000)
        Dim repeat = cli.GetInt("repeat", 10)

        Dim Intrinsics As CameraIntrinsics, Cameras As List(Of Camera)
        With LoadCameras()
            Intrinsics = .Intrinsics
            Cameras = .Cameras
        End With

        Dim views = cli.GetInt("views", Cameras.Count)
        Cameras = Cameras.Take(views).ToList()

        Dim images = LoadImages(Cameras.Count)
        Dim model = InitializeModel(Intrinsics, Cameras, images, splatCount)

        Dim options = SplatRenderOptions.Default.Clone()

        If cli.Has("serial") Then options.MaxDegreeOfParallelism = 1
        If cli.Has("exact") Then options.ExpMode = ExpMode.Exact

        options.TileSize = cli.GetInt("tile", options.TileSize)

        Call EnsureOutput()

        Console.WriteLine()
        Console.WriteLine($"基准配置: gaussians={model.Count}, views={Cameras.Count}, " &
                          $"{Intrinsics.Width}×{Intrinsics.Height}, tile={options.TileSize}, " &
                          $"exp={options.ExpMode}, repeat={repeat}")
        Console.WriteLine()

        Dim rows As New List(Of String)()
        rows.Add("stage,ms,unit")

        ' ---- 1. 投影 ----
        Dim t = WarmupAndTime(repeat,
            Sub()
                Dim screen = SplatProjection.Project(model, Cameras(0), options)
                If screen.VisibleCount < 0 Then Console.WriteLine("unreachable")
            End Sub)

        Call Report(rows, "projection", t, repeat, "per view")

        ' ---- 2. 分箱 ----
        t = WarmupAndTime(repeat,
            Sub()
                Dim screen = SplatProjection.Project(model, Cameras(0), options)
                Dim request = SplatRasterizer.Build(screen, options)
                If request.TileCount < 0 Then Console.WriteLine("unreachable")
            End Sub)

        Call Report(rows, "projection+binning", t, repeat, "per view")

        ' ---- 3. 前向光栅化 ----
        Dim request0 = SplatRasterizer.Build(model, Cameras(0), options)
        Dim image0 As Double() = Nothing
        Dim trans0 As Double() = Nothing

        SplatRenderer.Rasterize(request0, image0, trans0)

        Dim imgBuf = image0
        Dim trBuf = trans0

        t = WarmupAndTime(repeat,
            Sub()
                Call SplatBackends.Default.Rasterize(request0, imgBuf, trBuf)
            End Sub)

        Call Report(rows, "rasterize", t, repeat, "per view")

        ' ---- 4. 单视图前向（投影 + 分箱 + 光栅化）----
        t = WarmupAndTime(repeat,
            Sub()
                Dim rendered = SplatRenderer.Render(model, Cameras(0), options)
                If rendered.Length < 0 Then Console.WriteLine("unreachable")
            End Sub)

        Call Report(rows, "render", t, repeat, "per view")

        ' ---- 5. 前向 + 反向 ----
        Dim gradient As New SplatGradients(model.Count)
        Dim mse = SplatRenderer.MseAndGradient(Tensor.Wrap(image0, request0.Height, request0.Width, 3), images(0))

        t = WarmupAndTime(repeat,
            Sub()
                gradient.Clear()
                Call SplatBackends.Default.RasterizeBackward(request0, image0, mse.Gradient, gradient)
                Call SplatProjectionGradient.Chain(model, request0.Screen, gradient)
            End Sub)

        Call Report(rows, "backward", t, repeat, "per view")

        ' ---- 6. Adam ----
        Dim optimizer As New AdamOptimizer(0.01)

        t = WarmupAndTime(repeat,
            Sub()
                optimizer.StepModel(model)
            End Sub)

        Call Report(rows, "adam", t, repeat, "whole model")

        ' ---- 7. MSE ----
        Dim rendered0 = Tensor.Wrap(image0, request0.Height, request0.Width, 3)

        t = WarmupAndTime(repeat * 4,
            Sub()
                Dim v = SplatRenderer.ComputeMSE(rendered0, images(0))
                If v < 0 Then Console.WriteLine("unreachable")
            End Sub)

        Call Report(rows, "mse", t, repeat * 4, "per view")

        ' ---- 8. 中心差分基线（外推）----
        Dim sampleCount = std.Min(2, model.Count)
        Dim sample As New List(Of Integer)()

        For i = 0 To sampleCount - 1
            sample.Add(i)
        Next

        Dim legacy As New GradientCalculator() With {.Mode = SplatGradientMode.FiniteDifference, .Options = options}
        Dim legacyModel = CloneModel(model)
        Dim watch = System.Diagnostics.Stopwatch.StartNew()

        Call legacy.ComputeFiniteDifference(legacyModel, Cameras, images, sample)

        watch.Stop()

        Dim perSplat = watch.Elapsed.TotalMilliseconds / sampleCount
        Dim projected = perSplat * model.Count

        Console.WriteLine($"中心差分基线: 每个高斯 {perSplat:F1} ms（{Cameras.Count} 视图），" &
                          $"外推全模型 {projected / 1000.0:F1} s / 次梯度")
        Console.WriteLine()

        rows.Add($"legacy-fd-per-splat,{perSplat:F3},per splat")
        rows.Add($"legacy-fd-projected,{projected:F1},whole model")

        ' ---- 汇总 ----
        Console.WriteLine($"{"stage",-24}{"ms",12}{"unit",-16}")
        For Each row In rows.Skip(1)
            Dim parts = row.Split(","c)
            Console.WriteLine($"{parts(0),-24}{parts(1),12}{parts(2),-16}")
        Next

        Dim csvPath = Path.Combine(OutputDir, $"bench_{model.Count}.csv")
        Call File.WriteAllLines(csvPath, rows)
        Console.WriteLine()
        Console.WriteLine($"基准结果已保存: {csvPath}")

        If PerfTrace.Enabled Then
            Console.WriteLine()
            Console.WriteLine("分阶段计时（最近一次前向 + 反向）:")
            Console.Write(PerfTrace.Report())
        End If
    End Sub

    Private Sub Report(rows As List(Of String), stage As String, totalMs As Double, count As Integer, unit As String)
        rows.Add($"{stage},{totalMs / std.Max(1, count):F3},{unit}")
    End Sub

    ''' <summary>先跑一次热机（JIT + 缓存），再取 count 次的平均总耗时。</summary>
    Private Function WarmupAndTime(count As Integer, body As Action) As Double
        Call body()

        Dim watch = System.Diagnostics.Stopwatch.StartNew()

        For i = 0 To std.Max(1, count) - 1
            Call body()
        Next

        watch.Stop()

        Return watch.Elapsed.TotalMilliseconds
    End Function

End Module
