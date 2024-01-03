Namespace model

    Public Class ExpressionWrapper
        Implements Expression, UnaryExpression, BinaryExpression

        Private expressionField As Expression

        Public Sub New(expression As Expression)
            expressionField = expression
        End Sub

        Public Overridable Property Expression As Expression
            Get
                Return expressionField
            End Get
            Set(value As Expression)
                expressionField = value
            End Set
        End Property


        Public Overridable ReadOnly Property Unary As Boolean
            Get
                Return TypeOf expressionField Is UnaryExpression
            End Get
        End Property

        Public Overridable ReadOnly Property Binary As Boolean
            Get
                Return TypeOf expressionField Is BinaryExpression
            End Get
        End Property

        Public Overridable Function duplicate() As Expression Implements Expression.duplicate
            Return New ExpressionWrapper(expressionField.duplicate())
        End Function

        Public Overridable Property LeftChild As Expression Implements BinaryExpression.LeftChild
            Get
                Return CType(expressionField, BinaryExpression).LeftChild
            End Get
            Set(value As Expression)
                CType(expressionField, BinaryExpression).LeftChild = value
            End Set
        End Property

        Public Overridable Property RightChild As Expression Implements BinaryExpression.RightChild
            Get
                Return CType(expressionField, BinaryExpression).RightChild
            End Get
            Set(value As Expression)
                CType(expressionField, BinaryExpression).RightChild = value
            End Set
        End Property



        Public Overridable Function swapLeftChild(newLeftChild As Expression) As Expression Implements BinaryExpression.swapLeftChild
            Return CType(expressionField, BinaryExpression).swapLeftChild(newLeftChild)
        End Function

        Public Overridable Function swapRightChild(newRightChild As Expression) As Expression Implements BinaryExpression.swapRightChild
            Return CType(expressionField, BinaryExpression).swapRightChild(newRightChild)
        End Function

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return CType(expressionField, UnaryExpression).Child
            End Get
            Set(value As Expression)
                CType(expressionField, UnaryExpression).Child = value
            End Set
        End Property


        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Return CType(expressionField, UnaryExpression).swapChild(newChild)
        End Function

        Public Overridable Function eval(x As Double) As Double Implements Expression.eval
            Return expressionField.eval(x)
        End Function

        Public Overridable ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return expressionField.Terminal
            End Get
        End Property

        Public Overridable Function toStringExpression() As String Implements Expression.toStringExpression
            Return expressionField.toStringExpression()
        End Function

        Public Overridable ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return expressionField.Depth
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return expressionField.ToString()
        End Function

    End Class

End Namespace
