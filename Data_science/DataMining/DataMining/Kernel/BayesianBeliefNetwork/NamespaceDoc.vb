Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 离散贝叶斯信念网络内核。
    ''' </summary>
    ''' <remarks>
    ''' <para>本命名空间提供一整套通用的离散贝叶斯网络设施，模块划分如下：</para>
    ''' <list type="table">
    ''' <listheader>
    ''' <term>模块</term>
    ''' <description>职责</description>
    ''' </listheader>
    ''' <item>
    ''' <term><see cref="BeliefNode"/> / <see cref="BeliefNetwork"/></term>
    ''' <description>网络结构、任意多值节点的条件概率表、证据管理(合并语义)以及 XML 序列化。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="Factor"/></term>
    ''' <description>对数空间之中的离散因子，提供相乘、求和消元、取最大值消元与边缘化。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="VariableElimination"/></term>
    ''' <description>基于 min-fill 消除序的精确推断，支持任意多值节点。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="JunctionTree"/></term>
    ''' <description>团树之上的 Shafer-Shenoy 消息传递，作为变量消除的对照实现。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="LoopyBeliefPropagation"/></term>
    ''' <description>因子图之上的近似推断，适用于大规模或含环的网络。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="ParameterLearner"/></term>
    ''' <description>Dirichlet 平滑的最大后验参数估计。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="StructureLearner"/></term>
    ''' <description>Chow-Liu 树学习与 BIC 评分的爬山法结构学习。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="ExpectationMaximization"/></term>
    ''' <description>含隐变量的参数学习。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="Sampling"/></term>
    ''' <description>祖先采样、Gibbs 采样与似然加权采样。</description>
    ''' </item>
    ''' <item>
    ''' <term><see cref="NumericLogSpace"/></term>
    ''' <description>LogGamma、LogSumExp 与 LogDet 等数值工具。</description>
    ''' </item>
    ''' </list>
    ''' </remarks>
    Module NamespaceDoc
    End Module
End Namespace
