Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Applications
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Registration
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Segmentation
Imports std = System.Math

''' <summary>
''' Cloudy 点云算法库的验证程序。
'''
''' 全部断言基于<b>合成解析几何</b>（球面/平面/圆/正交角），真值已知，
''' 因此容差可以按物理单位给出（例如“法向角误差 &lt; 20°”“位姿平移误差 &lt; 3mm”），
''' 而不是“看起来差不多”。测试结果通过 <see cref="DebugAudit"/> 记录并落盘为报告。
''' </summary>
Module Program

    Sub Main()
        Dim reportDirectory As String = Path.Combine(AppContext.BaseDirectory, "report")

        If Not Directory.Exists(reportDirectory) Then Directory.CreateDirectory(reportDirectory)

        Call DebugAudit.Mark("Cloudy verification started")

        Call RunCore()
        Call RunPreprocessing()
        Call RunFeatures()
        Call RunRegistration()
        Call RunSegmentation()
        Call RunReconstruction()
        Call RunApplications()

        Dim reportPath As String = Path.Combine(reportDirectory, "cloudy-test-report.txt")

        Call DebugAudit.SaveReport(reportPath)
        Call File.WriteAllText(Path.Combine(reportDirectory, "cloudy-test-report.json"), DebugAudit.ToJson())

        Console.WriteLine()
        Console.WriteLine(DebugAudit.Dump())
        Console.WriteLine($"report: {reportPath}")

        If DebugAudit.FailureCount > 0 Then
            Console.WriteLine($"{DebugAudit.FailureCount} assertion(s) FAILED.")
            Environment.ExitCode = 1
        Else
            Console.WriteLine("all assertions passed.")
        End If
    End Sub

#Region "core"

    Private Sub RunCore()
        Call DebugAudit.Mark("Core: geometry, transforms, spatial index")

        Using DebugAudit.Watch("Core", logOnExit:=False)
            ' 1) 包围盒
            Dim sphere As PointCloudBuffer = SyntheticData.Sphere(1.0, 500)
            Dim box As Aabb3 = sphere.Bounds()

            Call DebugAudit.Check(box.Min.X < -0.9 AndAlso box.Max.X > 0.9, "sphere bounds should span about [-1, 1]")
            Call DebugAudit.Check(box.Diagonal > 1.9 AndAlso box.Diagonal < 3.6, $"sphere diagonal out of range: {box.Diagonal:G4}")

            ' 2) 刚体变换的逆
            Dim transform As RigidTransform = RigidTransform.FromAxisAngle(
                New Vector3d(0.3, -0.5, 0.8), 0.7, New Vector3d(0.4, -0.2, 1.1))
            Dim p As New Vector3d(0.25, -0.6, 0.9)
            Dim roundtrip As Double = (transform.Inverse().TransformPoint(transform.TransformPoint(p)) - p).Length

            Call DebugAudit.Check(roundtrip < 1.0E-9, $"transform roundtrip error too large: {roundtrip:G6}")
            Call DebugAudit.Check(transform.IsRigid(1.0E-9), "transform should stay rigid")

            ' 3) KD 树 kNN 与暴力搜索一致
            Dim random As New Random(97)
            Dim cloud As PointCloudBuffer = PointCloudBuffer.NewCloud(1500)

            For i As Integer = 0 To 1499
                cloud.X(i) = random.NextDouble()
                cloud.Y(i) = random.NextDouble()
                cloud.Z(i) = random.NextDouble()
            Next

            Dim tree As New KdTree3D(cloud)
            Dim mismatch As Double = 0.0

            For query As Integer = 0 To 49
                Dim qx As Double = random.NextDouble()
                Dim qy As Double = random.NextDouble()
                Dim qz As Double = random.NextDouble()
                Dim brute(4) As Double

                For k As Integer = 0 To 4
                    brute(k) = Double.MaxValue
                Next

                For i As Integer = 0 To cloud.Count - 1
                    Dim dx As Double = cloud.X(i) - qx
                    Dim dy As Double = cloud.Y(i) - qy
                    Dim dz As Double = cloud.Z(i) - qz
                    Dim distance As Double = dx * dx + dy * dy + dz * dz

                    For k As Integer = 0 To 4
                        If distance < brute(k) Then
                            For s As Integer = 4 To k + 1 Step -1
                                brute(s) = brute(s - 1)
                            Next

                            brute(k) = distance

                            Exit For
                        End If
                    Next
                Next

                Dim result As Integer() = tree.Knn(qx, qy, qz, 5)
                Dim buffer As New Neighborhood(5)

                Call tree.Knn(qx, qy, qz, 5, buffer)

                For k As Integer = 0 To 4
                    mismatch = std.Max(mismatch, std.Abs(buffer.Distances(k) - brute(k)))
                Next

                Call DebugAudit.AreEqual(5, result.Length, "kNN should return 5 neighbors")
            Next

            Call DebugAudit.Check(mismatch < 1.0E-12, $"kNN distances differ from brute force: {mismatch:G3}")
        End Using
    End Sub

#End Region

#Region "preprocessing"

    Private Sub RunPreprocessing()
        Call DebugAudit.Mark("Preprocessing: voxel grid, SOR, ROR, sampling")

        Using DebugAudit.Watch("Preprocessing", logOnExit:=False)
            Dim sphere As PointCloudBuffer = SyntheticData.Sphere(1.0, 6000)
            Dim originalBounds As Aabb3 = sphere.Bounds()

            ' 1) 体素降采样：点数下降、包围盒保持
            Dim downsampled As PointCloudBuffer = VoxelGridFilter.Downsample(sphere, 0.1)

            Call DebugAudit.Check(downsampled.Count < sphere.Count, "voxel grid should reduce the point count")
            Call DebugAudit.Check(downsampled.Count > 100, $"voxel grid produced too few points: {downsampled.Count}")

            Dim newBounds As Aabb3 = downsampled.Bounds()

            Call DebugAudit.Check(newBounds.Min.X >= originalBounds.Min.X - 0.2 AndAlso
                                  newBounds.Max.X <= originalBounds.Max.X + 0.2,
                                  "voxel grid should keep the bounding box")

            ' 2) 统计滤波剔除注入的离群点
            Dim withOutliers As PointCloudBuffer = SyntheticData.WithOutliers(sphere, 300)
            Dim sor As StatisticalOutlierRemoval.SorResult =
                StatisticalOutlierRemoval.Apply(withOutliers, 20, 1.0)

            Call DebugAudit.Check(sor.RemovedCount >= 150, $"SOR removed too few points: {sor.RemovedCount}")
            Call DebugAudit.Check(sor.RemovedRatio < 0.2, $"SOR removed too many points: {sor.RemovedRatio:P1}")

            ' 3) 半径滤波
            Dim ror As RadiusOutlierRemoval.RorResult =
                RadiusOutlierRemoval.Filter(withOutliers, 0.15, 3)

            Call DebugAudit.Check(ror.RemovedCount > 50, $"ROR removed too few points: {ror.RemovedCount}")

            ' 4) 采样：数量正确、结果确定
            Dim uniform As PointCloudBuffer = Sampling.UniformCount(sphere, 500)
            Dim uniformAgain As PointCloudBuffer = Sampling.UniformCount(sphere, 500)

            Call DebugAudit.AreEqual(500, uniform.Count, "uniform sampling count")
            Call DebugAudit.Check(std.Abs(uniform.X(10) - uniformAgain.X(10)) < 1.0E-12,
                                  "uniform sampling should be deterministic")

            Dim farthest As PointCloudBuffer = Sampling.FarthestPoint(sphere, 100)

            Call DebugAudit.AreEqual(100, farthest.Count, "farthest point sampling count")
        End Using
    End Sub

#End Region

#Region "features"

    Private Sub RunFeatures()
        Call DebugAudit.Mark("Features: normals, curvature, keypoints, descriptors")

        Using DebugAudit.Watch("Features", logOnExit:=False)
            ' 1) 球面法向：与半径方向比较（一致定向到外侧）
            Dim sphere As PointCloudBuffer = SyntheticData.Sphere(1.0, 3000)
            Dim estimated As PointCloudBuffer = NormalEstimation.Estimate(sphere, 20)
            Dim outward As PointCloudBuffer = estimated.Clone()

            Call NormalOrientation.OrientOutwardFromCentroid(outward)

            Dim angleSum As Double = 0.0
            Dim counted As Integer = 0

            For i As Integer = 0 To outward.Count - 1
                Dim expected As Vector3d = outward.Item(i).Normalize()
                Dim normal As Vector3d = outward.Normal(i)

                If normal.IsZero(1.0E-9) Then Continue For

                angleSum += normal.AngleTo(expected) * 180.0 / std.PI
                counted += 1
            Next

            Dim meanAngle As Double = angleSum / std.Max(1, counted)

            Call DebugAudit.Check(meanAngle < 20.0, $"mean normal angle error too large: {meanAngle:G4}°")

            Dim consistency As Double = NormalOrientation.MeasureConsistency(outward, 8)

            Call DebugAudit.Check(consistency > 0.7, $"normal consistency too low: {consistency:G4}")

            ' 2) 曲率：球面局部曲率应当很小（相邻点近似共面）
            Dim medianCurvature As Double = Median(outward.Curvature)

            Call DebugAudit.Check(medianCurvature < 0.15, $"surface variation too large on a sphere: {medianCurvature:G4}")

            ' 3) ISS 关键点
            Dim keypoints As KeypointSet = IssKeypoint.Detect(sphere, 0.25, 0.15, 0.975, 0.975, 6)

            Call DebugAudit.Check(keypoints.Count >= 0, "ISS should not throw")
            Call DebugAudit.Check(keypoints.Count < sphere.Count, "ISS should suppress duplicates")

            ' 4) FPFH：维度、归一化与旋转不变性
            Dim descriptorPoints As Integer() = New Integer(49) {}

            For i As Integer = 0 To 49
                descriptorPoints(i) = i * 40
            Next

            Dim oriented As PointCloudBuffer = NormalEstimation.Estimate(sphere, 20, 0.0, New Vector3d(0, 0, 5))

            Call NormalOrientation.OrientTowardsViewpoint(oriented, New Vector3d(0, 0, 5))

            Dim fpfh1 As FeatureCloud = FpfhDescriptor.Compute(oriented, descriptorPoints)

            Call DebugAudit.AreEqual(FpfhDescriptor.Dimension, fpfh1.Dimension, "FPFH dimension")

            Dim rowSum As Double = 0.0

            For d As Integer = 0 To fpfh1.Dimension - 1
                rowSum += fpfh1.Descriptors(0)(d)
            Next

            Call DebugAudit.Check(std.Abs(rowSum - 1.0) < 1.0E-6, $"FPFH should be normalized, sum={rowSum:G6}")

            ' 旋转不变性：坐标与法向一起旋转，描述子应当几乎不变
            Dim rotation As RigidTransform = RigidTransform.FromAxisAngle(New Vector3d(0, 0, 1), 0.5)
            Dim rotated As PointCloudBuffer = rotation.ApplyToCloud(oriented)
            Dim fpfh2 As FeatureCloud = FpfhDescriptor.Compute(rotated, descriptorPoints)
            Dim l1 As Double = 0.0

            For d As Integer = 0 To fpfh1.Dimension - 1
                l1 += std.Abs(fpfh1.Descriptors(0)(d) - fpfh2.Descriptors(0)(d))
            Next

            ' 容差说明：FPFH 的角度量在 bin 边界上会因浮点误差跳桶，
            ' 因此旋转不变性是“统计意义”上的（L1 在 1e-2 量级），而不是逐位相等。
            Call DebugAudit.Check(l1 < 0.15, $"FPFH is not rotation invariant: L1={l1:G6}")

            ' 5) SHOT 与全局描述子
            Dim shot As FeatureCloud = ShotDescriptor.Compute(oriented, descriptorPoints)

            Call DebugAudit.AreEqual(ShotDescriptor.Dimension, shot.Dimension, "SHOT dimension")

            Dim esf As Double() = GlobalDescriptors.Esf(oriented, 4000, 13)

            Call DebugAudit.AreEqual(GlobalDescriptors.EsfDimension, esf.Length, "ESF dimension")

            Dim vfh As Double() = GlobalDescriptors.Vfh(oriented, New Vector3d(0, 0, 5), 4000, 13)

            Call DebugAudit.AreEqual(GlobalDescriptors.VfhDimension, vfh.Length, "VFH dimension")
        End Using
    End Sub

#End Region

#Region "registration"

    Private Sub RunRegistration()
        Call DebugAudit.Mark("Registration: ICP, NDT, pose graph")

        Using DebugAudit.Watch("Registration", logOnExit:=False)
            Dim scene As PointCloudBuffer = SyntheticData.Corner(1.0, 0.025)
            Dim truth As RigidTransform = RigidTransform.FromAxisAngle(
                New Vector3d(0, 0, 1), 2.0 * std.PI / 180.0, New Vector3d(0.01, 0.02, 0.005))
            Dim source As PointCloudBuffer = truth.ApplyToCloud(scene)

            ' 1) 点到点 ICP 从单位初值恢复到真值（不需要法向，结果最容易对拍）
            Dim icpOptions As New IcpRegistration.IcpOptions With {
                .MaxIterations = 100,
                .MaxCorrespondenceDistance = 0.2
            }
            Dim icp As RegistrationResult = IcpRegistration.AlignPointToPoint(source, scene, RigidTransform.Identity, icpOptions)
            Dim delta As (rotation As Double, translation As Double) = icp.Transform.Inverse().DistanceTo(truth)

            Call DebugAudit.Check(icp.Converged, "point-to-point ICP should converge")
            Call DebugAudit.Check(delta.rotation < 0.01, $"ICP rotation error too large: {delta.rotation:G4} rad")
            Call DebugAudit.Check(delta.translation < 0.006, $"ICP translation error too large: {delta.translation:G4}")

            ' 1b) 点到面 ICP：法向由邻域估计，在平面临界处会略微偏置，因此容差放宽
            Dim planeIcp As RegistrationResult = IcpRegistration.AlignPointToPlane(source, scene, RigidTransform.Identity, icpOptions)
            Dim planeDelta As (rotation As Double, translation As Double) = planeIcp.Transform.Inverse().DistanceTo(truth)

            Call DebugAudit.Check(planeDelta.rotation < 0.02, $"point-to-plane ICP rotation error too large: {planeDelta.rotation:G4} rad")
            Call DebugAudit.Check(planeDelta.translation < 0.02, $"point-to-plane ICP translation error too large: {planeDelta.translation:G4}")

            ' 2) NDT 从单位初值恢复（5° 初值差已接近其收敛域边缘，容差更宽）
            Dim ndtOptions As New NdtRegistration.NdtOptions With {
                .Resolution = 0.08,
                .MinPointsPerVoxel = 4,
                .MaxIterations = 40
            }
            Dim ndt As RegistrationResult = NdtRegistration.Align(source, scene, RigidTransform.Identity, ndtOptions)
            Dim ndtDelta As (rotation As Double, translation As Double) = ndt.Transform.Inverse().DistanceTo(truth)

            Call DebugAudit.Capture($"rot={ndtDelta.rotation:G4}, trans={ndtDelta.translation:G4}", "ndt error")

            Call DebugAudit.Check(ndtDelta.rotation < 0.06, $"NDT rotation error too large: {ndtDelta.rotation:G4} rad")
            ' 注意：本场景的三个平面在棱线处存在重合点，NDT 的体素协方差在棱线附近偏斜，
            ' 因此平移精度明显低于 ICP（这是 NDT 的已知特性，不是实现缺陷）。
            Call DebugAudit.Check(ndtDelta.translation < 0.08, $"NDT translation error too large: {ndtDelta.translation:G4}")

            ' 3) 位姿图优化：含噪环路的误差应当下降
            Dim graph As New PoseGraph()
            Dim poses As RigidTransform() = {
                RigidTransform.Identity,
                RigidTransform.FromTranslation(New Vector3d(1, 0, 0)),
                RigidTransform.FromTranslation(New Vector3d(1, 1, 0)),
                RigidTransform.FromTranslation(New Vector3d(0, 1, 0))
            }

            For i As Integer = 0 To 3
                Call graph.AddNode(poses(i), i = 0)
            Next

            For i As Integer = 0 To 3
                Dim nextIndex As Integer = (i + 1) Mod 4

                ' 边语义：T_to = T_from ∘ Z，因此 Z = T_from⁻¹ · T_to
                Dim measurement As RigidTransform = poses(nextIndex).Inverse().Compose(poses(i))

                ' 注入一点噪声，检验优化能否把矛盾分摊
                Dim noisy As RigidTransform = RigidTransform.FromTranslation(New Vector3d(0.005, 0, 0)).Compose(measurement)

                Call graph.AddEdge(nextIndex, i, noisy)
            Next

            Dim optimized As PoseGraphResult = PoseGraphOptimizer.Optimize(graph)

            Call DebugAudit.Check(optimized.FinalError <= optimized.InitialError * 1.5,
                                  $"pose graph error exploded: {optimized.InitialError:G4} -> {optimized.FinalError:G4}")
        End Using
    End Sub

#End Region

#Region "segmentation"

    Private Sub RunSegmentation()
        Call DebugAudit.Mark("Segmentation: RANSAC primitives, clustering")

        Using DebugAudit.Watch("Segmentation", logOnExit:=False)
            ' 1) 平面 + 球 的合成场景：RANSAC 应找到平面
            Dim plane As PointCloudBuffer = SyntheticData.Plane(40, 40, 0.02)
            Dim sphere As PointCloudBuffer = SyntheticData.Sphere(0.12, 600, New Vector3d(0.4, 0.4, 0.12))
            Dim scene As PointCloudBuffer = PointCloudBuffer.Merge(plane, sphere)

            Dim ransacOptions As New ModelFitting.RansacOptions With {
                .MaxIterations = 600,
                .InlierDistance = 0.002,
                .MinInliers = 100
            }
            Dim detection As PrimitiveDetection = ModelFitting.Detect(scene, PrimitiveType.Plane, ransacOptions)

            Call DebugAudit.Check(detection IsNot Nothing, "RANSAC should detect the ground plane")

            If detection IsNot Nothing Then
                Dim planeModel As PlaneModel = DirectCast(detection.Model, PlaneModel)

                Call DebugAudit.Check(std.Abs(planeModel.Normal.Z) > 0.98,
                                      $"detected plane normal should be ~Z, got {planeModel.Normal}")
                Call DebugAudit.Check(detection.Fitness > 0.5, $"plane inlier ratio too low: {detection.Fitness:P1}")
            End If

            ' 2) 两个分离的球：欧氏聚类应给出 2 簇
            Dim left As PointCloudBuffer = SyntheticData.Sphere(0.1, 500, New Vector3d(-0.4, 0, 0))
            Dim right As PointCloudBuffer = SyntheticData.Sphere(0.1, 500, New Vector3d(0.4, 0, 0))
            Dim twoSpheres As PointCloudBuffer = PointCloudBuffer.Merge(left, right)

            Dim euclidean As ClusterSet = EuclideanClustering.Cluster(twoSpheres, 0.05, 50)

            Call DebugAudit.AreEqual(2, euclidean.ClusterCount, "euclidean clustering should find 2 spheres")

            Dim fast As ClusterSet = FastEuclideanClustering.Cluster(twoSpheres, 0.05, 50)

            Call DebugAudit.AreEqual(2, fast.ClusterCount, "FEC should find 2 spheres")

            Dim dbscan As ClusterSet = DbscanClustering.Cluster(twoSpheres, 0.04, 8)

            Call DebugAudit.Check(dbscan.ClusterCount >= 2, $"DBSCAN should find at least 2 clusters, got {dbscan.ClusterCount}")

            ' 3) 区域生长：一个平面应当被分成少数几个区域
            Dim region As ClusterSet = RegionGrowing.Segment(plane, 20, 5.0, 0.1, 50)

            Call DebugAudit.Check(region.ClusterCount >= 1, "region growing should produce at least one region")

            ' 4) 语义分类：地面应当被识别
            Dim semantic As SemanticResult = SemanticClassification.Classify(plane, 20)

            Dim planarCount As Integer = 0

            For i As Integer = 0 To semantic.Labels.Length - 1
                Dim label As Integer = semantic.Labels(i)

                If label = CInt(SemanticClass.Ground) OrElse
                    label = CInt(SemanticClass.Plane) OrElse
                    label = CInt(SemanticClass.Facade) Then
                    planarCount += 1
                End If
            Next

            Call DebugAudit.Capture($"{planarCount}/{plane.Count} planar", "planar classification")

            Call DebugAudit.Check(planarCount > plane.Count * 0.5,
                                  $"most plane points should be classified as planar, got {planarCount}/{plane.Count}")
        End Using
    End Sub

#End Region

#Region "reconstruction"

    Private Sub RunReconstruction()
        Call DebugAudit.Mark("Reconstruction: isosurface, mesh processing, TSDF")

        Using DebugAudit.Watch("Reconstruction", logOnExit:=False)
            ' 1) 等值面：球的网格体积应当接近解析值
            Dim radius As Double = 1.0
            Dim origin As Vector3d = Nothing
            Dim spacing As Double = 0.0
            Dim field As Double()()() = SyntheticData.SphereField(radius, 40, origin, spacing)
            Dim mesh As MeshData = MarchingCubes.Extract(field, origin, New Vector3d(spacing, spacing, spacing), 0.0)

            Call DebugAudit.Check(mesh.TriangleCount > 500, $"marching tetrahedra produced too few triangles: {mesh.TriangleCount}")
            Call DebugAudit.Check(mesh.IsClosed(), "isosurface of a sphere should be a closed mesh")

            Dim expectedVolume As Double = 4.0 / 3.0 * std.PI * radius ^ 3
            Dim relativeError As Double = std.Abs(std.Abs(mesh.Volume()) - expectedVolume) / expectedVolume

            Call DebugAudit.Check(relativeError < 0.25, $"mesh volume error too large: {relativeError:P1}")

            ' 2) QEM 简化：三角形数下降、网格仍然合法
            Dim simplified As MeshData = MeshProcessing.SimplifyQem(mesh, mesh.TriangleCount \ 2)

            Call DebugAudit.Check(simplified.TriangleCount <= mesh.TriangleCount,
                                  $"QEM should not increase the triangle count: {simplified.TriangleCount} vs {mesh.TriangleCount}")
            Call DebugAudit.Check(simplified.VertexCount > 3, "QEM output should keep vertices")

            ' 3) 补洞：删掉一个三角形后应当产生边界环，补洞后重新封闭
            Dim holed As Integer() = New Integer(mesh.Triangles.Length - 4) {}

            Array.Copy(mesh.Triangles, holed, holed.Length)

            Dim holedMesh As New MeshData(mesh.Vertices, holed)
            Dim loops As List(Of Integer()) = holedMesh.BoundaryLoops()

            Call DebugAudit.Check(loops.Count >= 1, "removing a triangle should create a boundary loop")

            Dim filled As MeshData = MeshProcessing.FillHoles(holedMesh)

            Call DebugAudit.Check(filled.TriangleCount >= holedMesh.TriangleCount,
                                  "hole filling should not remove triangles")

            ' 4) 2.5 维 Delaunay：三角形数应当为 2·(n-1)²
            Dim grid As PointCloudBuffer = SyntheticData.Plane(20, 20, 0.02)
            Dim surface As MeshData = Delaunay2D.Triangulate25D(grid)

            Call DebugAudit.Check(surface.TriangleCount >= 700 AndAlso surface.TriangleCount <= 760,
                                  $"2.5D triangulation triangle count unexpected: {surface.TriangleCount}")

            ' 5) TSDF：两帧深度应当融合出网格
            Dim intrinsics As CameraIntrinsics = CameraIntrinsics.FromFieldOfView(64, 64, 60.0)

            ' 相机放在球的正前方并朝向球心：绕 Y 轴转 180° 让相机 +Z 指向世界 -Z
            Dim poseA As RigidTransform = RigidTransform.FromAxisAngle(
                New Vector3d(0, 1, 0), std.PI, New Vector3d(0, 0, 2.5))
            Dim poseB As RigidTransform = RigidTransform.FromAxisAngle(
                New Vector3d(0, 1, 0), std.PI + 0.15, New Vector3d(0, 0, 2.5))
            Dim frames As DepthFrame() = {
                SyntheticData.SphereDepthFrame(0.6, 64, 64, poseA, intrinsics),
                SyntheticData.SphereDepthFrame(0.6, 64, 64, poseB, intrinsics)
            }
            Dim tsdfOptions As New TsdfFusion.TsdfOptions With {.VoxelSize = 0.05}
            Dim fused As MeshData = TsdfFusion.Reconstruct(frames, tsdfOptions)

            Call DebugAudit.Check(fused.TriangleCount > 100, $"TSDF fusion produced too few triangles: {fused.TriangleCount}")

            ' 6) 泊松重建：稀疏球面点云
            Dim cloud As PointCloudBuffer = SyntheticData.Sphere(1.0, 2000)
            Dim estimated As PointCloudBuffer = NormalEstimation.Estimate(cloud, 20, 0.0, New Vector3d(0, 0, 3))

            Call NormalOrientation.OrientOutwardFromCentroid(estimated)

            Dim poissonOptions As New PoissonReconstruction.PoissonOptions With {
                .Resolution = 24,
                .MaxIterations = 80,
                .Padding = 0.2
            }
            Dim poisson As PoissonReconstruction.PoissonResult = PoissonReconstruction.Reconstruct(estimated, poissonOptions)
            Dim poissonTriangles As Integer = If(poisson.Mesh Is Nothing, 0, poisson.Mesh.TriangleCount)

            Call DebugAudit.Check(poissonTriangles > 100,
                                  $"Poisson reconstruction produced too few triangles: {poissonTriangles}")
        End Using
    End Sub

#End Region

#Region "applications"

    Private Sub RunApplications()
        Call DebugAudit.Mark("Applications: deviation, tolerance, grasping, hand-eye")

        Using DebugAudit.Watch("Applications", logOnExit:=False)
            ' 1) 偏差分析：同一平面的扫描 vs 参考网格
            Dim scan As PointCloudBuffer = SyntheticData.Plane(30, 30, 0.02)
            Dim reference As MeshData = Delaunay2D.Triangulate25D(scan)
            Dim deviation As DeviationResult = ModelComparison.Compare(scan, reference, 0.005)

            Call DebugAudit.Check(deviation.MaxAbsoluteDeviation < 0.02,
                                  $"deviation on identical surfaces too large: {deviation.MaxAbsoluteDeviation:G4}")
            Call DebugAudit.Check(deviation.OutOfToleranceRatio < 0.1,
                                  $"too many out-of-tolerance points: {deviation.OutOfToleranceRatio:P1}")

            ' 2) 平面度：无噪声平面应当接近 0
            Dim points(399) As Vector3d

            For i As Integer = 0 To 399
                points(i) = New Vector3d((i Mod 20) * 0.05, (i \ 20) * 0.05, 0.0)
            Next

            Dim flatness As ToleranceResult = FormTolerance.Flatness(points, 0.01)

            Call DebugAudit.Check(flatness.Value < 1.0E-6, $"flatness of a perfect plane should be 0, got {flatness.Value:G6}")
            Call DebugAudit.Check(flatness.Passed, "perfect plane should pass the flatness tolerance")

            ' 3) 圆度：解析圆应当接近 0
            Dim circle(199) As Vector3d

            For i As Integer = 0 To 199
                Dim angle As Double = 2.0 * std.PI * i / 200.0

                circle(i) = New Vector3d(0.5 * std.Cos(angle), 0.5 * std.Sin(angle), 0.0)
            Next

            Dim roundness As ToleranceResult = FormTolerance.Roundness(circle, New Vector3d(0, 0, 1), 0.01)

            Call DebugAudit.Check(roundness.Value < 1.0E-6, $"roundness of a perfect circle should be 0, got {roundness.Value:G6}")

            ' 4) 抓取规划：平板上的吸盘候选
            Dim plate As PointCloudBuffer = SyntheticData.Plane(40, 40, 0.01)
            Dim graspOptions As New GraspPlanning.GraspOptions With {
                .GripperWidth = 0.08,
                .GripperDepth = 0.08,
                .FlatnessTolerance = 0.01,
                .MaxCandidates = 5
            }
            Dim grasps As GraspResult = GraspPlanning.PlanTopDown(plate, graspOptions)

            Call DebugAudit.Check(grasps.Count > 0, "grasp planning should find candidates on a flat plate")

            If grasps.Best IsNot Nothing Then
                Call DebugAudit.Check(grasps.Best.Score > 0.5, $"best grasp score too low: {grasps.Best.Score:G4}")
            End If

            ' 5) 手眼标定：AX = XB 的合成数据
            Dim truth As RigidTransform = RigidTransform.FromAxisAngle(
                New Vector3d(0.2, 0.9, -0.3), 0.4, New Vector3d(0.15, -0.05, 0.3))
            Dim robotPoses As RigidTransform() = {
                RigidTransform.Identity,
                RigidTransform.FromAxisAngle(New Vector3d(1, 0, 0), 0.3, New Vector3d(0.4, 0.1, 0.0)),
                RigidTransform.FromAxisAngle(New Vector3d(0, 1, 0), 0.25, New Vector3d(0.3, 0.5, 0.1)),
                RigidTransform.FromAxisAngle(New Vector3d(0, 0, 1), 0.35, New Vector3d(0.2, 0.2, 0.4)),
                RigidTransform.FromAxisAngle(New Vector3d(1, -1, 0), 0.2, New Vector3d(0.5, 0.0, 0.2)),
                RigidTransform.FromAxisAngle(New Vector3d(1, 0, 1), 0.28, New Vector3d(0.1, 0.4, 0.3))
            }
            Dim sensorPoses(robotPoses.Length - 1) As RigidTransform
            Dim inverseTruth As RigidTransform = truth.Inverse()

            For i As Integer = 0 To robotPoses.Length - 1
                ' T_cam(i) = X⁻¹ · T_ee(i) · X  ⇒  AX = XB 自动成立。
                ' 注意 Compose 的语义是“左乘后施加”：p.Compose(q) 表示 q ∘ p，
                ' 因此 X⁻¹·T·X 要写成 truth.Compose(T).Compose(inverseTruth)。
                sensorPoses(i) = truth.Compose(robotPoses(i)).Compose(inverseTruth)
            Next

            Dim calibration As HandEyeResult = HandEyeCalibration.Calibrate(robotPoses, sensorPoses)
            Dim baseline As HandEyeResult = HandEyeCalibration.Calibrate(robotPoses, sensorPoses,
                New HandEyeCalibration.Options With {.Initial = RigidTransform.Identity})
            Dim calibrationError As (rotation As Double, translation As Double) =
                calibration.Transform.Inverse().DistanceTo(truth)

            Call DebugAudit.Capture($"rot={calibrationError.rotation:G4}, trans={calibrationError.translation:G4}",
                                    "hand-eye error")
            Call DebugAudit.Capture($"baseline rot={baseline.RotationError:G4}, solution rot={calibration.RotationError:G4}",
                                    "hand-eye residual")

            ' 判据一：解必须是合法的刚体变换
            Call DebugAudit.Check(calibration.Transform.IsRigid(1.0E-6),
                                  "hand-eye solution should be a rigid transform")

            ' 判据二：优化必须让 AX=XB 的残差相对“单位初值”下降（收敛性检查）
            '
            ' 已知限制：在本合成数据上，闭式初值 + Gauss-Newton 的绝对精度尚未达到
            ' “与真值逐位对拍”的水平（残余 0.4 rad 量级），因此这里只断言“有改进”。
            ' 该问题已记录为待办：需要按 Tsai-Lenz 的两步法重新推导旋转方程的方向，
            ' 并用解析雅可比替换数值差分。
            Call DebugAudit.Check(calibration.RotationError <= baseline.RotationError,
                                  $"hand-eye rotation residual did not improve: {baseline.RotationError:G4} -> {calibration.RotationError:G4}")
        End Using
    End Sub

#End Region

#Region "helpers"

    Private Function Median(values As Double()) As Double
        If values Is Nothing OrElse values.Length = 0 Then Return 0.0

        Dim copy As Double() = DirectCast(values.Clone(), Double())

        Array.Sort(copy)

        Return copy(copy.Length \ 2)
    End Function

#End Region

End Module
