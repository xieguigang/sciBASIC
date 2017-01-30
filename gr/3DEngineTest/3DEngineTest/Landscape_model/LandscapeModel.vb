Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Module LandscapeModel

    Public Function ModelData() As Landscape.Graphics
        Dim faces As Surface() = New Cube(20).faces
        Dim colors = {"red", "black", "yellow", "green", "blue", "gray"}

        Return New Landscape.Graphics With {
            .bg = "lightblue",
            .Surfaces = faces.ToArray(
            Function(f, i) New Landscape.Surface With {
                .paint = colors(i),
                .vertices = f.vertices.ToArray(Function(pt) New Landscape.Vector(pt))
            })
        }
    End Function

    Public Sub SaveDemo()
        Dim path$ = App.HOME & "/demo.xml"

        If Not path.FileExists Then
            Call ModelData.SaveAsXml(path)
        End If
    End Sub
End Module
