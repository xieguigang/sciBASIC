#Region "Microsoft.VisualBasic::4043dce4f12f61534e9bffaa53cb878e, Data_science\DataMining\UMAP\Umap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 477
    '    Code Lines: 297 (62.26%)
    ' Comment Lines: 105 (22.01%)
    '    - Xml Docs: 64.76%
    ' 
    '   Blank Lines: 75 (15.72%)
    '     File Size: 20.18 KB


    ' Class Umap
    ' 
    '     Properties: dimension
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) [Step], FindABParams, FuzzySimplicialSet, GetEmbedding, GetGraph
    '               GetNEpochs, InitializeFit, InitializeFitImpl, InitializeSimplicialSetEmbedding, MakeEpochsPerSample
    ' 
    '     Sub: InitializeOptimization, Iterate, OptimizeLayoutStep, PrepareForOptimizationLoop, RunIterate
    '          SetMaxTreeDepth
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.UMAP.KNN
Imports Microsoft.VisualBasic.DataMining.UMAP.KNN.KDTreeMethod
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

''' <summary>
''' UMAP: Uniform Manifold Approximation and Projection for Dimension Reduction
''' </summary>
''' <remarks>
''' https://github.com/curiosity-ai/umap-sharp
''' </remarks>
Public NotInheritable Class Umap : Inherits IDataEmbedding

    Friend Const SMOOTH_K_TOLERANCE As Double = 0.00001F
    Friend Const MIN_K_DIST_SCALE As Double = 0.001F

    ReadOnly _learningRate As Double = 1.0F
    ReadOnly _minDist As Double = 0.1F
    ''' <summary>
    ''' The number of negative samples to select per positive sample in the 
    ''' optimization process. Increasing this value will result in greater 
    ''' repulsive force being applied, greater optimization cost, but slightly 
    ''' more accuracy.
    ''' </summary>
    ReadOnly _negativeSampleRate As Integer = 5
    ReadOnly _repulsionStrength As Double = 1
    ReadOnly _setOpMixRatio As Double = 1
    ReadOnly _spread As Double = 1
    Friend ReadOnly _distanceFn As DistanceCalculation
    Friend ReadOnly _random As IProvideRandomValues
    ReadOnly _customNumberOfEpochs As Integer?
    ReadOnly _progressReporter As RunSlavePipeline.SetProgressEventHandler

    ''' <summary>
    ''' the gradient clipping value of the SGD optimization step
    ''' </summary>
    ReadOnly _gradientClipValue As Double = 4.0
    ''' <summary>
    ''' the value range [-a, a] of the uniformly distributed random 
    ''' initialization of the embedding
    ''' </summary>
    ReadOnly _initialEmbeddingRange As Double = 10
    ''' <summary>
    ''' move the other vertex of the edge while doing the SGD optimization?
    ''' </summary>
    ReadOnly _moveOther As Boolean = True
    ''' <summary>
    ''' the number-of-epochs heuristic of the SGD optimization
    ''' </summary>
    ReadOnly _epochsSchedule As EpochSchedule
    ''' <summary>
    ''' the parallelism configuration of the whole UMAP pipeline
    ''' </summary>
    Friend ReadOnly _parallelism As ParallelConfig

    ''' <summary>
    ''' graph data:
    ''' 
    ''' + head  source index
    ''' + tail  target index
    ''' + value edge weight
    ''' </summary>
    ReadOnly _optimizationState As OptimizationState
    ReadOnly _customMapCutoff As Double?

    ''' <summary>
    ''' run knn search via kd-tree as mectric engine?
    ''' </summary>
    ReadOnly _kdTreeKNNEngine As Boolean = False

    Friend ReadOnly KNNArguments As KNNArguments

    ''' <summary>
    ''' Internal graph connectivity representation
    ''' </summary>
    Private _graph As SparseMatrix = Nothing
    Private _x As Double()() = Nothing
    Private _isInitialized As Boolean = False

    ''' <summary>
    ''' KNN state (can be precomputed and supplied via initializeFit)
    ''' </summary>
    Dim _knn As KNNState
    ''' <summary>
    ''' Projected embedding
    ''' </summary>
    Dim _embedding As Double()

    Public Overrides ReadOnly Property dimension As Integer
        Get
            Return _optimizationState.Dim
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="distance"></param>
    ''' <param name="random"></param>
    ''' <param name="dimensions"></param>
    ''' <param name="numberOfNeighbors"></param>
    ''' <param name="localConnectivity"></param>
    ''' <param name="KnnIter"></param>
    ''' <param name="bandwidth"></param>
    ''' <param name="customNumberOfEpochs">
    ''' a fixed number of the SGD epochs, the <paramref name="epochsSchedule"/> 
    ''' heuristic is used when this parameter value is not set.
    ''' </param>
    ''' <param name="customMapCutoff">cutoff value in range ``[0,1]``</param>
    ''' <param name="progressReporter"></param>
    ''' <param name="negativeSampleRate">
    ''' The number of negative samples to select per positive sample in the 
    ''' optimization process (default 5).
    ''' </param>
    ''' <param name="epochsSchedule">
    ''' the number-of-epochs heuristic that is used by the SGD optimization, 
    ''' the default schedule is: ``n&lt;=2500 -> 500``, ``n&lt;=5000 -> 400``, 
    ''' ``n&lt;=7500 -> 300`` and ``n&gt;7500 -> 200``.
    ''' </param>
    ''' <param name="gradientClipValue">
    ''' the gradient clipping value of the SGD optimization step (default 4.0)
    ''' </param>
    ''' <param name="initialEmbeddingRange">
    ''' the value range [-a, a] of the uniformly distributed random 
    ''' initialization of the embedding (default 10).
    ''' </param>
    ''' <param name="moveOther">
    ''' move the other vertex of the edge while doing the SGD optimization? 
    ''' (default TRUE)
    ''' </param>
    ''' <param name="nTrees">
    ''' the number of the random projection trees of the rp-forest, the 
    ''' adaptive formula ``5 + round(sqrt(n) / 20)`` is used when this 
    ''' parameter value is less than or equals to zero.
    ''' </param>
    ''' <param name="leafSize">
    ''' the max size of the leaf node of the random projection tree, the 
    ''' adaptive formula ``max(10, k)`` is used when this parameter value 
    ''' is less than or equals to zero.
    ''' </param>
    ''' <param name="maxCandidates">
    ''' the max number of the candidate neighbours of each vertex that is 
    ''' used by the nearest neighbour descent (default 50).
    ''' </param>
    ''' <param name="nnDescentDelta">
    ''' the early stop threshold of the nearest neighbour descent (default 0.001).
    ''' </param>
    ''' <param name="nnDescentRho">
    ''' the sample rate of the nearest neighbour descent, a lower value means 
    ''' a faster but more approximate neighbour graph (default 0.5).
    ''' </param>
    ''' <param name="rpTreeInit">
    ''' init the neighbour graph via the random projection forest? (default TRUE)
    ''' </param>
    ''' <param name="nnDescentIters">
    ''' the number of the iterations of the nearest neighbour descent, the 
    ''' adaptive formula ``max(5, floor(round(log2(n))))`` is used when this 
    ''' parameter value is less than or equals to zero.
    ''' </param>
    ''' <param name="parallelism">
    ''' the parallelism configuration of the whole UMAP pipeline, the 
    ''' <see cref="ParallelConfig.Sequential"/> is used when this parameter 
    ''' value is not set.
    ''' </param>
    Public Sub New(Optional distance As DistanceCalculation = Nothing,
                   Optional random As IProvideRandomValues = Nothing,
                   Optional dimensions As Integer = 2,
                   Optional numberOfNeighbors As Integer = 15,
                   Optional localConnectivity As Double = 1,
                   Optional KnnIter As Integer = 64,
                   Optional bandwidth As Double = 1,
                   Optional customNumberOfEpochs As Integer? = Nothing,
                   Optional customMapCutoff As Double? = Nothing,
                   Optional kdTreeKNNEngine As Boolean = False,
                   Optional setOpMixRatio As Double = 1,
                   Optional minDist As Double = 0.1F,
                   Optional spread As Double = 1,
                   Optional learningRate As Double = 1.0F,
                   Optional repulsionStrength As Double = 1,
                   Optional progressReporter As RunSlavePipeline.SetProgressEventHandler = Nothing,
                   Optional negativeSampleRate As Integer = 5,
                   Optional epochsSchedule As EpochSchedule = Nothing,
                   Optional gradientClipValue As Double = 4.0,
                   Optional initialEmbeddingRange As Double = 10,
                   Optional moveOther As Boolean = True,
                   Optional nTrees As Integer = 0,
                   Optional leafSize As Integer = 0,
                   Optional maxCandidates As Integer = 50,
                   Optional nnDescentDelta As Double = 0.001F,
                   Optional nnDescentRho As Double = 0.5F,
                   Optional rpTreeInit As Boolean = True,
                   Optional nnDescentIters As Integer = 0,
                   Optional parallelism As ParallelConfig = Nothing)

        If customNumberOfEpochs IsNot Nothing AndAlso customNumberOfEpochs <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(customNumberOfEpochs), "if non-null then must be a positive value")
        Else
            KNNArguments = New KNNArguments(numberOfNeighbors, localConnectivity, KnnIter, bandwidth,
                                            nTrees:=nTrees,
                                            leafSize:=leafSize,
                                            maxCandidates:=maxCandidates,
                                            delta:=nnDescentDelta,
                                            rho:=nnDescentRho,
                                            rpTreeInit:=rpTreeInit,
                                            nDescentIters:=nnDescentIters,
                                            parallelism:=parallelism)
        End If

        If negativeSampleRate <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(negativeSampleRate), "the negative sample rate must be a positive value")
        End If
        If gradientClipValue <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(gradientClipValue), "the gradient clip value must be a positive value")
        End If
        If initialEmbeddingRange <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(initialEmbeddingRange), "the initial embedding range must be a positive value")
        End If

        _setOpMixRatio = setOpMixRatio
        _minDist = minDist
        _spread = spread
        _repulsionStrength = repulsionStrength
        _learningRate = learningRate
        _negativeSampleRate = negativeSampleRate
        _gradientClipValue = gradientClipValue
        _initialEmbeddingRange = initialEmbeddingRange
        _moveOther = moveOther
        _epochsSchedule = If(epochsSchedule, EpochSchedule.Default)
        _parallelism = If(parallelism, ParallelConfig.Default)
        _kdTreeKNNEngine = kdTreeKNNEngine
        _customMapCutoff = customMapCutoff
        _distanceFn = If(distance, AddressOf DistanceFunctions.Cosine)
        _random = If(random, DefaultRandomGenerator.Instance)
        _optimizationState = New OptimizationState With {
            .[Dim] = dimensions
        }
        _customNumberOfEpochs = customNumberOfEpochs
        _progressReporter = progressReporter
    End Sub

    Public Function GetGraph() As SparseMatrix
        'Return New SparseMatrix(
        '    rows:=_optimizationState.Head,
        '    cols:=_optimizationState.Tail,
        '    values:=_optimizationState.EpochOfNextSample,
        '    dims:=_graph.Dims
        ')
        Return _graph
    End Function

    Private Function InitializeFitImpl() As Integer
        ' For large quantities of data (which is where the progress estimating is more useful), 
        ' InitializeFit Takes at least 80% of the total time (the calls to Step are
        ' completed much more quickly AND they naturally lend themselves to granular progress updates; 
        ' one per loop compared to the recommended number of epochs)
        If _kdTreeKNNEngine Then
            _knn = KDTreeMetric.GetKNN(_x, k:=KNNArguments.k)
        Else
            ' This part of the process very roughly accounts for 1/3 of the work
            ' the whole KNNArguments is passed into the engine now, so that the 
            ' rp-forest/nearest neighbour descent arguments are configurable
            _knn = New KNearestNeighbour(KNNArguments, _distanceFn, _random).NearestNeighbors(_x)
        End If

        ' This part of the process very roughly accounts for 2/3 of the work (the reamining work is in the Step calls)
        _graph = Me.FuzzySimplicialSet(x:=_x, setOpMixRatio:=_setOpMixRatio)

        With InitializeSimplicialSetEmbedding()
            ' Set the optimization routine state
            _optimizationState.Head = .head
            _optimizationState.Tail = .tail
            _optimizationState.EpochsPerSample = .epochsPerSample
        End With

        ' Now, initialize the optimization steps
        Call InitializeOptimization()
        Call PrepareForOptimizationLoop()

        _isInitialized = True

        Return GetNEpochs()
    End Function

    ''' <summary>
    ''' Initializes fit by computing KNN and a fuzzy simplicial set, as well as initializing 
    ''' the projected embeddings. Sets the optimization state ahead of optimization steps.
    ''' 
    ''' Returns the number of epochs to be used for the SGD optimization.
    ''' </summary>
    Public Function InitializeFit(x As Double()()) As Integer
        ' We don't need to reinitialize if we've already initialized for this data
        If _x Is x AndAlso _isInitialized Then
            Return GetNEpochs()
        Else
            _x = x
            Return InitializeFitImpl()
        End If
    End Function

    ''' <summary>
    ''' get projection result
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetEmbedding() As Double()()
        Dim final As Double()() = New Double(_optimizationState.NVertices - 1)() {}
        Dim span As Double() = _embedding
        Dim dims As Integer = _optimizationState.Dim

        For i As Integer = 0 To _optimizationState.NVertices - 1
            Dim vector As Double() = New Double(dims - 1) {}

            ' Array.Copy is much faster than the span slice plus ToArray
            Array.Copy(span, i * dims, vector, Scan0, dims)
            final(i) = vector
        Next

        Return final
    End Function

    ''' <summary>
    ''' Gets the number of epochs for optimizing the projection - NOTE: This heuristic differs from the python version
    ''' </summary>
    ''' <remarks>
    ''' the threshold/epoch value pairs are configurable now via the 
    ''' <see cref="EpochSchedule"/> object, the default schedule is 
    ''' completely the same as the original hardcoded heuristic.
    ''' </remarks>
    Private Function GetNEpochs() As Integer
        Dim length = _graph.Dims.rows

        If _customNumberOfEpochs IsNot Nothing Then
            Return _customNumberOfEpochs.Value
        End If

        Return _epochsSchedule.GetEpochs(length)
    End Function

    ''' <summary>
    ''' Get the underlying embedding vector as a <see cref="Span(Of Double)"/> 
    ''' view for the SGD optimization.
    ''' </summary>
    ''' <returns>
    ''' a span view which is bound to the shared embedding vector, the write 
    ''' operation of the span view is directly applied to the embedding 
    ''' vector.
    ''' </returns>
    Friend Function GetEmbeddingSpan() As Span(Of Double)
        Return New Span(Of Double)(_embedding)
    End Function

    ''' <summary>
    ''' Given a set of data X, a neighborhood size, and a measure of distance compute the fuzzy simplicial 
    ''' set(here represented as a fuzzy graph in the form of a sparse matrix) associated to the data. This 
    ''' is done by locally approximating geodesic distance at each point, creating a fuzzy simplicial set 
    ''' for each such point, and then combining all the local fuzzy simplicial sets into a global one via 
    ''' a fuzzy union.
    ''' </summary>
    Private Function FuzzySimplicialSet(x As Double()(), setOpMixRatio As Double) As SparseMatrix
        Dim knnIndices = If(_knn.knnIndices, New Integer(-1)() {})
        Dim knnDistances = If(_knn.knnDistances, New Single(-1)() {})
        Dim sigmasRhos = New SmoothKNN(knnDistances, KNNArguments).SmoothKNNDistance()
        Dim rowsColsVals = SmoothKNN.ComputeMembershipStrengths(knnIndices, knnDistances, sigmasRhos.sigmas, sigmasRhos.rhos, parallelism:=_parallelism)
        Dim sparseMatrix = New SparseMatrix(rowsColsVals.Row, rowsColsVals.Col, rowsColsVals.X, (x.Length, x.Length))
        Dim transpose = sparseMatrix.Transpose(_parallelism)
        Dim prodMatrix = sparseMatrix.PairwiseMultiply(transpose, _parallelism)
        Dim a = sparseMatrix.Add(CType(transpose, SparseMatrix), _parallelism).Subtract(prodMatrix, _parallelism)
        Dim b = a.MultiplyScalar(setOpMixRatio, _parallelism)
        Dim c = prodMatrix.MultiplyScalar(1 - setOpMixRatio, _parallelism)
        Dim result = b.Add(c, _parallelism)

        Return result
    End Function

    ''' <summary>
    ''' Initialize a fuzzy simplicial set embedding, using a specified initialisation method and then minimizing the 
    ''' fuzzy set cross entropy between the 1-skeletons of the high and low dimensional fuzzy simplicial sets.
    ''' </summary>
    Private Function InitializeSimplicialSetEmbedding() As (head As Integer(), tail As Integer(), epochsPerSample As Double())
        Dim nEpochs As Integer = GetNEpochs()
        Dim graphMax As Double = _graph.GetValues().Max
        Dim cutoff As Double = If(_customMapCutoff Is Nothing, graphMax / nEpochs, graphMax * _customMapCutoff)
        Dim graph As SparseMatrix = _graph.Map(Function(value) If(value < cutoff, 0, value), _parallelism)

        ' We're not computing the spectral initialization in this implementation 
        ' until we determine a better eigenvalue/eigenvector computation approach
        _embedding = New Double(graph.Dims.rows * _optimizationState.Dim - 1) {}

        ' the value range of the random initialization is configurable now, 
        ' the default value 10 is the same as the original implementation
        Call SIMDint.Uniform(_embedding, _initialEmbeddingRange, _random)

        ' Get graph data in ordered way...
        Dim weights As New List(Of Double)()
        Dim head As New List(Of Integer)()
        Dim tail As New List(Of Integer)()

        For Each rowColValue In graph.GetAll()
            Dim row = rowColValue.Item1
            Dim col = rowColValue.Item2
            Dim value = rowColValue.Item3

            If value <> 0 Then
                weights.Add(value)
                tail.Add(row)
                head.Add(col)
            End If
        Next

        Call ShuffleTogether(head, tail, weights, _random)

        Return (head.ToArray(), tail.ToArray(), Umap.MakeEpochsPerSample(weights.ToArray(), nEpochs))
    End Function

    Private Shared Function MakeEpochsPerSample(weights As Double(), nEpochs As Integer) As Double()
        Dim result = Utils.Filled(weights.Length, -1)
        Dim max = weights.Max

        For Each nI In weights.Select(Function(w, i) (w / max * nEpochs, i))
            Dim n = nI.Item1
            Dim i = nI.Item2

            If n > 0 Then
                result(i) = nEpochs / n
            End If
        Next

        Return result
    End Function

    Private Sub InitializeOptimization()
        ' Initialized in initializeSimplicialSetEmbedding()
        Dim head = _optimizationState.Head
        Dim tail = _optimizationState.Tail
        Dim epochsPerSample = _optimizationState.EpochsPerSample
        Dim nEpochs = GetNEpochs()
        Dim nVertices = _graph.Dims.cols
        Dim aB = Umap.FindABParams(_spread, _minDist)

        _optimizationState.Head = head
        _optimizationState.Tail = tail
        _optimizationState.EpochsPerSample = epochsPerSample
        _optimizationState.A = aB.a
        _optimizationState.B = aB.b
        _optimizationState.NEpochs = nEpochs
        _optimizationState.NVertices = nVertices
    End Sub

    ''' <summary>
    ''' Fit a, b params for the differentiable curve used in lower dimensional 
    ''' fuzzy simplicial complex construction. We want the smooth curve (from a 
    ''' pre-defined family with simple gradient) that best matches an offset 
    ''' exponential decay.
    ''' </summary>
    ''' <param name="spread">the effective scale of the embedded points</param>
    ''' <param name="minDist">
    ''' the effective minimum distance between the embedded points
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the Levenberg-Marquardt algorithm that is required by the curve 
    ''' fitting is implemented inside the <see cref="ABParams"/> module, 
    ''' so that any combination of the ``spread``/``minDist`` parameters 
    ''' is supported now (previously an <see cref="ArgumentException"/> 
    ''' will be thrown for any non-default configuration).
    ''' 
    ''' the result of the default configuration (spread = 1, minDist = 0.1) 
    ''' is still the original hardcoded constant, so that the default 
    ''' behaviour is not changed at all.
    ''' </remarks>
    Friend Shared Function FindABParams(spread As Double, minDist As Double) As (a As Double, b As Double)
        Return ABParams.FindABParams(spread, minDist)
    End Function

    Private Sub PrepareForOptimizationLoop()
        ' Hyperparameters
        Dim repulsionStrength = _repulsionStrength
        Dim learningRate = _learningRate
        Dim negativeSampleRate = _negativeSampleRate
        Dim epochsPerSample = _optimizationState.EpochsPerSample
        Dim [dim] = _optimizationState.Dim
        Dim epochsPerNegativeSample = epochsPerSample.[Select](Function(e) e / negativeSampleRate).ToArray()
        Dim epochOfNextNegativeSample = epochsPerNegativeSample.ToArray()
        Dim epochOfNextSample = epochsPerSample.ToArray()

        _optimizationState.EpochOfNextSample = epochOfNextSample
        _optimizationState.EpochOfNextNegativeSample = epochOfNextNegativeSample
        _optimizationState.EpochsPerNegativeSample = epochsPerNegativeSample
        _optimizationState.MoveOther = _moveOther
        _optimizationState.InitialAlpha = learningRate
        _optimizationState.Alpha = learningRate
        _optimizationState.Gamma = repulsionStrength
        _optimizationState.Dim = [dim]
    End Sub

    Public Function [Step](nEpochs As Integer) As Umap
        Dim epochsIndex As IEnumerable(Of Integer)

        If App.EnableTqdm Then
            epochsIndex = Tqdm.Range(0, nEpochs)
        Else
            epochsIndex = Enumerable.Range(0, nEpochs)
        End If

        For Each i As Integer In epochsIndex
            Call [Step]()
        Next

        Return Me
    End Function

    ''' <summary>
    ''' Manually step through the optimization process one epoch at a time
    ''' </summary>
    Public Function [Step]() As Integer
        Dim currentEpoch = _optimizationState.CurrentEpoch
        Dim numberOfEpochsToComplete = GetNEpochs()

        If currentEpoch < numberOfEpochsToComplete Then
            Call OptimizeLayoutStep(currentEpoch)

            If _progressReporter IsNot Nothing Then
                ' InitializeFit roughly approximately takes 80% of the processing time for large quantities of data, 
                ' leaving 20% for the Step iterations - the progress reporter calls made here are based on the 
                ' assumption that Step will be called the recommended number of times (the number-of-epochs value 
                ' returned From InitializeFit)
                ' Umap.ScaleProgressReporter(_progressReporter, 0.8F, 1)(CSng(currentEpoch) / numberOfEpochsToComplete, "OptimizeLayoutStep")
                ' Call Console.Write(".")
            End If
        End If

        Return _optimizationState.CurrentEpoch
    End Function

    ''' <summary>
    ''' Improve an embedding using stochastic gradient descent to minimize the fuzzy set cross entropy between 
    ''' the 1-skeletons of the high dimensional and low dimensional fuzzy simplicial sets.
    ''' 
    ''' In practice this is done by sampling edges based on their membership strength(with the (1-p) terms 
    ''' coming from negative sampling similar to word2vec).
    ''' </summary>
    Private Sub OptimizeLayoutStep(n As Integer)
        Dim workLen As Integer = _optimizationState.EpochsPerSample.Length
        ' 在这里可以进行并行化？
        ' 
        ' yes, the SGD optimization of one epoch is a Hogwild style parallel 
        ' workload now: the vertex may be hit by more than one edge at the 
        ' same time, which makes the result no longer bit-level reproducible, 
        ' but the convergence is not affected at all.
        '
        ' note about that the negative sampling requires a thread safe random 
        ' source, so the parallel branch is disabled for a random source 
        ' with the IsThreadSafe flag equals to FALSE.
        Dim degree As Integer = If(_parallelism.CanParallel(workLen) AndAlso _random.IsThreadSafe,
                                   _parallelism.MaxDegreeOfParallelism,
                                   1)

        If degree > 1 Then
            Call New SgdEpochTask(workLen, Me, n, _gradientClipValue, workers:=degree).Run()
        Else
            Dim embeddingSpan As Span(Of Double) = _embedding

            For i As Integer = 0 To workLen - 1
                Call RunIterate(i, n, _gradientClipValue, embeddingSpan)
            Next
        End If

        ' Preparation for future work for interpolating the table before optimizing
        _optimizationState.Alpha = _optimizationState.InitialAlpha * (1.0F - n / _optimizationState.NEpochs)
        _optimizationState.CurrentEpoch += 1
    End Sub

    ''' <summary>
    ''' run the SGD iteration of the i th edge of the epoch <paramref name="n"/>
    ''' </summary>
    ''' <param name="i">the index of the edge</param>
    ''' <param name="n">the current epoch</param>
    ''' <param name="clipValue">the gradient clipping value</param>
    ''' <param name="embeddingSpan">
    ''' a span view which is bound to the shared embedding vector, each 
    ''' worker thread of the parallel SGD creates its own view via 
    ''' <see cref="Umap.GetEmbeddingSpan"/>.
    ''' </param>
    Friend Sub RunIterate(i As Integer, n As Integer, clipValue As Double, embeddingSpan As Span(Of Double))
        If Not (_optimizationState.EpochOfNextSample(i) >= n) Then
            Call Iterate(i, n, clipValue, embeddingSpan)
        End If
    End Sub

    Private Sub Iterate(i As Integer, n As Integer, clipValue As Double, embeddingSpan As Span(Of Double))
        Dim j As Integer = _optimizationState.Head(i)
        Dim k As Integer = _optimizationState.Tail(i)

        Dim current_start As Integer = j * _optimizationState.Dim
        Dim other_start As Integer = k * _optimizationState.Dim
        Dim current = embeddingSpan.Slice(current_start, _optimizationState.Dim)
        Dim other = embeddingSpan.Slice(other_start, _optimizationState.Dim)

        Dim distSquared As Double = DistanceMethods.SquareDistance(current, other)
        Dim gradCoeff = 0F
        Dim gradD As Double

        If (distSquared > 0) Then
            gradCoeff = -2 * _optimizationState.A * _optimizationState.B * std.Pow(distSquared, _optimizationState.B - 1)
            gradCoeff /= _optimizationState.A * std.Pow(distSquared, _optimizationState.B) + 1
        End If

        For d As Integer = 0 To _optimizationState.Dim - 1
            gradD = Math.Clip(gradCoeff * (current(d) - other(d)), clipValue)
            current(d) += gradD * _optimizationState.Alpha

            If (_optimizationState.MoveOther) Then
                other(d) += -gradD * _optimizationState.Alpha
            End If
        Next

        _optimizationState.EpochOfNextSample(i) += _optimizationState.EpochsPerSample(i)

        Dim nNegSamples As Integer = std.Floor((n - _optimizationState.EpochOfNextNegativeSample(i)) / _optimizationState.EpochsPerNegativeSample(i))

        For p = 0 To nNegSamples - 1

            k = _random.Next(0, _optimizationState.NVertices)
            other_start = k * _optimizationState.Dim
            other = embeddingSpan.Slice(other_start, _optimizationState.Dim)
            distSquared = DistanceMethods.SquareDistance(current, other)
            gradCoeff = 0F

            If (distSquared > 0) Then
                ' Preparation For future work For interpolating the table before optimizing
                gradCoeff = 2 * _optimizationState.Gamma * _optimizationState.B
                gradCoeff *= _optimizationState.GetDistanceFactor(distSquared)
            ElseIf (j = k) Then
                Continue For
            End If

            For d As Integer = 0 To _optimizationState.Dim - 1
                gradD = clipValue

                If (gradCoeff > 0) Then
                    gradD = Math.Clip(gradCoeff * (current(d) - other(d)), clipValue)
                End If

                current(d) += gradD * _optimizationState.Alpha
            Next
        Next

        _optimizationState.EpochOfNextNegativeSample(i) += nNegSamples * _optimizationState.EpochsPerNegativeSample(i)
    End Sub

    Public Shared Sub SetMaxTreeDepth(maxDepth As Integer)
        Tree.maxDepth = maxDepth
    End Sub
End Class
