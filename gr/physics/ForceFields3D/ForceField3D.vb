Imports Microsoft.VisualBasic.Imaging.Physics.Collision3D

Namespace ForceFields3D

    ''' <summary>
    ''' 3D 力场抽象基类，与 2D 的 <see cref="ForceFields.ForceField"/> 同构：
    ''' 场可以只在某个 <see cref="Region"/> 内生效，也可以作用于整个世界。
    ''' </summary>
    Public MustInherit Class ForceField3D

        ''' <summary>
        ''' 作用区域；<c>Nothing</c> 表示对整个世界生效。
        ''' </summary>
        Public Property Region As AABB3D? = Nothing

        ''' <summary>把场的作用力累加到各刚体。</summary>
        Public MustOverride Sub Apply(bodies As IEnumerable(Of RigidBody3D))

        ''' <summary>刚体是否位于本场的作用区域内。</summary>
        Protected Function InRegion(b As RigidBody3D) As Boolean
            If Region Is Nothing Then
                Return True
            End If

            Return Region.Value.Overlaps(b.GetAABB())
        End Function
    End Class
End Namespace
