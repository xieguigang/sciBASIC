#Region "Microsoft.VisualBasic::036725cfd3942d802d21446fc050eb84, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Extensions.vb"

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

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric

Namespace Drawing3D.Models

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Model3D(path As Path3D, color As Color) As Surface
            Return New Surface With {
                .brush = New SolidBrush(color),
                .vertices = path.Points.ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Model3D(shape As Shape3D, color As Color) As Surface()
            Return shape _
                .paths _
                .Select(Function(path) path.Model3D(color)) _
                .ToArray
        End Function
    End Module
End Namespace
