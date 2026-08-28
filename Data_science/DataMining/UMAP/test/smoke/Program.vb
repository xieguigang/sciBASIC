Imports System.Diagnostics
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports std = System.Math

Module Program

    Sub Main()
        Call TestABParams()
        Call TestUmap("sequential      ", ParallelConfig.Sequential, 1.0, 0.1)
        Call TestUmap("parallel        ", ParallelConfig.Default, 1.0, 0.1)
        Call TestUmap("parallel spread2", ParallelConfig.Default, 2.0, 0.5)
        Call TestUmap("parallel negRate", ParallelConfig.Default, 1.0, 0.1, negativeSampleRate:=20)
    End Sub

    Private Sub TestABParams()
        Console.WriteLine("========== ABParams ==========")

        Dim d = ABParams.FindABParams(1, 0.1)
        Console.WriteLine($"default(1, 0.1) => a={d.a}, b={d.b}  (expect 1.5694705247879 / 0.8941996)")

        For Each s In {0.5, 1.0, 1.5, 2.0}
            For Each m In {0.01, 0.1, 0.5, 0.9}
                Dim ab = ABParams.FindABParams(s, m)
                Console.WriteLine($"spread={s}, minDist={m} => a={ab.a.ToString("F6")}, b={ab.b.ToString("F6")}")
            Next
        Next
    End Sub

    Private Function MakeData(n As Integer, dims As Integer) As Double()()
        Dim rnd As New Random(42)
        Dim data As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            Dim cluster As Integer = i Mod 3
            Dim v As Double() = New Double(dims - 1) {}

            For d As Integer = 0 To dims - 1
                v(d) = cluster * 5 + rnd.NextDouble()
            Next

            data(i) = v
        Next

        Return data
    End Function

    Private Sub TestUmap(tag As String, par As ParallelConfig, spread As Double, minDist As Double,
                         Optional negativeSampleRate As Integer = 5)

        Dim data As Double()() = MakeData(6000, 24)
        Dim sw As Stopwatch = Stopwatch.StartNew()

        Dim umap As New Umap(
            distance:=AddressOf DistanceFunctions.Euclidean,
            numberOfNeighbors:=15,
            dimensions:=2,
            spread:=spread,
            minDist:=minDist,
            negativeSampleRate:=negativeSampleRate,
            customNumberOfEpochs:=150,
            parallelism:=par)

        Dim nEpochs As Integer = umap.InitializeFit(data)
        sw.Stop()
        Console.WriteLine($"[{tag}] InitializeFit = {sw.ElapsedMilliseconds} ms, nEpochs={nEpochs}")

        sw.Restart()
        Call umap.Step(nEpochs)
        sw.Stop()

        Dim emb As Double()() = umap.GetEmbedding()
        Dim graph = umap.GetGraph()

        ' sanity check: the three clusters should be separated in the 2d space
        Dim cx As Double() = New Double(2) {}
        Dim cy As Double() = New Double(2) {}

        For i As Integer = 0 To emb.Length - 1
            cx(i Mod 3) += emb(i)(0)
            cy(i Mod 3) += emb(i)(1)
        Next
        For c As Integer = 0 To 2
            cx(c) /= emb.Length / 3
            cy(c) /= emb.Length / 3
        Next

        Dim centreDist As Double = 0

        For a As Integer = 0 To 2
            For b As Integer = a + 1 To 2
                centreDist += std.Sqrt((cx(a) - cx(b)) ^ 2 + (cy(a) - cy(b)) ^ 2)
            Next
        Next

        Console.WriteLine($"[{tag}] SGD = {sw.ElapsedMilliseconds} ms, embedding={emb.Length}x{emb(0).Length}, graph={graph.Dims.rows}x{graph.Dims.cols}, cluster separation={centreDist.ToString("F3")}")
    End Sub

End Module
