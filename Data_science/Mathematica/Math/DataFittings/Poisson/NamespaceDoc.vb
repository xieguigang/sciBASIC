Namespace Poisson

    ''' <summary>
    ''' 泊松回归（Poisson regression）：面向非负整数计数型响应变量的广义线性模型。
    ''' 
    ''' 模型使用对数链接函数 ``log(mu) = b0 + b1*x1 + ... + bp*xp + log(exposure)``，
    ''' 通过极大似然估计（IRLS / Newton-Raphson）迭代求解系数，并提供发生率比
    ''' （IRR）、系数标准误与 Z 值，以及残差偏差、离散比等过度离散诊断量。
    ''' 
    ''' 本命名空间只实现标准泊松回归及其诊断，不覆盖准泊松、负二项回归、
    ''' 零膨胀（ZIP / ZINB）与 Hurdle 等扩展模型；当
    ''' <see cref="PoissonFit.IsOverDispersed"/> 为真或者零值比例显著偏高时，
    ''' 应改用这些扩展模型重新建模。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
