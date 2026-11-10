Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 体素网格滤波：按固定边长的体素把点云聚成质心，实现“保形降采样”。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>算法</b>：把空间划分成边长 <c>v</c> 的立方体，所有落在同一体素内的点用其质心代替。
    ''' 输出点数约等于“表面面积 / v²”（扫描数据是二维流形），因此采样率随
    ''' <c>v</c> 呈平方关系下降，而不是像随机采样那样按比例下降。
    ''' </para>
    ''' <para>
    ''' <b>为什么工业流水线首选它</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>点分布均匀——后续法向/描述子对采样密度非常敏感，随机采样会在稀疏区产生劣质特征。</description></item>
    ''' <item><description>输出与输入顺序无关（按体素键排序），多次运行结果完全一致，便于检测流程的可追溯性。</description></item>
    ''' <item><description>复杂度 <c>O(n)</c>：一次哈希分桶 + 一次质心归约，没有任何近邻查询。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>属性处理</b>：强度/曲率取体素内的算术平均（它们是连续量）；
    ''' 颜色取体素内第一个点的值（平均值会让边界颜色浑浊）；
    ''' 标签取体素内的众数（标签是离散语义，平均没有意义）；
    ''' 法向取平均后重新归一化（同向法向的平均仍然代表该面元的朝向，
    ''' 前提是调用方已经做过一致定向，否则会互相抵消）。
    ''' </para>
    ''' <para>
    ''' <b>体素尺寸怎么选</b>：经验值是平均点距的 2~5 倍（<see cref="PointCloudBuffer.EstimateAverageSpacing"/>）；
    ''' 太小没有压缩效果，太大会抹掉薄壁、倒角等关键特征。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class VoxelGridFilter

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 体素滤波结果。
        ''' </summary>
        Public Class VoxelGridResult

            ''' <summary>降采样后的点云。</summary>
            Public Property Cloud As PointCloudBuffer

            ''' <summary>输入点数。</summary>
            Public Property InputCount As Integer

            ''' <summary>被占用的体素数量（= 输出点数）。</summary>
            Public Property OccupiedCells As Integer

            ''' <summary>体素边长。</summary>
            Public Property VoxelSize As Double

            ''' <summary>压缩比 <c>输出/输入</c>。</summary>
            Public ReadOnly Property CompressionRatio As Double
                Get
                    If InputCount = 0 Then Return 0.0

                    Return CDbl(OccupiedCells) / InputCount
                End Get
            End Property

            ''' <summary>平均每体素点数（诊断：远大于 1 说明体素偏大或存在大量重复点）。</summary>
            Public ReadOnly Property MeanPointsPerCell As Double
                Get
                    If OccupiedCells = 0 Then Return 0.0

                    Return CDbl(InputCount) / OccupiedCells
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"voxel={VoxelSize:G4}: {InputCount} -> {OccupiedCells} points " &
                    $"(ratio={CompressionRatio:P1}, {MeanPointsPerCell:G2} pts/cell)"
            End Function
        End Class

        ''' <summary>
        ''' 体素降采样（只返回点云，供流水线串接使用）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="voxelSize">体素边长（与点云同单位）</param>
        ''' <param name="options">并行选项（<c>Nothing</c> 表示使用默认）</param>
        Public Shared Function Downsample(cloud As PointCloudBuffer, voxelSize As Double,
                                          Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            Return Apply(cloud, voxelSize, options).Cloud
        End Function

        ''' <summary>
        ''' 体素降采样（返回带诊断信息的结果对象）。
        ''' </summary>
        Public Shared Function Apply(cloud As PointCloudBuffer, voxelSize As Double,
                                     Optional options As CloudyOptions = Nothing) As VoxelGridResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If voxelSize <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(voxelSize), "the voxel size should be positive!")

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)

            If cloud.Count = 0 Then
                Return New VoxelGridResult With {
                    .Cloud = cloud.Clone(),
                    .InputCount = 0,
                    .OccupiedCells = 0,
                    .VoxelSize = voxelSize
                }
            End If

            Dim hash As New VoxelHash(cloud, voxelSize)
            Dim keys As Long() = hash.SortedKeys()
            Dim cells As Integer = keys.Length
            Dim out As PointCloudBuffer = PointCloudBuffer.NewCloud(cells)

            Dim keepNormals As Boolean = cloud.HasNormals
            Dim normalsX As Double() = Nothing
            Dim normalsY As Double() = Nothing
            Dim normalsZ As Double() = Nothing

            If keepNormals Then
                normalsX = GC.AllocateUninitializedArray(Of Double)(cells)
                normalsY = GC.AllocateUninitializedArray(Of Double)(cells)
                normalsZ = GC.AllocateUninitializedArray(Of Double)(cells)
            End If

            Dim intensity As Double() = Nothing
            Dim curvature As Double() = Nothing
            Dim color As Integer() = Nothing
            Dim labels As Integer() = Nothing

            If cloud.Intensity IsNot Nothing Then intensity = GC.AllocateUninitializedArray(Of Double)(cells)
            If cloud.Curvature IsNot Nothing Then curvature = GC.AllocateUninitializedArray(Of Double)(cells)
            If cloud.Color IsNot Nothing Then color = GC.AllocateUninitializedArray(Of Integer)(cells)
            If cloud.Labels IsNot Nothing Then labels = GC.AllocateUninitializedArray(Of Integer)(cells)

            ' 体素之间完全独立：按体素键并行，每个块只写自己的输出下标区间
            Call opt.ForRange(cells,
                Sub(start, ends)
                    For c As Integer = start To ends - 1
                        Dim members As List(Of Integer) = hash.Bucket(keys(c))
                        Dim n As Integer = members.Count

                        Dim sx As Double = 0, sy As Double = 0, sz As Double = 0
                        Dim snx As Double = 0, sny As Double = 0, snz As Double = 0
                        Dim sIntensity As Double = 0, sCurvature As Double = 0

                        For i As Integer = 0 To n - 1
                            Dim p As Integer = members(i)

                            sx += cloud.X(p)
                            sy += cloud.Y(p)
                            sz += cloud.Z(p)

                            If keepNormals Then
                                snx += cloud.Nx(p)
                                sny += cloud.Ny(p)
                                snz += cloud.Nz(p)
                            End If
                            If intensity IsNot Nothing Then sIntensity += cloud.Intensity(p)
                            If curvature IsNot Nothing Then sCurvature += cloud.Curvature(p)
                        Next

                        Dim inv As Double = 1.0 / n

                        out.X(c) = sx * inv
                        out.Y(c) = sy * inv
                        out.Z(c) = sz * inv

                        If keepNormals Then
                            Dim len As Double = std.Sqrt(snx * snx + sny * sny + snz * snz)

                            If len > 1.0E-12 Then
                                normalsX(c) = snx / len
                                normalsY(c) = sny / len
                                normalsZ(c) = snz / len
                            Else
                                ' 法向互相抵消（未定向或曲面折叠）：保留第一个点的法向，避免输出零向量
                                Dim p As Integer = members(0)

                                normalsX(c) = cloud.Nx(p)
                                normalsY(c) = cloud.Ny(p)
                                normalsZ(c) = cloud.Nz(p)
                            End If
                        End If

                        If intensity IsNot Nothing Then intensity(c) = sIntensity * inv
                        If curvature IsNot Nothing Then curvature(c) = sCurvature * inv
                        If color IsNot Nothing Then color(c) = cloud.Color(members(0))
                        If labels IsNot Nothing Then labels(c) = MajorityLabel(cloud.Labels, members)
                    Next
                End Sub, 256)

            If keepNormals Then Call out.SetNormals(normalsX, normalsY, normalsZ)

            out.Intensity = intensity
            out.Curvature = curvature
            out.Color = color
            out.Labels = labels

            Return New VoxelGridResult With {
                .Cloud = out,
                .InputCount = cloud.Count,
                .OccupiedCells = cells,
                .VoxelSize = voxelSize
            }
        End Function

        ''' <summary>
        ''' 体素内出现次数最多的标签（并列时取较小标签，保证确定性）。
        ''' </summary>
        ''' <remarks>
        ''' 体素内的点数很少（通常 1~20），这里用“线性扫描计数”而不是哈希表：
        ''' 对小集合，无分配的 O(n²) 比较远快于建表。
        ''' </remarks>
        Private Shared Function MajorityLabel(labels As Integer(), members As List(Of Integer)) As Integer
            Dim bestLabel As Integer = labels(members(0))
            Dim bestCount As Integer = 0
            Dim n As Integer = members.Count

            For i As Integer = 0 To n - 1
                Dim label As Integer = labels(members(i))
                Dim count As Integer = 0

                For j As Integer = 0 To n - 1
                    If labels(members(j)) = label Then count += 1
                Next

                If count > bestCount OrElse (count = bestCount AndAlso label < bestLabel) Then
                    bestCount = count
                    bestLabel = label
                End If
            Next

            Return bestLabel
        End Function

    End Class
End Namespace
