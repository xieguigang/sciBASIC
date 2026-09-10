# Data_science/DataMining/t-SNE/t-SNE.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.tSNE
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.t-SNE
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: Barnes-Hut t-SNE Dimensionality Reduction Embedding
- Existing Desc : t-Distributed Stochastic Neighbor Embedding for sciBASIC#: converts pairwise affinities into joint probabilities at a given perplexity and minimises KL divergence, with a Barnes-Hut SPTree approximation for large data.
- Existing Tags : scibasic;t-sne;dimensionality-reduction;barnes-hut;visualization

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.tSNE)

## Public types
- Module BarnesHutGradient (BarnesHutGradient.vb) - dC/dy_i = 4 * [ sum_j p_ij * qu_ij * (y_i - y_j) - (1/Z) * sum_j qu_ij^2 * (y_i - y_j) ] 其中 qu_ij = 1 / (1 + ||y_i - y_j||^2)，Z = sum_{k != l} qu_kl。 第一部分只涉及稀疏概率矩阵中的 O(N·k) 个近邻对，直接按行遍历即可；
- Class CostFunction (CostFunction.vb)
- Module Helper (Helper.vb)
- Class RandomHelper (RandomHelper.vb)
- Class SparseP (SparseProbability.vb) - 稀疏联合概率矩阵（CSR 行压缩存储） 精确的 t-SNE 需要一个 N×N 的稠密联合概率矩阵，其内存占用为 8N² 字节， 在 N 达到万级时就已经不可行。Barnes-Hut 近似只需要每一个点的 k 个近邻，
- Module SparseProbability (SparseProbability.vb) - 稀疏联合概率矩阵的装配工具
- Class SPNode (SPTree.vb) - Barnes-Hut 空间划分树上的一个节点
- Class SPTree (SPTree.vb) - 移植自 L. van der Maaten 的 bh_tsne 参考实现（SPTree）。 t-SNE 梯度中的斥力项需要对所有 N² 个点对求和，这是 O(N²) 复杂度的根源。 Barnes-Hut 的做法是把当前的低维嵌入组织成一棵空间划分树，
- Class tSNE (tSNE.vb)

## Notable public members
- Friend Sub Evaluate(tSNE As tSNE, Y As Double())
- Public ReadOnly Property mN As Integer
- Public Sub CostGrad(Y As Double())
- Friend Function ParallelOptions(nthreads As Integer) As System.Threading.Tasks.ParallelOptions
- Friend Function TaskBlockSize(total As Integer,
- Friend Function TaskBlockCount(total As Integer, blockSize As Integer) As Integer
- Friend Function SumParts(parts As Double()) As Double
- Friend Function SumColumnParts(parts As Double()(), [dim] As Integer) As Double()
- Friend Function zeros(n As Integer) As Double()
- Friend Function L2(x1 As Double(), x2 As Double()) As Double
- Friend Function xtod(X As Double()(), Optional nthreads As Integer = 0) As Double()
- Friend Function d2pSparse(perplexity As Double,
- Friend Function d2p(D As Double(),
- Friend Function randn2d(n As Integer, d As Integer) As Double()
- Friend Shared Function randn2d(n As Integer, d As Integer, s As Double) As Double()
- Friend Sub New(N As Integer, rowPtr As Integer(), colP As Integer(), valP As Double())
- Friend Function MemorySize() As Long
- Public Overrides Function ToString() As String
- Friend Function SuggestK(perplexity As Double) As Integer
- Friend Function Build(keys As Long(), vals As Double(), N As Integer) As SparseP
- Friend ReadOnly Property Count As Integer
- Friend Sub New([dim] As Integer)
- Friend Sub New([dim] As Integer, start As Integer, finish As Integer)
- Friend Const DEFAULT_LEAF_SIZE As Integer = 4
- Friend Sub New([dim] As Integer, Y As Double(), N As Integer, Optional leafSize As Integer = DEFAULT_LEAF_SIZE)
- Friend Sub ComputeNonEdgeForces(pointIndex As Integer, theta As Double,
- Friend ReadOnly Property NodeCount As Integer
- Public Property nthreads As Integer
- Public Property UseBarnesHut As Boolean = False
- Public Property theta As Double = 0.5
- Public Property LeafSize As Integer = SPTree.DEFAULT_LEAF_SIZE
- Public Property KNN As Integer = 0
- Public ReadOnly Property SparseNonZeros As Integer
- Public Overrides ReadOnly Property dimension As Integer
- Public Sub New(perplexity As Double, [dim] As Integer, epsilon As Double)
- Public Sub New(perplexity As Double, [dim] As Integer, epsilon As Double,
- Friend Function EffectiveK() As Integer
- Public Overrides Function GetEmbedding() As Double()()
- Public Sub InitDataRaw(X As IEnumerable(Of Double()))
- Public Sub InitDataDist(D As Double()())

## Imports
- Microsoft.VisualBasic.DataMining.ComponentModel
- Microsoft.VisualBasic.Linq
- randf = Microsoft.VisualBasic.Math.RandomExtensions
- std = System.Math

## File tree
- BarnesHutGradient.vb
- CostFunction.vb
- Helper.vb
- RandomHelper.vb
- SparseProbability.vb
- SPTree.vb
- tSNE.vb

