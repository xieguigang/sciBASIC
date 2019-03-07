Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module VectorTest

    Sub Main()
        Dim x As Double() = {423, 4, 2, 4, 24, 2, 3, 423, 4, 2, 3, 4, 23, 4, 2, 4, 2, 3, 4, 2, 4, 2}
        Dim y As Vector = Vector.Call(Of Double)(New Func(Of Double, Double, Double)(AddressOf Math.Log), x, 2).ToArray
        Dim z As Vector = Vector.Call(Function(a, b) a / b, x, 1000000)

        Pause()
    End Sub
End Module
