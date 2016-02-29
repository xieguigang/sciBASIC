Imports Microsoft.VisualBasic.Linq

Namespace Scripting.TokenIcer

    Public Class Func(Of Tokens)

        Public Property Caller As List(Of InnerToken(Of Tokens))
        Public Property Args As Func(Of Tokens)()

        Public ReadOnly Property IsFuncCalls As Boolean
            Get
                Return Not Args.IsNullOrEmpty
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(currStack As InnerToken(Of Tokens))
            Caller = New List(Of InnerToken(Of Tokens)) From {currStack}
        End Sub

        ''' <summary>
        ''' 将表达式的栈空间展开
        ''' </summary>
        ''' <returns></returns>
        Public Function ToArray() As Token(Of Tokens)()

        End Function

        Public Overrides Function ToString() As String
            If Args.IsNullOrEmpty Then
                Return String.Join(" ", Caller.ToArray(Function(x) x.ToString))
            Else
                Dim caller As String = String.Join(" ", Me.Caller.ToArray(Function(x) x.ToString))
                Dim params As String() = Me.Args.ToArray(Function(x) x.ToString)
                Dim args As String = String.Join(", ", params)
                Return $"{caller}({args})"
            End If
        End Function
    End Class

    Public Class InnerToken(Of Tokens)
        Public Property obj As Token(Of Tokens)
        Public Property InnerStack As Func(Of Tokens)()

        Sub New(x As Token(Of Tokens), stack As IEnumerable(Of Func(Of Tokens)))
            obj = x
            InnerStack = stack.ToArray
        End Sub

        Sub New(x As Token(Of Tokens))
            obj = x
        End Sub

        Sub New(pretend As Tokens, funcCall As Func(Of Tokens))
            obj = New Token(Of Tokens)(pretend, "FuncCalls")
            InnerStack = {funcCall}
        End Sub

        Public Overrides Function ToString() As String
            If InnerStack.IsNullOrEmpty Then
                Return obj.TokenValue
            Else
                Dim inner As String() = InnerStack.ToArray(Function(x) x.ToString)
                Dim s As String = String.Join(" ", inner)
                Return $"({s})"
            End If
        End Function
    End Class
End Namespace