Imports System.Runtime.CompilerServices

Namespace Serialization.JSON

    ''' <summary>
    ''' Extension helpers for deal with the anonymous type
    ''' </summary>
    Public Module AnonymousJSONExtensions

        <Extension>
        Public Function GetJson(obj As String(,)) As String
            With New Dictionary(Of String, String)
                For Each prop As String() In obj.RowIterator
                    Call .Add(prop(0), prop(1))
                Next

                Return .GetJson
            End With
        End Function
    End Module
End Namespace