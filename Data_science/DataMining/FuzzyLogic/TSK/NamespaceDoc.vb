Option Strict On
Option Explicit On

Namespace TSK

    ''' <summary>
    ''' Takagi-Sugeno-Kang（TSK）型可微模糊推理内核：隶属函数体系、规则表、五层前向推理与解析反向梯度。
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
    ''' <item>**输出**：<c>z = sum_r w-bar_r f_r</c>（加权平均去模糊，省掉 Mamdani 的重心数值积分）。</item>
    ''' </list>
    ''' <para>
    ''' ### 二、为什么结论层要写成矩阵形式
    ''' </para>
    ''' <para>
    ''' 把每条规则的结论参数按块拼成 <c>theta</c>（<c>R*(n+1)</c> 个），第 4、5 层就等价于
    ''' <c>z = A * theta</c>，其中 <c>A</c> 的第 r 块是 <c>[w-bar_r*x_1, ..., w-bar_r*x_n, w-bar_r]</c>。
    ''' 于是"结论参数"的估计变成一个标准线性最小二乘问题，可走闭式解，
    ''' 多个输出也只是同一个 <c>A</c> 的多个右端项。
    ''' </para>
    ''' <para>
    ''' ### 三、与 Mamdani 前端的关系
    ''' </para>
    ''' <para>
    ''' 本命名空间是纯计算的公共内核，**不包含任何训练算法**（最小二乘、反向传播、优化器、
    ''' 早停、剪枝、交叉验证都在消费方工程里实现，参考 <c>Microsoft.VisualBasic.DeepLearning.ANFIS</c>）。
    ''' 同程序集的 <c>Logical.FuzzyLogic</c>（DotFuzzy 派生的 Mamdani 控制器）已经改为复用本内核的
    ''' 隶属函数求值（<c>MembershipFunction</c> 是 <see cref="TrapezoidalMembership"/> 的门面），
    ''' 但两者的合成与去模糊语义不同：Mamdani 用 min/max + 重心法，TSK 用乘积 + 加权平均。
    ''' </para>
    ''' <para>
    ''' ### 四、数值约定
    ''' </para>
    ''' <list type="bullet">
    ''' <item>乘积在对数域累加（<c>sum log mu</c>），低于 <c>-700</c> 视为 0；</item>
    ''' <item>触发强度之和小于 <c>1e-12</c> 时该行输出 0 且梯度为 0（避免 0/0）；</item>
    ''' <item>前提参数梯度用 <c>(f_r - z)*w-bar_r*delta</c> 的形式，
    ''' 不需要先算 <c>dE/dw</c> 再除 <c>mu</c>，因此隶属度下溢时不会产生 NaN；</item>
    ''' <item>宽度/斜率类参数在对数空间优化，天然保持为正；</item>
    ''' <item>分段线性隶属函数（三角、梯形）在节点处的导数取 0。</item>
    ''' </list>
    ''' </remarks>
    Public Class NamespaceDoc

    End Class

End Namespace
