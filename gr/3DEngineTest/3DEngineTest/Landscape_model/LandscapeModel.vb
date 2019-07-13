#Region "Microsoft.VisualBasic::d7b2432fe1447982f3b810eb330716f7, gr\3DEngineTest\3DEngineTest\Landscape_model\LandscapeModel.vb"

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

    ' Module LandscapeModel
    ' 
    '     Function: ModelData
    ' 
    '     Sub: SaveDemo
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Linq

Module LandscapeModel

    Public Function ModelData() As Landscape.Data.Graphics
        Dim faces As Surface() = New Cube(20).faces
        Dim colors = {"red", "black", "yellow", "green", "blue", "gray"}

        Return New Landscape.Data.Graphics With {
            .bg = "lightblue",
            .Surfaces = faces _
                .Select(Function(f, i)
                            Return New Landscape.Data.Surface With {
                                .paint = colors(i),
                                .vertices = f.vertices _
                                    .Select(Function(pt) New Landscape.Data.Vector(pt)) _
                                    .ToArray
                            }
                        End Function) _
                .ToArray
        }
    End Function

    Public Sub SaveDemo()
        Dim path$ = App.HOME & "/demo.xml"

        If Not path.FileExists Then
            Call ModelData.SaveAsXml(path)
        End If
    End Sub
End Module
