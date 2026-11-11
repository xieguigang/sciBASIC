''' <summary>
''' 六个 family 求解器共用的最小契约。
''' </summary>
Friend Interface IGlmnetFamily

    ''' <summary>该求解器负责的家族</summary>
    ReadOnly Property Kind As FamilyKind

    ''' <summary>在数据上拟合完整的 lambda 路径。</summary>
    Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath

    ''' <summary>
    ''' 计算折外评估指标。
    ''' </summary>
    ''' <param name="linkPredictions">
    ''' <c>(lambda step)(sample)(response)</c> 的 link scale 预测
    ''' </param>
    ''' <param name="truth">留出集的真实响应</param>
    ''' <param name="measure">评估指标</param>
    ''' <returns>每个 lambda 步骤的指标值</returns>
    Function Measure(linkPredictions As Double()()(), truth As GlmnetData, measure As CVMeasure) As Double()

    ''' <summary>把 link scale 预测转换为响应尺度。</summary>
    Function LinkToResponse(link As Double()()) As Double()()

    ''' <summary>该指标是越大越好（C 指数 / AUC），还是越小越好。</summary>
    Function IsMaximize(measure As CVMeasure) As Boolean

End Interface
