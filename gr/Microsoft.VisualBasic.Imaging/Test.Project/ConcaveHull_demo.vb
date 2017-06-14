Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Module ConcaveHull_demo

    Sub Run_ConcaveHull_demo()
        Dim size = 50
        Dim x = New DoubleRange(100, 900).rand(size)
        Dim y = New DoubleRange(100, 800).rand(size)
        Dim z = New DoubleRange(100, 850).rand(size)
        Dim v = size.Sequence.Select(Function(i) New dVertex With {.x = x(i), .y = y(i), .z = z(i)}).ToArray
        Dim engine As New DelaunayTriangulation With {.Vertex = v}

        Call engine.Triangulate(10)

        Pause()
    End Sub
End Module
