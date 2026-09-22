#Region "Microsoft.VisualBasic::ModelEvaluation, Data_science\DataMining\DataMining\Evaluation\ModelEvaluation.vb"

Namespace Evaluation

    ''' <summary>
    ''' 统一评估框架的入口（Facade）。
    ''' 
    ''' 对「机器学习分类结果 / 回归结果 / 聚类结果」三类结果数据，调用
    ''' <see cref="Evaluate(ClassificationResult, String)"/> 等重载即可得到统一的
    ''' <see cref="EvaluationReport"/>，其中包含该结果种类下所有已注册指标
    ''' （参见 <see cref="MetricRegistry"/>）以及可选的 ROC 曲线。
    ''' </summary>
    Public Module ModelEvaluation

        ''' <summary>
        ''' 评估机器学习分类结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="ClassificationResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As ClassificationResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估回归结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="RegressionResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As RegressionResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估聚类结果。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="name">可选的结果名字，覆盖 <see cref="ClusteringResult.Name"/></param>
        ''' <returns></returns>
        Public Function Evaluate(result As ClusteringResult, Optional name As String = Nothing) As EvaluationReport
            If Not String.IsNullOrEmpty(name) Then
                result.Name = name
            End If

            Return MetricRegistry.Compute(result)
        End Function

        ''' <summary>
        ''' 评估任意实现了 <see cref="IEvaluationResult"/> 的结果数据
        ''' （例如用户自定义的结果类型）。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Function Evaluate(result As IEvaluationResult) As EvaluationReport
            Return MetricRegistry.Compute(result)
        End Function

    End Module

End Namespace

#End Region
