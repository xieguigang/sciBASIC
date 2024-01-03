Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports rndf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace model.factory

    Public Class ExpressionFactory

        Private ReadOnly leaf As Expression = Variable.X

        Public Overridable Property TerminalExpressions As Expression()
        Public Overridable Property UnaryExpressions As CompositeExpression()
        Public Overridable Property BinaryExpressions As CompositeExpression()

        Public Sub New()
            TerminalExpressions = New Expression() {Variable.X, New Number(0.0), New Number(1.0), E.e, PI.Pi, Tau.Tau}
            UnaryExpressions = CompositeExpression.UnaryTypes
            BinaryExpressions = CompositeExpression.BinaryTypes
        End Sub

        Public Overridable Function generatePolyExpressions(size As Integer, order As Integer, rangeFrom As Double, rangeTo As Double) As IList(Of ExpressionWrapper)
            Dim list As IList(Of ExpressionWrapper) = New List(Of ExpressionWrapper)(size)
            For i = 0 To size - 1
                list.Add(generatePolyExpression(order, rangeFrom, rangeTo))
            Next
            Return list
        End Function

        Public Overridable Function generatePolyExpression(order As Integer, rangeFrom As Double, rangeTo As Double) As ExpressionWrapper
            If order < 1 Then
                Return New ExpressionWrapper(New Number(rndf.NextDouble(rangeFrom, rangeTo)))
            Else

                Dim root As ExpressionWrapper = New ExpressionWrapper(New Plus(leaf, leaf))
                root.RightChild = generatePolyExpression(order - 1, rangeFrom, rangeTo)

                Dim node As ExpressionWrapper = New ExpressionWrapper(New Multiply(leaf, leaf))
                node.LeftChild = New ExpressionWrapper(New Number(rndf.NextDouble(rangeFrom, rangeTo)))
                root.LeftChild = node

                For i = 0 To order - 1 - 1
                    Dim [next] As ExpressionWrapper = New ExpressionWrapper(New Multiply(leaf, leaf))
                    [next].LeftChild = New ExpressionWrapper(Variable.X)
                    node.RightChild = [next]
                    node = [next]
                Next
                node.RightChild = New ExpressionWrapper(Variable.X)

                Return root
            End If
        End Function

        Public Overridable Function generateExpressions(size As Integer, depth As Integer) As IList(Of ExpressionWrapper)
            Dim list As IList(Of ExpressionWrapper) = New List(Of ExpressionWrapper)(size)
            For i = 0 To size - 1
                list.Add(generateExpression(depth))
            Next
            Return list
        End Function

        Public Overridable Function generateExpression(depth As Integer) As ExpressionWrapper
            If depth < 1 Then
                Return createTerminalExpression()
            ElseIf rndf.NextDouble() < 0.75 Then
                ' 75% chance for non-terminal, then 50% chance for binary/unary
                If BinaryExpressions.Length > 0 AndAlso rndf.NextBoolean() Then
                    Dim binary As ExpressionWrapper = createBinaryExpression()
                    binary.LeftChild = generateExpression(depth - 1)
                    binary.RightChild = generateExpression(depth - 1)
                    Return binary
                ElseIf unaryExpressions.Length > 0 Then
                    Dim unary As ExpressionWrapper = createUnaryExpression()
                    unary.Child = generateExpression(depth - 1)
                    Return unary
                End If
            End If

            Return generateExpression(depth - 1)
        End Function

        Public Overridable Function createTerminalExpression() As ExpressionWrapper
            Return New ExpressionWrapper(rndf.[Next](TerminalExpressions))
        End Function

        Public Overridable Function createUnaryExpression() As ExpressionWrapper
            Dim expr = rndf.[Next](UnaryExpressions)
            Dim constructor = expr.type.GetConstructorInfo(GetType(Expression))

            Return New ExpressionWrapper(CType(constructor.Invoke(New Object() {leaf}), Expression))
        End Function

        Public Overridable Function createBinaryExpression() As ExpressionWrapper
            Dim expr = rndf.[Next](BinaryExpressions)
            Dim constructor = expr.type.GetConstructorInfo(GetType(Expression), GetType(Expression))

            Return New ExpressionWrapper(CType(constructor.Invoke(New Object() {leaf, leaf}), Expression))
        End Function

    End Class

End Namespace
