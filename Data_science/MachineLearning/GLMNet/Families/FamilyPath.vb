''' <summary>
''' family 求解器的内部输出：一条 lambda 路径上的全部解。
''' </summary>
''' <remarks>
''' 所有系数与截距都已还原到< b >原始量纲</b>，与 R 端 <c>coef(fit)</c> 一致；
''' <c>Beta(step)(feature)(response)</c> 为三维锯齿数组，单响应 family 的最后一维长度为 1。
''' </remarks>
Friend NotInheritable Class FamilyPath

    ''' <summary>每一步的 lambda（严格递减）</summary>
    Public Property Lambda As Double()
    ''' <summary><c>A0(step)(response)</c>，原始量纲截距</summary>
    Public Property A0 As Double()()
    ''' <summary><c>Beta(step)(feature)(response)</c>，原始量纲系数</summary>
    Public Property Beta As Double()()()
    ''' <summary>每一步的非零变量数（glmnet 的 df）</summary>
    Public Property Df As Integer()
    ''' <summary>每一步的 <c>1 - deviance/nulldev</c></summary>
    Public Property DevRatio As Double()
    ''' <summary>每一步的模型偏差</summary>
    Public Property Deviance As Double()
    ''' <summary>零模型偏差</summary>
    Public Property NullDev As Double
    ''' <summary>每一步的非零系数个数</summary>
    Public Property NZero As Integer()
    ''' <summary>每一步是否收敛</summary>
    Public Property Converged As Boolean()
    ''' <summary>每一步实际消耗的坐标下降轮数</summary>
    Public Property Iterations As Integer()
    ''' <summary>数值内核产生的错误码</summary>
    Public Property Jerr As GlmnetError

    Public Property Family As FamilyKind
    Public Property Options As GlmnetOptions
    Public Property FeatureNames As String()
    Public Property ResponseNames As String()
    Public Property ClassLabels As String()
    Public Property TrainingData As GlmnetData

    ''' <summary>路径步数</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return If(Lambda Is Nothing, 0, Lambda.Length)
        End Get
    End Property

End Class
