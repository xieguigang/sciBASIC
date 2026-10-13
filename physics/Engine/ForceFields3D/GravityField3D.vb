#Region "Microsoft.VisualBasic::f0d8c7a6e9c3363d9c1140bc8a298aa5, physics\Engine\ForceFields3D\GravityField3D.vb"

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

    '   Total Lines: 24
    '    Code Lines: 15 (62.50%)
    ' Comment Lines: 4 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 822 B


    '     Class GravityField3D
    ' 
    '         Properties: Gravity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ForceFields3D

    ''' <summary>
    ''' 3D 重力场：对区域内每个非静态刚体施加 <c>F = m · g</c>。
    ''' </summary>
    Public Class GravityField3D : Inherits ForceField3D

        ''' <summary>重力加速度向量（默认 -9.81 m/s²，Y 轴向上）。</summary>
        Public Property Gravity As Vector3

        Sub New(Optional g As Vector3 = Nothing)
            Me.Gravity = If(g Is Nothing, New Vector3(0, -9.81, 0), g)
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody3D))
            For Each b In bodies
                If b.IsStatic Then Continue For
                If Not InRegion(b) Then Continue For

                Call b.ApplyForce(Gravity * b.Mass)
            Next
        End Sub
    End Class
End Namespace
