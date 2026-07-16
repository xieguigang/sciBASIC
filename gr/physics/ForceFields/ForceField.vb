#Region "Microsoft.VisualBasic::198cb901a70d7b3510043bec886b0d37, gr\physics\ForceFields\ForceField.vb"

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

    '   Total Lines: 27
    '    Code Lines: 12 (44.44%)
    ' Comment Lines: 9 (33.33%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 1.06 KB


    '     Class ForceField
    ' 
    '         Function: InRegion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 力场基类：在其作用区域内对所有刚体施加力。

Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics

Namespace ForceFields

    ''' <summary>
    ''' 力场基类。覆盖一个区域（<see cref="Region"/>，为 Nothing 时表示全局），
    ''' 每步对场内刚体施加力。派生类实现 <see cref="Apply"/>。
    ''' </summary>
    Public MustInherit Class ForceField

        ''' <summary>作用区域；为 Nothing 时作用于世界内所有刚体</summary>
        Public Region As AABB?

        ''' <summary>对给定刚体集合施加力</summary>
        Public MustOverride Sub Apply(bodies As IEnumerable(Of RigidBody))

        ''' <summary>刚体是否处于力场作用区域内</summary>
        Protected Function InRegion(b As RigidBody) As Boolean
            If Not Region.HasValue Then Return True
            Return Region.Value.Overlaps(b.GetAABB())
        End Function
    End Class
End Namespace

