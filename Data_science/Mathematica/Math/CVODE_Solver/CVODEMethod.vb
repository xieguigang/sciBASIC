
''' <summary>
''' 求解方法枚举
''' </summary>
Public Enum CVODEMethod
    ''' <summary>
    ''' Adams-Bashforth-Moulton预测-校正方法
    ''' 适用于非刚性问题
    ''' </summary>
    Adams
    ''' <summary>
    ''' 后向微分公式（BDF）方法
    ''' 适用于刚性问题
    ''' </summary>
    BDF
End Enum