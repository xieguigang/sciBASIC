Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Module Arrays

        <Extension>
        Public Sub Fill(Of T)(ByRef a As T(), val As T)
            For i% = 0 To a.Length - 1
                a(i%) = val
            Next
        End Sub
    End Module
End Namespace