#Region "Microsoft.VisualBasic::0c722db3af181929666356b9d8337d62, Data_science\DataMining\HMM\Algorithm\HMMAlgorithm\Forward.vb"

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

    '   Total Lines: 23
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 838 B


    '     Class Forward
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: forwardAlgorithm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models

Namespace Algorithm.HMMAlgorithm

    Public Class Forward : Inherits HMMAlgorithmBase

        Sub New(HMM As HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function forwardAlgorithm(obSequence As Chain) As Alpha
            Dim forward As New forwardFactory(HMM, obSequence)
            Dim initAlphas = forward.initForward()
            Dim allAlphas = forward.recForward(initAlphas.ToArray, 1, New List(Of List(Of Double)) From {initAlphas})

            Return New Alpha With {
                .alphas = allAlphas,
                .alphaF = forward.termForward(allAlphas)
            }
        End Function
    End Class
End Namespace
