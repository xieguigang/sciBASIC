#Region "Microsoft.VisualBasic::b269572ddb20e9b52d4e244c892a1b6d, Data_science\DataMining\HMM\Algorithm\calculateProb.vb"

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

    '   Total Lines: 31
    '    Code Lines: 25 (80.65%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (19.35%)
    '     File Size: 1.09 KB


    '     Class CalculateProb
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: SequenceProb
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm

    Public Class CalculateProb

        Dim stateTrans2 As Double()()
        Dim init As Double()
        Dim states As StatesObject()

        Sub New(stateTrans2 As Double()(), init As Double(), states As StatesObject())
            Me.states = states
            Me.init = init
            Me.stateTrans2 = stateTrans2
        End Sub

        Public Function SequenceProb(sequence As Chain) As Double
            Return findSequence(sequence, states) _
                .reduce(Function(total As Double, curr As Integer, i As Integer, arr As Integer())
                            If (i = 0) Then
                                total += init(curr)
                            Else
                                total *= stateTrans2(arr(i - 1))(curr)
                            End If

                            Return total
                        End Function, 0.0)
        End Function
    End Class
End Namespace
