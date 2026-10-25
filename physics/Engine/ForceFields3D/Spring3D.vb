#Region "Microsoft.VisualBasic::spring3d, physics\Engine\ForceFields3D\Spring3D.vb"
' 
'                                                                                         
'     ircpp(SciBASIC#) is a computer algebra and scientific computing library                
'     for numerical analysis, data visualization and GPU programming.                          
'                                                                                             
'                                                                                             

Imports Microsoft.VisualBasic.Engine.Physics.Math3D

Namespace ForceFields3D

    ''' <summary>
    ''' 三维压簧：一端固定在世界锚点、另一端挂在刚体的某个局部点上。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 引擎原本只有 2D 的 <c>Joints\SpringJoint</c>，3D 侧没有弹簧。这里以力场的形式实现：
    ''' 每步算出挂点的世界坐标与速度，按 <c>F = −k·(L − L0)·d − c·(v·d)·d</c> 施加力，
    ''' 再通过 <see cref="RigidBody3D.ApplyForceAtPoint"/> 同时产生作用在刚体质心上的力与扭矩。
    ''' </para>
    ''' <para>
    ''' 力场在积分速度之前被应用（<c>PhysicsWorld3D.StepSub</c> 的顺序是 重力 → 力场 → 积分），
    ''' 所以弹簧力会正确地进入本步的积分。
    ''' </para>
    ''' <para>
    ''' <b>数值稳定性</b>：这是显式力，刚度过大或阻尼过小都会发散。经验取值
    ''' <c>k ≤ 6000 N/m</c>、<c>c ≥ 40 N·s/m</c>，并用 <see cref="MaxForce"/> 限幅。
    ''' 判据：<c>k·dt²/m ≪ 1</c>（dt 是子步长，本项目约 1 ms）。
    ''' </para>
    ''' </remarks>
    Public Class Spring3D
        Inherits ForceField3D

        ''' <summary>固定端（世界坐标），通常落在地面或某个静态物体上。</summary>
        Public Property Anchor As Vector3

        ''' <summary>被弹簧拉住的刚体。</summary>
        Public Property Body As RigidBody3D

        ''' <summary>弹簧在刚体上的挂点（刚体局部坐标，相对质心）。</summary>
        Public Property LocalPoint As Vector3

        ''' <summary>刚度 k（N/m）。</summary>
        Public Property Stiffness As Double = 3000.0

        ''' <summary>阻尼 c（N·s/m）。</summary>
        Public Property Damping As Double = 60.0

        ''' <summary>自然长度 L0（米）。</summary>
        Public Property RestLength As Double = 0.1

        ''' <summary>
        ''' 横向（剪切）刚度 k_s（N/m）。
        ''' </summary>
        ''' <remarks>
        ''' 只沿轴向施力的理想压簧**不提供任何横向约束**：四角弹簧撑着的木板在水平方向是自由漂浮的，
        ''' 一点点横向扰动就会把整块板"推走"（实测被重块撞一下横向漂了 13 cm）。
        ''' 真实压簧有明显的剪切刚度，这里用一根把挂点拉回锚点轴线的弹簧来近似。
        ''' </remarks>
        Public Property ShearStiffness As Double = 0.0

        ''' <summary>横向阻尼 c_s（N·s/m）。</summary>
        Public Property ShearDamping As Double = 0.0

        ''' <summary>
        ''' 只压不拉：为 True 时弹簧只能把两端推开（螺旋压簧的近似行为），
        ''' 拉伸状态不产生拉力。
        ''' </summary>
        Public Property CompressionOnly As Boolean = True

        ''' <summary>力的限幅（N），防止显式积分在大穿透时炸掉。</summary>
        Public Property MaxForce As Double = 600.0

        ''' <summary>弹簧的标签（调试用）。</summary>
        Public Property Label As String = "spring"

        ''' <summary><see cref="Apply"/> 被调用的次数（诊断用）。</summary>
        Public Property ApplyCount As Long

        ''' <summary>最近一次 <see cref="Apply"/> 算出的弹簧长度（米）。</summary>
        Public Property CurrentLength As Double

        ''' <summary>最近一次的压缩量（米，正值表示被压短）。</summary>
        Public Property Compression As Double

        ''' <summary>最近一次施加的力（N，沿 <see cref="CurrentDirection"/>）。</summary>
        Public Property CurrentForce As Double

        ''' <summary>最近一次的轴向单位向量（由锚点指向挂点）。</summary>
        Public Property CurrentDirection As Vector3 = New Vector3(0, 0, 1)

        ''' <summary>挂点的世界坐标。</summary>
        Public ReadOnly Property AttachPoint As Vector3
            Get
                If Body Is Nothing Then Return Anchor

                Return Body.Position + Body.Orientation.Rotate(LocalPoint)
            End Get
        End Property

        ''' <summary>弹簧当前的高度增量（渲染螺旋线时用）。</summary>
        Public ReadOnly Property LengthRatio As Double
            Get
                If RestLength <= 0.0 Then Return 1.0

                Return CurrentLength / RestLength
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub New(anchor As Vector3, body As RigidBody3D, localPoint As Vector3,
                       Optional stiffness As Double = 3000.0,
                       Optional damping As Double = 60.0,
                       Optional restLength As Double = 0.1)

            Me.Anchor = anchor
            Me.Body = body
            Me.LocalPoint = localPoint
            Me.Stiffness = stiffness
            Me.Damping = damping
            Me.RestLength = restLength
        End Sub

        Public Overrides Sub Apply(bodies As System.Collections.Generic.IEnumerable(Of RigidBody3D))
            ApplyCount += 1

            If Body Is Nothing Then Return
            If Body.IsStatic Then Return

            ' 弹簧撑着的物体不该被判定为"静止"：一旦睡眠，弹簧就不再施力，
            ' 整块木板会直接塌到地上并且再也起不来。这里强制唤醒它。
            If Body.IsSleeping Then Call Body.Wake()

            If Stiffness <= 0.0 AndAlso Damping <= 0.0 Then Return

            Dim point As Vector3 = Body.Position + Body.Orientation.Rotate(LocalPoint)
            Dim delta As Vector3 = point - Anchor
            Dim length As Double = Vector3Math.Length(delta)

            If length < 1.0E-9 Then Return

            Dim dir As Vector3 = delta / length
            Dim arm As Vector3 = point - Body.Position
            Dim pointVelocity As Vector3 = Body.Velocity + Vector3Math.Cross(Body.AngularVelocity, arm)
            Dim along As Double = Vector3Math.Dot(pointVelocity, dir)
            Dim extension As Double = length - RestLength

            Dim magnitude As Double = -Stiffness * extension - Damping * along

            If CompressionOnly AndAlso magnitude < 0.0 Then magnitude = 0.0

            If MaxForce > 0.0 Then
                magnitude = System.Math.Max(-MaxForce, System.Math.Min(MaxForce, magnitude))
            End If

            CurrentLength = length
            CurrentDirection = dir
            CurrentForce = magnitude
            Compression = RestLength - length

            Dim force As Vector3 = dir * magnitude

            ' 横向（剪切）约束：把挂点拉回锚点所在的轴线
            If ShearStiffness > 0.0 OrElse ShearDamping > 0.0 Then
                Dim lateral As Vector3 = delta - dir * Vector3Math.Dot(delta, dir)
                Dim lateralVelocity As Vector3 = pointVelocity - dir * along

                force = force - lateral * ShearStiffness - lateralVelocity * ShearDamping
            End If

            If MaxForce > 0.0 Then
                Dim total As Double = Vector3Math.Length(force)

                If total > MaxForce Then force = force * (MaxForce / total)
            End If

            If Vector3Math.Length(force) < 1.0E-9 Then Return

            Call Body.ApplyForceAtPoint(point, force)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Label}: L={CurrentLength:F4} (L0={RestLength:F4})  F={CurrentForce:F1} N"
        End Function

    End Class

End Namespace
#End Region
