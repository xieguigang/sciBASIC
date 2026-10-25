' ---------------------------------------------------------------------------
' verify：四组对拍
'
'   1. 协方差链（Σ_world / Σ_cam / Σ'）与独立实现逐位比对
'   2. tile 光栅化前向与"逐像素 gather"参考实现比对
'   3. 解析梯度与中心差分比对
'   4. 损失精度（fp64 SIMD vs fp32 张量后端）与后端回退行为
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports std = System.Math

Module VerifyCommand

    Private _failures As Integer
    Private _checks As Integer

    Public Sub Run(args As String())
        Dim cli As New CommandLine(args)
        Dim splatCount = cli.GetInt("gaussians", 48)
        Dim sample = cli.GetInt("sample", 8)

        _failures = 0
        _checks = 0

        Dim Intrinsics As CameraIntrinsics, Cameras As List(Of Camera)
        With LoadCameras()
            Intrinsics = .Intrinsics
            Cameras = .Cameras
        End With

        Dim images = LoadImages(Cameras.Count)

        ' 用小分辨率做参考实现（O(像素 × 高斯)）
        Dim small As New CameraIntrinsics(Intrinsics.Fx / 4, Intrinsics.Fy / 4, Intrinsics.Cx / 4, Intrinsics.Cy / 4,
                                          Intrinsics.Width \ 4, Intrinsics.Height \ 4)

        Dim smallCameras As New List(Of Camera)()

        For v = 0 To std.Min(3, Cameras.Count) - 1
            smallCameras.Add(New Camera(small, Cameras(v).Extrinsics, v))
        Next

        Dim model = InitializeModel(Intrinsics, Cameras, images, splatCount)
        Dim options = SplatRenderOptions.Default

        Console.WriteLine()
        Console.WriteLine($"verify 配置: gaussians={model.Count}, sample={sample}, " &
                          $"参考分辨率={small.Width}×{small.Height}, 视图={smallCameras.Count}")
        Console.WriteLine()

        Call VerifyCovariance(model, smallCameras(0))
        Call VerifyForward(model, smallCameras(0), options)
        Call VerifyGradient(model, smallCameras, ResizeTargets(images, smallCameras.Count), sample, options)
        Call VerifyLossPrecision(model, smallCameras(0), options)
        Call VerifyBackendFallback(model, smallCameras(0), options)

        Console.WriteLine()
        Console.WriteLine($"共 {_checks} 项检查，失败 {_failures} 项")

        If _failures = 0 Then
            Console.WriteLine("verify: PASS")
        Else
            Console.WriteLine("verify: FAILED")
            Environment.ExitCode = 1
        End If
    End Sub

    ''' <summary>为目标图像重新采样到参考分辨率（最近邻）。</summary>
    Private Function ResizeTargets(images As List(Of Tensor), count As Integer) As List(Of Tensor)
        Dim result As New List(Of Tensor)()

        For v = 0 To count - 1
            Dim src = images(v)
            Dim H = src.Shape(0), W = src.Shape(1)
            Dim h = std.Max(8, H \ 4), w = std.Max(8, W \ 4)
            Dim dst As New Tensor(h, w, 3)

            For y = 0 To h - 1
                For x = 0 To w - 1
                    Dim sy = std.Min(H - 1, y * 4)
                    Dim sx = std.Min(W - 1, x * 4)

                    For c = 0 To 2
                        dst(y, x, c) = src(sy, sx, c)
                    Next
                Next
            Next

            result.Add(dst)
        Next

        Return result
    End Function

#Region "1. 协方差链"

    Private Sub VerifyCovariance(model As GaussianModel, camera As Camera)
        Dim screen = SplatProjection.Project(model, camera)
        Dim Rc = camera.Extrinsics.RFlat
        Dim maxErr = 0.0

        For i = 0 To model.Count - 1
            If Not screen.Visible(i) Then Continue For

            Dim expected = ReferenceSigmaCam(model, i, Rc)
            Dim actual = {
                screen.Sigma00(i), screen.Sigma01(i), screen.Sigma02(i),
                screen.Sigma11(i), screen.Sigma12(i), screen.Sigma22(i)
            }

            For k = 0 To 5
                maxErr = std.Max(maxErr, std.Abs(expected(k) - actual(k)) / std.Max(1.0, std.Abs(expected(k))))
            Next
        Next

        Call Check("协方差链 Σ_cam（vs 独立矩阵乘）", maxErr, 1.0E-12)
    End Sub

    ''' <summary>独立的 Σ_cam 计算（朴素 3x3 矩阵乘，不复用库内代码路径）。</summary>
    Private Function ReferenceSigmaCam(model As GaussianModel, i As Integer, Rc As Double()) As Double()
        Dim pos = model.PositionsData, scale = model.ScalesData, rot = model.RotationsData
        Dim o3 = i * 3, o4 = i * 4

        Dim qw = rot(o4), qx = rot(o4 + 1), qy = rot(o4 + 2), qz = rot(o4 + 3)
        Dim qn = std.Sqrt(qw * qw + qx * qx + qy * qy + qz * qz)
        qw /= qn : qx /= qn : qy /= qn : qz /= qn

        Dim xx = qx * qx, yy = qy * qy, zz = qz * qz
        Dim xy = qx * qy, xz = qx * qz, yz = qy * qz
        Dim wx = qw * qx, wy = qw * qy, wz = qw * qz

        Dim R(8) As Double
        R(0) = 1 - 2 * (yy + zz) : R(1) = 2 * (xy - wz) : R(2) = 2 * (xz + wy)
        R(3) = 2 * (xy + wz) : R(4) = 1 - 2 * (xx + zz) : R(5) = 2 * (yz - wx)
        R(6) = 2 * (xz - wy) : R(7) = 2 * (yz + wx) : R(8) = 1 - 2 * (xx + yy)

        Dim sx = std.Exp(scale(o3)), sy = std.Exp(scale(o3 + 1)), sz = std.Exp(scale(o3 + 2))
        Dim M(8) As Double
        M(0) = sx * sx : M(4) = sy * sy : M(8) = sz * sz

        ' Σw = R·M·Rᵀ
        Dim RM(8) As Double

        For row = 0 To 2
            For col = 0 To 2
                Dim s = 0.0

                For k = 0 To 2
                    s += R(row * 3 + k) * M(k * 3 + col)
                Next

                RM(row * 3 + col) = s
            Next
        Next

        Dim Sw(8) As Double

        For row = 0 To 2
            For col = 0 To 2
                Dim s = 0.0

                For k = 0 To 2
                    s += RM(row * 3 + k) * R(col * 3 + k)
                Next

                Sw(row * 3 + col) = s
            Next
        Next

        ' Σcam = Rc·Σw·Rcᵀ
        Dim AS1(8) As Double

        For row = 0 To 2
            For col = 0 To 2
                Dim s = 0.0

                For k = 0 To 2
                    s += Rc(row * 3 + k) * Sw(k * 3 + col)
                Next

                AS1(row * 3 + col) = s
            Next
        Next

        Dim Sc(8) As Double

        For row = 0 To 2
            For col = 0 To 2
                Dim s = 0.0

                For k = 0 To 2
                    s += AS1(row * 3 + k) * Rc(col * 3 + k)
                Next

                Sc(row * 3 + col) = s
            Next
        Next

        Return {
            Sc(0), Sc(1), Sc(2),
            Sc(4), Sc(5), Sc(8)
        }
    End Function

#End Region

#Region "2. 前向光栅化"

    Private Sub VerifyForward(model As GaussianModel, camera As Camera, options As SplatRenderOptions)
        Dim request = SplatRasterizer.Build(model, camera, options)
        Dim image As Double() = Nothing
        Dim trans As Double() = Nothing

        Call SplatRenderer.Rasterize(request, image, trans)

        Dim reference = ReferenceRender(request, options)
        Dim maxErr = 0.0

        For i = 0 To image.Length - 1
            maxErr = std.Max(maxErr, std.Abs(image(i) - reference(i)))
        Next

        Call Check("tile 前向（vs 逐像素 gather 参考实现）", maxErr, 1.0E-12)

        Dim coverage = image.Count(Function(v) v > 0.000001)

        Console.WriteLine($"    覆盖像素分量: {coverage} / {image.Length}")
    End Sub

    ''' <summary>
    ''' 参考前向渲染：逐像素 gather（完全独立的循环结构，不复用 tile 内核）。
    ''' </summary>
    Private Function ReferenceRender(request As SplatRasterRequest, options As SplatRenderOptions) As Double()
        Dim W = request.Width, H = request.Height
        Dim screen = request.Screen
        Dim image(W * H * 3 - 1) As Double
        Dim order = request.Order
        Dim alphaMax = options.AlphaMax

        For y = 0 To H - 1
            For x = 0 To W - 1
                Dim tr = 1.0
                Dim cr = 0.0, cg = 0.0, cb = 0.0

                For k = 0 To order.Length - 1
                    Dim idx = order(k)

                    If x < screen.RectX0(idx) OrElse x > screen.RectX1(idx) Then Continue For
                    If y < screen.RectY0(idx) OrElse y > screen.RectY1(idx) Then Continue For
                    If tr <= 0.000001 Then Continue For

                    Dim dx = x - screen.MeanX(idx)
                    Dim dy = y - screen.MeanY(idx)

                    Dim q = screen.ConicA(idx) * dx * dx + 2 * screen.ConicB(idx) * dx * dy +
                            screen.ConicC(idx) * dy * dy

                    If q > 9.0 Then Continue For

                    Dim alpha = screen.Opacity(idx) * std.Exp(-0.5 * q)

                    If alpha <= 0.000001 Then Continue For
                    If alpha > alphaMax Then alpha = alphaMax

                    Dim w = alpha * tr

                    cr += screen.ColorR(idx) * w
                    cg += screen.ColorG(idx) * w
                    cb += screen.ColorB(idx) * w

                    tr *= (1 - alpha)
                Next

                Dim o = (y * W + x) * 3

                image(o) = cr
                image(o + 1) = cg
                image(o + 2) = cb
            Next
        Next

        Return image
    End Function

#End Region

#Region "3. 解析梯度"

    Private Sub VerifyGradient(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor),
                               sample As Integer, options As SplatRenderOptions)
        Dim analytic As New GradientCalculator() With {
            .Mode = SplatGradientMode.Analytic,
            .Options = options
        }

        Dim lossA = analytic.ComputeLossAndGradients(model, cameras, targets)

        Dim gPos = CType(model.PositionsGradData.Clone(), Double())
        Dim gScale = CType(model.ScalesGradData.Clone(), Double())
        Dim gRot = CType(model.RotationsGradData.Clone(), Double())
        Dim gColor = CType(model.ColorsGradData.Clone(), Double())
        Dim gOpac = CType(model.OpacitiesGradData.Clone(), Double())

        Dim clone = CloneModel(model)
        Dim finite As New GradientCalculator() With {
            .Mode = SplatGradientMode.FiniteDifference,
            .Options = options,
            .Epsilon = 0.01
        }

        Dim indices As New List(Of Integer)()
        Dim stride = std.Max(1, model.Count \ std.Max(1, sample))

        For i = 0 To model.Count - 1 Step stride
            indices.Add(i)

            If indices.Count >= sample Then Exit For
        Next

        clone.ZeroGradients()
        Call finite.ComputeFiniteDifference(clone, cameras, targets, indices)

        Dim maxRel = 0.0
        Dim worst As String = Nothing

        For Each idx In indices
            For d = 0 To 2
                maxRel = Compare($"pos[{idx},{d}]", analytic:=gPos(idx * 3 + d), finite:=clone.PositionsGradData(idx * 3 + d), maxRel, worst)
                maxRel = Compare($"scale[{idx},{d}]", analytic:=gScale(idx * 3 + d), finite:=clone.ScalesGradData(idx * 3 + d), maxRel, worst)
                maxRel = Compare($"color[{idx},{d}]", analytic:=gColor(idx * 3 + d), finite:=clone.ColorsGradData(idx * 3 + d), maxRel, worst)
            Next

            For d = 0 To 3
                maxRel = Compare($"rot[{idx},{d}]", analytic:=gRot(idx * 4 + d), finite:=clone.RotationsGradData(idx * 4 + d), maxRel, worst)
            Next

            maxRel = Compare($"opacity[{idx}]", analytic:=gOpac(idx), finite:=clone.OpacitiesGradData(idx), maxRel, worst)
        Next

        Console.WriteLine($"    多视图平均 MSE: 解析模式 {lossA:F8}")
        Console.WriteLine($"    最大相对误差出现在: {worst}")

        Call Check("解析梯度（vs 中心差分，抽样高斯 × 14 参数）", maxRel, 0.05)
    End Sub

    Private Function Compare(label As String, analytic As Double, finite As Double, ByRef maxRel As Double, ByRef worst As String) As Double
        Dim denom = std.Max(std.Max(std.Abs(analytic), std.Abs(finite)), 0.000000001)
        Dim rel = std.Abs(analytic - finite) / denom

        If rel > maxRel Then
            maxRel = rel
            worst = $"{label}: analytic={analytic:E4}, finite={finite:E4}"
        End If

        Return maxRel
    End Function

#End Region

#Region "4. 精度与后端回退"

    Private Sub VerifyLossPrecision(model As GaussianModel, camera As Camera, options As SplatRenderOptions)
        Dim request = SplatRasterizer.Build(model, camera, options)
        Dim image As Double() = Nothing
        Dim trans As Double() = Nothing

        Call SplatRenderer.Rasterize(request, image, trans)

        Dim rendered = Tensor.Wrap(CType(image.Clone(), Double()), request.Height, request.Width, 3)
        Dim target = Tensor.Zeros({request.Height, request.Width, 3})

        Dim fp64 = SplatRenderer.ComputeMSE(rendered, target)

        Dim a = rendered.ToTensorF()
        Dim b = target.ToTensorF()
        Dim fp32 = CDbl(TensorF.computeKernelF.MseLoss(a, b).Data(0))

        Dim rel = std.Abs(fp64 - fp32) / std.Max(1.0E-12, std.Abs(fp64))

        Console.WriteLine($"    fp64 SIMD={fp64:E6}, fp32 {TensorF.computeKernelF.Name}={fp32:E6}")
        Call Check("损失精度（fp64 vs fp32 张量后端）", rel, 1.0E-4)
    End Sub

    Private Sub VerifyBackendFallback(model As GaussianModel, camera As Camera, options As SplatRenderOptions)
        Dim request = SplatRasterizer.Build(model, camera, options)
        Dim baseline As Double() = Nothing
        Dim baselineT As Double() = Nothing

        Call SplatBackends.Reset()
        Call SplatRenderer.Rasterize(request, baseline, baselineT)

        Dim stub As New UnavailableBackend()
        SplatBackends.Register(stub, makeDefault:=True)

        Dim image As Double() = Nothing
        Dim trans As Double() = Nothing

        Call SplatRenderer.Rasterize(request, image, trans)

        Dim maxErr = 0.0

        For i = 0 To image.Length - 1
            maxErr = std.Max(maxErr, std.Abs(image(i) - baseline(i)))
        Next

        Call SplatBackends.Reset()

        Console.WriteLine($"    默认后端 = {SplatBackends.Default.Name}（注册了 {stub.Name} 后自动回退）")
        Call Check("后端不可用时的自动回退", maxErr, 0.0)
    End Sub

    ''' <summary>声明"可用"但拒绝执行的后端：用于验证调用方会自动回退。</summary>
    Private Class UnavailableBackend
        Implements ISplatRasterBackend

        Public ReadOnly Property Name As String Implements ISplatRasterBackend.Name
            Get
                Return "CUDA-Splat (stub)"
            End Get
        End Property

        Public Function IsAvailable() As Boolean Implements ISplatRasterBackend.IsAvailable
            Return True
        End Function

        Public Function Rasterize(request As SplatRasterRequest, image As Double(),
                                   transmittance As Double()) As Boolean Implements ISplatRasterBackend.Rasterize
            Return False
        End Function

        Public Function RasterizeBackward(request As SplatRasterRequest, image As Double(), imageGradient As Double(),
                                           grads As SplatGradients) As Boolean Implements ISplatRasterBackend.RasterizeBackward
            Return False
        End Function

    End Class

#End Region

    Private Sub Check(label As String, errorValue As Double, tolerance As Double)
        _checks += 1

        Dim ok = errorValue <= tolerance

        If Not ok Then _failures += 1

        Console.WriteLine($"  [{(If(ok, " OK ", "FAIL"))}] {label,-52} err={errorValue:E3} tol={tolerance:E1}")
    End Sub

End Module
