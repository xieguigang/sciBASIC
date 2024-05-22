#Region "Microsoft.VisualBasic::cb85c185e637a5a5a817554650c735e1, Data_science\Mathematica\Math\GeneticProgramming\evolution\GPTree.vb"

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

    '   Total Lines: 107
    '    Code Lines: 86 (80.37%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (19.63%)
    '     File Size: 3.69 KB


    '     Class GPTree
    ' 
    '         Properties: Depth, Expression, Fitness, NonTerminals, Objective
    '                     Root, Terminals
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, computeFitness
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model

Namespace evolution

    Public Class GPTree : Implements Individual

        Private Shared OBJECTIVEField As Objective = Nothing

        Public Shared Property Objective As Objective
            Set(value As Objective)
                OBJECTIVEField = value
            End Set
            Get
                Return OBJECTIVEField
            End Get
        End Property

        Private ReadOnly rootField As ExpressionWrapper

        Private ReadOnly terminalsField As ISet(Of ExpressionWrapper)
        Private ReadOnly nonTerminalsField As ISet(Of ExpressionWrapper)

        Private depthField As Integer
        Private fitnessField As Double

        Public Sub New(root As ExpressionWrapper)
            rootField = root
            Dim result = GPTreeUtils.traverse(root)
            depthField = result.depth
            terminalsField = result.terminals
            nonTerminalsField = result.nonTerminals
            fitnessField = Double.NaN
        End Sub

        Protected Friend Overridable ReadOnly Property Root As ExpressionWrapper
            Get
                Return rootField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property Terminals As ISet(Of ExpressionWrapper)
            Get
                Return terminalsField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property NonTerminals As ISet(Of ExpressionWrapper)
            Get
                Return nonTerminalsField
            End Get
        End Property

        Protected Friend Overridable Property Depth As Integer
            Set(value As Integer)
                depthField = value
            End Set
            Get
                Return depthField
            End Get
        End Property


        Public Overridable ReadOnly Property Expression As Expression Implements Individual.Expression
            Get
                Return rootField.Expression
            End Get
        End Property

        Public Overridable ReadOnly Property Fitness As Double Implements Individual.Fitness
            Get
                Return fitnessField
            End Get
        End Property

        Public Overridable Function computeFitness(dataTuples As IList(Of DataPoint)) As Double Implements Individual.computeFitness

            Dim errors = New Double(dataTuples.Count - 1) {}
            Dim iterator As IEnumerator(Of DataPoint) = dataTuples.GetEnumerator()
            Dim i = 0

            While iterator.MoveNext()
                Dim tuple = iterator.Current
                Dim y = rootField.eval(tuple.x)
                errors(i) = OBJECTIVEField.getError(tuple.y, y)
                i += 1
            End While
            fitnessField = OBJECTIVEField.getOverallError(errors)
            fitnessField = If(Double.IsNaN(fitnessField), Double.PositiveInfinity, fitnessField)
            Return fitnessField
        End Function

        Public Overridable Function CompareTo(other As Individual) As Integer Implements IComparable(Of Individual).CompareTo
            If other Is Nothing OrElse Fitness < other.Fitness Then
                Return -1
            ElseIf Fitness > other.Fitness Then
                Return +1
            Else
                Return 0
            End If
        End Function

    End Class

End Namespace
