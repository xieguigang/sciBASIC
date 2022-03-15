#Region "Microsoft.VisualBasic::d7b2432fe1447982f3b810eb330716f7, sciBASIC#\gr\3DEngineTest\3DEngineTest\Landscape_model\LandscapeModel.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 33
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.15 KB


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
