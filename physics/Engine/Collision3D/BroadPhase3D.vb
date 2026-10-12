Namespace Collision3D

    ''' <summary>宽相位输出的候选刚体对。</summary>
    Public Structure BodyPair3D

        ''' <summary>候选对的第一个刚体。</summary>
        Public A As RigidBody3D
        ''' <summary>候选对的第二个刚体。</summary>
        Public B As RigidBody3D

        Sub New(a As RigidBody3D, b As RigidBody3D)
            Me.A = a
            Me.B = b
        End Sub
    End Structure

    ''' <summary>
    ''' 3D 宽相位：AABB 重叠剔除 + 暴力配对。
    ''' </summary>
    ''' <remarks>
    ''' 本库面向的规模（几十个刚体）远小于空间哈希的收益拐点，因此采用
    ''' 与 2D <see cref="Collision.BroadPhase"/> 相同的均匀网格思路的简化版：
    ''' 直接用 O(n²) 的 AABB 相交测试，并跳过静态-静态对与碰撞分组不匹配的对。
    ''' </remarks>
    Public Module BroadPhase3D

        ''' <summary>AABB 的"皮肤"厚度，避免贴着表面的物体反复进出宽相位。</summary>
        Public Property Margin As Double = 0.05

        ''' <summary>求所有需要进入窄相位的候选对。</summary>
        Public Function ComputePairs(bodies As List(Of RigidBody3D)) As List(Of BodyPair3D)
            Dim pairs As New List(Of BodyPair3D)()

            If bodies Is Nothing Then
                Return pairs
            End If

            Dim count As Integer = bodies.Count
            Dim boxes(count - 1) As AABB3D

            For i As Integer = 0 To count - 1
                boxes(i) = bodies(i).GetAABB().Expand(Margin)
            Next

            For i As Integer = 0 To count - 2
                Dim a As RigidBody3D = bodies(i)

                For j As Integer = i + 1 To count - 1
                    Dim b As RigidBody3D = bodies(j)

                    ' 两个静态物体之间不可能产生接触
                    If a.IsStatic AndAlso b.IsStatic Then
                        Continue For
                    End If

                    ' 碰撞分组：双方都必须允许对方的分组
                    If (a.CollisionMask And b.CollisionGroup) = 0 Then
                        Continue For
                    End If
                    If (b.CollisionMask And a.CollisionGroup) = 0 Then
                        Continue For
                    End If

                    If boxes(i).Overlaps(boxes(j)) Then
                        pairs.Add(New BodyPair3D(a, b))
                    End If
                Next
            Next

            Return pairs
        End Function
    End Module
End Namespace
