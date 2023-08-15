Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports RealMatrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    ''' <summary>
    ''' Use Expectation Maximization (EM) algorithm to fit maximum likelihood parameters for a Gaussian Mixture Model.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/MatthewSwahn/EMGaussianMixtureModel
    ''' </remarks>
    Public Class GaussianMixtureModel

        Public componentsField As IList(Of GaussianMixtureComponent)
        Public ReadOnly data As IList(Of Double())


        Public Sub New(data As IList(Of Double()))
            Me.data = data
        End Sub

        Public Overridable ReadOnly Property Components As IList(Of GaussianMixtureComponent)
            Get
                Return componentsField
            End Get
        End Property


        Public Overridable ReadOnly Property ComponentValues As IList(Of IList(Of Object))
            Get
                Dim values As IList(Of IList(Of Object)) = New List(Of IList(Of Object))()
                For i = 0 To componentsField.Count - 1
                    Dim inner As IList(Of Object) = New List(Of Object)()
                    inner.Add(componentsField(i).Weight)
                    inner.Add(componentsField(i).Mean)
                    inner.Add(componentsField(i).CovMatrix)
                    values.Add(inner)
                Next
                Return values
            End Get
        End Property

        ' Computes the E step for a single datum.
        ' 		    wk array = pdfandweight(x, for each k)/ sum(pdfandweight(x, for each k))
        ' 		    Assumes K existing weight parameters, K means corresponding to each component,
        ' 		    and K variances corresponding to each component.
        ' 		     

        Private Function eStepDatum(datum As Double(), components As IList(Of GaussianMixtureComponent)) As IList(Of Double)

            Dim wkList As IList(Of Double) = New List(Of Double)()
            For Each GMMk In components
                wkList.Add(GMMk.componentPDFandProb(datum))
            Next

            Dim denominator As Double = 0
            For Each component In components
                denominator += component.componentPDFandProb(datum)
            Next

            For i = 0 To wkList.Count - 1
                wkList(i) = wkList(i) / denominator
                If wkList(i) < 0 OrElse wkList(i) > 1 Then
                    Throw New Exception("Probability must be between 0 and 1, returned" & wkList(i).ToString())
                End If
            Next
            Return wkList
        End Function

        Private Function eStep(data As IList(Of Double()), components As IList(Of GaussianMixtureComponent)) As IList(Of IList(Of Double))
            Dim results As IList(Of IList(Of Double)) = New List(Of IList(Of Double))()
            For Each datum In data
                results.Add(eStepDatum(datum, components))
            Next
            Return results
        End Function

        Private Function meansMStep(data As IList(Of Double()), NkList As IList(Of Double), wkList As IList(Of IList(Of Double))) As IList(Of RealMatrix)
            Dim K = wkList(0).Count
            Dim N = data.Count
            Dim d = data(0).Length

            Dim mukList As IList(Of RealMatrix) = New List(Of RealMatrix)()
            For j = 0 To K - 1
                Dim initMatrixDby1 = New Double(d - 1) {}
                ' Arrays.fill(initMatrixDby1, 0.0);
                Dim componentMean As RealMatrix = New RealMatrix(initMatrixDby1)
                For i = 0 To N - 1
                    Dim insideSum = multiplicationScalar(data(i), wkList(i)(j))
                    componentMean = CType((componentMean + (New RealMatrix(insideSum))), RealMatrix)
                Next
                componentMean = CType(componentMean * (1 / NkList(j)), RealMatrix)
                mukList.Add(componentMean)
            Next
            Return mukList
        End Function

        Private Function covMStep(data As IList(Of Double()), NkList As IList(Of Double), mukList As IList(Of RealMatrix), wkList As IList(Of IList(Of Double))) As IList(Of RealMatrix)
            Dim K = NkList.Count
            Dim N = data.Count
            Dim d = data(0).Length

            Dim sigmakList As IList(Of RealMatrix) = New List(Of RealMatrix)()
            For j = 0 To K - 1

                Dim insideSumVal As RealMatrix = New RealMatrix(RectangularArray.Matrix(Of Double)(d, d))
                For i = 0 To N - 1
                    Dim xiMinusMu As RealMatrix = (New RealMatrix(data(i))) - mukList(j)
                    Dim xiMinusMuT As RealMatrix = CType(xiMinusMu.Transpose(), RealMatrix)
                    insideSumVal = CType(insideSumVal + xiMinusMu.DotProduct(xiMinusMuT) * wkList(i)(j), RealMatrix)
                Next
                If NkList(j) = 0 Then
                    Throw New Exception("Error in covariance maximization, Nk is equal to zero. " & "Found in component " & j.ToString())
                End If
                insideSumVal = CType(insideSumVal * (1 / NkList(j)), RealMatrix)
                sigmakList.Add(insideSumVal)
            Next
            Return sigmakList
        End Function

        Private Function mStep(data As IList(Of Double()), wkList As IList(Of IList(Of Double))) As IList(Of GaussianMixtureComponent)
            Dim K = wkList(0).Count
            Dim N = data.Count
            Dim d = data(0).Length

            Dim NkList = columnSum(wkList)

            ' Calculate component probabilities (Alphas)
            Dim alphakList = divisionScalar(NkList, N)

            ' means calculation
            Dim mukList = meansMStep(data, NkList, wkList)

            ' covariance calculation
            Dim sigmakList = covMStep(data, NkList, mukList, wkList)

            Dim results As IList(Of GaussianMixtureComponent) = New List(Of GaussianMixtureComponent)()
            For i = 0 To K - 1
                results.Add(New GaussianMixtureComponent(i, mukList(i), sigmakList(i), alphakList(i)))
            Next
            Return results
        End Function

        Private Function logLikelihoodGMM(data As IList(Of Double()), components As IList(Of GaussianMixtureComponent)) As Double
            Dim logLikelihoodSum = 0.0
            For Each datum In data
                Dim componentPDFSum = 0.0
                For Each comp In components
                    componentPDFSum += comp.componentPDFandProb(datum)
                Next
                logLikelihoodSum += std.Log(componentPDFSum)
            Next
            Return logLikelihoodSum
        End Function

        Private Function emStep(data As IList(Of Double()), estimatedCompCenters As IList(Of Double()), maxNumberIterations As Integer, deltaLogLikelihoodThreshold As Double) As IList(Of GaussianMixtureComponent)

            ' initialize wkList, weights are the L1 norm of the distance of a point xi to each estimated component center
            Dim EStepVals As IList(Of IList(Of Double)) = New List(Of IList(Of Double))()
            For Each datum In data
                EStepVals.Add(distToCenterL1(datum, estimatedCompCenters))
            Next

            Dim MStepVals = mStep(data, EStepVals)
            Dim prevLogLikelihood = logLikelihoodGMM(data, MStepVals)

            For k = 0 To maxNumberIterations - 1 - 1
                EStepVals = eStep(data, MStepVals)
                MStepVals = mStep(data, EStepVals)
                Dim currentLogLikelihood = logLikelihoodGMM(data, MStepVals)
                Dim deltaLogLikelihood = std.Abs(currentLogLikelihood - prevLogLikelihood)

                Console.WriteLine($" * {k} - {currentLogLikelihood} [{deltaLogLikelihood}]")

                If deltaLogLikelihood < deltaLogLikelihoodThreshold Then
                    Console.WriteLine("after " & (k + 1).ToString() & " iterations, EM converged.")
                    Return MStepVals
                End If
                prevLogLikelihood = currentLogLikelihood
            Next

            Throw New Exception("After " & maxNumberIterations.ToString() & " iterations there was no convergence.")
        End Function

        Public Overridable Sub fitGMM(estimatedCompCenters As IList(Of Double()), maxIterations As Integer, convergenceCriteria As Double)
            componentsField = emStep(data, estimatedCompCenters, maxIterations, convergenceCriteria)
        End Sub
    End Class

End Namespace
