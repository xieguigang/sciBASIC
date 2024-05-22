#Region "Microsoft.VisualBasic::a620d39478c3138276dfae1688597d5d, Data_science\Mathematica\Math\GeneticProgramming\evolution\GAPolynomialUtils.vb"

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

    '   Total Lines: 147
    '    Code Lines: 108 (73.47%)
    ' Comment Lines: 9 (6.12%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 30 (20.41%)
    '     File Size: 6.01 KB


    '     Class GAPolynomialUtils
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: traverse
    ' 
    '         Sub: arithmeticalCrossover, crossover, gaussianPointMutation, mutation, randomPointMutation
    '              recTraverse, simpleCrossover, simulatedBinaryCrossover
    '         Enum PolyMutationType
    ' 
    '             GAUSSIAN_POINT_MUTATION, RANDOM_POINT_MUTATION
    ' 
    ' 
    ' 
    '         Enum PolyCrossoverType
    ' 
    '             ARITHMETICAL_CROSSOVER, SIMPLE_CROSSOVER, SIMULATED_BINARY_CROSSOVER
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports rndf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace evolution


    Public Class GAPolynomialUtils

        Private Sub New()
        End Sub

        Public Shared Sub mutation(type As PolyMutationType, poly As GAPolynomial, rangeForm As Double, rangeTo As Double)
            Select Case type
                Case PolyMutationType.RANDOM_POINT_MUTATION
                    randomPointMutation(poly, rangeForm, rangeTo)
                Case PolyMutationType.GAUSSIAN_POINT_MUTATION
                    gaussianPointMutation(poly, rangeForm, rangeTo)
            End Select
        End Sub

        Public Shared Sub randomPointMutation(poly As GAPolynomial, rangeForm As Double, rangeTo As Double)
            ' select one parameter to change
            Dim toChange = Rndf.Next(poly.Parameters)
            ' assign a random number
            toChange.setNumber(Rndf.NextDouble(rangeForm, rangeTo))
        End Sub

        Public Shared Sub gaussianPointMutation(poly As GAPolynomial, rangeForm As Double, rangeTo As Double)

            ' select one parameter to change
            Dim toChange = Rndf.[Next](poly.Parameters)
            Dim delta = 0.1 * (rangeTo - rangeForm)
            ' change number by 'normal' increment
            toChange.setNumber(toChange.getNumber() + Rndf.NextGaussian() * delta)
        End Sub

        Public Shared Sub crossover(type As PolyCrossoverType, polyOne As GAPolynomial, polyTwo As GAPolynomial)
            Select Case type
                Case PolyCrossoverType.SIMPLE_CROSSOVER
                    simpleCrossover(polyOne, polyTwo)
                Case PolyCrossoverType.ARITHMETICAL_CROSSOVER
                    arithmeticalCrossover(polyOne, polyTwo)
                Case PolyCrossoverType.SIMULATED_BINARY_CROSSOVER
                    simulatedBinaryCrossover(polyOne, polyTwo)
            End Select
        End Sub

        Public Shared Sub simpleCrossover(polyOne As GAPolynomial, polyTwo As GAPolynomial)

            Dim paramOne As IEnumerator(Of Number) = polyOne.Parameters.GetEnumerator()
            Dim paramTwo As IEnumerator(Of Number) = polyTwo.Parameters.GetEnumerator()
            Dim n = polyOne.Parameters.Count
            If n < 2 Then
                ' order 0 is not supported
                Return
            End If

            ' selection of crossover point
            Dim point = Rndf.Next(n)
            ' swap parameters
            For i = 0 To point
                paramOne.MoveNext()
                paramTwo.MoveNext()

                Dim one = paramOne.Current
                Dim two = paramTwo.Current
                Dim tmp As Double = one.getNumber()
                one.setNumber(two.getNumber())
                two.setNumber(tmp)
            Next
        End Sub

        Public Shared Sub arithmeticalCrossover(polyOne As GAPolynomial, polyTwo As GAPolynomial)

            Dim paramOne As IEnumerator(Of Number) = polyOne.Parameters.GetEnumerator()
            Dim paramTwo As IEnumerator(Of Number) = polyTwo.Parameters.GetEnumerator()

            ' arithmetic recombination (affine combination)
            Dim lambda As Double = Rndf.NextDouble()
            While paramOne.MoveNext()
                paramTwo.MoveNext()

                Dim one = paramOne.Current
                Dim two = paramTwo.Current
                Dim n1 As Double = one.getNumber()
                Dim n2 As Double = two.getNumber()
                one.setNumber(lambda * n1 + (1 - lambda) * n2)
                two.setNumber((1 - lambda) * n1 + lambda * n2)
            End While
        End Sub

        Public Shared Sub simulatedBinaryCrossover(polyOne As GAPolynomial, polyTwo As GAPolynomial)


            Dim paramOne As IEnumerator(Of Number) = polyOne.Parameters.GetEnumerator()
            Dim paramTwo As IEnumerator(Of Number) = polyTwo.Parameters.GetEnumerator()

            ' simulated binary crossover
            Dim beta As Double = 1.0 + Rndf.NextGaussian()
            While paramOne.MoveNext()
                paramTwo.MoveNext()

                Dim one = paramOne.Current
                Dim two = paramTwo.Current
                Dim n1 As Double = one.getNumber()
                Dim n2 As Double = two.getNumber()
                one.setNumber(0.5 * (n1 + n2 + beta * std.Abs(n2 - n1)))
                two.setNumber(0.5 * (n1 + n2 - beta * std.Abs(n2 - n1)))
            End While
        End Sub

        Public Shared Function traverse(expression As ExpressionWrapper) As ISet(Of Number)

            Dim depth = expression.Depth
            Dim parameters As ISet(Of Number) = New HashSet(Of Number)()

            recTraverse(expression, parameters)
            Return parameters
        End Function

        Private Shared Sub recTraverse(expression As ExpressionWrapper, parameters As ISet(Of Number))
            If expression.Terminal AndAlso TypeOf expression.Expression Is Number Then
                parameters.Add(CType(expression.Expression, Number))
            ElseIf expression.Unary Then
                recTraverse(CType(expression.Child, ExpressionWrapper), parameters)
            ElseIf expression.Binary Then
                recTraverse(CType(expression.LeftChild, ExpressionWrapper), parameters)
                recTraverse(CType(expression.RightChild, ExpressionWrapper), parameters)
            End If
        End Sub

        Public Enum PolyMutationType
            RANDOM_POINT_MUTATION
            GAUSSIAN_POINT_MUTATION
        End Enum

        Public Enum PolyCrossoverType
            SIMPLE_CROSSOVER
            ARITHMETICAL_CROSSOVER
            SIMULATED_BINARY_CROSSOVER
        End Enum

    End Class

End Namespace
