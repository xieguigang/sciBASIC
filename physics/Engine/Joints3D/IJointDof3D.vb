Imports Microsoft.VisualBasic.Engine.Physics.Math3D

Namespace Joints3D

    ''' <summary>
    ''' 单自由度关节（转轴关节 / 移动副）的统一抽象。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="JointServo3D"/> 只依赖这个接口，所以同一个伺服控制器既能驱动
    ''' 机械臂的转动关节，也能驱动夹爪手指的移动副：
    '''
    ''' * 转动关节：<see cref="Position"/> 是弧度，<see cref="ApplyControlForce(Double)"/>
    '''   的参数是扭矩（N·m），<see cref="MaxVelocity"/> 是 rad/s。
    ''' * 移动副：<see cref="Position"/> 是米，<see cref="ApplyControlForce(Double)"/>
    '''   的参数是力（N），<see cref="MaxVelocity"/> 是 m/s。
    '''
    ''' 所有量都是 "B 相对 A" 的约定，轴方向由 <see cref="WorldAxis"/> 给出。
    ''' </remarks>
    Public Interface IJointDof3D

        ''' <summary>参考刚体（通常是上游连杆或基座）。</summary>
        ReadOnly Property A As RigidBody3D

        ''' <summary>被约束刚体（通常是下游连杆）。</summary>
        ReadOnly Property B As RigidBody3D

        ''' <summary>世界坐标系下的单位运动轴，指向自由度的正方向。</summary>
        ReadOnly Property WorldAxis As Vector3

        ''' <summary>
        ''' 当前自由度坐标：转动关节是相对零位的角度（弧度），移动副是相对零位的位移（米）。
        ''' </summary>
        ReadOnly Property Position As Double

        ''' <summary>当前自由度速度：角速度（rad/s）或线速度（m/s），以 B 相对 A 计。</summary>
        ReadOnly Property Speed As Double

        ''' <summary>自由度下限（弧度或米）。</summary>
        ReadOnly Property LowerLimit As Double

        ''' <summary>自由度上限（弧度或米）。</summary>
        ReadOnly Property UpperLimit As Double

        ''' <summary>沿正方向施加一个广义力：转动关节为扭矩，移动副为力。</summary>
        Sub ApplyControlForce(value As Double)

        ''' <summary>把当前位姿记为自由度的零位。</summary>
        Sub SetZeroReference()
    End Interface

End Namespace
