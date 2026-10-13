#Region "Microsoft.VisualBasic::8a1e8cff017e5a60c2bddcfabe72034d, physics\Engine\ForceFields\ForceField.vb"

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

    '   Total Lines: 26
    '    Code Lines: 11 (42.31%)
    ' Comment Lines: 9 (34.62%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 6 (23.08%)
    '     File Size: 1015 B


    '     Class ForceField
    ' 
    '         Function: InRegion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 力场基类：在其作用区域内对所有刚体施加力。

Imports Microsoft.VisualBasic.Engine.Physics.Collision

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
