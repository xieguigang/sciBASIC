#Region "Microsoft.VisualBasic::d54095729ae88b6dc62ca0f25975dfe7, gr\physics\Joints\RevoluteJoint.vb"

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

    '   Total Lines: 64
    '    Code Lines: 44 (68.75%)
    ' Comment Lines: 9 (14.06%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 11 (17.19%)
    '     File Size: 2.60 KB


    '     Class RevoluteJoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: SolveAxis, SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 铰链/球窝关节：允许绕单轴旋转，约束两个锚点重合（2D 下等价于球窝）。

Imports Microsoft.VisualBasic.Imaging.Physics

Namespace Joints

    ''' <summary>
    ''' 铰链（旋转）关节：将两个刚体在各自的局部锚点处钉在一起，允许绕该点自由旋转。
    ''' 在 2D 中这与球窝关节等价。
    ''' </summary>
    Public Class RevoluteJoint : Implements IConstraint
        Public A As RigidBody
        Public B As RigidBody
        ''' <summary>物体 A 局部坐标系下的锚点（相对质心）</summary>
        Public anchorA As Vector2
        ''' <summary>物体 B 局部坐标系下的锚点（相对质心）</summary>
        Public anchorB As Vector2

        Private jAcc As Vector2 = Vector2.Zero

        Sub New(a As RigidBody, b As RigidBody, anchorA As Vector2, anchorB As Vector2)
            Me.A = a
            Me.B = b
            Me.anchorA = anchorA
            Me.anchorB = anchorB
        End Sub

        Public Sub SolveVelocity(dt As Double) Implements IConstraint.SolveVelocity
            Dim rA = Rotate(anchorA, A.Rotation)
            Dim rB = Rotate(anchorB, B.Rotation)
            SolveAxis(New Vector2(1, 0), rA, rB)
            SolveAxis(New Vector2(0, 1), rA, rB)
        End Sub

        Private Sub SolveAxis(normal As Vector2, rA As Vector2, rB As Vector2)
            Dim rv = B.Velocity + Cross(B.AngularVelocity, rB) - A.Velocity - Cross(A.AngularVelocity, rA)
            Dim vn = Dot(rv, normal)
            Dim rnA = Cross(rA, normal), rnB = Cross(rB, normal)
            Dim k = A.InvMass + B.InvMass + A.InvInertia * rnA * rnA + B.InvInertia * rnB * rnB
            If k < 1.0e-12 Then Return

            Dim j = -vn / k
            If normal.x <> 0 Then jAcc = New Vector2(jAcc.x + j, jAcc.y) Else jAcc = New Vector2(jAcc.x, jAcc.y + j)

            Dim P = normal * j
            A.ApplyImpulse(-P, rA)
            B.ApplyImpulse(P, rB)
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint.SolvePosition
            Dim rA = Rotate(anchorA, A.Rotation)
            Dim rB = Rotate(anchorB, B.Rotation)
            Dim pa = A.Position + rA
            Dim pb = B.Position + rB
            Dim err = pb - pa

            ' 软修正（Baumgarte），按比例分配到两个物体
            Dim corr = err * 0.2
            A.Position -= A.InvMass * corr
            B.Position += B.InvMass * corr
        End Sub
    End Class
End Namespace

