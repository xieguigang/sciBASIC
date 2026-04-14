
''' <summary>
''' 线性求解器类型枚举
''' </summary>
Public Enum LinearSolverType
    ''' <summary>
    ''' 稠密矩阵直接求解（LU分解）
    ''' </summary>
    Dense
    ''' <summary>
    ''' 带状矩阵求解
    ''' </summary>
    Band
    ''' <summary>
    ''' 对角矩阵求解
    ''' </summary>
    Diagonal
End Enum