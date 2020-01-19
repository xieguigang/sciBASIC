Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Language

    ''' <summary>
    ''' Helper for implements ``lapply`` and ``sapply`` liked operations from R language
    ''' </summary>
    Public Module Rapply

        <Extension>
        Public Function lapply(Of Tin As INamedValue, TOut)(sequence As IEnumerable(Of Tin), apply As [Delegate], ParamArray args As Object()) As Dictionary(Of String, TOut)
            Dim result As New Dictionary(Of String, TOut)
            Dim key$
            Dim value As Object
            Dim arguments As Object() = New Object(args.Length) {}
            Dim method As MethodInfo = apply.Method
            Dim obj As Object = apply.Target

            Call Array.ConstrainedCopy(args, Scan0, arguments, 1, args.Length)

            For Each item As Tin In sequence
                key = item.Key
                arguments(Scan0) = item
                value = method.Invoke(obj, arguments)
                result.Add(key, value)
            Next

            Return result
        End Function
    End Module
End Namespace
