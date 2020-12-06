#Region "Microsoft.VisualBasic::be8d2c2dd97689f3f7f07059ae7662ae, Data_science\DataMining\UMAP\Umap.vb"

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

' Class Umap
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: [Step], Clip, ComputeMembershipStrengths, FindABParams, FuzzySimplicialSet
'               GetEmbedding, GetNEpochs, GetProgress, InitializeFit, InitializeSimplicialSetEmbedding
'               MakeEpochsPerSample, NearestNeighbors, RDist, Round, ScaleProgressReporter
'               SmoothKNNDistance
' 
'     Sub: InitializeOptimization, Iterate, OptimizeLayoutStep, PrepareForOptimizationLoop, RunIterate
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.Math
Imports stdNum = System.Math

''' <summary>
''' Umap projection algorithm module
''' </summary>
''' <remarks>
''' https://github.com/curiosity-ai/umap-sharp
''' </remarks>
Public NotInheritable Class Umap

    Const SMOOTH_K_TOLERANCE As Double = 0.00001F
    Const MIN_K_DIST_SCALE As Double = 0.001F

    ReadOnly _learningRate As Double = 1.0F
    ReadOnly _localConnectivity As Double = 1.0F
    ReadOnly _minDist As Double = 0.1F
    ReadOnly _negativeSampleRate As Integer = 5
    ReadOnly _repulsionStrength As Double = 1
    ReadOnly _setOpMixRatio As Double = 1
    ReadOnly _spread As Double = 1
    ReadOnly _distanceFn As DistanceCalculation
    ReadOnly _random As IProvideRandomValues
    ReadOnly _nNeighbors As Integer
    ReadOnly _customNumberOfEpochs As Integer?
    ReadOnly _progressReporter As IProgressReporter
    ReadOnly _optimizationState As OptimizationState

    ''' <summary>
    ''' KNN state (can be precomputed and supplied via initializeFit)
    ''' </summary>
    ReadOnly knn As New KNNState

    ''' <summary>
    ''' Internal graph connectivity representation
    ''' </summary>
    Private _graph As SparseMatrix = Nothing
    Private _x As Double()() = Nothing
    Private _isInitialized As Boolean = False
    Private _rpForest As Tree.FlatTree() = New Tree.FlatTree(-1) {}

    ''' <summary>
    ''' Projected embedding
    ''' </summary>
    Dim _embedding As Double()

    Public ReadOnly Property dimension As Integer
        Get
            Return _optimizationState.Dim
        End Get
    End Property

    Public Sub New(Optional distance As DistanceCalculation = Nothing,
                   Optional random As IProvideRandomValues = Nothing,
                   Optional dimensions As Integer = 2,
                   Optional numberOfNeighbors As Integer = 15,
                   Optional customNumberOfEpochs As Integer? = Nothing,
                   Optional progressReporter As IProgressReporter = Nothing)

        If customNumberOfEpochs IsNot Nothing AndAlso customNumberOfEpochs <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(customNumberOfEpochs), "if non-null then must be a positive value")
        End If

        _distanceFn = If(distance, AddressOf DistanceFunctions.Cosine)
        _random = If(random, DefaultRandomGenerator.Instance)
        _nNeighbors = numberOfNeighbors
        _optimizationState = New OptimizationState With {
            .[Dim] = dimensions
        }
        _customNumberOfEpochs = customNumberOfEpochs
        _progressReporter = progressReporter
    End Sub

    Private Function GetProgress() As IProgressReporter
        If _progressReporter Is Nothing Then
            Return Sub(progress)
                       ' do nothing
                   End Sub
        Else
            Return Umap.ScaleProgressReporter(_progressReporter, 0, 0.8F)
        End If
    End Function

    Public Function GetGraph() As SparseMatrix
        Return _graph
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
        End If

        ' For large quantities of data (which is where the progress estimating is more useful), 
        ' InitializeFit Takes at least 80% of the total time (the calls to Step are
        ' completed much more quickly AND they naturally lend themselves to granular progress updates; 
        ' one per loop compared to the recommended number of epochs)
        Dim initializeFitProgressReporter As IProgressReporter = GetProgress()

        _x = x

        If knn._knnIndices Is Nothing AndAlso knn._knnDistances Is Nothing Then
            ' This part of the process very roughly accounts for 1/3 of the work
            With Me.NearestNeighbors(x, Umap.ScaleProgressReporter(initializeFitProgressReporter, 0, 0.3F))
                knn._knnIndices = .knnIndices
                knn._knnDistances = .knnDistances
            End With
        End If

        ' This part of the process very roughly accounts for 2/3 of the work (the reamining work is in the Step calls)
        _graph = Me.FuzzySimplicialSet(
            x:=x,
            nNeighbors:=_nNeighbors,
            setOpMixRatio:=_setOpMixRatio,
            progressReporter:=ScaleProgressReporter(initializeFitProgressReporter, 0.3F, 1)
        )

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
    ''' get projection result
    ''' </summary>
    ''' <returns></returns>
    Public Function GetEmbedding() As Double()()
        Dim final As Double()() = New Double(_optimizationState.NVertices - 1)() {}
        Dim span As Double() = _embedding

        For i As Integer = 0 To _optimizationState.NVertices - 1
            ' slice函数需要进行验证
            final(i) = span.SpanSlice(i * _optimizationState.Dim, _optimizationState.Dim).ToArray()
        Next

        Return final
    End Function

    ''' <summary>
    ''' Gets the number of epochs for optimizing the projection - NOTE: This heuristic differs from the python version
    ''' </summary>
    Private Function GetNEpochs() As Integer
        Dim length = _graph.Dims.rows

        If _customNumberOfEpochs IsNot Nothing Then
            Return _customNumberOfEpochs.Value
        End If

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
    Friend Function NearestNeighbors(x As Double()(), progressReporter As IProgressReporter) As (knnIndices As Integer()(), knnDistances As Double()())
        Dim metricNNDescent = New NNDescent(_distanceFn, _random)

        Call progressReporter(0.05F)

        Dim nTrees = 5 + Round(stdNum.Sqrt(x.Length) / 20)
        Dim nIters = stdNum.Max(5, CInt(stdNum.Floor(stdNum.Round(stdNum.Log(x.Length, 2)))))

        Call progressReporter(0.1F)

        Dim leafSize = stdNum.Max(10, _nNeighbors)
        Dim forestProgressReporter = Umap.ScaleProgressReporter(progressReporter, 0.1F, 0.4F)

        _rpForest = Enumerable.Range(0, nTrees) _
            .[Select](Function(i)
                          forestProgressReporter(CSng(i) / nTrees)
                          Return Tree.FlattenTree(Tree.MakeTree(x, leafSize, i, _random), leafSize)
                      End Function) _
            .ToArray()

        Dim leafArray = Tree.MakeLeafArray(_rpForest)

        Call progressReporter(0.45F)

        Dim nnDescendProgressReporter = Umap.ScaleProgressReporter(progressReporter, 0.5F, 1)

        ' Handle python3 rounding down from 0.5 discrpancy
        Return metricNNDescent.MakeNNDescent(x, leafArray, _nNeighbors, nIters, startingIteration:=Sub(i, max) nnDescendProgressReporter(CSng(i) / max))
    End Function

    ''' <summary>
    ''' Handle python3 rounding down from 0.5 discrpancy
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Private Shared Function Round(n As Double) As Integer
        If n = 0.5 Then
            Return 0
        Else
            Return stdNum.Floor(stdNum.Round(n))
        End If
    End Function

    ''' <summary>
    ''' Given a set of data X, a neighborhood size, and a measure of distance compute the fuzzy simplicial set(here represented as a fuzzy graph in the form of a sparse matrix) associated
    ''' to the data. This is done by locally approximating geodesic distance at each point, creating a fuzzy simplicial set for each such point, and then combining all the local fuzzy
    ''' simplicial sets into a global one via a fuzzy union.
    ''' </summary>
    Private Function FuzzySimplicialSet(x As Double()(), nNeighbors As Integer, setOpMixRatio As Double, progressReporter As IProgressReporter) As SparseMatrix
        Dim knnIndices = If(knn._knnIndices, New Integer(-1)() {})
        Dim knnDistances = If(knn._knnDistances, New Single(-1)() {})
        Dim report As New ProgressReporter With {.report = progressReporter}
        Dim sigmasRhos = report.Run(Function() Umap.SmoothKNNDistance(knnDistances, nNeighbors, _localConnectivity), 0.1)
        Dim rowsColsVals = report.Run(Function() Umap.ComputeMembershipStrengths(knnIndices, knnDistances, sigmasRhos.sigmas, sigmasRhos.rhos), 0.2)
        Dim sparseMatrix = report.Run(Function() New SparseMatrix(rowsColsVals.rows, rowsColsVals.cols, rowsColsVals.vals, (x.Length, x.Length)), 0.3)
        Dim transpose = sparseMatrix.Transpose()
        Dim prodMatrix = sparseMatrix.PairwiseMultiply(transpose)
        Dim a = report.Run(Function() sparseMatrix.Add(CType(transpose, SparseMatrix)).Subtract(prodMatrix), 0.4)
        Dim b = report.Run(Function() a.MultiplyScalar(setOpMixRatio), 0.5)
        Dim c = report.Run(Function() prodMatrix.MultiplyScalar(1 - setOpMixRatio), 0.6)
        Dim result = report.Run(Function() b.Add(c), 0.7)

        Return result
    End Function

    Private Shared Function SmoothKNNDistance(distances As Double()(), k As Integer,
                                              Optional localConnectivity As Double = 1,
                                              Optional nIter As Integer = 64,
                                              Optional bandwidth As Double = 1) As (sigmas As Double(), rhos As Double())

        ' TODO: Use Math.Log2 (when update framework to a version that supports it) or consider a pre-computed table
        Dim target = stdNum.Log(k, 2) * bandwidth
        Dim rho = New Double(distances.Length - 1) {}
        Dim result = New Double(distances.Length - 1) {}

        For i As Integer = 0 To distances.Length - 1
            Dim lo = 0F
            Dim hi = Single.MaxValue
            Dim mid = 1.0F

            ' TODO[umap-js]: This is very inefficient, but will do for now. FIXME
            Dim ithDistances = distances(i)
            Dim nonZeroDists = ithDistances.Where(Function(d) d > 0).ToArray()

            If nonZeroDists.Length >= localConnectivity Then
                Dim index = CInt(stdNum.Floor(localConnectivity))
                Dim interpolation = localConnectivity - index

                If index > 0 Then
                    rho(i) = nonZeroDists(index - 1)

                    If interpolation > Umap.SMOOTH_K_TOLERANCE Then
                        rho(i) += interpolation * (nonZeroDists(index) - nonZeroDists(index - 1))
                    End If
                Else
                    rho(i) = interpolation * nonZeroDists(0)
                End If
            ElseIf nonZeroDists.Length > 0 Then
                rho(i) = nonZeroDists.Max
            End If

            For n As Integer = 0 To nIter - 1
                Dim psum = 0.0

                For j = 1 To distances(i).Length - 1
                    Dim d = distances(i)(j) - rho(i)

                    If d > 0 Then
                        psum += stdNum.Exp(-(d / mid))
                    Else
                        psum += 1.0
                    End If
                Next

                If stdNum.Abs(psum - target) < Umap.SMOOTH_K_TOLERANCE Then
                    Exit For
                End If

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
                Dim meanIthDistances = ithDistances.Average

                If result(i) < Umap.MIN_K_DIST_SCALE * meanIthDistances Then
                    result(i) = Umap.MIN_K_DIST_SCALE * meanIthDistances
                End If
            Else
                Dim meanDistances = distances.Select(Function(d) d.Average).Average

                If result(i) < Umap.MIN_K_DIST_SCALE * meanDistances Then
                    result(i) = Umap.MIN_K_DIST_SCALE * meanDistances
                End If
            End If
        Next

        Return (result, rho)
    End Function

    Private Shared Function ComputeMembershipStrengths(knnIndices As Integer()(), knnDistances As Double()(), sigmas As Double(), rhos As Double()) As (rows As Integer(), cols As Integer(), vals As Double())
        Dim nSamples = knnIndices.Length
        Dim nNeighbors = knnIndices(0).Length
        Dim rows = New Integer(nSamples * nNeighbors - 1) {}
        Dim cols = New Integer(nSamples * nNeighbors - 1) {}
        Dim vals = New Double(nSamples * nNeighbors - 1) {}
        Dim val As Double

        For i = 0 To nSamples - 1
            For j = 0 To nNeighbors - 1
                If knnIndices(i)(j) = -1 Then
                    ' We didn't get the full knn for i
                    Continue For
                End If

                If knnIndices(i)(j) = i Then
                    val = 0
                ElseIf knnDistances(i)(j) - rhos(i) <= 0.0 Then
                    val = 1
                Else
                    val = CSng(stdNum.Exp(-((knnDistances(i)(j) - rhos(i)) / sigmas(i))))
                End If

                rows(i * nNeighbors + j) = i
                cols(i * nNeighbors + j) = knnIndices(i)(j)
                vals(i * nNeighbors + j) = val
            Next
        Next

        Return (rows, cols, vals)
    End Function

    ''' <summary>
    ''' Initialize a fuzzy simplicial set embedding, using a specified initialisation method and then minimizing the 
    ''' fuzzy set cross entropy between the 1-skeletons of the high and low dimensional fuzzy simplicial sets.
    ''' </summary>
    Private Function InitializeSimplicialSetEmbedding() As (head As Integer(), tail As Integer(), epochsPerSample As Double())
        Dim nEpochs = GetNEpochs()
        Dim graphMax = 0F

        For Each value In _graph.GetValues()
            If graphMax < value Then
                graphMax = value
            End If
        Next

        Dim graph = _graph.Map(Function(value) If(value < graphMax / nEpochs, 0, value))

        ' We're not computing the spectral initialization in this implementation 
        ' until we determine a better eigenvalue/eigenvector computation approach
        _embedding = New Double(graph.Dims.rows * _optimizationState.Dim - 1) {}

        Call SIMDint.Uniform(_embedding, 10, _random)

        ' Get graph data in ordered way...
        Dim weights = New List(Of Double)()
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
        Dim aB As (a!, b!) = Umap.FindABParams(_spread, _minDist)

        _optimizationState.Head = head
        _optimizationState.Tail = tail
        _optimizationState.EpochsPerSample = epochsPerSample
        _optimizationState.A = aB.a
        _optimizationState.B = aB.b
        _optimizationState.NEpochs = nEpochs
        _optimizationState.NVertices = nVertices
    End Sub

    Friend Shared Function FindABParams(spread As Double, minDist As Double) As (Single, Double)
        ' 2019-06-21 DWR: If we need to support other spread, minDist values then we might 
        ' be able to use the LM implementation in Accord.NET but I'll hard code values that 
        ' relate to the default configuration for now
        If spread <> 1 OrElse minDist <> 0.1F Then
            Throw New ArgumentException($"Currently, the {NameOf(FindABParams)} method only supports spread, minDist values of 1, 0.1 (the Levenberg-Marquardt algorithm is required to process other values")
        End If

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
            Call OptimizeLayoutStep(currentEpoch)

            If _progressReporter IsNot Nothing Then
                ' InitializeFit roughly approximately takes 80% of the processing time for large quantities of data, 
                ' leaving 20% for the Step iterations - the progress reporter calls made here are based on the 
                ' assumption that Step will be called the recommended number of times (the number-of-epochs value 
                ' returned From InitializeFit)
                Umap.ScaleProgressReporter(_progressReporter, 0.8F, 1)(CSng(currentEpoch) / numberOfEpochsToComplete)
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
        ' 在这里可以进行并行化？
        For i = 0 To _optimizationState.EpochsPerSample.Length - 1
            RunIterate(i, n)
        Next

        ' Preparation for future work for interpolating the table before optimizing
        _optimizationState.Alpha = _optimizationState.InitialAlpha * (1.0F - n / _optimizationState.NEpochs)
        _optimizationState.CurrentEpoch += 1
    End Sub

    Private Sub RunIterate(i As Integer, n As Integer)
        If Not (_optimizationState.EpochOfNextSample(i) >= n) Then
            Call Iterate(i, n)
        End If
    End Sub

    Private Sub Iterate(i As Integer, n As Integer)
        Dim embeddingSpan As Span(Of Double) = _embedding

        Dim j As Integer = _optimizationState.Head(i)
        Dim k As Integer = _optimizationState.Tail(i)

        Dim current_start As Integer = j * _optimizationState.Dim
        Dim other_start As Integer = k * _optimizationState.Dim
        Dim current = embeddingSpan.Slice(current_start, _optimizationState.Dim)
        Dim other = embeddingSpan.Slice(other_start, _optimizationState.Dim)

        Dim distSquared = Umap.RDist(current, other)
        Dim gradCoeff = 0F
        Dim gradD As Double

        If (distSquared > 0) Then
            gradCoeff = -2 * _optimizationState.A * _optimizationState.B * stdNum.Pow(distSquared, _optimizationState.B - 1)
            gradCoeff /= _optimizationState.A * stdNum.Pow(distSquared, _optimizationState.B) + 1
        End If

        Const clipValue = 4.0F

        For d As Integer = 0 To _optimizationState.Dim - 1
            gradD = Umap.Clip(gradCoeff * (current(d) - other(d)), clipValue)
            current(d) += gradD * _optimizationState.Alpha

            If (_optimizationState.MoveOther) Then
                other(d) += -gradD * _optimizationState.Alpha
            End If
        Next

        _optimizationState.EpochOfNextSample(i) += _optimizationState.EpochsPerSample(i)

        Dim nNegSamples As Integer = stdNum.Floor((n - _optimizationState.EpochOfNextNegativeSample(i)) / _optimizationState.EpochsPerNegativeSample(i))

        For p = 0 To nNegSamples - 1

            k = _random.Next(0, _optimizationState.NVertices)
            other_start = k * _optimizationState.Dim
            other = embeddingSpan.Slice(other_start, _optimizationState.Dim)
            distSquared = Umap.RDist(current, other)
            gradCoeff = 0F

            If (distSquared > 0) Then
                ' Preparation For future work For interpolating the table before optimizing
                gradCoeff = 2 * _optimizationState.Gamma * _optimizationState.B
                gradCoeff *= _optimizationState.GetDistanceFactor(distSquared)
            ElseIf (j = k) Then
                Continue For
            End If

            For d As Integer = 0 To _optimizationState.Dim - 1
                gradD = 4.0F

                If (gradCoeff > 0) Then
                    gradD = Umap.Clip(gradCoeff * (current(d) - other(d)), clipValue)
                End If

                current(d) += gradD * _optimizationState.Alpha
            Next
        Next

        _optimizationState.EpochOfNextNegativeSample(i) += nNegSamples * _optimizationState.EpochsPerNegativeSample(i)
    End Sub

    ''' <summary>
    ''' Reduced Euclidean distance
    ''' </summary>
    Private Shared Function RDist(x As Span(Of Double), y As Span(Of Double)) As Double
        Dim distSquared = 0F
        Dim d As Double

        For i = 0 To x.Length - 1
            d = x(i) - y(i)
            distSquared += d * d
        Next

        Return distSquared
    End Function

    ''' <summary>
    ''' Standard clamping of a value into a fixed range
    ''' </summary>
    Private Shared Function Clip(x As Double, clipValue As Double) As Double
        If x > clipValue Then
            Return clipValue
        ElseIf x < -clipValue Then
            Return -clipValue
        Else
            Return x
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Shared Function ScaleProgressReporter(progressReporter As IProgressReporter, start As Double, [end] As Double) As IProgressReporter
        Return Sub(progress) progressReporter(([end] - start) * progress + start)
    End Function
End Class
