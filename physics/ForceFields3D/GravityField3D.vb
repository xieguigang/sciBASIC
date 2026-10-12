Imports Microsoft.VisualBasic.Imaging.Physics.Math3D

Namespace ForceFields3D

    ''' <summary>
    ''' 3D 重力场：对区域内每个非静态刚体施加 <c>F = m · g</c>。
    ''' </summary>
    Public Class GravityField3D : Inherits ForceField3D

        ''' <summary>重力加速度向量（默认 -9.81 m/s²，Y 轴向上）。</summary>
        Public Property Gravity As Vector3

        Sub New(Optional g As Vector3 = Nothing)
            Me.Gravity = If(g Is Nothing, New Vector3(0, -9.81, 0), g)
        End Sub

        Public Overrides Sub Apply(bodies As IEnumerable(Of RigidBody3D))
            For Each b In bodies
                If b.IsStatic Then Continue For
                If Not InRegion(b) Then Continue For

                Call b.ApplyForce(Gravity * b.Mass)
            Next
        End Sub
    End Class
End Namespace
