#Region "Microsoft.VisualBasic::EvaluationReport, Data_science\DataMining\DataMining\Evaluation\EvaluationReport.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Evaluation

    ''' <summary>
    ''' 评估结果数据的形式（对应统一评估框架所支持的三类结果）。
    ''' </summary>
    Public Enum ResultKinds
        ''' <summary>
        ''' 机器学习分类结果：``(分数, 0/1 标签)``
        ''' </summary>
        classification
        ''' <summary>
        ''' 回归结果：``(预测值, 连续真值)``
        ''' </summary>
        regression
        ''' <summary>
        ''' 聚类结果：``(特征矩阵, 簇标签)``，可附带真值标签
        ''' </summary>
        clustering
    End Enum

    ''' <summary>
    ''' 统一评估框架的输入契约：任何可被评估的结果数据都需要声明自己的种类与名字。
    ''' </summary>
    Public Interface IEvaluationResult

        ''' <summary>
        ''' 结果数据的种类。
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Kind As ResultKinds

        ''' <summary>
        ''' 结果数据的名字（用于报告）。
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Name As String

    End Interface

    ''' <summary>
    ''' 能够产生 ROC 曲线的结果数据（分类与回归结果）。
    ''' </summary>
    Public Interface IRocResult

        ''' <summary>
        ''' 构建该结果的 ROC 曲线。
        ''' </summary>
        ''' <returns></returns>
        Function Curve() As RocCurve

    End Interface

    ''' <summary>
    ''' 统一评估报告：一个结果数据在统一框架之下计算出的全部指标。
    ''' </summary>
    Public Class EvaluationReport

        ''' <summary>
        ''' 结果数据的种类。
        ''' </summary>
        ''' <returns></returns>
        Public Property Kind As ResultKinds

        ''' <summary>
        ''' 被评估对象的名字。
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        ''' <summary>
        ''' 命名指标字典（指标名 → 指标值）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Metrics As New Dictionary(Of String, Double)

        ''' <summary>
        ''' 可选的 ROC 曲线（分类/回归结果会产生；聚类结果为 Nothing）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Curve As RocCurve

        ''' <summary>
        ''' 读取指定名字的指标值；当指标不存在的时候返回 ``0``。
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function Metric(name As String) As Double
            If Metrics IsNot Nothing AndAlso Metrics.ContainsKey(name) Then
                Return Metrics(name)
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' 是否包含指定名字的指标。
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function HasMetric(name As String) As Boolean
            Return Metrics IsNot Nothing AndAlso Metrics.ContainsKey(name)
        End Function

        ''' <summary>
        ''' 把命名指标导出为普通字典（便于序列化）。
        ''' </summary>
        ''' <returns></returns>
        Public Function ToDataSet() As Dictionary(Of String, Double)
            Return New Dictionary(Of String, Double)(Metrics)
        End Function

        Public Overrides Function ToString() As String
            Dim text As String = Metrics _
                .OrderBy(Function(kv) kv.Key) _
                .Select(Function(kv) $"{kv.Key}={kv.Value.ToString("G6")}") _
                .JoinBy(", ")

            Return $"[{Kind}] {Name}: {text}"
        End Function

        ''' <summary>
        ''' 以 JSON 文本的形式输出报告（便于日志记录）。
        ''' </summary>
        ''' <returns></returns>
        Public Function ToJson() As String
            Return Metrics.GetJson
        End Function

    End Class

End Namespace

#End Region
