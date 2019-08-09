Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.Numerics

Module HalfTest

    Sub Main()
        Dim one As Half = 1.01!

        Console.WriteLine(one)

        Dim p100 As Half() = Replicate(CType(-100.31!, Half), 5).ToArray

        ' Console.WriteLine(p100)


        Dim dbl As Double() = Replicate(9999.009999, 5).ToArray

        Console.WriteLine(Half.MaxValue)
        Console.WriteLine(Half.MinValue)

        Console.WriteLine(HeapSizeOf.MeasureSize(one))
        Console.WriteLine(HeapSizeOf.MeasureSize(p100))
        Console.WriteLine(HeapSizeOf.MeasureSize(dbl))


        Pause()
    End Sub
End Module
