Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Module LandscapeModel

    Public Function ModelData() As Landscape.Graphics
        Dim faces As Surface() = New Cube(20).faces
        Return New Landscape.Graphics With {
            .bg = "lightblue",
            .Surfaces = faces.ToArray(
            Function(f) New Landscape.Surface With {
                .paint = "white",
                .vertices = f.vertices
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
