Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' B 样条/NURBS 曲面最小二乘拟合（逆向工程中的参数化曲面）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么用 NURBS/B 样条</b>：三角网格是“离散近似”，而 CAD（STEP/IGES）
    ''' 需要的是<b>解析曲面</b>：控制点 + 节点向量 + 次数。
    ''' 逆向工程的标准产物就是一张（或几张）拟合的 B 样条曲面，
    ''' 它可以直接被 CAD 内核缝合成实体。
    ''' </para>
    ''' <para>
    ''' <b>算法（可分离最小二乘）</b>：目标是最小化
    ''' <c>‖B_u·C·B_vᵀ − Z‖²</c>（<c>C</c> 为控制点网格）。
    ''' 直接对 <c>C</c> 的 <c>nu·nv</c> 个未知量解正规方程需要
    ''' <c>O((nu·nv)²)</c> 的矩阵，代价太高；利用张量积结构可分解为
    ''' </para>
    ''' <code>
    ''' C = (B_uᵀB_u)⁻¹ · (B_uᵀ·Z·B_v) · (B_vᵀB_v)⁻¹
    ''' </code>
    ''' <para>
    ''' 先累加两个 Gram 矩阵（<c>nu×nu</c>、<c>nv×nv</c>）与右端矩阵
    ''' （<c>nu×nv</c>，每个坐标一份），再做三次小矩阵求逆/乘法。
    ''' 单点开销是 <c>O(nu² + nv² + nu·nv)</c>，与控制点数量线性相关。
    ''' </para>
    ''' <para>
    ''' <b>参数化</b>：用点云协方差主轴（PCA）作为参数域，
    ''' 把点投影到平面上再归一化到 <c>[0,1]²</c>。这等价于“均匀参数化”，
    ''' 对单值曲面（板材、叶片、自由曲面）足够；对强烈折皱/多值曲面
    ''' 需要更复杂的重新参数化（chord-length / centripetal）。
    ''' </para>
    ''' <para>
    ''' <b>能力边界</b>：本类只做“曲面拟合与求值”，<b>不</b>生成 STEP/IGES 文件
    ''' （那属于 CAD 内核的职责）；控制点与节点向量可以直接交给外部 CAD 工具。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NurbsSurface

        Private ReadOnly m_controlX As Double(,)
        Private ReadOnly m_controlY As Double(,)
        Private ReadOnly m_controlZ As Double(,)
        Private ReadOnly m_knotsU As Double()
        Private ReadOnly m_knotsV As Double()
        Private ReadOnly m_degreeU As Integer
        Private ReadOnly m_degreeV As Integer
        Private ReadOnly m_countU As Integer
        Private ReadOnly m_countV As Integer

        Private Sub New(controlX As Double(,), controlY As Double(,), controlZ As Double(,),
                        knotsU As Double(), knotsV As Double(), degreeU As Integer, degreeV As Integer)
            m_controlX = controlX
            m_controlY = controlY
            m_controlZ = controlZ
            m_knotsU = knotsU
            m_knotsV = knotsV
            m_degreeU = degreeU
            m_degreeV = degreeV
            m_countU = controlX.GetLength(0)
            m_countV = controlX.GetLength(1)
        End Sub

        ''' <summary>U 方向控制点数。</summary>
        Public ReadOnly Property ControlCountU As Integer
            Get
                Return m_countU
            End Get
        End Property

        ''' <summary>V 方向控制点数。</summary>
        Public ReadOnly Property ControlCountV As Integer
            Get
                Return m_countV
            End Get
        End Property

        ''' <summary>U 方向次数。</summary>
        Public ReadOnly Property DegreeU As Integer
            Get
                Return m_degreeU
            End Get
        End Property

        ''' <summary>V 方向次数。</summary>
        Public ReadOnly Property DegreeV As Integer
            Get
                Return m_degreeV
            End Get
        End Property

        ''' <summary>U 方向节点向量。</summary>
        Public ReadOnly Property KnotsU As Double()
            Get
                Return m_knotsU
            End Get
        End Property

        ''' <summary>V 方向节点向量。</summary>
        Public ReadOnly Property KnotsV As Double()
            Get
                Return m_knotsV
            End Get
        End Property

        ''' <summary>取控制点网格（<c>(i, j)</c> 对应 U/V 方向）。</summary>
        Public Function ControlPoint(i As Integer, j As Integer) As Vector3d
            Return New Vector3d(m_controlX(i, j), m_controlY(i, j), m_controlZ(i, j))
        End Function

        ''' <summary>
        ''' 曲面求值：<c>S(u,v) = ΣΣ C(i,j)·Bu(i,u)·Bv(j,v)</c>。
        ''' </summary>
        Public Function Evaluate(u As Double, v As Double) As Vector3d
            Dim bu As Double() = Basis(m_degreeU, Clamp01(u), m_knotsU, m_countU)
            Dim bv As Double() = Basis(m_degreeV, Clamp01(v), m_knotsV, m_countV)
            Dim x As Double = 0, y As Double = 0, z As Double = 0

            For i As Integer = 0 To m_countU - 1
                If bu(i) = 0.0 Then Continue For

                For j As Integer = 0 To m_countV - 1
                    Dim w As Double = bu(i) * bv(j)

                    If w = 0.0 Then Continue For

                    x += m_controlX(i, j) * w
                    y += m_controlY(i, j) * w
                    z += m_controlZ(i, j) * w
                Next
            Next

            Return New Vector3d(x, y, z)
        End Function

        ''' <summary>
        ''' 采样成规则网格点云（用于可视化/网格化）。
        ''' </summary>
        Public Function SampleGrid(uSteps As Integer, vSteps As Integer) As PointCloudBuffer
            Dim nu As Integer = std.Max(2, uSteps)
            Dim nv As Integer = std.Max(2, vSteps)
            Dim total As Integer = nu * nv
            Dim cloud As PointCloudBuffer = PointCloudBuffer.NewCloud(total)
            Dim index As Integer = 0

            For i As Integer = 0 To nu - 1
                For j As Integer = 0 To nv - 1
                    Dim p As Vector3d = Evaluate(CDbl(i) / (nu - 1), CDbl(j) / (nv - 1))

                    cloud.X(index) = p.X
                    cloud.Y(index) = p.Y
                    cloud.Z(index) = p.Z
                    index += 1
                Next
            Next

            Return cloud
        End Function

        Private Shared Function Clamp01(value As Double) As Double
            If value < 0.0 Then Return 0.0
            If value > 1.0 Then Return 1.0

            Return value
        End Function

        ''' <summary>
        ''' 由点云拟合 B 样条曲面。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="controlU">U 方向控制点数（默认 12）</param>
        ''' <param name="controlV">V 方向控制点数（默认 12）</param>
        ''' <param name="degreeU">U 方向次数（默认 3；须 &lt; controlU）</param>
        ''' <param name="degreeV">V 方向次数（默认 3）</param>
        Public Shared Function Fit(cloud As PointCloudBuffer, Optional controlU As Integer = 12,
                                   Optional controlV As Integer = 12,
                                   Optional degreeU As Integer = 3,
                                   Optional degreeV As Integer = 3) As NurbsSurface
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If cloud.Count < 16 Then
                Throw New ArgumentException("at least 16 points are required to fit a B-spline surface!", NameOf(cloud))
            End If

            Dim nu As Integer = std.Max(4, controlU)
            Dim nv As Integer = std.Max(4, controlV)
            Dim pu As Integer = std.Max(1, std.Min(nu - 1, degreeU))
            Dim pv As Integer = std.Max(1, std.Min(nv - 1, degreeV))

            Dim knotsU As Double() = ClampedKnots(nu, pu)
            Dim knotsV As Double() = ClampedKnots(nv, pv)

            ' 1) 参数化：PCA 平面投影 + 归一化
            Dim cx As Double = 0, cy As Double = 0, cz As Double = 0

            For i As Integer = 0 To cloud.Count - 1
                cx += cloud.X(i)
                cy += cloud.Y(i)
                cz += cloud.Z(i)
            Next

            cx /= cloud.Count
            cy /= cloud.Count
            cz /= cloud.Count

            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

            For i As Integer = 0 To cloud.Count - 1
                Dim dx As Double = cloud.X(i) - cx
                Dim dy As Double = cloud.Y(i) - cy
                Dim dz As Double = cloud.Z(i) - cz

                cxx += dx * dx
                cxy += dx * dy
                cxz += dx * dz
                cyy += dy * dy
                cyz += dy * dz
                czz += dz * dz
            Next

            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(cxx, cxy, cxz, cyy, cyz, czz)
            Dim uAxis As Vector3d = eigen.V1
            Dim vAxis As Vector3d = eigen.V2

            Dim minU As Double = Double.MaxValue, maxU As Double = Double.MinValue
            Dim minV As Double = Double.MaxValue, maxV As Double = Double.MinValue
            Dim paramsU(cloud.Count - 1) As Double
            Dim paramsV(cloud.Count - 1) As Double

            For i As Integer = 0 To cloud.Count - 1
                Dim p As Vector3d = cloud.Item(i)

                paramsU(i) = Vector3d.Dot(p, uAxis)
                paramsV(i) = Vector3d.Dot(p, vAxis)
                minU = std.Min(minU, paramsU(i))
                maxU = std.Max(maxU, paramsU(i))
                minV = std.Min(minV, paramsV(i))
                maxV = std.Max(maxV, paramsV(i))
            Next

            Dim spanU As Double = std.Max(maxU - minU, 1.0E-12)
            Dim spanV As Double = std.Max(maxV - minV, 1.0E-12)

            ' 2) 累加 Gram 矩阵与右端
            Dim gu As Double()() = Zeros(nu)
            Dim gv As Double()() = Zeros(nv)
            Dim bx As Double()() = Zeros2(nu, nv)
            Dim by As Double()() = Zeros2(nu, nv)
            Dim bz As Double()() = Zeros2(nu, nv)

            For i As Integer = 0 To cloud.Count - 1
                Dim u As Double = (paramsU(i) - minU) / spanU
                Dim v As Double = (paramsV(i) - minV) / spanV
                Dim bu As Double() = Basis(pu, u, knotsU, nu)
                Dim bv As Double() = Basis(pv, v, knotsV, nv)
                Dim px As Double = cloud.X(i), py As Double = cloud.Y(i), pz As Double = cloud.Z(i)

                For a As Integer = 0 To nu - 1
                    If bu(a) = 0.0 Then Continue For

                    For c As Integer = 0 To nu - 1
                        gu(a)(c) += bu(a) * bu(c)
                    Next

                    For b As Integer = 0 To nv - 1
                        Dim w As Double = bu(a) * bv(b)

                        bx(a)(b) += w * px
                        by(a)(b) += w * py
                        bz(a)(b) += w * pz
                    Next
                Next

                For b As Integer = 0 To nv - 1
                    If bv(b) = 0.0 Then Continue For

                    For c As Integer = 0 To nv - 1
                        gv(b)(c) += bv(b) * bv(c)
                    Next
                Next
            Next

            ' 3) C = Gu⁻¹ · B · Gv⁻¹
            Dim invGu As Double()() = Invert(gu)
            Dim invGv As Double()() = Invert(gv)

            Dim controlX As Double(,) = New Double(nu - 1, nv - 1) {}
            Dim controlY As Double(,) = New Double(nu - 1, nv - 1) {}
            Dim controlZ As Double(,) = New Double(nu - 1, nv - 1) {}

            For i As Integer = 0 To nu - 1
                For j As Integer = 0 To nv - 1
                    Dim sx As Double = 0, sy As Double = 0, sz As Double = 0

                    For a As Integer = 0 To nu - 1
                        If invGu(i)(a) = 0.0 Then Continue For

                        For b As Integer = 0 To nv - 1
                            Dim w As Double = invGu(i)(a) * invGv(b)(j)

                            If w = 0.0 Then Continue For

                            sx += w * bx(a)(b)
                            sy += w * by(a)(b)
                            sz += w * bz(a)(b)
                        Next
                    Next

                    controlX(i, j) = sx
                    controlY(i, j) = sy
                    controlZ(i, j) = sz
                Next
            Next

            Return New NurbsSurface(controlX, controlY, controlZ, knotsU, knotsV, pu, pv)
        End Function

        ''' <summary>
        ''' 钳制（clamped）均匀节点向量：首尾各重复 <c>degree+1</c> 次，保证曲线通过首末控制点。
        ''' </summary>
        Public Shared Function ClampedKnots(count As Integer, degree As Integer) As Double()
            Dim n As Integer = std.Max(degree + 1, count)
            Dim total As Integer = n + degree + 1
            Dim knots(total - 1) As Double
            Dim interior As Integer = n - degree - 1

            For i As Integer = 0 To total - 1
                If i <= degree Then
                    knots(i) = 0.0
                ElseIf i >= n Then
                    knots(i) = 1.0
                Else
                    knots(i) = CDbl(i - degree) / (interior + 1)
                End If
            Next

            Return knots
        End Function

        ''' <summary>
        ''' Cox–de Boor 递推求 B 样条基函数值（返回 <c>count</c> 个非零以外的全部分量）。
        ''' </summary>
        Public Shared Function Basis(degree As Integer, u As Double, knots As Double(), count As Integer) As Double()
            Dim values As Double() = New Double(count - 1) {}

            ' 0 次基函数
            For i As Integer = 0 To count - 1
                If (knots(i) <= u AndAlso u < knots(i + 1)) OrElse
                    (u >= 1.0 AndAlso knots(i + 1) = 1.0 AndAlso knots(i) < 1.0) Then
                    values(i) = 1.0
                End If
            Next

            For d As Integer = 1 To std.Max(0, degree)
                Dim nextValues As Double() = New Double(count - 1) {}

                For i As Integer = 0 To count - 1
                    Dim left As Double = 0.0
                    Dim right As Double = 0.0

                    If i + d < knots.Length Then
                        Dim denominator As Double = knots(i + d) - knots(i)

                        If denominator > 1.0E-15 Then left = (u - knots(i)) / denominator * values(i)
                    End If

                    If i + 1 < values.Length AndAlso i + d + 1 < knots.Length Then
                        Dim denominator As Double = knots(i + d + 1) - knots(i + 1)

                        If denominator > 1.0E-15 Then right = (knots(i + d + 1) - u) / denominator * values(i + 1)
                    End If

                    nextValues(i) = left + right
                Next

                values = nextValues
            Next

            Return values
        End Function

        Private Shared Function Zeros(n As Integer) As Double()()
            Dim m As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                m(i) = New Double(n - 1) {}
            Next

            Return m
        End Function

        Private Shared Function Zeros2(rows As Integer, cols As Integer) As Double()()
            Dim m As Double()() = New Double(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                m(i) = New Double(cols - 1) {}
            Next

            Return m
        End Function

        ''' <summary>
        ''' 小矩阵求逆（带阻尼的正规方程求逆，失败时退化为对角占优近似）。
        ''' </summary>
        Private Shared Function Invert(matrix As Double()()) As Double()()
            Dim n As Integer = matrix.Length

            ' 加一个极小的岭项，保证 Gram 矩阵可逆（控制点数量大于数据自由度时会奇异）
            Dim ridge As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                ridge(i) = DirectCast(matrix(i).Clone(), Double())
                ridge(i)(i) += 1.0E-9
            Next

            Dim result As Double()() = New Double(n - 1)() {}

            For col As Integer = 0 To n - 1
                Dim unit As Double() = New Double(n - 1) {}

                unit(col) = 1.0

                Dim solution As Double()

                Try
                    solution = Linalg.SolveN(ridge, unit)
                Catch ex As InvalidOperationException
                    solution = New Double(n - 1) {}
                    solution(col) = 1.0 / std.Max(1.0E-12, ridge(col)(col))
                End Try

                For row As Integer = 0 To n - 1
                    If result(row) Is Nothing Then result(row) = New Double(n - 1) {}

                    result(row)(col) = solution(row)
                Next
            Next

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"B-spline surface {m_countU}x{m_countV} control points, degree {m_degreeU}x{m_degreeV}"
        End Function

    End Class
End Namespace
