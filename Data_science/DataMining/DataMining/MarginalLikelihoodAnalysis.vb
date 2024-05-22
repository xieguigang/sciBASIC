#Region "Microsoft.VisualBasic::d10d937373783f10747424848847da93, Data_science\DataMining\DataMining\MarginalLikelihoodAnalysis.vb"

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

    '   Total Lines: 281
    '    Code Lines: 160 (56.94%)
    ' Comment Lines: 68 (24.20%)
    '    - Xml Docs: 58.82%
    ' 
    '   Blank Lines: 53 (18.86%)
    '     File Size: 10.46 KB


    ' Enum AnalysisTypes
    ' 
    '     AICM, Arithmetic, Harmonic, Smoothed
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class MarginalLikelihoodAnalysis
    ' 
    '     Properties: BootstrappedSE, Burnin, LogMarginalLikelihood
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: calculateLogMarginalLikelihood, logMarginalLikelihoodAICM, logMarginalLikelihoodArithmetic, logMarginalLikelihoodHarmonic, (+2 Overloads) logMarginalLikelihoodSmoothed
    ' 
    '     Sub: calculate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java
Imports std = System.Math

'
' * MarginalLikelihoodAnalysis.java
' *
' * Copyright (C) 2002-2006 Alexei Drummond and Andrew Rambaut
' *
' * This file is part of BEAST.
' * See the NOTICE file distributed with this work for additional
' * information regarding copyright ownership and licensing.
' *
' * BEAST is free software; you can redistribute it and/or modify
' * it under the terms of the GNU Lesser General Public License as
' * published by the Free Software Foundation; either version 2
' * of the License, or (at your option) any later version.
' *
' *  BEAST is distributed in the hope that it will be useful,
' *  but WITHOUT ANY WARRANTY; without even the implied warranty of
' *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' *  GNU Lesser General Public License for more details.
' *
' * You should have received a copy of the GNU Lesser General Public
' * License along with BEAST; if not, write to the
' * Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
' * Boston, MA  02110-1301  USA
' 

Public Enum AnalysisTypes
    AICM
    Smoothed
    Arithmetic
    Harmonic
End Enum

''' <summary>
''' @author Marc Suchard
''' @author Alexei Drummond
''' 
''' Source translated from ``model_P.c`` (a component of BAli-Phy by Benjamin Redelings and Marc Suchard
''' </summary>
Public Class MarginalLikelihoodAnalysis

    ReadOnly sample As List(Of Double)

    ''' <summary>
    ''' "harmonic" for harmonic mean, 
    ''' "smoothed" for smoothed harmonic mean, 
    ''' "aicm" for AICM, 
    ''' "arithmetic" for arithmetic mean
    ''' </summary>
    ReadOnly analysisType As AnalysisTypes
    ReadOnly bootstrapLength As Integer

    Dim marginalLikelihoodCalculated As Boolean = False
    Dim _logMarginalLikelihood As Double
    Dim _bootstrappedSE As Double

    Public Property Burnin As Integer

    Public ReadOnly Property LogMarginalLikelihood As Double
        Get
            If Not marginalLikelihoodCalculated Then
                calculate()
            End If

            Return _logMarginalLikelihood
        End Get
    End Property

    Public ReadOnly Property BootstrappedSE As Double
        Get
            If Not marginalLikelihoodCalculated Then
                calculate()
            End If

            Return _bootstrappedSE
        End Get
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="sample"> </param>
    ''' <param name="burnin">          used for 'toString' display purposes only </param>
    ''' <param name="analysisType"> </param>
    ''' <param name="bootstrapLength"> a value of zero will turn off bootstrapping </param>
    Public Sub New(sample As List(Of Double), burnin As Integer, analysisType As AnalysisTypes, bootstrapLength As Integer)
        Me.sample = sample
        Me.Burnin = burnin
        Me.analysisType = analysisType
        Me.bootstrapLength = bootstrapLength
    End Sub

    Public Function calculateLogMarginalLikelihood(sample As List(Of Double)) As Double
        Select Case analysisType
            Case AnalysisTypes.AICM : Return logMarginalLikelihoodAICM(sample)
            Case AnalysisTypes.Smoothed : Return logMarginalLikelihoodSmoothed(sample)
            Case AnalysisTypes.Arithmetic : Return logMarginalLikelihoodArithmetic(sample)
            Case Else
                Return logMarginalLikelihoodHarmonic(sample)
        End Select
    End Function

    ''' <summary>
    ''' Calculates the log marginal likelihood of a model using the arithmetic mean estimator
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the log marginal likelihood </returns>
    Public Function logMarginalLikelihoodArithmetic(v As List(Of Double)) As Double
        Dim size As Integer = v.Count
        Dim sum As Double = LogTricks.logZero

        For i As Integer = 0 To size - 1
            sum = LogTricks.logSum(sum, v(i))
        Next

        Return sum - std.Log(size)
    End Function

    ''' <summary>
    ''' Calculates the log marginal likelihood of a model using Newton and Raftery's harmonic mean estimator
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the log marginal likelihood </returns>
    Public Function logMarginalLikelihoodHarmonic(v As List(Of Double)) As Double
        Dim sum As Double = 0
        Dim size As Integer = v.Count

        For i As Integer = 0 To size - 1
            sum += v(i)
        Next

        Dim denominator As Double = LogTricks.logZero

        For i As Integer = 0 To size - 1
            denominator = LogTricks.logSum(denominator, sum - v(i))
        Next

        Return sum - denominator + std.Log(size)
    End Function

    ''' <summary>
    ''' Calculates the AICM of a model using method-of-moments from Raftery et al. (2007)
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the AICM (lower values are better) </returns>

    Public Function logMarginalLikelihoodAICM(v As List(Of Double)) As Double

        Dim sum As Double = 0
        Dim size As Integer = v.Count

        For i As Integer = 0 To size - 1
            sum += v(i)
        Next

        Dim mean As Double = sum / CDbl(size)
        Dim var As Double = 0

        For i As Integer = 0 To size - 1
            var += (v(i) - mean) * (v(i) - mean)
        Next

        var /= CDbl(size) - 1

        Return 2 * var - 2 * mean
    End Function

    Public Sub calculate()
        _logMarginalLikelihood = calculateLogMarginalLikelihood(sample)

        If bootstrapLength > 1 Then
            Dim sampleLength As Integer = sample.Count
            Dim bsSample As New List(Of Double)
            Dim bootstrappedLogML As Double() = New Double(bootstrapLength) {}
            Dim sum As Double = 0

            For i As Integer = 0 To bootstrapLength - 1
                Dim indices As Integer() = MathUtils.sampleIndicesWithReplacement(sampleLength)

                For k As Integer = 0 To sampleLength - 1
                    bsSample.Insert(k, sample(indices(k)))
                Next k

                bootstrappedLogML(i) = calculateLogMarginalLikelihood(bsSample)
                sum += bootstrappedLogML(i)
            Next

            sum /= bootstrapLength

            Dim bootstrappedAverage As Double = sum
            ' Summarize bootstrappedLogML
            Dim var As Double = 0
            For i As Integer = 0 To bootstrapLength - 1
                var += (bootstrappedLogML(i) - bootstrappedAverage) * (bootstrappedLogML(i) - bootstrappedAverage)
            Next
            var /= (bootstrapLength - 1.0)
            _bootstrappedSE = std.Sqrt(var)
        End If

        marginalLikelihoodCalculated = True
    End Sub

    ''' <summary>
    ''' Calculates the log marginal likelihood of a model using Newton and Raftery's smoothed estimator
    ''' </summary>
    ''' <param name="v">     a posterior sample of logLikelihood </param>
    ''' <param name="delta"> proportion of pseudo-samples from the prior </param>
    ''' <param name="Pdata"> current estimate of the log marginal likelihood </param>
    ''' <returns> the log marginal likelihood </returns>
    Public Function logMarginalLikelihoodSmoothed(v As IList(Of Double), delta As Double, Pdata As Double) As Double

        Dim logDelta As Double = std.Log(delta)
        Dim logInvDelta As Double = std.Log(1.0 - delta)
        Dim n As Integer = v.Count
        Dim logN As Double = std.Log(n)

        Dim offset As Double = logInvDelta - Pdata

        Dim bottom As Double = logN + logDelta - logInvDelta
        Dim top As Double = bottom + Pdata
        Dim weight As Double

        For i As Integer = 0 To n - 1
            weight = -LogTricks.logSum(logDelta, offset + v(i))
            top = LogTricks.logSum(top, weight + v(i))
            bottom = LogTricks.logSum(bottom, weight)
        Next

        Return top - bottom
    End Function

    Public Function logMarginalLikelihoodSmoothed(v As IList(Of Double?)) As Double

        Const delta As Double = 0.01 ' todo make class adjustable by accessor/setter

        ' Start with harmonic estimator as first guess
        Dim Pdata As Double = logMarginalLikelihoodHarmonic(v)
        Dim deltaP As Double = 1.0
        Dim iterations As Integer = 0
        Dim dx As Double

        Const tolerance As Double = 0.001 ' todo make class adjustable by accessor/setter

        Do While std.Abs(deltaP) > tolerance
            Dim g1 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata) - Pdata
            Dim Pdata2 As Double = Pdata + g1
            dx = g1 * 10.0
            Dim g2 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata + dx) - (Pdata + dx)
            Dim dgdx As Double = (g2 - g1) / dx ' find derivative at Pdata

            Dim Pdata3 As Double = Pdata - g1 / dgdx ' find new evaluation point

            If Pdata3 < 2.0 * Pdata OrElse Pdata3 > 0 OrElse Pdata3 > 0.5 * Pdata Then ' step is too large Pdata3 = Pdata + 10.0 * g1
                Dim g3 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata3) - Pdata3

                ' Try to do a Newton's method step
                If std.Abs(g3) <= std.Abs(g2) AndAlso ((g3 > 0) OrElse (std.Abs(dgdx) > 0.01)) Then
                    deltaP = Pdata3 - Pdata
                    Pdata = Pdata3 ' otherwise try to go 10 times as far as one step
                ElseIf std.Abs(g2) <= std.Abs(g1) Then
                    Pdata2 += g2
                    deltaP = Pdata2 - Pdata
                    Pdata = Pdata2 ' otherwise go just one step
                Else
                    deltaP = g1
                    Pdata += g1
                End If

                iterations += 1

                If iterations > 400 Then ' todo make class adjustable by acessor/setter
                    Console.Error.WriteLine("Probabilities are not converging!!!") ' todo should throw exception
                    Return LogTricks.logZero
                End If
            End If
        Loop
        Return Pdata
    End Function
End Class
