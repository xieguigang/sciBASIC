Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports System.Runtime.CompilerServices

Module ConcaveHull_demo

    <Extension>
    Sub Run_ConcaveHull_demo(points As List(Of Point))
        Dim mm As New DelaunayMesh2d With {
            .Points = points
        }

        mm.InitEdgesInfo()

        Dim edges = mm.GetBoundaryEdges
        Dim vex = mm.Points.AsList(edges.Select(Function(l) {l.P0Index, l.P1Index}).IteratesALL.Distinct.ToArray)

        Call ConvexHull_demo.Draw(points, vex)
    End Sub
End Module
