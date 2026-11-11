Namespace Cox

    ''' <summary>
    ''' Cox 比例风险模型（glmnet 的 ``family = "cox"``），面向右删失生存数据。
    ''' 
    ''' 响应为 ``N x 2`` 矩阵：第 0 列 ``time``（观测时间）、第 1 列 ``status``
    ''' （1 = 事件、0 = 删失）。模型假设 ``h_i(t) = h0(t) * exp(x_i'beta)``，
    ''' 基线风险不设参数形式，系数由偏似然估计；**模型没有截距项**。
    ''' 本命名空间在负对数偏似然上施加弹性网惩罚，用循环坐标下降沿 lambda
    ''' 路径求解，支持 Breslow / Efron 平局处理、变量级惩罚因子、风险比 HR、
    ''' Harrell C 指数、基线累积风险与个体化生存曲线，以及 K 折交叉验证
    ''' （deviance / C 指数与 lambda.min / lambda.1se）。
    ''' 
    ''' 不覆盖：(start, stop] 区间/时变协变量数据、分层 Cox、稀疏矩阵输入、
    ''' 逐样本权重与图形绘制；生存曲线不带置信带（与 glmnet 一致）。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
