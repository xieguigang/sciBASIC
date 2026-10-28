
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

''' <summary>
''' Ward.D2 最小方差层次聚类（等价 R 的 <c>hclust(method = "ward.D2")</c>）。
'''
''' <para>
''' 每一轮挑选使 Ward 准则增量 <c>Δ = n_i·n_j/(n_i+n_j) · ||c_i − c_j||²</c> 最小的一对簇合并，
''' 合并高度取 <c>sqrt(2·Δ)</c>（即 R 的 ward.D2 高度标度：两个单元素的合并高度等于它们之间的欧氏距离）。
''' </para>
'''
''' <para>
''' 既有 <c>LinkageStrategy</c> 接口只传递两个距离值、拿不到簇大小与质心，无法表达 Ward 准则，
''' 因此这里直接基于质心做 O(n³) 的朴素实现（n = 300 时约 10⁷ 次简单运算，可以接受；
''' 更大的矩阵建议改用平均连接或者 BIRCH 近似通道）。
''' </para>
''' </summary>
Public Module WardClustering

    ''' <summary>
    ''' 对观测矩阵执行 Ward.D2 凝聚聚类。
    ''' </summary>
    ''' <param name="data">观测矩阵（每一行是一个观测）</param>
    ''' <param name="names">与观测一一对应的唯一名称</param>
    ''' <param name="metric">
    ''' 距离度量（缺省欧氏距离）。Ward 准则建立在方差解释之上，使用非欧氏度量会改变其含义，请谨慎使用。
    ''' </param>
    Public Function Agglomerate(data As Double()(), names As String(),
                                Optional metric As Func(Of Double(), Double(), Double) = Nothing) As Cluster

        Dim n As Integer = data.Length

        If n = 0 Then
            Return Nothing
        End If

        Dim f As Func(Of Double(), Double(), Double) = If(metric, AddressOf DistanceMethods.EuclideanDistance)
        Dim nodes(n - 1) As Cluster
        Dim centroid(n - 1)() As Double
        Dim sizes(n - 1) As Integer
        Dim active As New List(Of Integer)

        For i As Integer = 0 To n - 1
            nodes(i) = New Cluster(names(i))
            centroid(i) = DirectCast(data(i).Clone(), Double())
            sizes(i) = 1
            Call active.Add(i)
        Next

        Dim serial As Integer = 0

        Do While active.Count > 1
            Dim bi As Integer = -1
            Dim bj As Integer = -1
            Dim best As Double = Double.MaxValue
            Dim bestHeight As Double = 0

            For a As Integer = 0 To active.Count - 1
                Dim i As Integer = active(a)

                For b As Integer = a + 1 To active.Count - 1
                    Dim j As Integer = active(b)
                    Dim d As Double = f(centroid(i), centroid(j))

                    If Double.IsNaN(d) Then
                        Continue For
                    End If

                    Dim delta As Double = sizes(i) * CDbl(sizes(j)) / (sizes(i) + sizes(j)) * d * d

                    If delta < best Then
                        best = delta
                        bestHeight = std.Sqrt(2 * delta)
                        bi = i
                        bj = j
                    End If
                Next
            Next

            If bi < 0 Then
                ' 所有距离都是 NaN：退化为按顺序合并，保证不会死循环
                bi = active(0)
                bj = active(1)
                bestHeight = 0
            End If

            serial += 1

            Dim parent As New Cluster("clstr#" & serial) With {
                .Distance = New Hierarchy.Distance(bestHeight)
            }

            parent.AddChild(nodes(bi))
            parent.AddChild(nodes(bj))
            nodes(bi).Parent = parent
            nodes(bj).Parent = parent

            Dim nLeft As Integer = sizes(bi)
            Dim nRight As Integer = sizes(bj)
            Dim total As Integer = nLeft + nRight
            Dim merged(centroid(bi).Length - 1) As Double

            For k As Integer = 0 To merged.Length - 1
                merged(k) = (centroid(bi)(k) * nLeft + centroid(bj)(k) * nRight) / total
            Next

            nodes(bi) = parent
            centroid(bi) = merged
            sizes(bi) = total
            Call active.Remove(bj)
        Loop

        Return nodes(active(0))
    End Function
End Module
