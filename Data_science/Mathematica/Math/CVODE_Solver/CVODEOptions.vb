
''' <summary>
''' CVODE求解器配置选项
''' </summary>
Public Class CVODEOptions
    ''' <summary>
    ''' 相对误差容差
    ''' </summary>
    Public Property RelativeTolerance As Double = 0.0001

    ''' <summary>
    ''' 绝对误差容差（标量或向量）
    ''' </summary>
    Public Property AbsoluteTolerance As Double = 0.00000001

    ''' <summary>
    ''' 最大阶数（Adams: 1-12, BDF: 1-5）
    ''' </summary>
    Public Property MaxOrder As Integer = 5

    ''' <summary>
    ''' 最大步数
    ''' </summary>
    Public Property MaxSteps As Integer = 10000

    ''' <summary>
    ''' 最大步长（0表示自动）
    ''' </summary>
    Public Property MaxStep As Double = 0.0

    ''' <summary>
    ''' 最小步长（0表示自动）
    ''' </summary>
    Public Property MinStep As Double = 0.0

    ''' <summary>
    ''' 初始步长（0表示自动）
    ''' </summary>
    Public Property InitialStep As Double = 0.0

    ''' <summary>
    ''' Newton迭代最大次数
    ''' </summary>
    Public Property MaxNewtonIterations As Integer = 4

    ''' <summary>
    ''' Newton迭代收敛因子
    ''' </summary>
    Public Property NewtonConvergenceFactor As Double = 0.1

    ''' <summary>
    ''' 步长增长因子上限
    ''' </summary>
    Public Property MaxGrowthFactor As Double = 10.0

    ''' <summary>
    ''' 步长缩减因子下限
    ''' </summary>
    Public Property MinReductionFactor As Double = 0.2

    ''' <summary>
    ''' 安全因子
    ''' </summary>
    Public Property SafetyFactor As Double = 0.9

    ''' <summary>
    ''' 是否使用用户提供的Jacobian
    ''' </summary>
    Public Property UseUserJacobian As Boolean = False

    ''' <summary>
    ''' Jacobian更新策略
    ''' </summary>
    Public Property JacobianUpdateFrequency As Integer = 20
End Class
