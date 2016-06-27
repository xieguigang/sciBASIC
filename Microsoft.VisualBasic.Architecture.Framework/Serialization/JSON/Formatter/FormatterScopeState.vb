Imports System.Collections.Generic

Namespace Serialization.JSON.Formatter.Internals

    Friend NotInheritable Class FormatterScopeState
        Public Enum JsonScope
            [Object]
            Array
        End Enum

        ReadOnly scopeStack As New Stack(Of JsonScope)()

        Public ReadOnly Property IsTopTypeArray() As Boolean
            Get
                Return scopeStack.Count > 0 AndAlso scopeStack.Peek() = JsonScope.Array
            End Get
        End Property

        Public ReadOnly Property ScopeDepth() As Integer
            Get
                Return scopeStack.Count
            End Get
        End Property

        Public Sub PushObjectContextOntoStack()
            scopeStack.Push(JsonScope.[Object])
        End Sub

        Public Function PopJsonType() As JsonScope
            Return scopeStack.Pop()
        End Function

        Public Sub PushJsonArrayType()
            scopeStack.Push(JsonScope.Array)
        End Sub
    End Class
End Namespace
