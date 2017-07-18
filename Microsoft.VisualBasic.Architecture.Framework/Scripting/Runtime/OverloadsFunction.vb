Imports System.Reflection
Imports Microsoft.VisualBasic.Language

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

        End Function

        Public Overrides Function ToString() As String
            Return $"{Name} (+{functions.Length} Overloads)"
        End Function
    End Class
End Namespace