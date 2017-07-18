Imports System.Reflection

Namespace Scripting.Runtime

    Public Class OverloadsFunction

        Public ReadOnly Property Name As String

        ReadOnly functions As MethodInfo()

        Sub New(name$, methods As IEnumerable(Of MethodInfo))
            Me.Name = name
            Me.functions = methods.ToArray
        End Sub

        Public Function Match(args As Type()) As MethodInfo
            Dim alignments = functions.Select(Function(m) Align(m, args)).ToArray
            Dim p = Which.Max(alignments)

            If alignments(p) <= 0 Then
                Return Nothing
            End If

            Dim method As MethodInfo = functions(p)
            Return method
        End Function

        Public Shared Function Align(target As MethodInfo, args As Type()) As Double
            Dim params = target.GetParameters

            If args.Length > params.Length Then
                Return -1
            End If

            Dim score#
            Dim tmp%

            For i As Integer = 0 To args.Length - 1
                tmp = 1000

                If Not args(i).IsInheritsFrom(params(i).ParameterType, False, tmp) Then
                    Return -1  ' 类型不符，则肯定不可以使用这个方法
                Else
                    score += (Short.MaxValue - tmp)
                End If
            Next

            Return score
        End Function

        Public Overrides Function ToString() As String
            Return $"{Name} (+{functions.Length} Overloads)"
        End Function
    End Class
End Namespace