Namespace model

    Public Class ExpressionWrapper
        Implements Expression, UnaryExpression, BinaryExpression

        Public Overridable Property Expression As Expression

        Public Overridable ReadOnly Property Unary As Boolean
            Get
                Return TypeOf _Expression Is UnaryExpression
            End Get
        End Property

        Public Overridable ReadOnly Property Binary As Boolean
            Get
                Return TypeOf _Expression Is BinaryExpression
            End Get
        End Property

        Public Overridable Property LeftChild As Expression Implements BinaryExpression.LeftChild
            Get
                Return DirectCast(_Expression, BinaryExpression).LeftChild
            End Get
            Set(value As Expression)
                DirectCast(_Expression, BinaryExpression).LeftChild = value
            End Set
        End Property

        Public Overridable Property RightChild As Expression Implements BinaryExpression.RightChild
            Get
                Return DirectCast(_Expression, BinaryExpression).RightChild
            End Get
            Set(value As Expression)
                DirectCast(_Expression, BinaryExpression).RightChild = value
            End Set
        End Property

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return CType(Expression, UnaryExpression).Child
            End Get
            Set(value As Expression)
                CType(Expression, UnaryExpression).Child = value
            End Set
        End Property

        Public Overridable ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return Expression.Terminal
            End Get
        End Property

        Public Overridable ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return Expression.Depth
            End Get
        End Property

        Public Sub New(expression As Expression)
            _Expression = expression
        End Sub

        Public Overridable Function duplicate() As Expression Implements Expression.duplicate
            Return New ExpressionWrapper(_Expression.duplicate())
        End Function

        Public Overridable Function swapLeftChild(newLeftChild As Expression) As Expression Implements BinaryExpression.swapLeftChild
            Return CType(Expression, BinaryExpression).swapLeftChild(newLeftChild)
        End Function

        Public Overridable Function swapRightChild(newRightChild As Expression) As Expression Implements BinaryExpression.swapRightChild
            Return CType(Expression, BinaryExpression).swapRightChild(newRightChild)
        End Function

        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Return CType(Expression, UnaryExpression).swapChild(newChild)
        End Function

        Public Overridable Function eval(x As Double) As Double Implements Expression.eval
            Return Expression.eval(x)
        End Function

        Public Overridable Function toStringExpression() As String Implements Expression.toStringExpression
            Return Expression.toStringExpression()
        End Function

        Public Overrides Function ToString() As String
            Return Expression.ToString()
        End Function

    End Class

End Namespace
