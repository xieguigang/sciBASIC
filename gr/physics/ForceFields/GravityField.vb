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
