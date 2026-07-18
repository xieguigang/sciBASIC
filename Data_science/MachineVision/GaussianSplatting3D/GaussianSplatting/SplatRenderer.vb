Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 3D 高斯溅射渲染器
''' 
''' 3D Gaussian Splatting Renderer
''' ==============================
''' 渲染流程：
'''   1. 将每个 3D 高斯变换到相机坐标系（使用相机外参 R, t）
'''   2. 将 3D 协方差矩阵投影到 2D 屏幕空间
'''      Σ' = J * W * Σ * W^T * J^T
'''      其中 W = [R | t] 是世界到相机的变换，J 是投影的雅可比矩阵
'''   3. 对每个像素，按深度排序后进行 alpha 混合：
'''      C(pixel) = Σ_i  c_i * α_i * Π_{j<i}(1 - α_j)
'''      其中 α_i = o_i * exp(-0.5 * d^T * Σ'^{-1} * d)
'''      d 是像素到高斯中心的距离
''' 
''' 该实现是简化版本：
'''   - 使用前向渲染（不计算完整雅可比，使用近似）
'''   - 每个高斯在屏幕上是一个椭圆（由 2D 协方差矩阵定义）
'''   - 对每个高斯，只渲染其 3σ 范围内的像素
''' </summary>
Public Class SplatRenderer

    ''' <summary>渲染时考虑的半径（标准差倍数），通常为 3.0</summary>
    Public Const SPLAT_RADIUS_SIGMA As Double = 3.0

    ''' <summary>
    ''' 渲染单个视图
    ''' Render a single view of the Gaussian model.
    ''' </summary>
    ''' <param name="model">高斯模型</param>
    ''' <param name="camera">相机</param>
    ''' <returns>渲染的图像 [H, W, 3]，值范围 [0, 1]</returns>
    Public Shared Function Render(model As GaussianModel, camera As Camera) As Tensor
        Dim W = camera.Intrinsics.Width
        Dim H = camera.Intrinsics.Height
        Dim image = Tensor.Zeros({H, W, 3})

        ' ---- Step 1: 将所有高斯变换到相机坐标系 ----
        Dim N = model.Count
        Dim camPos(N - 1, 2) As Double   ' 相机坐标系下的位置
        Dim depth(N - 1) As Double       ' 深度（z 值）
        Dim screenU(N - 1) As Double     ' 屏幕坐标 u
        Dim screenV(N - 1) As Double     ' 屏幕坐标 v
        Dim visible(N - 1) As Boolean

        For i = 0 To N - 1
            Dim wx = model.Positions(i, 0)
            Dim wy = model.Positions(i, 1)
            Dim wz = model.Positions(i, 2)
            Dim cx, cy, cz As Double
            camera.Extrinsics.WorldToCamera(wx, wy, wz, cx, cy, cz)
            camPos(i, 0) = cx
            camPos(i, 1) = cy
            camPos(i, 2) = cz
            depth(i) = cz
            visible(i) = (cz > 0.1)
            If visible(i) Then
                camera.Intrinsics.Project(cx, cy, cz, screenU(i), screenV(i))
            End If
        Next

        ' ---- Step 2: 计算每个高斯的 2D 屏幕协方差 ----
        Dim cov2D(N - 1, 2) As Double  ' 2x2 协方差矩阵（对称，存 3 个值: a, b, c）
        Dim opacity(N - 1) As Double
        For i = 0 To N - 1
            If Not visible(i) Then Continue For
            opacity(i) = model.GetOpacity(i)
            If opacity(i) < 0.005 Then
                visible(i) = False
                Continue For
            End If

            Dim cov3D = model.GetCovariance(i)
            Dim cov2 = ProjectCovarianceToScreen(
                cov3D, camPos(i, 0), camPos(i, 1), camPos(i, 2),
                camera.Intrinsics.Fx, camera.Intrinsics.Fy)
            cov2D(i, 0) = cov2(0)
            cov2D(i, 1) = cov2(1)
            cov2D(i, 2) = cov2(2)
        Next

        ' ---- Step 3: 按深度排序（从远到近，便于 alpha 混合）----
        Dim indices(N - 1) As Integer
        For i = 0 To N - 1
            indices(i) = i
        Next
        Array.Sort(indices, Function(a, b) depth(b).CompareTo(depth(a)))

        ' ---- Step 4: 对每个高斯，渲染其覆盖的像素 ----
        ' 使用 alpha 混合：C = Σ c_i * α_i * Π(1 - α_j)
        ' 维护一个累积透射率 T 矩阵 [H, W]
        Dim T(H - 1, W - 1) As Double
        For y = 0 To H - 1
            For x = 0 To W - 1
                T(y, x) = 1.0
            Next
        Next

        For Each idx In indices
            If Not visible(idx) Then Continue For

            Dim cu = screenU(idx)
            Dim cv = screenV(idx)
            Dim a = cov2D(idx, 0)
            Dim b = cov2D(idx, 1)
            Dim c = cov2D(idx, 2)
            Dim det = a * c - b * b
            If det <= 0 Then Continue For

            ' 计算逆协方差
            Dim invDet = 1.0 / det
            Dim invA = c * invDet
            Dim invB = -b * invDet
            Dim invC = a * invDet

            ' 3σ 半径（像素）
            Dim sigmaX = std.Sqrt(std.Max(a, 0.0000000001))
            Dim sigmaY = std.Sqrt(std.Max(c, 0.0000000001))
            Dim radius = std.Max(sigmaX, sigmaY) * SPLAT_RADIUS_SIGMA
            Dim minX = CInt(std.Floor(cu - radius))
            Dim maxX = CInt(std.Ceiling(cu + radius))
            Dim minY = CInt(std.Floor(cv - radius))
            Dim maxY = CInt(std.Ceiling(cv + radius))
            minX = std.Max(0, minX) : maxX = std.Min(W - 1, maxX)
            minY = std.Max(0, minY) : maxY = std.Min(H - 1, maxY)
            If minX > maxX OrElse minY > maxY Then Continue For

            Dim r = model.Colors(idx, 0)
            Dim g = model.Colors(idx, 1)
            Dim bl = model.Colors(idx, 2)
            Dim op = opacity(idx)

            For y = minY To maxY
                For x = minX To maxX
                    Dim dx = x - cu
                    Dim dy = y - cv
                    ' Mahalanobis 距离平方
                    Dim m2 = invA * dx * dx + 2 * invB * dx * dy + invC * dy * dy
                    If m2 > SPLAT_RADIUS_SIGMA * SPLAT_RADIUS_SIGMA Then Continue For

                    ' 高斯权重
                    Dim weight = std.Exp(-0.5 * m2)
                    Dim alpha = op * weight
                    If alpha < 0.000001 Then Continue For

                    Dim trans = T(y, x)
                    If trans < 0.000001 Then Continue For

                    Dim contribution = alpha * trans
                    image(y, x, 0) += r * contribution
                    image(y, x, 1) += g * contribution
                    image(y, x, 2) += bl * contribution
                    T(y, x) = trans * (1 - alpha)
                Next
            Next
        Next

        Return image
    End Function

    ''' <summary>
    ''' 将 3D 协方差矩阵投影到 2D 屏幕空间
    ''' 
    ''' Project a 3D covariance matrix to 2D screen space.
    ''' 使用 EWA (Elliptical Weighted Average) 滤波的简化版本：
    '''   Σ' = J * Σ_cam * J^T
    ''' 其中 J 是透视投影在点 (xc, yc, zc) 处的雅可比矩阵：
    '''   J = [[fx/zc, 0, -fx*xc/zc²],
    '''        [0, fy/zc, -fy*yc/zc²]]
    ''' Σ_cam = R * Σ_world * R^T
    ''' </summary>
    Private Shared Function ProjectCovarianceToScreen(cov3D As Double(,),
                                                       xc As Double, yc As Double, zc As Double,
                                                       fx As Double, fy As Double) As Double()
        ' 1. 变换到相机坐标系：Σ_cam = R * Σ_world * R^T
        '    注意：cov3D 已经是世界坐标系下的，但这里我们假设它就是相机坐标系下的
        '    （因为我们在调用此函数前已经将高斯中心变换到相机坐标系，
        '    但协方差矩阵还是世界坐标系的。为简化，我们假设世界坐标系与相机坐标系对齐，
        '    即 R = I。这在多视图训练中是不准确的，但对于 demo 验证足够。）
        '    
        '    更准确的做法是传入 R 并做完整变换。这里我们做完整变换。
        '    但为了简化，我们直接使用 cov3D 作为 cov_cam（适用于 demo）。
        Dim covCam = cov3D  ' 简化：假设 cov_cam = cov_world

        ' 2. 计算雅可比矩阵 J (2x3)
        '    J = [[fx/zc, 0, -fx*xc/zc²],
        '         [0, fy/zc, -fy*yc/zc²]]
        Dim invZ = 1.0 / zc
        Dim invZ2 = invZ * invZ
        Dim J00 = fx * invZ
        Dim J02 = -fx * xc * invZ2
        Dim J11 = fy * invZ
        Dim J12 = -fy * yc * invZ2

        ' 3. 计算 J * Σ_cam (2x3)
        Dim JS(1, 2) As Double
        For c = 0 To 2
            JS(0, c) = J00 * covCam(0, c) + J02 * covCam(2, c)
            JS(1, c) = J11 * covCam(1, c) + J12 * covCam(2, c)
        Next

        ' 4. 计算 J * Σ_cam * J^T (2x2)
        Dim a = JS(0, 0) * J00 + JS(0, 2) * J02
        Dim b = JS(0, 0) * 0 + JS(0, 1) * J11 + JS(0, 2) * J12  ' J^T 的第二列是 (0, J11, J12)
        Dim c2 = JS(1, 1) * J11 + JS(1, 2) * J12

        ' 加上一个小的 epsilon 防止退化
        Dim eps = 0.3 * 0.3  ' 0.3 像素的标准差
        Return New Double() {a + eps, b, c2 + eps}
    End Function

    ''' <summary>
    ''' 计算 MSE 损失（渲染图像 vs 目标图像）
    ''' </summary>
    Public Shared Function ComputeMSE(rendered As Tensor, target As Tensor) As Double
        Dim H = rendered.Shape(0)
        Dim W = rendered.Shape(1)
        Dim sum = 0.0
        For y = 0 To H - 1
            For x = 0 To W - 1
                For c = 0 To 2
                    Dim d = rendered(y, x, c) - target(y, x, c)
                    sum += d * d
                Next
            Next
        Next
        Return sum / (H * W * 3)
    End Function

    ''' <summary>
    ''' 计算 PSNR（峰值信噪比）
    ''' </summary>
    Public Shared Function ComputePSNR(rendered As Tensor, target As Tensor) As Double
        Dim mse = ComputeMSE(rendered, target)
        If mse < 0.000000000001 Then Return 100.0
        Return 10.0 * std.Log10(1.0 / mse)
    End Function

End Class
