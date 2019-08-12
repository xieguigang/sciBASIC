Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Numerics

Module HalfTest

    Sub Main()

        Call MathTest()
        Call memoryTest()

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

    Sub MathTest()
        Dim raw As Double() = 3.0.Replicate(10).AsList + 0.000000000001.Replicate(5000000)
        Dim t1, t2, t3 As Long

        Dim vec As New Vector(raw)
        Dim half As New HalfVector(raw)
        Dim start = App.ElapsedMilliseconds

        Dim result = (Math.E ^ (-1 * (vec + 2))).Sum
        t1 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim resultHalf = (Math.E ^ (-1 * (half + 2))).Sum
        t2 = App.ElapsedMilliseconds - start


        Call Console.WriteLine($"result1={result}, {t1}ms")
        Call Console.WriteLine($"result2={resultHalf}, {t2}ms")
    End Sub

    Sub memoryTest()

        Dim raw As Double() = 200.0.Replicate(10).AsList + 0.000000000001.Replicate(5000000)
        Dim start = App.ElapsedMilliseconds
        Dim t1, t2, t3 As Long

        start = App.ElapsedMilliseconds
        Dim vector As New Vector(raw)

        t1 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim sparse As New SparseVector(raw)

        t2 = App.ElapsedMilliseconds - start

        start = App.ElapsedMilliseconds
        Dim halfs As New HalfVector(raw)

        t3 = App.ElapsedMilliseconds - start


        Call $"Takes {t1}ms to allocate {HeapSizeOf.MeasureSize(vector) / 1024 / 1024} MB".__DEBUG_ECHO
        Call $"Takes {t2}ms to allocate {HeapSizeOf.MeasureSize(sparse) / 1024 / 1024} MB".__DEBUG_ECHO
        Call $"Takes {t3}ms to allocate {HeapSizeOf.MeasureSize(halfs) / 1024 / 1024} MB".__DEBUG_ECHO

        Pause()
    End Sub
End Module
