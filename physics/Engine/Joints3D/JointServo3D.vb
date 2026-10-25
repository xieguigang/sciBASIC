Imports std = System.Math

Namespace Joints3D

    ''' <summary>关节伺服的工作模式。</summary>
    Public Enum ServoMode3D
        ''' <summary>关闭输出（关节自由，仅受重力与约束）。</summary>
        Disabled = 0
        ''' <summary>位置伺服：外环位置 P + 内环速度 P + 力矩饱和。</summary>
        Position = 1
        ''' <summary>速度伺服：直接把速度误差换算成力/力矩。</summary>
        Velocity = 2
        ''' <summary>力/力矩模式：<see cref="JointServo3D.Target"/> 直接就是广义力（夹爪力控）。</summary>
        Force = 3
    End Enum

    ''' <summary>
    ''' 单自由度关节伺服：对任意 <see cref="IJointDof3D"/>（转轴关节或移动副）输出受限的广义力。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 输出链路是"位置环 → 期望速度 → 速度环 → 力矩"，最后按 <see cref="MaxForce"/> 饱和。
    ''' 因为力矩上限真实存在，所以：
    '''
    ''' * 机械臂在重力下会有稳态误差（位置环刚度不足时下垂），
    ''' * 夹爪的夹持力就是 <see cref="MaxForce"/>，一旦积木质量产生的摩擦需求超过
    '''   <c>μ·N</c>，积木会真实地从指间滑落。
    ''' </para>
    ''' <para>
    ''' 本类型实现 <see cref="IStepHook"/>，由 <see cref="PhysicsWorld3D.Hooks"/> 在每个子步开始
    ''' （重力之前、积分速度之前）自动调用，因此不需要每帧手动施力。
    ''' </para>
    ''' </remarks>
    Public Class JointServo3D
        Implements IStepHook

        ''' <summary>被驱动的自由度。</summary>
        Public ReadOnly Property Dof As IJointDof3D

        ''' <summary>工作模式。</summary>
        Public Property Mode As ServoMode3D = ServoMode3D.Disabled

        ''' <summary>目标值：位置模式为目标位置，速度模式为目标速度，力模式为目标力。</summary>
        Public Property Target As Double

        ''' <summary>位置模式下的最大跟踪速度（rad/s 或 m/s）。</summary>
        Public Property MaxVelocity As Double = 2.0

        ''' <summary>输出上限（N·m 或 N），力模式下就是最大夹持力。</summary>
        Public Property MaxForce As Double = 100.0

        ''' <summary>位置环增益（1/s）。</summary>
        Public Property Stiffness As Double = 25.0

        ''' <summary>速度环增益（N·m/(rad/s) 或 N/(m/s)）。</summary>
        Public Property Damping As Double = 6.0

        ''' <summary>
        ''' 力模式下附加的速度阻尼（N/(m/s) 或 N·m/(rad/s)）。
        ''' </summary>
        ''' <remarks>
        ''' 纯力模式下手指是"恒力推进"，空载时会一路加速到撞上工件，接触瞬间的
        ''' 速度会穿透薄壁。附加一个粘性项可以把空载速度限制在
        ''' <c>Target / ForceDamping</c>，同时不影响最终的接触力。
        ''' </remarks>
        Public Property ForceDamping As Double = 20.0

        ''' <summary>积分增益（抗静差），默认关闭。</summary>
        Public Property IntegralGain As Double = 0.0

        ''' <summary>前馈力（用于抵消重力等已知载荷）。</summary>
        Public Property FeedForward As Double = 0.0

        ''' <summary>使能开关（关闭时保持 <see cref="ServoMode3D.Disabled"/> 行为）。</summary>
        Public Property Enabled As Boolean = True

        ''' <summary>到位判定容差（rad 或 m）。</summary>
        Public Property Tolerance As Double = 0.01

        ''' <summary>名称（供上层显示）。</summary>
        Public Property Label As String

        ''' <summary>上一子步实际输出的广义力。</summary>
        Public Property LastForce As Double
        ''' <summary>上一子步的位置误差。</summary>
        Public Property LastError As Double
        ''' <summary>上一子步的输出是否被 <see cref="MaxForce"/> 截断。</summary>
        Public Property Saturated As Boolean

        Private _integral As Double

        ''' <summary>为指定自由度创建伺服。</summary>
        Sub New(dof As IJointDof3D, Optional mode As ServoMode3D = ServoMode3D.Disabled)
            Me.Dof = dof
            Me.Mode = mode
        End Sub

        ''' <summary>是否已到达目标位置（位置模式）。</summary>
        Public ReadOnly Property IsAtTarget As Boolean
            Get
                If Dof Is Nothing Then Return True

                Return std.Abs(Dof.Position - Target) <= Tolerance
            End Get
        End Property

        ''' <summary>当前自由度坐标（弧度或米）。</summary>
        Public ReadOnly Property CurrentPosition As Double
            Get
                If Dof Is Nothing Then Return 0.0

                Return Dof.Position
            End Get
        End Property

        ''' <summary>当前自由度速度（rad/s 或 m/s）。</summary>
        Public ReadOnly Property CurrentSpeed As Double
            Get
                If Dof Is Nothing Then Return 0.0

                Return Dof.Speed
            End Get
        End Property

        ''' <summary>清空积分项（模式切换或复位时调用）。</summary>
        Public Sub Reset()
            _integral = 0.0
            LastForce = 0.0
            LastError = 0.0
            Saturated = False
        End Sub

        ''' <summary>切换到位置模式并给出目标。</summary>
        Public Sub MoveTo(position As Double)
            Me.Target = position
            Me.Mode = ServoMode3D.Position
        End Sub

        ''' <summary>切换到力模式并给出目标力（夹爪力控）。</summary>
        Public Sub ApplyForce(force As Double)
            Me.Target = force
            Me.Mode = ServoMode3D.Force
        End Sub

        Public Sub BeforeSubstep(dt As Double) Implements IStepHook.BeforeSubstep
            If Dof Is Nothing Then Return
            If Not Enabled OrElse Mode = ServoMode3D.Disabled Then
                LastForce = 0.0
                Return
            End If

            Dim position As Double = Dof.Position
            Dim speed As Double = Dof.Speed
            Dim err As Double = Target - position
            Dim force As Double

            LastError = err

            Select Case Mode
                Case ServoMode3D.Position
                    Dim wantSpeed As Double = Stiffness * err

                    If MaxVelocity > 0.0 Then
                        wantSpeed = std.Max(-MaxVelocity, std.Min(MaxVelocity, wantSpeed))
                    End If

                    force = Damping * (wantSpeed - speed)

                    If IntegralGain > 0.0 Then
                        _integral += err * dt
                        force += IntegralGain * _integral
                    End If

                Case ServoMode3D.Velocity
                    force = Damping * (Target - speed)

                Case ServoMode3D.Force
                    force = Target - ForceDamping * speed

                Case Else
                    Return
            End Select

            force += FeedForward

            Saturated = False

            If MaxForce > 0.0 AndAlso std.Abs(force) > MaxForce Then
                force = std.Max(-MaxForce, std.Min(MaxForce, force))
                Saturated = True

                ' 抗积分饱和：输出被截断时不再累积误差
                If IntegralGain > 0.0 Then
                    _integral -= err * dt
                End If
            End If

            LastForce = force

            Call Dof.ApplyControlForce(force)
        End Sub
    End Class

End Namespace
