Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' Pre-allocated working buffers for the ODEs integration kernels.
    ''' 
    ''' (ODE 方程组积分内核所使用的预分配工作缓冲)
    ''' </summary>
    ''' <remarks>
    ''' 全部缓冲都在求解器构造的时候一次性分配完成，因此 RK4 / RK45 的每一个积分步
    ''' 都不会再产生任何托管堆分配。原实现每一个阶段都要通过 <see cref="Vector"/> 的
    ''' 算术运算符计算出若干个临时向量(每个步骤约 14 个 <see cref="Double"/> 数组)，
    ''' 在蒙特卡洛参数拟合这种需要成千上万次求解的场景下会成为主要的性能瓶颈。
    ''' </remarks>
    Friend NotInheritable Class SolverBuffers

        ''' <summary>
        ''' The number of the state variables in the ODEs system.(状态变量的个数)
        ''' </summary>
        Friend ReadOnly Size As Integer

        ''' <summary>
        ''' The current state vector ``y``.(当前的状态向量)
        ''' </summary>
        Friend ReadOnly y As Double()
        ''' <summary>
        ''' Scratch buffer for the stage combination ``y + scale * k``.(阶段组合的临时缓冲)
        ''' </summary>
        Friend ReadOnly tmp As Double()
        ''' <summary>
        ''' The candidate state of the next step which is produced by the adaptive solver.
        ''' (自适应求解器所计算出来的下一步状态)
        ''' </summary>
        Friend ReadOnly ynext As Double()
        ''' <summary>
        ''' The derivative buffers ``k1`` to ``k7`` of the Runge-Kutta stages.(各个阶段的导数缓冲)
        ''' </summary>
        Friend ReadOnly k As Double()()

        ''' <summary>
        ''' The <see cref="Vector"/> wrapper of <see cref="y"/>, this wrapper is required by
        ''' the <see cref="ODEs.ODEs"/> stage evaluator.(状态向量的 Vector 包装，供用户函数访问)
        ''' </summary>
        Friend ReadOnly vy As Vector
        ''' <summary>
        ''' The <see cref="Vector"/> wrapper of <see cref="tmp"/>.
        ''' </summary>
        Friend ReadOnly vtmp As Vector
        ''' <summary>
        ''' The <see cref="Vector"/> wrapper of <see cref="ynext"/>.
        ''' </summary>
        Friend ReadOnly vynext As Vector
        ''' <summary>
        ''' The <see cref="Vector"/> wrapper of <see cref="k"/>.
        ''' </summary>
        Friend ReadOnly vk As Vector()

        ''' <summary>
        ''' Create the working buffers for a ODEs system which contains the given number of
        ''' state variables.
        ''' </summary>
        ''' <param name="size"></param>
        Friend Sub New(size As Integer)
            If size < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(size), "The ODEs system should contains at least one state variable!")
            End If

            Me.Size = size
            Me.vy = New Vector(size)
            Me.vtmp = New Vector(size)
            Me.vynext = New Vector(size)
            Me.vk = New Vector(6) {}

            For i As Integer = 0 To vk.Length - 1
                vk(i) = New Vector(size)
            Next

            ' Vector.Array 属性返回的是内部缓冲数组的引用，故这里不会产生拷贝
            Me.y = vy.Array
            Me.tmp = vtmp.Array
            Me.ynext = vynext.Array
            Me.k = New Double(vk.Length - 1)() {}

            For i As Integer = 0 To vk.Length - 1
                k(i) = vk(i).Array
            Next
        End Sub

        ''' <summary>
        ''' Copy the values of the <paramref name="source"/> buffer into the current state vector.
        ''' </summary>
        ''' <param name="source"></param>
        Friend Sub SetState(source As Double())
            Call System.Array.Copy(source, y, y.Length)
        End Sub
    End Class

    ''' <summary>
    ''' The fused, zero allocation arithmetic kernels of the ODEs integration.
    ''' (ODE 方程组的融合算术内核，全部为原位运算，没有任何内存分配)
    ''' </summary>
    Friend Module SolverKernels

        ''' <summary>
        ''' ``dst = y + scale * k``, all of the parameters are working on the pre-allocated buffers.
        ''' </summary>
        ''' <param name="dst"></param>
        ''' <param name="y"></param>
        ''' <param name="scale"></param>
        ''' <param name="k"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine(dst As Double(), y As Double(), scale As Double, k As Double())
            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + scale * k(i)
            Next
        End Sub

        ''' <summary>
        ''' ``dst = y + h * (w1 * k1 + w2 * k2)``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine2(dst As Double(), y As Double(), h As Double,
                            w1 As Double, k1 As Double(),
                            w2 As Double, k2 As Double())

            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + h * (w1 * k1(i) + w2 * k2(i))
            Next
        End Sub

        ''' <summary>
        ''' ``dst = y + h * (w1 * k1 + w2 * k2 + w3 * k3)``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine3(dst As Double(), y As Double(), h As Double,
                            w1 As Double, k1 As Double(),
                            w2 As Double, k2 As Double(),
                            w3 As Double, k3 As Double())

            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + h * (w1 * k1(i) + w2 * k2(i) + w3 * k3(i))
            Next
        End Sub

        ''' <summary>
        ''' ``dst = y + h * (w1 * k1 + w2 * k2 + w3 * k3 + w4 * k4)``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine4(dst As Double(), y As Double(), h As Double,
                            w1 As Double, k1 As Double(),
                            w2 As Double, k2 As Double(),
                            w3 As Double, k3 As Double(),
                            w4 As Double, k4 As Double())

            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + h * (w1 * k1(i) + w2 * k2(i) + w3 * k3(i) + w4 * k4(i))
            Next
        End Sub

        ''' <summary>
        ''' ``dst = y + h * (w1 * k1 + w2 * k2 + w3 * k3 + w4 * k4 + w5 * k5)``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine5(dst As Double(), y As Double(), h As Double,
                            w1 As Double, k1 As Double(),
                            w2 As Double, k2 As Double(),
                            w3 As Double, k3 As Double(),
                            w4 As Double, k4 As Double(),
                            w5 As Double, k5 As Double())

            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + h * (w1 * k1(i) + w2 * k2(i) + w3 * k3(i) + w4 * k4(i) + w5 * k5(i))
            Next
        End Sub

        ''' <summary>
        ''' ``dst = y + h * (w1 * k1 + w2 * k2 + w3 * k3 + w4 * k4 + w5 * k5 + w6 * k6)``
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Combine6(dst As Double(), y As Double(), h As Double,
                            w1 As Double, k1 As Double(),
                            w2 As Double, k2 As Double(),
                            w3 As Double, k3 As Double(),
                            w4 As Double, k4 As Double(),
                            w5 As Double, k5 As Double(),
                            w6 As Double, k6 As Double())

            For i As Integer = 0 To dst.Length - 1
                dst(i) = y(i) + h * (w1 * k1(i) + w2 * k2(i) + w3 * k3(i) + w4 * k4(i) + w5 * k5(i) + w6 * k6(i))
            Next
        End Sub

        ''' <summary>
        ''' The fused update of the classical 4th order Runge-Kutta method:
        ''' 
        ''' ```
        ''' y(i) = y(i) + h/6 * (k1 + 2 * k2 + 2 * k3 + k4)
        ''' ```
        ''' 
        ''' The result values are written into the output columns at the same time, so no
        ''' extra pass over the state vector is required.(在更新状态向量的同时写入输出列)
        ''' </summary>
        ''' <param name="y">The state vector, it will be updated in place.</param>
        ''' <param name="k1"></param>
        ''' <param name="k2"></param>
        ''' <param name="k3"></param>
        ''' <param name="k4"></param>
        ''' <param name="h6">``h / 6``</param>
        ''' <param name="columns">The output columns of each state variable.</param>
        ''' <param name="index">The position of current step in the output columns.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Rk4Update(y As Double(),
                             k1 As Double(),
                             k2 As Double(),
                             k3 As Double(),
                             k4 As Double(),
                             h6 As Double,
                             columns As Double()(),
                             index As Integer)

            For i As Integer = 0 To y.Length - 1
                Dim vi As Double = y(i) + h6 * (k1(i) + 2.0 * (k2(i) + k3(i)) + k4(i))

                y(i) = vi
                columns(i)(index) = vi
            Next
        End Sub

        ''' <summary>
        ''' The RMS norm of the local truncation error estimate of the Dormand-Prince 5(4)
        ''' method, the difference between the 5th order solution and the 4th order solution
        ''' is scaled by ``atol + rtol * max(|y|, |y5|)``.
        ''' </summary>
        ''' <param name="y">The state vector at the begin of the step.</param>
        ''' <param name="y5">The 5th order solution of the step.</param>
        ''' <param name="k">The 7 derivative buffers of the Dormand-Prince stages.</param>
        ''' <param name="h">The step size.</param>
        ''' <param name="atol">Absolute tolerance.</param>
        ''' <param name="rtol">Relative tolerance.</param>
        ''' <returns></returns>
        Friend Function ErrorNorm(y As Double(), y5 As Double(), k As Double()(), h As Double,
                                  atol As Double, rtol As Double) As Double

            Dim b As Double() = Dp5Coefficients.b4
            Dim sum As Double = 0.0

            For i As Integer = 0 To y.Length - 1
                Dim y5i As Double = y5(i)
                Dim y4i As Double = y(i) + h * (
                    b(0) * k(0)(i) +
                    b(2) * k(2)(i) +
                    b(3) * k(3)(i) +
                    b(4) * k(4)(i) +
                    b(5) * k(5)(i) +
                    b(6) * k(6)(i))
                Dim scale As Double = atol + rtol * System.Math.Max(System.Math.Abs(y(i)), System.Math.Abs(y5i))
                Dim e As Double = (y5i - y4i) / scale

                sum += e * e
            Next

            Return System.Math.Sqrt(sum / y.Length)
        End Function

        ''' <summary>
        ''' Compute the step size change factor from the scaled error norm of the Dormand-Prince
        ''' error estimator: ``0.9 * err^(-1/5)``, clamped into ``[0.2, 5]``.
        ''' </summary>
        ''' <param name="err">The RMS norm of the scaled local truncation error.</param>
        ''' <returns></returns>
        Friend Function StepFactor(err As Double) As Double
            Dim f As Double

            If Double.IsNaN(err) Then
                Return Dp5Coefficients.MinFactor
            ElseIf err <= 0.0 Then
                Return Dp5Coefficients.MaxFactor
            End If

            f = Dp5Coefficients.Safety * System.Math.Pow(err, -Dp5Coefficients.ErrorExponent)

            If f < Dp5Coefficients.MinFactor Then
                Return Dp5Coefficients.MinFactor
            ElseIf f > Dp5Coefficients.MaxFactor Then
                Return Dp5Coefficients.MaxFactor
            Else
                Return f
            End If
        End Function

        ''' <summary>
        ''' Cubic Hermite interpolation on the step ``[0, h]``: the function values and the
        ''' derivatives of both endpoints are used, so the dense output stays consistent
        ''' with the adaptive integration steps.
        ''' </summary>
        ''' <param name="y0"></param>
        ''' <param name="m0">The derivative at the begin of the step.</param>
        ''' <param name="y1"></param>
        ''' <param name="m1">The derivative at the end of the step.</param>
        ''' <param name="h"></param>
        ''' <param name="theta">The relative position inside the step, in ``[0, 1]``.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function Hermite(y0 As Double, m0 As Double, y1 As Double, m1 As Double,
                                h As Double, theta As Double) As Double

            Dim t2 As Double = theta * theta
            Dim t3 As Double = t2 * theta
            Dim h00 As Double = 2.0 * t3 - 3.0 * t2 + 1.0
            Dim h10 As Double = t3 - 2.0 * t2 + theta
            Dim h01 As Double = -2.0 * t3 + 3.0 * t2
            Dim h11 As Double = t3 - t2

            Return h00 * y0 + h10 * h * m0 + h01 * y1 + h11 * h * m1
        End Function
    End Module

    ''' <summary>
    ''' The Butcher tableau of the Dormand-Prince 5(4) pair with the FSAL property.
    ''' (Dormand-Prince 5(4) 方法(具有 FSAL 性质)的 Butcher 表)
    ''' </summary>
    ''' <remarks>
    ''' ``a7`` 行与 5 阶解的权重 ``b5`` 完全相同，这就是 FSAL 性质的来源：第 7 个阶段
    ''' 的导数 ``k7 = f(t + h, y5)`` 可以直接作为下一步的 ``k1`` 复用，从而每个积分步
    ''' 只需要 6 次函数求值。
    ''' </remarks>
    Friend Module Dp5Coefficients

        ' 节点的位置
        Friend ReadOnly c As Double() = {0.0, 1.0 / 5.0, 3.0 / 10.0, 4.0 / 5.0, 8.0 / 9.0, 1.0, 1.0}

        ' 阶段系数
        Friend ReadOnly a2 As Double() = {1.0 / 5.0}
        Friend ReadOnly a3 As Double() = {3.0 / 40.0, 9.0 / 40.0}
        Friend ReadOnly a4 As Double() = {44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0}
        Friend ReadOnly a5 As Double() = {19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0}
        Friend ReadOnly a6 As Double() = {9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0}
        Friend ReadOnly a7 As Double() = {35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0}

        ' 5 阶解的权重(= a7)
        Friend ReadOnly b5 As Double() = a7
        ' 4 阶解的权重，用于估计局部截断误差
        Friend ReadOnly b4 As Double() = {5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0}

        ''' <summary>
        ''' The exponent of the local truncation error, ``1 / 5`` for a 5th order method.
        ''' </summary>
        Friend Const ErrorExponent As Double = 0.2
        ''' <summary>
        ''' The safety factor of the step size controller.
        ''' </summary>
        Friend Const Safety As Double = 0.9
        ''' <summary>
        ''' The minimal/maximal step size change factors.
        ''' </summary>
        Friend Const MinFactor As Double = 0.2
        Friend Const MaxFactor As Double = 5.0
    End Module
End Namespace
