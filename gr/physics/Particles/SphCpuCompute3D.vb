' /********************************************************************************/
'
'   SphCpuCompute3D.vb
'
'   3D SPH 默认计算后端 —— 并行 CPU 实现
'
'   作用：
'       一个子步内完成两趟：
'         1. 密度趟：从预测位置计算 density / near density（含自密度项 W(0)）
'         2. 受力趟：对称压力力 + 归一化粘性，写入加速度缓冲 ax/ay/az
'
'   与旧实现的关键差别：
'       - 无竞态：两趟都是 "gather 只读"，写只落在自己槽位上
'         （旧实现在 Parallel.For 里边读邻居速度边写自己速度）
'       - 三趟合一：旧实现密度/压力/粘性各自重新遍历 27 格并重复求距离，
'         这里网格只建一次，粘性在受力趟里顺带累加，距离只求一遍
'       - 每步零分配：全部读写预分配的 Single() / Integer()
'       - 对称压力力：动量守恒，消除旧实现的整体漂移与抖动
'
' /********************************************************************************/

Imports std = System.Math
Imports par = System.Threading.Tasks.Parallel

''' <summary>
''' The default parallel CPU backend of the 3D SPH solver.
''' </summary>
Public Class SphCpuCompute3D : Implements ISphCompute3D

    Public ReadOnly Property Name As String Implements ISphCompute3D.Name
        Get
            Return "CPU-Parallel"
        End Get
    End Property

    Public Function RunSubstep(state As SphState3D, grid As UniformGrid3D,
                               params As SphParams3D, dt As Single) As Boolean Implements ISphCompute3D.RunSubstep

        Call DensityPass(state, grid, params)
        Call ForcePass(state, grid, params, dt)

        Return True
    End Function

    ''' <summary>
    ''' pass 1: density + near density from the predicted positions.
    ''' </summary>
    Public Sub DensityPass(state As SphState3D, grid As UniformGrid3D, params As SphParams3D)
        Dim n = state.Count
        Dim qx = state.qx, qy = state.qy, qz = state.qz
        Dim dens = state.dens, densNear = state.densNear

        Dim h = params.SmoothingRadius
        Dim h2 = params.SqrRadius
        Dim k2 = params.SpikyPow2
        Dim k3 = params.SpikyPow3
        Dim selfD = params.SelfDensity
        Dim selfN = params.SelfNearDensity

        Dim cellStart = grid.CellStart
        Dim entries = grid.Entries
        Dim gnx = grid.Nx, gny = grid.Ny, gnz = grid.Nz

        Dim body As Action(Of Integer) = Sub(i)
                                             Dim xi = qx(i), yi = qy(i), zi = qz(i)
                                             Dim d As Single = selfD
                                             Dim dn As Single = selfN

                                             Dim cx = grid.CoordX(xi)
                                             Dim cy = grid.CoordY(yi)
                                             Dim cz = grid.CoordZ(zi)

                                             For ddx As Integer = -1 To 1
                                                 Dim px2 = cx + ddx
                                                 If px2 < 0 OrElse px2 >= gnx Then Continue For
                                                 For ddy As Integer = -1 To 1
                                                     Dim py2 = cy + ddy
                                                     If py2 < 0 OrElse py2 >= gny Then Continue For
                                                     For ddz As Integer = -1 To 1
                                                         Dim pz2 = cz + ddz
                                                         If pz2 < 0 OrElse pz2 >= gnz Then Continue For

                                                         Dim c = (px2 * gny + py2) * gnz + pz2
                                                         Dim s = cellStart(c)
                                                         Dim e = cellStart(c + 1)

                                                         For t As Integer = s To e - 1
                                                             Dim j = entries(t)
                                                             If j = i Then Continue For

                                                             Dim rx = qx(j) - xi
                                                             Dim ry = qy(j) - yi
                                                             Dim rz = qz(j) - zi
                                                             Dim r2 = rx * rx + ry * ry + rz * rz

                                                             If r2 >= h2 Then Continue For

                                                             Dim u = h - std.Sqrt(r2)
                                                             d += u * u * k2
                                                             dn += u * u * u * k3
                                                         Next
                                                     Next
                                                 Next
                                             Next

                                             dens(i) = d
                                             densNear(i) = dn
                                         End Sub

        If n < 2048 Then
            For i As Integer = 0 To n - 1
                Call body(i)
            Next
        Else
            Call par.For(0, n, body)
        End If
    End Sub

    ''' <summary>
    ''' pass 2: symmetric pressure force + normalized viscosity.
    ''' </summary>
    Public Sub ForcePass(state As SphState3D, grid As UniformGrid3D, params As SphParams3D, dt As Single)
        Dim n = state.Count
        Dim qx = state.qx, qy = state.qy, qz = state.qz
        Dim vx = state.vx, vy = state.vy, vz = state.vz
        Dim dens = state.dens, densNear = state.densNear
        Dim press = state.press, pressNear = state.pressNear
        Dim ax = state.ax, ay = state.ay, az = state.az

        Dim h = params.SmoothingRadius
        Dim h2 = params.SqrRadius
        Dim restD = params.RestDensity
        Dim restND = params.RestNearDensity
        Dim vol = params.VolumeScale
        Dim minR = params.MinDensityRatio
        Dim maxR = params.MaxDensityRatio
        Dim pMin = params.MinPressureRatio * params.PressureK
        Dim K = params.PressureK
        Dim KN = params.NearPressureK
        Dim g2k = params.GradSpikyPow2
        Dim g3k = params.GradSpikyPow3
        Dim poly6 = params.Poly6
        Dim maxA = params.MaxAccel

        ' 粘性松弛：单步混合系数不得超过 0.5，保证任何 dt 下都不会过冲
        Dim vis = params.Viscosity
        If vis > 0 AndAlso vis * dt > 0.5F Then vis = 0.5F / dt
        Dim visScale = vis / If(restD > 0, restD, 1.0F)

        Dim cellStart = grid.CellStart
        Dim entries = grid.Entries
        Dim gnx = grid.Nx, gny = grid.Ny, gnz = grid.Nz

        Dim body As Action(Of Integer) = Sub(i)
                                             Dim xi = qx(i), yi = qy(i), zi = qz(i)
                                             Dim vxi = vx(i), vyi = vy(i), vzi = vz(i)

                                             Dim rhoI = ClampF(dens(i) / restD, minR, maxR)
                                             Dim rhoNI = ClampF(densNear(i) / restND, minR, maxR)

                                             Dim pI = K * (rhoI - 1.0F)
                                             If pI < pMin Then pI = pMin

                                             ' 近压力只作"抗团聚"修正：以静止近密度为参考并截断到非负，
                                             ' 否则它会在液体内部产生恒定的排斥力，把平衡密度整体压低
                                             ' （表现为全场压力恒为负、密度系统性偏低于静止密度）。
                                             Dim pnI = KN * (rhoNI - 1.0F)
                                             If pnI < 0 Then pnI = 0.0F

                                             press(i) = pI
                                             pressNear(i) = pnI

                                             Dim cI = pI / (rhoI * rhoI)
                                             Dim cnI = pnI / (rhoNI * rhoNI)

                                             Dim fx As Single = 0, fy As Single = 0, fz As Single = 0
                                             Dim dvx As Single = 0, dvy As Single = 0, dvz As Single = 0

                                             Dim cx = grid.CoordX(xi)
                                             Dim cy = grid.CoordY(yi)
                                             Dim cz = grid.CoordZ(zi)

                                             For ddx As Integer = -1 To 1
                                                 Dim px2 = cx + ddx
                                                 If px2 < 0 OrElse px2 >= gnx Then Continue For
                                                 For ddy As Integer = -1 To 1
                                                     Dim py2 = cy + ddy
                                                     If py2 < 0 OrElse py2 >= gny Then Continue For
                                                     For ddz As Integer = -1 To 1
                                                         Dim pz2 = cz + ddz
                                                         If pz2 < 0 OrElse pz2 >= gnz Then Continue For

                                                         Dim c = (px2 * gny + py2) * gnz + pz2
                                                         Dim s = cellStart(c)
                                                         Dim e = cellStart(c + 1)

                                                         For t As Integer = s To e - 1
                                                             Dim j = entries(t)
                                                             If j = i Then Continue For

                                                             Dim rx = qx(j) - xi
                                                             Dim ry = qy(j) - yi
                                                             Dim rz = qz(j) - zi
                                                             Dim r2 = rx * rx + ry * ry + rz * rz

                                                             If r2 >= h2 Then Continue For

                                                             Dim r = std.Sqrt(r2)
                                                             If r < 0.0000001F Then Continue For

                                                             Dim inv = 1.0F / r
                                                             Dim dx = rx * inv
                                                             Dim dy = ry * inv
                                                             Dim dz = rz * inv
                                                             Dim u = h - r

                                                             Dim rhoJ = ClampF(dens(j) / restD, minR, maxR)
                                                             Dim rhoNJ = ClampF(densNear(j) / restND, minR, maxR)

                                                             Dim pJ = K * (rhoJ - 1.0F)
                                                             If pJ < pMin Then pJ = pMin
                                                             Dim pnJ = KN * (rhoNJ - 1.0F)
                                                             If pnJ < 0 Then pnJ = 0.0F

                                                             ' 对称（动量守恒）压力梯度：
                                                             ' a_i = -m Σ (p_i/rho_i^2 + p_j/rho_j^2) ∇_i W
                                                             ' 由于 ∇_i W = |W'| * dir(i->j)，正压时结果背离邻居 = 排斥
                                                             Dim coef = vol * (cI + pJ / (rhoJ * rhoJ))
                                                             Dim coefN = vol * (cnI + pnJ / (rhoNJ * rhoNJ))
                                                             Dim g2 = u * g2k
                                                             Dim g3 = u * u * g3k
                                                             Dim f = coef * g2 + coefN * g3

                                                             fx -= f * dx
                                                             fy -= f * dy
                                                             fz -= f * dz

                                                             If visScale <> 0 Then
                                                                 Dim v = h2 - r2
                                                                 Dim w = v * v * v * poly6 / rhoJ
                                                                 dvx += (vx(j) - vxi) * w
                                                                 dvy += (vy(j) - vyi) * w
                                                                 dvz += (vz(j) - vzi) * w
                                                             End If
                                                         Next
                                                     Next
                                                 Next
                                             Next

                                             fx += visScale * dvx
                                             fy += visScale * dvy
                                             fz += visScale * dvz

                                             If maxA > 0 Then
                                                 Dim m2 = fx * fx + fy * fy + fz * fz
                                                 If m2 > maxA * maxA Then
                                                     Dim s2 = maxA / std.Sqrt(m2)
                                                     fx *= s2
                                                     fy *= s2
                                                     fz *= s2
                                                 End If
                                             End If

                                             ax(i) = fx
                                             ay(i) = fy
                                             az(i) = fz
                                         End Sub

        If n < 2048 Then
            For i As Integer = 0 To n - 1
                Call body(i)
            Next
        Else
            Call par.For(0, n, body)
        End If
    End Sub

    ''' <summary>clamp helper (kept local so that the hot loop stays inlinable)</summary>
    Private Shared Function ClampF(v As Single, lo As Single, hi As Single) As Single
        If v < lo Then Return lo
        If v > hi Then Return hi
        Return v
    End Function

End Class
