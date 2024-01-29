Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace model.impl

    Public MustInherit Class AbstractUnaryExpression
        Inherits AbstractExpression
        Implements UnaryExpression
        Public MustOverride Overrides Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Overrides Function eval(x As Double) As Double Implements Expression.eval

        Private ReadOnly constructor As ConstructorInfo

        Protected Friend m_child As Expression

        Public Sub New(child As Expression)
            m_child = child
            constructor = [GetType]().GetConstructorInfo(GetType(Expression))
        End Sub

        Public Overrides Function duplicate() As Expression
            Return constructor.Invoke(New Object() {m_child.duplicate()})
        End Function

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return m_child
            End Get
            Set(value As Expression)
                m_child = value
            End Set
        End Property


        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Dim old = m_child
            m_child = newChild
            Return old
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return 1 + m_child.Depth
            End Get
        End Property

    End Class

End Namespace
