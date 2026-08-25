Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.DataMining.Bonsai

Module Program

    ' ---- configuration for the transcriptome test ---------------------------------
    ' The input CSV is gene (rows) x sample (columns). Bonsai expects N (samples) x D (features),
    ' so we transpose and keep only the top-K highly-variable genes as the D-dimensional feature set
    ' (standard single-cell preprocessing; keeps the run tractable on a 60k-gene matrix).
    Private Const CSV_PATH As String = "K:\hsa\Homo_sapiens_expr_advanced_all_conditions.csv"
    Private Const TOP_K As Integer = 500          ' number of highly-variable genes kept as features (D)
    Private Const MAX_MERGES As Integer = 8       ' cap tree-search rounds so the demo finishes quickly
    Private Const MAX_TIME_ITERS As Integer = 5   ' L-BFGS iterations per optTimes call
    Private Const MAX_SAMPLES As Integer = 1000    ' cap #samples used (O(C^2) tree search is costly on 1888)

    ' simple quadratic objective for optimizer self-test
    Private Function quad(x As Double(), args() As Object) As (f As Double, grad As Double())
        Dim f = (x(0) - 2.0) ^ 2
        Dim g = New Double(0) {2.0 * (x(0) - 2.0)}
        Return (f, g)
    End Function

    Sub Main()
        ' ---- optimizer self-test (keeps the lightweight sanity check) ----
        Dim bounds As New List(Of (lo As Double, hi As Double)) From {( -10.0, 10.0)}
        Dim r = Optimizer.Minimize(AddressOf quad, New Double() {0.0}, bounds)
        Console.WriteLine("OPT_SELFTEST x=" & r.x(0).ToString("G4") & " f=" & r.fun.ToString("G4") & " ok=" & r.success)

        ' ---- load the transcriptome expression matrix and build the N x D point set ----
        Console.WriteLine("Loading transcriptome matrix: " & CSV_PATH)
        Dim sw = Stopwatch.StartNew()
        Dim data = LoadExpressionMatrix(CSV_PATH, TOP_K)
        sw.Stop()
        Console.WriteLine($"Loaded {data.names.Length} samples x {data.D} HV-genes in {sw.Elapsed.TotalSeconds:G2}s")

        ' ---- fit the Bonsai tree ----
        Console.WriteLine("Fitting Bonsai tree ...")
        Dim b As New Bonsai() With {.verbose = True, .maxMerges = MAX_MERGES, .maxTimeIters = MAX_TIME_ITERS}
        b.Fit(data.means, names:=data.names)
        Console.WriteLine("LogLikelihood = " & b.LogLikelihood().ToString("G6"))
        Console.WriteLine("Nodes        = " & b.Tree.CountNodes())

        ' ---- results to visualize ----
        Dim coords = b.Transform()                 ' N x D leaf effective positions (sample order)
        Dim times = b.BranchTimeCoords()           ' N branch-time (tree depth)
        Dim root = b.Tree.root

        ' project the high-dimensional embedding to 2-D for the scatter plot
        Dim coords2d = Plot.PCA2D(coords)

        ' ---- render the three PNGs ----
        Dim outDir = AppContext.BaseDirectory
        Dim scatterPath = Path.Combine(outDir, "bonsai_scatter.png")
        Dim treePath = Path.Combine(outDir, "bonsai_tree.png")
        Dim histPath = Path.Combine(outDir, "bonsai_branchtime.png")

        Plot.PlotScatter(coords2d, data.names, times, scatterPath)
        Plot.PlotTree(root, treePath)
        Plot.PlotBranchTimeHistogram(times, histPath)

        Console.WriteLine()
        Console.WriteLine("=== Visualization demo complete ===")
        Console.WriteLine("  scatter : " & scatterPath)
        Console.WriteLine("  tree    : " & treePath)
        Console.WriteLine("  hist    : " & histPath)
    End Sub

    ' =========================================================================
    ' Streaming loader: two passes over the (large) CSV.
    '   Pass 1: per-gene mean / variance  -> pick the top-K highly-variable genes.
    '   Pass 2: transpose the selected genes into an N (samples) x K (genes) matrix.
    ' Memory stays tiny (a few MB) regardless of the 800 MB file size.
    ' =========================================================================
    Private Function LoadExpressionMatrix(path As String, topK As Integer) As (means As Double()(), names As String(), D As Integer)
        Dim sep = ","c
        Dim cult = CultureInfo.InvariantCulture

        ' ---- pass 1 ----
        Dim sampleNames As String() = Nothing
        Dim geneSum As Double() = Nothing
        Dim geneSumSq As Double() = Nothing
        Dim nGenes = 0

        Using reader = New StreamReader(path)
            ' header row = sample names
            Dim header = reader.ReadLine().Split(sep)
            sampleNames = header.Skip(1).ToArray()        ' drop the leading geneID column
            Dim N = sampleNames.Length

            Dim line = reader.ReadLine()
            While line IsNot Nothing
                Dim parts = line.Split(sep)
                If geneSum Is Nothing Then
                    nGenes = parts.Length - 1
                    geneSum = New Double(nGenes - 1) {}
                    geneSumSq = New Double(nGenes - 1) {}
                End If
                For g = 0 To nGenes - 1
                    Dim v = Double.Parse(parts(g + 1), cult)
                    geneSum(g) += v
                    geneSumSq(g) += v * v
                Next
                line = reader.ReadLine()
            End While

            ' per-gene variance: E[x^2] - E[x]^2  (estimated over ALL samples)
            Dim varOfGene(nGenes - 1) As Double
            For g = 0 To nGenes - 1
                Dim mean = geneSum(g) / N
                varOfGene(g) = geneSumSq(g) / N - mean * mean
            Next

            ' cap the number of samples fed to Bonsai (the tree search is O(C^2) per round)
            Dim useN = If(N > MAX_SAMPLES, MAX_SAMPLES, N)
            sampleNames = sampleNames.Take(useN).ToArray()

            ' top-K gene indices by variance
            Dim order = Enumerable.Range(0, nGenes).OrderByDescending(Function(g) varOfGene(g)).ToArray()
            Dim keep = If(topK < nGenes, topK, nGenes)
            Dim keepSet As New HashSet(Of Integer)
            For Each gg In order.Take(keep)
                keepSet.Add(gg)
            Next
            Dim keepIndex = keepSet.OrderBy(Function(g) g).ToArray()   ' sorted for stable pass-2 lookup

            ' ---- pass 2 ----
            Dim means(useN - 1)() As Double
            For i = 0 To useN - 1
                means(i) = New Double(keep - 1) {}
            Next

            reader.BaseStream.Seek(0, SeekOrigin.Begin)
            reader.DiscardBufferedData()
            reader.ReadLine()   ' skip header again

            Dim gi = 0
            line = reader.ReadLine()
            While line IsNot Nothing
                If keepSet.Contains(gi) Then
                    Dim parts = line.Split(sep)
                    Dim col = Array.IndexOf(keepIndex, gi)   ' position within the kept feature set
                    For i = 0 To useN - 1
                        means(i)(col) = Double.Parse(parts(i + 1), cult)
                    Next
                End If
                gi += 1
                line = reader.ReadLine()
            End While

            Return (means, sampleNames, keep)
        End Using
    End Function

End Module
