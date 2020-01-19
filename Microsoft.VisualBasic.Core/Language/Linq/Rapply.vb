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
            Dim populateArguments = Iterator Function(item As Tin) As IEnumerable(Of Object)
                                        Yield item

                                        For Each val As Object In args
                                            Yield val
                                        Next
                                    End Function
            Dim method As MethodInfo = apply.Method

            For Each item As Tin In sequence
                key = item.Key
                value = method.Invoke(apply.Target, populateArguments(item).ToArray)
                result.Add(key, value)
            Next

            Return result
        End Function
    End Module
End Namespace
