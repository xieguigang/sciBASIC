Imports std = System.Math

' ============================================================================
' CVODE.vb - CVODE常微分方程求解器核心模块
' 实现变阶变步长的多步方法求解常微分方程组
' 支持Adams方法（非刚性问题）和BDF方法（刚性问题）
' 仅基于.NET基础数学函数库实现，不依赖第三方库
' ============================================================================

''' <summary>
''' CVODE常微分方程求解器
''' 实现变阶变步长的多步方法
''' </summary>
Public Class CVODESolver : Implements IDisposable

#Region "常量定义"

    ' Adams方法最大阶数
    Private Const ADAMS_MAX_ORDER As Integer = 12
    ' BDF方法最大阶数
    Private Const BDF_MAX_ORDER As Integer = 5
    ' 默认相对误差
    Private Const DEFAULT_REL_TOL As Double = 0.0001
    ' 默认绝对误差
    Private Const DEFAULT_ABS_TOL As Double = 0.00000001
    ' 最小步长因子
    Private Const MIN_H_FACTOR As Double = 0.1
    ' 最大步长因子
    Private Const MAX_H_FACTOR As Double = 10.0
    ' Newton迭代收敛阈值
    Private Const NEWTON_CONV_RATE As Double = 0.3
    ' 零阈值
    Private Const ZERO_THRESHOLD As Double = 0.000000000000001

#End Region

#Region "私有字段"

    ' 问题维度
    Private _n As Integer
    ' 求解方法
    Private _method As CVODEMethod
    ' 配置选项
    Private _options As CVODEOptions

    ' 右端函数
    Private _rhsFunc As RHSFunction
    ' Jacobian函数（可选）
    Private _jacobianFunc As JacobianFunction

    ' 当前时间
    Private _t As Double
    ' 当前状态
    Private _y As NVector
    ' 当前导数
    Private _ydot As NVector

    ' 当前步长
    Private _h As Double
    ' 当前阶数
    Private _q As Integer
    ' 当前步数
    Private _nSteps As Long
    ' 右端函数调用次数
    Private _nRHSEvals As Long
    ' Newton迭代次数
    Private _nNewtonIters As Long
    ' 线性求解次数
    Private _nLinearSolves As Long

    ' 历史数据存储（Nordsieck数组）
    Private _zn As NVector()  ' 导数历史
    Private _znm As NVector() ' 状态历史

    ' 线性求解器
    Private _linearSolver As DenseLinearSolver
    ' Jacobian矩阵
    Private _J As DenseMatrix
    ' 预条件向量
    Private _tempV As NVector
    Private _tempV2 As NVector
    Private _tempV3 As NVector

    ' 误差权重向量
    Private _ewt As NVector
    ' 绝对误差容差向量
    Private _atol As NVector

    ' Newton迭代相关
    Private _gamma As Double
    Private _gammaInv As Double
    Private _prevGamma As Double

    ' 步长和阶数控制
    Private _eta As Double
    Private _etaMax As Double
    Private _etaMin As Double
    Private _etaThreshold As Double

    ' 系数表
    Private _adamsCoeffs As Double(,)
    Private _bdfCoeffs As Double(,)

    ' 初始化标志
    Private _isInitialized As Boolean
    ' 已释放标志
    Private _isDisposed As Boolean

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建CVODE求解器
    ''' </summary>
    ''' <param name="method">求解方法</param>
    ''' <param name="rhsFunc">右端函数</param>
    ''' <param name="n">问题维度</param>
    ''' <param name="options">配置选项（可选）</param>
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

        ' 验证最大阶数
        Dim maxOrder As Integer = If(_method = CVODEMethod.Adams, ADAMS_MAX_ORDER, BDF_MAX_ORDER)
        If _options.MaxOrder < 1 OrElse _options.MaxOrder > maxOrder Then
            _options.MaxOrder = maxOrder
        End If

        Initialize()
    End Sub

    ''' <summary>
    ''' 初始化求解器
    ''' </summary>
    Private Sub Initialize()
        ' 分配向量
        _y = New NVector(_n)
        _ydot = New NVector(_n)
        _ewt = New NVector(_n)
        _atol = NVector.Constant(_n, _options.AbsoluteTolerance)
        _tempV = New NVector(_n)
        _tempV2 = New NVector(_n)
        _tempV3 = New NVector(_n)

        ' 分配历史存储
        Dim maxOrder As Integer = _options.MaxOrder
        _zn = New NVector(maxOrder + 1) {}
        _znm = New NVector(maxOrder + 1) {}
        For i As Integer = 0 To maxOrder
            _zn(i) = New NVector(_n)
            _znm(i) = New NVector(_n)
        Next

        ' 分配线性求解器
        _linearSolver = New DenseLinearSolver(_n)
        _J = New DenseMatrix(_n, _n)

        ' 初始化系数表
        InitializeCoefficients()

        ' 初始化控制参数
        _eta = 1.0
        _etaMax = _options.MaxGrowthFactor
        _etaMin = _options.MinReductionFactor
        _etaThreshold = 0.9

        _isInitialized = True
    End Sub

    ''' <summary>
    ''' 初始化Adams和BDF系数表
    ''' </summary>
    Private Sub InitializeCoefficients()
        ' Adams系数（预测和校正）
        _adamsCoeffs = New Double(,) {
            {1.0, 0, 0, 0, 0, 0},
            {3.0 / 2.0, -1.0 / 2.0, 0, 0, 0, 0},
            {23.0 / 12.0, -16.0 / 12.0, 5.0 / 12.0, 0, 0, 0},
            {55.0 / 24.0, -59.0 / 24.0, 37.0 / 24.0, -9.0 / 24.0, 0, 0},
            {1901.0 / 720.0, -2774.0 / 720.0, 2616.0 / 720.0, -1274.0 / 720.0, 251.0 / 720.0, 0},
            {4277.0 / 1440.0, -7923.0 / 1440.0, 9982.0 / 1440.0, -7298.0 / 1440.0, 2877.0 / 1440.0, -475.0 / 1440.0}
        }

        ' BDF系数
        _bdfCoeffs = New Double(,) {
            {1.0, 0, 0, 0, 0, 0},
            {3.0 / 2.0, -1.0 / 2.0, 0, 0, 0, 0},
            {11.0 / 6.0, -7.0 / 6.0, 1.0 / 3.0, 0, 0, 0},
            {25.0 / 12.0, -23.0 / 12.0, 13.0 / 12.0, -1.0 / 4.0, 0, 0},
            {137.0 / 60.0, -163.0 / 60.0, 137.0 / 60.0, -21.0 / 20.0, 1.0 / 5.0, 0},
            {147.0 / 60.0, -213.0 / 60.0, 243.0 / 60.0, -183.0 / 60.0, 61.0 / 60.0, -1.0 / 6.0}
        }
    End Sub

#End Region

#Region "属性"

    ''' <summary>
    ''' 获取当前时间
    ''' </summary>
    Public ReadOnly Property CurrentTime As Double
        Get
            Return _t
        End Get
    End Property

    ''' <summary>
    ''' 获取当前状态向量
    ''' </summary>
    Public ReadOnly Property CurrentState As NVector
        Get
            Return _y
        End Get
    End Property

    ''' <summary>
    ''' 获取当前步长
    ''' </summary>
    Public ReadOnly Property CurrentStep As Double
        Get
            Return _h
        End Get
    End Property

    ''' <summary>
    ''' 获取当前阶数
    ''' </summary>
    Public ReadOnly Property CurrentOrder As Integer
        Get
            Return _q
        End Get
    End Property

    ''' <summary>
    ''' 获取总步数
    ''' </summary>
    Public ReadOnly Property TotalSteps As Long
        Get
            Return _nSteps
        End Get
    End Property

    ''' <summary>
    ''' 获取右端函数调用次数
    ''' </summary>
    Public ReadOnly Property RHSFunctionEvaluations As Long
        Get
            Return _nRHSEvals
        End Get
    End Property

    ''' <summary>
    ''' 获取Newton迭代次数
    ''' </summary>
    Public ReadOnly Property NewtonIterations As Long
        Get
            Return _nNewtonIters
        End Get
    End Property

    ''' <summary>
    ''' 获取线性求解次数
    ''' </summary>
    Public ReadOnly Property LinearSolves As Long
        Get
            Return _nLinearSolves
        End Get
    End Property

#End Region

#Region "初始化求解"

    ''' <summary>
    ''' 初始化求解器状态
    ''' </summary>
    ''' <param name="t0">初始时间</param>
    ''' <param name="y0">初始状态</param>
    ''' <returns>状态码</returns>
    Public Function Initialize(t0 As Double, y0 As NVector) As CVODEStatus
        If y0 Is Nothing OrElse y0.Length <> _n Then
            Return CVODEStatus.BadInput
        End If

        ' 设置初始状态
        _t = t0
        _y.CopyFrom(y0)

        ' 计算初始导数
        _rhsFunc(_t, _y, _ydot)
        _nRHSEvals += 1

        ' 初始化历史数据
        _zn(0).CopyFrom(_ydot)
        For i As Integer = 1 To _options.MaxOrder
            _zn(i).SetConstant(0.0)
        Next

        ' 初始化阶数
        _q = 1

        ' 估计初始步长
        Dim status As CVODEStatus = EstimateInitialStep()
        If status <> CVODEStatus.Success Then
            Return status
        End If

        ' 计算误差权重
        ComputeErrorWeights()

        ' 初始化gamma
        _gamma = 1.0
        _gammaInv = 1.0
        _prevGamma = 1.0

        ' 重置计数器
        _nSteps = 0
        _nNewtonIters = 0
        _nLinearSolves = 0

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' 估计初始步长
    ''' </summary>
    Private Function EstimateInitialStep() As CVODEStatus
        ' 如果用户指定了初始步长，使用用户值
        If _options.InitialStep > 0 Then
            _h = _options.InitialStep
            Return CVODEStatus.Success
        End If

        ' 基于初始导数估计步长
        Dim yNorm As Double = _y.WRMSNorm(_ewt)
        Dim ydotNorm As Double = _ydot.WRMSNorm(_ewt)

        If yNorm < ZERO_THRESHOLD AndAlso ydotNorm < ZERO_THRESHOLD Then
            yNorm = 1.0
            ydotNorm = 1.0
        ElseIf yNorm < ZERO_THRESHOLD Then
            yNorm = 1.0
        ElseIf ydotNorm < ZERO_THRESHOLD Then
            ydotNorm = 1.0
        End If

        ' 初始步长估计
        _h = 0.01 * yNorm / ydotNorm

        ' 限制步长范围
        If _options.MaxStep > 0 Then
            _h = std.Min(_h, _options.MaxStep)
        End If
        If _options.MinStep > 0 Then
            _h = std.Max(_h, _options.MinStep)
        End If

        ' 确保步长为正
        If _h <= 0 Then
            _h = 0.000001
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' 计算误差权重向量
    ''' </summary>
    Private Sub ComputeErrorWeights()
        For i As Integer = 0 To _n - 1
            Dim denom As Double = 1.0 / (_options.RelativeTolerance * std.Abs(_y(i)) + _atol(i))

            ' 防止除以极小值
            If denom < 0.000000000001 Then
                _ewt(i) = 1000000.0  ' 设置一个合理的上限
            Else
                _ewt(i) = 1.0 / denom
            End If
        Next
    End Sub

#End Region

#Region "主求解循环"

    ''' <summary>
    ''' 执行一步积分
    ''' </summary>
    ''' <param name="tOut">目标输出时间</param>
    ''' <returns>状态码</returns>
    Public Function [Step](tOut As Double) As CVODEStatus
        If Not _isInitialized Then
            Return CVODEStatus.BadInput
        End If

        Dim status As CVODEStatus

        ' 检查是否已到达目标时间
        If _t >= tOut Then
            Return CVODEStatus.Success
        End If

        ' 检查步数限制
        If _nSteps >= _options.MaxSteps Then
            Return CVODEStatus.TooManySteps
        End If

        ' 调整步长以精确到达tOut
        Dim hTarget As Double = tOut - _t
        If _h > hTarget Then
            _h = hTarget
        End If

        ' 执行一步
        status = TakeStep()

        If status = CVODEStatus.Success Then
            _nSteps += 1
        End If

        Return status
    End Function

    ''' <summary>
    ''' 积分到指定时间
    ''' </summary>
    ''' <param name="tOut">目标输出时间</param>
    ''' <param name="yOut">输出状态向量（可选）</param>
    ''' <returns>状态码</returns>
    Public Function Integrate(tOut As Double, Optional yOut As NVector = Nothing) As CVODEStatus
        Dim status As CVODEStatus

        ' 循环积分直到到达目标时间
        Do While _t < tOut
            status = [Step](tOut)
            If status <> CVODEStatus.Success Then
                Return status
            End If

            ' 检查步数限制
            If _nSteps >= _options.MaxSteps Then
                Return CVODEStatus.TooManySteps
            End If
        Loop

        ' 输出结果
        If yOut IsNot Nothing Then
            yOut.CopyFrom(_y)
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' 执行单步积分
    ''' </summary>
    Private Function TakeStep() As CVODEStatus
        Dim status As CVODEStatus

        ' 预测阶段
        status = Predict()
        If status <> CVODEStatus.Success Then
            Return status
        End If

        ' 校正阶段（Newton迭代）
        status = Correct()
        If status <> CVODEStatus.Success Then
            ' 校正失败，缩减步长重试
            _h *= 0.5
            If _h < ZERO_THRESHOLD Then
                Return CVODEStatus.StepTooSmall
            End If
            Return CVODEStatus.ConvFail
        End If

        ' 误差估计
        Dim errorNorm As Double = EstimateError()

        ' 步长和阶数控制
        If errorNorm > 1.0 Then
            ' 误差过大，缩减步长
            Dim factor As Double = _options.SafetyFactor / std.Pow(errorNorm, 1.0 / (_q + 1))
            factor = std.Max(factor, _options.MinReductionFactor)
            _h *= factor
            Return CVODEStatus.TestFail
        End If

        ' 更新历史数据
        UpdateHistory()

        ' 调整步长和阶数
        AdjustStepAndOrder(errorNorm)

        ' 更新误差权重
        ComputeErrorWeights()

        Return CVODEStatus.Success
    End Function

#End Region

#Region "预测-校正"

    ''' <summary>
    ''' 预测阶段：计算预测值
    ''' </summary>
    Private Function Predict() As CVODEStatus
        ' 使用多步公式预测
        If _method = CVODEMethod.Adams Then
            ' Adams预测
            PredictAdams()
        Else
            ' BDF预测
            PredictBDF()
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' Adams方法预测
    ''' </summary>
    Private Sub PredictAdams()
        ' 预测值: y_pred = y_n + h * sum_{i=0}^{q-1} beta_i * zn[i]
        _tempV.SetConstant(0.0)

        For i As Integer = 0 To _q - 1
            Dim coeff As Double = GetAdamsPredictorCoefficient(i)
            _tempV.LinearSumInPlace(coeff, _zn(i))
        Next

        _y.CopyFrom(_znm(0))
        _y.LinearSumInPlace(_h, _tempV)
    End Sub

    ''' <summary>
    ''' BDF方法预测
    ''' </summary>
    Private Sub PredictBDF()
        ' BDF预测值
        _tempV.SetConstant(0.0)

        For i As Integer = 0 To _q
            Dim coeff As Double = GetBDFCoefficient(i)
            _tempV.LinearSumInPlace(coeff, _znm(i))
        Next

        _y.CopyFrom(_tempV)
    End Sub

    ''' <summary>
    ''' 校正阶段：Newton迭代求解
    ''' </summary>
    Private Function Correct() As CVODEStatus
        Dim status As CVODEStatus

        ' 计算Jacobian矩阵
        status = ComputeJacobian()
        If status <> CVODEStatus.Success Then
            Return status
        End If

        ' 构造线性系统矩阵: (I - gamma*J)
        ConstructSystemMatrix()

        ' LU分解
        Dim linStatus As LinearSolverResult = _linearSolver.Factorize(_J)
        If linStatus <> LinearSolverResult.Success Then
            Return CVODEStatus.LinearSolveFail
        End If

        ' Newton迭代
        Dim converged As Boolean = False
        Dim newtonIter As Integer = 0
        Dim lastDeltaNorm As Double = Double.MaxValue

        ' 计算初始残差
        _rhsFunc(_t + _h, _y, _ydot)
        _nRHSEvals += 1

        Do While newtonIter < _options.MaxNewtonIterations
            newtonIter += 1
            _nNewtonIters += 1

            ' 计算残差: b = ydot_pred - ydot
            _tempV.CopyFrom(_zn(0))
            _tempV.SubtractVector(_ydot)

            ' 求解线性系统
            linStatus = _linearSolver.Solve(_tempV, _tempV2)
            _nLinearSolves += 1

            If linStatus <> LinearSolverResult.Success Then
                Return CVODEStatus.LinearSolveFail
            End If

            ' 更新解
            _y.AddVector(_tempV2)

            ' 20260414
            ' 重新计算误差权重（可选，但建议）
            ComputeErrorWeights()

            ' 检查收敛
            Dim deltaNorm As Double = _tempV2.WRMSNorm(_ewt)
            If deltaNorm < _options.NewtonConvergenceFactor Then
                converged = True
                Exit Do
            End If

            ' 检查是否发散
            If deltaNorm > 1000.0 * lastDeltaNorm Then
                Return CVODEStatus.ConvFail
            End If

            lastDeltaNorm = deltaNorm

            ' 重新计算导数
            _rhsFunc(_t + _h, _y, _ydot)
            _nRHSEvals += 1
        Loop

        If Not converged Then
            Return CVODEStatus.ConvFail
        End If

        ' 更新时间
        _t += _h

        Return CVODEStatus.Success
    End Function

#End Region

#Region "Jacobian计算"

    ''' <summary>
    ''' 计算Jacobian矩阵
    ''' </summary>
    Private Function ComputeJacobian() As CVODEStatus
        If _options.UseUserJacobian AndAlso _jacobianFunc IsNot Nothing Then
            ' 使用用户提供的Jacobian
            _jacobianFunc(_t + _h, _y, _ydot, _J)
        Else
            ' 数值差分计算Jacobian
            ComputeNumericalJacobian()
        End If

        Return CVODEStatus.Success
    End Function

    ''' <summary>
    ''' 数值差分计算Jacobian矩阵
    ''' 使用前向差分
    ''' </summary>
    Private Sub ComputeNumericalJacobian()
        Dim yTemp As NVector = _tempV3
        Dim fTemp As NVector = _tempV2

        ' 差分步长因子
        Const DIFF_FACTOR As Double = 0.00000001

        ' 保存当前导数
        _rhsFunc(_t + _h, _y, _ydot)
        _nRHSEvals += 1

        ' 逐列计算Jacobian
        For j As Integer = 0 To _n - 1
            ' 计算差分步长
            Dim yj As Double = _y(j)
            Dim delta As Double = std.Max(DIFF_FACTOR * std.Abs(yj), DIFF_FACTOR)

            ' 扰动
            yTemp.CopyFrom(_y)
            yTemp(j) = yj + delta

            ' 计算扰动后的导数
            _rhsFunc(_t + _h, yTemp, fTemp)
            _nRHSEvals += 1

            ' 计算Jacobian列
            For i As Integer = 0 To _n - 1
                _J(i, j) = (fTemp(i) - _ydot(i)) / delta
            Next
        Next
    End Sub

    ''' <summary>
    ''' 构造线性系统矩阵: I - gamma * J
    ''' </summary>
    Private Sub ConstructSystemMatrix()
        _gamma = _h * _gamma
        _gammaInv = 1.0 / _gamma

        ' 构造 I - gamma * J
        For i As Integer = 0 To _n - 1
            For j As Integer = 0 To _n - 1
                If i = j Then
                    _J(i, j) = 1.0 - _gamma * _J(i, j)
                Else
                    _J(i, j) = -_gamma * _J(i, j)
                End If
            Next
        Next
    End Sub

#End Region

#Region "误差估计"

    ''' <summary>
    ''' 估计局部截断误差
    ''' </summary>
    Private Function EstimateError() As Double
        ' 使用历史数据估计误差
        Dim errorVec As NVector = _tempV

        ' 误差估计: err = C_{q+1} * h^{q+1} * y^{(q+1)}
        ' 简化估计：使用最后一步的差分
        errorVec.CopyFrom(_zn(_q))
        errorVec.ScaleInPlace(_h)

        ' 计算加权RMS范数
        Return errorVec.WRMSNorm(_ewt)
    End Function

#End Region

#Region "步长和阶数控制"

    ''' <summary>
    ''' 更新历史数据
    ''' </summary>
    Private Sub UpdateHistory()
        ' 更新Nordsieck数组
        ' 移位历史数据
        For i As Integer = _q To 1 Step -1
            _zn(i).CopyFrom(_zn(i - 1))
            _znm(i).CopyFrom(_znm(i - 1))
        Next

        ' 更新最新数据
        _zn(0).CopyFrom(_ydot)
        _znm(0).CopyFrom(_y)
    End Sub

    ''' <summary>
    ''' 调整步长和阶数
    ''' </summary>
    Private Sub AdjustStepAndOrder(errorNorm As Double)
        ' 计算步长调整因子
        Dim factor As Double = 1.0

        If errorNorm > 0 Then
            factor = _options.SafetyFactor / std.Pow(errorNorm, 1.0 / (_q + 1))
            factor = std.Max(factor, _options.MinReductionFactor)
            factor = std.Min(factor, _options.MaxGrowthFactor)
        End If

        ' 更新步长
        _h *= factor

        ' 限制步长范围
        If _options.MaxStep > 0 Then
            _h = std.Min(_h, _options.MaxStep)
        End If
        If _options.MinStep > 0 Then
            _h = std.Max(_h, _options.MinStep)
        End If

        ' 阶数控制（简化版本）
        ' 如果误差很小，可以考虑增加阶数
        If errorNorm < 0.1 AndAlso _q < _options.MaxOrder Then
            _q += 1
        ElseIf errorNorm > 0.9 AndAlso _q > 1 Then
            _q -= 1
        End If
    End Sub

#End Region

#Region "系数获取"

    ''' <summary>
    ''' 获取Adams预测系数
    ''' </summary>
    Private Function GetAdamsPredictorCoefficient(i As Integer) As Double
        If i < 0 OrElse i >= _adamsCoeffs.GetLength(1) Then
            Return 0.0
        End If
        Dim qIdx As Integer = std.Min(_q - 1, _adamsCoeffs.GetLength(0) - 1)
        Return _adamsCoeffs(qIdx, i)
    End Function

    ''' <summary>
    ''' 获取BDF系数
    ''' </summary>
    Private Function GetBDFCoefficient(i As Integer) As Double
        If i < 0 OrElse i >= _bdfCoeffs.GetLength(1) Then
            Return 0.0
        End If
        Dim qIdx As Integer = std.Min(_q, _bdfCoeffs.GetLength(0) - 1)
        Return _bdfCoeffs(qIdx, i)
    End Function

#End Region

#Region "设置方法"

    ''' <summary>
    ''' 设置Jacobian函数
    ''' </summary>
    Public Sub SetJacobianFunction(jacFunc As JacobianFunction)
        _jacobianFunc = jacFunc
        _options.UseUserJacobian = True
    End Sub

    ''' <summary>
    ''' 设置绝对误差容差（标量）
    ''' </summary>
    Public Sub SetAbsoluteTolerance(atol As Double)
        If atol <= 0 Then
            Throw New ArgumentException("绝对误差容差必须为正数", NameOf(atol))
        End If
        _options.AbsoluteTolerance = atol
        _atol = NVector.Constant(_n, atol)
    End Sub

    ''' <summary>
    ''' 设置绝对误差容差（向量）
    ''' </summary>
    Public Sub SetAbsoluteTolerance(atol As NVector)
        If atol Is Nothing OrElse atol.Length <> _n Then
            Throw New ArgumentException("绝对误差容差向量维度不匹配")
        End If
        _atol = New NVector(atol)
    End Sub

    ''' <summary>
    ''' 设置相对误差容差
    ''' </summary>
    Public Sub SetRelativeTolerance(rtol As Double)
        If rtol <= 0 Then
            Throw New ArgumentException("相对误差容差必须为正数", NameOf(rtol))
        End If
        _options.RelativeTolerance = rtol
    End Sub

    ''' <summary>
    ''' 设置最大步长
    ''' </summary>
    Public Sub SetMaxStep(hMax As Double)
        If hMax < 0 Then
            Throw New ArgumentException("最大步长不能为负", NameOf(hMax))
        End If
        _options.MaxStep = hMax
    End Sub

    ''' <summary>
    ''' 设置最小步长
    ''' </summary>
    Public Sub SetMinStep(hMin As Double)
        If hMin < 0 Then
            Throw New ArgumentException("最小步长不能为负", NameOf(hMin))
        End If
        _options.MinStep = hMin
    End Sub

#End Region

#Region "IDisposable实现"

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _isDisposed Then
            If disposing Then
                ' 释放托管资源
                _y = Nothing
                _ydot = Nothing
                _ewt = Nothing
                _atol = Nothing
                _tempV = Nothing
                _tempV2 = Nothing
                _tempV3 = Nothing
                _zn = Nothing
                _znm = Nothing
                _linearSolver = Nothing
                _J = Nothing
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


