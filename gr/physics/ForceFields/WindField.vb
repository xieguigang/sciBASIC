' Copyright (c) 2018 GPL3 Licensed
' 风力场：沿方向施加带湍流扰动的力。

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports std = System.Math

Namespace ForceFields

    ''' <summary>
    ''' 风力场：在区域内沿 <see cref="Direction"/> 施加风力，叠加正弦湍流使效果更自然。
    ''' 力大小与刚体质量成正比（类比气压对物体的作用）。
    ''' </summary>
    Public Class WindField : Inherits ForceField

        ''' <summary>风向（单位向量）</summary>
        Public Direction As Vector2

        ''' <summary>风力强度</summary>
        Public Strength As Double

        ''' <summary>湍流强度（0 = 恒定风）</summary>
        Public Turbulence As Double = 0.0

        Private phase As Double = 0.0

        Sub New(dir As Vector2, strength As Double, Optional turbulence As Double = 0.0)
            Me.Direction = Normalize(dir)
            Me.Strength = strength
            Me.Turbulence = turbulence
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody))
            phase += 0.1

            For Each b In bodies
                If b.IsStatic Then Continue For
                If Not InRegion(b) Then Continue For

                Dim scale = Strength * (1.0 + Turbulence * 0.5 * std.Sin(phase + b.Position.x * 0.01))
                b.ApplyForce(Direction * (scale * b.Mass))
            Next
        End Sub
    End Class
End Namespace
