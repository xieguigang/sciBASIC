#Region "Microsoft.VisualBasic::77621f10cab45fd4dbddc9a2c569fdfc, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Light.vb"

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

    '   Total Lines: 84
    '    Code Lines: 41 (48.81%)
    ' Comment Lines: 30 (35.71%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 13 (15.48%)
    '     File Size: 3.87 KB


    '     Module Light
    ' 
    '         Function: ComputeLighting, Lighting, Shade
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric
Imports std = System.Math

Namespace Drawing3D

    ''' <summary>
    ''' <see cref="Surface"/>的表面光照的计算
    ''' </summary>
    Public Module Light

        ''' <summary>
        ''' Lambert diffuse + ambient shading.
        ''' Computes the face normal from the first three vertices, orients it
        ''' toward the viewer, and blends the base color toward the light color
        ''' by the lit amount. Degenerate faces (zero normal) keep the base color.
        ''' </summary>
        ''' <param name="vertices">vertices forming one face of the model</param>
        ''' <param name="lightDirection">unit vector pointing toward the light source</param>
        ''' <param name="baseColor">intrinsic surface color</param>
        ''' <param name="ambientStrength">ambient term in [0,1]</param>
        ''' <param name="lightColor">light color, usually white</param>
        ''' <returns></returns>
        <Extension>
        Public Function ComputeLighting(vertices As Point3D(), lightDirection As Point3D, baseColor As Color, ambientStrength As Double, lightColor As Color) As Color
            If vertices Is Nothing OrElse vertices.Length < 3 Then
                Return baseColor
            End If

            ' Face normal via cross product of two edges.
            Dim a = vertices(0)
            Dim b = vertices(1)
            Dim c = vertices(2)
            Dim normal = (b - a).CrossProduct(c - a)

            ' Degenerate face (zero area) -> keep base color.
            Dim mag = normal.Length()

            If mag = 0 Then
                Return baseColor
            End If

            normal = normal.Multiply(1 / mag)

            ' Orient the normal toward the viewer (positive Z) so back-faces are lit.
            If normal.Z < 0 Then
                normal = normal.Multiply(-1)
            End If

            ' Diffuse term, clamped to [0,1].
            Dim diffuse = Max(0, normal.DotProduct(lightDirection))
            Dim factor = ambientStrength + (1 - ambientStrength) * diffuse

            Return Shade(baseColor, lightColor, factor)
        End Function

        ''' <summary>
        ''' 对模型之中的某一个表面进行光照处理
        ''' </summary>
        ''' <param name="path">这个对象是一个平面</param>
        ''' <param name="lightDirection">unit vector pointing toward the light source</param>
        ''' <param name="baseColor">intrinsic surface color</param>
        ''' <param name="ambientStrength">ambient term in [0,1]</param>
        ''' <param name="lightColor">light color, usually white</param>
        <Extension>
        Public Function Lighting(path As Path3D, lightDirection As Point3D, baseColor As Color, ambientStrength As Double, lightColor As Color) As Color
            Return path.Points.ToArray().ComputeLighting(lightDirection, baseColor, ambientStrength, lightColor)
        End Function

        ''' <summary>
        ''' Shade <paramref name="baseColor"/> toward <paramref name="lightColor"/> by <paramref name="factor"/> (in [0,1]).
        ''' </summary>
        Private Function Shade(baseColor As Color, lightColor As Color, factor As Double) As Color
            Dim r = CInt(Fix(baseColor.R * (1 - factor) + lightColor.R * factor))
            Dim g = CInt(Fix(baseColor.G * (1 - factor) + lightColor.G * factor))
            Dim b = CInt(Fix(baseColor.B * (1 - factor) + lightColor.B * factor))

            Return Color.FromArgb(baseColor.A, std.Clamp(r, 0, 255), std.Clamp(g, 0, 255), std.Clamp(b, 0, 255))
        End Function
    End Module
End Namespace
