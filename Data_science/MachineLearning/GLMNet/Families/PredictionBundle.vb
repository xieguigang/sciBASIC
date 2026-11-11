''' <summary>
''' <c>predict.glmnet</c> 的统一返回容器。
''' </summary>
''' <remarks>
''' 不同的 <see cref="PredictionType"/> 只会填充其中一组字段，其余保持为空；
''' 这样 <c>predict</c> 只有一个返回类型，调用方通过 <see cref="Type"/> 判别。
''' </remarks>
Public NotInheritable Class PredictionBundle

    ''' <summary>本次预测的类型</summary>
    Public Property Type As PredictionType
    ''' <summary>link / response 结果，形状 <c>n x k</c></summary>
    Public Property Values As Double()()
    ''' <summary>coefficients 结果，形状 <c>(p+1) x k</c>，第 0 行是截距</summary>
    Public Property Coefficients As Double()()
    ''' <summary>nonzero 结果：非零变量的 0 基下标</summary>
    Public Property Indices As Integer()
    ''' <summary>class 结果：每个样本的预测类别下标</summary>
    Public Property ClassIndex As Integer()
    ''' <summary>类别标签（multinomial）</summary>
    Public Property ClassLabels As String()
    ''' <summary>实际使用的 lambda 值</summary>
    Public Property Lambda As Double

    ''' <summary>生成一个 link/response 类型的结果。</summary>
    Public Shared Function FromValues(type As PredictionType, values As Double()(), lambda As Double) As PredictionBundle
        Return New PredictionBundle With {.Type = type, .Values = values, .Lambda = lambda}
    End Function

End Class
