#Region "Microsoft.VisualBasic::4b1a726bf8fb2cb8b4e36e8256d8c5e9, Data_science\DataMining\HMM\Algorithm\HMMChainAlgorithm\viterbiFactory.vb"

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

    '   Total Lines: 87
    '    Code Lines: 72
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 3.94 KB


    '     Class viterbiFactory
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: backViterbi, initViterbi, recViterbi, termViterbi
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMChainAlgorithm

    Public Class viterbiFactory : Inherits HMMChainAlgorithm

        Sub New(HMM As HMM, obSequence As Chain)
            Call MyBase.New(HMM, obSequence)
        End Sub

        Public Function initViterbi() As List(Of Double)
            Dim initTrellis As New List(Of Double)
            Dim obIndex = HMM.observables.IndexOf(obSequence(0))
            Dim obEmission = HMM.emissionMatrix(obIndex)

            Call HMM.initialProb _
                .ForEach(Sub(p, i)
                             initTrellis.Add(p * obEmission(i))
                         End Sub)

            Return initTrellis
        End Function

        Public Function recViterbi(prevTrellis() As Double, obIndex As Integer, psiArrays As PsiArray, trellisSequence As List(Of Double())) As TrellisPsi
            If (obIndex = obSequence.length) Then
                Return New TrellisPsi With {
                    .psiArrays = psiArrays,
                    .trellisSequence = trellisSequence.ToArray
                }
            End If

            Dim nextTrellis As Double() = HMM.states _
                .map(Function(state, stateIndex)
                         Dim trellisArr As Double() = prevTrellis _
                            .map(Function(prob, i)
                                     Dim trans = HMM.transMatrix(i)(stateIndex)
                                     Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(stateIndex)
                                     Return prob * trans * emiss
                                 End Function)
                         Dim maximized = trellisArr.Max()
                         psiArrays.Add(stateIndex, trellisArr.IndexOf(maximized))
                         Return maximized
                     End Function)

            trellisSequence.Add(nextTrellis)

            Return recViterbi(nextTrellis, obIndex + 1, psiArrays, trellisSequence)
        End Function

        Public Function termViterbi(recTrellisPsi As TrellisPsi) As termViterbi
            Dim finalTrellis As Double() = recTrellisPsi.trellisSequence(recTrellisPsi.trellisSequence.Length - 1)
            Dim maximizedProbability = finalTrellis.Max()

            recTrellisPsi.psiArrays _
                .forEach(Sub(psiArr, i)
                             psiArr.Add(finalTrellis.IndexOf(maximizedProbability))
                         End Sub)

            Return New termViterbi With {
                .maximizedProbability = maximizedProbability,
                .psiArrays = recTrellisPsi.psiArrays
            }
        End Function

        Public Function backViterbi(psiArrays As PsiArray) As String()
            Dim backtraceObj As List(Of Psi) = obSequence.obSequence _
                .reduce(Function(acc As List(Of Psi), currS As String, i As Integer)
                            If (acc.Count = 0) Then
                                Dim finalPsiIndex = psiArrays(0).Count - 1
                                Dim finalPsi = psiArrays(0)(finalPsiIndex)
                                acc.Add(New Psi With {.psi = finalPsi, .index = finalPsiIndex})
                                Return acc
                            End If
                            Dim prevPsi = acc(acc.Count - 1)
                            Dim psi = psiArrays(prevPsi.psi)(prevPsi.index - 1)
                            acc.Add(New Psi With {.psi = psi, .index = prevPsi.index - 1})
                            Return acc
                        End Function, New List(Of Psi))

            Return backtraceObj _
                .AsEnumerable _
                .Reverse() _
                .map(Function(e) HMM.states(e.psi))
        End Function
    End Class
End Namespace
