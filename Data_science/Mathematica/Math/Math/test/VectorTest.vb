#Region "Microsoft.VisualBasic::d8c29cc86662446cb5362b6bc3bd5b25, Data_science\Mathematica\Math\Math\test\VectorTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module VectorTest
    ' 
    '     Sub: Main, numpyTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports numpy = Microsoft.VisualBasic.Math.NumpyExtensions

Module VectorTest

    Sub Main()
        Dim aa As Vector = {0, 0, 0, 0}
        Dim taa = 1 / aa
        Dim NaN = aa * taa

        Dim inf = 1 / 0
        Dim nanQ = 0 * inf

        Pause()

        Dim x As Double() = {423, 4, 2, 4, 24, 2, 3, 423, 4, 2, 3, 4, 23, 4, 2, 4, 2, 3, 4, 2, 4, 2}
        Dim y As Vector = Vector.Call(Of Double)(New Func(Of Double, Double, Double)(AddressOf Math.Log), x, 2).ToArray
        Dim z As Vector = Vector.Call(Function(a, b) a / b, x, 1000000)

        Call numpyTest()

        Pause()
    End Sub

    Sub numpyTest()
        Dim a = {New Vector({1.0#, 2.0#}), New Vector({3.0#, 4.0#})}

        Console.WriteLine("mean")

        Console.WriteLine(numpy.Mean(a).ToString)
        Console.WriteLine(numpy.Mean(a, axis:=0).ToString)
        Console.WriteLine(numpy.Mean(a, axis:=1).ToString)

        Console.WriteLine("std")

        Console.WriteLine(numpy.Std(a).ToString)
        Console.WriteLine(numpy.Std(a, axis:=0).ToString)
        Console.WriteLine(numpy.Std(a, axis:=1).ToString)

        a = {New Vector({0#, 1.0#}), New Vector({0#, 5.0#})}

        Console.WriteLine("sum")

        Console.WriteLine(numpy.Sum(a).ToString)
        Console.WriteLine(numpy.Sum(a, axis:=0).ToString)
        Console.WriteLine(numpy.Sum(a, axis:=1).ToString)

        a = {New Vector({1.0#, 4.0#}), New Vector({3.0#, 1.0#})}

        Console.WriteLine("sort")

        Console.WriteLine(numpy.Sort(a).ToMatrix.ToVectorList.GetJson(indent:=True))
        Console.WriteLine(numpy.Sort(a, axis:=Nothing).ToMatrix.ToVectorList.GetJson)
        Console.WriteLine(numpy.Sort(a, axis:=0).ToMatrix.ToVectorList.GetJson(indent:=True))

        Pause()
    End Sub
End Module
