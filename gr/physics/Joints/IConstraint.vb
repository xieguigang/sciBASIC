' Copyright (c) 2018 GPL3 Licensed
' 约束/关节统一接口。

Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody

Namespace Microsoft.VisualBasic.Imaging.Physics.Joints

    ''' <summary>
    ''' 约束（关节）统一接口。在物理世界的速度迭代阶段调用 <see cref="SolveVelocity"/>，
    ''' 在位置阶段调用 <see cref="SolvePosition"/> 做穿透/漂移修正。
    ''' </summary>
    Public Interface IConstraint
        ''' <summary>在速度求解迭代中施加冲量</summary>
        Sub SolveVelocity(dt As Double)

        ''' <summary>位置修正（消除约束漂移）</summary>
        Sub SolvePosition(dt As Double)
    End Interface
End Namespace
