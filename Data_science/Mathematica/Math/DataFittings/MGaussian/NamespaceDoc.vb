Namespace MGaussian

    ''' <summary>
    ''' 多响应高斯模型（multi-response Gaussian，glmnet 的 ``family = "mgaussian"``）。
    ''' 
    ''' 把 ``N x K`` 的响应矩阵与共享的特征矩阵联合拟合，对每个自变量的 K 维系数向量
    ''' 施加组-lasso（``l2``）惩罚，使变量在全部响应上「同进同出」（shared sparsity / 多任务学习）。
    ''' 本命名空间提供：组-lasso 块坐标下降求解器、自动 lambda 正则化路径（warm start）、
    ''' 单 lambda 与多 lambda 预测、变量 2-norm 系数路径导出，以及 K 折交叉验证
    ''' （mse / mae 与 lambda.min / lambda.1se）。
    ''' 
    ''' 注意：<see cref="IFitted"/> 是单响应契约（``GetY`` 返回标量），多响应结果模型
    ''' 不实现它，而是提供 <see cref="MGaussianFit.Predict"/>；若需要把某个响应的预测
    ''' 写回二维表，可用 <see cref="MGaussianFit.GetResponseModel"/> 取得逐响应的
    ''' <see cref="IFitted"/> 视图。绘图（``type.coef="2norm"`` / ``"coef"``）属于可视化，
    ''' 不在本库实现范围内，但 <see cref="MGaussianFit.toNumericTable"/> 会导出
    ''' 2-norm 路径表供绘图使用。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
