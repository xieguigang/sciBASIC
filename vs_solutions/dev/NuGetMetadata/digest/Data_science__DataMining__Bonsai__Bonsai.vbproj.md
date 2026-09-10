# Data_science/DataMining/Bonsai/Bonsai.vbproj

- RootNamespace : Microsoft.VisualBasic.DataMining.Bonsai
- AssemblyName  : Microsoft.VisualBasic.DataMining.Bonsai
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: Bonsai Diffusion Tree Layout for High-Dimensional Data
- Existing Desc : Reconstructs a diffusion tree from high-dimensional observations, optimising every branch length and internal node coordinate against a Felsenstein tree likelihood, then laying the tree out for visualisation in sciBASIC#.
- Existing Tags : scibasic;bonsai;dimensionality-reduction;diffusion-tree;visualization

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.DataMining.Bonsai)

## Public types
- Class Bonsai (BonsaiApi.vb) - Public entry point for the Bonsai high-dimensional data visualisation. The API mirrors the fit/transform style used by the sibling UMAP and t-SNE projects in this solution so that the three reducers can be swapped transparently.
- Class BonsaiNode (BonsaiNode.vb) - A node in the Bonsai tree. This is a faithful translation of the python ``TreeNode`` class, but with all single-cell semantics removed. The position of a node is described by a per-dimension effective coordinate (<see cref="ltqs"/>) and a per-dimension variance
- Class BonsaiTree (BonsaiTree.vb) - The Bonsai tree-reconstruction engine. Given a <see cref="PointSet"/> of high-dimensional observations it builds a tree whose non-root edges are diffusion times and whose internal nodes are effective positions, then optimises every edge length and internal-node coordinate to maximise the (continuous Felsenstein)
- Module TreeLayout (Layout.vb) - Tree layout routines that turn the reconstructed Bonsai topology (parent/child + branch times) into a two-dimensional embedding, mirroring the distortion-free 2D visualisation that distinguishes Bonsai from UMAP / t-SNE. Only the topology and the branch lengths (<see cref="BonsaiNode.tParent"/>) are used, so the
- Module Likelihood (Likelihood.vb) - Pure numeric core of the Bonsai likelihood model. Everything factorises over dimensions, so the methods below operate on D-length vectors and never touch full matrices. This is a direct translation of the functions in ``bonsai_treeHelpers.py`` (getLoglikAndGradStarTree,
- Module Optimizer (Optimizer.vb) - Lightweight numerical optimisers used by the Bonsai core. These mirror the scipy primitives the reference python code relies on: a 1-D Brent root-finder (scipy.optimize.brentq) and a bound-constrained L-BFGS (scipy.optimize.minimize(method="L-BFGS-B", jac=True)).
- Structure OptResult (Optimizer.vb)
- Class PointSet (PointSet.vb) - A point set of high-dimensional observations, stripped of all single-cell biology semantics. Each sample is represented by a mean vector (coords) and a per-dimension standard deviation (uncertainty). This mirrors the Bonsai ``SCData`` abstraction but

## Notable public members
- Public ReadOnly Property Tree As BonsaiTree
- Public ReadOnly Property Data As PointSet
- Public Property maxMerges As Integer = -1
- Public Property maxTimeIters As Integer = 20
- Public Property verbose As Boolean = False
- Public Property filterLowSNR As Boolean = True
- Public Property snrThreshold As Double = 1.0
- Public Property useGlobalVariance As Boolean = False
- Public Property layout As String = "dendrogram"
- Public ReadOnly Property RawData As PointSet
- Public Function Fit(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Bonsai
- Public Function Fit(data As PointSet) As Bonsai
- Public Function Transform() As Double()()
- Public Function Get2DLayout() As Double()()
- Public Function GetHighDimStates() As Double()()
- Public Function BranchTimeCoords() As Double()
- Public Function ToNewick() As String
- Public Function LogLikelihood() As Double
- Public Shared Function Embed(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Double()()
- Public Function getLtqsVars() As Double()
- Public Function getW() As Double()
- Public Sub setLtqsVarsOrW(Optional ltqsVars As Double() = Nothing, Optional W_g As Double() = Nothing)
- Public Function isLeafNode() As Boolean
- Public Function isRootNode() As Boolean
- Public Function getLeafs() As List(Of BonsaiNode)
- Public Function getScale(g As Integer) As Double
- Public Function getTransferVariance(g As Integer) As Double
- Public Function getInfo() As (tParent As Double, wbar As Double(), ltqs As Double(), ltqsVars As Double(), nodeInd As Integer)
- Public Sub getLtqsUponMerge()
- Public Function toNewick(Optional useIds As Boolean = True) As String
- Public Function countDataLeafs() As Integer
- Public Shared Function Build(data As PointSet,
- Public Function optTimes(Optional maxiter As Integer = 20, Optional verbose As Boolean = False) As Double
- Public Function mergeChildrenUB(Optional verbose As Boolean = False) As Boolean
- Public Sub mergeZeroTimeChilds(Optional verbose As Boolean = False)
- Public Sub PerformNNI(Optional randomPhase As Boolean = True, Optional maxRounds As Integer = 3, Optional verbose As Boolean = False, Optional maxiter…
- Public Sub PerformSPR(Optional maxRounds As Integer = 2, Optional verbose As Boolean = False, Optional maxiter As Integer = 10)
- Public Sub RerootToMinInternalDist(Optional verbose As Boolean = False)
- Public Function calcLogLComplete() As Double
- Public Function CountNodes() As Integer
- Public Function ToNewick() As String
- Public Function GetLowDimCoords() As Double()()
- Public Function GetBranchTimeCoords() As Double()
- Public Function DendrogramLayout(root As BonsaiNode) As Double()()
- Public Function RadialLayout(root As BonsaiNode, Optional radiusScale As Double = 1.0) As Double()()
- Public Const EPS As Double = 0.000000000001
- Public Function findNodeLtqs(childs As List(Of BonsaiNode)) As Double()
- Public Function findNodeW(childs As List(Of BonsaiNode)) As Double()
- Public Function loglikStarTree(childs As List(Of BonsaiNode), xr_g As Double(), W_g As Double()) As Double
- Public Function loglikGradStarTree(childs As List(Of BonsaiNode), xr_g As Double(), W_g As Double()) As (loglik As Double, grad As Double())
- Public Function der2LeafTree(t12 As Double, args() As Object) As Double
- Public Function getOptTime2LeafTree(ltqs1 As Double(), ltqsVars1 As Double(), ltqs2 As Double(), ltqsVars2 As Double(), Optional scale As Double() = N…
- Public Function optimiseT3LeafStar(ltqs_gi As Double()(), ltqsVars_gi As Double()(), t0_i As Double(), Optional scale As Double() = Nothing) As (logli…
- Public Sub getDerivativesDownstream(root As BonsaiNode)
- Public Function calcLogLComplete(root As BonsaiNode) As Double
- Public Function calcSingleDLogL(xrAsIfRoot_g As Double(), WAsIfRoot_g As Double(), child1 As BonsaiNode, child2 As BonsaiNode) As Double
- Public Delegate Function ObjWithGrad(x As Double(), args() As Object) As (f As Double, grad As Double())
- Public Delegate Function ScalarFunc(t As Double, args() As Object) As Double
- Public Function BrentZero(f As ScalarFunc, a As Double, b As Double, ParamArray args() As Object) As Double
- Public Function Minimize(obj As ObjWithGrad, x0 As Double(), bounds As List(Of (lo As Double, hi As Double)), ParamArray args() As Object) As OptResul…
- ... and 7 more

## Imports
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- System.Runtime.CompilerServices

## File tree
- BonsaiApi.vb
- BonsaiNode.vb
- BonsaiTree.vb
- Layout.vb
- Likelihood.vb
- Optimizer.vb
- PointSet.vb

