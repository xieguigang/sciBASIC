Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 主曲率估计：局部二次曲面拟合 + Weingarten 映射的特征值。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么需要主曲率</b>：表面变化度（<c>λ3/Σλ</c>，见 <see cref="NormalEstimation"/>）
    ''' 只给出“这个邻域有多弯”的一个标量，无法区分球面（两个方向都弯）
    ''' 与圆柱面（一个方向弯、一个方向直）。工业上很多判据依赖这种区分：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>圆柱面/管道：<c>|κ1| ≈ 1/R</c>，<c>κ2 ≈ 0</c>，可直接反算半径。</description></item>
    ''' <item><description>球面：<c>κ1 ≈ κ2 ≈ 1/R</c>。</description></item>
    ''' <item><description>平面：两者都 ≈ 0。</description></item>
    ''' <item><description>折边/锐边：<c>κ1</c> 极大而 <c>κ2</c> 正常，是分割与缺陷检测的强特征。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>算法</b>：在点 <c>p</c> 的切平面上建立局部坐标系 <c>(u, v, n)</c>，
    ''' 把邻点投影成 <c>(x, y, z)</c>，最小二乘拟合<b>二次曲面</b>
    ''' <c>z = a·x² + b·y² + c·x·y + d·x + e·y</c>
    ''' （常数项为 0，因为原点就是 <c>p</c>）。
    ''' 对 Monge 参数化，原点处的形状算子矩阵近似为
    ''' <c>[[2a, c], [c, 2b]]</c>，它的两个特征值即主曲率，
    ''' 行列式与迹分别对应高斯曲率与两倍平均曲率。
    ''' </para>
    ''' <para>
    ''' <b>数值注意</b>：拟合解出的 <c>d, e</c> 应当接近 0（切平面通过原点）；
    ''' 若 <c>|d|, |e|</c> 明显非零，说明法向估计有偏差，
    ''' 此时曲率不可信——本实现会在结果中保留这一信息（通过 <see cref="PrincipalCurvature.IsReliable"/>）。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·k + n·5³)</c>，即邻域 gather 加一个 5x5 正规方程求解。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class CurvatureEstimation

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 单点的主曲率结果。
        ''' </summary>
        Public Structure PrincipalCurvature

            ''' <summary>主曲率 1（按绝对值降序）。</summary>
            Public ReadOnly K1 As Double

            ''' <summary>主曲率 2。</summary>
            Public ReadOnly K2 As Double

            ''' <summary>高斯曲率 <c>K = κ1·κ2</c>（椭球面为正、鞍面为负）。</summary>
            Public ReadOnly Gaussian As Double

            ''' <summary>平均曲率 <c>H = (κ1 + κ2)/2</c>。</summary>
            Public ReadOnly Mean As Double

            ''' <summary>拟合是否可信（切平面偏移小且邻域充足）。</summary>
            Public ReadOnly IsReliable As Boolean

            Public Sub New(k1 As Double, k2 As Double, reliable As Boolean)
                Me.K1 = k1
                Me.K2 = k2
                Me.Gaussian = k1 * k2
                Me.Mean = (k1 + k2) * 0.5
                Me.IsReliable = reliable
            End Sub

            Public Overrides Function ToString() As String
                Return $"κ=({K1:G4}, {K2:G4}) H={Mean:G4} K={Gaussian:G4}"
            End Function
        End Structure

        ''' <summary>
        ''' 批量估计主曲率。
        ''' </summary>
        ''' <param name="cloud">输入点云（应已由 <see cref="NormalEstimation"/> 计算法向）</param>
        ''' <param name="k">邻域点数（默认 20；太少无法稳定拟合 5 参数二次曲面）</param>
        ''' <param name="radius">邻域半径（<c>≤ 0</c> 表示 kNN）</param>
        ''' <param name="options">并行选项</param>
        ''' <remarks>
        ''' 若点云没有法向，会自动先做一次法向估计（这会额外付出一次邻域协方差的开销）。
        ''' </remarks>
        Public Shared Function Estimate(cloud As PointCloudBuffer, Optional k As Integer = 20,
                                        Optional radius As Double = 0.0,
                                        Optional options As CloudyOptions = Nothing) As PrincipalCurvature()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then Return Array.Empty(Of PrincipalCurvature)()

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, k, radius, opt)
            End If

            Dim kk As Integer = std.Max(6, k)
            Dim tree As New KdTree3D(source)
            Dim result As PrincipalCurvature() = New PrincipalCurvature(n - 1) {}

            Dim scratch As New ThreadLocal(Of FitScratch)(Function() New FitScratch())
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree, kk + 1))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim local As FitScratch = scratch.Value
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()

                        For i As Integer = start To ends - 1
                            result(i) = FitAt(source, tree, buffer, local, i, kk, radius)
                        Next
                    End Sub, 256)
            Finally
                Call scratch.Dispose()
                Call pool.Dispose()
            End Try

            Return result
        End Function

        ''' <summary>
        ''' 线程本地拟合缓冲。
        ''' </summary>
        Private Class FitScratch
            Public X As Double() = New Double(127) {}
            Public Y As Double() = New Double(127) {}
            Public Z As Double() = New Double(127) {}

            Public Sub EnsureCapacity(count As Integer)
                If X.Length >= count Then Return

                Dim size As Integer = std.Max(count, X.Length * 2)

                Array.Resize(X, size)
                Array.Resize(Y, size)
                Array.Resize(Z, size)
            End Sub
        End Class

        Private Shared Function FitAt(cloud As PointCloudBuffer, tree As KdTree3D, buffer As Neighborhood,
                                      scratch As FitScratch, index As Integer, k As Integer,
                                      radius As Double) As PrincipalCurvature
            Dim px As Double = cloud.X(index)
            Dim py As Double = cloud.Y(index)
            Dim pz As Double = cloud.Z(index)

            If radius > 0.0 Then
                Call tree.RadiusSearch(px, py, pz, radius, buffer, index, k * 4)
            Else
                Call tree.Knn(px, py, pz, k, buffer, index)
            End If

            Dim count As Integer = buffer.Count

            ' 少于 6 个点无法稳定解出 5 个参数
            If count < 6 Then Return New PrincipalCurvature(0.0, 0.0, False)

            Dim normal As Vector3d = cloud.Normal(index)

            If normal.IsZero(1.0E-9) Then Return New PrincipalCurvature(0.0, 0.0, False)

            normal = normal.Normalize()

            ' 构造切平面基：任取一个与法向不平行 的参考轴
            Dim reference As Vector3d = If(std.Abs(normal.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
            Dim u As Vector3d = (reference - normal * Vector3d.Dot(reference, normal)).Normalize()
            Dim v As Vector3d = Vector3d.Cross(normal, u)

            ' 5x5 正规方程 A·c = b，基函数 [x², y², xy, x, y]
            Dim ata As Double()() = {
                New Double() {0, 0, 0, 0, 0},
                New Double() {0, 0, 0, 0, 0},
                New Double() {0, 0, 0, 0, 0},
                New Double() {0, 0, 0, 0, 0},
                New Double() {0, 0, 0, 0, 0}
            }
            Dim atb As Double() = New Double(4) {}
            Dim basis As Double() = New Double(4) {}

            For j As Integer = 0 To count - 1
                Dim q As Integer = buffer.Indices(j)
                Dim dx As Double = cloud.X(q) - px
                Dim dy As Double = cloud.Y(q) - py
                Dim dz As Double = cloud.Z(q) - pz

                Dim lx As Double = dx * u.X + dy * u.Y + dz * u.Z
                Dim ly As Double = dx * v.X + dy * v.Y + dz * v.Z
                Dim lz As Double = dx * normal.X + dy * normal.Y + dz * normal.Z

                basis(0) = lx * lx
                basis(1) = ly * ly
                basis(2) = lx * ly
                basis(3) = lx
                basis(4) = ly

                For r As Integer = 0 To 4
                    Dim br As Double = basis(r)

                    If br = 0.0 Then Continue For

                    Dim row As Double() = ata(r)

                    For col As Integer = 0 To 4
                        row(col) += br * basis(col)
                    Next

                    atb(r) += br * lz
                Next
            Next

            Dim coef As Double()

            Try
                coef = Linalg.SolveN(ata, atb, 1.0E-12)
            Catch ex As InvalidOperationException
                ' 退化（共线/共面邻域）：曲率不可解
                Return New PrincipalCurvature(0.0, 0.0, False)
            End Try

            Dim a As Double = coef(0)
            Dim b As Double = coef(1)
            Dim c As Double = coef(2)
            Dim d As Double = coef(3)
            Dim e As Double = coef(4)

            ' 形状算子 [[2a, c], [c, 2b]] 的特征值
            Dim p As Double = a + b
            Dim qq As Double = 4.0 * a * b - c * c
            Dim disc As Double = p * p - qq

            If disc < 0.0 Then disc = 0.0

            Dim root As Double = std.Sqrt(disc)
            Dim k1 As Double = p + root
            Dim k2 As Double = p - root

            If std.Abs(k2) > std.Abs(k1) Then
                Dim swap As Double = k1

                k1 = k2
                k2 = swap
            End If

            ' 切平面偏移应当接近 0；否则说明法向有偏，曲率不可信
            Dim gradient As Double = std.Sqrt(d * d + e * e)
            Dim scale As Double = std.Max(1.0, std.Max(std.Abs(k1), std.Abs(k2)))
            Dim reliable As Boolean = gradient <= 0.5 * scale

            Return New PrincipalCurvature(k1, k2, reliable)
        End Function

    End Class
End Namespace
