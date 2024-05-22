#Region "Microsoft.VisualBasic::4d8da7e78fb2844a8c7c24267625be82, Data_science\Mathematica\Math\GeneticProgramming\evolution\GAPolynomial.vb"

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

    '   Total Lines: 95
    '    Code Lines: 75 (78.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (21.05%)
    '     File Size: 3.28 KB


    '     Class GAPolynomial
    ' 
    '         Properties: Expression, Fitness, Objective, Order, Parameters
    '                     Root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, computeFitness
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl

Namespace evolution

    Public Class GAPolynomial : Implements Individual

        Private Shared OBJECTIVEField As Objective = Nothing

        Public Shared WriteOnly Property Objective As Objective
            Set(value As Objective)
                OBJECTIVEField = value
            End Set
        End Property

        Private ReadOnly rootField As ExpressionWrapper

        Private ReadOnly parametersField As ISet(Of Number)

        Private orderField As Integer
        Private fitnessField As Double

        Public Sub New(root As ExpressionWrapper)
            rootField = root
            parametersField = GAPolynomialUtils.traverse(root)
            orderField = parametersField.Count - 1
            fitnessField = Double.NaN
        End Sub

        Protected Friend Overridable ReadOnly Property Root As ExpressionWrapper
            Get
                Return rootField
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property Parameters As ISet(Of Number)
            Get
                Return parametersField
            End Get
        End Property

        Protected Friend Overridable Property Order As Integer
            Set(value As Integer)
                orderField = value
            End Set
            Get
                Return orderField
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
