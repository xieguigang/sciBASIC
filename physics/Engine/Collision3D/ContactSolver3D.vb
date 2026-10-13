#Region "Microsoft.VisualBasic::74b11c6d98beb098680b3af9f40a669e, physics\Engine\Collision3D\ContactSolver3D.vb"

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
                Dim deltaVn As Double = c.restitutionBias + c.bias - vn
                Dim jn As Double = deltaVn * c.normalMass

                ' 法向冲量只能"推"不能"拉"
                Dim oldNormal As Double = c.NormalImpulse
                c.NormalImpulse = std.Max(oldNormal + jn, 0.0)
                jn = c.NormalImpulse - oldNormal

                Dim Pn As Vector3 = n * jn
                Call ApplyPair(m, Pn, c.rA, c.rB)

                ' ---------- 摩擦（两条正交切线） ----------
                Dim maxFriction As Double = m.Friction * c.NormalImpulse

                If maxFriction > 0.0 Then
                    Call SolveFriction(m, c, c.tangent1, c.tangentMass1, maxFriction, c.TangentImpulse1)
                    Call SolveFriction(m, c, c.tangent2, c.tangentMass2, maxFriction, c.TangentImpulse2)
                End If
            Next
        End Sub

        ''' <summary>沿单条切线求解摩擦冲量，并按库仑锥裁剪。</summary>
        Private Sub SolveFriction(m As Manifold3D, c As ContactPoint3D,
                                  tangent As Vector3, tangentMass As Double,
                                  maxFriction As Double,
                                  ByRef accumulated As Double)
            Dim rv As Vector3 = m.RelativeVelocity(c.rA, c.rB)
            Dim vt As Double = Vector3Math.Dot(rv, tangent)
            Dim jt As Double = -vt * tangentMass
            Dim old As Double = accumulated
            Dim clamped As Double = std.Min(std.Max(old + jt, -maxFriction), maxFriction)

            accumulated = clamped
            jt = clamped - old

            Call ApplyPair(m, tangent * jt, c.rA, c.rB)
        End Sub

        ''' <summary>把冲量 P 施加到 B、把 -P 施加到 A。</summary>
        Private Sub ApplyPair(m As Manifold3D, P As Vector3, rA As Vector3, rB As Vector3)
            Call m.A.ApplyImpulse(P * -1.0, rA)
            Call m.B.ApplyImpulse(P, rB)
        End Sub
    End Module
End Namespace

