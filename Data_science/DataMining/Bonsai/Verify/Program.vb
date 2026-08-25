Imports Microsoft.VisualBasic.DataMining.Bonsai
Imports System.Math
Imports System
Imports System.Linq

Module Program
    ' simple quadratic objective for optimizer self-test
    Private Function quad(x As Double(), args() As Object) As (f As Double, grad As Double())
        Dim f = (x(0) - 2.0) ^ 2
        Dim g = New Double(0) {2.0 * (x(0) - 2.0)}
        Return (f, g)
    End Function

    Sub Main()
        ' optimizer self-test
        Dim bounds As New List(Of (lo As Double, hi As Double)) From {( -10.0, 10.0)}
        Dim r = Microsoft.VisualBasic.DataMining.Bonsai.Optimizer.Minimize(AddressOf quad, New Double() {0.0}, bounds)
        Console.WriteLine("OPT_SELFTEST x=" & r.x(0).ToString("G4") & " f=" & r.fun.ToString("G4") & " ok=" & r.success)


        ' Small synthetic dataset: 6 points in 3-D, two tight clusters.
        Dim rnd As New Random(42)
        Dim D = 3
        Dim N = 6
        Dim means(N - 1)() As Double
        Dim stds(N - 1)() As Double
        Dim names(N - 1) As String

        For i = 0 To N - 1
            means(i) = New Double(D - 1) {}
            stds(i) = New Double(D - 1) {}
            names(i) = "p" & i
            Dim center As Double
            If i < 3 Then center = 0.0 Else center = 5.0
            For g = 0 To D - 1
                means(i)(g) = center + (rnd.NextDouble() - 0.5) * 0.4
                stds(i)(g) = 0.2
            Next
        Next

        Console.WriteLine("=== Bonsai verification ===")
        Dim b As New Microsoft.VisualBasic.DataMining.Bonsai.Bonsai()
        b.verbose = True
        b.maxTimeIters = 50
        b.Fit(means, stds, names)

        ' diagnostics: gradient of first branch time
        Dim firstChild = b.Tree.root.childs(0)
        Console.WriteLine("DIAG firstChild.tParent=" & firstChild.tParent.ToString("G4") &
                          " dLogL/dt=" & firstChild.dLoglikdtParent.ToString("G4") &
                          " nChilds=" & b.Tree.root.childs.Count)

        Console.WriteLine()
        Console.WriteLine("LogLikelihood = " & b.LogLikelihood().ToString("G6"))
        Console.WriteLine("Newick       = " & b.ToNewick())
        Console.WriteLine()

        Dim coords = b.Transform()
        Console.WriteLine("Low-dim coords (leaf effective positions):")
        For i = 0 To coords.Length - 1
            Console.WriteLine("  " & names(i) & " -> " & String.Join(", ", coords(i).Select(Function(x) x.ToString("G3"))))
        Next

        Dim times = b.BranchTimeCoords()
        Console.WriteLine()
        Console.WriteLine("Branch-time (tree depth) per leaf:")
        For i = 0 To times.Length - 1
            Console.WriteLine("  " & names(i) & " -> " & times(i).ToString("G3"))
        Next

        Console.WriteLine()
        Console.WriteLine("=== Done (no crash) ===")
    End Sub
End Module
