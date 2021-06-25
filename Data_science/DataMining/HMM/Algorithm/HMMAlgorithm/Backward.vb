#Region "Microsoft.VisualBasic::0bcea484f1a1b3722fbe95ae4938b9ea, Data_science\DataMining\HMM\Algorithm\HMMAlgorithm\Backward.vb"

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

    '     Class Backward
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: backwardAlgorithm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class Backward : Inherits HMMAlgorithmBase

        Sub New(HMM As HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function backwardAlgorithm(obSequence As Chain) As Beta
            Dim backward As New backwardFactory(HMM, obSequence)
            Dim initBetas = HMM.states.map(Function(s) 1.0)
            Dim allBetas = backward.recBackward(initBetas, obSequence.length - 1, New List(Of Double()) From {initBetas})

            Return New Beta With {
                .betas = allBetas,
                .betaF = backward.termBackward(allBetas)
            }
        End Function
    End Class
End Namespace
