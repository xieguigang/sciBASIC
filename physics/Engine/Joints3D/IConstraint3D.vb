Namespace Joints3D

    ''' <summary>
    ''' 3D 约束（关节）统一接口，与 2D 的 <see cref="Joints.IConstraint"/> 完全同构。
    ''' </summary>
    Public Interface IConstraint3D

        ''' <summary>速度层求解：在不改变位置的前提下消除违反约束的相对速度。</summary>
        Sub SolveVelocity(dt As Double)

        ''' <summary>位置层求解（软修正）：直接微调位置/姿态以消除累积漂移。</summary>
        Sub SolvePosition(dt As Double)
    End Interface
End Namespace
