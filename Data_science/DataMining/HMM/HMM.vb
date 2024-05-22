#Region "Microsoft.VisualBasic::58af541c831b9f7f8a6f6e72d15f9cbc, Data_science\DataMining\HMM\HMM.vb"

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

    '   Total Lines: 62
    '    Code Lines: 46 (74.19%)
    ' Comment Lines: 5 (8.06%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (17.74%)
    '     File Size: 2.46 KB


    ' Class HMM
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: backwardAlgorithm, baumWelchAlgorithm, bayesTheorem, forwardAlgorithm, GetTransMatrix
    '               viterbiAlgorithm
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix

''' <summary>
''' HMM用于建模数据序列，无论是从连续概率分布还是从离散概率分布得出的。
''' 它们与状态空间和高斯混合模型相关，因为它们旨在估计引起观测的状态。
''' 状态是未知或“隐藏”的，并且HMM试图估计状态，类似于无监督聚类过程。
''' </summary>
Public Class HMM

    Friend ReadOnly states As String()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly observables As String()
    Friend ReadOnly emissionMatrix As Double()()

    Friend ReadOnly Bayes As Bayes
    Friend ReadOnly Viterbi As Viterbi
    Friend ReadOnly Forward As Forward
    Friend ReadOnly Backward As Backward
    Friend ReadOnly BaumWelch As BaumWelch

    Sub New(states As StatesObject(), observables As Observable(), init As Double())
        Me.states = states.map(Function(s) s.state)
        Me.transMatrix = states.map(Function(s) s.prob)
        Me.initialProb = init
        Me.observables = observables.map(Function(o) o.obs)
        Me.emissionMatrix = observables.map(Function(o) o.prob)

        Me.Bayes = New Bayes(Me)
        Me.Viterbi = New Viterbi(Me)
        Me.Forward = New Forward(Me)
        Me.Backward = New Backward(Me)
        Me.BaumWelch = New BaumWelch(Me)
    End Sub

    Public Function GetTransMatrix() As Matrix
        Return New Matrix(transMatrix)
    End Function

    Public Function bayesTheorem(ob, hState) As Double
        Return Bayes.bayesTheorem(ob, hState)
    End Function

    Public Function forwardAlgorithm(obSequence As Chain) As Alpha
        Return Forward.forwardAlgorithm(obSequence)
    End Function

    Public Function backwardAlgorithm(obSequence As Chain) As Beta
        Return Backward.backwardAlgorithm(obSequence)
    End Function

    Public Function viterbiAlgorithm(obSequence As Chain) As viterbiSequence
        Return Viterbi.viterbiAlgorithm(obSequence)
    End Function

    Public Function baumWelchAlgorithm(obSequence As Chain) As HMM
        Return BaumWelch.baumWelchAlgorithm(obSequence)
    End Function
End Class
