Imports System.Reflection

Namespace Scripting.Runtime

    Public Class OverloadsFunction

        Public ReadOnly Property Name As String

        ReadOnly functions As MethodInfo()

        Sub New(name$, methods As IEnumerable(Of MethodInfo))
            Me.Name = name
            Me.functions = methods.ToArray
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} (+{functions.Length} Overloads)"
        End Function
    End Class
End Namespace