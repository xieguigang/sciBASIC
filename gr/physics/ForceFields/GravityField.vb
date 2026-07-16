#Region "Microsoft.VisualBasic::3f578cb31526f6df7fcc00b0cf4757d2, gr\physics\ForceFields\GravityField.vb"

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
    '    Code Lines: 15 (53.57%)
    ' Comment Lines: 7 (25.00%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 971 B


    '     Class GravityField
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 重力场：在区域内施加与质量成正比的恒定重力。

Imports Microsoft.VisualBasic.Imaging.Physics

Namespace ForceFields

    ''' <summary>
    ''' 重力场：对场内每个刚体施加 F = m·g（默认向下）。
    ''' 可用于创建局部重力井或不同强度的重力区域。
    ''' </summary>
    Public Class GravityField : Inherits ForceField

        ''' <summary>重力加速度向量（单位：长度/秒²）</summary>
        Public Gravity As Vector2

        Sub New(Optional g As Vector2 = Nothing)
            Me.Gravity = If(g Is Nothing, New Vector2(0, 9.8), g)
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody))
            For Each b In bodies
                If b.IsStatic Then Continue For
                If InRegion(b) Then b.ApplyForce(Gravity * b.Mass)
            Next
        End Sub
    End Class
End Namespace

