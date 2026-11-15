Namespace Ensemble

    ''' <summary>
    ''' 集成学习框架：把项目内异质的算法族统一到同一套分类/回归契约下，
    ''' 并提供 Voting、Bagging、Stacking 三类集成器。
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <b>契约</b>：分类器实现 <c>IClassifier</c>（<c>Classes</c> + 与之严格对齐的
    ''' <c>PredictProbability</c>），回归器实现 <c>IRegressor</c>（单输出标量）；
    ''' 可训练的算法通过 <c>IClassifierLearner</c> / <c>IRegressorLearner</c> 适配。
    ''' 集成器只依赖这些接口，因此新增算法只需要补一个适配器。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>类别对齐</b>：分类器的概率向量顺序必须等于 <c>Classes</c>。集成器在入口处
    ''' 用 <c>LabelEncoder</c> 把原始标签统一成升序类别数组与整数索引，
    ''' 适配器不再重复编码，避免多模型之间类别索引错位。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>可复现</b>：Bagging 的有放回采样与 Stacking 的 K 折划分都使用独立的
    ''' <c>Random(seed)</c> 实例；基模型训练默认串行，<c>Parallel</c> 开关默认关闭，
    ''' 避免与各算法库内部已有的批量并行形成嵌套。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>拒绝静默降级</b>：单类别训练集、概率全零、权重为负或和为零、
    ''' 特征行长度不齐、<c>X.Length</c> 与 <c>y.Length</c> 不等，全部抛出
    ''' 带上下文信息的异常。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>维度守卫</b>：集成器与适配器统一实现 <see cref="IFeatureSchema"/>，
    ''' 训练期记录特征维度，预测期逐样本校验，维度错位不再静默产出错误结果。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>可持久化</b>：<c>Ensemble.Persistence</c> 子命名空间提供
    ''' 「JSON 清单 + CSV 数值附表」的模型包格式。三类集成器实现
    ''' <c>IPersistableModel</c>（<c>Save</c> / <c>SaveModel</c>），
    ''' <c>EnsembleModelStore</c> 负责保存与加载的统一入口；
    ''' 加载走底层库的 <c>Restore</c> 工厂，<b>零重训</b>且预测值与保存前逐位一致。
    ''' 文件为纯文本，可读、可 diff、可被其它语言直接解析。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </remarks>
    Friend Class NamespaceDoc
    End Class
End Namespace
