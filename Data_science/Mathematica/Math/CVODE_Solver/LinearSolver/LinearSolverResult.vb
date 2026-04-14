

''' <summary>
''' 线性求解器返回状态
''' </summary>
Public Enum LinearSolverResult
    ''' <summary>
    ''' 求解成功
    ''' </summary>
    Success = 0
    ''' <summary>
    ''' 矩阵奇异
    ''' </summary>
    SingularMatrix = -1
    ''' <summary>
    ''' 求解失败
    ''' </summary>
    SolveFailed = -2
    ''' <summary>
    ''' 内存分配失败
    ''' </summary>
    MemoryFail = -3
End Enum