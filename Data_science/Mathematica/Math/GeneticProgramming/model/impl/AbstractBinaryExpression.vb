Imports System.Reflection
Imports std = System.Math

Namespace model.impl

    Public MustInherit Class AbstractBinaryExpression
        Inherits AbstractExpression
        Implements BinaryExpression
        Public MustOverride Overrides Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Overrides Function eval(x As Double) As Double Implements Expression.eval

        Private ReadOnly constructor As ConstructorInfo

        Protected Friend leftChildField As Expression
        Protected Friend rightChildField As Expression

        Public Sub New(leftChild As Expression, rightChild As Expression)
            leftChildField = leftChild
            rightChildField = rightChild

            constructor = [GetType]().GetConstructor(BindingFlags.Public, New Type() {GetType(Expression), GetType(Expression)})

        End Sub

        Public Overrides Function duplicate() As Expression

            Return CType(constructor.Invoke(New Object() {leftChildField.duplicate(), rightChildField.duplicate()}), BinaryExpression)

        End Function

        Public Overridable Property LeftChild As Expression Implements BinaryExpression.LeftChild
            Get
                Return leftChildField
            End Get
            Set(value As Expression)
                leftChildField = value
            End Set
        End Property

        Public Overridable Property RightChild As Expression Implements BinaryExpression.RightChild
            Get
                Return rightChildField
            End Get
            Set(value As Expression)
                rightChildField = value
            End Set
        End Property



        Public Overridable Function swapLeftChild(newLeftChild As Expression) As Expression Implements BinaryExpression.swapLeftChild
            Dim old = leftChildField
            leftChildField = newLeftChild
            Return old
        End Function

        Public Overridable Function swapRightChild(newRightChild As Expression) As Expression Implements BinaryExpression.swapRightChild
            Dim old = rightChildField
            rightChildField = newRightChild
            Return old
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return 1 + std.Max(leftChildField.Depth, rightChildField.Depth)
            End Get
        End Property

    End Class

End Namespace
