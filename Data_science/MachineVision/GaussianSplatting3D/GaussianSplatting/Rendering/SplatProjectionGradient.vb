' ---------------------------------------------------------------------------
' 屏幕空间梯度 → 世界空间参数梯度（解析反传的投影链）
'
' 输入（由光栅化后端累加）：dL/dconic(3)、dL/dmean(2)、dL/dcolor(3)、dL/dopacity(1)
' 输出：dL/dposition(3)、dL/dscale(3)、dL/drotation(4)
'
' 推导要点（全部按 3x3/2x2 矩阵求导写成显式公式，避免任何数值差分）：
'   cov2D = conic⁻¹        ⇒ dL/dΣ' = −C·H·C  （H 为 conic 梯度的对称矩阵）
'   Σ'    = J·Σ_cam·Jᵀ     ⇒ dL/dJ = 2·G·J·Σ_cam，  dL/dΣ_cam = Jᵀ·G·J
'   Σ_cam = R_cam·Σ_world·R_camᵀ ⇒ dL/dΣ_world = R_camᵀ·G·R_cam
'   Σ_world = R(q)·diag(s²)·R(q)ᵀ ⇒ dL/ds = 2s²·diag(R(q)ᵀ·G·R(q))，
'                                    dL/dR = 2·G·R(q)·diag(s²)
'   R(q)  → 四元数：对 9 个元素的多项式偏导逐个回传，再乘以归一化链式因子
'
' 对称矩阵的梯度约定（关键，容易差 2 倍）：
'   存储标量梯度 (g00, g01, g11) 对应 full-matrix 偏导 H，其中 H01 = H10 = g01/2，
'   这样 tr(H·dΣ) 才等于标量形式的 dL。
' ---------------------------------------------------------------------------

Imports std = System.Math

''' <summary>
''' 解析反传的投影链：屏幕空间中间梯度 → 高斯参数梯度。
''' </summary>
Public Module SplatProjectionGradient

        ''' <summary>
        ''' 把 <paramref name="grads"/> 中的 conic/mean 梯度就地转换为
        ''' position/scale/rotation 梯度（写入同一对象的对应数组）。
        ''' </summary>
        Public Sub Chain(model As GaussianModel, screen As SplatScreenData, grads As SplatGradients)
            Dim n = model.Count
            If n = 0 Then Return

            Dim Rc = screen.Rcam
            Dim fx = screen.Fx, fy = screen.Fy
            Dim scaleData = model.ScalesData
            Dim rotData = model.RotationsData

            Dim hc(8) As Double
            Dim matA(8) As Double
            Dim matB(8) As Double
            Dim hw(8) As Double
            Dim rq(8) As Double
            Dim rd(8) As Double

            For i = 0 To n - 1
                If Not screen.Visible(i) Then Continue For

                Dim o3 = i * 3
                Dim o2 = i * 2
                Dim o4 = i * 4

                Dim gA = grads.Conic(o3)
                Dim gB = grads.Conic(o3 + 1)
                Dim gC = grads.Conic(o3 + 2)
                Dim gmx = grads.Means(o2)
                Dim gmy = grads.Means(o2 + 1)

                If gA = 0 AndAlso gB = 0 AndAlso gC = 0 AndAlso gmx = 0 AndAlso gmy = 0 Then Continue For

                Dim camX = screen.CamX(i)
                Dim camY = screen.CamY(i)
                Dim z = screen.CamZ(i)

                If z <= 0 Then Continue For

                Dim invZ = 1.0 / z
                Dim invZ2 = invZ * invZ

                Dim j00 = fx * invZ
                Dim j02 = -fx * camX * invZ2
                Dim j11 = fy * invZ
                Dim j12 = -fy * camY * invZ2

                ' ---- conic -> cov2D ----
                Dim ca = screen.ConicA(i)
                Dim cb = screen.ConicB(i)
                Dim cc = screen.ConicC(i)

                Dim h01 = gB * 0.5

                ' H·C
                Dim m00 = gA * ca + h01 * cb
                Dim m01 = gA * cb + h01 * cc
                Dim m10 = h01 * ca + gC * cb
                Dim m11 = h01 * cb + gC * cc

                ' C·(H·C)
                Dim chc00 = ca * m00 + cb * m10
                Dim chc01 = ca * m01 + cb * m11
                Dim chc11 = cb * m01 + cc * m11

                Dim gCovA = -chc00
                Dim gCovB = -2 * chc01
                Dim gCovC = -chc11

                ' ---- cov2D -> J ----
                Dim G00 = gCovA
                Dim G01 = gCovB * 0.5
                Dim G11 = gCovC

                ' G2·J  （J = [[j00, 0, j02], [0, j11, j12]]）
                Dim gj00 = G00 * j00
                Dim gj01 = G01 * j11
                Dim gj02 = G00 * j02 + G01 * j12
                Dim gj10 = G01 * j00
                Dim gj11 = G11 * j11
                Dim gj12 = G01 * j02 + G11 * j12

                ' dL/dJ = 2·(G2·J)·Σ_cam
                Dim s00 = screen.Sigma00(i)
                Dim s01 = screen.Sigma01(i)
                Dim s02 = screen.Sigma02(i)
                Dim s11 = screen.Sigma11(i)
                Dim s12 = screen.Sigma12(i)
                Dim s22 = screen.Sigma22(i)

                Dim dJ00 = 2 * (gj00 * s00 + gj01 * s01 + gj02 * s02)
                Dim dJ02 = 2 * (gj00 * s02 + gj01 * s12 + gj02 * s22)
                Dim dJ11 = 2 * (gj10 * s01 + gj11 * s11 + gj12 * s12)
                Dim dJ12 = 2 * (gj10 * s02 + gj11 * s12 + gj12 * s22)

                ' ---- dL/dΣ_cam = Jᵀ·(G2·J) ----
                Dim t00 = j00 * gj00
                Dim t01 = j00 * gj01
                Dim t02 = j00 * gj02
                Dim t10 = j11 * gj10
                Dim t11 = j11 * gj11
                Dim t12 = j11 * gj12
                Dim t20 = j02 * gj00 + j12 * gj10
                Dim t21 = j02 * gj01 + j12 * gj11
                Dim t22 = j02 * gj02 + j12 * gj12

                ' 标量化（对称存储：非对角相加）
                Dim h00 = t00
                Dim h11 = t11
                Dim h22 = t22
                Dim h01v = t01 + t10
                Dim h02v = t02 + t20
                Dim h12v = t12 + t21

                ' 重构 halved full matrix
                hc(0) = h00 : hc(1) = h01v * 0.5 : hc(2) = h02v * 0.5
                hc(3) = hc(1) : hc(4) = h11 : hc(5) = h12v * 0.5
                hc(6) = hc(2) : hc(7) = hc(5) : hc(8) = h22

                ' ---- dL/dΣ_world = Rcᵀ·hc·Rc ----
                Call Mul3(hc, Rc, matA)
                Call Mul3TransposedLeft(Rc, matA, hw)

                ' ---- 旋转矩阵 R(q) ----
                Dim qw = rotData(o4), qx = rotData(o4 + 1), qy = rotData(o4 + 2), qz = rotData(o4 + 3)
                Dim qn = std.Sqrt(qw * qw + qx * qx + qy * qy + qz * qz)

                If qn < 0.000000000001 Then
                    Call Identity3(rq)
                Else
                    qw /= qn : qx /= qn : qy /= qn : qz /= qn

                    Dim xx = qx * qx, yy = qy * qy, zz = qz * qz
                    Dim xy = qx * qy, xz = qx * qz, yz = qy * qz
                    Dim wx = qw * qx, wy = qw * qy, wz = qw * qz

                    rq(0) = 1 - 2 * (yy + zz) : rq(1) = 2 * (xy - wz) : rq(2) = 2 * (xz + wy)
                    rq(3) = 2 * (xy + wz) : rq(4) = 1 - 2 * (xx + zz) : rq(5) = 2 * (yz - wx)
                    rq(6) = 2 * (xz - wy) : rq(7) = 2 * (yz + wx) : rq(8) = 1 - 2 * (xx + yy)
                End If

                ' ---- dL/dM（M = diag(s²)）----
                Call Mul3(hw, rq, matA)
                Call Mul3TransposedLeft(rq, matA, matB)

                Dim dm0 = matB(0), dm1 = matB(4), dm2 = matB(8)

                Dim sc0 = scaleData(o3), sc1 = scaleData(o3 + 1), sc2 = scaleData(o3 + 2)
                Dim m0 = std.Exp(2 * sc0), m1 = std.Exp(2 * sc1), m2 = std.Exp(2 * sc2)

                grads.Scales(o3) += 2 * m0 * dm0
                grads.Scales(o3 + 1) += 2 * m1 * dm1
                grads.Scales(o3 + 2) += 2 * m2 * dm2

                ' ---- dL/dR = 2·(hw·Rq)·M ----
                For r = 0 To 2
                    Dim r3 = r * 3
                    rd(r3) = 2 * matA(r3) * m0
                    rd(r3 + 1) = 2 * matA(r3 + 1) * m1
                    rd(r3 + 2) = 2 * matA(r3 + 2) * m2
                Next

                ' ---- dL/dq̂（R 的元素对 q̂ 的多项式偏导）----
                Dim gw = rd(1) * (-2 * qz) + rd(2) * (2 * qy) +
                         rd(3) * (2 * qz) + rd(5) * (-2 * qx) +
                         rd(6) * (-2 * qy) + rd(7) * (2 * qx)

                Dim gq_x = rd(1) * (2 * qy) + rd(2) * (2 * qz) +
                           rd(3) * (2 * qy) + rd(4) * (-4 * qx) + rd(5) * (-2 * qw) +
                           rd(6) * (2 * qz) + rd(7) * (2 * qw) + rd(8) * (-4 * qx)

                Dim gq_y = rd(0) * (-4 * qy) + rd(1) * (2 * qx) + rd(2) * (2 * qw) +
                           rd(3) * (2 * qx) + rd(5) * (2 * qz) +
                           rd(6) * (-2 * qw) + rd(7) * (2 * qz) + rd(8) * (-4 * qy)

                Dim gq_z = rd(0) * (-4 * qz) + rd(1) * (-2 * qw) + rd(2) * (2 * qx) +
                           rd(3) * (2 * qw) + rd(4) * (-4 * qz) + rd(5) * (2 * qy) +
                           rd(6) * (2 * qx) + rd(7) * (2 * qy)

                If qn >= 0.000000000001 Then
                    ' dL/dq = (I − q̂q̂ᵀ)·dL/dq̂ / |q|
                    Dim dot = qw * gw + qx * gq_x + qy * gq_y + qz * gq_z
                    Dim inv = 1.0 / qn

                    grads.Rotations(o4) += (gw - qw * dot) * inv
                    grads.Rotations(o4 + 1) += (gq_x - qx * dot) * inv
                    grads.Rotations(o4 + 2) += (gq_y - qy * dot) * inv
                    grads.Rotations(o4 + 3) += (gq_z - qz * dot) * inv
                Else
                    grads.Rotations(o4) += gw
                    grads.Rotations(o4 + 1) += gq_x
                    grads.Rotations(o4 + 2) += gq_y
                    grads.Rotations(o4 + 3) += gq_z
                End If

                ' ---- 位置：mean 与 J 都依赖相机空间坐标 ----
                Dim gx = gmx * fx * invZ + dJ02 * (-fx * invZ2)
                Dim gy = gmy * fy * invZ + dJ12 * (-fy * invZ2)
                Dim gz = gmx * (-fx * camX * invZ2) + gmy * (-fy * camY * invZ2) +
                         dJ00 * (-fx * invZ2) + dJ11 * (-fy * invZ2) +
                         dJ02 * (2 * fx * camX * invZ2 * invZ) + dJ12 * (2 * fy * camY * invZ2 * invZ)

                ' 相机空间 → 世界空间：pos_cam = Rc·pos + t ⇒ dpos = Rcᵀ·dcam
                grads.Positions(o3) += Rc(0) * gx + Rc(3) * gy + Rc(6) * gz
                grads.Positions(o3 + 1) += Rc(1) * gx + Rc(4) * gy + Rc(7) * gz
                grads.Positions(o3 + 2) += Rc(2) * gx + Rc(5) * gy + Rc(8) * gz
            Next
        End Sub

        ''' <summary>把 <paramref name="source"/> 累加到 <paramref name="target"/>。</summary>
        Public Sub Accumulate(source As SplatGradients, target As SplatGradients)
            Call ChannelGradients.Accumulate(source.Colors, target.Colors)
            Call ChannelGradients.Accumulate(source.Opacities, target.Opacities)
            Call ChannelGradients.Accumulate(source.Conic, target.Conic)
            Call ChannelGradients.Accumulate(source.Means, target.Means)
            Call ChannelGradients.Accumulate(source.Positions, target.Positions)
            Call ChannelGradients.Accumulate(source.Scales, target.Scales)
            Call ChannelGradients.Accumulate(source.Rotations, target.Rotations)
        End Sub

        ''' <summary>把累加结果写入模型梯度缓冲。</summary>
        Public Sub CopyTo(source As SplatGradients, model As GaussianModel)
            Call ChannelGradients.Accumulate(source.Positions, model.PositionsGradData)
            Call ChannelGradients.Accumulate(source.Scales, model.ScalesGradData)
            Call ChannelGradients.Accumulate(source.Rotations, model.RotationsGradData)
            Call ChannelGradients.Accumulate(source.Colors, model.ColorsGradData)
            Call ChannelGradients.Accumulate(source.Opacities, model.OpacitiesGradData)
        End Sub

        ''' <summary>3x3 矩阵乘（行优先扁平）。</summary>
        Private Sub Mul3(a As Double(), b As Double(), dst As Double())
            For i = 0 To 2
                Dim i3 = i * 3

                For j = 0 To 2
                    dst(i3 + j) = a(i3) * b(j) + a(i3 + 1) * b(3 + j) + a(i3 + 2) * b(6 + j)
                Next
            Next
        End Sub

        ''' <summary>3x3：dst = aᵀ·b（行优先扁平）。</summary>
        Private Sub Mul3TransposedLeft(a As Double(), b As Double(), dst As Double())
            For i = 0 To 2
                Dim i3 = i * 3

                For j = 0 To 2
                    dst(i3 + j) = a(i) * b(j) + a(3 + i) * b(3 + j) + a(6 + i) * b(6 + j)
                Next
            Next
        End Sub

    Private Sub Identity3(dst As Double())
        Array.Clear(dst, 0, 9)
        dst(0) = 1 : dst(4) = 1 : dst(8) = 1
    End Sub

End Module
