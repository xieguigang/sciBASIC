' ============================================================
' corstore_test.vb — CorrelationMatrixWriter / Store round-trip 冒烟测试
' 运行: dotnet run --project test.vbproj -p:StartupObject=test.corstore_test
' ============================================================

Imports Microsoft.VisualBasic.Math.Matrix

Module corstore_test

    Const N As Integer = 500
    Const SampleN As Integer = 1888

    Dim failures As Integer = 0

    Sub Main()
        Dim tmpDir = IO.Path.Combine(IO.Path.GetTempPath, "corstore_test")

        If IO.Directory.Exists(tmpDir) Then Call IO.Directory.Delete(tmpDir, True)
        Call IO.Directory.CreateDirectory(tmpDir)

        Dim geneIds = Enumerable.Range(0, N).Select(Function(i) $"GENE{i:0000}").ToArray
        Dim truth = MakeRandomMatrix(N)

        Call RunRoundTrip(IO.Path.Combine(tmpDir, "f32.corstore"), geneIds, truth, CorrelationEncodings.Float32, 0.000001)
        Call RunRoundTrip(IO.Path.Combine(tmpDir, "i16.corstore"), geneIds, truth, CorrelationEncodings.QuantizedInt16, 0.0001)
        Call RunStreamRoundTrip(geneIds, truth)

        Check("PValue(0, n) = 1", CorrelationPValues.PValue(0.0, SampleN) = 1.0)
        Check("PValue(0.5, n) in (0, 0.001)",
              CorrelationPValues.PValue(0.5, SampleN) > 0 AndAlso CorrelationPValues.PValue(0.5, SampleN) < 0.001)
        Check("PValue(0.9, n) tiny", CorrelationPValues.PValue(0.9, SampleN) < 0.0000001)

        If failures = 0 Then
            Call Console.WriteLine(vbCrLf & "=== ALL CORSTORE TESTS PASSED ===")
        Else
            Call Console.WriteLine(vbCrLf & $"=== {failures} CHECK(S) FAILED ===")
            Environment.ExitCode = 1
        End If

        Call Console.ReadLine()
    End Sub

    ''' <summary>随机对称相关矩阵（约 2% NaN，对角线 1.0）</summary>
    Private Function MakeRandomMatrix(n As Integer) As Single(,)
        Dim rng As New Random(20261004)
        Dim m(n - 1, n - 1) As Single

        For i = 0 To n - 1 : m(i, i) = 1.0F : Next

        For i = 0 To n - 2
            For j = i + 1 To n - 1
                If rng.NextDouble() < 0.02 Then
                    ' 显式写入 NaN（模拟未定义的基因对）
                    m(i, j) = Single.NaN : m(j, i) = Single.NaN
                    Continue For
                End If
                Dim v = CSng(rng.NextDouble() * 2.0 - 1.0)
                m(i, j) = v : m(j, i) = v
            Next
        Next

        Return m
    End Function

    Private Sub RunRoundTrip(path$, geneIds As String(), truth As Single(,), encoding As CorrelationEncodings, tolerance#)
        Dim label = encoding.ToString
        Call Console.WriteLine($"--- round-trip [{label}] ---")

        Using writer As New CorrelationMatrixWriter(path, geneIds, SampleN, encoding)
            For i = 0 To geneIds.Length - 1
                Dim row(geneIds.Length - 1) As Single
                For j = 0 To geneIds.Length - 1 : row(j) = truth(i, j) : Next
                Call writer.WriteRow(geneIds(i), row)
            Next
            Call writer.Complete()
        End Using

        Call Console.WriteLine($"  written: {New IO.FileInfo(path).Length / 1024.0:F1} KB (N={geneIds.Length})")

        Using store = CorrelationMatrixStore.Open(path)
            Check($"[{label}] size = N", store.size = geneIds.Length)
            Check($"[{label}] SampleN preserved", store.SampleN = SampleN)
            Check($"[{label}] gene table preserved",
                  store.genes(0) = geneIds(0) AndAlso store.genes(N - 1) = geneIds(N - 1))

            ' 点查精度（随机 2000 对）+ 大小写/顺序无关
            Dim rng As New Random(42)
            Dim maxErr As Double = 0, ok = True

            For t = 0 To 2000
                Dim i = rng.Next(N), j = rng.Next(N)
                If i = j OrElse Single.IsNaN(truth(i, j)) Then Continue For
                Dim r = store.GetCorrelation(geneIds(i), geneIds(j))
                Dim err = Math.Abs(r.cor - CDbl(truth(i, j)))
                If err > maxErr Then maxErr = err
                If err > tolerance Then ok = False : Exit For
            Next

            Check($"[{label}] point max error {maxErr:E2} <= {tolerance:E}", ok)

            Dim diag = store.GetCorrelation(geneIds(3), geneIds(3))
            Check($"[{label}] diagonal = (1, 0)", diag.cor = 1.0 AndAlso diag.pvalue = 0.0)

            Dim r1 = store.GetCorrelation(geneIds(10).ToLower, geneIds(20).ToUpper)
            Dim r2 = store.GetCorrelation(geneIds(20), geneIds(10))
            Check($"[{label}] case-insensitive & order-independent", Math.Abs(r1.cor - r2.cor) <= tolerance)

            ' NaN 哨兵
            Dim nanTest = False
            For i = 0 To N - 2
                For j = i + 1 To N - 1
                    If Single.IsNaN(truth(i, j)) Then
                        nanTest = Single.IsNaN(store.GetCorrelation(geneIds(i), geneIds(j)).cor)
                        Exit For
                    End If
                Next
                If nanTest Then Exit For
            Next
            Check($"[{label}] NaN sentinel round-trip", nanTest)

            ' 邻域查询 vs 暴力扫描
            Dim probe = 7
            Dim nb = store.Neighbors(geneIds(probe), 0.5)
            Dim expected = 0
            For j = 0 To N - 1
                If j <> probe AndAlso Not Single.IsNaN(truth(probe, j)) AndAlso Math.Abs(truth(probe, j)) >= 0.5 Then expected += 1
            Next
            Check($"[{label}] Neighbors count = {expected}", nb.Length = expected)
            Check($"[{label}] Neighbors sorted desc",
                  nb.SequenceEqual(nb.OrderByDescending(Function(h) Math.Abs(h.cor)).ToArray))
            Check($"[{label}] Neighbors all >= threshold", nb.All(Function(h) Math.Abs(h.cor) >= 0.5))

            ' StreamEdges vs 暴力上三角
            Dim edges = store.StreamEdges(0.8).ToList()
            Dim brute = 0
            For i = 0 To N - 2
                For j = i + 1 To N - 1
                    If Not Single.IsNaN(truth(i, j)) AndAlso Math.Abs(truth(i, j)) >= 0.8 Then brute += 1
                Next
            Next
            Check($"[{label}] StreamEdges count = {brute}", edges.Count = brute)
            Check($"[{label}] StreamEdges no duplicates",
                  edges.Select(Function(e) If(String.Compare(e.a, e.b) <= 0, e.a & "|" & e.b, e.b & "|" & e.a)).Distinct.Count = edges.Count)

            Dim subEdges = store.StreamEdges(0.8, geneIds.Take(50)).ToList()
            Check($"[{label}] StreamEdges subset ({subEdges.Count} edges)",
                  subEdges.All(Function(e) Array.IndexOf(geneIds, e.a) < 50 AndAlso Array.IndexOf(geneIds, e.b) < 50))
        End Using
    End Sub

    Private Sub Check(name$, ok As Boolean)
        Call Console.WriteLine($"  [{If(ok, "PASS", "FAIL")}] {name}")
        If Not ok Then failures += 1
    End Sub

    ''' <summary>
    ''' 外部 Stream 模式 round-trip：矩阵与索引分别写入两个自定义 MemoryStream，
    ''' 然后用 Open(stream, stream) 重新打开验证点查/邻域/流式筛选。
    ''' </summary>
    Private Sub RunStreamRoundTrip(geneIds As String(), truth As Single(,))
        Call Console.WriteLine("--- round-trip [external streams] ---")

        Dim dataStream As New System.IO.MemoryStream
        Dim indexStream As New System.IO.MemoryStream

        ' ① 写入（writer 不关闭外部流）
        Using writer As New CorrelationMatrixWriter(dataStream, indexStream, geneIds, SampleN,
                                                    CorrelationEncodings.Float32)
            For i = 0 To geneIds.Length - 1
                Dim row(geneIds.Length - 1) As Single
                For j = 0 To geneIds.Length - 1 : row(j) = truth(i, j) : Next
                Call writer.WriteRow(geneIds(i), row)
            Next
            Call writer.Complete()
        End Using

        Check($"[streams] data stream written ({dataStream.Length} bytes)", dataStream.Length > 0)
        Check($"[streams] index stream written ({indexStream.Length} bytes)", indexStream.Length > 0)

        ' ② 读取前把可写流转为可读视图（MemoryStream 直接 Seek(0)；真实场景由外部存储实现）
        Call dataStream.Seek(0, System.IO.SeekOrigin.Begin)
        Call indexStream.Seek(0, System.IO.SeekOrigin.Begin)

        Using store = CorrelationMatrixStore.Open(dataStream, indexStream)
            Dim rng As New Random(7)
            Dim ok = True

            For t = 0 To 500
                Dim i = rng.Next(N), j = rng.Next(N)
                If i = j OrElse Single.IsNaN(truth(i, j)) Then Continue For
                Dim r = store.GetCorrelation(geneIds(i), geneIds(j))
                If Math.Abs(r.cor - CDbl(truth(i, j))) > 0.000001 Then ok = False : Exit For
            Next

            Check($"[streams] point query precision", ok)

            Dim nb = store.Neighbors(geneIds(7), 0.5)
            Dim expected = 0
            For j = 0 To N - 1
                If j <> 7 AndAlso Not Single.IsNaN(truth(7, j)) AndAlso Math.Abs(truth(7, j)) >= 0.5 Then expected += 1
            Next
            Check($"[streams] Neighbors count = {expected}", nb.Length = expected)

            Dim edges = store.StreamEdges(0.8).ToList()
            Dim brute = 0
            For i = 0 To N - 2
                For j = i + 1 To N - 1
                    If Not Single.IsNaN(truth(i, j)) AndAlso Math.Abs(truth(i, j)) >= 0.8 Then brute += 1
                Next
            Next
            Check($"[streams] StreamEdges count = {brute}", edges.Count = brute)
        End Using
    End Sub
End Module
