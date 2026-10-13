#Region "Microsoft.VisualBasic::cde7447eeffa926a5afbbae4c62a06b4, gr\Landscape\Voxelization\SDFTriangle.vb"

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

    '   Total Lines: 50
    '    Code Lines: 30 (60.00%)
    ' Comment Lines: 11 (22.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (18.00%)
    '     File Size: 1.92 KB


    '     Structure SDFTriangle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

Namespace Voxelization

    ''' <summary>
    ''' SDF 体素化使用的三角面片，预存三个顶点、轴对齐包围盒 (AABB) 与质心，
    ''' 以加速 BVH 的构建与最近三角面查询。
    ''' </summary>
    Public Structure SDFTriangle

        Public A As Point3D
        Public B As Point3D
        Public C As Point3D

        ''' <summary>三角形 AABB 最小角点</summary>
        Public MinCorner As Point3D
        ''' <summary>三角形 AABB 最大角点</summary>
        Public MaxCorner As Point3D
        ''' <summary>三角形质心（AABB 中位数分割用）</summary>
        Public Centroid As Point3D

        Public Sub New(a As Point3D, b As Point3D, c As Point3D)
            Me.A = a
            Me.B = b
            Me.C = c

            Dim minX As Double = std.Min(a.X, std.Min(b.X, c.X))
            Dim minY As Double = std.Min(a.Y, std.Min(b.Y, c.Y))
            Dim minZ As Double = std.Min(a.Z, std.Min(b.Z, c.Z))
            Dim maxX As Double = std.Max(a.X, std.Max(b.X, c.X))
            Dim maxY As Double = std.Max(a.Y, std.Max(b.Y, c.Y))
            Dim maxZ As Double = std.Max(a.Z, std.Max(b.Z, c.Z))

            Me.MinCorner = New Point3D(minX, minY, minZ)
            Me.MaxCorner = New Point3D(maxX, maxY, maxZ)
            Me.Centroid = New Point3D((minX + maxX) * 0.5, (minY + maxY) * 0.5, (minZ + maxZ) * 0.5)
        End Sub

        ''' <summary>
        ''' 三角面的几何法向 (未必单位化前的方向为 (B-A) × (C-A))，返回单位法向。
        ''' 退化三角形返回零向量。
        ''' </summary>
        Public Function Normal() As Point3D
            Dim n = Point3D.Cross(B.Subtract(A), C.Subtract(A))
            Return n.Normalize()
        End Function

    End Structure
End Namespace
