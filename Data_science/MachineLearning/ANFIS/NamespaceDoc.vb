Option Strict On
Option Explicit On

''' <summary>
''' 自适应神经模糊推理系统（Adaptive Network-based Fuzzy Inference System，ANFIS）。
''' </summary>
''' <remarks>
''' <para>
''' ### 一、模型结构
''' </para>
''' <para>
''' 把 Takagi-Sugeno 模糊推理展开成五层前馈网络（<c>R</c> 条规则、<c>n</c> 个输入）：
''' </para>
''' <list type="number">
''' <item>**模糊化**：<c>mu[i,k] = exp(-(x_i - c_ik)^2 / (2*sigma_ik^2))</c>，见 <see cref="TskLayers"/>；</item>
''' <item>**规则激励**：<c>w_r = prod_i mu[i, m(i,r)]</c>（乘积 T-范数，对数域累加防下溢）；</item>
''' <item>**归一化**：<c>w-bar_r = w_r / sum_r w_r</c>；</item>
''' <item>**结论**：<c>f_r = x.p_r + q_r</c>（一阶 Sugeno，常数项 q 让每条规则自带截距）；</item>
''' <item>**输出**：<c>z = sum_r w-bar_r f_r</c>。因为用加权平均代替重心法去模糊，
''' 省掉了 Mamdani 系统里那一步数值积分。</item>
''' </list>
''' <para>
''' ### 二、为什么第 4、5 层要写成矩阵形式
''' </para>
''' <para>
''' 把每条规则的参数按块拼成 <c>theta</c>（<c>R*(n+1)</c> 个），第 4、5 层就等价于
''' <c>z = A * theta</c>，其中 <c>A</c> 的第 r 块是 <c>[w-bar_r*x_1, ..., w-bar_r*x_n, w-bar_r]</c>。
''' 于是"结论参数"的估计变成一个标准线性最小二乘问题，可以走闭式解（全局最优、一次到位），
''' 多个输出也只是同一个 <c>A</c> 的多个右端项。
''' </para>
''' <para>
''' ### 三、混合学习
''' </para>
''' <para>
''' 每轮先固定前提参数解 <c>theta</c>（<see cref="LeastSquaresAccumulator"/>），
''' 再固定 <c>theta</c> 用反向传播修正隶属函数的中心与宽度
''' （<see cref="TskLayers.Accumulate"/> + <see cref="AnfisOptimizer"/>）。
''' 四种策略由 <see cref="AnfisLearningMode"/> 选择，训练调度见 <see cref="AnfisHybridTrainer"/>。
''' </para>
''' <para>
''' ### 四、与推理内核的分层（重要）
''' </para>
''' <para>
''' 模糊推理的**数学对象**（隶属函数、规则表、五层前向、解析反向梯度、网络与持久化）
''' 已经抽提到公共程序集 <c>Microsoft.VisualBasic.DataMining.FuzzyLogic</c> 的
''' <c>TSK</c> 命名空间，本工程只保留训练与评估：
''' </para>
''' <list type="bullet">
''' <item>推理内核（依赖 FuzzyLogic）：<c>TskNetwork</c>、<c>TskInputVariable</c>、
''' <c>FuzzyRuleBase</c>、<c>TskLayers</c>、<c>TskInferenceTrace</c>、<c>TskGradient</c>、
''' <c>MembershipKind</c>、<c>IMembershipFunction</c> 与五种隶属函数、<c>TskStructureOptions</c>。</item>
''' <item>训练层（本程序集）：<see cref="AnfisHybridTrainer"/>、<see cref="AnfisOptions"/>、
''' <see cref="LeastSquaresAccumulator"/>、<see cref="RecursiveLeastSquares"/>、
''' <see cref="AnfisOptimizer"/>、<see cref="AnfisData"/>、<see cref="AnfisFitting"/>、
''' <see cref="AnfisFitResult"/>。</item>
''' <item>矩阵乘不再依赖 <c>TensorFlow</c>：结论层的 "输入 × 结论系数" 走 Math 工程的
''' <c>MatrixOps.Multiply</c>（行主序 FMA 点积），因此本工程只依赖
''' <c>Core</c> + <c>Math</c> + <c>FuzzyLogic</c>。</item>
''' <item>`Math`：最小二乘用 <c>SIMDIntrinsics.DotFma</c> 累加正规方程，
''' 求解用 <c>NumericMatrix.chol()</c>（失败降级到 <c>OLS.Solve</c>）。</item>
''' <item>`DeepLearning`：没有复用其 <c>TrainerAlgorithm</c>（它对所有参数块统一更新、
''' 按 batch 归一化梯度、默认带 L1/L2 衰减，与"轮级 LSE + 批级 BP"冲突），
''' 仅沿用其"每层 forward/backward 各自缓存中间量"的代码组织方式。</item>
''' </list>
''' <para>
''' ### 五、数值约定
''' </para>
''' <list type="bullet">
''' <item>乘积在对数域累加（<c>sum log mu</c>），低于 <c>-700</c> 视为 0；</item>
''' <item>触发强度之和小于 <c>1e-12</c> 时该行输出 0 且梯度为 0（避免 0/0）；</item>
''' <item>前提参数梯度用 <c>(f_r - z)*w-bar_r*delta</c> 的形式，
''' 不需要先算 <c>dE/dw</c> 再除 <c>mu</c>，因此隶属度下溢时不会产生 NaN；</item>
''' <item>宽度/斜率类参数在对数空间优化，天然保持为正；</item>
''' <item>正规方程只累加上三角再镜像，满足 Cholesky 的精确对称判据。</item>
''' </list>
''' <para>
''' 本工程的类型都位于根命名空间 <c>Microsoft.VisualBasic.DeepLearning.ANFIS</c> 下，
''' 目录（<c>Training</c>）只用于组织文件；推理相关类型请见
''' <c>Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK</c>。
''' </para>
''' </remarks>
Public Class NamespaceDoc

End Class
