#Region "Microsoft.VisualBasic::4010cf71e1aea71c9c3ec47b1a103053, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\MarginalLikelihoodAnalysis.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Language.Java

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

''' <summary>
''' @author Marc Suchard
''' @author Alexei Drummond
''' 
''' Source translated from ``model_P.c`` (a component of BAli-Phy by Benjamin Redelings and Marc Suchard
''' </summary>
Public Class MarginalLikelihoodAnalysis

    Private ReadOnly sample As IList(Of Double)
    Private ReadOnly analysisType As String ' "harmonic" for harmonic mean, "smoothed" for smoothed harmonic mean, "aicm" for AICM, "arithmetic" for arithmetic mean
    Private ReadOnly bootstrapLength As Integer

    Private marginalLikelihoodCalculated As Boolean = False
    Private _logMarginalLikelihood As Double
    Private _bootstrappedSE As Double

    Public Overridable Property Burnin As Integer

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="sample"> </param>
    ''' <param name="burnin">          used for 'toString' display purposes only </param>
    ''' <param name="analysisType"> </param>
    ''' <param name="bootstrapLength"> a value of zero will turn off bootstrapping </param>
    Public Sub New(ByVal sample As IList(Of Double), ByVal burnin As Integer, ByVal analysisType As String, ByVal bootstrapLength As Integer)
        Me.sample = sample
        Me.Burnin = burnin
        Me.analysisType = analysisType
        Me.bootstrapLength = bootstrapLength
    End Sub

    Public Overridable Function calculateLogMarginalLikelihood(ByVal sample As IList(Of Double)) As Double
        If analysisType.Equals("aicm") Then
            Return logMarginalLikelihoodAICM(sample)
        ElseIf analysisType.Equals("smoothed") Then
            Return logMarginalLikelihoodSmoothed(sample)
        ElseIf analysisType.Equals("arithmetic") Then
            Return logMarginalLikelihoodArithmetic(sample)
        Else
            Return logMarginalLikelihoodHarmonic(sample)
        End If
    End Function

    ''' <summary>
    ''' Calculates the log marginal likelihood of a model using the arithmetic mean estimator
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the log marginal likelihood </returns>
    Public Overridable Function logMarginalLikelihoodArithmetic(ByVal v As IList(Of Double?)) As Double
        Dim size As Integer = v.Count
        Dim sum As Double = LogTricks.logZero

        For i As Integer = 0 To size - 1
            sum = LogTricks.logSum(sum, v(i))
        Next i

        Return sum - Math.Log(size)
    End Function

    ''' <summary>
    ''' Calculates the log marginal likelihood of a model using Newton and Raftery's harmonic mean estimator
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the log marginal likelihood </returns>
    Public Overridable Function logMarginalLikelihoodHarmonic(ByVal v As IList(Of Double)) As Double
        Dim sum As Double = 0
        Dim size As Integer = v.Count
        For i As Integer = 0 To size - 1
            sum += v(i)
        Next i

        Dim denominator As Double = LogTricks.logZero

        For i As Integer = 0 To size - 1
            denominator = LogTricks.logSum(denominator, sum - v(i))
        Next i

        Return sum - denominator + Math.Log(size)
    End Function

    ''' <summary>
    ''' Calculates the AICM of a model using method-of-moments from Raftery et al. (2007)
    ''' </summary>
    ''' <param name="v"> a posterior sample of logLikelihoods </param>
    ''' <returns> the AICM (lower values are better) </returns>

    Public Overridable Function logMarginalLikelihoodAICM(ByVal v As IList(Of Double)) As Double

        Dim sum As Double = 0
        Dim size As Integer = v.Count
        For i As Integer = 0 To size - 1
            sum += v(i)
        Next i

        Dim mean As Double = sum / CDbl(size)

        Dim var As Double = 0
        For i As Integer = 0 To size - 1
            var += (v(i) - mean) * (v(i) - mean)
        Next i
        var /= CDbl(size) - 1

        Return 2 * var - 2 * mean

    End Function

    Public Overridable Sub calculate()
        _logMarginalLikelihood = calculateLogMarginalLikelihood(sample)

        If bootstrapLength > 1 Then
            Dim sampleLength As Integer = sample.Count
            Dim bsSample As IList(Of Double) = New List(Of Double?)
            Dim bootstrappedLogML As Double() = New Double(bootstrapLength) {}
            Dim sum As Double = 0

            For i As Integer = 0 To bootstrapLength - 1
                Dim indices As Integer() = MathUtils.sampleIndicesWithReplacement(sampleLength)
                For k As Integer = 0 To sampleLength - 1
                    bsSample.Insert(k, sample(indices(k)))
                Next k
                bootstrappedLogML(i) = calculateLogMarginalLikelihood(bsSample)
                sum += bootstrappedLogML(i)
            Next i
            sum /= bootstrapLength
            Dim bootstrappedAverage As Double = sum
            ' Summarize bootstrappedLogML
            Dim var As Double = 0
            For i As Integer = 0 To bootstrapLength - 1
                var += (bootstrappedLogML(i) - bootstrappedAverage) * (bootstrappedLogML(i) - bootstrappedAverage)
            Next i
            var /= (bootstrapLength - 1.0)
            _bootstrappedSE = Math.Sqrt(var)
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
    Public Overridable Function logMarginalLikelihoodSmoothed(ByVal v As IList(Of Double), ByVal delta As Double, ByVal Pdata As Double) As Double

        Dim logDelta As Double = Math.Log(delta)
        Dim logInvDelta As Double = Math.Log(1.0 - delta)
        Dim n As Integer = v.Count
        Dim logN As Double = Math.Log(n)

        Dim offset As Double = logInvDelta - Pdata

        Dim bottom As Double = logN + logDelta - logInvDelta
        Dim top As Double = bottom + Pdata

        For i As Integer = 0 To n - 1
            Dim weight As Double = -LogTricks.logSum(logDelta, offset + v(i))
            top = LogTricks.logSum(top, weight + v(i))
            bottom = LogTricks.logSum(bottom, weight)
        Next i

        Return top - bottom
    End Function

    Public Overridable ReadOnly Property LogMarginalLikelihood As Double
        Get
            If Not marginalLikelihoodCalculated Then calculate()
            Return _logMarginalLikelihood
        End Get
    End Property

    Public Overridable ReadOnly Property BootstrappedSE As Double
        Get
            If Not marginalLikelihoodCalculated Then calculate()
            Return _bootstrappedSE
        End Get
    End Property

    Public Overridable Function logMarginalLikelihoodSmoothed(ByVal v As IList(Of Double?)) As Double

        Const delta As Double = 0.01 ' todo make class adjustable by accessor/setter

        ' Start with harmonic estimator as first guess
        Dim Pdata As Double = logMarginalLikelihoodHarmonic(v)
        Dim deltaP As Double = 1.0
        Dim iterations As Integer = 0
        Dim dx As Double

        Const tolerance As Double = 0.001 ' todo make class adjustable by accessor/setter

        Do While Math.Abs(deltaP) > tolerance
            Dim g1 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata) - Pdata
            Dim Pdata2 As Double = Pdata + g1
            dx = g1 * 10.0
            Dim g2 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata + dx) - (Pdata + dx)
            Dim dgdx As Double = (g2 - g1) / dx ' find derivative at Pdata

            Dim Pdata3 As Double = Pdata - g1 / dgdx ' find new evaluation point

            If Pdata3 < 2.0 * Pdata OrElse Pdata3 > 0 OrElse Pdata3 > 0.5 * Pdata Then ' step is too large Pdata3 = Pdata + 10.0 * g1
                Dim g3 As Double = logMarginalLikelihoodSmoothed(v, delta, Pdata3) - Pdata3

                ' Try to do a Newton's method step
                If Math.Abs(g3) <= Math.Abs(g2) AndAlso ((g3 > 0) OrElse (Math.Abs(dgdx) > 0.01)) Then
                    deltaP = Pdata3 - Pdata
                    Pdata = Pdata3 ' otherwise try to go 10 times as far as one step
                ElseIf Math.Abs(g2) <= Math.Abs(g1) Then
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
