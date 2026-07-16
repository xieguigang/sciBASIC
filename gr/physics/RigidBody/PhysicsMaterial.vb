#Region "Microsoft.VisualBasic::bf9b82a6bd779219c79046f47d8904dc, gr\physics\RigidBody\PhysicsMaterial.vb"

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

    '   Total Lines: 36
    '    Code Lines: 16 (44.44%)
    ' Comment Lines: 10 (27.78%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 10 (27.78%)
    '     File Size: 1.41 KB


    ' Class PhysicsMaterial
    ' 
    '     Properties: Friction, Restitution
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CombineFriction, CombineRestitution
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 物理材质：定义表面摩擦系数与恢复系数，碰撞时由两材质组合决定接触行为。

Imports System.Math
Imports std = System.Math



''' <summary>
''' 物理材质：摩擦系数 (friction) 与恢复系数 (restitution)。
''' 碰撞时由碰撞双方材质组合得到最终的接触参数。
''' </summary>
Public Class PhysicsMaterial

    ''' <summary>静/动摩擦系数（库仑摩擦），0 = 无摩擦，1 = 很强摩擦</summary>
    Public Property Friction As Double = 0.4

    ''' <summary>恢复系数（弹性），0 = 完全非弹性，1 = 完全弹性</summary>
    Public Property Restitution As Double = 0.0

    Sub New(Optional friction As Double = 0.4, Optional restitution As Double = 0.0)
        Me.Friction = friction
        Me.Restitution = restitution
    End Sub

    ''' <summary>两个材质组合的摩擦系数（几何平均，确定性且稳定）</summary>
    Public Shared Function CombineFriction(a As PhysicsMaterial, b As PhysicsMaterial) As Double
        Return std.Sqrt(a.Friction * b.Friction)
    End Function

    ''' <summary>两个材质组合的恢复系数（取较大值）</summary>
    Public Shared Function CombineRestitution(a As PhysicsMaterial, b As PhysicsMaterial) As Double
        Return std.Max(a.Restitution, b.Restitution)
    End Function
End Class


