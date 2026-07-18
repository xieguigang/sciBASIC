#Region "Microsoft.VisualBasic::1f4c96fb017cece83d7a55a311a20bf7, Data_science\Mathematica\Math\CVODE_Solver\CVODESolver.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 934
    '    Code Lines: 712 (76.23%)
    ' Comment Lines: 97 (10.39%)
    '    - Xml Docs: 47.42%
    ' 
    '   Blank Lines: 125 (13.38%)
    '     File Size: 32.14 KB


    ' Class CVODESolver
    ' 
    '     Properties: CurrentOrder, CurrentState, CurrentStep, CurrentTime, LinearSolves
    '                 NewtonIterations, RHSFunctionEvaluations, TotalSteps
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: [Step], AttemptStep, BuildAndFactorizeSystem, ComputeJacobian, EstimateAdamsError
    '               EstimateBDFFrror, EstimateInitialStep, Factorial, Initialize, Integrate
    '               IntegrateLagrangeBasis, LagrangeBasisDeriv, LagrangeBasisValue, PolyEval, PolyFromRoots
    '               PolyIntegrate, PolyMul
    ' 
    '     Sub: AllocateResources, Commit, ComputeErrorWeights, ComputeNumericalJacobian, (+2 Overloads) Dispose
    '          Predict, (+2 Overloads) SetAbsoluteTolerance, SetJacobianFunction, SetMaxStep, SetMinStep
    '          SetRelativeTolerance
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' CVODESolver.vb - CVODE常微分方程求解器核心模块（重写版）
' 采用经典变步长、变阶数多步法（Adams 非刚性 / BDF 刚性）。
' 预测-校正系数通过 Lagrange / 差商在每一步从真实历史点直接计算，
' 因此对任意步长变化都正确，无需记忆固定系数表。
' 仅基于 .NET 基础数学函数库实现，不依赖第三方库。
' ============================================================================

Imports std = System.Math

''' <summary>
''' CVODE常微分方程求解器（变阶变步长多步法）
''' </summary>
Public Class CVODESolver : Implements IDisposable

#Region "常量定义"

    Private Const ADAMS_MAX_ORDER As Integer = 12
    Private Const BDF_MAX_ORDER As Integer = 5
    Private Const DEFAULT_REL_TOL As Double = 0.0001
    Private Const DEFAULT_ABS_TOL As Double = 0.00000001
    Private Const MIN_H_FACTOR As Double = 0.1
    Private Const ZERO_THRESHOLD As Double = 0.000000000000001
    ' 步长控制安全因子（CVODE 标准取值，目标误差 ~SAFETY^{q+1}，过小时步长塌缩）
    Private Const SAFETY As Double = 0.9
    Private Const MAX_GROWTH As Double = 2.0
    Private Const MIN_REDUCTION As Double = 0.25
    ' 单次 Step 内允许的最大失败重试次数
    Private Const MAX_FAILS As Integer = 10

#End Region

#Region "私有字段"

    Private _n As Integer
    Private _method As CVODEMethod
    Private _options As CVODEOptions
    Private _maxOrder As Integer

    Private _rhsFunc As RHSFunction
    Private _jacobianFunc As JacobianFunction

    Private _t As Double
    Private _y As NVector
    Private _ydot As NVector

    Private _h As Double
    Private _q As Integer
    Private _nSteps As Long
    Private _nRHSEvals As Long
    Private _nNewtonIters As Long
    Private _nLinearSolves As Long

    ' 历史数据（index 0 = 最新）
    Private _cap As Integer
    Private _tHist() As Double
    Private _yHist() As NVector
    Private _fHist() As NVector
    Private _histCount As Integer

    ' 误差权重与绝对误差容差
    Private _ewt As NVector
    Private _atol As NVector

    ' 线性求解器与矩阵 / 临时向量
    Private _linearSolver As DenseLinearSolver
    Private _J As DenseMatrix
    Private _A As DenseMatrix
    Private _tempV As NVector
    Private _tempV2 As NVector
    Private _tempV3 As NVector
    Private _delta As NVector
    Private _knownSumV As NVector
    Private _fNewHist As NVector

    Private _isInitialized As Boolean
    Private _isDisposed As Boolean

#End Region

#Region "构造函数"

    Public Sub New(method As CVODEMethod, rhsFunc As RHSFunction, n As Integer, Optional options As CVODEOptions = Nothing)
        If rhsFunc Is Nothing Then
            Throw New ArgumentNullException(NameOf(rhsFunc))
        End If
        If n <= 0 Then
            Throw New ArgumentException("问题维度必须为正数", NameOf(n))
        End If

        _method = method
        _rhsFunc = rhsFunc
        _n = n
        _options = If(options, New CVODEOptions())

        Dim maxOrder As Integer = If(_method = CVODEMethod.Adams, ADAMS_MAX_ORDER, BDF_MAX_ORDER)
        If _options.MaxOrder < 1 OrElse _options.MaxOrder > maxOrder Then
            _options.MaxOrder = maxOrder
        End If
        _maxOrder = _options.MaxOrder

        AllocateResources()
    End Sub

    ''' <summary>分配所有内部资源（不依赖初值）。</summary>
    Private Sub AllocateResources()
        _y = New NVector(_n)
        _ydot = New NVector(_n)
        _ewt = New NVector(_n)
        _atol = NVector.Constant(_n, _options.AbsoluteTolerance)
        _tempV = New NVector(_n)
        _tempV2 = New NVector(_n)
        _tempV3 = New NVector(_n)
        _delta = New NVector(_n)
        _knownSumV = New NVector(_n)
        _fNewHist = New NVector(_n)

        _cap = _maxOrder + 1
        _tHist = New Double(_cap - 1) {}
        _yHist = New NVector(_cap - 1) {}
        _fHist = New NVector(_cap - 1) {}
        For i As Integer = 0 To _cap - 1
            _yHist(i) = New NVector(_n)
            _fHist(i) = New NVector(_n)
        Next

        _linearSolver = New DenseLinearSolver(_n)
        _J = New DenseMatrix(_n, _n)
        _A = New DenseMatrix(_n, _n)

        _isInitialized = False
    End Sub

#End Region

#Region "属性"

    Public ReadOnly Property CurrentTime As Double
        Get
            Return _t
        End Get
    End Property

    Public ReadOnly Property CurrentState As NVector
        Get
            Return _y
        End Get
    End Property

    Public ReadOnly Property CurrentStep As Double
        Get
            Return _h
        End Get
    End Property

    Public ReadOnly Property CurrentOrder As Integer
        Get
            Return _q
        End Get
    End Property

    Public ReadOnly Property TotalSteps As Long
        Get
            Return _nSteps
        End Get
    End Property

    Public ReadOnly Property RHSFunctionEvaluations As Long
        Get
            Return _nRHSEvals
        End Get
    End Property

    Public ReadOnly Property NewtonIterations As Long
        Get
            Return _nNewtonIters
        End Get
    End Property

    Public ReadOnly Property LinearSolves As Long
        Get
            Return _nLinearSolves
        End Get
    End Property

#End Region

#Region "初始化求解"

    Public Function Initialize(t0 As Double, y0 As NVector) As CVODEStatus
        If y0 Is Nothing OrElse y0.Length <> _n Then
            Return CVODEStatus.BadInput
        End If

        _t = t0
        _y.CopyFrom(y0)

        ' 计算初始导数
        _rhsFunc(_t, _y, _ydot)
        _nRHSEvals += 1

        ' 初始化历史（仅 1 个点，阶数从 1 起步，后续逐步提升）
        _histCount = 1
        _tHist(0) = _t
        _yHist(0).CopyFrom(_y)
        _fHist(0).CopyFrom(_ydot)
        _q = 1

        ' 误差权重（基于当前状态）
        ComputeErrorWeights()

        ' 估计初始步长
        Dim status As CVODEStatus = EstimateInitialStep()
        If status <> CVODEStatus.Success Then
            Return status
        End If

        _nSteps = 0
        _nNewtonIters = 0
        _nLinearSolves = 0
        _isInitialized = True

        Return CVODEStatus.Success
    End Function

    ''' <summary>估计初始步长（用户指定优先，否则基于初值规模）。</summary>
    Private Function EstimateInitialStep() As CVODEStatus
        If _options.InitialStep > 0 Then
            _h = _options.InitialStep
        Else
            Dim yn As Double = _y.WRMSNorm(NVector.Ones(_n))
            Dim fn As Double = _ydot.WRMSNorm(NVector.Ones(_n))
            If fn > ZERO_THRESHOLD Then
                _h = 0.01 * yn / fn
            Else
                _h = 0.001
            End If
            If _h <= 0 Then
                _h = 0.001
            End If
        End If

        If _options.MaxStep > 0 Then
            _h = std.Min(_h, _options.MaxStep)
        End If
        If _options.MinStep > 0 Then
            _h = std.Max(_h, _options.MinStep)
        End If
        If _h <= ZERO_THRESHOLD Then
            _h = ZERO_THRESHOLD
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>计算误差权重 w_i = 1 / (rtol*|y_i| + atol_i)。</summary>
    Private Sub ComputeErrorWeights()
        For i As Integer = 0 To _n - 1
            Dim denom As Double = _options.RelativeTolerance * std.Abs(_y(i)) + _atol(i)
            If denom < ZERO_THRESHOLD Then
                denom = ZERO_THRESHOLD
            End If
            _ewt(i) = 1.0 / denom
        Next
    End Sub

#End Region

#Region "主求解循环"

    Public Function [Step](tOut As Double) As CVODEStatus
        If Not _isInitialized Then
            Return CVODEStatus.BadInput
        End If
        If _t >= tOut Then
            Return CVODEStatus.Success
        End If
        If _nSteps >= _options.MaxSteps Then
            Return CVODEStatus.TooManySteps
        End If

        ' 调整步长以精确到达 tOut
        Dim hTry As Double = _h
        If (tOut - _t) <= hTry Then
            hTry = tOut - _t
        End If
        If hTry <= 0 Then
            Return CVODEStatus.Success
        End If

        Dim tries As Integer = 0
        Do
            tries += 1
            Dim hNext As Double = hTry
            Dim errEst As Double = 0.0
            Dim st As CVODEStatus = AttemptStep(hTry, hNext, errEst)

            If st = CVODEStatus.Success Then
                _nSteps += 1
                Return CVODEStatus.Success
            ElseIf st = CVODEStatus.TestFail Then
                ' 误差过大：使用建议的更小步长重试
                hTry = hNext
                If _q > 1 Then
                    _q -= 1
                End If
                If tries >= MAX_FAILS OrElse hTry <= ZERO_THRESHOLD Then
                    Return CVODEStatus.ConvFail
                End If
            ElseIf st = CVODEStatus.ConvFail Then
                ' Newton 不收敛：缩小步长并重试（更小步长使 I-gamma*J 更易求解）
                If _q > 1 Then
                    _q -= 1
                End If
                hTry = hTry * 0.25
                If _options.MinStep > 0 Then
                    hTry = std.Max(hTry, _options.MinStep)
                End If
                If tries >= MAX_FAILS OrElse hTry <= ZERO_THRESHOLD Then
                    Return CVODEStatus.ConvFail
                End If
            Else
                ' 线性求解失败等硬错误
                Return st
            End If
        Loop
    End Function

    Public Function Integrate(tOut As Double, Optional yOut As NVector = Nothing) As CVODEStatus
        Dim status As CVODEStatus

        Do While _t < tOut
            status = [Step](tOut)
            If status <> CVODEStatus.Success Then
                Return status
            End If
            If _nSteps >= _options.MaxSteps Then
                Return CVODEStatus.TooManySteps
            End If
        Loop

        If yOut IsNot Nothing Then
            yOut.CopyFrom(_y)
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' 尝试前进一步。成功则提交历史并返回 Success；
    ''' 误差过大返回 TestFail（hNext 为建议的更小步长）；
    ''' 不收敛/线性求解失败返回相应状态且不提交。
    ''' </summary>
    Private Function AttemptStep(hTry As Double, ByRef hNext As Double, ByRef errEst As Double) As CVODEStatus
        Dim q As Integer = _q
        Dim tNew As Double = _t + hTry

        ComputeErrorWeights()

        ' ---- 预测 ----
        Dim yPred As New NVector(_n)
        Predict(q, hTry, tNew, yPred)

        ' ---- 校正系数（按当前真实历史点计算）----
        Dim gamma As Double = 0.0
        Dim linStatus As LinearSolverResult

        If _method = CVODEMethod.Adams Then
            ' 校正节点：tNew + 过去 q 个 f 点
            Dim nodes As Double() = New Double(q) {}
            nodes(0) = tNew
            For j As Integer = 1 To q
                nodes(j) = _tHist(j - 1)
            Next
            ' I(j) = ∫_{t}^{tNew} L_j(τ) dτ
            Dim Icoeff(q) As Double
            For j As Integer = 0 To q
                Icoeff(j) = IntegrateLagrangeBasis(nodes, j, _t, tNew)
            Next
            gamma = Icoeff(0)

            ' knownSum = Σ_{j=1}^{q} I(j) * fHist(j-1)
            _knownSumV.SetConstant(0.0)
            For j As Integer = 1 To q
                _knownSumV.LinearSumInPlace(Icoeff(j), _fHist(j - 1))
            Next
        Else
            ' BDF：校正节点 = tNew + 过去 q 个 y 点
            Dim nodes As Double() = New Double(q) {}
            nodes(0) = tNew
            For j As Integer = 1 To q
                nodes(j) = _tHist(j - 1)
            Next
            Dim c0 As Double = LagrangeBasisDeriv(nodes, 0, tNew)
            If std.Abs(c0) < ZERO_THRESHOLD Then
                Return CVODEStatus.LinearSolveFail
            End If
            gamma = 1.0 / c0

            ' knownSum = Σ_{j=1}^{q} alphaHat(j-1) * yHist(j-1)
            ' alphaHat(k-1) = -L'_k(tNew) / c0
            _knownSumV.SetConstant(0.0)
            For k As Integer = 1 To q
                Dim ak As Double = -LagrangeBasisDeriv(nodes, k, tNew) / c0
                _knownSumV.LinearSumInPlace(ak, _yHist(k - 1))
            Next
        End If

        ' ---- Jacobian 与线性系统 (I - gamma*J) ----
        ComputeJacobian(tNew, yPred)
        linStatus = BuildAndFactorizeSystem(gamma)
        If linStatus <> LinearSolverResult.Success Then
            Return CVODEStatus.LinearSolveFail
        End If

        ' ---- Newton 迭代 ----
        Dim y As New NVector(_n)
        y.CopyFrom(yPred)
        ' 收敛阈值采用 CVODE 标准 RCON=0.33（按 WRMS 加权范数）。
        ' 过紧（如 1e-4 或 0.1）会使非线性 BDF 示例无法在有限次迭代内收敛。
        Dim maxIters As Integer = std.Max(_options.MaxNewtonIterations, 10)
        Dim convTol As Double = std.Max(_options.NewtonConvergenceFactor, 0.33)
        Dim converged As Boolean = False

        For iter As Integer = 1 To maxIters
            _rhsFunc(tNew, y, _tempV3)
            _nRHSEvals += 1
            _fNewHist.CopyFrom(_tempV3)

            ' 残差 R(y)
            If _method = CVODEMethod.Adams Then
                _tempV.CopyFrom(y)
                _tempV.SubtractVector(_y)            ' y - y_n
                _tempV2.CopyFrom(_tempV3)
                _tempV2.ScaleInPlace(gamma)          ' gamma * f_new
                _tempV.SubtractVector(_tempV2)       ' (y - y_n) - gamma f_new
                _tempV.SubtractVector(_knownSumV)    ' - knownSum
            Else
                _tempV.CopyFrom(y)
                _tempV2.CopyFrom(_tempV3)
                _tempV2.ScaleInPlace(gamma)          ' gamma * f_new
                _tempV.SubtractVector(_tempV2)       ' y - gamma f_new
                _tempV.SubtractVector(_knownSumV)    ' - knownSum
            End If

            ' 求解 (I - gamma J) delta = -R
            _tempV2.CopyFrom(_tempV)
            _tempV2.ScaleInPlace(-1.0)
            linStatus = _linearSolver.Solve(_tempV2, _delta)
            _nLinearSolves += 1
            If linStatus <> LinearSolverResult.Success Then
                Return CVODEStatus.LinearSolveFail
            End If

            y.AddVector(_delta)
            _nNewtonIters += 1

            Dim dNorm As Double = _delta.WRMSNorm(_ewt)
            If dNorm < convTol Then
                converged = True
                Exit For
            End If
        Next

        If Not converged Then
            Console.Error.WriteLine($"NEWTFAIL m={_method} q={q} h={hTry:E4} tNew={tNew:E4}")
            Return CVODEStatus.ConvFail
        End If

        ' ---- 误差估计 ----
        If _method = CVODEMethod.Adams Then
            ' Adams：预测(AB_{q+1})与校正(AM_{q+1})之差即局部截断误差（主阶），
            ' 该方法对任意阶均稳定（不依赖高阶差商）。
            _tempV.CopyFrom(y)
            _tempV.SubtractVector(yPred)
            errEst = _tempV.WRMSNorm(_ewt)
        Else
            ' BDF：用差商 + BDF 误差常数得到 O(h^{q+1}) 的真实截断误差
            errEst = EstimateBDFFrror(q, hTry, tNew, y)
        End If

        If errEst > 1.0 Then
            ' 拒绝：按误差缩小步长
            Dim factor As Double = SAFETY / std.Pow(errEst, 1.0 / (q + 1))
            factor = std.Max(factor, MIN_REDUCTION)
            hNext = hTry * factor
            If _options.MinStep > 0 Then
                hNext = std.Max(hNext, _options.MinStep)
            End If
            Return CVODEStatus.TestFail
        End If

        ' ---- 接受：步长与阶数控制 ----
        Dim grow As Double = SAFETY / std.Pow(errEst, 1.0 / (q + 1))
        grow = std.Max(grow, MIN_REDUCTION)
        grow = std.Min(grow, MAX_GROWTH)
        hNext = hTry * grow
        If _options.MaxStep > 0 Then
            hNext = std.Min(hNext, _options.MaxStep)
        End If
        If _options.MinStep > 0 Then
            hNext = std.Max(hNext, _options.MinStep)
        End If

        ' 写回自适应步长，供下一步使用（此前漏写导致步长控制失效、步数爆炸）
        _h = hNext

        If errEst < 0.1 AndAlso q < _maxOrder AndAlso _histCount >= q + 1 Then
            _q = q + 1
        ElseIf errEst > 0.5 AndAlso q > 1 Then
            _q = q - 1
        End If

        Commit(tNew, y)
        Return CVODEStatus.Success
    End Function

    ''' <summary>提交一步成功结果到历史。</summary>
    Private Sub Commit(tNew As Double, yCorr As NVector)
        ' 历史下移
        For i As Integer = _histCount - 1 To 1 Step -1
            _tHist(i) = _tHist(i - 1)
            _yHist(i).CopyFrom(_yHist(i - 1))
            _fHist(i).CopyFrom(_fHist(i - 1))
        Next
        _tHist(0) = tNew
        _yHist(0).CopyFrom(yCorr)
        _fHist(0).CopyFrom(_fNewHist)
        If _histCount < _cap Then
            _histCount += 1
        End If

        _t = tNew
        _y = _yHist(0)
        _ydot.CopyFrom(_fNewHist)
    End Sub

#End Region

#Region "预测"

    ''' <summary>
    ''' 计算预测值 yPred（写入 yPred）。
    ''' Adams：对过去 f 做多项式积分；BDF：对过去 y 做多项式外推。
    ''' 历史点不足时自动降阶使用可用点数。
    ''' </summary>
    Private Sub Predict(q As Integer, h As Double, tNew As Double, yPred As NVector)
        If _method = CVODEMethod.Adams Then
            Dim numPast As Integer = std.Min(q, _histCount - 1)
            Dim nodes(numPast) As Double
            For j As Integer = 0 To numPast
                nodes(j) = _tHist(j)
            Next
            yPred.CopyFrom(_y)
            For j As Integer = 0 To numPast
                Dim Ij As Double = IntegrateLagrangeBasis(nodes, j, _t, tNew)
                yPred.LinearSumInPlace(Ij, _fHist(j))
            Next
        Else
            Dim numPast As Integer = std.Min(q, _histCount - 1)
            Dim nodes(numPast) As Double
            For j As Integer = 0 To numPast
                nodes(j) = _tHist(j)
            Next
            yPred.SetConstant(0.0)
            For j As Integer = 0 To numPast
                Dim w As Double = LagrangeBasisValue(nodes, j, tNew)
                yPred.LinearSumInPlace(w, _yHist(j))
            Next
        End If
    End Sub

    ''' <summary>
    ''' 估计 BDF(q) 的局部截断误差（加权 RMS 范数）。
    '''   LTE_i = C_{q+1} · h^{q+1} · q! · DD_q[f]_i
    ''' 其中 DD_q[f] 为 f 在节点 (tNew, t_n, ..., t_{n-q+1}) 上的 q 阶差商，
    '''   C_{q+1} = (1/(q+1)!) · Σ_{k=0}^{q} α_k·(-k)^{q+1} 为 BDF 误差常数（α 为 BDF 系数）。
    ''' </summary>
    Private Function EstimateBDFFrror(q As Integer, h As Double, tNew As Double, yCorr As NVector) As Double
        ' 1) BDF 系数 α_k = c_k / c0，c_k = L'_k(tNew)
        Dim nodes(q) As Double
        nodes(0) = tNew
        For k As Integer = 1 To q
            nodes(k) = _tHist(k - 1)
        Next
        Dim c0 As Double = LagrangeBasisDeriv(nodes, 0, tNew)
        If std.Abs(c0) < ZERO_THRESHOLD Then
            Return Double.MaxValue
        End If
        Dim alpha(q) As Double
        alpha(0) = 1.0
        For k As Integer = 1 To q
            alpha(k) = LagrangeBasisDeriv(nodes, k, tNew) / c0
        Next

        ' 2) 误差常数 C_{q+1}
        Dim cq As Double = 0.0
        For k As Integer = 0 To q
            cq += alpha(k) * std.Pow(-k, q + 1)
        Next
        cq = cq / Factorial(q + 1)

        ' 3) q 阶差商 DD_q[f]_i（节点 xs=[tNew, tHist(0..q-1)]，值=[fNew, fHist(0..q-1)]）
        Dim m As Integer = q
        Dim xs(m) As Double
        Dim fv(m) As NVector
        xs(0) = tNew
        fv(0) = _fNewHist
        For k As Integer = 1 To m
            xs(k) = _tHist(k - 1)
            fv(k) = _fHist(k - 1)
        Next

        Dim errVec As New NVector(_n)
        For i As Integer = 0 To _n - 1
            Dim tab(m) As Double
            For k As Integer = 0 To m
                tab(k) = fv(k)(i)
            Next
            For j As Integer = 1 To m
                For k As Integer = 0 To m - j
                    tab(k) = (tab(k + 1) - tab(k)) / (xs(k + j) - xs(k))
                Next
            Next
            ' LTE_i = C_{q+1} · h^{q+1} · q! · DD_q[f]_i
            errVec(i) = cq * std.Pow(h, q + 1) * Factorial(q) * tab(0)
        Next

        Return errVec.WRMSNorm(_ewt)
    End Function

    ''' <summary>
    ''' 估计 Adams(q) 正确器（阶 p=q+1）的局部截断误差（加权 RMS 范数）。
    '''   LTE_i = C_AM · h^{p+1} · p! · DD_p[f]_i
    ''' 其中 DD_p[f] 为 f 在节点 (tNew, tHist(0..q)) 上的 p 阶差商，
    '''   C_AM = (-1)^p/(p+1)! · [1 - (p+1)·Σ_{j=0}^{p-1} β̄_j·j^p]，
    '''   β̄_j = Icoeff(j)/Icoeff(0)，Icoeff(j)=∫_{t_n}^{tNew} L_j dτ。
    ''' 调用前需保证 _histCount >= q+1（即拥有 q+1 个历史 f 点）。
    ''' </summary>
    Private Function EstimateAdamsError(q As Integer, h As Double, tNew As Double) As Double
        Dim p As Integer = q + 1

        Dim nodes(q) As Double
        nodes(0) = tNew
        For j As Integer = 1 To q
            nodes(j) = _tHist(j - 1)
        Next
        Dim Icoeff(q) As Double
        For j As Integer = 0 To q
            Icoeff(j) = IntegrateLagrangeBasis(nodes, j, _t, tNew)
        Next
        If std.Abs(Icoeff(0)) < ZERO_THRESHOLD Then
            Return Double.MaxValue
        End If
        Dim betaBar(q) As Double
        For j As Integer = 0 To q
            betaBar(j) = Icoeff(j) / Icoeff(0)
        Next

        Dim cam As Double = 0.0
        For j As Integer = 0 To q
            cam += betaBar(j) * std.Pow(j, p)
        Next
        cam = std.Pow(-1, p) / Factorial(p + 1) * (1.0 - (p + 1) * cam)

        ' p 阶差商 DD_p[f]：节点 xs=[tNew, tHist(0..q)]，值=[fNew, fHist(0..q)]
        Dim m As Integer = p
        Dim xs(m) As Double
        Dim fv(m) As NVector
        xs(0) = tNew
        fv(0) = _fNewHist
        For k As Integer = 1 To m
            xs(k) = _tHist(k - 1)
            fv(k) = _fHist(k - 1)
        Next

        Dim errVec As New NVector(_n)
        For i As Integer = 0 To _n - 1
            Dim tab(m) As Double
            For k As Integer = 0 To m
                tab(k) = fv(k)(i)
            Next
            For j As Integer = 1 To m
                For k As Integer = 0 To m - j
                    tab(k) = (tab(k + 1) - tab(k)) / (xs(k + j) - xs(k))
                Next
            Next
            errVec(i) = cam * std.Pow(h, p + 1) * Factorial(p) * tab(0)
        Next

        Return errVec.WRMSNorm(_ewt)
    End Function

#End Region

#Region "Jacobian 与线性系统"

    Private Function ComputeJacobian(t As Double, yRef As NVector) As CVODEStatus
        If _options.UseUserJacobian AndAlso _jacobianFunc IsNot Nothing Then
            _jacobianFunc(t, yRef, _ydot, _J)
        Else
            ComputeNumericalJacobian(t, yRef)
        End If
        Return CVODEStatus.Success
    End Function

    ''' <summary>前向差分数值 Jacobian：J(i,j) = (f_i(y+e_j) - f_i(y)) / e_j。</summary>
    Private Sub ComputeNumericalJacobian(t As Double, yRef As NVector)
        _rhsFunc(t, yRef, _tempV3)
        _nRHSEvals += 1

        Const DIFF_FACTOR As Double = 0.00000001
        For j As Integer = 0 To _n - 1
            Dim yj As Double = yRef(j)
            Dim delta As Double = std.Max(DIFF_FACTOR * std.Abs(yj), DIFF_FACTOR)
            _tempV2.CopyFrom(yRef)
            _tempV2(j) = yj + delta
            _rhsFunc(t, _tempV2, _fNewHist)
            _nRHSEvals += 1
            For i As Integer = 0 To _n - 1
                _J(i, j) = (_fNewHist(i) - _tempV3(i)) / delta
            Next
        Next
        ' 保存 base f 供后续使用
        _ydot.CopyFrom(_tempV3)
    End Sub

    ''' <summary>构造 A = I - gamma*J 并 LU 分解。</summary>
    Private Function BuildAndFactorizeSystem(gamma As Double) As LinearSolverResult
        For i As Integer = 0 To _n - 1
            For j As Integer = 0 To _n - 1
                If i = j Then
                    _A(i, j) = 1.0 - gamma * _J(i, j)
                Else
                    _A(i, j) = -gamma * _J(i, j)
                End If
            Next
        Next
        Return _linearSolver.Factorize(_A)
    End Function

#End Region

#Region "Lagrange / 多项式工具"

    ''' <summary>Lagrange 基函数 L_j(x) = Π_{m≠j} (x - x_m)/(x_j - x_m)。</summary>
    Private Function LagrangeBasisValue(nodes() As Double, j As Integer, x As Double) As Double
        Dim val As Double = 1.0
        For m As Integer = 0 To nodes.Length - 1
            If m <> j Then
                val *= (x - nodes(m)) / (nodes(j) - nodes(m))
            End If
        Next
        Return val
    End Function

    ''' <summary>
    ''' Lagrange 基函数在 x 处的导数（乘积法则形式，对任意 x 包括节点均正确）。
    '''   L'_j(x) = Σ_{k≠j} [ 1/(x_j - x_k) · Π_{m≠j,k} (x - x_m)/(x_j - x_m) ]
    ''' 注意：原公式 L_j(x)·Σ_{m≠j}1/(x-x_m) 在 x 与其它节点重合时因 L_j(x)=0
    ''' 而给出错误结果，BDF 中 x=tNew 正是节点，故此处改用乘积法则。
    ''' </summary>
    Private Function LagrangeBasisDeriv(nodes() As Double, j As Integer, x As Double) As Double
        Dim s As Double = 0.0
        For k As Integer = 0 To nodes.Length - 1
            If k = j Then Continue For
            Dim prod As Double = 1.0 / (nodes(j) - nodes(k))
            For m As Integer = 0 To nodes.Length - 1
                If m = j OrElse m = k Then Continue For
                prod *= (x - nodes(m)) / (nodes(j) - nodes(m))
            Next
            s += prod
        Next
        Return s
    End Function

    ''' <summary>小整数阶乘（q ≤ BDF_MAX_ORDER=5，足够）。</summary>
    Private Function Factorial(k As Integer) As Double
        Dim r As Double = 1.0
        For i As Integer = 2 To k
            r *= i
        Next
        Return r
    End Function

    ''' <summary>∫_{a}^{b} L_j(τ) dτ，通过展开单根多项式后逐项积分。</summary>
    Private Function IntegrateLagrangeBasis(nodes() As Double, j As Integer, a As Double, b As Double) As Double
        ' 单个节点时 L_j(τ) ≡ 1，积分即为区间长度
        If nodes.Length = 1 Then
            Return b - a
        End If
        ' 构造单根多项式 Π_{m≠j} (τ - x_m) 的系数（升幂）
        Dim roots(nodes.Length - 2) As Double
        Dim idx As Integer = 0
        For m As Integer = 0 To nodes.Length - 1
            If m <> j Then
                roots(idx) = nodes(m)
                idx += 1
            End If
        Next
        Dim mono() As Double = PolyFromRoots(roots)
        Dim denom As Double = 1.0
        For m As Integer = 0 To nodes.Length - 1
            If m <> j Then
                denom *= (nodes(j) - nodes(m))
            End If
        Next
        If std.Abs(denom) < ZERO_THRESHOLD Then
            Return 0.0
        End If
        Dim anti() As Double = PolyIntegrate(mono)
        Dim val As Double = (PolyEval(anti, b) - PolyEval(anti, a)) / denom
        Return val
    End Function

    ''' <summary>由根构造多项式系数（升幂）：Π (τ - root)。</summary>
    Private Function PolyFromRoots(roots() As Double) As Double()
        Dim result() As Double = {1.0}
        For Each r As Double In roots
            ' 乘以 (τ - r) = (-r) + 1*τ
            result = PolyMul(result, New Double() {-r, 1.0})
        Next
        Return result
    End Function

    ''' <summary>多项式乘法（卷积，升幂）。</summary>
    Private Function PolyMul(p() As Double, q() As Double) As Double()
        Dim r(p.Length + q.Length - 1) As Double
        For i As Integer = 0 To p.Length - 1
            For j As Integer = 0 To q.Length - 1
                r(i + j) += p(i) * q(j)
            Next
        Next
        Return r
    End Function

    ''' <summary>多项式不定积分（升幂，常数项为 0）。</summary>
    Private Function PolyIntegrate(p() As Double) As Double()
        Dim anti(p.Length) As Double
        For i As Integer = 0 To p.Length - 1
            anti(i + 1) = p(i) / (i + 1)
        Next
        Return anti
    End Function

    ''' <summary>Horner 法求值多项式。</summary>
    Private Function PolyEval(p() As Double, x As Double) As Double
        Dim s As Double = 0.0
        For i As Integer = p.Length - 1 To 0 Step -1
            s = s * x + p(i)
        Next
        Return s
    End Function

#End Region

#Region "设置方法"

    Public Sub SetJacobianFunction(jacFunc As JacobianFunction)
        _jacobianFunc = jacFunc
        _options.UseUserJacobian = True
    End Sub

    Public Sub SetAbsoluteTolerance(atol As Double)
        If atol <= 0 Then
            Throw New ArgumentException("绝对误差容差必须为正数", NameOf(atol))
        End If
        _options.AbsoluteTolerance = atol
        _atol = NVector.Constant(_n, atol)
    End Sub

    Public Sub SetAbsoluteTolerance(atol As NVector)
        If atol Is Nothing OrElse atol.Length <> _n Then
            Throw New ArgumentException("绝对误差容差向量维度不匹配")
        End If
        _atol = New NVector(atol)
    End Sub

    Public Sub SetRelativeTolerance(rtol As Double)
        If rtol <= 0 Then
            Throw New ArgumentException("相对误差容差必须为正数", NameOf(rtol))
        End If
        _options.RelativeTolerance = rtol
    End Sub

    Public Sub SetMaxStep(hMax As Double)
        If hMax < 0 Then
            Throw New ArgumentException("最大步长不能为负", NameOf(hMax))
        End If
        _options.MaxStep = hMax
    End Sub

    Public Sub SetMinStep(hMin As Double)
        If hMin < 0 Then
            Throw New ArgumentException("最小步长不能为负", NameOf(hMin))
        End If
        _options.MinStep = hMin
    End Sub

#End Region

#Region "IDisposable 实现"

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _isDisposed Then
            If disposing Then
                _y = Nothing
                _ydot = Nothing
                _ewt = Nothing
                _atol = Nothing
                _tempV = Nothing
                _tempV2 = Nothing
                _tempV3 = Nothing
                _delta = Nothing
                _knownSumV = Nothing
                _fNewHist = Nothing
                _yHist = Nothing
                _fHist = Nothing
                _linearSolver = Nothing
                _J = Nothing
                _A = Nothing
            End If
            _isDisposed = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class

