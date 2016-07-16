Imports System.Runtime.CompilerServices

Namespace Language.Python

    Public Module Collection

        <Extension>
        Public Iterator Function slice(Of T)([set] As IEnumerable(Of T), start As Integer, [stop] As Integer, Optional [step] As Integer = 1) As IEnumerable(Of T)
            Dim array As T() = [set].Skip(start).ToArray

            If [stop] < 0 Then
                [stop] = array.Length + [stop]
            End If

            For i As Integer = 0 To [stop] Step [step]
                Yield array(i)
            Next
        End Function
    End Module
End Namespace