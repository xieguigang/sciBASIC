#Region "Microsoft.VisualBasic::23e02e6a8153ed98d677ee3f03da1034, ..\sciBASIC#\gr\Landscape\3DBuilder\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Vendor_3mf

    Public Module Extensions

        <Extension> Public Function Centra(surfaces As IEnumerable(Of Surface)) As Point3D
            Dim vertices = surfaces.Select(Function(s) s.vertices).ToVector
            Dim x = vertices.Select(Function(p3D) p3D.X).Average
            Dim y = vertices.Select(Function(p3D) p3D.Y).Average
            Dim z = vertices.Select(Function(p3D) p3D.Z).Average

            Return New Point3D(x, y, z)
        End Function

        <Extension>
        Public Function Offsets(offset As Point3D, model As IEnumerable(Of Surface)) As IEnumerable(Of Surface)
            Dim out As New List(Of Surface)

            For Each surface As Surface In model
                out += New Surface With {
                    .brush = surface.brush,
                    .vertices = surface _
                        .vertices _
                        .ToArray(Function(p3D) p3D - offset)
                }
            Next

            Return out
        End Function
    End Module
End Namespace
