Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 关键点集合：原始点云下标 + 检测得分 + 可选尺度。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 关键点检测的输出<b>不是</b>新的点云，而是“原云中的一组下标”。
    ''' 这点很重要：后续配准需要把关键点与其在原云中的邻域对应起来
    ''' （描述子计算、几何验证），若中间先生成一个裁剪后的点云，
    ''' 下标映射关系就需要额外维护，出错概率很高。
    ''' </para>
    ''' <para>
    ''' 约定：<see cref="Scores"/> 越大表示“越显著”（ISS 用 λ3、Harris 用响应值、
    ''' SIFT-3D 用尺度归一化响应）。<see cref="Scales"/> 可选，表示该关键点的特征尺度。
    ''' </para>
    ''' </remarks>
    Public Class KeypointSet

        ''' <summary>关键点在原始点云中的下标。</summary>
        Public Property Indices As Integer()

        ''' <summary>与下标一一对应的显著性得分。</summary>
        Public Property Scores As Double()

        ''' <summary>可选的特征尺度（SIFT-3D 使用）。</summary>
        Public Property Scales As Double()

        Public Sub New(indices As Integer(), scores As Double(), Optional scales As Double() = Nothing)
            Me.Indices = indices
            Me.Scores = scores
            Me.Scales = scales
        End Sub

        ''' <summary>关键点数量。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return If(Indices Is Nothing, 0, Indices.Length)
            End Get
        End Property

        ''' <summary>
        ''' 按得分降序排列（确定性：得分相同按下标升序）。
        ''' </summary>
        Public Sub SortByScore()
            If Count < 2 Then Return

            Array.Sort(Indices, Scores)

            Array.Reverse(Indices)
            Array.Reverse(Scores)

            If Scales IsNot Nothing Then Array.Reverse(Scales)
        End Sub

        ''' <summary>
        ''' 取出关键点对应的点云子集（保持得分顺序）。
        ''' </summary>
        Public Function ToPointCloud(cloud As PointCloudBuffer) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Return cloud.Subset(Indices)
        End Function

        ''' <summary>
        ''' 按下标子集抽取关键点（用于描述子只覆盖部分关键点的场景）。
        ''' </summary>
        Public Function Subset(selection As Integer()) As KeypointSet
            If selection Is Nothing Then Throw New ArgumentNullException(NameOf(selection))

            Dim idx(selection.Length - 1) As Integer
            Dim sc(selection.Length - 1) As Double

            For i As Integer = 0 To selection.Length - 1
                idx(i) = Indices(selection(i))
                sc(i) = Scores(selection(i))
            Next

            Return New KeypointSet(idx, sc)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Count} keypoints"
        End Function
    End Class

    ''' <summary>
    ''' 特征云：关键点 + 逐关键点描述子矩阵。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 描述子以“行 = 关键点、列 = 维度”的锯齿数组存放
    ''' （<c>Descriptors(i)</c> 是一条连续数组）。选择这种布局是为了让
    ''' <see cref="SimdReduce.DistanceSquared(Double(), Double())"/> 能直接作用于两行，
    ''' 而不需要在匹配时做跨步访问或转置。
    ''' </para>
    ''' <para>
    ''' 常见维度：FPFH = 33、SHOT = 352、ESF = 640、VFH = 768。
    ''' </para>
    ''' </remarks>
    Public Class FeatureCloud

        ''' <summary>关键点（含原云下标）。</summary>
        Public Property Keypoints As KeypointSet

        ''' <summary>描述子矩阵（每行一个关键点）。</summary>
        Public Property Descriptors As Double()()

        ''' <summary>描述子维度（所有行必须一致）。</summary>
        Public Property Dimension As Integer

        Public Sub New(keypoints As KeypointSet, descriptors As Double()())
            Me.Keypoints = keypoints
            Me.Descriptors = descriptors

            If descriptors IsNot Nothing AndAlso descriptors.Length > 0 AndAlso descriptors(0) IsNot Nothing Then
                Me.Dimension = descriptors(0).Length
            End If
        End Sub

        ''' <summary>特征数量。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return If(Descriptors Is Nothing, 0, Descriptors.Length)
            End Get
        End Property

        ''' <summary>取第 <paramref name="index"/> 行的描述子。</summary>
        Public Function Descriptor(index As Integer) As Double()
            Return Descriptors(index)
        End Function

        ''' <summary>
        ''' 按关键点子集抽取特征（描述子与关键点同步裁剪）。
        ''' </summary>
        Public Function Subset(selection As Integer()) As FeatureCloud
            If selection Is Nothing Then Throw New ArgumentNullException(NameOf(selection))

            Dim rows As Double()() = New Double(selection.Length - 1)() {}

            For i As Integer = 0 To selection.Length - 1
                rows(i) = Descriptors(selection(i))
            Next

            Return New FeatureCloud(Keypoints.Subset(selection), rows)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Count} descriptors x {Dimension} dimensions"
        End Function
    End Class

    ''' <summary>
    ''' 关键点非极大抑制：在半径为 <c>r</c> 的邻域内只保留得分最高的关键点。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 所有关键点检测器都会产生“成簇”的候选（一个角点周围十来个点响应都很高）。
    ''' 如果不做抑制，后续描述子匹配会被同一个物理特征的大量近邻对应淹没，
    ''' RANSAC 的采样效率会急剧下降。
    ''' </para>
    ''' <para>
    ''' 这里是<b>贪心</b>抑制：按得分降序依次保留，若与已保留点的距离小于 <c>r</c> 则丢弃。
    ''' 复杂度 <c>O(m·k·log m)</c>（<c>m</c> 为候选数）。
    ''' 与 PCL 的 <c>NonMaxSuppression</c> 语义一致（半径内比较、距离小于半径则淘汰）。
    ''' </para>
    ''' </remarks>
    Friend NotInheritable Class KeypointSuppression

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 执行贪心非极大抑制，返回保留下来的候选下标（指向输入数组）。
        ''' </summary>
        ''' <param name="cloud">原始点云（用于查询空间位置）</param>
        ''' <param name="candidates">候选点在原云中的下标</param>
        ''' <param name="scores">候选得分（越大越显著）</param>
        ''' <param name="radius">抑制半径</param>
        Public Shared Function Suppress(cloud As PointCloudBuffer, candidates As Integer(), scores As Double(),
                                        radius As Double) As Integer()
            If candidates Is Nothing Then Throw New ArgumentNullException(NameOf(candidates))
            If candidates.Length = 0 Then Return Array.Empty(Of Integer)()
            If radius <= 0.0 Then
                Dim all(candidates.Length - 1) As Integer

                For i As Integer = 0 To candidates.Length - 1
                    all(i) = i
                Next

                Return all
            End If

            ' 按得分降序的候选顺序（得分相同按原序，保证确定性）
            Dim order(candidates.Length - 1) As Integer

            For i As Integer = 0 To order.Length - 1
                order(i) = i
            Next

            Array.Sort(order,
                Function(a, b)
                    Dim cmp As Integer = scores(b).CompareTo(scores(a))

                    If cmp <> 0 Then Return cmp

                    Return candidates(a).CompareTo(candidates(b))
                End Function)

            Dim keptPoints As New List(Of Integer)(candidates.Length)
            Dim keptIndex As New List(Of Integer)(candidates.Length)
            Dim r2 As Double = radius * radius

            For Each o As Integer In order
                Dim p As Integer = candidates(o)
                Dim px As Double = cloud.X(p)
                Dim py As Double = cloud.Y(p)
                Dim pz As Double = cloud.Z(p)
                Dim suppressed As Boolean = False

                For j As Integer = 0 To keptPoints.Count - 1
                    Dim q As Integer = keptPoints(j)
                    Dim dx As Double = px - cloud.X(q)
                    Dim dy As Double = py - cloud.Y(q)
                    Dim dz As Double = pz - cloud.Z(q)

                    If dx * dx + dy * dy + dz * dz < r2 Then
                        suppressed = True

                        Exit For
                    End If
                Next

                If Not suppressed Then
                    Call keptPoints.Add(p)
                    Call keptIndex.Add(o)
                End If
            Next

            Return keptIndex.ToArray()
        End Function

    End Class
End Namespace
