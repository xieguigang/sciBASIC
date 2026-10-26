' ---------------------------------------------------------------------------
' 逐高斯屏幕投影
'
' 每个高斯在这里一次性算出光栅化与反向传播需要的全部量（SoA 布局）：
'   cam(x,y,z) → opacity(logit/sigmoid) → R(q) → Σ_world → Σ_cam → Σ' → conic
'   → 屏幕均值 → 半径 → 像素包围盒
'
' 协方差修正说明：
'   旧实现直接把世界坐标系下的 Σ 当成相机坐标系使用（等价于假设 R_cam = I），
'   在多视角训练下这是错误的。这里补上 Σ_cam = R_cam · Σ_world · R_camᵀ，
'   与 3DGS 参考实现（computeCov2D 中的 W·Σ·Wᵀ）一致。
' ---------------------------------------------------------------------------

Imports std = System.Math

''' <summary>
''' 把 3D 高斯投影到屏幕空间，产出 <see cref="SplatScreenData"/>。
''' </summary>
Public Module SplatProjection

    ''' <summary>协方差行列式低于该值时视为退化（不参与渲染）。</summary>
    Public Const MinDeterminant As Double = 0.000000000001

    Public Function Project(model As GaussianModel, camera As Camera,
                                       Optional options As SplatRenderOptions = Nothing) As SplatScreenData
            If options Is Nothing Then options = SplatRenderOptions.Default

            Dim n = model.Count
            Dim data As New SplatScreenData(n)
            Dim intr = camera.Intrinsics

            data.Width = intr.Width
            data.Height = intr.Height
            data.Fx = intr.Fx
            data.Fy = intr.Fy
            data.Cx = intr.Cx
            data.Cy = intr.Cy
            data.Rcam = camera.Extrinsics.RFlat

            Dim t0 = PerfTrace.Begin()
            Call ProjectKernel(model, camera, options, data)
            Call PerfTrace.Done(PerfPhase.Projection, t0)

            Return data
        End Function

    Private Sub ProjectKernel(model As GaussianModel, camera As Camera,
                                  options As SplatRenderOptions, data As SplatScreenData)

            Dim n = model.Count
            If n = 0 Then Return

            Dim pos = model.PositionsData
            Dim scale = model.ScalesData
            Dim rot = model.RotationsData
            Dim cols = model.ColorsData
            Dim opac = model.OpacitiesData

            Dim Rc = data.Rcam
            Dim Tr = camera.Extrinsics.T

            Dim fx = data.Fx, fy = data.Fy, cx = data.Cx, cy = data.Cy
            Dim W = data.Width, H = data.Height

            Dim minOpacity = options.MinOpacity
            Dim minDepth = options.MinDepth
            Dim eps = options.Cov2DEpsilon
            Dim sigma = options.RadiusSigma
            Dim wMax = W - 1, hMax = H - 1

            Dim visibleCount = 0

            For i = 0 To n - 1
                Dim o3 = i * 3
                Dim px = pos(o3), py = pos(o3 + 1), pz = pos(o3 + 2)

                Dim x = Rc(0) * px + Rc(1) * py + Rc(2) * pz + Tr(0)
                Dim y = Rc(3) * px + Rc(4) * py + Rc(5) * pz + Tr(1)
                Dim z = Rc(6) * px + Rc(7) * py + Rc(8) * pz + Tr(2)

                data.CamX(i) = x
                data.CamY(i) = y
                data.CamZ(i) = z

                If z <= minDepth Then Continue For

                Dim raw = opac(i)
                Dim op = GaussianModel.Sigmoid(raw)

                data.RawOpacity(i) = raw
                data.Opacity(i) = op

                If op < minOpacity Then Continue For

                ' ---- 四元数 -> 旋转矩阵 ----
                Dim o4 = i * 4
                Dim qw = rot(o4), qx = rot(o4 + 1), qy = rot(o4 + 2), qz = rot(o4 + 3)
                Dim qn = std.Sqrt(qw * qw + qx * qx + qy * qy + qz * qz)

                Dim r00, r01, r02, r10, r11, r12, r20, r21, r22 As Double

                If qn < 0.000000000001 Then
                    r00 = 1 : r01 = 0 : r02 = 0
                    r10 = 0 : r11 = 1 : r12 = 0
                    r20 = 0 : r21 = 0 : r22 = 1
                Else
                    qw /= qn : qx /= qn : qy /= qn : qz /= qn

                    Dim xx = qx * qx, yy = qy * qy, zz = qz * qz
                    Dim xy = qx * qy, xz = qx * qz, yz = qy * qz
                    Dim wx = qw * qx, wy = qw * qy, wz = qw * qz

                    r00 = 1 - 2 * (yy + zz) : r01 = 2 * (xy - wz) : r02 = 2 * (xz + wy)
                    r10 = 2 * (xy + wz) : r11 = 1 - 2 * (xx + zz) : r12 = 2 * (yz - wx)
                    r20 = 2 * (xz - wy) : r21 = 2 * (yz + wx) : r22 = 1 - 2 * (xx + yy)
                End If

                ' ---- Σ_world = R · diag(s²) · Rᵀ ----
                Dim sx = std.Exp(scale(o3)), sy = std.Exp(scale(o3 + 1)), sz = std.Exp(scale(o3 + 2))
                Dim m0 = sx * sx, m1 = sy * sy, m2 = sz * sz

                Dim s00 = r00 * m0 * r00 + r01 * m1 * r01 + r02 * m2 * r02
                Dim s01 = r00 * m0 * r10 + r01 * m1 * r11 + r02 * m2 * r12
                Dim s02 = r00 * m0 * r20 + r01 * m1 * r21 + r02 * m2 * r22
                Dim s11 = r10 * m0 * r10 + r11 * m1 * r11 + r12 * m2 * r12
                Dim s12 = r10 * m0 * r20 + r11 * m1 * r21 + r12 * m2 * r22
                Dim s22 = r20 * m0 * r20 + r21 * m1 * r21 + r22 * m2 * r22

                ' ---- Σ_cam = R_cam · Σ_world · R_camᵀ ----
                ' 先用 R_cam 左乘（得到 3x3），再用 R_cam 右乘（转置形式）
                Dim a00 = Rc(0) * s00 + Rc(1) * s01 + Rc(2) * s02
                Dim a01 = Rc(0) * s01 + Rc(1) * s11 + Rc(2) * s12
                Dim a02 = Rc(0) * s02 + Rc(1) * s12 + Rc(2) * s22

                Dim a10 = Rc(3) * s00 + Rc(4) * s01 + Rc(5) * s02
                Dim a11 = Rc(3) * s01 + Rc(4) * s11 + Rc(5) * s12
                Dim a12 = Rc(3) * s02 + Rc(4) * s12 + Rc(5) * s22

                Dim a20 = Rc(6) * s00 + Rc(7) * s01 + Rc(8) * s02
                Dim a21 = Rc(6) * s01 + Rc(7) * s11 + Rc(8) * s12
                Dim a22 = Rc(6) * s02 + Rc(7) * s12 + Rc(8) * s22

                Dim c00 = a00 * Rc(0) + a01 * Rc(1) + a02 * Rc(2)
                Dim c01 = a00 * Rc(3) + a01 * Rc(4) + a02 * Rc(5)
                Dim c02 = a00 * Rc(6) + a01 * Rc(7) + a02 * Rc(8)

                Dim c11 = a10 * Rc(3) + a11 * Rc(4) + a12 * Rc(5)
                Dim c12 = a10 * Rc(6) + a11 * Rc(7) + a12 * Rc(8)
                Dim c22 = a20 * Rc(6) + a21 * Rc(7) + a22 * Rc(8)

                data.Sigma00(i) = c00
                data.Sigma01(i) = c01
                data.Sigma02(i) = c02
                data.Sigma11(i) = c11
                data.Sigma12(i) = c12
                data.Sigma22(i) = c22

                ' ---- 透视雅可比的 2D 协方差 Σ' = J Σ_cam Jᵀ ----
                Dim invZ = 1.0 / z
                Dim invZ2 = invZ * invZ
                Dim j00 = fx * invZ
                Dim j02 = -fx * x * invZ2
                Dim j11 = fy * invZ
                Dim j12 = -fy * y * invZ2

                Dim ca = j00 * j00 * c00 + 2 * j00 * j02 * c02 + j02 * j02 * c22
                Dim cb = j00 * j11 * c01 + j00 * j12 * c02 + j02 * j11 * c12 + j02 * j12 * c22
                Dim cc = j11 * j11 * c11 + 2 * j11 * j12 * c12 + j12 * j12 * c22

                ca += eps
                cc += eps

                Dim det = ca * cc - cb * cb
                If det <= MinDeterminant Then Continue For

                Dim invDet = 1.0 / det
                data.ConicA(i) = cc * invDet
                data.ConicB(i) = -cb * invDet
                data.ConicC(i) = ca * invDet

                ' ---- 半径：3σ 椭圆半长轴（协方差最大特征值）----
                Dim mid = 0.5 * (ca + cc)
                Dim halfDiff = 0.5 * (ca - cc)
                Dim disc = std.Sqrt(std.Max(halfDiff * halfDiff + cb * cb, 0.0))
                Dim lambdaMax = mid + disc
                Dim radius = sigma * std.Sqrt(std.Max(lambdaMax, MinDeterminant))
                data.Radius(i) = radius

                Dim mu = fx * x * invZ + cx
                Dim mv = fy * y * invZ + cy
                data.MeanX(i) = mu
                data.MeanY(i) = mv

                ' ---- 像素包围盒（含端点，已裁剪）----
                Dim x0 = CInt(std.Floor(mu - radius))
                Dim x1 = CInt(std.Ceiling(mu + radius))
                Dim y0 = CInt(std.Floor(mv - radius))
                Dim y1 = CInt(std.Ceiling(mv + radius))

                If x1 < 0 OrElse y1 < 0 OrElse x0 > wMax OrElse y0 > hMax Then Continue For

                If x0 < 0 Then x0 = 0
                If y0 < 0 Then y0 = 0
                If x1 > wMax Then x1 = wMax
                If y1 > hMax Then y1 = hMax

                If x0 > x1 OrElse y0 > y1 Then Continue For

                data.RectX0(i) = x0
                data.RectX1(i) = x1
                data.RectY0(i) = y0
                data.RectY1(i) = y1

                data.ColorR(i) = cols(o3)
                data.ColorG(i) = cols(o3 + 1)
                data.ColorB(i) = cols(o3 + 2)

                data.Visible(i) = True
                visibleCount += 1
            Next

            data.VisibleCount = visibleCount
        End Sub

End Module
