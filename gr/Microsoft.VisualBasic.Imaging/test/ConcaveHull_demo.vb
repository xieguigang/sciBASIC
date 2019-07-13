#Region "Microsoft.VisualBasic::54d04c54ba3eca7e35e68261cc9fc0b6, gr\Microsoft.VisualBasic.Imaging\test\ConcaveHull_demo.vb"

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

    ' Module ConcaveHull_demo
    ' 
    '     Sub: Run_ConcaveHull_demo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Module ConcaveHull_demo

    <Extension>
    Sub Run_ConcaveHull_demo(points As List(Of PointF))
        'Dim size = 200
        'Dim x = New DoubleRange(100, 900).rand(size)
        'Dim y = New DoubleRange(100, 800).rand(size)
        'Dim z = New DoubleRange(100, 850).rand(size)
        'Dim v = size.Sequence.Select(Function(i) New Point3D With {.X = x(i), .Y = y(i), .Z = z(i)}).ToArray
        'Dim engine As New DelaunayTriangulation(v)

        'Call engine.Triangulate(150)

        With points
            Call .Draw(.ConcaveHull(200))
        End With
    End Sub
End Module
