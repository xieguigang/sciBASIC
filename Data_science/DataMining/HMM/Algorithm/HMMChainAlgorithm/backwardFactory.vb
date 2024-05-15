#Region "Microsoft.VisualBasic::a14f01d115b5df6c5c0a9906df6fb374, Data_science\DataMining\HMM\Algorithm\HMMChainAlgorithm\backwardFactory.vb"

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

    '   Total Lines: 50
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.01 KB


    '     Class backwardFactory
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: recBackward, termBackward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMChainAlgorithm

    Public Class backwardFactory : Inherits HMMChainAlgorithm

        Sub New(HMM As HMM, obSequence As Chain)
            Call MyBase.New(HMM, obSequence)
        End Sub

        Public Function recBackward(prevBetas As Double(), j As Integer, betas As List(Of Double())) As Double()()
            Dim obIndex = j
            Dim nextTrellis As New List(Of Double)

            If (obIndex = 0) Then
                Return betas.ToArray
            End If

            For s As Integer = 0 To HMM.states.Length - 1
                Dim trellisArr As New List(Of Double)
                Dim si As Integer = s

                Call prevBetas _
                    .ForEach(Sub(prob, i)
                                 Dim trans = HMM.transMatrix(si)(i)
                                 Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(i)
                                 trellisArr.Add(prob * trans * emiss)
                             End Sub)

                nextTrellis.Add(trellisArr.reduce(Function(tot, curr) tot + curr))
            Next

            betas.Add(nextTrellis.ToArray)

            Return recBackward(nextTrellis.ToArray, obIndex - 1, betas)
        End Function

        Public Function termBackward(betas As Double()()) As Double
            Dim finalBetas = betas(betas.Length - 1) _
                .reduce(Function(tot As List(Of Double), curr As Double, i As Integer)
                            Dim obIndex = HMM.observables.IndexOf(obSequence(0))
                            Dim obEmission = HMM.emissionMatrix(obIndex)
                            tot.Add(curr * HMM.initialProb(i) * obEmission(i))
                            Return tot
                        End Function, New List(Of Double))

            Return finalBetas.reduce(Function(tot, Val) tot + Val, 0.0)
        End Function
    End Class
End Namespace
