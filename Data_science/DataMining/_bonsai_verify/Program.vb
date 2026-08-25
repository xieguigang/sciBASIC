Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DataMining.Bonsai

Module Program
    Sub Main()
        Dim rnd = New Random(42)
        Dim N = 60
        Dim D = 20
        Dim means(N - 1)() As Double
        Dim stds(N - 1)() As Double
        Dim names(N - 1) As String
        For i = 0 To N - 1
            means(i) = New Double(D - 1) {}
            stds(i) = New Double(D - 1) {}
            Dim blob = If(i < N \ 2, 0, 1)
            For g = 0 To D - 1
                Dim center = If(blob = 0, -3.0, 3.0) * If(g = 0, 1, 0.5)
                means(i)(g) = center + rnd.NextGaussian() * 0.4
                stds(i)(g) = 0.3 + rnd.NextDouble() * 0.2
            Next
            names(i) = "c" & i
        Next

        Dim bA = New Bonsai() With {.verbose = True, .useGlobalVariance = False, .filterLowSNR = True}
        bA.Fit(means, stds, names)
        Dim coordsA = bA.Transform()
        Console.WriteLine("A: logL={0:G4}, 2D rows={1} cols={2}", bA.LogLikelihood(), coordsA.Length, coordsA(0).Length)
        Dim hiA = bA.GetHighDimStates()
        Console.WriteLine("A: highDim rows={0} cols={1}", hiA.Length, hiA(0).Length)

        Dim bB = New Bonsai() With {.verbose = False, .useGlobalVariance = True, .filterLowSNR = True}
        bB.Fit(means, stds, names)
        Dim coordsB = bB.Transform()
        Console.WriteLine("B: logL={0:G4}, 2D rows={1} cols={2}", bB.LogLikelihood(), coordsB.Length, coordsB(0).Length)

        Dim bC = New Bonsai() With {.verbose = False, .filterLowSNR = False, .layout = "radial"}
        bC.Fit(means, stds, names)
        Dim coordsC = bC.Transform()
        Console.WriteLine("C: logL={0:G4}, 2D rows={1} cols={2}, newick={3}", bC.LogLikelihood(), coordsC.Length, coordsC(0).Length, bC.ToNewick().Length > 0)

        Console.WriteLine("ALL PATHS OK")
    End Sub
End Module

Public Module RandomExtensions
    <System.Runtime.CompilerServices.Extension()>
    Public Function NextGaussian(rnd As Random) As Double
        Dim u1 = rnd.NextDouble()
        Dim u2 = rnd.NextDouble()
        Return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2)
    End Function
End Module
