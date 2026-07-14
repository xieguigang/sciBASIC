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
