#Region "Microsoft.VisualBasic::c5d1e3ec7307df4e88d493bad231bc3f, gr\physics\Collision\ContactSolver.vb"

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

    '   Total Lines: 84
    '    Code Lines: 51 (60.71%)
    ' Comment Lines: 14 (16.67%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 19 (22.62%)
    '     File Size: 3.50 KB


    '     Module ContactSolver
    ' 
    '         Sub: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 接触求解器：顺序冲量法，处理恢复系数(弹性)、库仑摩擦、Baumgarte 位置修正。

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports std = System.Math

Namespace Collision

    ''' <summary>
    ''' 顺序冲量接触求解器。对每个接触点迭代计算法向冲量（含恢复系数与 Baumgarte
    ''' 穿透修正）和切向冲量（库仑摩擦），并将冲量应用到两个刚体。
    ''' </summary>
    Public Module ContactSolver

        ''' <summary>Baumgarte 稳定系数（穿透修正强度）</summary>
        Public Const Baumgarte As Double = 0.2

        ''' <summary>允许的穿透余量，避免抖动</summary>
        Public Const Slop As Double = 0.05

        ''' <summary>低于此相对法向速度时不施加恢复（避免静止抖动）</summary>
        Public Const RestitutionThreshold As Double = 1.0

        ''' <summary>求解单个流形的所有接触点（应在每次速度迭代中调用）</summary>
        Public Sub Solve(m As Manifold, dt As Double)
            Dim A = m.A, B = m.B
            Dim n = m.Normal

            For c = 0 To m.Contacts.Length - 1
                Dim contact = m.Contacts(c)
                Dim rA = contact - A.Position
                Dim rB = contact - B.Position

                ' 接触点相对速度
                Dim rv = B.Velocity + Cross(B.AngularVelocity, rB) - A.Velocity - Cross(A.AngularVelocity, rA)
                Dim vn = Dot(rv, n)

                Dim rnA = Cross(rA, n), rnB = Cross(rB, n)
                Dim kNormal = A.InvMass + B.InvMass + A.InvInertia * rnA * rnA + B.InvInertia * rnB * rnB
                If kNormal < 1.0e-12 Then Continue For

                ' 恢复系数（仅在明显接近时生效）
                Dim e = m.Restitution
                If std.Abs(vn) < RestitutionThreshold Then e = 0

                ' Baumgarte 位置修正作为一项偏置冲量
                Dim bias = (Baumgarte / dt) * std.Max(0, m.Penetration - Slop)

                Dim jn = -(1.0 + e) * vn / kNormal
                jn += bias / kNormal

                Dim oldJn = m.jnAcc(c)
                m.jnAcc(c) = std.Max(oldJn + jn, 0.0)
                jn = m.jnAcc(c) - oldJn

                Dim P = n * jn
                A.ApplyImpulse(-P, rA)
                B.ApplyImpulse(P, rB)

                ' ---- 库仑摩擦（切向） ----
                rv = B.Velocity + Cross(B.AngularVelocity, rB) - A.Velocity - Cross(A.AngularVelocity, rA)
                Dim tangent = rv - n * Dot(rv, n)
                Dim tlen = Length(tangent)
                If tlen < 1.0e-9 Then Continue For
                tangent = tangent / tlen

                Dim rtA = Cross(rA, tangent), rtB = Cross(rB, tangent)
                Dim kTangent = A.InvMass + B.InvMass + A.InvInertia * rtA * rtA + B.InvInertia * rtB * rtB
                If kTangent < 1.0e-12 Then Continue For

                Dim jt = -Dot(rv, tangent) / kTangent
                Dim maxJt = m.Friction * m.jnAcc(c)
                Dim oldJt = m.jtAcc(c)
                m.jtAcc(c) = std.Max(-maxJt, std.Min(maxJt, oldJt + jt))
                jt = m.jtAcc(c) - oldJt

                Dim Pf = tangent * jt
                A.ApplyImpulse(-Pf, rA)
                B.ApplyImpulse(Pf, rB)
            Next
        End Sub
    End Module
End Namespace

