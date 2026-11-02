' ---------------------------------------------------------------------------
' GCN：Kipf & Welling 图卷积神经网络
'
' 定位：本程序集只做一件事 —— 把 readme.md 里的那条公式
'
'     H^(l+1) = σ( D̃^(-1/2) Ã D̃^(-1/2) H^(l) W^(l) ),   Ã = A + I
'
' 做成一个可训练、可复用、与仓库内其它神经网络工程同构的实现。与 GNN 工程的分工：
'
'   * GNN        —— 图神经网络的“全家桶”（GCN/GAT/GRU 图层、时序图、图分类），
'                   图层以 tensor(Double) 直存，追求覆盖面；
'   * GCN (本工程) —— 只做半监督节点分类这一条主线，但把这条线做透：
'                   归一化邻接的双表示（稠密 + CSR）、两条消息传递路径的对齐、
'                   fp64 主存储 + fp32 热点的混合精度路由、半监督训练与早停。
'
' 精度架构（与 DeepLearning/CNN、GNN、TensorFlow 完全一致，不新造机制）：
'
'   * fp64 —— 权重、偏置、全部梯度、AdamW 的一阶/二阶矩、归一化系数与度、
'             logits、交叉熵损失及其梯度、早停统计量；
'   * fp32 —— 只有算力密集的四个算子：Â·H（稠密 GEMM 或 CSR SpMM）、
'             W·Z、dW = dY·Zᵀ、dZ = Wᵀ·dY。
'             是否降精度由 MixedPrecisionPolicy 按规模与数值安全性裁决，
'             统一经 GCNPrecision 路由，可一键回退并计数。
'
' 布局约定（最容易踩的地方，详见 GraphConvLayer 的注释）：
'   因为 TensorFlow 的 SpMM 契约是“稠密 × 稀疏”，而 GCN 要的是“稀疏 × 稠密”，
'   本工程在层内部统一以 **特征主序** [features, nodes] 传递激活值 ——
'   对称归一化矩阵的对称性使得 (Â·H)ᵀ = Hᵀ·Â，两条路径都不需要额外转置。
' ---------------------------------------------------------------------------

''' <summary>
''' GCN 图卷积神经网络：<c>H' = σ( D̃^(-1/2) Ã D̃^(-1/2) H W )</c>。
''' </summary>
''' <remarks>
''' 主要类型：
''' <list type="bullet">
'''   <item><see cref="NormalizedAdjacency"/>：对称归一化邻接 Â 的承载与缓存（稠密 + CSR 双表示）；</item>
'''   <item><see cref="GCNPrecision"/>：GCN 专用精度路由（fp32 GEMM / fp32 SpMM，可回退、可计数）；</item>
'''   <item><see cref="GraphConvLayer"/>：单层图卷积；</item>
'''   <item><see cref="GCNNetwork"/>：多层堆叠 + Dropout，输出节点级 logits；</item>
'''   <item><see cref="GCNAdamW"/> / <see cref="EarlyStopping"/> / <see cref="SemiSupervisedTrainer"/>：训练侧三件套。</item>
''' </list>
''' </remarks>
Public Module NamespaceDoc

End Module
