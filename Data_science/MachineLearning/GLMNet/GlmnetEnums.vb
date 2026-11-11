' glmnet 5.1 复刻所使用的公共枚举类型。
' 命名空间为工程根命名空间 GLMNet；VB 大小写不敏感，因此所有类型名都刻意
' 避免与模块名、属性名或局部变量名仅靠大小写区分。

''' <summary>glmnet 支持的六种模型家族。</summary>
Public Enum FamilyKind
    ''' <summary>单响应线性回归（elnet）</summary>
    Gaussian = 0
    ''' <summary>逻辑回归（lognet）</summary>
    Binomial = 1
    ''' <summary>对数链接泊松回归（fishnet）</summary>
    Poisson = 2
    ''' <summary>多分类 softmax 回归（multnet）</summary>
    Multinomial = 3
    ''' <summary>Cox 比例风险模型（coxnet）</summary>
    Cox = 4
    ''' <summary>多响应高斯模型（mrelnet）</summary>
    MGaussian = 5
End Enum

''' <summary><c>predict.glmnet</c> 的 <c>type</c> 参数。</summary>
Public Enum PredictionType
    ''' <summary>线性预测子（link scale）</summary>
    Link
    ''' <summary>响应尺度（概率 / 期望计数 / 原始响应）</summary>
    Response
    ''' <summary>系数矩阵 <c>(p+1) x K</c></summary>
    Coefficients
    ''' <summary>非零系数的变量下标</summary>
    NonZero
    ''' <summary>分类标签（binomial / multinomial）</summary>
    [Class]
End Enum

''' <summary><c>cv.glmnet</c> 的 <c>type.measure</c> 参数。</summary>
Public Enum CVMeasure
    ''' <summary>均方误差（gaussian / mgaussian 默认）</summary>
    Mse
    ''' <summary>平均绝对误差（mgaussian）</summary>
    Mae
    ''' <summary>偏差（binomial / poisson / multinomial / cox 默认）</summary>
    Deviance
    ''' <summary>误分类率（binomial / multinomial）</summary>
    [Class]
    ''' <summary>ROC 曲线下面积（binomial）</summary>
    Auc
    ''' <summary>Harrell C 指数（cox，越大越好）</summary>
    CIndex
End Enum

''' <summary>glmnet 的错误码（对应 R 端 <c>jerr</c>）。0 表示正常收敛。</summary>
Public Enum GlmnetError
    ''' <summary>无错误</summary>
    None = 0
    ''' <summary>输入数据不合法（维度、NaN、Infinity）</summary>
    InvalidInput = 1001
    ''' <summary>参数不合法（alpha / lambda / 权重 / 惩罚因子 / 限值）</summary>
    InvalidOptions = 1002
    ''' <summary>权重之和非正或全部为零响应</summary>
    DegenerateWeights = 1003
    ''' <summary>坐标下降未能在 maxit 之内收敛</summary>
    NonConvergence = 1004
    ''' <summary>数值溢出 / 非有限值</summary>
    NumericalOverflow = 1005
    ''' <summary>该 family 不支持所请求的选项</summary>
    UnsupportedOption = 1006
    ''' <summary>内存 / 路径长度越界</summary>
    PathLimit = 1007
End Enum

''' <summary><c>type.gaussian</c>：n &lt; p 时是否走协方差更新路径。</summary>
Public Enum GaussianAlgorithm
    ''' <summary>n &lt; p 时使用 <c>X'X</c> 协方差更新（glmnet 默认）</summary>
    Covariance
    ''' <summary>始终使用朴素残差更新</summary>
    Naive
End Enum

''' <summary><c>type.multinomial</c>：多分类系数的分组惩罚方式。</summary>
Public Enum MultinomialGrouping
    ''' <summary>组-lasso，变量对全部类别同进同出（默认）</summary>
    Grouped
    ''' <summary>逐类别独立 lasso 惩罚</summary>
    Ungrouped
End Enum

''' <summary><c>cox.ties</c>：平局处理方式。glmnet 5.1 的默认值为 Efron。</summary>
Public Enum CoxTiesMode
    ''' <summary>Efron 近似（glmnet 5.1 默认）</summary>
    Efron
    ''' <summary>Breslow 近似</summary>
    Breslow
End Enum

''' <summary><c>type.logistic</c>：二项式 IRLS 的工作权重形式。</summary>
Public Enum LogisticAlgorithm
    ''' <summary>牛顿法（使用 Hessian 对角）</summary>
    Newton
    ''' <summary>修正 Newton（上界化权重，更稳健）</summary>
    ModifiedNewton
End Enum
