#Region "Microsoft.VisualBasic::b34e8f3d7fe019cf39c6bcd1fb3bbba4, gr\physics\ForceFields\MagneticField.vb"

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
    '    Code Lines: 29 (58.00%)
    ' Comment Lines: 10 (20.00%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 11 (22.00%)
    '     File Size: 1.82 KB


    '     Class MagneticField
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 磁力场：以点源为中心的吸引/排斥力场（随距离平方衰减）。

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports std = System.Math

Namespace ForceFields

    ''' <summary>
    ''' 磁力场（点源）：以 <see cref="Center"/> 为中心，对场内刚体施加随距离
    ''' 平方衰减的吸引或排斥力，模拟磁极/引力井等效果。
    ''' </summary>
    Public Class MagneticField : Inherits ForceField

        ''' <summary>力场中心</summary>
        Public Center As Vector2

        ''' <summary>强度</summary>
        Public Strength As Double

        ''' <summary>True = 吸引，False = 排斥</summary>
        Public Attract As Boolean = True

        ''' <summary>影响半径（超过此距离的刚体不受影响，默认全局）</summary>
        Public Radius As Double = Double.MaxValue

        Sub New(center As Vector2, strength As Double, Optional attract As Boolean = True)
            Me.Center = center
            Me.Strength = strength
            Me.Attract = attract
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody))
            For Each b In bodies
                If b.IsStatic Then Continue For
                If Not InRegion(b) Then Continue For

                Dim d = b.Position - Center
                Dim dist = Length(d)
                If dist < 1.0e-4 OrElse dist > Radius Then Continue For

                Dim dir = d / dist
                Dim sign = If(Attract, -1.0, 1.0)   ' 吸引时朝向中心 = -dir
                Dim f = sign * Strength / (dist * dist + 1.0)
                b.ApplyForce(dir * (f * b.Mass))
            Next
        End Sub
    End Class
End Namespace

