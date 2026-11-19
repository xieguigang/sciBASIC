# Data_science/MachineLearning/GCN/GCN.vbproj

- RootNamespace : Microsoft.VisualBasic.DeepLearning.GCN
- AssemblyName  : Microsoft.VisualBasic.DeepLearning.GCN
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: Graph Convolutional Networks (Kipf & Welling)
- Existing Desc : Kipf & Welling graph convolutional network for semi-supervised node classification, with symmetric normalized adjacency in both dense and CSR form, fp64 master weights and fp32 (CUDA-friendly) GEMM/SpMM hot spots, AdamW with decoupled weight decay and early stopping.
- Existing Tags : scibasic;graph-neural-network;gcn;graph-convolution;semi-supervised;node-classification

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DeepLearning.GCN)

## Public types
- Enum PropagationMode (Adjacency\NormalizedAdjacency.vb) - 消息传递路径：决定用稠密矩阵乘还是 CSR 稀疏乘。
- Class NormalizedAdjacency (Adjacency\NormalizedAdjacency.vb) - <para> 稠密矩阵与 CSR 结构共享同一份归一化系数，因此两条路径的结果在数值上一致 （差异只来自求和次序与精度，不含语义差异）。构造一次即可被所有层的每次前向复用。
- Class GraphConvLayer (Layers\GraphConvLayer.vb) - 一层 Kipf &amp; Welling 图卷积：<c>H' = σ(Â · H · Wᵀ + b)</c>。 <para> 输入输出都是<b>特征主序</b>布局 <c>[features, nodes]</c>（即 Hᵀ），
- Class GCNNetwork (Models\GCNNetwork.vb) - 图卷积神经网络（节点分类用）：若干 <see cref="GraphConvLayer"/> + 输入侧 Dropout。
- Module NamespaceDoc (NamespaceDoc.vb) - GCN 图卷积神经网络：<c>H' = σ( D̃^(-1/2) Ã D̃^(-1/2) H W )</c>。 主要类型： <list type="bullet">
- Module GCNPrecision (Precision\GCNPrecision.vb) - GCN 算子的 fp64 / fp32 路由（GEMM 与 SpMM）。
- Enum EarlyStoppingMode (Trainer\EarlyStopping.vb)
- Class EarlyStopping (Trainer\EarlyStopping.vb) - 早停：连续 <see cref="Patience"/> 个 epoch 没有达到 <see cref="MinDelta"/> 的改善就要求停止， 并能把参数回滚到最优那一轮。
- Class GCNAdamW (Trainer\GCNAdamW.vb) - AdamW 优化器：fp64 一阶/二阶矩 + 解耦权重衰减。
- Class EvaluationResult (Trainer\SemiSupervisedTrainer.vb)
- Class TrainingHistory (Trainer\SemiSupervisedTrainer.vb)
- Class SemiSupervisedTrainer (Trainer\SemiSupervisedTrainer.vb) - 半监督节点分类训练器：整图全批量 + 掩码交叉熵 + AdamW + 可选早停。

## Notable public members
- Public Shared Property DenseDensityThreshold As Double = 0.25
- Public Property Adjacency As NormalizedAdjacency
- Public Property Nodes As Integer = -1
- Public Property Edges As Integer = -1
- Public Property Mode As PropagationMode = PropagationMode.Auto
- Public Shared Sub Invalidate(graph As Graph)
- Public Shared Function FromGraph(graph As Graph,
- Public ReadOnly Property Nodes As Integer
- Public ReadOnly Property UndirectedEdges As Integer
- Public ReadOnly Property NonZeros As Integer
- Public ReadOnly Property Density As Double
- Public Property Mode As PropagationMode
- Public ReadOnly Property EffectiveMode As PropagationMode
- Public ReadOnly Property Degree As Double()
- Public ReadOnly Property Sparse As SparseCsr
- Public ReadOnly Property Dense As Tensor
- Public Sub EnsureDense()
- Public Sub ReleaseDense()
- Public Function ToDense() As Tensor
- Public Overrides Function ToString() As String
- Public Sub New(inFeatures As Integer,
- Public Property Name As String
- Public Property IsTraining As Boolean = True
- Public ReadOnly Property InFeatures As Integer
- Public ReadOnly Property OutFeatures As Integer
- Public ReadOnly Property Activation As ActivationType
- Public ReadOnly Property Weights As Tensor
- Public ReadOnly Property Bias As Tensor
- Public Function ForwardT(inputT As Tensor, adjacency As NormalizedAdjacency) As Tensor
- Public Function BackwardT(gradT As Tensor) As Tensor
- Public Function GetParameters() As List(Of Tensor)
- Public Function GetGradients() As List(Of Tensor)
- Public Sub ResetGradients()
- Public Overrides Function ToString() As String
- Public Sub New(inFeatures As Integer,
- Public Sub New(layerSizes As Integer(),
- Public ReadOnly Property InFeatures As Integer
- Public ReadOnly Property NumClasses As Integer
- Public ReadOnly Property Depth As Integer
- Public ReadOnly Property DropoutRate As Double
- Public ReadOnly Property Layers As IReadOnlyList(Of GraphConvLayer)
- Public ReadOnly Property LastLogits As Tensor
- Public Function Forward(features As Tensor, adjacency As NormalizedAdjacency) As Tensor
- Public Function ForwardProbabilities(features As Tensor, adjacency As NormalizedAdjacency) As Tensor
- Public Function Backward(dLogits As Tensor) As Tensor
- Public Function Predict(logits As Tensor) As Integer()
- Public Function Predict(features As Tensor, adjacency As NormalizedAdjacency) As Integer()
- Public Function GetParameters() As List(Of Tensor)
- Public Function GetGradients() As List(Of Tensor)
- Public Sub ResetGradients()
- Public Sub SetTraining(isTraining As Boolean)
- Public ReadOnly Property ParameterCount As Integer
- Public Sub PrintModelInfo()
- Public Property Enabled As Boolean = True
- Public Function TryMatMul(a As Tensor, b As Tensor) As Tensor
- Public Function MatMulOr(a As Tensor, b As Tensor) As Tensor
- Public Function Propagate(adjacency As NormalizedAdjacency, featuresT As Tensor) As Tensor
- Public Function PropagateDense(dense As Tensor, featuresT As Tensor) As Tensor
- Public Function PropagateSparse(csr As SparseCsr, featuresT As Tensor) As Tensor
- Public Function TrySpMM(csr As SparseCsr, featuresT As Tensor) As Tensor
- ... and 48 more

## Imports
- Microsoft.VisualBasic.DeepLearning.GNN
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
- std = System.Math
- System.Runtime.CompilerServices

## File tree
- Adjacency\NormalizedAdjacency.vb
- Layers\GraphConvLayer.vb
- Models\GCNNetwork.vb
- NamespaceDoc.vb
- Precision\GCNPrecision.vb
- Trainer\EarlyStopping.vb
- Trainer\GCNAdamW.vb
- Trainer\SemiSupervisedTrainer.vb

