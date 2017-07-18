Imports System.Runtime.CompilerServices
Imports System.Reflection

Namespace Scripting.Runtime

    Module Extensions

        <Extension>
        Public Function OverloadsBinaryOperator(methods As IEnumerable(Of MethodInfo)) As BinaryOperator
            Return BinaryOperator.CreateOperator(methods?.ToArray)
        End Function

        <Extension>
        Public Function CreateArray(data As IEnumerable, type As Type) As Object
            Dim src = data.Cast(Of Object).ToArray
            Dim array As Array = Array.CreateInstance(type, src.Length)

            For i As Integer = 0 To src.Length - 1
                array.SetValue(src(i), i)
            Next

            Return array
        End Function
    End Module
End Namespace