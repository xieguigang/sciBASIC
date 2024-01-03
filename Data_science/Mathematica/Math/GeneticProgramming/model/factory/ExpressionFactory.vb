
Imports System.Collections.Generic
Imports System.Reflection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace model.factory

    Public Class ExpressionFactory
        Private ReadOnly leaf As Expression = Variable.X

        Private terminalExpressionsField As Expression()
        Private unaryExpressionsField, binaryExpressionsField As CompositeExpression()

        Public Sub New()
            terminalExpressionsField = New Expression() {Variable.X, New Number(0.0), New Number(1.0), E.e, PI.Pi}
            UnaryExpressions = CompositeExpression.UnaryTypes
            BinaryExpressions = CompositeExpression.BinaryTypes
        End Sub

        Public Overridable Property TerminalExpressions As Expression()
            Set(value As Expression())
                terminalExpressionsField = value

            End Set
            Get
                Return terminalExpressionsField
            End Get
        End Property

        Public Overridable WriteOnly Property UnaryExpressions As CompositeExpression()
            Set(value As CompositeExpression())
                unaryExpressionsField = value
            End Set
        End Property

        Public Overridable Property BinaryExpressions As CompositeExpression()
            Set(value As CompositeExpression())
                binaryExpressionsField = value
            End Set
            Get
                Return binaryExpressionsField
            End Get
        End Property

        Public Overridable Function generatePolyExpressions(size As Integer, order As Integer, rangeFrom As Double, rangeTo As Double) As IList(Of ExpressionWrapper)
            Dim list As IList(Of ExpressionWrapper) = New List(Of ExpressionWrapper)(size)
            For i = 0 To size - 1
                list.Add(generatePolyExpression(order, rangeFrom, rangeTo))
            Next
            Return list
        End Function

        Public Overridable Function generatePolyExpression(order As Integer, rangeFrom As Double, rangeTo As Double) As ExpressionWrapper
            If order < 1 Then
                Return New ExpressionWrapper(New Number(randf.NextDouble(rangeFrom, rangeTo)))
            Else

                Dim root As ExpressionWrapper = New ExpressionWrapper(New Plus(leaf, leaf))
                root.RightChild = generatePolyExpression(order - 1, rangeFrom, rangeTo)

                Dim node As ExpressionWrapper = New ExpressionWrapper(New Multiply(leaf, leaf))
                node.LeftChild = New ExpressionWrapper(New Number(randf.NextDouble(rangeFrom, rangeTo)))
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
            ElseIf randf.NextDouble() < 0.75 Then
                ' 75% chance for non-terminal, then 50% chance for binary/unary
                If binaryExpressionsField.Length > 0 AndAlso randf.NextBoolean() Then
                    Dim binary As ExpressionWrapper = createBinaryExpression()
                    binary.LeftChild = generateExpression(depth - 1)
                    binary.RightChild = generateExpression(depth - 1)
                    Return binary
                ElseIf unaryExpressionsField.Length > 0 Then
                    Dim unary As ExpressionWrapper = createUnaryExpression()
                    unary.Child = generateExpression(depth - 1)
                    Return unary
                End If
            End If

            Return generateExpression(depth - 1)
        End Function

        Public Overridable Function createTerminalExpression() As ExpressionWrapper
            Return New ExpressionWrapper(randf.[Next](terminalExpressionsField))
        End Function

        Public Overridable Function createUnaryExpression() As ExpressionWrapper
            Dim type = randf.[Next](unaryExpressionsField)

            Dim constructor = type.type.GetConstructor(BindingFlags.Public, New Type() {GetType(Expression)})
            Return New ExpressionWrapper(CType(constructor.Invoke(New Object() {leaf}), Expression))
        End Function

        Public Overridable Function createBinaryExpression() As ExpressionWrapper
            Dim type = randf.[Next](binaryExpressionsField)
            Dim constructor = type.type.GetConstructor(BindingFlags.Public, New Type() {GetType(Expression), GetType(Expression)})
            Return New ExpressionWrapper(CType(constructor.Invoke(New Object() {leaf, leaf}), Expression))

        End Function

    End Class

End Namespace
