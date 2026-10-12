''' <summary>
''' 子步钩子：需要在<strong>每个子步</strong>都重新施力的外部控制器实现该接口。
''' </summary>
''' <remarks>
''' <see cref="PhysicsWorld3D.StepSub"/> 会在每个子步的末尾调用
''' <see cref="RigidBody3D.ClearForces"/>，因此在 <c>Step</c> 之前施加一次的力
''' 只会在第一个子步生效，等效强度被稀释成 <c>1/Substeps</c>。
''' 像平衡控制器、步态马达这类"每帧持续施加"的力必须通过本钩子在每个子步补充，
''' 而冲量（<see cref="RigidBody3D.ApplyImpulse"/>）直接改速度，不受影响。
''' </remarks>
Public Interface IStepHook

    ''' <summary>
    ''' 在施加重力之前、马达扭矩之后被调用。
    ''' </summary>
    ''' <param name="dt">当前子步长（秒）。</param>
    Sub BeforeSubstep(dt As Double)
End Interface
