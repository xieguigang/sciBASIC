#Region "Microsoft.VisualBasic::83604deefdef5879c60606c82d9e462a, physics\Engine\Collision3D\AABB3D.vb"

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

    '   Total Lines: 72
    '    Code Lines: 48 (66.67%)
    ' Comment Lines: 12 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (16.67%)
    '     File Size: 2.92 KB


    '     Structure AABB3D
    ' 
    '         Properties: Center, Extents
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Contains, Expand, FromPoint, Overlaps, ToString
    '                   Union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 三维轴对齐包围盒（AABB），用于宽相位剔除。
    ''' </summary>
    Public Structure AABB3D

        ''' <summary>最小角点。</summary>
        Public min As Vector3
        ''' <summary>最大角点。</summary>
        Public max As Vector3

        Sub New(min As Vector3, max As Vector3)
            Me.min = min
            Me.max = max
        End Sub

        ''' <summary>构造一个只包住单个点的退化包围盒。</summary>
        Public Shared Function FromPoint(p As Vector3) As AABB3D
            Return New AABB3D(p, p)
        End Function

        ''' <summary>包围盒中心。</summary>
        Public ReadOnly Property Center As Vector3
            Get
                Return New Vector3((min.x + max.x) * 0.5, (min.y + max.y) * 0.5, (min.z + max.z) * 0.5)
            End Get
        End Property

        ''' <summary>半长（各轴半径）。</summary>
        Public ReadOnly Property Extents As Vector3
            Get
                Return New Vector3((max.x - min.x) * 0.5, (max.y - min.y) * 0.5, (max.z - min.z) * 0.5)
            End Get
        End Property

        ''' <summary>与另一个包围盒是否相交（接触算相交）。</summary>
        Public Function Overlaps(other As AABB3D) As Boolean
            Return Not (max.x < other.min.x OrElse min.x > other.max.x OrElse
                        max.y < other.min.y OrElse min.y > other.max.y OrElse
                        max.z < other.min.z OrElse min.z > other.max.z)
        End Function

        ''' <summary>点是否落在包围盒内。</summary>
        Public Function Contains(p As Vector3) As Boolean
            Return p.x >= min.x AndAlso p.x <= max.x AndAlso
                   p.y >= min.y AndAlso p.y <= max.y AndAlso
                   p.z >= min.z AndAlso p.z <= max.z
        End Function

        ''' <summary>合并两个包围盒。</summary>
        Public Shared Function Union(a As AABB3D, b As AABB3D) As AABB3D
            Return New AABB3D(
                New Vector3(std.Min(a.min.x, b.min.x), std.Min(a.min.y, b.min.y), std.Min(a.min.z, b.min.z)),
                New Vector3(std.Max(a.max.x, b.max.x), std.Max(a.max.y, b.max.y), std.Max(a.max.z, b.max.z)))
        End Function

        ''' <summary>按给定边距向外扩张（宽相位的"皮肤"厚度）。</summary>
        Public Function Expand(margin As Double) As AABB3D
            Return New AABB3D(
                New Vector3(min.x - margin, min.y - margin, min.z - margin),
                New Vector3(max.x + margin, max.y + margin, max.z + margin))
        End Function

        Public Overrides Function ToString() As String
            Return $"[{min} .. {max}]"
        End Function
    End Structure
End Namespace

