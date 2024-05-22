#Region "Microsoft.VisualBasic::29edb176a093fc4f94012f7a1dfbca06, Data_science\DataMining\HMM\Algorithm\HMMChainAlgorithm\forwardFactory.vb"

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

    '   Total Lines: 46
    '    Code Lines: 38 (82.61%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (17.39%)
    '     File Size: 1.97 KB


    '     Class forwardFactory
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: initForward, recForward, termForward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMChainAlgorithm

    Public Class forwardFactory : Inherits HMMChainAlgorithm

        Sub New(HMM As HMM, obSequence As Chain)
            Call MyBase.New(HMM, obSequence)
        End Sub

        Public Function initForward() As List(Of Double)
            Dim initTrellis As New List(Of Double)
            Dim obIndex = HMM.observables.IndexOf(obSequence(0))
            Dim obEmission = HMM.emissionMatrix(obIndex)
            HMM.initialProb.ForEach(Sub(p, i) initTrellis.Add(p * obEmission(i)))
            Return initTrellis
        End Function

        Public Function recForward(prevTrellis As Double(), j As Integer, alphas As List(Of List(Of Double))) As List(Of List(Of Double))
            Dim obIndex = j
            If (obIndex = obSequence.length) Then
                Return alphas
            End If
            Dim nextTrellis As New List(Of Double)
            For s As Integer = 0 To HMM.states.Length - 1
                Dim trellisArr As New List(Of Double)
                Dim si As Integer = s

                Call prevTrellis _
                    .ForEach(Sub(prob As Double, i As Integer)
                                 Dim trans = HMM.transMatrix(i)(si)
                                 Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(si)
                                 trellisArr.Add(prob * trans * emiss)
                             End Sub)

                nextTrellis.Add(trellisArr.reduce(Function(tot, curr) tot + curr, 0.0))
            Next
            alphas.Add(nextTrellis)
            Return recForward(nextTrellis.ToArray, obIndex + 1, alphas)
        End Function

        Public Function termForward(alphas As List(Of List(Of Double))) As Double
            Return alphas(alphas.Count - 1).reduce(Function(tot, val) tot + val, 0.0)
        End Function
    End Class
End Namespace
