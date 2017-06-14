Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Module ConcaveHull_demo

    Sub Run_ConcaveHull_demo()
        Dim size = 200
        Dim x = New DoubleRange(100, 900).rand(size)
        Dim y = New DoubleRange(100, 800).rand(size)
        Dim z = New DoubleRange(100, 850).rand(size)
        Dim v = size.Sequence.Select(Function(i) New Point3D With {.X = x(i), .Y = y(i), .Z = z(i)}).ToArray
        Dim engine As New DelaunayTriangulation(v)

        Call engine.Triangulate(150)

        Pause()
    End Sub
End Module
