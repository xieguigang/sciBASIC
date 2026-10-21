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
            Me.vk = New Vector(6) {}

            For i As Integer = 0 To vk.Length - 1
                vk(i) = New Vector(size)
            Next

            ' Vector.Array 属性返回的是内部缓冲数组的引用，故这里不会产生拷贝
            Me.y = vy.Array
            Me.tmp = vtmp.Array
            Me.k = New Double(vk.Length - 1)() {}

            For i As Integer = 0 To vk.Length - 1
                k(i) = vk(i).Array
            Next
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
    End Module
End Namespace
