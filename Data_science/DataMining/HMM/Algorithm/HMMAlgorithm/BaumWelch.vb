#Region "Microsoft.VisualBasic::e82ea8282ba7b73a56ae4da36954a331, Data_science\DataMining\HMM\Algorithm\HMMAlgorithm\BaumWelch.vb"

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

    '   Total Lines: 66
    '    Code Lines: 54
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.74 KB


    '     Class BaumWelch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: baumWelchAlgorithm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class BaumWelch : Inherits HMMAlgorithmBase

        Sub New(HMM As HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function baumWelchAlgorithm(obSequence As Chain) As HMM
            Dim forwardObj = New Forward(HMM).forwardAlgorithm(obSequence)
            Dim backwardBetas = New Backward(HMM).backwardAlgorithm(obSequence).betas.Reverse().ToArray
            Dim EMSteps = New EM(HMM, forwardObj, backwardBetas, obSequence)
            Dim initProb As New List(Of Double)
            Dim transMatrix As New List(Of Double())
            Dim emissMatrix As New List(Of Double())
            For i = 0 To HMM.states.Length - 1
                Dim stateTrans As New List(Of Double)

                initProb.Add(EMSteps.initialGamma(i))

                For j = 0 To HMM.states.Length - 1
                    stateTrans.Add(EMSteps.xiTransFromTo(i, j) / EMSteps.gammaTransFromState(i))
                Next

                transMatrix.Add(stateTrans.ToArray)
            Next

            For o = 0 To HMM.observables.Length - 1
                Dim obsEmiss As New List(Of Double)

                For i = 0 To HMM.states.Length - 1
                    obsEmiss.Add(EMSteps.gammaTimesInStateWithOb(i, o) / EMSteps.gammaTimesInState(i))
                Next

                emissMatrix.Add(obsEmiss.ToArray)
            Next

            Dim hiddenStates As StatesObject() = transMatrix _
                .reduce(Function(tot, curr, i)
                            Dim stateObj As New StatesObject With {
                                .state = HMM.states(i),
                                .prob = curr
                            }
                            tot.Add(stateObj)
                            Return tot
                        End Function, New List(Of StatesObject)) _
                .ToArray
            Dim observables As Observable() = emissMatrix _
                .reduce(Function(tot, curr, i)
                            Dim obsObj As New Observable With {
                                .obs = HMM.observables(i),
                                .prob = curr
                            }
                            tot.Add(obsObj)
                            Return tot
                        End Function, New List(Of Observable)) _
                .ToArray

            Return New HMM(hiddenStates, observables, initProb.ToArray)
        End Function
    End Class
End Namespace
