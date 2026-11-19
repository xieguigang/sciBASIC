Option Strict On
Option Explicit On

''' <summary>
''' 布尔网络（Boolean Network, BN）与概率布尔网络（Probabilistic Boolean Network, PBN）组件。
''' </summary>
''' <remarks>
''' <para>
''' 本命名空间与 <c>TSK</c>、<c>Mamdani</c> 并列，服务于基因表达调控网络建模：
''' 每个基因是一个节点，节点的布尔输出由其父节点状态经布尔规则（真值表）决定。
''' </para>
''' <para>
''' BN 是每节点单规则、概率为 1 的确定性网络；PBN 在节点上扩展为「规则集合 + 选择概率」，
''' 每步按概率采样一条规则，因此 BN 可视为 PBN 的退化情形。
''' </para>
''' <para>
''' 连续表达值优先：节点接受连续输入，经 <see cref="IStateBinarizer"/> 转为布尔状态。
''' 二值化器可以复用 <c>TSK.IMembershipFunction</c>，从而与模糊逻辑内核共享隶属函数。
''' </para>
''' </remarks>
Namespace BooleanNetworks
End Namespace
