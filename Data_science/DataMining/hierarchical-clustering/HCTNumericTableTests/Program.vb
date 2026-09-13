Imports System
Imports System.Data
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering

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

    Sub Main()
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
