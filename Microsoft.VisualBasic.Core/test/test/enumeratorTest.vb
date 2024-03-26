Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class enumeratorTest : Implements Enumeration(Of String)

    Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
        Yield "1"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2000"
        Yield "29"
    End Function

End Class

Module enumeratorTestProgram

    Sub Mai2n()
        Dim strings As String() = New enumeratorTest().AsEnumerable.ToArray

        Call Console.WriteLine(strings.GetJson)

        Pause()
    End Sub
End Module