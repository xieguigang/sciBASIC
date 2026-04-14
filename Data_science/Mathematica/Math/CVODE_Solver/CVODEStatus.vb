

''' <summary>
''' 求解器返回状态
''' </summary>
Public Enum CVODEStatus
    ''' <summary>
    ''' 成功
    ''' </summary>
    Success = 0
    ''' <summary>
    ''' 达到停止时间
    ''' </summary>
    TStopReturn = 1
    ''' <summary>
    ''' 根找到
    ''' </summary>
    RootReturn = 2
    ''' <summary>
    ''' 函数未定义
    ''' </summary>
    FuncUndefined = -1
    ''' <summary>
    ''' 步长过小
    ''' </summary>
    StepTooSmall = -2
    ''' <summary>
    ''' 测试失败
    ''' </summary>
    TestFail = -3
    ''' <summary>
    ''' 求解失败
    ''' </summary>
    SolveFailed = -4
    ''' <summary>
    ''' 收敛失败
    ''' </summary>
    ConvFail = -5
    ''' <summary>
    ''' 线性求解器初始化失败
    ''' </summary>
    LinearSolverInitFail = -6
    ''' <summary>
    ''' 线性求解失败
    ''' </summary>
    LinearSolveFail = -7
    ''' <summary>
    ''' 右端函数重复失败
    ''' </summary>
    RHSFuncFail = -8
    ''' <summary>
    ''' 首步失败
    ''' </summary>
    FirstStepFail = -9
    ''' <summary>
    ''' 步数超限
    ''' </summary>
    TooManySteps = -10
    ''' <summary>
    ''' 内存错误
    ''' </summary>
    MemoryError = -11
    ''' <summary>
    ''' 参数错误
    ''' </summary>
    BadInput = -12
End Enum
