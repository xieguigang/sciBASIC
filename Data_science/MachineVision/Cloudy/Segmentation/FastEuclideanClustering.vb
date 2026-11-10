Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' FEC(Fast Euclidean Clustering)：在体素图上做连通分量标记，逐点赋值。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么不直接做点级 BFS</b>：传统欧氏聚类是“从每个未访问点出发，反复查询半径邻域”，
    ''' 每个点都要做一次邻域查询，且簇内的点会被重复访问。
    ''' </para>
    ''' <para>
    ''' <b>体素近似的关键观察</b>：当体素边长 <c>c = tolerance</c> 时，
    ''' 两个距离 ≤ <c>tolerance</c> 的点必然落在<b>同一体素或 26 邻域体素</b>内
    ''' （因为 <c>|Δx| ≤ c</c> 时 <c>floor(x/c)</c> 最多相差 1）。
    ''' 因此“体素图上的连通分量”与“点图上的连通分量”<b>完全等价</b>，
    ''' 而体素数量通常比点数少一个数量级（表面扫描的体素占用率约 10~30%）。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n + m)</c>（<c>m</c> 为占用体素数），
    ''' 比点级 BFS 的 <c>O(n·k)</c> 快一个数量级，且完全确定（按排序后的体素坐标遍历）。
    ''' </para>
    ''' <para>
    ''' <b>限制</b>：体素近似要求 <c>c = tolerance</c>；如果为了让体素更粗而取
    ''' <c>c &gt; tolerance</c>，连通判据会变松（可能把距离略大于 tolerance 的点连起来）。
    ''' 本实现固定 <c>c = tolerance</c>，不提供这个“加速但失真”的选项。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class FastEuclideanClustering

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 快速欧氏聚类。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="tolerance">连通距离阈值；<c>≤ 0</c> 表示按平均点距自适应（3 倍）</param>
        ''' <param name="minClusterSize">最小簇点数</param>
        ''' <param name="maxClusterSize">最大簇点数（<c>0</c> 表示不限制）</param>
        Public Shared Function Cluster(cloud As PointCloudBuffer, Optional tolerance As Double = 0.0,
                                       Optional minClusterSize As Integer = 10,
                                       Optional maxClusterSize As Integer = 0) As ClusterSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then Return New ClusterSet(Array.Empty(Of Integer)(), tolerance)

            Dim t As Double = If(tolerance > 0.0, tolerance, std.Max(cloud.EstimateAverageSpacing() * 3.0, 1.0E-9))
            Dim inverse As Double = 1.0 / t

            ' 1) 点 → 体素
            Dim cells As New Dictionary(Of (x As Integer, y As Integer, z As Integer), List(Of Integer))(std.Max(16, n \ 8))
            Dim pointCell(n - 1) As (x As Integer, y As Integer, z As Integer)

            For i As Integer = 0 To n - 1
                Dim cell As (x As Integer, y As Integer, z As Integer) =
                    (CInt(std.Floor(cloud.X(i) * inverse)),
                     CInt(std.Floor(cloud.Y(i) * inverse)),
                     CInt(std.Floor(cloud.Z(i) * inverse)))

                pointCell(i) = cell

                Dim members As List(Of Integer) = Nothing

                If Not cells.TryGetValue(cell, members) Then
                    members = New List(Of Integer)(8)
                    cells(cell) = members
                End If

                Call members.Add(i)
            Next

            ' 2) 体素图上的连通分量（按坐标排序保证确定性）
            Dim ordered As New List(Of (cell As (x As Integer, y As Integer, z As Integer), members As List(Of Integer)))(cells.Count)

            For Each entry As KeyValuePair(Of (x As Integer, y As Integer, z As Integer), List(Of Integer)) In cells
                Call ordered.Add((entry.Key, entry.Value))
            Next

            Call ordered.Sort(Function(a, b) CompareCell(a.cell, b.cell))

            Dim offsets As Integer()() = NeighborOffsets()
            Dim cellLabel As New Dictionary(Of (x As Integer, y As Integer, z As Integer), Integer)(cells.Count)
            Dim queue As New Queue(Of (x As Integer, y As Integer, z As Integer))()
            Dim label As Integer = -1

            For Each entry In ordered
                If cellLabel.ContainsKey(entry.cell) Then Continue For

                label += 1
                cellLabel(entry.cell) = label
                Call queue.Enqueue(entry.cell)

                While queue.Count > 0
                    Dim current As (x As Integer, y As Integer, z As Integer) = queue.Dequeue()

                    For Each offset As Integer() In offsets
                        Dim neighbor As (x As Integer, y As Integer, z As Integer) =
                            (current.x + offset(0), current.y + offset(1), current.z + offset(2))

                        If Not cells.ContainsKey(neighbor) Then Continue For
                        If cellLabel.ContainsKey(neighbor) Then Continue For

                        cellLabel(neighbor) = label
                        Call queue.Enqueue(neighbor)
                    Next
                End While
            Next

            ' 3) 点级标签 + 尺寸过滤
            Dim raw(n - 1) As Integer
            Dim sizes As Integer() = New Integer(std.Max(0, label) ) {}

            For i As Integer = 0 To n - 1
                raw(i) = cellLabel(pointCell(i))
            Next

            For i As Integer = 0 To n - 1
                sizes(raw(i)) += 1
            Next

            Dim remap(cellLabel.Count - 1) As Integer

            For i As Integer = 0 To remap.Length - 1
                remap(i) = -1
            Next

            Dim nextLabel As Integer = 0

            For c As Integer = 0 To sizes.Length - 1
                Dim size As Integer = sizes(c)

                If size >= minClusterSize AndAlso (maxClusterSize <= 0 OrElse size <= maxClusterSize) Then
                    remap(c) = nextLabel
                    nextLabel += 1
                End If
            Next

            Dim labels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim c As Integer = raw(i)

                labels(i) = If(c >= 0 AndAlso c < remap.Length, remap(c), -1)
            Next

            Return New ClusterSet(labels, t)
        End Function

        ''' <summary>
        ''' 26 邻域体素偏移（不含自身）。
        ''' </summary>
        ''' <remarks>
        ''' 刻意返回 <c>Integer()()</c> 而不是命名元组数组：VB 对元组字段的
        ''' 成员解析在“元组数组的 For Each 变量”上容易与框架类型成员混淆，
        ''' 用最朴素的整数数组可以完全避免歧义。
        ''' </remarks>
        Private Shared Function NeighborOffsets() As Integer()()
            Dim list As New List(Of Integer())(26)

            For dx As Integer = -1 To 1
                For dy As Integer = -1 To 1
                    For dz As Integer = -1 To 1
                        If dx = 0 AndAlso dy = 0 AndAlso dz = 0 Then Continue For

                        Call list.Add({dx, dy, dz})
                    Next
                Next
            Next

            Return list.ToArray()
        End Function

        Private Shared Function CompareCell(a As (x As Integer, y As Integer, z As Integer),
                                            b As (x As Integer, y As Integer, z As Integer)) As Integer
            Dim cmp As Integer = a.x.CompareTo(b.x)

            If cmp <> 0 Then Return cmp

            cmp = a.y.CompareTo(b.y)

            If cmp <> 0 Then Return cmp

            Return a.z.CompareTo(b.z)
        End Function

    End Class
End Namespace
