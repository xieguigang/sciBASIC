#Region "Microsoft.VisualBasic::07f06bb26c76b26030bce3d834694ae4, gr\physics\Joints\WeldJoint.vb"

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

    '   Total Lines: 89
    '    Code Lines: 66 (74.16%)
    ' Comment Lines: 9 (10.11%)
    '    - Xml Docs: 44.44%
    ' 
    '   Blank Lines: 14 (15.73%)
    '     File Size: 3.40 KB


    '     Class WeldJoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: WrapAngle
    ' 
    '         Sub: SolveAxis, SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 固定关节：锁定全部自由度（位置 + 旋转）。

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports std = System.Math

Namespace Joints

    ''' <summary>
    ''' 固定（焊接）关节：将两个刚体的相对位置与相对角度完全锁定，
    ''' 相当于刚性连接（如焊接、铆接）。
    ''' </summary>
    Public Class WeldJoint : Implements IConstraint
        Public A As RigidBody
        Public B As RigidBody
        Public anchorA As Vector2
        Public anchorB As Vector2

        Private jAcc As Vector2 = Vector2.Zero
        Private angleAcc As Double = 0.0

        Sub New(a As RigidBody, b As RigidBody, anchorA As Vector2, anchorB As Vector2)
            Me.A = a
            Me.B = b
            Me.anchorA = anchorA
            Me.anchorB = anchorB
        End Sub

        Public Sub SolveVelocity(dt As Double) Implements IConstraint.SolveVelocity
            Dim rA = Rotate(anchorA, A.Rotation)
            Dim rB = Rotate(anchorB, B.Rotation)

            ' 位置约束（X、Y 两个轴）
            SolveAxis(New Vector2(1, 0), rA, rB)
            SolveAxis(New Vector2(0, 1), rA, rB)

            ' 角度约束：相对角速度归零
            Dim relAng = B.AngularVelocity - A.AngularVelocity
            Dim invSum = A.InvInertia + B.InvInertia
            If invSum < 1.0e-12 Then Return
            Dim ja = -relAng / invSum
            angleAcc += ja
            A.AngularVelocity -= A.InvInertia * ja
            B.AngularVelocity += B.InvInertia * ja
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
            Dim corr = err * 0.2
            A.Position -= A.InvMass * corr
            B.Position += B.InvMass * corr

            ' 角度修正
            Dim errAngle = WrapAngle(B.Rotation - A.Rotation)
            Dim invSum = A.InvInertia + B.InvInertia
            If invSum > 1.0e-12 Then
                Dim acorr = errAngle * 0.2
                A.Rotation -= A.InvInertia * acorr
                B.Rotation += B.InvInertia * acorr
            End If
        End Sub

        Private Function WrapAngle(a As Double) As Double
            While a > std.PI : a -= 2 * std.PI : End While
            While a < -std.PI : a += 2 * std.PI : End While
            Return a
        End Function
    End Class
End Namespace

