Namespace Ensemble

    ''' <summary>
    ''' 统一的分类器输出契约。
    ''' </summary>
    ''' <remarks>
    ''' <c>PredictProbability</c> 返回的向量长度必须等于 <c>Classes.Length</c>，
    ''' 且顺序严格对齐 <c>Classes</c>；这是 Voting / Bagging / Stacking
    ''' 能够正确聚合概率的前提。
    ''' </remarks>
    Public Interface IClassifier

        ''' <summary>类别标签，升序排列</summary>
        ReadOnly Property Classes As Double()

        ''' <summary>
        ''' 与 <c>Classes</c> 顺序严格对齐的后验概率。
        ''' </summary>
        ''' <param name="x">单样本特征向量</param>
        Function PredictProbability(x As Double()) As Double()

    End Interface

    ''' <summary>统一的回归器输出契约（单输出标量）</summary>
    Public Interface IRegressor

        ''' <summary>对单样本给出回归预测值</summary>
        Function Predict(x As Double()) As Double

    End Interface

    ''' <summary>
    ''' 可报告训练特征维度的模型契约。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 集成框架在 <c>Train</c> 时记录特征维度，预测时逐样本校验，从而把
    ''' 「维度错位」从静默产出错误结果变成显式异常。
    ''' </para>
    ''' <para>
    ''' 返回 <c>0</c> 表示维度未知（例如由外部二进制模型包装、无法确知输入宽度的情形），
    ''' 此时调用方应跳过维度校验而不是把 <c>0</c> 当作合法维度。
    ''' </para>
    ''' </remarks>
    Public Interface IFeatureSchema

        ''' <summary>训练期使用的特征维度；<c>0</c> 表示未知</summary>
        ReadOnly Property FeatureWidth As Integer

    End Interface

    ''' <summary>
    ''' 可训练的分类基学习器。
    ''' </summary>
    ''' <remarks>
    ''' <c>Fit</c> 接收的 <c>yIndex</c> 已经是类别索引（取值范围
    ''' <c>[0, classes.Length)</c>），<c>classes</c> 是全局类别顺序；
    ''' 适配器不需要再做标签编码，因此不可能与集成层的类别顺序错位。
    ''' </remarks>
    Public Interface IClassifierLearner

        ''' <summary>用于诊断/日志的算法名称</summary>
        ReadOnly Property Name As String

        ''' <summary>训练并返回分类器</summary>
        ''' <param name="x">特征矩阵，每个元素是一行样本</param>
        ''' <param name="yIndex">类别索引，与 <paramref name="x"/> 行一一对应</param>
        ''' <param name="classes">全局类别标签顺序</param>
        Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier

    End Interface

    ''' <summary>可训练的回归基学习器</summary>
    Public Interface IRegressorLearner

        ''' <summary>用于诊断/日志的算法名称</summary>
        ReadOnly Property Name As String

        ''' <summary>训练并返回回归器</summary>
        ''' <param name="x">特征矩阵，每个元素是一行样本</param>
        ''' <param name="y">连续响应值，与 <paramref name="x"/> 行一一对应</param>
        Function Fit(x As Double()(), y As Double()) As IRegressor

    End Interface

End Namespace
