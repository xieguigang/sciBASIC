#Region "Microsoft.VisualBasic::81c81874090098a1255fe16716ed06f4, physics\Engine\Collision3D\ContactSolver3D.vb"

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

    '   Total Lines: 72
    '    Code Lines: 45 (62.50%)
    ' Comment Lines: 14 (19.44%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 13 (18.06%)
    '     File Size: 3.21 KB


    '     Module ContactSolver3D
    ' 
    '         Sub: ApplyPair, Solve, SolveFriction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 3D 顺序冲量接触求解器：对每个接触点先解法向（含 Baumgarte 位置修正与恢复），
    ''' 再解两条正交切线的库仑摩擦。跨迭代累积冲量，保证多接触点情况下收敛。
    ''' </summary>
    Public Module ContactSolver3D

        ''' <summary>
        ''' 求解一个流形。应在每个迭代轮次中对所有流形调用一次。
        ''' </summary>
        ''' <param name="m">已调用过 <see cref="Manifold3D.InitSolver(Double, Double, Double)"/> 的流形。</param>
        ''' <param name="dt">当前子步长。</param>
        Public Sub Solve(m As Manifold3D, dt As Double)
            If m Is Nothing OrElse Not m.HasContacts Then
                Return
            End If

            Dim n As Vector3 = m.Normal

            For Each c In m.Contacts
                ' ---------- 法向 ----------
                Dim rv As Vector3 = m.RelativeVelocity(c.rA, c.rB)
                Dim vn As Double = Vector3Math.Dot(rv, n)

                ' gamma 是软约束的软化项：已经累积的冲量要被扣掉，否则每一轮迭代都
                ' 会把它再加一遍。刚性接触的 gamma 为 0，这一项自动消失。
                Dim deltaVn As Double = c.restitutionBias + c.bias - vn - c.gamma * c.NormalImpulse
                Dim jn As Double = deltaVn * c.softMass

                ' 法向冲量只能"推"不能"拉"
                Dim oldNormal As Double = c.NormalImpulse
                c.NormalImpulse = std.Max(oldNormal + jn, 0.0)
                jn = c.NormalImpulse - oldNormal

                Dim Pn As Vector3 = n * jn
                Call ApplyPair(m, Pn, c.rA, c.rB)

                ' ---------- 摩擦（两条正交切线，合起来按库仑锥裁剪） ----------
                Dim maxFriction As Double = m.Friction * c.NormalImpulse

                If maxFriction > 0.0 Then
                    Call SolveFriction(m, c, maxFriction)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 在两条切线上同时求解摩擦冲量，并把合成冲量裁剪到库仑锥
        ''' <c>|λt| ≤ μ·λn</c> 之内。
        ''' </summary>
        ''' <remarks>
        ''' 两条切线各自独立地裁剪到 μλn（box friction）会让斜向滑动的等效摩擦
        ''' 系数最多偏大 √2 倍，脚掌在斜向蹬地时会被"粘"住，步态因此变得不可控。
        ''' </remarks>
        Private Sub SolveFriction(m As Manifold3D, c As ContactPoint3D, maxFriction As Double)
            Dim rv As Vector3 = m.RelativeVelocity(c.rA, c.rB)
            Dim vt1 As Double = Vector3Math.Dot(rv, c.tangent1)
            Dim vt2 As Double = Vector3Math.Dot(rv, c.tangent2)

            Dim t1 As Double = c.TangentImpulse1 - vt1 * c.tangentMass1
            Dim t2 As Double = c.TangentImpulse2 - vt2 * c.tangentMass2

            ' the cone is an ellipse when impratio <> 1: the second tangent is
            ' scaled before the length is measured, exactly like the reference solver
            ' scales the cone dimension of the constraint
            Dim ratio As Double = If(m.Impratio > 0, m.Impratio, 1.0)
            Dim scaled As Double = t2 / ratio
            Dim length As Double = std.Sqrt(t1 * t1 + scaled * scaled)

            If length > maxFriction Then
                Dim scale As Double = maxFriction / length

                t1 *= scale
                t2 *= scale
            End If

            Dim d1 As Double = t1 - c.TangentImpulse1
            Dim d2 As Double = t2 - c.TangentImpulse2

            c.TangentImpulse1 = t1
            c.TangentImpulse2 = t2

            Call ApplyPair(m, c.tangent1 * d1 + c.tangent2 * d2, c.rA, c.rB)
        End Sub

        ''' <summary>把冲量 P 施加到 B、把 -P 施加到 A。</summary>
        Private Sub ApplyPair(m As Manifold3D, P As Vector3, rA As Vector3, rB As Vector3)
            Call m.A.ApplyImpulse(P * -1.0, rA)
            Call m.B.ApplyImpulse(P, rB)
        End Sub
    End Module
End Namespace
