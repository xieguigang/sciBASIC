Imports Microsoft.VisualBasic.DataStorage.netCDF.Tests

Module Module1
    Sub Main()
        Dim failures = CDFReaderWriterTest.RunAll()

        If failures.Length = 0 Then
            Console.WriteLine("ALL TESTS PASSED")
        Else
            Console.WriteLine("TEST FAILURES: " & failures.Length)
            For Each f In failures
                Console.WriteLine("  - " & f)
            Next
        End If
    End Sub
End Module
