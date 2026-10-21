Namespace Dynamics

    ''' <summary>
    ''' Evaluate the Jacobian matrix ``J = ∂f/∂y`` of the ODEs system: when the analytic
    ''' Jacobian is not provided by the model, the central finite difference will be used.
    ''' (方程组 Jacobian 矩阵的求值：优先使用用户提供的解析 Jacobian，否则使用中心差商)
    ''' </summary>
    ''' <remarks>
    ''' 中心差商的步长取 ``δ_j = ε^(1/3) · max(1, |y_j|)``（``ε`` 为双精度机器精度），
    ''' 这个步长同时兼顾了截断误差 ``δ²`` 与舍入误差 ``ε/δ``，是中心差分的标准选择。
    ''' 
    ''' 所有的求值缓冲都在构造的时候预分配完成，因此每一次 Jacobian 求值除了
    ''' ``2 × (2N)`` 次右端函数调用之外没有任何内存分配。
    ''' </remarks>
    Friend NotInheritable Class JacobianEvaluator

        ''' <summary>
        ''' ``ε^(1/3)`` of the double precision: 6.0554544523933395E-06
        ''' </summary>
        Friend Const RelativeStep As Double = 6.0554544523933395E-06

        ''' <summary>
        ''' The number of the state variables.(状态变量的个数)
        ''' </summary>
        Friend ReadOnly Size As Integer

        Private ReadOnly model As ODEs
        Private ReadOnly jacobianOverride As Action(Of Double, Double(), Double(,))
        Private ReadOnly forward As Double()
        Private ReadOnly backward As Double()
        Private ReadOnly plus As Double()
        Private ReadOnly minus As Double()

        ''' <summary>
        ''' Create the Jacobian evaluator of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="jacobian">
        ''' The optional analytic Jacobian which is provided by the solver options, it has a
        ''' higher priority than the <see cref="ODEs.Jacobian"/> delegate of the model.
        ''' </param>
        Friend Sub New(system As ODEs, Optional jacobian As Action(Of Double, Double(), Double(,)) = Nothing)
            Me.model = system
            Me.jacobianOverride = jacobian
            Me.Size = system.Size
            Me.forward = New Double(Size - 1) {}
            Me.backward = New Double(Size - 1) {}
            Me.plus = New Double(Size - 1) {}
            Me.minus = New Double(Size - 1) {}
        End Sub

        ''' <summary>
        ''' Fill the Jacobian matrix at the given time and state.
        ''' </summary>
        ''' <param name="t">The evaluation time.</param>
        ''' <param name="y">The state vector.</param>
        ''' <param name="j">The ``N × N`` Jacobian output buffer.</param>
        Friend Sub Evaluate(t As Double, y As Double(), j As Double(,))
            Dim analytic As Action(Of Double, Double(), Double(,))

            If jacobianOverride IsNot Nothing Then
                analytic = jacobianOverride
            Else
                analytic = model.Jacobian
            End If

            If analytic IsNot Nothing Then
                Call analytic(t, y, j)
                Return
            End If

            Call System.Array.Copy(y, forward, Size)
            Call System.Array.Copy(y, backward, Size)

            For k As Integer = 0 To Size - 1
                Dim delta As Double = RelativeStep * System.Math.Max(1.0, System.Math.Abs(y(k)))

                forward(k) = y(k) + delta
                backward(k) = y(k) - delta

                ' 两个方向上的右端函数求值
                Call model.Derivatives(t, forward, plus)
                Call model.Derivatives(t, backward, minus)

                forward(k) = y(k)
                backward(k) = y(k)

                Dim scale As Double = 1.0 / (2.0 * delta)

                For i As Integer = 0 To Size - 1
                    j(i, k) = (plus(i) - minus(i)) * scale
                Next
            Next
        End Sub
    End Class
End Namespace
