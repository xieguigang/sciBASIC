#Region "Microsoft.VisualBasic::springjoint3d, physics\Engine\Joints3D\SpringJoint3D.vb"
' 
'                                                                                         
'     ircpp(SciBASIC#) is a computer algebra and scientific computing library                
'     for numerical analysis, data visualization and GPU programming.                          
'                                                                                             
'                                                                                             

Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 约束式弹簧关节：把刚体上的一点与"另一个刚体（或世界锚点）"用弹簧连起来。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <c>ForceFields3D.Spring3D</c>（每步施加 <c>F = −k·x − c·v</c> 的显式力）不同，
    ''' 这里在**速度层**用柔度（compliance）做软约束求解：
    ''' </para>
    ''' <code>
    ''' gamma = 1 / (dt·(c + dt·k))
    ''' bias  = C·dt·k·gamma            ' C = 当前长度 − 自然长度
    ''' m_eff = 1 / (有效质量项 + gamma)
    ''' λ     = −m_eff·(Ċ + bias)
    ''' </code>
    ''' <para>
    ''' <c>gamma</c> 让 <c>k</c> 很大时有效质量也不再趋近 0，所以刚度可以取到几千 N/m
    ''' 而不会像显式力那样发散（实测显式力在 <c>c=300</c> 时会把木板弹到 6 m 之外）。
    ''' 同时它天然是"双向"的：压缩时推开、拉伸时拉回。
    ''' </para>
    ''' <para>
    ''' 横向（剪切）方向用一个带死区的软约束：挂点相对锚点的横向错位超过
    ''' <see cref="ShearSlop"/> 才开始产生恢复力，避免把木板"锁平"（横向刚度会抵消
    ''' 负载力矩、把倾斜角压掉一个数量级）。
    ''' </para>
    ''' </remarks>
    Public Class SpringJoint3D
        Implements IConstraint3D

        ''' <summary>一端刚体；<c>Nothing</c> 表示这一端固定在世界上（<see cref="AnchorA"/> 即世界坐标）。</summary>
        Public Property A As RigidBody3D

        ''' <summary>另一端刚体。</summary>
        Public Property B As RigidBody3D

        ''' <summary>A 上的局部锚点；<see cref="A"/> 为 <c>Nothing</c> 时直接是世界坐标。</summary>
        Public Property AnchorA As Vector3

        ''' <summary>B 上的局部锚点（相对质心）。</summary>
        Public Property AnchorB As Vector3

        ''' <summary>自然长度（米）。</summary>
        Public Property RestLength As Double = 0.1

        ''' <summary>轴向刚度 k（N/m）。</summary>
        Public Property Stiffness As Double = 300.0

        ''' <summary>轴向阻尼 c（N·s/m）。</summary>
        Public Property Damping As Double = 30.0

        ''' <summary>横向（剪切）刚度（N/m）。为 0 表示不加横向约束。</summary>
        Public Property ShearStiffness As Double = 0.0

        ''' <summary>横向阻尼（N·s/m）。</summary>
        Public Property ShearDamping As Double = 0.0

        ''' <summary>横向死区（米）：小于这个错位不产生恢复力。</summary>
        Public Property ShearSlop As Double = 0.001

        ''' <summary>双向：False 时只在压缩状态（长度小于自然长度）产生推力。</summary>
        Public Property Bidirectional As Boolean = True

        ''' <summary>单个方向上的冲量限幅换算成的力上限（N），仅作数值兜底。</summary>
        Public Property MaxForce As Double = 4000.0

        ''' <summary>是否参与求解。</summary>
        Public Property Enabled As Boolean = True

        ''' <summary>便于外部按名字索引的标签。</summary>
        Public Property Label As String

        ' ---- 诊断量（每次 SolveVelocity 后更新）----

        ''' <summary>最近一次求解时的实际长度（米）。</summary>
        Public Property CurrentLength As Double

        ''' <summary>压缩量（米）：自然长度 − 实际长度，负值表示被拉伸。</summary>
        Public Property Compression As Double

        ''' <summary>最近一次求解施加的轴向力（N，正值表示推开两端）。</summary>
        Public Property CurrentForce As Double

        ''' <summary>最近一次求解时的轴向单位向量（由 A 指向 B）。</summary>
        Public Property Direction As Vector3 = New Vector3(0, 0, 1)

        ''' <summary><see cref="SolveVelocity"/> 被调用的次数（诊断用）。</summary>
        Public Property SolveCount As Long

        ''' <summary>最近一次轴向求解的有效质量（诊断用）。</summary>
        Public Property LastMass As Double

        ''' <summary>最近一次轴向求解的 bias 项（诊断用）。</summary>
        Public Property LastBias As Double

        ''' <summary>最近一次轴向求解的冲量（诊断用）。</summary>
        Public Property LastImpulse As Double

        ''' <summary>最近一次轴向求解的柔度项 gamma（诊断用）。</summary>
        Public Property LastGamma As Double

        ''' <summary>A 端的世界坐标。</summary>
        Public ReadOnly Property WorldAnchorA As Vector3
            Get
                If A Is Nothing Then Return AnchorA

                Return A.Position + A.Orientation.Rotate(AnchorA)
            End Get
        End Property

        ''' <summary>B 端的世界坐标。</summary>
        Public ReadOnly Property WorldAnchorB As Vector3
            Get
                If B Is Nothing Then Return AnchorB

                Return B.Position + B.Orientation.Rotate(AnchorB)
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub New(a As RigidBody3D, b As RigidBody3D, anchorA As Vector3, anchorB As Vector3,
                       Optional restLength As Double = 0.1,
                       Optional stiffness As Double = 300.0,
                       Optional damping As Double = 30.0)

            Me.A = a
            Me.B = b
            Me.AnchorA = anchorA
            Me.AnchorB = anchorB
            Me.RestLength = restLength
            Me.Stiffness = stiffness
            Me.Damping = damping
        End Sub

        ' /********************************************************************************/
        '  约束求解
        ' /********************************************************************************/

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            SolveCount += 1

            If Not Enabled Then Return
            If B Is Nothing OrElse B.IsStatic Then Return
            If dt <= 0.0 Then Return

            ' 弹簧撑着的物体不能被判成“静止”：一旦睡眠，约束就不再求解，
            ' 整块板会直接塌到挡块上再也起不来（实测空载就直接压到底）
            If B.IsSleeping Then Call B.Wake()
            If A IsNot Nothing AndAlso A.IsSleeping Then Call A.Wake()

            Dim pA As Vector3 = WorldAnchorA
            Dim pB As Vector3 = WorldAnchorB
            Dim delta As Vector3 = pB - pA
            Dim length As Double = Vector3Math.Length(delta)

            If length < 1.0E-9 Then Return

            Dim axis As Vector3 = delta / length
            Dim rA As Vector3 = If(A Is Nothing, Vector3.Zero, pA - A.Position)
            Dim rB As Vector3 = pB - B.Position
            Dim invIA As New Matrix3x3()

            If A IsNot Nothing Then invIA = A.InvInertiaWorld()

            Dim invIB As Matrix3x3 = B.InvInertiaWorld()
            Dim vA As Vector3 = If(A Is Nothing, Vector3.Zero, A.Velocity + Vector3Math.Cross(A.AngularVelocity, rA))
            Dim vB As Vector3 = B.Velocity + Vector3Math.Cross(B.AngularVelocity, rB)
            Dim vRel As Vector3 = vB - vA
            Dim errorValue As Double = length - RestLength

            CurrentLength = length
            Direction = axis
            Compression = RestLength - length

            Dim load As Double = 0.0
            Dim applied As Double

            ' ---- 轴向：压缩时推开、拉伸时拉回（Bidirectional=False 时只在压缩状态出力）----
            If Bidirectional OrElse errorValue < 0.0 Then
                applied = SolveAxis(axis, rA, rB, invIA, invIB, Vector3Math.Dot(vRel, axis),
                                    errorValue, dt, Stiffness, Damping)

                load += applied
            End If

            ' ---- 横向：带死区的软约束，只把"超出死区的错位"拉回去 ----
            '
            ' 这里的横向参照必须是**世界水平面**，不能取"垂直于当前弹簧轴线"的分量：
            ' 木板一旦水平漂移，整根弹簧会跟着倾斜，此时"垂直于轴线"的偏移又变回 0，
            ' 横向约束就再也拉不回来了（实测木板漂了 8.6 cm 而约束毫无反应）。
            If ShearStiffness > 0.0 OrElse ShearDamping > 0.0 Then
                Dim lateral As New Vector3(delta.x, delta.y, 0.0)
                Dim lateralLength As Double = Vector3Math.Length(lateral)

                If lateralLength > ShearSlop Then
                    Dim t As Vector3 = lateral / lateralLength
                    Dim excess As Double = lateralLength - ShearSlop

                    applied = SolveAxis(t, rA, rB, invIA, invIB, Vector3Math.Dot(vRel, t),
                                        excess, dt, ShearStiffness, ShearDamping)

                    load += applied
                End If
            End If

            CurrentForce = load
        End Sub

        ''' <summary>
        ''' 沿一个方向求解软约束，返回等效作用力（N）。
        ''' </summary>
        ''' <param name="axis">约束方向（单位向量，由 A 指向 B）</param>
        ''' <param name="errorValue">该方向上的位移误差（正值表示 B 偏离过远）</param>
        Private Function SolveAxis(axis As Vector3, rA As Vector3, rB As Vector3,
                                   invIA As Matrix3x3, invIB As Matrix3x3,
                                   vn As Double, errorValue As Double, dt As Double,
                                   stiffness As Double, damping As Double) As Double

            Dim invMassA As Double = If(A Is Nothing, 0.0, A.InvMass)
            Dim rnA As Vector3 = Vector3Math.Cross(rA, axis)
            Dim rnB As Vector3 = Vector3Math.Cross(rB, axis)
            Dim k As Double = invMassA + B.InvMass

            If A IsNot Nothing Then
                k += Vector3Math.Dot(rnA, invIA.MultiplyLeft(rnA))
            End If

            k += Vector3Math.Dot(rnB, invIB.MultiplyLeft(rnB))

            ' 柔度项：刚度再大也不会让有效质量趋近 0（显式力在大刚度时会发散）
            Dim gamma As Double = 1.0 / (dt * (damping + dt * stiffness))
            Dim mass As Double = k + gamma

            If mass < 1.0E-12 Then Return 0.0

            Dim bias As Double = errorValue * dt * stiffness * gamma
            Dim lambda As Double = -(vn + bias) / mass

            LastMass = mass
            LastBias = bias
            LastImpulse = lambda
            LastGamma = gamma

            If MaxForce > 0.0 Then
                Dim maxImpulse As Double = MaxForce * dt

                lambda = std.Max(-maxImpulse, std.Min(maxImpulse, lambda))
            End If

            Dim impulse As Vector3 = axis * lambda

            If A IsNot Nothing Then Call A.ApplyImpulse(impulse * -1.0, rA)
            Call B.ApplyImpulse(impulse, rB)

            Return std.Abs(lambda) / dt
        End Function

        ''' <summary>
        ''' 位置层不做修正：软约束的 bias 已经会把长度误差收敛掉，
        ''' 再做"传送式"修正会让木板抖动（实测 30% 修正就会让静止的板持续小幅跳动）。
        ''' </summary>
        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            ' 无操作
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Label}: L={CurrentLength:F4} (L0={RestLength:F4})  F={CurrentForce:F1} N"
        End Function

    End Class

End Namespace
#End Region
