Imports System
Imports System.Reflection

Namespace model.impl

    Public MustInherit Class AbstractUnaryExpression
        Inherits AbstractExpression
        Implements UnaryExpression
        Public MustOverride Overrides Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Overrides Function eval(x As Double) As Double Implements Expression.eval

        Private ReadOnly constructor As ConstructorInfo

        Protected Friend childField As Expression

        Public Sub New(child As Expression)
            childField = child


            constructor = [GetType]().GetConstructor(BindingFlags.Public, New Type() {GetType(Expression)})

        End Sub

        Public Overrides Function duplicate() As UnaryExpression


            Return CType(constructor.Invoke(New Object() {childField.duplicate()}), UnaryExpression)

        End Function

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return childField
            End Get
            Set(value As Expression)
                childField = value
            End Set
        End Property


        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Dim old = childField
            childField = newChild
            Return old
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return 1 + childField.Depth
            End Get
        End Property

    End Class

End Namespace
