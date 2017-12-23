Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Module TableTest

    Sub main()

        Call assertTest()
        Call test_dictionary()

        ' Dim dictionary As New Dictionary(Of Integer, Integer)
        Dim dictionary As BucketDictionary(Of Integer, Integer) = integers.CreateBuckets(Function(i) i, Function(i) i)


        Call VBDebugger.BENCHMARK(Sub() Console.WriteLine(55 = dictionary(55)))

        Console.WriteLine(dictionary.Count)

        Pause()
    End Sub

    Sub assertTest()
        Call Microsoft.VisualBasic.Language.Perl.ExceptionHandler.Default(New NamedValue(Of String))
    End Sub

    Private Sub test_dictionary()
        Dim dictionary As New Dictionary(Of Integer, Integer)

        Try
            For i As Integer = 0 To Integer.MaxValue
                dictionary.Add(i, i)
            Next
        Catch ex As Exception

        End Try

        Pause()
    End Sub

    Public Iterator Function integers() As IEnumerable(Of Integer)
        For i As Integer = 0 To Integer.MaxValue / 10 - 1
            Yield i
        Next
    End Function
End Module
