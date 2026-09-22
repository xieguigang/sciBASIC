#Region "Microsoft.VisualBasic::192fdac5ff3180c4fca966bc64df56c3, Data_science\DataMining\hierarchical-clustering\HCTNumericTableTests\Program.vb"

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

    '   Total Lines: 222
    '    Code Lines: 150 (67.57%)
    ' Comment Lines: 30 (13.51%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 42 (18.92%)
    '     File Size: 8.15 KB


    ' Module Program
    ' 
    '     Function: SampleData
    ' 
    '     Sub: CheckApproximation, CheckDistanceMatrix, CheckPartition, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Data
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.BIRCH

''' <summary>
''' 层次聚类统一二维表入口（<c>x.distanceMatrix()</c> / <c>x.hca()</c> / <c>x.hcut(...)</c>）的冒烟测试。
''' 
''' 运行方式：
''' ```bash
''' dotnet run --project HCTNumericTableTests/HCTNumericTableTests.vbproj
''' ```
''' </summary>
Module Program

    Private Const N As Integer = 40
    Private Const Half As Integer = 20

    Sub Main(args As String())
        ' dotnet run -- benchmark  -> 只运行性能基准
        If args IsNot Nothing AndAlso args.Any(Function(a) String.Equals(a, "benchmark", StringComparison.OrdinalIgnoreCase)) Then
            Call Benchmark.RunAll()
            Return
        End If

        Dim failures As New List(Of String)
        Dim x = SampleData()

        ' 1. 特征表 -> 距离矩阵表
        Dim dist = x.distanceMatrix()
        Call CheckDistanceMatrix(failures, "distanceMatrix", x, dist)

        ' 2. 距离矩阵契约：直接对特征表调用 hca 应当抛出 InvalidConstraintException
        Try
            Dim tree As Cluster = x.hca()

            Call failures.Add("hca: expected InvalidConstraintException for a non-distance-matrix input")
        Catch ex As InvalidConstraintException
            ' 预期行为：距离矩阵契约被正确执行
        Catch ex As Exception
            Call failures.Add("hca: unexpected exception type " & ex.GetType().Name)
        End Try

        ' 3. 树入口
        Dim root As Cluster = dist.hca()

        If root Is Nothing Then
            Call failures.Add("hca: the clustering tree is Nothing")
        Else
            If root.Leafs <> N Then
                Call failures.Add($"hca: leafs {root.Leafs} <> {N}")
            End If
            If root.OrderLeafs().Length <> N Then
                Call failures.Add($"hca: OrderLeafs size {root.OrderLeafs().Length} <> {N}")
            End If
        End If

        ' 4. 按簇数量切分
        Dim byK = dist.hcut(k:=2)
        Call CheckPartition(failures, "hcut(k:=2)", byK)

        ' 5. 按距离阈值切分（簇内最大距离约 1.4，簇间最小距离约 6，取 3.0 可分开两个簇）
        Dim byT = dist.hcut(threshold:=3.0)
        Call CheckPartition(failures, "hcut(threshold:=3.0)", byT)

        ' 6. BIRCH 近似通道（n 不超过目标子簇数时会退化为恒等预聚类，结果应与精确通道一致）
        Call CheckApproximation(failures, x)

        If failures.Count > 0 Then
            Console.WriteLine("FAILED:")

            For Each msg As String In failures
                Console.WriteLine("  - " & msg)
            Next

            Environment.Exit(1)
        Else
            Console.WriteLine("hierarchical clustering NumericTable checks passed.")
        End If
    End Sub

    ''' <summary>
    ''' 校验 BIRCH 近似通道（特征表入口）：当样本数不超过目标子簇数时预聚类退化为恒等映射，
    ''' 此时近似通道的簇划分应与精确通道保持一致（两个合成簇被正确分开）
    ''' </summary>
    Private Sub CheckApproximation(failures As List(Of String), x As NumericTable)
        Dim options As New BirchOptions With {.targetSubclusters = 1000, .silent = True}

        ' 近似通道的输入是**特征表**（而不是距离矩阵）
        Dim tree = x.hcaApprox(options)

        If tree Is Nothing Then
            Call failures.Add("hcaApprox: the clustering tree is Nothing")
        ElseIf tree.Leafs <> N Then
            Call failures.Add($"hcaApprox: leafs {tree.Leafs} <> {N}")
        End If

        Dim byK = x.hcutApprox(2, options)
        Call CheckPartition(failures, "hcutApprox(k:=2)", byK)

        Dim byT = x.hcutApprox(3.0, options)
        Call CheckPartition(failures, "hcutApprox(threshold:=3.0)", byT)
    End Sub

    ''' <summary>
    ''' 生成两个分离良好的合成簇（N 个样本，2 个特征）
    ''' </summary>
    Private Function SampleData() As NumericTable
        Dim rand As New Random(42)
        Dim rows As Double()() = New Double(N - 1)() {}
        Dim ids As String() = New String(N - 1) {}
        Dim groups As Double() = New Double(N - 1) {}

        For i As Integer = 0 To N - 1
            Dim cx As Double = If(i < Half, 0.0, 8.0)
            Dim cy As Double = If(i < Half, 0.0, 8.0)

            rows(i) = New Double() {
                cx + rand.NextDouble() * 2 - 1,
                cy + rand.NextDouble() * 2 - 1
            }
            ids(i) = "s" & (i + 1)
            groups(i) = If(i < Half, 1.0, 2.0)
        Next

        Dim table As New NumericTable(rows, ids, {"f1", "f2"})

        Call table.SetLabel("group", groups)

        Return table
    End Function

    ''' <summary>
    ''' 校验距离矩阵表：方阵、对角线为 0、对称、继承标签、行列名均为样本名
    ''' </summary>
    Private Sub CheckDistanceMatrix(failures As List(Of String),
                                    name As String,
                                    source As NumericTable,
                                    dist As NumericTable)

        If dist Is Nothing Then
            Call failures.Add($"{name}: the distance matrix is Nothing")
            Return
        End If

        If dist.nsamples <> source.nsamples Then
            Call failures.Add($"{name}: nsamples {dist.nsamples} <> {source.nsamples}")
        End If

        If dist.nfeatures <> source.nsamples Then
            Call failures.Add($"{name}: the result is not a square matrix ({dist.nsamples} x {dist.nfeatures})")
            Return
        End If

        For i As Integer = 0 To N - 1
            If dist(i)(i) <> 0.0 Then
                Call failures.Add($"{name}: the diagonal element[{i}] is {dist(i)(i)} <> 0")
                Exit For
            End If
        Next

        For i As Integer = 0 To N - 1
            For j As Integer = i + 1 To N - 1
                If dist(i)(j) <> dist(j)(i) Then
                    Call failures.Add($"{name}: the matrix is not symmetric at ({i},{j})")
                    Return
                End If
            Next
        Next

        Dim inherited As Double() = Nothing

        If Not dist.TryGetLabel("group", inherited) Then
            Call failures.Add($"{name}: the label column 'group' is not inherited")
        End If
    End Sub

    ''' <summary>
    ''' 校验切分结果：cluster 标签长度正确，且两个簇被正确的分开
    ''' </summary>
    Private Sub CheckPartition(failures As List(Of String), name As String, table As NumericTable)
        Dim labels As Double() = Nothing

        If table Is Nothing Then
            Call failures.Add($"{name}: the result table is Nothing")
            Return
        End If

        If Not table.TryGetLabel("cluster", labels) Then
            Call failures.Add($"{name}: the 'cluster' label column is missing")
            Return
        End If

        If labels.Length <> N Then
            Call failures.Add($"{name}: cluster size {labels.Length} <> {N}")
            Return
        End If

        Dim first As Integer = CInt(labels(0))
        Dim second As Integer = CInt(labels(Half))

        If first = second Then
            Call failures.Add($"{name}: the two clusters are not separated")
            Return
        End If

        For i As Integer = 0 To Half - 1
            If CInt(labels(i)) <> first Then
                Call failures.Add($"{name}: unexpected partition at sample {i}")
                Return
            End If
        Next

        For i As Integer = Half To N - 1
            If CInt(labels(i)) <> second Then
                Call failures.Add($"{name}: unexpected partition at sample {i}")
                Return
            End If
        Next
    End Sub
End Module

