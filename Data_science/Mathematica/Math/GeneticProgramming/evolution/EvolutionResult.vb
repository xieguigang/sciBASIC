#Region "Microsoft.VisualBasic::74b8de3d3955b2e72023b991e7a97c57, Data_science\Mathematica\Math\GeneticProgramming\evolution\EvolutionResult.vb"

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

    '   Total Lines: 26
    '    Code Lines: 22 (84.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (15.38%)
    '     File Size: 1001 B


    '     Class EvolutionResult
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model

Namespace evolution

    Public Class EvolutionResult
        Public ReadOnly result As Expression
        Public ReadOnly fitness As Double
        Public ReadOnly time As Long
        Public ReadOnly epochs As Integer
        Public ReadOnly fitnessProgress As IList(Of Double)
        Public ReadOnly timeProgress As IList(Of Long)

        Public Sub New(result As Expression, fitness As Double, time As Long, epochs As Integer, fitnessProgress As IList(Of Double), timeProgress As IList(Of Long))
            Me.result = result
            Me.fitness = fitness
            Me.time = time
            Me.epochs = epochs
            Me.fitnessProgress = fitnessProgress
            Me.timeProgress = timeProgress
        End Sub

        Public Overrides Function ToString() As String
            Return $"fitness:{fitness} - {result.toStringExpression}"
        End Function
    End Class
End Namespace
