Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module SIMDTest

    Sub Main()
        Dim nsize = 10000000
        Dim a As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray
        Dim b As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray

        For i As Integer = 0 To 10
            SIMD.enable = True

            Dim t1 = App.NanoTime
            Dim c = SIMD.Add(a, b)
            Dim t2 = App.NanoTime

            SIMD.enable = False

            Dim t3 = App.NanoTime
            Dim d = SIMD.Add(a, b)
            Dim t4 = App.NanoTime

            Call Console.WriteLine(c.SequenceEqual(d))
            Call Console.WriteLine(t2 - t1)
            Call Console.WriteLine(t4 - t3)
            Call Console.WriteLine((t4 - t3) / (t2 - t1))
        Next

        Pause()
    End Sub
End Module
