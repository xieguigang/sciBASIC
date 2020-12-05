Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks

Public NotInheritable Class Umap
    Private Const SMOOTH_K_TOLERANCE As Single = 0.00001F
    Private Const MIN_K_DIST_SCALE As Single = 0.001F
    Private ReadOnly _learningRate As Single = 1.0F
    Private ReadOnly _localConnectivity As Single = 1.0F
    Private ReadOnly _minDist As Single = 0.1F
    Private ReadOnly _negativeSampleRate As Integer = 5
    Private ReadOnly _repulsionStrength As Single = 1
    Private ReadOnly _setOpMixRatio As Single = 1
    Private ReadOnly _spread As Single = 1
    Private ReadOnly _distanceFn As UMAP.DistanceCalculation
    Private ReadOnly _random As UMAP.IProvideRandomValues
    Private ReadOnly _nNeighbors As Integer
    Private ReadOnly _customNumberOfEpochs As Integer?
    Private ReadOnly _progressReporter As UMAP.Umap.ProgressReporter

    ' KNN state (can be precomputed and supplied via initializeFit)
    Private _knnIndices As Integer()() = Nothing
    Private _knnDistances As Single()() = Nothing

    ' Internal graph connectivity representation
    Private _graph As UMAP.SparseMatrix = Nothing
    Private _x As Single()() = Nothing
    Private _isInitialized As Boolean = False
    Private _rpForest As UMAP.Tree.FlatTree() = New UMAP.Tree.FlatTree(-1) {}

    ' Projected embedding
    Private _embedding As Single()
    Private ReadOnly _optimizationState As UMAP.Umap.OptimizationState

    ''' <summary>
    ''' The progress will be a value from 0 to 1 that indicates approximately how much of the processing has been completed
    ''' </summary>
    Public Delegate Sub ProgressReporter(ByVal progress As Single)

    Public Sub New(ByVal Optional distance As UMAP.DistanceCalculation = Nothing, ByVal Optional random As UMAP.IProvideRandomValues = Nothing, ByVal Optional dimensions As Integer = 2, ByVal Optional numberOfNeighbors As Integer = 15, ByVal Optional customNumberOfEpochs As Integer? = Nothing, ByVal Optional progressReporter As UMAP.Umap.ProgressReporter = Nothing)
        If customNumberOfEpochs IsNot Nothing AndAlso customNumberOfEpochs <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(customNumberOfEpochs), "if non-null then must be a positive value")
        _distanceFn = If(distance, AddressOf UMAP.Umap.DistanceFunctions.Cosine)
        _random = If(random, UMAP.DefaultRandomGenerator.Instance)
        _nNeighbors = numberOfNeighbors
        _optimizationState = New UMAP.Umap.OptimizationState With {
            .[Dim] = dimensions
        }
        _customNumberOfEpochs = customNumberOfEpochs
        _progressReporter = progressReporter
    End Sub

    ''' <summary>
    ''' Initializes fit by computing KNN and a fuzzy simplicial set, as well as initializing the projected embeddings. Sets the optimization state ahead of optimization steps.
    ''' Returns the number of epochs to be used for the SGD optimization.
    ''' </summary>
    Public Function InitializeFit(ByVal x As Single()()) As Integer
        ' We don't need to reinitialize if we've already initialized for this data
        If _x Is x AndAlso _isInitialized Then Return GetNEpochs()

        ' For large quantities of data (which is where the progress estimating is more useful), InitializeFit takes at least 80% of the total time (the calls to Step are
        ' completed much more quickly AND they naturally lend themselves to granular progress updates; one per loop compared to the recommended number of epochs)
        Dim initializeFitProgressReporter As UMAP.Umap.ProgressReporter = If(_progressReporter Is Nothing, Sub(progress)
                                                                                                           End Sub, UMAP.Umap.ScaleProgressReporter(_progressReporter, 0, 0.8F))
        _x = x

        If _knnIndices Is Nothing AndAlso _knnDistances Is Nothing Then
                ' This part of the process very roughly accounts for 1/3 of the work
                (_knnIndices, _knnDistances) = Me.NearestNeighbors(x, UMAP.Umap.ScaleProgressReporter(initializeFitProgressReporter, 0, 0.3F))
            End If

        ' This part of the process very roughly accounts for 2/3 of the work (the reamining work is in the Step calls)
        _graph = Me.FuzzySimplicialSet(x, _nNeighbors, _setOpMixRatio, UMAP.Umap.ScaleProgressReporter(initializeFitProgressReporter, 0.3F, 1))
        Dim headTailEpochsPerSample = Nothing
        headTailEpochsPerSample = InitializeSimplicialSetEmbedding()

        ' Set the optimization routine state
        _optimizationState.Head = head
        _optimizationState.Tail = tail
        _optimizationState.EpochsPerSample = epochsPerSample

        ' Now, initialize the optimization steps
        InitializeOptimization()
        PrepareForOptimizationLoop()
        _isInitialized = True
        Return GetNEpochs()
    End Function

    Public Function GetEmbedding() As Single()()
        Dim final = New Single(_optimizationState.NVertices - 1)() {}
        Dim span As Span(Of Single) = _embedding.AsSpan()

        For i As Integer = 0 To _optimizationState.NVertices - 1
            final(i) = span.Slice(CInt(i * _optimizationState.Dim), CInt(_optimizationState.Dim)).ToArray()
        Next

        Return final
    End Function

    ''' <summary>
    ''' Gets the number of epochs for optimizing the projection - NOTE: This heuristic differs from the python version
    ''' </summary>
    Private Function GetNEpochs() As Integer
        If _customNumberOfEpochs IsNot Nothing Then Return _customNumberOfEpochs.Value
        Dim length = _graph.Dims.rows

        If length <= 2500 Then
            Return 500
        ElseIf length <= 5000 Then
            Return 400
        ElseIf length <= 7500 Then
            Return 300
        Else
            Return 200
        End If
    End Function

    ''' <summary>
    ''' Compute the ``nNeighbors`` nearest points for each data point in ``X`` - this may be exact, but more likely is approximated via nearest neighbor descent.
    ''' </summary>
    Friend Function NearestNeighbors(ByVal x As Single()(), ByVal progressReporter As UMAP.Umap.ProgressReporter) As (Integer()(), Single()())
        Dim metricNNDescent = UMAP.NNDescent.MakeNNDescent(_distanceFn, _random)
        progressReporter(0.05F)
        Dim nTrees = 5 + Round(Math.Sqrt(x.Length) / 20)
        Dim nIters = Math.Max(5, CInt(Math.Floor(Math.Round(Math.Log(x.Length, 2)))))
        progressReporter(0.1F)
        Dim leafSize = Math.Max(10, _nNeighbors)
        Dim forestProgressReporter = UMAP.Umap.ScaleProgressReporter(progressReporter, 0.1F, 0.4F)
        _rpForest = Enumerable.Range(0, nTrees).[Select](Function(i)
                                                             forestProgressReporter(CSng(i) / nTrees)
                                                             Return UMAP.Tree.FlattenTree(UMAP.Tree.MakeTree(x, leafSize, i, _random), leafSize)
                                                         End Function).ToArray()
        Dim leafArray = UMAP.Tree.MakeLeafArray(_rpForest)
        progressReporter(0.45F)
        Dim nnDescendProgressReporter = UMAP.Umap.ScaleProgressReporter(progressReporter, 0.5F, 1)

        ' Handle python3 rounding down from 0.5 discrpancy
        Return metricNNDescent(x, leafArray, _nNeighbors, nIters, startingIteration:=Sub(i, max) nnDescendProgressReporter(CSng(i) / max))
                        ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'int Round(double n) => (n =...' at character 7796
''' 
''' 
''' Input:
''' 
            // Handle python3 rounding down from 0.5 discrpancy
            Int Round(Double n) >= (n == 0.5) ? 0 : (int)System.Math.Floor(System.Math.Round(n));

''' 
        End Function

    ''' <summary>
    ''' Given a set of data X, a neighborhood size, and a measure of distance compute the fuzzy simplicial set(here represented as a fuzzy graph in the form of a sparse matrix) associated
    ''' to the data. This is done by locally approximating geodesic distance at each point, creating a fuzzy simplicial set for each such point, and then combining all the local fuzzy
    ''' simplicial sets into a global one via a fuzzy union.
    ''' </summary>
    Private Function FuzzySimplicialSet(ByVal x As Single()(), ByVal nNeighbors As Integer, ByVal setOpMixRatio As Single, ByVal progressReporter As UMAP.Umap.ProgressReporter) As UMAP.SparseMatrix
        Dim knnIndices = If(_knnIndices, New Integer(-1)() {})
        Dim knnDistances = If(_knnDistances, New Single(-1)() {})
        progressReporter(0.1F)
        Dim sigmasRhos = Nothing
        sigmasRhos = UMAP.Umap.SmoothKNNDistance(knnDistances, nNeighbors, _localConnectivity)
        progressReporter(0.2F)
        Dim rowsColsVals = Nothing
        rowsColsVals = UMAP.Umap.ComputeMembershipStrengths(knnIndices, knnDistances, sigmas, rhos)
        progressReporter(0.3F)
        Dim sparseMatrix = New UMAP.SparseMatrix(rows, cols, vals, (x.Length, x.Length))
        Dim transpose = sparseMatrix.Transpose()
        Dim prodMatrix = sparseMatrix.PairwiseMultiply(transpose)
        progressReporter(0.4F)
        Dim a = sparseMatrix.Add(CType(transpose, SparseMatrix)).Subtract(prodMatrix)
        progressReporter(0.5F)
        Dim b = a.MultiplyScalar(setOpMixRatio)
        progressReporter(0.6F)
        Dim c = prodMatrix.MultiplyScalar(1 - setOpMixRatio)
        progressReporter(0.7F)
        Dim result = b.Add(c)
        progressReporter(0.8F)
        Return result
    End Function

    Private Shared Function SmoothKNNDistance(ByVal distances As Single()(), ByVal k As Integer, ByVal Optional localConnectivity As Single = 1, ByVal Optional nIter As Integer = 64, ByVal Optional bandwidth As Single = 1) As (Single(), Single())
        Dim target = Math.Log(k, 2) * bandwidth ' TODO: Use Math.Log2 (when update framework to a version that supports it) or consider a pre-computed table
        Dim rho = New Single(distances.Length - 1) {}
        Dim result = New Single(distances.Length - 1) {}

        For i = 0 To distances.Length - 1
            Dim lo = 0F
            Dim hi = Single.MaxValue
            Dim mid = 1.0F

            ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
            Dim ithDistances = distances(i)
            Dim nonZeroDists = ithDistances.Where(Function(d) d > 0).ToArray()

            If nonZeroDists.Length >= localConnectivity Then
                Dim index = CInt(Math.Floor(localConnectivity))
                Dim interpolation = localConnectivity - index

                If index > 0 Then
                    rho(i) = nonZeroDists(index - 1)
                    If interpolation > UMAP.Umap.SMOOTH_K_TOLERANCE Then rho(i) += interpolation * (nonZeroDists(index) - nonZeroDists(index - 1))
                Else
                    rho(i) = interpolation * nonZeroDists(0)
                End If
            ElseIf nonZeroDists.Length > 0 Then
                rho(i) = UMAP.Utils.Max(nonZeroDists)
            End If

            For n = 0 To nIter - 1
                Dim psum = 0.0

                For j = 1 To distances(i).Length - 1
                    Dim d = distances(i)(j) - rho(i)

                    If d > 0 Then
                        psum += Math.Exp(-(d / mid))
                    Else
                        psum += 1.0
                    End If
                Next

                If Math.Abs(psum - target) < UMAP.Umap.SMOOTH_K_TOLERANCE Then Exit For

                If psum > target Then
                    hi = mid
                    mid = (lo + hi) / 2
                Else
                    lo = mid

                    If hi = Single.MaxValue Then
                        mid *= 2
                    Else
                        mid = (lo + hi) / 2
                    End If
                End If
            Next

            result(i) = mid

            ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
            If rho(i) > 0 Then
                Dim meanIthDistances = UMAP.Utils.Mean(ithDistances)
                If result(i) < UMAP.Umap.MIN_K_DIST_SCALE * meanIthDistances Then result(i) = UMAP.Umap.MIN_K_DIST_SCALE * meanIthDistances
            Else
                Dim meanDistances = UMAP.Utils.Mean(distances.[Select](New Func(Of Single(), Single)(AddressOf UMAP.Utils.Mean)).ToArray())
                If result(i) < UMAP.Umap.MIN_K_DIST_SCALE * meanDistances Then result(i) = UMAP.Umap.MIN_K_DIST_SCALE * meanDistances
            End If
        Next

        Return (result, rho)
    End Function

    Private Shared Function ComputeMembershipStrengths(ByVal knnIndices As Integer()(), ByVal knnDistances As Single()(), ByVal sigmas As Single(), ByVal rhos As Single()) As (Integer(), Integer(), Single())
        Dim nSamples = knnIndices.Length
        Dim nNeighbors = knnIndices(0).Length
        Dim rows = New Integer(nSamples * nNeighbors - 1) {}
        Dim cols = New Integer(nSamples * nNeighbors - 1) {}
        Dim vals = New Single(nSamples * nNeighbors - 1) {}

        For i = 0 To nSamples - 1

            For j = 0 To nNeighbors - 1
                If knnIndices(i)(j) = -1 Then Continue For ' We didn't get the full knn for i
                Dim val As Single

                If knnIndices(i)(j) = i Then
                    val = 0
                ElseIf knnDistances(i)(j) - rhos(i) <= 0.0 Then
                    val = 1
                Else
                    val = CSng(Math.Exp(-((knnDistances(i)(j) - rhos(i)) / sigmas(i))))
                End If

                rows(i * nNeighbors + j) = i
                cols(i * nNeighbors + j) = knnIndices(i)(j)
                vals(i * nNeighbors + j) = val
            Next
        Next

        Return (rows, cols, vals)
    End Function

    ''' <summary>
    ''' Initialize a fuzzy simplicial set embedding, using a specified initialisation method and then minimizing the fuzzy set cross entropy between the 1-skeletons of the high and low
    ''' dimensional fuzzy simplicial sets.
    ''' </summary>
    Private Function InitializeSimplicialSetEmbedding() As (Integer(), Integer(), Single())
        Dim nEpochs = GetNEpochs()
        Dim graphMax = 0F

        For Each value In _graph.GetValues()
            If graphMax < value Then graphMax = value
        Next

        Dim graph = _graph.Map(Function(value) If(value < graphMax / nEpochs, 0, value))

        ' We're not computing the spectral initialization in this implementation until we determine a better eigenvalue/eigenvector computation approach

        _embedding = New Single(graph.Dims.rows * _optimizationState.Dim - 1) {}
        UMAP.SIMDint.Uniform(_embedding, 10, _random)

        ' Get graph data in ordered way...
        Dim weights = New List(Of Single)()
        Dim head = New List(Of Integer)()
        Dim tail = New List(Of Integer)()

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

        ShuffleTogether(head, tail, weights)
        Return (head.ToArray(), tail.ToArray(), UMAP.Umap.MakeEpochsPerSample(weights.ToArray(), nEpochs))
    End Function

    Private Sub ShuffleTogether(Of T, T2, T3)(ByVal list As List(Of T), ByVal other As List(Of T2), ByVal weights As List(Of T3))
        Dim n = list.Count

        If other.Count <> n Then
            Throw New Exception()
        End If

        While n > 1
            n -= 1
            Dim k As Integer = _random.Next(0, n + 1)
            Dim value = list(k)
            list(k) = list(n)
            list(n) = value
            Dim otherValue = other(k)
            other(k) = other(n)
            other(n) = otherValue
            Dim weightsValue = weights(k)
            weights(k) = weights(n)
            weights(n) = weightsValue
        End While
    End Sub

    Private Shared Function MakeEpochsPerSample(ByVal weights As Single(), ByVal nEpochs As Integer) As Single()
        Dim result = UMAP.Utils.Filled(weights.Length, -1)
        Dim max = UMAP.Utils.Max(weights)

        For Each nI In weights.Select(Function(w, i) (w / max * nEpochs, i))
            Dim n = nI.Item1
            Dim i = nI.Item2
            If n > 0 Then result(i) = nEpochs / n
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
        Dim aB = Nothing
        aB = UMAP.Umap.FindABParams(_spread, _minDist)
        _optimizationState.Head = head
        _optimizationState.Tail = tail
        _optimizationState.EpochsPerSample = epochsPerSample
        _optimizationState.A = a
        _optimizationState.B = b
        _optimizationState.NEpochs = nEpochs
        _optimizationState.NVertices = nVertices
    End Sub

    Friend Shared Function FindABParams(ByVal spread As Single, ByVal minDist As Single) As (Single, Single)
        ' 2019-06-21 DWR: If we need to support other spread, minDist values then we might be able to use the LM implementation in Accord.NET but I'll hard code values that relate to the default configuration for now
        If spread <> 1 OrElse minDist <> 0.1F Then Throw New ArgumentException($"Currently, the {NameOf(FindABParams)} method only supports spread, minDist values of 1, 0.1 (the Levenberg-Marquardt algorithm is required to process other values")
        Return (1.56947052F, 0.8941996F)
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
        _optimizationState.MoveOther = True
        _optimizationState.InitialAlpha = learningRate
        _optimizationState.Alpha = learningRate
        _optimizationState.Gamma = repulsionStrength
        _optimizationState.Dim = [dim]
    End Sub

    ''' <summary>
    ''' Manually step through the optimization process one epoch at a time
    ''' </summary>
    Public Function [Step]() As Integer
        Dim currentEpoch = _optimizationState.CurrentEpoch
        Dim numberOfEpochsToComplete = GetNEpochs()

        If currentEpoch < numberOfEpochsToComplete Then
            Me.OptimizeLayoutStep(currentEpoch)

            If _progressReporter IsNot Nothing Then
                ' InitializeFit roughly approximately takes 80% of the processing time for large quantities of data, leaving 20% for the Step iterations - the progress reporter
                ' calls made here are based on the assumption that Step will be called the recommended number of times (the number-of-epochs value returned from InitializeFit)
                UMAP.Umap.ScaleProgressReporter(_progressReporter, 0.8F, 1)(CSng(currentEpoch) / numberOfEpochsToComplete)
            End If
        End If

        Return _optimizationState.CurrentEpoch
    End Function

    ''' <summary>
    ''' Improve an embedding using stochastic gradient descent to minimize the fuzzy set cross entropy between the 1-skeletons of the high dimensional and low dimensional fuzzy simplicial sets.
    ''' In practice this is done by sampling edges based on their membership strength(with the (1-p) terms coming from negative sampling similar to word2vec).
    ''' </summary>
    Private Sub OptimizeLayoutStep(ByVal n As Integer)
        If _random.IsThreadSafe Then
            Parallel.For(0, _optimizationState.EpochsPerSample.Length, New Action(Of Integer)(AddressOf Iterate))
        Else

            For i = 0 To _optimizationState.EpochsPerSample.Length - 1
                Iterate(i)
            Next
        End If

        _optimizationState.Alpha = _optimizationState.InitialAlpha * (1.0F - n / _optimizationState.NEpochs)
        _optimizationState.CurrentEpoch += 1 'Preparation for future work for interpolating the table before optimizing
                        ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'void Iterate(int i)
   {
  ...' at character 22131
        ''' 
        ''' 
        ''' Input:
        ''' 
        void Iterate(Int i)
            {
                If (this._optimizationState.EpochOfNextSample[i] >= n)
                    Return;

                System.Span<float> embeddingSpan = this._embedding.AsSpan();

                int j = this._optimizationState.Head[i];
                int k = this._optimizationState.Tail[i];

                var current = embeddingSpan.Slice(j * this._optimizationState.Dim, this._optimizationState.Dim);
                var other = embeddingSpan.Slice(k * this._optimizationState.Dim, this._optimizationState.Dim);

                var distSquared = UMAP.Umap.RDist(current, other);
                var gradCoeff = 0f;

                if (distSquared > 0)
                {
                    gradCoeff = -2 * this._optimizationState.A * this._optimizationState.B * (float)System.Math.Pow(distSquared, this._optimizationState.B - 1);
                    gradCoeff /= this._optimizationState.A * (float)System.Math.Pow(distSquared, this._optimizationState.B) + 1;
                }

                const float clipValue = 4f;
                for (var d = 0; d <this._optimizationState.Dim; d++)
                {
                    var gradD = UMAP.Umap.Clip(gradCoeff * (current[d] - other[d]), clipValue);
                    current[d] += gradD * this._optimizationState.Alpha;
                    if (this._optimizationState.MoveOther)
                        other[d] += -gradD * this._optimizationState.Alpha;
                }

                this._optimizationState.EpochOfNextSample[i] += this._optimizationState.EpochsPerSample[i];

                var nNegSamples = (int)System.Math.Floor((double)(n - this._optimizationState.EpochOfNextNegativeSample[i]) / this._optimizationState.EpochsPerNegativeSample[i]);

                for (var p = 0; p <nNegSamples; p++)
                {
                    k = this._random.Next(0, this._optimizationState.NVertices);
                    other = embeddingSpan.Slice(k * this._optimizationState.Dim, this._optimizationState.Dim);
                    distSquared = UMAP.Umap.RDist(current, other);
                    gradCoeff = 0f;
                    if (distSquared > 0)
                    {
                        gradCoeff = 2 * this._optimizationState.Gamma * this._optimizationState.B;
                        gradCoeff *= this._optimizationState.GetDistanceFactor(distSquared); //Preparation for future work for interpolating the table before optimizing
                    }
                    else if (j == k)
                        continue;

                    for (var d = 0; d <this._optimizationState.Dim; d++)
                    {
                        var gradD = 4f;
                        if (gradCoeff > 0)
                            gradD = UMAP.Umap.Clip(gradCoeff * (current[d] - other[d]), clipValue);
                        current[d] += gradD * this._optimizationState.Alpha;
                    }
                }

                this._optimizationState.EpochOfNextNegativeSample[i] += nNegSamples * this._optimizationState.EpochsPerNegativeSample[i];
            }

''' 
        End Sub

        ''' <summary>
        ''' Reduced Euclidean distance
        ''' </summary>
        Private Shared Function RDist(ByVal x As Span(Of Single), ByVal y As Span(Of Single)) As Single
            'return Mosaik.Core.SIMD.Euclidean(ref x, ref y);
            Dim distSquared = 0F

            For i = 0 To x.Length - 1
                Dim d = x(i) - y(i)
                distSquared += d * d
            Next

            Return distSquared
        End Function

        ''' <summary>
        ''' Standard clamping of a value into a fixed range
        ''' </summary>
        Private Shared Function Clip(ByVal x As Single, ByVal clipValue As Single) As Single
            If x > clipValue Then
                Return clipValue
            ElseIf x <-clipValue Then
                Return -clipValue
            Else
                Return x
        End If
        End Function

        Private Shared Function ScaleProgressReporter(ByVal progressReporter As UMAP.Umap.ProgressReporter, ByVal start As Single, ByVal [end] As Single) As UMAP.Umap.ProgressReporter
        Dim range = [end] - start
        Return Sub(progress) progressReporter(range * progress + start)
    End Function

    Public NotInheritable Class DistanceFunctions
        Public Shared Function Cosine(ByVal lhs As Single(), ByVal rhs As Single()) As Single
            Return 1 - UMAP.SIMD.DotProduct(lhs, rhs) / (UMAP.SIMD.Magnitude(lhs) * UMAP.SIMD.Magnitude(rhs))
        End Function

        Public Shared Function CosineForNormalizedVectors(ByVal lhs As Single(), ByVal rhs As Single()) As Single
            Return 1 - UMAP.SIMD.DotProduct(lhs, rhs)
        End Function

        Public Shared Function Euclidean(ByVal lhs As Single(), ByVal rhs As Single()) As Single
            Return Math.Sqrt(UMAP.SIMD.Euclidean(lhs, rhs)) ' TODO: Replace with netcore3 MathF class when the framework is available
        End Function
    End Class

    Private NotInheritable Class OptimizationState
        Public CurrentEpoch As Integer = 0
        Public Head As Integer() = New Integer(-1) {}
        Public Tail As Integer() = New Integer(-1) {}
        Public EpochsPerSample As Single() = New Single(-1) {}
        Public EpochOfNextSample As Single() = New Single(-1) {}
        Public EpochOfNextNegativeSample As Single() = New Single(-1) {}
        Public EpochsPerNegativeSample As Single() = New Single(-1) {}
        Public MoveOther As Boolean = True
        Public InitialAlpha As Single = 1
        Public Alpha As Single = 1
        Public Gamma As Single = 1
        Public A As Single = 1.57694352F
        Public B As Single = 0.8950609F
        Public [Dim] As Integer = 2
        Public NEpochs As Integer = 500
        Public NVertices As Integer = 0

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDistanceFactor(ByVal distSquared As Single) As Single
            Return 1.0F / ((0.001F + distSquared) * CSng(A * Math.Pow(distSquared, B) + 1))
        End Function
    End Class
End Class
