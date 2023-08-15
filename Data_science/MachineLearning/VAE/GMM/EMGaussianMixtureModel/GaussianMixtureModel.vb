Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra
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

        Dim m_components As GaussianMixtureComponent()
        Dim m_data As Double()()
        Dim m_safe As Boolean = False
        Dim m_abs As Boolean = False

        Public Overridable ReadOnly Property Components As GaussianMixtureComponent()
            Get
                Return m_components
            End Get
        End Property

        Public Sub New(data As IList(Of Double()), Optional safe As Boolean = False, Optional abs As Boolean = False)
            Me.m_data = data.ToArray
            Me.m_safe = safe
            Me.m_abs = abs
        End Sub

        ''' <summary>
        ''' Computes the E step for a single datum.
        ''' wk array = pdfandweight(x, for each k)/ sum(pdfandweight(x, for each k))
        ''' Assumes K existing weight parameters, K means corresponding to each component,
        ''' and K variances corresponding to each component.
        ''' </summary>
        ''' <param name="datum"></param>
        ''' <param name="components"></param>
        ''' <returns></returns>
        Private Function eStepDatum(datum As Double(), components As GaussianMixtureComponent()) As IList(Of Double)
            Dim wkList As New List(Of Double)()

            For Each GMMk As GaussianMixtureComponent In components
                wkList.Add(GMMk.componentPDFandProb(datum))
            Next

            Dim denominator As Double = 0

            For Each component As GaussianMixtureComponent In components
                denominator += component.componentPDFandProb(datum)
            Next

            For i As Integer = 0 To wkList.Count - 1
                wkList(i) = wkList(i) / denominator

                If wkList(i) < 0 OrElse wkList(i) > 1 Then
                    Throw New Exception("Probability must be between 0 and 1, returned" & wkList(i).ToString())
                End If
            Next

            Return wkList
        End Function

        Private Function eStep(data As IList(Of Double()), components As GaussianMixtureComponent()) As IList(Of IList(Of Double))
            Dim results As New List(Of IList(Of Double))()

            For Each datum As Double() In data
                results.Add(eStepDatum(datum, components))
            Next

            Return results
        End Function

        Private Iterator Function meansMStep(data As IList(Of Double()),
                                             NkList As IList(Of Double),
                                             wkList As IList(Of IList(Of Double))) As IEnumerable(Of RealMatrix)
            Dim K = wkList(0).Count
            Dim N = data.Count
            Dim d = data(0).Length

            For j As Integer = 0 To K - 1
                Dim initMatrixDby1 = New Double(d - 1) {}
                Dim componentMean As RealMatrix = New RealMatrix(initMatrixDby1)

                For i As Integer = 0 To N - 1
                    Dim insideSum As Double() = New Vector(data(i)) * wkList(i)(j)
                    componentMean = CType((componentMean + (New RealMatrix(insideSum))), RealMatrix)
                Next

                componentMean = CType(componentMean * (1 / NkList(j)), RealMatrix)

                Yield componentMean
            Next
        End Function

        Private Iterator Function covMStep(data As IList(Of Double()),
                                           NkList As IList(Of Double),
                                           mukList As RealMatrix(),
                                           wkList As IList(Of IList(Of Double))) As IEnumerable(Of RealMatrix)
            Dim K = NkList.Count
            Dim N = data.Count
            Dim d = data(0).Length

            For j As Integer = 0 To K - 1
                Dim insideSumVal As RealMatrix = New RealMatrix(RectangularArray.Matrix(Of Double)(d, d))

                For i As Integer = 0 To N - 1
                    Dim xiMinusMu As RealMatrix = (New RealMatrix(data(i))) - mukList(j)
                    Dim xiMinusMuT As RealMatrix = CType(xiMinusMu.Transpose(), RealMatrix)

                    insideSumVal = CType(insideSumVal + xiMinusMu.DotProduct(xiMinusMuT) * wkList(i)(j), RealMatrix)
                Next

                If NkList(j) = 0 Then
                    Throw New Exception("Error in covariance maximization, Nk is equal to zero. " & "Found in component " & j.ToString())
                End If

                insideSumVal = CType(insideSumVal * (1 / NkList(j)), RealMatrix)

                Yield insideSumVal
            Next
        End Function

        Private Iterator Function mStep(data As IList(Of Double()), wkList As IList(Of IList(Of Double))) As IEnumerable(Of GaussianMixtureComponent)
            Dim K = wkList(0).Count
            Dim N = data.Count
            Dim d = data(0).Length
            Dim NkList = columnSum(wkList)
            ' Calculate component probabilities (Alphas)
            Dim alphakList = divisionScalar(NkList, N)
            ' means calculation
            Dim mukList = meansMStep(data, NkList, wkList).ToArray
            ' covariance calculation
            Dim sigmakList = covMStep(data, NkList, mukList, wkList).ToArray

            For i As Integer = 0 To K - 1
                Yield New GaussianMixtureComponent(i, mukList(i), sigmakList(i), alphakList(i))
            Next
        End Function

        Private Function logLikelihoodGMM(data As IList(Of Double()), components As IList(Of GaussianMixtureComponent)) As Double
            Dim logLikelihoodSum = 0.0

            For Each datum As Double() In data
                Dim componentPDFSum = 0.0

                For Each comp In components
                    componentPDFSum += comp.componentPDFandProb(datum)
                Next

                logLikelihoodSum += std.Log(componentPDFSum)
            Next

            Return logLikelihoodSum
        End Function

        Private Function emStep(data As IList(Of Double()),
                                estimatedCompCenters As IList(Of Double()),
                                maxNumberIterations As Integer,
                                deltaLogLikelihoodThreshold As Double) As GaussianMixtureComponent()

            ' initialize wkList, weights are the L1 norm of the distance of a point xi to each estimated component center
            Dim EStepVals As IList(Of IList(Of Double)) = New List(Of IList(Of Double))()

            For Each datum As Double() In data
                EStepVals.Add(distToCenterL1(datum, estimatedCompCenters))
            Next

            Dim MStepVals As GaussianMixtureComponent() = mStep(data, EStepVals).ToArray
            Dim prevLogLikelihood = logLikelihoodGMM(data, MStepVals)
            Dim currentLogLikelihood, deltaLogLikelihood As Double

            For k As Integer = 0 To maxNumberIterations - 1 - 1
                EStepVals = eStep(data, MStepVals)
                MStepVals = mStep(data, EStepVals).ToArray
                currentLogLikelihood = logLikelihoodGMM(data, MStepVals)

                If m_abs Then
                    deltaLogLikelihood = std.Abs(currentLogLikelihood - prevLogLikelihood)
                Else
                    deltaLogLikelihood = currentLogLikelihood - prevLogLikelihood
                End If

                VBDebugger.EchoLine($" * {k} - {currentLogLikelihood} [{deltaLogLikelihood}]")

                If deltaLogLikelihood < deltaLogLikelihoodThreshold Then
                    VBDebugger.EchoLine("after " & (k + 1).ToString() & " iterations, EM converged.")
                    Return MStepVals
                Else
                    prevLogLikelihood = currentLogLikelihood
                End If
            Next

            Dim msg As String = "After " & maxNumberIterations.ToString() & " iterations there was no convergence."

            Call VBDebugger.EchoLine($"[WARN] *** {msg} ***")

            If m_safe Then
                Return MStepVals
            Else
                Throw New Exception(msg)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FitGMM(estimatedCompCenters As IList(Of Double()), maxIterations As Integer, convergenceCriteria As Double) As GaussianMixtureModel
            m_components = emStep(m_data, estimatedCompCenters, maxIterations, convergenceCriteria)
            Return Me
        End Function
    End Class
End Namespace
