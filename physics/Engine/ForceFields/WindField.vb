#Region "Microsoft.VisualBasic::1b1c24a7917aefa33424a56fb65b0290, physics\Engine\ForceFields\WindField.vb"

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

    '   Total Lines: 43
    '    Code Lines: 23 (53.49%)
    ' Comment Lines: 9 (20.93%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 11 (25.58%)
    '     File Size: 1.46 KB


    '     Class WindField
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 风力场：沿方向施加带湍流扰动的力。

Imports std = System.Math

Namespace ForceFields

    ''' <summary>
    ''' 风力场：在区域内沿 <see cref="Direction"/> 施加风力，叠加正弦湍流使效果更自然。
    ''' 力大小与刚体质量成正比（类比气压对物体的作用）。
    ''' </summary>
    Public Class WindField : Inherits ForceField

        ''' <summary>风向（单位向量）</summary>
        Public Direction As Vector2

        ''' <summary>风力强度</summary>
        Public Strength As Double

        ''' <summary>湍流强度（0 = 恒定风）</summary>
        Public Turbulence As Double = 0.0

        Private phase As Double = 0.0

        Sub New(dir As Vector2, strength As Double, Optional turbulence As Double = 0.0)
            Me.Direction = Normalize(dir)
            Me.Strength = strength
            Me.Turbulence = turbulence
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody))
            phase += 0.1

            For Each b In bodies
                If b.IsStatic Then Continue For
                If Not InRegion(b) Then Continue For

                Dim scale = Strength * (1.0 + Turbulence * 0.5 * std.Sin(phase + b.Position.x * 0.01))
                b.ApplyForce(Direction * (scale * b.Mass))
            Next
        End Sub
    End Class
End Namespace
