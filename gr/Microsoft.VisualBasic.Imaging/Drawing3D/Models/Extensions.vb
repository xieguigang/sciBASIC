#Region "Microsoft.VisualBasic::8130913df7755e09b4eb88313a7ec630, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Extensions.vb"

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

    '   Total Lines: 32
    '    Code Lines: 27 (84.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.03 KB


    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) Model3D, TupleZ
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric

Namespace Drawing3D.Models

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Model3D(path As Path3D, color As Color) As Surface
            Return New Surface With {
                .brush = New SolidBrush(color),
                .vertices = path.Points.ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Model3D(shape As Shape3D, color As Color) As Surface()
            Return shape.paths _
                .Select(Function(path) path.Model3D(color)) _
                .ToArray
        End Function

        <Extension>
        Public Function TupleZ(p As PointF, z#) As Point3D
            Return New Point3D(p, z)
        End Function
    End Module
End Namespace
