#Region "Microsoft.VisualBasic::28d44ac585d9edee84e666f7522bacc0, ..\sciBASIC#\gr\3DEngineTest\3DEngineTest\Simple\CubeModel.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

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
