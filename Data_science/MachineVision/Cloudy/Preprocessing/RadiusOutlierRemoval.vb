Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 半径滤波(ROR)：剔除半径 <c>r</c> 内邻点少于 <c>k</c> 个的孤立点。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <see cref="StatisticalOutlierRemoval"/> 的区别：
    ''' SOR 只看“平均邻域距离”的全局分布，对密度分层（近处 1mm、远处 5mm）时会误杀远处合法点；
    ''' ROR 直接用一个<b>绝对尺度</b>判断“这个点周围还有没有东西”，
    ''' 对“飞点/孤立杂点”的判定更直观，也与物理尺寸（例如“零件最小壁厚 3mm”）直接对应。
    ''' </para>
    ''' <para>
    ''' <b>加速</b>：用 <see cref="VoxelHash"/> 并以 <c>cellSize = r</c> 建桶，
    ''' 每次查询只需检查 27 个相邻体素（而不是全点集或 KD 树的多次下降），
    ''' 对百万级点云是线性时间。
    ''' </para>
    ''' <para>
    ''' <b>自适应半径</b>：当点云密度未知时传 <c>radius ≤ 0</c>，
    ''' 会用 <see cref="PointCloudBuffer.EstimateAverageSpacing"/> 反推一个半径
    ''' （间距 × <c>radiusMultiplier</c>，默认 3）。
    ''' 这只是“合理的起点”，真正的阈值仍应由工艺尺寸决定。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RadiusOutlierRemoval

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 半径滤波结果。
        ''' </summary>
        Public Class RorResult

            ''' <summary>过滤后的点云。</summary>
            Public Property Cloud As PointCloudBuffer

            ''' <summary>保留掩码。</summary>
            Public Property Mask As Boolean()

            ''' <summary>逐点在半径内的邻点数量（不含自身）。</summary>
            Public Property NeighborCounts As Integer()

            ''' <summary>实际使用的半径。</summary>
            Public Property Radius As Double

            ''' <summary>邻点数量下限。</summary>
            Public Property MinNeighbors As Integer

            ''' <summary>被剔除的点数。</summary>
            Public ReadOnly Property RemovedCount As Integer
                Get
                    If Mask Is Nothing Then Return 0

                    Dim removed As Integer = 0

                    For i As Integer = 0 To Mask.Length - 1
                        If Not Mask(i) Then removed += 1
                    Next

                    Return removed
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"ROR: r={Radius:G4}, minNeighbors={MinNeighbors}, removed={RemovedCount}"
            End Function
        End Class

        ''' <summary>
        ''' 执行半径滤波（只返回点云）。
        ''' </summary>
        Public Shared Function Apply(cloud As PointCloudBuffer, radius As Double, minNeighbors As Integer,
                                     Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            Return Filter(cloud, radius, minNeighbors, options).Cloud
        End Function

        ''' <summary>
        ''' 执行半径滤波（返回诊断结果）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="radius">搜索半径；<c>≤ 0</c> 表示按平均点距自适应估计</param>
        ''' <param name="minNeighbors">半径内至少需要的邻点数量（不含自身）</param>
        ''' <param name="options">并行选项</param>
        ''' <param name="radiusMultiplier">自适应半径时的倍数（默认 3）</param>
        Public Shared Function Filter(cloud As PointCloudBuffer, radius As Double, minNeighbors As Integer,
                                      Optional options As CloudyOptions = Nothing,
                                      Optional radiusMultiplier As Double = 3.0) As RorResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If minNeighbors < 0 Then Throw New ArgumentOutOfRangeException(NameOf(minNeighbors), "the neighbor count can not be negative!")

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            Dim actualRadius As Double = radius

            If actualRadius <= 0.0 Then
                actualRadius = cloud.EstimateAverageSpacing() * std.Max(1.0, radiusMultiplier)
            End If

            If n = 0 OrElse actualRadius <= 0.0 Then
                Dim emptyMask As Boolean() = New Boolean(std.Max(0, n) - 1) {}

                For i As Integer = 0 To emptyMask.Length - 1
                    emptyMask(i) = True
                Next

                Return New RorResult With {
                    .Cloud = cloud.Clone(),
                    .Mask = emptyMask,
                    .NeighborCounts = New Integer(std.Max(0, n) - 1) {},
                    .Radius = actualRadius,
                    .MinNeighbors = minNeighbors
                }
            End If

            Dim hash As New VoxelHash(cloud, actualRadius)
            Dim counts As Integer() = GC.AllocateUninitializedArray(Of Integer)(n)
            Dim scratch As New ThreadLocal(Of List(Of Integer))(Function() New List(Of Integer)(64))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim buffer As List(Of Integer) = scratch.Value

                        For i As Integer = start To ends - 1
                            counts(i) = hash.NeighborsInRadius(cloud.X(i), cloud.Y(i), cloud.Z(i), actualRadius, buffer, i)
                        Next
                    End Sub, 512)
            Finally
                Call scratch.Dispose()
            End Try

            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                mask(i) = counts(i) >= minNeighbors
            Next

            Return New RorResult With {
                .Cloud = cloud.Where(mask),
                .Mask = mask,
                .NeighborCounts = counts,
                .Radius = actualRadius,
                .MinNeighbors = minNeighbors
            }
        End Function

        ''' <summary>
        ''' 多半径投票：在多个半径/邻点阈值下都通过才保留。
        ''' </summary>
        ''' <remarks>
        ''' 单一半径对“既有密集团簇又有稀疏薄壁”的场景很难兼顾；
        ''' 同时要求“小半径内不孤立”与“大半径内不孤立”，
        ''' 相当于对噪声点做了更严格的联合判定，
        ''' 代价是要建两套体素哈希（<c>O(n)</c> 额外内存）。
        ''' </remarks>
        Public Shared Function FilterMultiRadius(cloud As PointCloudBuffer, radii As Double(), minNeighbors As Integer(),
                                                 Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If radii Is Nothing OrElse radii.Length = 0 Then
                Throw New ArgumentException("at least one radius is required!", NameOf(radii))
            End If
            If minNeighbors Is Nothing OrElse minNeighbors.Length <> radii.Length Then
                Throw New ArgumentException("the neighbor thresholds should match the radius count!", NameOf(minNeighbors))
            End If

            Dim mask As Boolean() = Nothing

            For r As Integer = 0 To radii.Length - 1
                Dim result As RorResult = Filter(cloud, radii(r), minNeighbors(r), options)

                If mask Is Nothing Then
                    mask = result.Mask
                Else
                    For i As Integer = 0 To mask.Length - 1
                        mask(i) = mask(i) AndAlso result.Mask(i)
                    Next
                End If
            Next

            Return cloud.Where(mask)
        End Function

    End Class
End Namespace
