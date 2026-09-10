# Data_science/DataMining/PaCMAP/PaCMAP.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.PaCMAP
- AssemblyName  : Microsoft.VisualBasic.DataMining.PaCMAP
- TargetFramework: net10.0
- Source files  : 4
- Existing Title: PaCMAP Pairwise Controlled Manifold Approximation Embedding
- Existing Desc : PaCMAP dimensionality reduction for sciBASIC#: preserves local and global structure by optimising neighbour, mid-near and further point pairs with a three-phase loss and an Adagrad optimiser over tensor data.
- Existing Tags : scibasic;pacmap;dimensionality-reduction;manifold-learning;visualization

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DataMining.PaCMAP)

## Public types
- Module EuclideanDistance (Euclidean.vb) - 欧几里得距离计算 Euclidean distance calculation 原始JavaScript代码:
- Module EuclideanDistanceExtensions (Euclidean.vb) - 欧几里得距离的扩展方法 Extension methods for Euclidean distance
- Module NormalizedDistanceExtensions (Normalized.vb) - const k = 7; const topKNearestDistances = tf.topk(distances.neg(), k, true).values; const fourthToSixthNearestDistances = topKNearestDistances.slice([0, 4], [-1, -1]);
- Class PaCMAP (PaCMAP.vb) - PaCMAP (Pairwise Controlled Manifold Approximation Projection) 算法实现 Implementation of PaCMAP dimensionality reduction algorithm 原始JavaScript代码来自 pacmapTF.js
- Class AdagradOptimizer (PaCMAP.vb) - Adagrad优化器 Adagrad optimizer implementation
- Module TensorExtensions (TensorExtensions.vb) - 张量扩展方法，实现链式调用 Tensor extension methods for chainable operations
- Class TopKResult (TensorExtensions.vb) - TopK结果 TopK result structure

## Notable public members
- Public Function Compute(a As Tensor, b As Tensor) As Tensor
- Public Function ComputeChainable(a As Tensor, b As Tensor) As Tensor
- Public Function EuclideanDistanceTo(a As Tensor, b As Tensor) As Tensor
- Public Function BroadcastSubForDistance(a As Tensor, b As Tensor) As Tensor
- Public Function NormalizedDistance(distances As Tensor) As Tensor
- Public Function Compute(distances As Tensor) As Tensor
- Public Property NDimensions As Integer
- Public Property NumNeighbourPairs As Integer?
- Public Property RatioMidNearPairs As Double
- Public Property RatioFurtherPairs As Double
- Public Property LearningRate As Double
- Public Property NumIterations As Integer
- Public Sub New(Optional nDimensions As Integer = 2, Optional numNeighbourPairs As Integer? = Nothing, Optional ratioMidNearPairs As Double = 0.5, Opti…
- Public Function Fit(X As Double(,), Optional init As String = "random") As Double(,)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Sub New(learningRate As Double)
- Public Sub Update(parameters As Tensor, gradients As Tensor)
- Public Function Neg(a As Tensor) As Tensor
- Public Function Square(a As Tensor) As Tensor
- Public Function Sqrt(a As Tensor) As Tensor
- Public Function Add(a As Tensor, b As Tensor) As Tensor
- Public Function Add(a As Tensor, scalar As Double) As Tensor
- Public Function Mul(a As Tensor, b As Tensor) As Tensor
- Public Function Mul(a As Tensor, scalar As Double) As Tensor
- Public Function Div(a As Tensor, b As Tensor) As Tensor
- Public Function Div(a As Tensor, scalar As Double) As Tensor
- Public Function Reshape(a As Tensor, ParamArray newShape As Integer()) As Tensor
- Public Function ExpandDims(a As Tensor, Optional axis As Integer = 0) As Tensor
- Public Function Squeeze(a As Tensor, Optional axis As Integer? = Nothing) As Tensor
- Public Function Tile(a As Tensor, reps As Integer()) As Tensor
- Public Function Slice(a As Tensor, start As Integer(), size As Integer()) As Tensor
- Public Function Concat(a As Tensor, b As Tensor, Optional axis As Integer = 0) As Tensor
- Public Function Stack(tensors As Tensor(), Optional axis As Integer = 0) As Tensor
- Public Function Sum(a As Tensor, Optional axis As Integer? = Nothing, Optional keepDims As Boolean = False) As Tensor
- Public Function MatMul(a As Tensor, b As Tensor, Optional transposeA As Boolean = False, Optional transposeB As Boolean = False) As Tensor
- Public Property Values As Tensor
- Public Property Indices As Tensor
- Public Function TopK(a As Tensor, k As Integer, Optional sorted As Boolean = True) As TensorExtensions.TopKResult
- Public Function Gather(a As Tensor, indices As Tensor, Optional axis As Integer = 0) As Tensor
- Public Function GatherND(a As Tensor, indices As Tensor) As Tensor
- Public Function RandInt(shape As Integer(), min As Integer, max As Integer, Optional seed As Integer? = Nothing) As Tensor
- Public Function BroadcastSub(a As Tensor, b As Tensor) As Tensor
- Public Sub PrintTensor(t As Tensor)

## Imports
- Microsoft.VisualBasic.MachineLearning.TensorFlow
- std = System.Math
- System.Runtime.CompilerServices

## File tree
- Euclidean.vb
- Normalized.vb
- PaCMAP.vb
- TensorExtensions.vb

