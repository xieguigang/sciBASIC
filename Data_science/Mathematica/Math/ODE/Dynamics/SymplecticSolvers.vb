Imports std = System.Math

Namespace Dynamics

    ''' <summary>
    ''' The base class of the symplectic (energy preserving) integrators.
    ''' (辛几何积分器的基类)
    ''' </summary>
    ''' <remarks>
    ''' 辛几何积分器只适用于可以分解为「漂移 ``dq/dt = G(p)``」与「踢 ``dp/dt = F(q)``」的
    ''' 哈密顿系统，因此它要求模型继承 <see cref="SymplecticODEs"/>；否则会在构造的时候
    ''' 抛出 <see cref="InvalidOperationException"/>。
    ''' 
    ''' 与显式 Runge-Kutta 方法不同，辛几何方法并不追求单步的高阶精度，而是保证离散映射
    ''' 的辛结构，从而在长时程积分之中保持能量有界（不出现系统性的能量漂移）。
    ''' </remarks>
    Public MustInherit Class SymplecticSolverBase : Inherits ExplicitFixedStepSolver

        Friend ReadOnly symplectic As SymplecticODEs

        Private ReadOnly dp As Double()
        Private ReadOnly dq As Double()

        ''' <summary>
        ''' Create the symplectic solver of the given Hamiltonian system.
        ''' </summary>
        ''' <param name="system"></param>
        Protected Sub New(system As ODEs)
            MyBase.New(system)

            Me.symplectic = TryCast(system, SymplecticODEs)

            If symplectic Is Nothing Then
                Throw New InvalidOperationException(
                    $"{Me.GetType().Name} requires a SymplecticODEs model which defines the Kick/Drift parts, the {system.GetType().Name} model is not supported!")
            End If

            Dim count As Integer = symplectic.Positions.Length

            dp = New Double(count - 1) {}
            dq = New Double(count - 1) {}
        End Sub

        ''' <summary>
        ''' Perform one Stormer-Verlet step: ``p += h/2·F(q)`` → ``q += h·G(p)`` →
        ''' ``p += h/2·F(q)``.
        ''' (执行一个 Verlet 步：踢-漂移-踢)
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="h"></param>
        Friend Sub VerletStep(t As Double, h As Double)
            Dim y As Double() = buffers.y
            Dim momenta As Integer() = symplectic.Momenta
            Dim positions As Integer() = symplectic.Positions
            Dim half As Double = 0.5 * h

            ' 第一个半踢
            Call symplectic.ComputeKick(t, y, dp)

            For i As Integer = 0 To momenta.Length - 1
                y(momenta(i)) += half * dp(i)
            Next

            ' 整步漂移
            Call symplectic.ComputeDrift(t, y, dq)

            For i As Integer = 0 To positions.Length - 1
                y(positions(i)) += h * dq(i)
            Next

            ' 第二个半踢
            Call symplectic.ComputeKick(t + h, y, dp)

            For i As Integer = 0 To momenta.Length - 1
                y(momenta(i)) += half * dp(i)
            Next

            Call CountEvaluations(3)
        End Sub
    End Class

    ''' <summary>
    ''' The 2nd order Stormer-Verlet (leapfrog) symplectic integrator.(二阶辛几何算法)
    ''' </summary>
    ''' <remarks>
    ''' 二阶精度、时间可逆、辛结构保持，特别适合分子动力学与天体力学这类需要长期能量
    ''' 守恒的仿真。
    ''' </remarks>
    Public Class StormerVerletSolver : Inherits SymplecticSolverBase

        ''' <summary>
        ''' Create the Stormer-Verlet solver of the given Hamiltonian system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Call VerletStep(t, h)
            Call grid.WriteNext(buffers.y)
        End Sub
    End Class

    ''' <summary>
    ''' The 4th order Yoshida composition of the Stormer-Verlet method.(四阶吉田分解)
    ''' </summary>
    ''' <remarks>
    ''' 用三次 Verlet 步（步长系数 ``w1, w0, w1``）组合出四阶精度，同时保持辛结构：
    ''' 
    ''' ```
    ''' w1 = 1 / (2 − 2^(1/3)) ≈ 1.3512071919596578
    ''' w0 = −2^(1/3) / (2 − 2^(1/3)) ≈ −1.7024143839193153
    ''' ```
    ''' 
    ''' 每一个积分步需要 9 次「踢/漂移」求值，代价约为 Verlet 的 3 倍，但是精度提升到四阶。
    ''' </remarks>
    Public Class Yoshida4Solver : Inherits SymplecticSolverBase

        Private Shared ReadOnly CubeRoot2 As Double = std.Pow(2.0, 1.0 / 3.0)
        Private Shared ReadOnly W1 As Double = 1.0 / (2.0 - CubeRoot2)
        Private Shared ReadOnly W0 As Double = -CubeRoot2 / (2.0 - CubeRoot2)

        ''' <summary>
        ''' Create the Yoshida 4th order solver of the given Hamiltonian system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        ''' <summary>
        ''' The step size coefficient of the first/last Verlet sub-step.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Weight1 As Double
            Get
                Return W1
            End Get
        End Property

        ''' <summary>
        ''' The step size coefficient of the middle Verlet sub-step.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Weight0 As Double
            Get
                Return W0
            End Get
        End Property

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Dim h1 As Double = W1 * h
            Dim h0 As Double = W0 * h

            Call VerletStep(t, h1)
            Call VerletStep(t + h1, h0)
            Call VerletStep(t + h1 + h0, h1)

            Call grid.WriteNext(buffers.y)
        End Sub
    End Class
End Namespace
