#Region "Microsoft.VisualBasic::68a661afeb2df8b705c9b0a727e83107, ..\sciBASIC#\gr\3DEngineTest\3DEngineTest\Landscape_model\LandscapeModel.vb"

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

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Module LandscapeModel

    Public Function ModelData() As Landscape.Data.Graphics
        Dim faces As Surface() = New Cube(20).faces
        Dim colors = {"red", "black", "yellow", "green", "blue", "gray"}

        Return New Landscape.Data.Graphics With {
            .bg = "lightblue",
            .Surfaces = faces.ToArray(
            Function(f, i) New Landscape.Data.Surface With {
                .paint = colors(i),
                .vertices = f.vertices.ToArray(Function(pt) New Landscape.Data.Vector(pt))
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

