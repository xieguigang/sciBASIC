#Region "Microsoft.VisualBasic::4d47d48bd0cfa2ef00ad1fa9fe6a86ff, Data_science\DataMining\HMM\Algorithm\HMMAlgorithm\Bayes.vb"

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

    '     Class Bayes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: bayesTheorem
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class Bayes : Inherits HMMAlgorithmBase

        Sub New(HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function bayesTheorem(ob, hState) As Double
            Dim hStateIndex = HMM.states.IndexOf(hState)
            Dim obIndex As Integer = HMM.observables.indexOf(ob)
            Dim emissionProb = HMM.emissionMatrix(obIndex)(hStateIndex)
            Dim initHState = HMM.initialProb(hStateIndex)
            Dim obProb = HMM.emissionMatrix(obIndex) _
                .reduce(Function(total As Double, em As Double, i As Integer)
                            total += (em * HMM.initialProb(i))
                            Return total
                        End Function, 0.0)
            Dim bayesResult = (emissionProb * initHState) / obProb

            Return bayesResult
        End Function
    End Class
End Namespace
