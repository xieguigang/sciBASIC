Imports System.Runtime.CompilerServices
Imports System.Threading

Module toStringTest

    Sub Main()

        Dim strings$()

        Call BENCHMARK(Sub() strings = populatestrings(no:=True).Select(AddressOf Scripting.ToString).ToArray)
        Call BENCHMARK(Sub() strings = populatestrings(no:=False).Select(AddressOf Scripting.ToString).ToArray)

        Pause()
    End Sub

    Public Iterator Function populatestrings(no As Boolean) As IEnumerable(Of NoCStr)
        For i As Integer = 0 To 200
            If no Then
                Yield New NoCStr
            Else
                Yield New hasCStr
            End If

            Thread.Sleep(10)
        Next
    End Function

    Public Class NoCStr

        Dim s As String = RandomASCIIString(5, skipSymbols:=True)

        Public Overrides Function ToString() As String
            Return s
        End Function
    End Class

    Public Class hasCStr : Inherits NoCStr

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(h As hasCStr) As String
            Return h.ToString
        End Operator
    End Class
End Module
