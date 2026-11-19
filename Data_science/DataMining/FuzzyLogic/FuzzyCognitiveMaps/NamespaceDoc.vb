Option Strict On
Option Explicit On

''' <summary>
''' 模糊认知图（Fuzzy Cognitive Map, FCM）组件。
''' </summary>
''' <remarks>
''' <para>
''' FCM 以加权有向图描述概念节点之间的因果强度，激活值在 <c>[-1,1]</c> 或 <c>[0,1]</c> 内连续传播：
''' <c>c_i(t+1) = f( sum_j w_ji * c_j(t) )</c>。权重可正可负，分别表示促进与抑制。
''' </para>
''' <para>
''' 与 <c>BooleanNetworks</c>（离散布尔逻辑）互补：FCM 保持连续因果强度，适合表达调控强度的渐变；
''' 与 <c>TSK</c>/<c>Mamdani</c> 共享隶属函数语义，是联合建模的自然中间层。
''' </para>
''' </remarks>
Namespace FuzzyCognitiveMaps
End Namespace
