Imports System.Runtime.CompilerServices
Imports System.Reflection

Namespace Scripting.Runtime

    Module Extensions

        <Extension>
        Public Function OverloadsBinaryOperator(methods As IEnumerable(Of MethodInfo)) As BinaryOperator
            Return BinaryOperator.CreateOperator(methods?.ToArray)
        End Function
    End Module
End Namespace