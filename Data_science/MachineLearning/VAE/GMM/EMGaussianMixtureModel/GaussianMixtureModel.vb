#Region "Microsoft.VisualBasic::5a0c13c9af0ff01bb01258bbcfa3c737, Data_science\MachineLearning\VAE\GMM\EMGaussianMixtureModel\GaussianMixtureModel.vb"

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

    '   Total Lines: 243
    '    Code Lines: 164
    ' Comment Lines: 30
    '   Blank Lines: 49
    '     File Size: 10.18 KB


    '     Class GaussianMixtureModel
    ' 
    '         Properties: Components, DataSet, Probs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: covMStep, emStep, eStep, eStepDatum, FitGMM
    '                   logLikelihoodGMM, meansMStep, mStep, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
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
        Dim m_data As ClusterEntity()
        Dim m_safe As Boolean = False
        Dim m_abs As Boolean = False

        Const min As Double = 1.0 / Long.MaxValue

        ReadOnly m_width As Integer

        Public ReadOnly Property DataSet As ClusterEntity()
            Get
                Return m_data
            End Get
        End Property

        Public Overridable ReadOnly Property Components As GaussianMixtureComponent()
            Get
                Return m_components
            End Get
        End Property

        ''' <summary>
        ''' The probility of each datum in the input dataset clustering to the specific components
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Probs As Double()()
            Get
                Return eStep(m_components) _
                    .Select(Function(c) c.ToArray) _
                    .ToArray
            End Get
        End Property

        Public Sub New(data As IEnumerable(Of ClusterEntity), Optional strict As Boolean = True, Optional abs As Boolean = False)
            Me.m_data = data.ToArray
            Me.m_safe = Not strict
            Me.m_abs = abs
            Me.m_width = m_data(0).Length
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

            Dim denominator As Double = wkList.Sum

            If denominator = 0.0 Then
                denominator = min
            End If

            For i As Integer = 0 To wkList.Count - 1
                wkList(i) = wkList(i) / denominator

                If wkList(i) < 0 OrElse wkList(i) > 1 Then
                    Throw New Exception("Probability must be between 0 and 1, returned" & wkList(i).ToString())
                End If
            Next

            Return wkList
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="components"></param>
        ''' <returns>
        ''' the populate out sequence order is keeps the same as the source <see cref="m_data"/>
        ''' </returns>
        Private Iterator Function eStep(components As GaussianMixtureComponent()) As IEnumerable(Of IList(Of Double))
            For Each datum As ClusterEntity In m_data
                Yield eStepDatum(datum.entityVector, components)
            Next
        End Function

        Private Iterator Function meansMStep(NkList As IList(Of Double), wkList As IList(Of IList(Of Double))) As IEnumerable(Of RealMatrix)
            Dim K = wkList(0).Count
            Dim N = m_data.Length
            Dim d = m_width
            Dim insideSum As Double()

            For j As Integer = 0 To K - 1
                Dim initMatrixDby1 = New Double(d - 1) {}
                Dim componentMean As RealMatrix = New RealMatrix(initMatrixDby1)

                For i As Integer = 0 To N - 1
                    insideSum = New Vector(m_data(i).entityVector) * wkList(i)(j)
                    componentMean = CType((componentMean + (New RealMatrix(insideSum))), RealMatrix)
                Next

                componentMean = CType(componentMean * (1 / NkList(j)), RealMatrix)

                Yield componentMean
            Next
        End Function

        Private Iterator Function covMStep(NkList As IList(Of Double),
                                           mukList As RealMatrix(),
                                           wkList As IList(Of IList(Of Double))) As IEnumerable(Of RealMatrix)
            Dim K = NkList.Count
            Dim N = m_data.Length
            Dim d = m_width

            For j As Integer = 0 To K - 1
                Dim insideSumVal As RealMatrix = New RealMatrix(RectangularArray.Matrix(Of Double)(d, d))

                For i As Integer = 0 To N - 1
                    Dim xiMinusMu As RealMatrix = (New RealMatrix(m_data(i).entityVector)) - mukList(j)
                    Dim xiMinusMuT As RealMatrix = CType(xiMinusMu.Transpose(), RealMatrix)

                    insideSumVal = CType(insideSumVal + DirectCast(xiMinusMu.DotProduct(xiMinusMuT), RealMatrix) * wkList(i)(j), RealMatrix)
                Next

                If NkList(j) = 0 Then
                    Throw New Exception("Error in covariance maximization, Nk is equal to zero. " & "Found in component " & j.ToString())
                End If

                insideSumVal = CType(insideSumVal * (1 / NkList(j)), RealMatrix)

                Yield insideSumVal
            Next
        End Function

        Private Iterator Function mStep(wkList As IList(Of IList(Of Double))) As IEnumerable(Of GaussianMixtureComponent)
            Dim K = wkList(0).Count
            Dim N = m_data.Length
            Dim NkList = columnSum(wkList)
            ' Calculate component probabilities (Alphas)
            Dim alphakList = divisionScalar(NkList, N)
            ' means calculation
            Dim mukList = meansMStep(NkList, wkList).ToArray
            ' covariance calculation
            Dim sigmakList = covMStep(NkList, mukList, wkList).ToArray

            For i As Integer = 0 To K - 1
                Yield New GaussianMixtureComponent(i, mukList(i), sigmakList(i), alphakList(i))
            Next
        End Function

        Private Function logLikelihoodGMM(components As GaussianMixtureComponent()) As Double
            Dim logLikelihoodSum = 0.0

            For Each datum As ClusterEntity In m_data
                Dim componentPDFSum As Double = 0.0

                For Each comp As GaussianMixtureComponent In components
                    componentPDFSum += comp.componentPDFandProb(datum.entityVector)
                Next

                If componentPDFSum = 0.0 Then
                    componentPDFSum = min
                End If

                logLikelihoodSum += std.Log(componentPDFSum)
            Next

            Return logLikelihoodSum
        End Function

        Private Function emStep(estimatedCompCenters As IList(Of Double()), maxNumberIterations As Integer, deltaLogLikelihoodThreshold As Double) As GaussianMixtureComponent()
            ' initialize wkList, weights are the L1 norm of the distance of a point xi to each estimated component center
            Dim EStepVals As IList(Of IList(Of Double)) = New List(Of IList(Of Double))()

            For Each datum As ClusterEntity In m_data
                EStepVals.Add(distToCenterL1(datum.entityVector, estimatedCompCenters))
            Next

            Dim MStepVals As GaussianMixtureComponent() = mStep(EStepVals).ToArray
            Dim prevLogLikelihood = logLikelihoodGMM(MStepVals)
            Dim currentLogLikelihood, deltaLogLikelihood As Double

            For k As Integer = 0 To maxNumberIterations - 1 - 1
                EStepVals = eStep(MStepVals).AsList
                MStepVals = mStep(EStepVals).ToArray
                currentLogLikelihood = logLikelihoodGMM(MStepVals)

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
            m_components = emStep(estimatedCompCenters, maxIterations, convergenceCriteria)
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"{m_data.Length} dataset(width: {m_width}) clustering as {m_components.Length} components({m_components.Select(Function(c) c.Weight).ToArray.GetJson})."
        End Function
    End Class
End Namespace
