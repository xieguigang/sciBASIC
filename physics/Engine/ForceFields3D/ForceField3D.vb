#Region "Microsoft.VisualBasic::deac87561453e2e0c982444b06a4325d, physics\Engine\ForceFields3D\ForceField3D.vb"

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

    '   Total Lines: 28
    '    Code Lines: 13 (46.43%)
    ' Comment Lines: 9 (32.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 1.03 KB


    '     Class ForceField3D
    ' 
    '         Properties: Region
    ' 
    '         Function: InRegion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Engine.Physics.Collision3D

Namespace ForceFields3D

    ''' <summary>
    ''' 3D 力场抽象基类，与 2D 的 <see cref="ForceFields.ForceField"/> 同构：
    ''' 场可以只在某个 <see cref="Region"/> 内生效，也可以作用于整个世界。
    ''' </summary>
    Public MustInherit Class ForceField3D

        ''' <summary>
        ''' 作用区域；<c>Nothing</c> 表示对整个世界生效。
        ''' </summary>
        Public Property Region As AABB3D? = Nothing

        ''' <summary>把场的作用力累加到各刚体。</summary>
        Public MustOverride Sub Apply(bodies As IEnumerable(Of RigidBody3D))

        ''' <summary>刚体是否位于本场的作用区域内。</summary>
        Protected Function InRegion(b As RigidBody3D) As Boolean
            If Region Is Nothing Then
                Return True
            End If

            Return Region.Value.Overlaps(b.GetAABB())
        End Function
    End Class
End Namespace
