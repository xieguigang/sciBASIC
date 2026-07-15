#Region "Microsoft.VisualBasic::bbd87c868892e265fe01566f3d9191b9, gr\physics\Collision\Manifold.vb"

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

    '   Total Lines: 44
    '    Code Lines: 21 (47.73%)
    ' Comment Lines: 13 (29.55%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.56 KB


    '     Class Manifold
    ' 
    '         Sub: InitSolver
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 碰撞流形：承载法向、穿透深度、接触点，以及顺序冲量求解所需的累加器。

Imports Microsoft.VisualBasic.Imaging.Physics

Namespace Collision

    ''' <summary>
    ''' 碰撞流形（接触信息）。法向量 <see cref="Normal"/> 由 A 指向 B。
    ''' 接触点最多 2 个（世界坐标），用于顺序冲量求解。
    ''' </summary>
    Public Class Manifold
        Public A As RigidBody
        Public B As RigidBody

        ''' <summary>由 A 指向 B 的单位法向量</summary>
        Public Normal As Vector2

        ''' <summary>穿透深度（正数表示重叠）</summary>
        Public Penetration As Double

        ''' <summary>世界坐标接触点（1 或 2 个）</summary>
        Public Contacts As Vector2()

        ''' <summary>组合后的恢复系数</summary>
        Public Restitution As Double

        ''' <summary>组合后的摩擦系数</summary>
        Public Friction As Double

        ' ---- 求解器工作数据（每个接触点一份） ----
        Public jnAcc As Double()
        Public jtAcc As Double()
        Public bias As Double()

        ''' <summary>初始化冲量累加器（每步、每个接触点清零）</summary>
        Public Sub InitSolver()
            Dim n = If(Contacts Is Nothing, 0, Contacts.Length)
            jnAcc = New Double(n - 1) {}
            jtAcc = New Double(n - 1) {}
            bias = New Double(n - 1) {}
        End Sub
    End Class
End Namespace

