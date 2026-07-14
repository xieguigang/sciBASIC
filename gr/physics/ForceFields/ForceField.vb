' Copyright (c) 2018 GPL3 Licensed
' 力场基类：在其作用区域内对所有刚体施加力。

Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody

Namespace Microsoft.VisualBasic.Imaging.Physics.ForceFields

    ''' <summary>
    ''' 力场基类。覆盖一个区域（<see cref="Region"/>，为 Nothing 时表示全局），
    ''' 每步对场内刚体施加力。派生类实现 <see cref="Apply"/>。
    ''' </summary>
    Public MustInherit Class ForceField

        ''' <summary>作用区域；为 Nothing 时作用于世界内所有刚体</summary>
        Public Region As AABB?

        ''' <summary>对给定刚体集合施加力</summary>
        Public MustOverride Sub Apply(bodies As IEnumerable(Of RigidBody))

        ''' <summary>刚体是否处于力场作用区域内</summary>
        Protected Function InRegion(b As RigidBody) As Boolean
            If Not Region.HasValue Then Return True
            Return Region.Value.Overlaps(b.GetAABB())
        End Function
    End Class
End Namespace
