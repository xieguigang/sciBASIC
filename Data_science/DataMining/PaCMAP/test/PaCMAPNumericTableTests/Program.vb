Imports System
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.PaCMAP

''' <summary>
''' PaCMAP 统一二维表入口 <c>x.pacmap(...)</c> 的冒烟测试。
''' 
''' 运行方式：
''' ```bash
''' dotnet run --project test/PaCMAPNumericTableTests/PaCMAPNumericTableTests.vbproj
''' ```
''' </summary>
Module Program

    Sub Main()
        Dim failures As New List(Of String)

        ' PaCMAP 的远距离对采样要求样本数量明显大于近邻数量，
        ' 因此这里使用 N = 64 的合成数据，并把迭代次数调小以缩短测试时间。
        Dim source = SampleData(n:=64, seed:=1234)
        Dim result = source.pacmap(dims:=2, neighbors:=10, iterations:=20)

        Call CheckEmbedding(failures, "pacmap", source, result, dims:=2)

        If failures.Count > 0 Then
            Console.WriteLine("FAILED:")

            For Each msg As String In failures
                Console.WriteLine("  - " & msg)
            Next

            Environment.Exit(1)
        Else
            Console.WriteLine("PaCMAP NumericTable checks passed.")
        End If
    End Sub

    ''' <summary>
    ''' 生成两个分离良好的合成簇（N 个样本，3 个特征）
    ''' </summary>
    Private Function SampleData(n As Integer, seed As Integer) As NumericTable
        Dim rand As New Random(seed)
        Dim rows As Double()() = New Double(n - 1)() {}
        Dim ids As String() = New String(n - 1) {}
        Dim groups As Double() = New Double(n - 1) {}
        Dim half As Integer = n \ 2

        For i As Integer = 0 To n - 1
            Dim cx As Double = If(i < half, 0.0, 8.0)
            Dim cy As Double = If(i < half, 0.0, 8.0)

            rows(i) = New Double() {
                cx + rand.NextDouble() * 2 - 1,
                cy + rand.NextDouble() * 2 - 1,
                rand.NextDouble() * 0.5
            }
            ids(i) = "s" & (i + 1)
            groups(i) = If(i < half, 1.0, 2.0)
        Next

        Dim table As New NumericTable(rows, ids, {"f1", "f2", "f3"})

        Call table.SetLabel("group", groups)

        Return table
    End Function

    ''' <summary>
    ''' 校验嵌入结果表的结构：样本数/维度/列名/行名/标签继承
    ''' </summary>
    Private Sub CheckEmbedding(failures As List(Of String),
                               name As String,
                               source As NumericTable,
                               result As NumericTable,
                               dims As Integer)

        If result Is Nothing Then
            Call failures.Add($"{name}: the embedding result is Nothing")
            Return
        End If

        If result.nsamples <> source.nsamples Then
            Call failures.Add($"{name}: nsamples {result.nsamples} <> {source.nsamples}")
        End If

        If result.nfeatures <> dims Then
            Call failures.Add($"{name}: nfeatures {result.nfeatures} <> {dims}")
        End If

        For i As Integer = 1 To dims
            Dim expected As String = $"dim_{i}"

            If result.featureNames(i - 1) <> expected Then
                Call failures.Add($"{name}: featureNames({i - 1}) = {result.featureNames(i - 1)} <> {expected}")
                Exit For
            End If
        Next

        If String.Join(",", result.RowNamesOrDefault()) <> String.Join(",", source.RowNamesOrDefault()) Then
            Call failures.Add($"{name}: the row names are not inherited from the source table")
        End If

        Dim inherited As Double() = Nothing

        If Not result.TryGetLabel("group", inherited) Then
            Call failures.Add($"{name}: the label column 'group' is not inherited")
        ElseIf String.Join(",", inherited) <> String.Join(",", source.GetLabel("group")) Then
            Call failures.Add($"{name}: the label values are not inherited")
        End If
    End Sub
End Module
