Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports std = System.Math

Namespace LASSO

    ''' <summary>
    ''' This implemenation is based on: Friedman, J., Hastie, T. and Tibshirani, R.
    ''' (2008) Regularization Paths for Generalized Linear Models via Coordinate
    ''' Descent. http://www-stat.stanford.edu/~hastie/Papers/glmnet.pdf
    ''' 
    ''' @author Yasser Ganjisaffar (http://www.ics.uci.edu/~yganjisa/)
    ''' </summary>
    Public Class LassoFitGenerator
        ' This module shouldn't consume more than 8GB of memory
        Private Shared ReadOnly MAX_AVAILABLE_MEMORY As Long = 8L * 1024 * 1024 * 1024

        ' In order to speed up the compression, we limit the number of
        ' observations,
        ' but this limit is dependent on the number of features that we should
        ' learn
        ' their weights. In other words, for learning weights of more features, we
        ' need more observations.
        Private Const MAX_OBSERVATIONS_TO_FEATURES_RATIO As Integer = 100

        Private Const EPSILON As Double = 0.000001

        ' The default number of lambda values to use
        Private Const DEFAULT_NUMBER_OF_LAMBDAS As Integer = 100

        ' Convergence threshold for coordinate descent
        ' Each inner coordination loop continues until the relative change
        ' in any coefficient is less than this threshold
        Private Const CONVERGENCE_THRESHOLD As Double = 0.0001

        Private Const SMALL As Double = 0.00001
        Private Const MIN_NUMBER_OF_LAMBDAS As Integer = 5
        Private Const MAX_RSQUARED As Double = 0.99999

        Private targetsField As Double()
        Private observations As Double()()
        Private numFeatures As Integer
        Private numObservations As Integer

        Dim featureNames As String()

        Public Shared Property tqdm_verbose As Boolean = True

        Public Overridable Function getMaxAllowedObservations(maxNumFeatures As Integer) As Integer
            Dim maxObservations As Integer = MAX_AVAILABLE_MEMORY / maxNumFeatures / (HeapSizeOf.float * 8 / 8)
            If maxObservations > MAX_OBSERVATIONS_TO_FEATURES_RATIO * maxNumFeatures Then
                'maxObservations = MAX_OBSERVATIONS_TO_FEATURES_RATIO * maxNumFeatures;
            End If
            Return maxObservations
        End Function

        Public Sub init(featureNames As String(), numObservations As Integer)
            Me.featureNames = featureNames
            ' make initialize of the matrix data
            Call init(featureNames.Length, numObservations)
        End Sub

        Public Overridable Sub init(maxNumFeatures As Integer, numObservations As Integer)
            numFeatures = maxNumFeatures
            If numObservations > getMaxAllowedObservations(maxNumFeatures) Then
                Throw New Exception("Number of observations (" & numObservations.ToString() & ") exceeds the maximum allowed number: " & getMaxAllowedObservations(maxNumFeatures).ToString())
            End If
            Me.numObservations = numObservations
            observations = New Double(Me.numObservations - 1)() {}
            For t = 0 To maxNumFeatures - 1
                observations(t) = New Double(Me.numObservations - 1) {}
            Next
            targetsField = New Double(Me.numObservations - 1) {}
        End Sub

        Public Overridable WriteOnly Property NumberOfFeatures As Integer
            Set(value As Integer)
                numFeatures = value
            End Set
        End Property

        Public Overridable Sub setFeatureValues(idx As Integer, values As Double())
            For i = 0 To values.Length - 1
                observations(idx)(i) = values(i)
            Next
        End Sub

        Public Overridable Function getFeatureValues(idx As Integer) As Double()
            Return observations(idx)
        End Function

        Public Overridable Sub setObservationValues(idx As Integer, values As Double())
            For f = 0 To numFeatures - 1
                observations(f)(idx) = values(f)
            Next
        End Sub

        Private Function getLassoFit(maxAllowedFeaturesPerModel As Integer) As LassoFit
            If maxAllowedFeaturesPerModel < 0 Then
                maxAllowedFeaturesPerModel = numFeatures
            End If
            Dim numberOfLambdas = DEFAULT_NUMBER_OF_LAMBDAS
            Dim maxAllowedFeaturesAlongPath As Integer = std.Min(maxAllowedFeaturesPerModel * 1.2, numFeatures)

            ' lambdaMin = flmin * lambdaMax
            Dim flmin = If(numObservations < numFeatures, 0.05, 0.0001)

            ' ******************************
            ' Standardize features and target: Center the target and features
            ' (mean 0) and normalize their vectors to have the same standard
            ' deviation
            Dim featureMeans = New Double(numFeatures - 1) {}
            Dim featureStds = New Double(numFeatures - 1) {}
            Dim feature2residualCorrelations = New Double(numFeatures - 1) {}

            Dim factor As Double = 1.0 / std.Sqrt(numObservations)
            For j = 0 To numFeatures - 1
                Dim mean As Double = observations(j).Average()
                featureMeans(j) = mean
                For i = 0 To numObservations - 1
                    observations(j)(i) = CSng(factor * (observations(j)(i) - mean))
                Next
                featureStds(j) = std.Sqrt(MathUtil.getDotProduct(observations(j), observations(j), observations(j).Length))

                MathUtil.divideInPlace(observations(j), featureStds(j))
            Next

            Dim targetMean As Double = CSng(targetsField.Average())
            For i = 0 To numObservations - 1
                targetsField(i) = factor * (targetsField(i) - targetMean)
            Next
            Dim targetStd As Double = std.Sqrt(MathUtil.getDotProduct(targetsField, targetsField, targetsField.Length))
            MathUtil.divideInPlace(targetsField, targetStd)

            For j = 0 To numFeatures - 1
                feature2residualCorrelations(j) = MathUtil.getDotProduct(targetsField, observations(j), targetsField.Length)
            Next

            Dim feature2featureCorrelations = MathUtil.allocateDoubleMatrix(numFeatures, maxAllowedFeaturesAlongPath)
            Dim activeWeights = New Double(numFeatures - 1) {}
            Dim correlationCacheIndices = New Integer(numFeatures - 1) {}
            Dim denseActiveSet = New Double(numFeatures - 1) {}

            Dim fit As New LassoFit(numberOfLambdas, maxAllowedFeaturesAlongPath, numFeatures) With {
                .featureNames = featureNames,
                .numberOfLambdas = 0
            }

            Dim alf = std.Pow(std.Max(EPSILON, flmin), 1.0 / (numberOfLambdas - 1))
            Dim rsquared = 0.0
            fit.numberOfPasses = 0
            Dim numberOfInputs = 0
            Dim minimumNumberOfLambdas = std.Min(MIN_NUMBER_OF_LAMBDAS, numberOfLambdas)

            Dim curLambda As Double = 0
            Dim maxDelta As Double
            Dim bar As ProgressBar = Nothing

            For Each iteration As Integer In TqdmWrapper.Range(1, numberOfLambdas, bar:=bar, wrap_console:=tqdm_verbose)
                Call bar.SetLabel("Starting iteration " & iteration.ToString() & " of Compression.")

                ' ********
                ' Compute lambda for this round
                If iteration = 1 Then
                    curLambda = Double.MaxValue ' first lambda is infinity
                ElseIf iteration = 2 Then
                    curLambda = 0.0
                    For j = 0 To numFeatures - 1
                        curLambda = std.Max(curLambda, std.Abs(feature2residualCorrelations(j)))
                    Next
                    curLambda = alf * curLambda
                Else
                    curLambda = curLambda * alf
                End If

                Dim prevRsq = rsquared
                Dim v As Double
                While True
                    fit.numberOfPasses += 1
                    maxDelta = 0.0
                    For k = 0 To numFeatures - 1
                        Dim prevWeight = activeWeights(k)
                        Dim u = feature2residualCorrelations(k) + prevWeight
                        v = If(u >= 0, u, -u) - curLambda
                        ' Computes sign(u)(|u| - curLambda)+
                        activeWeights(k) = If(v > 0, If(u >= 0, v, -v), 0.0)

                        ' Is the weight of this variable changed?
                        ' If not, we go to the next one
                        If activeWeights(k) = prevWeight Then
                            Continue For
                        End If

                        ' If we have not computed the correlations of this
                        ' variable with other variables, we do this now and
                        ' cache the result
                        If correlationCacheIndices(k) = 0 Then
                            numberOfInputs += 1
                            If numberOfInputs > maxAllowedFeaturesAlongPath Then
                                ' we have reached the maximum
                                Exit For
                            End If
                            For j = 0 To numFeatures - 1
                                ' if we have already computed correlations for
                                ' the jth variable, we will reuse it here.
                                If correlationCacheIndices(j) <> 0 Then
                                    feature2featureCorrelations(j)(numberOfInputs - 1) = feature2featureCorrelations(k)(correlationCacheIndices(j) - 1)
                                Else
                                    ' Correlation of variable with itself if one
                                    If j = k Then
                                        feature2featureCorrelations(j)(numberOfInputs - 1) = 1.0
                                    Else
                                        feature2featureCorrelations(j)(numberOfInputs - 1) = MathUtil.getDotProduct(observations(j), observations(k), observations(j).Length)
                                    End If
                                End If
                            Next
                            correlationCacheIndices(k) = numberOfInputs
                            fit.indices(numberOfInputs - 1) = k
                        End If

                        ' How much is the weight changed?
                        Dim delta = activeWeights(k) - prevWeight
                        rsquared += delta * (2.0 * feature2residualCorrelations(k) - delta)
                        maxDelta = std.Max(If(delta >= 0, delta, -delta), maxDelta)

                        For j = 0 To numFeatures - 1
                            feature2residualCorrelations(j) -= feature2featureCorrelations(j)(correlationCacheIndices(k) - 1) * delta
                        Next
                    Next

                    If maxDelta < CONVERGENCE_THRESHOLD OrElse numberOfInputs > maxAllowedFeaturesAlongPath Then
                        Exit While
                    End If

                    For ii = 0 To numberOfInputs - 1
                        denseActiveSet(ii) = activeWeights(fit.indices(ii))
                    Next

                    Do
                        fit.numberOfPasses += 1
                        maxDelta = 0.0
                        For l = 0 To numberOfInputs - 1
                            Dim k = fit.indices(l)
                            Dim prevWeight = activeWeights(k)
                            Dim u = feature2residualCorrelations(k) + prevWeight
                            v = If(u >= 0, u, -u) - curLambda
                            activeWeights(k) = If(v > 0, If(u >= 0, v, -v), 0.0)
                            If activeWeights(k) = prevWeight Then
                                Continue For
                            End If
                            Dim delta = activeWeights(k) - prevWeight
                            rsquared += delta * (2.0 * feature2residualCorrelations(k) - delta)
                            maxDelta = std.Max(If(delta >= 0, delta, -delta), maxDelta)
                            For j = 0 To numberOfInputs - 1
                                feature2residualCorrelations(fit.indices(j)) -= feature2featureCorrelations(fit.indices(j))(correlationCacheIndices(k) - 1) * delta
                            Next
                        Next
                    Loop While maxDelta >= CONVERGENCE_THRESHOLD

                    For ii = 0 To numberOfInputs - 1
                        denseActiveSet(ii) = activeWeights(fit.indices(ii)) - denseActiveSet(ii)
                    Next
                    For j = 0 To numFeatures - 1
                        If correlationCacheIndices(j) = 0 Then
                            feature2residualCorrelations(j) -= MathUtil.getDotProduct(denseActiveSet, feature2featureCorrelations(j), numberOfInputs)
                        End If
                    Next
                End While

                If numberOfInputs > maxAllowedFeaturesAlongPath Then
                    Exit For
                End If
                If numberOfInputs > 0 Then
                    For ii = 0 To numberOfInputs - 1
                        fit.compressedWeights(iteration - 1)(ii) = activeWeights(fit.indices(ii))
                    Next
                End If
                fit.numberOfWeights(iteration - 1) = numberOfInputs
                fit.rsquared(iteration - 1) = rsquared
                fit.lambdas(iteration - 1) = curLambda
                fit.numberOfLambdas = iteration

                If iteration < minimumNumberOfLambdas Then
                    Continue For
                End If

                Dim [me] = 0
                For j = 0 To numberOfInputs - 1
                    If fit.compressedWeights(iteration - 1)(j) <> 0.0 Then
                        [me] += 1
                    End If
                Next
                If [me] > maxAllowedFeaturesPerModel OrElse rsquared - prevRsq < SMALL * rsquared OrElse rsquared > MAX_RSQUARED Then
                    Exit For
                End If
            Next

            For k = 0 To fit.numberOfLambdas - 1
                fit.lambdas(k) = targetStd * fit.lambdas(k)
                Dim nk = fit.numberOfWeights(k)
                For l = 0 To nk - 1
                    fit.compressedWeights(k)(l) = targetStd * fit.compressedWeights(k)(l) / featureStds(fit.indices(l))
                    If fit.compressedWeights(k)(l) <> 0 Then
                        fit.nonZeroWeights(k) += 1
                    End If
                Next
                Dim product As Double = 0
                For i = 0 To nk - 1
                    product += fit.compressedWeights(k)(i) * featureMeans(fit.indices(i))
                Next
                fit.intercepts(k) = targetMean - product
            Next

            ' First lambda was infinity; fixing it
            fit.lambdas(0) = std.Exp(2 * std.Log(fit.lambdas(1)) - std.Log(fit.lambdas(2)))

            Return fit
        End Function

        Public Overridable WriteOnly Property Targets As Double()
            Set(value As Double())
                For i = 0 To numObservations - 1
                    targetsField(i) = CSng(value(i))
                Next
            End Set
        End Property

        Public Overridable Sub setTarget(idx As Integer, target As Double)
            targetsField(idx) = CSng(target)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="maxAllowedFeaturesPerModel">
        ''' Generate the Lasso fit. The -1 arguments means that
        ''' there would be no limit on the maximum number of 
        ''' features per model
        ''' </param>
        ''' <returns></returns>
        Public Overridable Function fit(maxAllowedFeaturesPerModel As Integer) As LassoFit
            Dim lFit = getLassoFit(maxAllowedFeaturesPerModel)
            Return lFit
        End Function
    End Class

End Namespace
