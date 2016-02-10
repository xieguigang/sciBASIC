Imports Microsoft.VisualBasic.Linq

Namespace Scripting.TokenIcer

    Public Class Func(Of Tokens)

        Public Property Caller As List(Of Token(Of Tokens))
        Public Property Args As Func(Of Tokens)()

        Public ReadOnly Property IsFuncCalls As Boolean
            Get
                Return Not Args.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Args.IsNullOrEmpty Then
                Return String.Join(" ", Caller.ToArray(Function(x) x.TokenValue))
            Else
                Dim caller As String = String.Join(" ", Me.Caller.ToArray(Function(x) x.TokenValue))
                Dim params As String() = Me.Args.ToArray(Function(x) x.ToString)
                Dim args As String = String.Join(", ", params)
                Return $"{caller}({args})"
            End If
        End Function
    End Class
End Namespace