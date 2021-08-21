#Region "Microsoft.VisualBasic::cc6b7a0e153c50499f77769576782af0, Data_science\DataMining\HMM\MarkovChain.vb"

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

    ' Class MarkovChain
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetTransMatrix, SequenceProb
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix

Public Class MarkovChain

    Friend ReadOnly states As String()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly prob As CalculateProb

    Sub New(states As StatesObject(), init As Double())
        Me.states = states.map(Function(s) s.state)
        Me.transMatrix = states.map(Function(s) s.prob)
        Me.initialProb = init
        Me.prob = New CalculateProb(transMatrix, init, states)
    End Sub

    Public Function GetTransMatrix() As Matrix
        Return New Matrix(transMatrix)
    End Function

    Public Function SequenceProb(sequence As Chain) As Double
        Return prob.SequenceProb(sequence)
    End Function
End Class
