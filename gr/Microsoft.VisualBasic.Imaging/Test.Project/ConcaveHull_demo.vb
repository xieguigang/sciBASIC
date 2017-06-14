Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Module ConcaveHull_demo

    <Extension>
    Sub Run_ConcaveHull_demo(points As List(Of Point))
        'Dim size = 200
        'Dim x = New DoubleRange(100, 900).rand(size)
        'Dim y = New DoubleRange(100, 800).rand(size)
        'Dim z = New DoubleRange(100, 850).rand(size)
        'Dim v = size.Sequence.Select(Function(i) New Point3D With {.X = x(i), .Y = y(i), .Z = z(i)}).ToArray
        'Dim engine As New DelaunayTriangulation(v)

        'Call engine.Triangulate(150)

        With points
            Call .Draw(.ConcaveHull(500))
        End With
    End Sub
End Module
