Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports std = System.Math

''' <summary>
''' 合成测试数据：全部为解析几何，真值已知，便于对算法给出“物理量纲”的断言。
''' </summary>
Friend NotInheritable Class SyntheticData

        Private Sub New()
        End Sub

        ''' <summary>
        ''' XY 平面上的规则栅格（法向朝 +Z）。
        ''' </summary>
        Public Shared Function Plane(width As Integer, height As Integer, spacing As Double,
                                     Optional z As Double = 0.0, Optional noise As Double = 0.0,
                                     Optional seed As Integer = 1) As PointCloudBuffer
            Dim rnd As New Random(seed)
            Dim count As Integer = width * height
            Dim cloud As PointCloudBuffer = PointCloudBuffer.NewCloud(count)
            Dim index As Integer = 0

            For i As Integer = 0 To width - 1
                For j As Integer = 0 To height - 1
                    Dim jitter As Double = If(noise > 0.0, (rnd.NextDouble() - 0.5) * noise, 0.0)

                    cloud.X(index) = i * spacing
                    cloud.Y(index) = j * spacing
                    cloud.Z(index) = z + jitter
                    index += 1
                Next
            Next

            Return cloud
        End Function

        ''' <summary>
        ''' 斐波那契球面（近似均匀采样）。
        ''' </summary>
        Public Shared Function Sphere(radius As Double, count As Integer,
                                      Optional center As Vector3d = Nothing,
                                      Optional noise As Double = 0.0,
                                      Optional seed As Integer = 7) As PointCloudBuffer
            Dim c As Vector3d = center
            Dim rnd As New Random(seed)
            Dim cloud As PointCloudBuffer = PointCloudBuffer.NewCloud(count)
            Dim golden As Double = std.PI * (3.0 - std.Sqrt(5.0))

            For i As Integer = 0 To count - 1
                Dim y As Double = 1.0 - (i / CDbl(std.Max(1, count - 1))) * 2.0
                Dim r As Double = std.Sqrt(std.Max(0.0, 1.0 - y * y))
                Dim theta As Double = golden * i
                Dim jitter As Double = If(noise > 0.0, (rnd.NextDouble() - 0.5) * noise, 0.0)

                cloud.X(i) = c.X + std.Cos(theta) * r * radius + jitter
                cloud.Y(i) = c.Y + std.Sin(theta) * r * radius + jitter
                cloud.Z(i) = c.Z + y * radius + jitter
            Next

            Return cloud
        End Function

        ''' <summary>
        ''' 一个“房间角”场景：地面 + 两面正交墙（三个正交平面约束全部 6 个自由度）。
        ''' </summary>
        Public Shared Function Corner(Optional size As Double = 1.0, Optional spacing As Double = 0.02) As PointCloudBuffer
            Dim n As Integer = CInt(size / spacing) + 1
            Dim floor As PointCloudBuffer = Plane(n, n, spacing, 0.0)
            Dim wallX As PointCloudBuffer = PointCloudBuffer.NewCloud(n * n)
            Dim wallY As PointCloudBuffer = PointCloudBuffer.NewCloud(n * n)
            Dim index As Integer = 0

            For i As Integer = 0 To n - 1
                For k As Integer = 0 To n - 1
                    wallX.X(index) = i * spacing
                    wallX.Y(index) = 0.0
                    wallX.Z(index) = k * spacing

                    wallY.X(index) = 0.0
                    wallY.Y(index) = i * spacing
                    wallY.Z(index) = k * spacing
                    index += 1
                Next
            Next

            Return PointCloudBuffer.Merge(floor, wallX, wallY)
        End Function

        ''' <summary>
        ''' 在点云中加入离群点（远离主体、均匀分布于扩展包围盒）。
        ''' </summary>
        Public Shared Function WithOutliers(cloud As PointCloudBuffer, count As Integer,
                                            Optional seed As Integer = 11) As PointCloudBuffer
            Dim box As Aabb3 = cloud.Bounds().ExpandBy(cloud.Bounds().Diagonal * 0.5)
            Dim rnd As New Random(seed)
            Dim extra As PointCloudBuffer = PointCloudBuffer.NewCloud(count)

            For i As Integer = 0 To count - 1
                extra.X(i) = box.Min.X + rnd.NextDouble() * box.Size.X
                extra.Y(i) = box.Min.Y + rnd.NextDouble() * box.Size.Y
                extra.Z(i) = box.Min.Z + rnd.NextDouble() * box.Size.Z
            Next

            Return PointCloudBuffer.Merge(cloud, extra)
        End Function

        ''' <summary>
        ''' 球的有符号距离场（内部为负），用于验证等值面提取。
        ''' </summary>
        Public Shared Function SphereField(radius As Double, resolution As Integer,
                                           ByRef origin As Vector3d, ByRef spacing As Double) As Double()()()
            spacing = (radius * 2.4) / resolution
            origin = New Vector3d(-radius * 1.2, -radius * 1.2, -radius * 1.2)

            Dim field As Double()()() = New Double(resolution - 1)()() {}

            For i As Integer = 0 To resolution - 1
                field(i) = New Double(resolution - 1)() {}

                For j As Integer = 0 To resolution - 1
                    field(i)(j) = New Double(resolution - 1) {}

                    For k As Integer = 0 To resolution - 1
                        Dim x As Double = origin.X + i * spacing
                        Dim y As Double = origin.Y + j * spacing
                        Dim z As Double = origin.Z + k * spacing

                        field(i)(j)(k) = std.Sqrt(x * x + y * y + z * z) - radius
                    Next
                Next
            Next

            Return field
        End Function

        ''' <summary>
        ''' 从正面渲染一个球的深度图（用于 TSDF 融合测试）。
        ''' </summary>
        Public Shared Function SphereDepthFrame(radius As Double, width As Integer, height As Integer,
                                                pose As RigidTransform, intrinsics As CameraIntrinsics,
                                                Optional distance As Double = 3.0) As DepthFrame
            Dim depth As Double()() = New Double(height - 1)() {}

            For v As Integer = 0 To height - 1
                depth(v) = New Double(width - 1) {}

                For u As Integer = 0 To width - 1
                    ' 相机光线（相机坐标系）
                    Dim rx As Double = (u - intrinsics.Cx) / intrinsics.Fx
                    Dim ry As Double = (v - intrinsics.Cy) / intrinsics.Fy
                    Dim ray As New Vector3d(rx, ry, 1.0)
                    Dim direction As Vector3d = ray.Normalize()

                    ' 相机在世界中的位置与朝向（球位于世界原点）
                    Dim cameraPosition As Vector3d = pose.Translation
                    Dim worldDirection As Vector3d = pose.TransformDirection(direction)
                    Dim sphereCenter As Vector3d = New Vector3d(0, 0, 0)
                    Dim originToCenter As Vector3d = cameraPosition - sphereCenter
                    Dim b As Double = 2.0 * Vector3d.Dot(originToCenter, worldDirection)
                    Dim c As Double = Vector3d.Dot(originToCenter, originToCenter) - radius * radius
                    Dim discriminant As Double = b * b - 4.0 * c

                    If discriminant <= 0.0 Then
                        depth(v)(u) = 0.0

                        Continue For
                    End If

                    Dim t As Double = (-b - std.Sqrt(discriminant)) * 0.5

                    If t <= 0.0 Then
                        depth(v)(u) = 0.0

                        Continue For
                    End If

                    ' 深度取沿光轴的 z 分量
                    depth(v)(u) = t * direction.Z
                Next
            Next

            Return New DepthFrame(depth, intrinsics, pose)
        End Function

        ''' <summary>
        ''' 把点云施加刚体变换（用于构造“已知位姿差”的测试对）。
        ''' </summary>
        Public Shared Function Transformed(cloud As PointCloudBuffer, transform As RigidTransform) As PointCloudBuffer
            Return transform.ApplyToCloud(cloud)
        End Function

End Class
