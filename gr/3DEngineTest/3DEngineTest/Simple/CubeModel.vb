#Region "Microsoft.VisualBasic::921c3d9e7cfd101519d19d64790802e7, gr\3DEngineTest\3DEngineTest\Simple\CubeModel.vb"

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

    ' Class CubeModel
    ' 
    '     Sub: CubeModel_Load
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models

Public Class CubeModel : Inherits GDIDevice

    Dim cubeModel As New Cube(10)

    Private Sub CubeModel_Load(sender As Object, e As EventArgs) Handles Me.Load
        Model = Function() cubeModel.faces
        bg = Color.LightBlue
        Animation = Sub()
                        ' Update the variable after each frame.
                        _camera.angleX += 1
                        _camera.angleY += 1
                        _camera.angleZ += 1
                    End Sub
    End Sub
End Class
