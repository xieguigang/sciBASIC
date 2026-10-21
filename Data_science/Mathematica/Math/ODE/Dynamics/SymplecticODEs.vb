Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' The base class of the Hamiltonian systems which can be split into the momentum update
    ''' ("kick") and the position update ("drift") parts.
    ''' (可以分解为「动量踢」与「位置漂移」两个部分的哈密顿系统基类)
    ''' </summary>
    ''' <remarks>
    ''' 辛几何积分器要求方程可以写成：
    ''' 
    ''' ```
    ''' dq/dt = G(p)       ' 漂移(drift)
    ''' dp/dt = F(q)       ' 踢(kick)
    ''' ```
    ''' 
    ''' 因此模型需要显式给出 ``F`` 与 ``G`` 两个部分，而不能从通用的 ``f(t, y)`` 之中自动
    ''' 推断出来。状态向量的位置分量与动量分量通过 <see cref="PositionIndices"/> 与
    ''' <see cref="MomentumIndices"/> 指定，默认取前半为位置 ``q``、后半为动量 ``p``
    ''' （此时状态维度必须为偶数）。
    ''' 
    ''' 该模型的 ``func`` 由 ``Kick`` 与 ``Drift`` 自动组装，因此在辛几何求解器之外，
    ''' 也可以直接使用 RK4 / RK45 等通用求解器对它进行积分。
    ''' </remarks>
    Public MustInherit Class SymplecticODEs : Inherits ODEs

        Private qIndex As Integer()
        Private pIndex As Integer()
        Private stateQ As Double()
        Private stateP As Double()
        Private kickBuffer As Double()
        Private driftBuffer As Double()
        Private fallbackState As Double()
        Private fallbackDeriv As Double()

        ''' <summary>
        ''' Create the symplectic model, the state variables are discovered by the reflection
        ''' of the ``var`` fields.(从 var 字段之中发现状态变量)
        ''' </summary>
        Protected Sub New()
            Call MyBase.New()
            Me.FastDerivative = AddressOf ComputeDerivative
            Call EnsureSplit()
        End Sub

        ''' <summary>
        ''' Create the symplectic model from the given state variables.
        ''' </summary>
        ''' <param name="vars"></param>
        Protected Sub New(vars As var())
            Call MyBase.New(vars)
            Me.FastDerivative = AddressOf ComputeDerivative
            Call EnsureSplit()
        End Sub

        ''' <summary>
        ''' The state index of the positions ``q``, when this value is not presented the first
        ''' half of the state vector will be used.(位置分量的状态索引)
        ''' </summary>
        ''' <returns></returns>
        Public Property PositionIndices As Integer()

        ''' <summary>
        ''' The state index of the momenta ``p``, when this value is not presented the second
        ''' half of the state vector will be used.(动量分量的状态索引)
        ''' </summary>
        ''' <returns></returns>
        Public Property MomentumIndices As Integer()

        ''' <summary>
        ''' The momentum update ``dp = F(q)``.(动量更新)
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="q"></param>
        ''' <param name="p"></param>
        ''' <param name="dp"></param>
        Protected MustOverride Sub Kick(t As Double, q As Double(), p As Double(), dp As Double())

        ''' <summary>
        ''' The position update ``dq = G(p)``.(位置更新，通常就是 ``dq = p``)
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="q"></param>
        ''' <param name="p"></param>
        ''' <param name="dq"></param>
        Protected MustOverride Sub Drift(t As Double, q As Double(), p As Double(), dq As Double())

        ''' <summary>
        ''' The resolved position indices of the state vector.(位置分量的索引)
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Positions As Integer()
            Get
                Call EnsureSplit()
                Return qIndex
            End Get
        End Property

        ''' <summary>
        ''' The resolved momentum indices of the state vector.(动量分量的索引)
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Momenta As Integer()
            Get
                Call EnsureSplit()
                Return pIndex
            End Get
        End Property

        ''' <summary>
        ''' Resolve the position/momentum split of the state vector.
        ''' </summary>
        Friend Sub EnsureSplit()
            If qIndex IsNot Nothing Then
                Return
            End If

            ' 注意：局部变量不能命名为 size —— VB 的标识符是大小写不敏感的，
            ' 那样会在初始化表达式之中遮蔽 Size 属性(读到未初始化的局部变量 0)
            Dim dimension As Integer = Size

            If PositionIndices Is Nothing OrElse MomentumIndices Is Nothing Then
                If dimension Mod 2 <> 0 Then
                    Throw New ArgumentException(
                        $"The symplectic model requires an even number of the state variables when the position/momentum split is not presented, but {dimension} state variables are found!")
                End If

                Dim half As Integer = dimension \ 2

                qIndex = New Integer(half - 1) {}
                pIndex = New Integer(half - 1) {}

                For i As Integer = 0 To half - 1
                    qIndex(i) = i
                    pIndex(i) = half + i
                Next
            Else
                If PositionIndices.Length = 0 OrElse PositionIndices.Length <> MomentumIndices.Length Then
                    Throw New ArgumentException("The position indices and the momentum indices of the symplectic model are not matched!")
                End If

                qIndex = PositionIndices
                pIndex = MomentumIndices
            End If

            Dim count As Integer = qIndex.Length

            stateQ = New Double(count - 1) {}
            stateP = New Double(count - 1) {}
            kickBuffer = New Double(count - 1) {}
            driftBuffer = New Double(count - 1) {}
        End Sub

        ''' <summary>
        ''' The number of the position (and momentum) components of the split state vector.
        ''' (位置/动量分量的个数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SplitSize As Integer
            Get
                Return Positions.Length
            End Get
        End Property

        ''' <summary>
        ''' Compute the momentum derivative ``F(q)`` at the given state vector.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="dp"></param>
        Friend Sub ComputeKick(t As Double, y As Double(), dp As Double())
            Call LoadSplitedState(y)
            Call Kick(t, stateQ, stateP, dp)
        End Sub

        ''' <summary>
        ''' Compute the position derivative ``G(p)`` at the given state vector.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="dq"></param>
        Friend Sub ComputeDrift(t As Double, y As Double(), dq As Double())
            Call LoadSplitedState(y)
            Call Drift(t, stateQ, stateP, dq)
        End Sub

        ''' <summary>
        ''' Copy the position/momentum components of the given state vector into the internal
        ''' split buffers.
        ''' </summary>
        ''' <param name="y"></param>
        Private Sub LoadSplitedState(y As Double())
            Call EnsureSplit()

            For i As Integer = 0 To qIndex.Length - 1
                stateQ(i) = y(qIndex(i))
                stateP(i) = y(pIndex(i))
            Next
        End Sub

        ''' <summary>
        ''' Assemble the derivatives from the ``Kick`` and ``Drift`` parts.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="dy"></param>
        Friend Sub ComputeDerivative(t As Double, y As Double(), dy As Double())
            Call LoadSplitedState(y)

            Call Kick(t, stateQ, stateP, kickBuffer)
            Call Drift(t, stateQ, stateP, driftBuffer)

            Call System.Array.Clear(dy, 0, dy.Length)

            For i As Integer = 0 To qIndex.Length - 1
                dy(qIndex(i)) = driftBuffer(i)
                dy(pIndex(i)) = kickBuffer(i)
            Next
        End Sub

        ''' <summary>
        ''' The generic right-hand side function: it is a fallback for the case when the array
        ''' based <see cref="ODEs.FastDerivative"/> path is disabled.
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim dimension As Integer = Size

            If fallbackState Is Nothing Then
                fallbackState = New Double(dimension - 1) {}
                fallbackDeriv = New Double(dimension - 1) {}
            End If

            For i As Integer = 0 To dimension - 1
                fallbackState(i) = vars(i).Value
            Next

            Call ComputeDerivative(dx, fallbackState, fallbackDeriv)

            For i As Integer = 0 To dimension - 1
                dy(i) = fallbackDeriv(i)
            Next
        End Sub
    End Class
End Namespace
