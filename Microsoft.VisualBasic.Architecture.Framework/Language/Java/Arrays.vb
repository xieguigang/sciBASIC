Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Module Arrays

        <Extension>
        Public Sub Fill(Of T)(ByRef a As T(), val As T)
            For i% = 0 To a.Length - 1
                a(i%) = val
            Next
        End Sub

        Public Function copyOfRange(Of T)(matrix As T(), start As Integer, length As Integer) As T()
            Dim out As T() = New T(length - 1) {}
            Call Array.Copy(matrix, start, out, Scan0, length)
            Return out
        End Function
    End Module
End Namespace