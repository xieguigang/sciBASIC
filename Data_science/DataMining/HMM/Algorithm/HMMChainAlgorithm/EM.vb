#Region "Microsoft.VisualBasic::5c43d712ea72f4f6c37b87a20b620846, Data_science\DataMining\HMM\Algorithm\HMMChainAlgorithm\EM.vb"

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

    '   Total Lines: 78
    '    Code Lines: 57 (73.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (26.92%)
    '     File Size: 3.06 KB


    '     Class EM
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: gammaTimesInState, gammaTimesInStateWithOb, gammaTransFromState, initialGamma, xiTransFromTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMChainAlgorithm

    Public Class EM : Inherits HMMChainAlgorithm

        Dim forwardObj As Alpha
        Dim backwardBetas As Double()()

        Sub New(HMM As HMM, forwardObj As Alpha, backwardBetas As Double()(), obSequence As Chain)
            Call MyBase.New(HMM, obSequence)

            Me.forwardObj = forwardObj
            Me.backwardBetas = backwardBetas
        End Sub

        Public Function initialGamma(stateI As Integer) As Double
            Return gamma(forwardObj.alphas(0)(stateI), backwardBetas(0)(stateI), forwardObj.alphaF)
        End Function

        Public Function gammaTimesInState(stateI As Integer) As Double
            Dim gammas As New List(Of Double)

            For t = 0 To obSequence.length - 1
                gammas.Add(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
            Next

            Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
        End Function

        Public Function gammaTransFromState(stateI As Integer) As Double
            Dim gammas As New List(Of Double)

            For t = 0 To obSequence.length - 2
                gammas.Add(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
            Next

            Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
        End Function

        Public Function xiTransFromTo(stateI As Integer, stateJ As Integer) As Double
            Dim xis As New List(Of Double)

            For t = 0 To obSequence.length - 2
                Dim alpha = forwardObj.alphas(t)(stateI)
                Dim trans = HMM.transMatrix(stateI)(stateJ)
                Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(t + 1)))(stateJ)
                Dim beta = backwardBetas(t + 1)(stateJ)

                xis.Add(xi(alpha, trans, emiss, beta, forwardObj.alphaF))
            Next

            Return xis.reduce(Function(tot, curr) tot + curr, 0.0)
        End Function

        Public Function gammaTimesInStateWithOb(stateI As Integer, obIndex As Integer) As Double
            Dim obsK = HMM.observables(obIndex)
            Dim stepsWithOb = obSequence.obSequence _
                .reduce(Function(tot, curr, i)
                            If (curr = obsK) Then
                                tot.Add(i)
                            End If

                            Return tot
                        End Function, New List(Of Integer))
            Dim gammas As New List(Of Double)

            Call stepsWithOb _
                .ForEach(Sub([step])
                             gammas.Add(gamma(forwardObj.alphas([step])(stateI), backwardBetas([step])(stateI), forwardObj.alphaF))
                         End Sub)

            Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
        End Function

    End Class
End Namespace
