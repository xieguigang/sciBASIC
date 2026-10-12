Imports System.Linq
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 凸多面体网格：顶点表 + 面表（每个面是一圈顶点索引，外向法右手序）。
    ''' 物理（碰撞与惯量）与渲染（三角形面）共用同一份几何，避免两处定义不一致。
    ''' </summary>
    ''' <remarks>
    ''' 局部坐标以质心为原点：刚体的 <see cref="RigidBody3D.Position"/> 是质心，
    ''' 姿态四元数绕质心旋转，因此所有工厂方法在返回前都会把顶点平移到体积质心。
    ''' 平移量记录在 <see cref="PivotOffset"/>，供上层把"用户指定的放置点"换算回质心。
    ''' </remarks>
    Public Class ConvexMesh3D

        ''' <summary>局部坐标顶点（质心为原点）。</summary>
        Public Property Vertices As Vector3()
        ''' <summary>每个面的顶点索引环（外向法右手序）。</summary>
        Public Property Faces As Integer()()
        ''' <summary>
        ''' 质心在"构造坐标系"中的位置。把用户指定的放置点 P_place 换算成质心时使用：
        ''' <c>Position = P_place + q · PivotOffset</c>。
        ''' </summary>
        Public Property PivotOffset As Vector3

        Private _normals As Vector3()
        Private _edges As Integer()()
        Private _radius As Double = -1.0

        Sub New()
            Me.Vertices = New Vector3() {}
            Me.Faces = New Integer()() {}
            Me.PivotOffset = New Vector3(0, 0, 0)
        End Sub

        Sub New(vertices As Vector3(), faces As Integer()())
            Me.Vertices = vertices
            Me.Faces = faces
            Me.PivotOffset = New Vector3(0, 0, 0)
            Call FixWinding()
        End Sub

        ' /********************************************************************************/
        '  拓扑与缓存
        ' /********************************************************************************/

        ''' <summary>面法向表（惰性计算并缓存）。</summary>
        Public ReadOnly Property FaceNormals As Vector3()
            Get
                If _normals Is Nothing Then
                    Dim n(Faces.Length - 1) As Vector3

                    For i As Integer = 0 To Faces.Length - 1
                        n(i) = FaceNormal(i)
                    Next

                    _normals = n
                End If

                Return _normals
            End Get
        End Property

        ''' <summary>第 <paramref name="faceIndex"/> 个面的单位外法向（局部坐标）。</summary>
        Public Function FaceNormal(faceIndex As Integer) As Vector3
            Dim f As Integer() = Faces(faceIndex)
            Dim a As Vector3 = Vertices(f(0))
            Dim b As Vector3 = Vertices(f(1))
            Dim c As Vector3 = Vertices(f(2))

            Return Vector3Math.Normalize(Vector3Math.Cross(b - a, c - a))
        End Function

        ''' <summary>去重后的边表（顶点索引对），SAT 的边×边轴需要它。</summary>
        Public ReadOnly Property Edges As Integer()()
            Get
                If _edges Is Nothing Then
                    Dim edgeMap As New Dictionary(Of Long, Integer())()

                    For Each f As Integer() In Faces
                        For i As Integer = 0 To f.Length - 1
                            Dim a As Integer = f(i)
                            Dim b As Integer = f((i + 1) Mod f.Length)
                            Dim lo As Integer = std.Min(a, b)
                            Dim hi As Integer = std.Max(a, b)
                            Dim key As Long = CLng(lo) * 1000003L + CLng(hi)

                            If Not edgeMap.ContainsKey(key) Then
                                edgeMap.Add(key, New Integer() {a, b})
                            End If
                        Next
                    Next

                    _edges = edgeMap.Values.ToArray()
                End If

                Return _edges
            End Get
        End Property

        ''' <summary>局部坐标下的最大顶点半径（AABB 与惯量退化回退时使用）。</summary>
        Public ReadOnly Property BoundRadius As Double
            Get
                If _radius < 0.0 Then
                    Dim r2 As Double = 0.0

                    For Each v As Vector3 In Vertices
                        r2 = std.Max(r2, Vector3Math.LengthSquared(v))
                    Next

                    _radius = std.Sqrt(r2)
                End If

                Return _radius
            End Get
        End Property

        ''' <summary>
        ''' 统一面绕向：保证每个面的法向朝外。判据用"顶点平均值"作为内部点
        ''' （凸多面体的顶点平均必定严格位于内部）。
        ''' </summary>
        Private Sub FixWinding()
            If Vertices Is Nothing OrElse Vertices.Length < 4 Then Return
            If Faces Is Nothing Then Return

            Dim interior As Vector3 = New Vector3(0, 0, 0)

            For Each v As Vector3 In Vertices
                interior = interior + v
            Next

            interior = interior * (1.0 / Vertices.Length)

            For i As Integer = 0 To Faces.Length - 1
                Dim f As Integer() = Faces(i)

                If f Is Nothing OrElse f.Length < 3 Then Continue For

                Dim a As Vector3 = Vertices(f(0))
                Dim b As Vector3 = Vertices(f(1))
                Dim c As Vector3 = Vertices(f(2))
                Dim n As Vector3 = Vector3Math.Cross(b - a, c - a)
                Dim mid As Vector3 = New Vector3(0, 0, 0)

                For Each idx As Integer In f
                    mid = mid + Vertices(idx)
                Next

                mid = mid * (1.0 / f.Length)

                If Vector3Math.Dot(n, mid - interior) < 0.0 Then
                    Call Array.Reverse(f)
                End If
            Next

            _normals = Nothing
            _edges = Nothing
        End Sub

        ' /********************************************************************************/
        '  质量属性
        ' /********************************************************************************/

        Private Shared Function TetraVolume(a As Vector3, b As Vector3, c As Vector3, d As Vector3) As Double
            Dim u As Vector3 = b - a
            Dim v As Vector3 = c - a
            Dim w As Vector3 = d - a

            Return Vector3Math.Dot(u, Vector3Math.Cross(v, w)) / 6.0
        End Function

        ''' <summary>有向体积（面绕向正确时为正）。</summary>
        Public Function Volume() As Double
            Dim v As Double = 0.0

            For Each f As Integer() In Faces
                For i As Integer = 1 To f.Length - 2
                    v += TetraVolume(Vector3.Zero, Vertices(f(0)), Vertices(f(i)), Vertices(f(i + 1)))
                Next
            Next

            Return v
        End Function

        ''' <summary>体积质心（局部坐标）。</summary>
        Public Function VolumeCentroid() As Vector3
            Dim vol As Double = 0.0
            Dim acc As Vector3 = New Vector3(0, 0, 0)

            For Each f As Integer() In Faces
                For i As Integer = 1 To f.Length - 2
                    Dim a As Vector3 = Vector3.Zero
                    Dim b As Vector3 = Vertices(f(0))
                    Dim c As Vector3 = Vertices(f(i))
                    Dim d As Vector3 = Vertices(f(i + 1))
                    Dim tv As Double = TetraVolume(a, b, c, d)

                    vol += tv
                    acc = acc + (a + b + c + d) * (tv * 0.25)
                Next
            Next

            If std.Abs(vol) < 1.0E-12 Then
                Return New Vector3(0, 0, 0)
            End If

            Return acc * (1.0 / vol)
        End Function

        ''' <summary>
        ''' 相对质心的惯性张量。做法：以质心为公共顶点把每个面扇形三角化，
        ''' 对得到的每个四面体解析积分二阶矩 <c>C = ∫ p·pᵀ dV</c>，
        ''' 再由 <c>I = ρ·(tr(C)·1 − C)</c> 得到惯性张量。
        ''' </summary>
        Public Function InertiaAboutCentroid(mass As Double) As Matrix3x3
            If mass <= 0.0 Then Return Matrix3x3.Zero

            Dim cen As Vector3 = VolumeCentroid()
            Dim vol As Double = 0.0
            Dim C As Matrix3x3 = Matrix3x3.Zero

            For Each f As Integer() In Faces
                For i As Integer = 1 To f.Length - 2
                    Dim a As Vector3 = Vector3.Zero - cen
                    Dim b As Vector3 = Vertices(f(0)) - cen
                    Dim c As Vector3 = Vertices(f(i)) - cen
                    Dim d As Vector3 = Vertices(f(i + 1)) - cen
                    Dim tv As Double = TetraVolume(a, b, c, d)
                    Dim s As Vector3 = a + b + c + d

                    ' 单纯形上均匀分布的一、二阶矩：
                    '   ∫ p pᵀ dV = V/20 · (S Sᵀ + Σ v vᵀ)，其中 S = Σ v
                    Dim contrib As Matrix3x3 = Matrix3x3.Outer(s, s)

                    contrib = contrib + Matrix3x3.Outer(a, a)
                    contrib = contrib + Matrix3x3.Outer(b, b)
                    contrib = contrib + Matrix3x3.Outer(c, c)
                    contrib = contrib + Matrix3x3.Outer(d, d)

                    vol += tv
                    C = C + contrib * (tv / 20.0)
                Next
            Next

            If std.Abs(vol) < 1.0E-12 Then
                Return FallbackInertia(mass)
            End If

            Dim tr As Double = C.m11 + C.m22 + C.m33
            Dim I As Matrix3x3 = Matrix3x3.Diagonal(tr, tr, tr) + C * (-1.0)

            I = I * (mass / vol)

            ' 数值保护：惯性张量必须对称正定，否则四元数积分会发散
            If I.m11 <= 0.0 OrElse I.m22 <= 0.0 OrElse I.m33 <= 0.0 OrElse I.Determinant() <= 1.0E-12 Then
                Return FallbackInertia(mass)
            End If

            Return I
        End Function

        Private Function FallbackInertia(mass As Double) As Matrix3x3
            Dim r As Double = std.Max(BoundRadius, 1.0E-3)
            Dim i As Double = 0.4 * mass * r * r

            Return Matrix3x3.Diagonal(i, i, i)
        End Function

        ''' <summary>返回把顶点平移到体积质心后的新网格（<see cref="PivotOffset"/> 记录平移量）。</summary>
        Public Function Centered() As ConvexMesh3D
            Dim cen As Vector3 = VolumeCentroid()
            Dim vs(Vertices.Length - 1) As Vector3

            For i As Integer = 0 To Vertices.Length - 1
                vs(i) = Vertices(i) - cen
            Next

            Return New ConvexMesh3D With {
                .Vertices = vs,
                .Faces = Faces,
                .PivotOffset = cen
            }
        End Function

        ''' <summary>把局部顶点变换到世界坐标。</summary>
        Public Function WorldVertices(position As Vector3, rotation As Quaternion) As Vector3()
            Dim out(Vertices.Length - 1) As Vector3

            For i As Integer = 0 To Vertices.Length - 1
                out(i) = position + rotation.Rotate(Vertices(i))
            Next

            Return out
        End Function

        ' /********************************************************************************/
        '  工厂：基本体
        ' /********************************************************************************/

        ''' <summary>长方体（<paramref name="width"/>×<paramref name="height"/>×<paramref name="depth"/>）。</summary>
        Public Shared Function CreateBox(width As Double, height As Double, depth As Double) As ConvexMesh3D
            Dim hx As Double = std.Max(width, 1.0E-4) * 0.5
            Dim hy As Double = std.Max(height, 1.0E-4) * 0.5
            Dim hz As Double = std.Max(depth, 1.0E-4) * 0.5
            Dim vs As Vector3() = {
                New Vector3(-hx, -hy, -hz), New Vector3(hx, -hy, -hz), New Vector3(hx, hy, -hz), New Vector3(-hx, hy, -hz),
                New Vector3(-hx, -hy, hz), New Vector3(hx, -hy, hz), New Vector3(hx, hy, hz), New Vector3(-hx, hy, hz)
            }
            Dim fs As Integer()() = {
                New Integer() {0, 1, 2, 3},
                New Integer() {5, 4, 7, 6},
                New Integer() {4, 0, 3, 7},
                New Integer() {1, 5, 6, 2},
                New Integer() {4, 5, 1, 0},
                New Integer() {3, 2, 6, 7}
            }

            Return New ConvexMesh3D(vs, fs).Centered()
        End Function

        ''' <summary>正四面体，<paramref name="edge"/> 为棱长。</summary>
        Public Shared Function CreateTetrahedron(edge As Double) As ConvexMesh3D
            Dim s As Double = std.Max(edge, 1.0E-4) / (2.0 * std.Sqrt(2.0))
            Dim vs As Vector3() = {
                New Vector3(s, s, s),
                New Vector3(s, -s, -s),
                New Vector3(-s, s, -s),
                New Vector3(-s, -s, s)
            }
            Dim fs As Integer()() = {
                New Integer() {0, 1, 2},
                New Integer() {0, 3, 1},
                New Integer() {0, 2, 3},
                New Integer() {1, 3, 2}
            }

            Return New ConvexMesh3D(vs, fs).Centered()
        End Function

        ''' <summary>圆锥（局部 Y 轴为轴向，顶点朝 +Y），底面用 <paramref name="segment"/> 边形近似。</summary>
        Public Shared Function CreateCone(radius As Double, height As Double, Optional segment As Integer = 12) As ConvexMesh3D
            Dim seg As Integer = std.Max(segment, 3)
            Dim r As Double = std.Max(radius, 1.0E-4)
            Dim h As Double = std.Max(height, 1.0E-4)
            Dim vs(seg) As Vector3

            For i As Integer = 0 To seg - 1
                Dim a As Double = 2.0 * std.PI * i / seg
                vs(i) = New Vector3(r * std.Cos(a), -h * 0.5, r * std.Sin(a))
            Next

            vs(seg) = New Vector3(0.0, h * 0.5, 0.0)

            Dim fs As New List(Of Integer())()
            Dim baseFace(seg - 1) As Integer

            For i As Integer = 0 To seg - 1
                fs.Add(New Integer() {seg, i, (i + 1) Mod seg})
                baseFace(seg - 1 - i) = i
            Next

            fs.Add(baseFace)

            Return New ConvexMesh3D(vs, fs.ToArray()).Centered()
        End Function

        ''' <summary>圆柱（局部 Y 轴为轴向），侧面用 <paramref name="segment"/> 边形近似。</summary>
        Public Shared Function CreateCylinder(radius As Double, height As Double, Optional segment As Integer = 12) As ConvexMesh3D
            Dim seg As Integer = std.Max(segment, 3)
            Dim r As Double = std.Max(radius, 1.0E-4)
            Dim h As Double = std.Max(height, 1.0E-4)
            Dim vs(2 * seg - 1) As Vector3

            For i As Integer = 0 To seg - 1
                Dim a As Double = 2.0 * std.PI * i / seg
                Dim cx As Double = r * std.Cos(a)
                Dim cz As Double = r * std.Sin(a)

                vs(i) = New Vector3(cx, -h * 0.5, cz)
                vs(seg + i) = New Vector3(cx, h * 0.5, cz)
            Next

            Dim fs As New List(Of Integer())()

            For i As Integer = 0 To seg - 1
                Dim j As Integer = (i + 1) Mod seg
                fs.Add(New Integer() {i, j, seg + j, seg + i})
            Next

            Dim top(seg - 1) As Integer
            Dim bottom(seg - 1) As Integer

            For i As Integer = 0 To seg - 1
                top(i) = seg + i
                bottom(seg - 1 - i) = i
            Next

            fs.Add(top)
            fs.Add(bottom)

            Return New ConvexMesh3D(vs, fs.ToArray()).Centered()
        End Function

        ''' <summary>
        ''' 楔形斜坡（三角柱）：底面是 <paramref name="width"/>×<paramref name="depth"/> 的矩形，
        ''' 沿 +X 方向从 0 抬升到 <paramref name="height"/>。构造原点位于底面中心。
        ''' </summary>
        Public Shared Function CreateWedge(width As Double, height As Double, depth As Double) As ConvexMesh3D
            Dim w As Double = std.Max(width, 1.0E-4) * 0.5
            Dim h As Double = std.Max(height, 1.0E-4)
            Dim d As Double = std.Max(depth, 1.0E-4) * 0.5
            Dim vs As Vector3() = {
                New Vector3(-w, 0.0, -d), New Vector3(w, 0.0, -d), New Vector3(w, h, -d),
                New Vector3(-w, 0.0, d), New Vector3(w, 0.0, d), New Vector3(w, h, d)
            }
            Dim fs As Integer()() = {
                New Integer() {0, 2, 1},
                New Integer() {3, 4, 5},
                New Integer() {0, 1, 4, 3},
                New Integer() {1, 2, 5, 4},
                New Integer() {0, 3, 5, 2}
            }

            Return New ConvexMesh3D(vs, fs).Centered()
        End Function

        ''' <summary>
        ''' UV 球面网格（仅供渲染：球体在物理侧使用解析的 <see cref="SphereCollider3D"/>）。
        ''' </summary>
        Public Shared Function CreateSphereMesh(radius As Double, Optional segmentU As Integer = 12, Optional segmentV As Integer = 8) As ConvexMesh3D
            Dim su As Integer = std.Max(segmentU, 3)
            Dim sv As Integer = std.Max(segmentV, 2)
            Dim r As Double = std.Max(radius, 1.0E-4)
            Dim rings As Integer = sv - 1
            Dim vs(1 + rings * su) As Vector3

            vs(0) = New Vector3(0.0, r, 0.0)

            For j As Integer = 1 To rings
                Dim theta As Double = std.PI * j / sv
                Dim y As Double = r * std.Cos(theta)
                Dim rr As Double = r * std.Sin(theta)

                For i As Integer = 0 To su - 1
                    Dim phi As Double = 2.0 * std.PI * i / su
                    vs(1 + (j - 1) * su + i) = New Vector3(rr * std.Cos(phi), y, rr * std.Sin(phi))
                Next
            Next

            Dim south As Integer = 1 + rings * su

            vs(south) = New Vector3(0.0, -r, 0.0)

            Dim fs As New List(Of Integer())()

            ' 顶部扇形
            For i As Integer = 0 To su - 1
                fs.Add(New Integer() {0, 1 + i, 1 + (i + 1) Mod su})
            Next

            ' 中间四边形带
            For j As Integer = 0 To rings - 2
                For i As Integer = 0 To su - 1
                    Dim a As Integer = 1 + j * su + i
                    Dim b As Integer = 1 + j * su + (i + 1) Mod su
                    Dim c As Integer = 1 + (j + 1) * su + (i + 1) Mod su
                    Dim d As Integer = 1 + (j + 1) * su + i

                    fs.Add(New Integer() {a, b, c, d})
                Next
            Next

            ' 底部扇形
            Dim lastRing As Integer = 1 + (rings - 1) * su

            For i As Integer = 0 To su - 1
                fs.Add(New Integer() {south, lastRing + (i + 1) Mod su, lastRing + i})
            Next

            Return New ConvexMesh3D(vs, fs.ToArray()).Centered()
        End Function
    End Class
End Namespace
