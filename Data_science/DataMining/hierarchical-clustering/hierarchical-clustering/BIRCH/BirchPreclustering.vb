#Region "Microsoft.VisualBasic::0d18235bc7d6dfd9301e22b5b84f9bff, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\BIRCH\BirchPreclustering.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 277
    '    Code Lines: 137 (49.46%)
    ' Comment Lines: 83 (29.96%)
    '    - Xml Docs: 87.95%
    ' 
    '   Blank Lines: 57 (20.58%)
    '     File Size: 11.46 KB


    '     Class BirchOptions
    ' 
    '         Properties: automaticRebuild, distFunction, linkage, maxNodeEntries, silent
    '                     targetSubclusters, threshold, thresholdRefineIterations
    ' 
    '     Class BirchPreclustering
    ' 
    '         Properties: Centroids, Members, Names, Threshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: build, centroid, fromTree, identity, Precluster
    '                   seedThreshold
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Namespace BIRCH

    ''' <summary>
    ''' BIRCH 预聚类（CF-tree）参数。
    ''' 
    ''' <para>
    ''' BIRCH（Balanced Iterative Reducing and Clustering using Hierarchies）先用 CF-tree 把
    ''' n 个样本压缩成 m 个子簇（m &lt;&lt; n），再对子簇质心执行（优化后的）凝聚层次聚类。
    ''' 这样可以完全避免构造 n×n 距离矩阵，是面向 2 万样本以上数据集的近似加速通道。
    ''' </para>
    ''' </summary>
    Public Class BirchOptions

        ''' <summary>
        ''' 每个 CF 节点最多容纳的条目数（BIRCH 参数 B，branching factor）
        ''' </summary>
        Public Property maxNodeEntries As Integer = 50

        ''' <summary>
        ''' 距离阈值（BIRCH 参数 T，radius）。当 &lt;= 0 时由 <see cref="targetSubclusters"/> 自动推导。
        ''' </summary>
        Public Property threshold As Double = 0

        ''' <summary>
        ''' 距离函数，取值 <see cref="CFTree.D0_DIST"/>、<see cref="CFTree.D1_DIST"/>、
        ''' <see cref="CFTree.D2_DIST"/>、<see cref="CFTree.D3_DIST"/> 或 <see cref="CFTree.D4_DIST"/>
        ''' </summary>
        Public Property distFunction As Integer = CFTree.D0_DIST

        ''' <summary>
        ''' 期望压缩到的子簇数量上限（m）。当 <see cref="threshold"/> 未显式指定时，以该值为目标
        ''' 自动推导阈值（指数增长 + 二分细化）。
        ''' </summary>
        Public Property targetSubclusters As Integer = 2000

        ''' <summary>
        ''' 第二级凝聚层次聚类的连接策略，缺省为平均连接 <see cref="AverageLinkageStrategy"/>
        ''' </summary>
        Public Property linkage As LinkageStrategy

        ''' <summary>
        ''' 是否静默运行（缺省 True）
        ''' </summary>
        Public Property silent As Boolean = True

        ''' <summary>
        ''' 是否启用 CF-tree 的内存超限自动重建（缺省 False）。
        ''' 
        ''' 自动重建会周期性使用反射估算整棵树的内存占用，开销较大，
        ''' 因此在大数据近似通道中默认关闭；如需严格控制内存可将其置为 True。
        ''' </summary>
        Public Property automaticRebuild As Boolean = False

        ''' <summary>
        ''' 阈值自动推导时二分细化的迭代次数（缺省 6）
        ''' </summary>
        Public Property thresholdRefineIterations As Integer = 6
    End Class

    ''' <summary>
    ''' BIRCH 预聚类的结果：m 个子簇的名称、质心，以及「子簇 -&gt; 成员样本行索引」的映射。
    ''' </summary>
    Public Class BirchPreclustering

        ''' <summary>
        ''' 子簇名称（<c>sc_1, sc_2, ...</c>），顺序与 <see cref="Centroids"/> 一致，
        ''' 并作为第二级凝聚层次聚类的叶节点名称
        ''' </summary>
        Public ReadOnly Property Names As String()

        ''' <summary>
        ''' 子簇质心（m x d）
        ''' </summary>
        Public ReadOnly Property Centroids As Double()()

        ''' <summary>
        ''' 子簇名称 -&gt; 该子簇包含的原始样本（特征表）行索引
        ''' </summary>
        Public ReadOnly Property Members As Dictionary(Of String, Integer())

        ''' <summary>
        ''' 实际使用的 CF-tree 距离阈值
        ''' </summary>
        Public ReadOnly Property Threshold As Double

        Friend Sub New(names As String(), centroids As Double()(), members As Dictionary(Of String, Integer()), threshold As Double)
            Me.Names = names
            Me.Centroids = centroids
            Me.Members = members
            Me.Threshold = threshold
        End Sub

        ''' <summary>
        ''' 对特征表执行 BIRCH 预聚类，返回至多 <see cref="BirchOptions.targetSubclusters"/> 个子簇。
        ''' </summary>
        ''' <param name="rows">特征矩阵（n x d），每一行为一个样本</param>
        ''' <param name="options">预聚类参数，缺省使用 <see cref="BirchOptions"/> 的默认值</param>
        ''' <returns>预聚类结果</returns>
        Public Shared Function Precluster(rows As Double()(), Optional options As BirchOptions = Nothing) As BirchPreclustering
            If rows Is Nothing OrElse rows.Length = 0 Then
                Throw New ArgumentException("the feature rows are empty for BIRCH pre-clustering!", NameOf(rows))
            End If

            Dim opt As BirchOptions = If(options, New BirchOptions())
            Dim n As Integer = rows.Length
            Dim target As Integer = std.Max(1, std.Min(opt.targetSubclusters, n))

            ' 样本数本身不超过目标子簇数：无需压缩，直接返回恒等预聚类
            If n <= target Then
                Return identity(rows)
            End If

            Dim maxEntries As Integer = std.Max(2, opt.maxNodeEntries)
            Dim distFunction As Integer = opt.distFunction
            Dim threshold As Double = opt.threshold

            If threshold <= 0 Then
                threshold = seedThreshold(rows) * 0.5
            End If

            ' 先在较小的阈值下构建基准 CF-tree（插入全部 n 个样本）
            Dim baseTree As CFTree = build(rows, maxEntries, threshold, distFunction, opt.automaticRebuild)
            Dim bestTree As CFTree = baseTree
            Dim lo As Double = 0
            Dim rounds As Integer = 0

            ' 指数增长阈值，直到子簇数量不再超过目标（或达到安全上限）。
            ' 
            ' 这里复用 BIRCH 自身的 rebuildTree（由 leaf entries 重新插入，而不是重新插入全部原始样本），
            ' 并保留 baseTree（discardOldTree=False），以便后续二分细化时可以反复以它为起点。
            While bestTree.countLeafEntries() > target AndAlso rounds < 32
                lo = threshold
                threshold *= 2
                bestTree = baseTree.rebuildTree(maxEntries, threshold, distFunction, False, False)
                rounds += 1
            End While

            Dim finalThreshold As Double = threshold

            ' 在 (lo, threshold] 之间二分细化，使子簇数量尽量贴近目标
            If lo > 0 AndAlso bestTree.countLeafEntries() > 0 AndAlso opt.thresholdRefineIterations > 0 Then
                Dim hi As Double = threshold

                For i As Integer = 1 To opt.thresholdRefineIterations
                    Dim mid As Double = (lo + hi) / 2
                    Dim trial As CFTree = baseTree.rebuildTree(maxEntries, mid, distFunction, False, False)

                    If trial.countLeafEntries() > target Then
                        lo = mid
                    Else
                        hi = mid
                        bestTree = trial
                    End If
                Next

                finalThreshold = hi
            End If

            Return fromTree(bestTree, rows, finalThreshold)
        End Function

#Region "Helpers"

        ''' <summary>
        ''' 以给定的阈值构建 CF-tree 并把所有样本插入其中
        ''' </summary>
        Private Shared Function build(rows As Double()(), maxEntries As Integer, threshold As Double,
                                      distFunction As Integer, automaticRebuild As Boolean) As CFTree
            Dim tree As New CFTree(maxEntries, threshold, distFunction, False)

            ' 关闭周期性/根分裂时的内存估算与自动重建，避免预聚类阶段自身成为新的性能瓶颈
            Call tree.AutomaticRebuild(automaticRebuild)

            For i As Integer = 0 To rows.Length - 1
                Call tree.insertEntry(rows(i), i)
            Next

            Call tree.finishedInsertingData()

            Return tree
        End Function

        ''' <summary>
        ''' 由 CF-tree 的叶条目（子簇）构造预聚类结果
        ''' </summary>
        Private Shared Function fromTree(tree As CFTree, rows As Double()(), threshold As Double) As BirchPreclustering
            Dim members As List(Of List(Of Integer)) = tree.SubclusterMembers
            Dim names As String() = New String(std.Max(members.Count - 1, -1)) {}
            Dim centroids As Double()() = New Double(std.Max(members.Count - 1, -1))() {}
            Dim map As New Dictionary(Of String, Integer())

            For i As Integer = 0 To members.Count - 1
                Dim name As String = "sc_" & (i + 1)

                names(i) = name
                centroids(i) = centroid(rows, members(i))
                map.Add(name, members(i).ToArray)
            Next

            Return New BirchPreclustering(names, centroids, map, threshold)
        End Function

        ''' <summary>
        ''' 恒等预聚类：每个样本自成一个子簇
        ''' </summary>
        Private Shared Function identity(rows As Double()()) As BirchPreclustering
            Dim names As String() = New String(rows.Length - 1) {}
            Dim centroids As Double()() = New Double(rows.Length - 1)() {}
            Dim map As New Dictionary(Of String, Integer())

            For i As Integer = 0 To rows.Length - 1
                Dim name As String = "sc_" & (i + 1)

                names(i) = name
                centroids(i) = CType(rows(i).Clone(), Double())
                map.Add(name, {i})
            Next

            Return New BirchPreclustering(names, centroids, map, 0)
        End Function

        ''' <summary>
        ''' 计算一个子簇的质心
        ''' </summary>
        Private Shared Function centroid(rows As Double()(), members As List(Of Integer)) As Double()
            Dim dims As Integer = rows(0).Length
            Dim sums As Double() = New Double(dims - 1) {}

            For Each idx As Integer In members
                Dim r As Double() = rows(idx)

                For j As Integer = 0 To dims - 1
                    sums(j) += r(j)
                Next
            Next

            Dim n As Integer = members.Count

            For j As Integer = 0 To dims - 1
                sums(j) /= n
            Next

            Return sums
        End Function

        ''' <summary>
        ''' 由随机采样的成对距离估计一个初始阈值（半径）种子
        ''' </summary>
        Private Shared Function seedThreshold(rows As Double()()) As Double
            Dim n As Integer = rows.Length
            Dim pairs As Integer = std.Min(1024, std.Max(n * 2, 2))
            Dim rnd As New Random(2718)
            Dim sum As Double = 0

            For i As Integer = 1 To pairs
                Dim a As Integer = rnd.Next(n)
                Dim b As Integer = rnd.Next(n)

                sum += DistanceMethods.EuclideanDistance(rows(a), rows(b))
            Next

            Dim seed As Double = (sum / pairs) / 4.0

            If seed <= 0 OrElse Double.IsNaN(seed) OrElse Double.IsInfinity(seed) Then
                seed = 1.0
            End If

            Return seed
        End Function

#End Region

    End Class
End Namespace

