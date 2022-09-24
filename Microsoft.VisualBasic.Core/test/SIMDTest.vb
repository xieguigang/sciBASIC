Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module SIMDTest

    Sub Main()
        Dim nsize = 90000001
        Dim a As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray
        Dim b As Double() = nsize.Sequence.Select(Function(i) randf.NextDouble).ToArray
        Dim samplesLegacy As New List(Of Double)
        Dim samplesAvx As New List(Of Double)

        For i As Integer = 0 To 100
            SIMD.config = SIMDConfiguration.enable

            Dim t1 = App.NanoTime
            Dim c = SIMD.Add(a, b)
            Dim t2 = App.NanoTime

            SIMD.config = SIMDConfiguration.disable

            Dim t3 = App.NanoTime
            Dim d = SIMD.Add(a, b)
            Dim t4 = App.NanoTime

            SIMD.config = SIMDConfiguration.legacy

            Dim t5 = App.NanoTime
            Dim e = SIMD.Add(a, b)
            Dim t6 = App.NanoTime

            Call Console.WriteLine(c.SequenceEqual(d))
            Call Console.WriteLine(c.SequenceEqual(e))
            Call Console.WriteLine(t2 - t1)
            Call Console.WriteLine(t4 - t3)
            Call Console.WriteLine(t6 - t5)
            Call Console.WriteLine((t2 - t1) / (t4 - t3))
            Call Console.WriteLine((t6 - t5) / (t4 - t3))

            Call samplesAvx.Add((t2 - t1) / (t4 - t3))
            Call samplesLegacy.Add((t6 - t5) / (t4 - t3))

            Call Console.WriteLine("--------------")
        Next

        Call Console.WriteLine()
        Call Console.WriteLine()
        Call Console.WriteLine()

        Call Console.WriteLine(samplesAvx.Average)
        Call Console.WriteLine(samplesLegacy.Average)

        Pause()
    End Sub
End Module
