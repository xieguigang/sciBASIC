#Region "Microsoft.VisualBasic::6ef1a2f35c001382b3869809465abd8c, Data_science\DataMining\HMM\Algorithm\HMMAlgorithm\Viterbi.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24 (80.00%)
    ' Comment Lines: 1 (3.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 1.38 KB


    '     Class Viterbi
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: viterbiAlgorithm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class Viterbi : Inherits HMMAlgorithmBase

        Sub New(HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function viterbiAlgorithm(obSequence As Chain) As viterbiSequence
            Dim viterbi As New viterbiFactory(HMM, obSequence)
            Dim initTrellis = viterbi.initViterbi()
            ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
            Dim psiArrays As New PsiArray(HMM.states.map(Function(s) New List(Of Integer)))
            Dim recTrellisPsi = viterbi.recViterbi(initTrellis.ToArray, 1, psiArrays, New List(Of Double()) From {initTrellis.ToArray})
            Dim pTerm = viterbi.termViterbi(recTrellisPsi)
            Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)

            Return New viterbiSequence With {
                .stateSequence = backtrace,
                .trellisSequence = recTrellisPsi.trellisSequence,
                .terminationProbability = pTerm.maximizedProbability
            }
        End Function
    End Class
End Namespace
