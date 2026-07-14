' Copyright (c) 2018 GPL3 Licensed
' 物理材质：定义表面摩擦系数与恢复系数，碰撞时由两材质组合决定接触行为。

Imports System.Math
Imports std = System.Math

Namespace RigidBody

    ''' <summary>
    ''' 物理材质：摩擦系数 (friction) 与恢复系数 (restitution)。
    ''' 碰撞时由碰撞双方材质组合得到最终的接触参数。
    ''' </summary>
    Public Class PhysicsMaterial

        ''' <summary>静/动摩擦系数（库仑摩擦），0 = 无摩擦，1 = 很强摩擦</summary>
        Public Property Friction As Double = 0.4

        ''' <summary>恢复系数（弹性），0 = 完全非弹性，1 = 完全弹性</summary>
        Public Property Restitution As Double = 0.0

        Sub New(Optional friction As Double = 0.4, Optional restitution As Double = 0.0)
            Me.Friction = friction
            Me.Restitution = restitution
        End Sub

        ''' <summary>两个材质组合的摩擦系数（几何平均，确定性且稳定）</summary>
        Public Shared Function CombineFriction(a As PhysicsMaterial, b As PhysicsMaterial) As Double
            Return std.Sqrt(a.Friction * b.Friction)
        End Function

        ''' <summary>两个材质组合的恢复系数（取较大值）</summary>
        Public Shared Function CombineRestitution(a As PhysicsMaterial, b As PhysicsMaterial) As Double
            Return std.Max(a.Restitution, b.Restitution)
        End Function
    End Class
End Namespace
