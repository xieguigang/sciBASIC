Imports Microsoft.VisualBasic.DataMining.Bonsai
Imports System.Math
Imports System
Imports System.Linq

Module Program
    Sub Main()
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
