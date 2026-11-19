Option Strict On
Option Explicit On

''' <summary>
''' 模糊逻辑网络与布尔/因果网络的联合建模桥接层。
''' </summary>
''' <remarks>
''' <para>
''' 数据流：连续表达值 → TSK/Mamdani 隶属函数模糊化 → FCM 连续因果传播
''' → 二值化 → BN/PBN 布尔步进 → 布尔状态反馈为模糊规则触发强度。
''' </para>
''' <para>
''' 本命名空间不修改 TSK/Mamdani 内部语义，只在两者之间传递值，
''' 因此可独立启用，也不会破坏既有模型的兼容性。
''' </para>
''' </remarks>
Namespace Hybrid
End Namespace
