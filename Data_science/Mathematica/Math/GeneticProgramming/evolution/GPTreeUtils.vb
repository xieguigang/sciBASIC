#Region "Microsoft.VisualBasic::9d05ff7f586e6bcc1320a4f81533426e, Data_science\Mathematica\Math\GeneticProgramming\evolution\GPTreeUtils.vb"

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

    '   Total Lines: 176
    '    Code Lines: 123 (69.89%)
    ' Comment Lines: 17 (9.66%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 36 (20.45%)
    '     File Size: 7.12 KB


    '     Class GPTreeUtils
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: traverse
    ' 
    '         Sub: crossover, mutation, pointMutation, recTraverse, subtreeCrossover
    '              subtreeMutation
    '         Class TraverseResult
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Enum TreeMutationType
    ' 
    '             POINT_MUTATION, SUBTREE_MUTATION
    ' 
    ' 
    ' 
    '         Enum TreeCrossoverType
    ' 
    '             SUBTREE_CROSSOVER
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
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory
Imports rndf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace evolution

    Public Class GPTreeUtils

        Private Sub New()
        End Sub

        Public Shared Sub mutation(type As TreeMutationType, tree As GPTree, factory As ExpressionFactory)
            Select Case type
                Case TreeMutationType.POINT_MUTATION
                    pointMutation(tree, factory)
                Case TreeMutationType.SUBTREE_MUTATION
                    subtreeMutation(tree, factory)
            End Select
        End Sub

        Public Shared Sub pointMutation(tree As GPTree, factory As ExpressionFactory)


            ' select set of terminals or non-terminals
            Dim takeTerm As Boolean = tree.NonTerminals.Count = 0 OrElse rndf.NextBoolean() ' 50% chance
            Dim [set] As ISet(Of ExpressionWrapper) = If(takeTerm, tree.Terminals, tree.NonTerminals)

            ' select random element
            Dim node = rndf.[Next]([set])
            If node.Equals(tree.Root) Then
                Return
            End If

            ' create new expression
            Dim newNode As ExpressionWrapper
            If node.Unary Then
                newNode = factory.createUnaryExpression()
                newNode.Child = node.Child
            ElseIf node.Binary Then
                newNode = factory.createBinaryExpression()
                newNode.LeftChild = node.LeftChild
                newNode.RightChild = node.RightChild
            Else
                newNode = factory.createTerminalExpression()
            End If

            ' swap the expression
            node.Expression = newNode.Expression
        End Sub

        Public Shared Sub subtreeMutation(tree As GPTree, factory As ExpressionFactory)
            ' select set of terminals or non-terminals
            Dim takeTerm As Boolean = tree.NonTerminals.Count = 0 OrElse rndf.NextBoolean() ' 50% chance
            Dim [set] As ISet(Of ExpressionWrapper) = If(takeTerm, tree.Terminals, tree.NonTerminals)

            ' select random element
            Dim node = rndf.[Next]([set])
            If node.Equals(tree.Root) Then
                Return
            End If

            ' create new expression
            Dim newNode = factory.generateExpression(2)

            Dim resultOld = traverse(node)
            Dim resultNew = traverse(newNode)

            ' remove old subtrees
            tree.Terminals.removeAll(resultOld.terminals)
            tree.NonTerminals.removeAll(resultOld.nonTerminals)

            ' add new subtrees
            tree.Terminals.addAll(resultNew.terminals)
            tree.NonTerminals.addAll(resultNew.nonTerminals)

            ' swap the expression
            node.Expression = newNode.Expression

            ' re-compute depth
            tree.Depth = tree.Root.Depth
        End Sub

        Public Shared Sub crossover(type As TreeCrossoverType, treeOne As GPTree, treeTwo As GPTree)
            Select Case type
                Case TreeCrossoverType.SUBTREE_CROSSOVER
                    subtreeCrossover(treeOne, treeTwo)
            End Select
        End Sub

        Public Shared Sub subtreeCrossover(treeOne As GPTree, treeTwo As GPTree)
            ' select set of terminals or non-terminals
            Dim takeTerm1 = treeOne.NonTerminals.Count = 0 OrElse rndf.Next(10) = 0 ' 10% chance
            Dim oneSet = If(takeTerm1, treeOne.Terminals, treeOne.NonTerminals)
            Dim takeTerm2 = treeTwo.NonTerminals.Count = 0 OrElse rndf.Next(10) = 0 ' 10% chance
            Dim twoSet = If(takeTerm2, treeTwo.Terminals, treeTwo.NonTerminals)

            ' select random elements
            Dim one = rndf.Next(oneSet)
            Dim two = rndf.Next(twoSet)
            If one.Equals(treeOne.Root) OrElse two.Equals(treeTwo.Root) Then
                Return
            End If

            Dim resultOne = traverse(one)
            Dim resultTwo = traverse(two)

            ' remove old subtrees
            treeOne.Terminals.removeAll(resultOne.terminals)
            treeOne.NonTerminals.removeAll(resultOne.nonTerminals)
            treeTwo.Terminals.removeAll(resultTwo.terminals)
            treeTwo.NonTerminals.removeAll(resultTwo.nonTerminals)

            ' add new subtrees
            treeOne.Terminals.addAll(resultTwo.terminals)
            treeOne.NonTerminals.addAll(resultTwo.nonTerminals)
            treeTwo.Terminals.addAll(resultOne.terminals)
            treeTwo.NonTerminals.addAll(resultOne.nonTerminals)

            ' swap the expressions
            Dim tmp = one.Expression
            one.Expression = two.Expression
            two.Expression = tmp

            ' re-compute depths
            treeOne.Depth = treeOne.Root.Depth
            treeTwo.Depth = treeTwo.Root.Depth
        End Sub

        Public Shared Function traverse(expression As ExpressionWrapper) As TraverseResult

            Dim depth = expression.Depth
            Dim maxNodes = 1 << depth ' 2^depth
            Dim terminals As ISet(Of ExpressionWrapper) = New HashSet(Of ExpressionWrapper)()
            Dim nonTerminals As ISet(Of ExpressionWrapper) = New HashSet(Of ExpressionWrapper)()

            recTraverse(expression, terminals, nonTerminals)
            Return New TraverseResult(depth, terminals, nonTerminals)
        End Function

        Private Shared Sub recTraverse(expression As ExpressionWrapper, terminals As ISet(Of ExpressionWrapper), nonTerminals As ISet(Of ExpressionWrapper))
            If expression.Terminal Then
                terminals.Add(expression)
            ElseIf expression.Unary Then
                nonTerminals.Add(expression)
                recTraverse(CType(expression.Child, ExpressionWrapper), terminals, nonTerminals)
            ElseIf expression.Binary Then
                nonTerminals.Add(expression)
                recTraverse(CType(expression.LeftChild, ExpressionWrapper), terminals, nonTerminals)
                recTraverse(CType(expression.RightChild, ExpressionWrapper), terminals, nonTerminals)
            End If
        End Sub

        Public Class TraverseResult
            Public ReadOnly depth As Integer
            Public ReadOnly terminals As ISet(Of ExpressionWrapper)
            Public ReadOnly nonTerminals As ISet(Of ExpressionWrapper)

            Public Sub New(depth As Integer, terminals As ISet(Of ExpressionWrapper), nonTerminals As ISet(Of ExpressionWrapper))
                Me.depth = depth
                Me.terminals = terminals
                Me.nonTerminals = nonTerminals
            End Sub
        End Class

        Public Enum TreeMutationType
            POINT_MUTATION
            SUBTREE_MUTATION
        End Enum

        Public Enum TreeCrossoverType
            SUBTREE_CROSSOVER
        End Enum

    End Class

End Namespace
