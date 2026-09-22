#Region "Microsoft.VisualBasic::ClassificationResult, Data_science\DataMining\DataMining\Evaluation\Results\ClassificationResult.vb"

Imports System.Linq

Namespace Evaluation

    ''' <summary>
    ''' 机器学习分类结果的统一数据模型：``(分数, 0/1 标签)``。
    ''' 
    ''' 该类型替代了原先散落在 <see cref="Metric.auc"/>、<see cref="ROC.AUC(Double(), Double())"/>
    ''' 等处的裸 ``Double()`` 参数组合，是统一评估框架之中分类结果的标准输入。
    ''' </summary>
    Public Class ClassificationResult : Implements IEvaluationResult
        Implements IRocResult

        ''' <summary>
        ''' 结果名字。
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String Implements IEvaluationResult.Name

        ''' <summary>
        ''' 模型的连续打分（分数越大越倾向于正类）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Scores As Double()

        ''' <summary>
        ''' 真实标签。可以被二值化为 ``0/1``，也可以是连续值（此时按照 <see cref="Cutoff"/> 判定）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Labels As Double()

        ''' <summary>
        ''' 判定正类的阈值，默认为 0.5。
        ''' </summary>
        ''' <returns></returns>
        Public Property Cutoff As Double = 0.5

        ''' <summary>
        ''' 是否在报告中附带上由 <see cref="Curve"/> 产生的 ROC 曲线。
        ''' </summary>
        ''' <returns></returns>
        Public Property IncludeRoc As Boolean = True

        Private _curve As RocCurve

        Public ReadOnly Property Kind As ResultKinds Implements IEvaluationResult.Kind
            Get
                Return ResultKinds.classification
            End Get
        End Property

        ''' <summary>
        ''' 样本数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return If(Scores Is Nothing, 0, Scores.Length)
            End Get
        End Property

        ''' <summary>
        ''' 二值化之后的真实标签。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PositiveLabels As Boolean()
            Get
                If Labels Is Nothing Then
                    Return New Boolean() {}
                End If

                Return Labels.Select(Function(v) v >= Cutoff).ToArray
            End Get
        End Property

        ''' <summary>
        ''' 由「分数 + 标签」构建分类结果。
        ''' </summary>
        ''' <param name="scores"></param>
        ''' <param name="labels"></param>
        ''' <param name="name"></param>
        ''' <param name="cutoff"></param>
        ''' <returns></returns>
        Public Shared Function Create(scores As Double(),
                                      labels As Double(),
                                      Optional name As String = Nothing,
                                      Optional cutoff As Double = 0.5) As ClassificationResult

            If scores Is Nothing OrElse labels Is Nothing Then
                Throw New ArgumentNullException("the scores and labels vectors can not be nothing!")
            End If

            If scores.Length <> labels.Length Then
                Throw New DataMisalignedException($"the scores({scores.Length}) and labels({labels.Length}) vectors must have the same length!")
            End If

            Return New ClassificationResult With {
                .Name = If(String.IsNullOrEmpty(name), "classification", name),
                .Scores = scores,
                .Labels = labels,
                .Cutoff = cutoff
            }
        End Function

        ''' <summary>
        ''' 构建 ROC 曲线（统一委托到 <see cref="RocBuilder"/>）。结果会被缓存，避免重复计算。
        ''' </summary>
        ''' <returns></returns>
        Public Function Curve() As RocCurve Implements IRocResult.Curve
            If _curve Is Nothing Then
                _curve = RocBuilder.FromScores(Scores, Labels, Cutoff)
            End If

            Return _curve
        End Function

        ''' <summary>
        ''' 在给定阈值之上的混淆矩阵。
        ''' </summary>
        ''' <param name="cutoff"></param>
        ''' <returns></returns>
        Public Function Confusion(Optional cutoff As Double = 0.5) As Validation
            Return Validation.Calc(
                entity:=Enumerable.Range(0, Size).ToArray,
                getValidate:=Function(i) Labels(i) >= Cutoff,
                getPredict:=Function(i) Scores(i) >= cutoff,
                percentile:=cutoff)
        End Function

    End Class

End Namespace

#End Region
