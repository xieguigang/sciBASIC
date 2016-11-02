Imports System.Runtime.CompilerServices

Namespace Language.C

    Public Module Vector

        <Extension>
        Public Sub Resize(Of T)(ByRef list As List(Of T), len%, Optional fill As T = Nothing)
            Call list.Clear()

            For i As Integer = 0 To len - 1
                Call list.Add(fill)
            Next
        End Sub
    End Module
End Namespace