Imports Microsoft.VisualBasic.Data.IO.SearchEngine.Index

Module Module1

    Sub Main()
        Dim testfile$ = "./test_trieindex.db"

        Using index As New TrieIndexWriter(testfile.Open(doClear:=True))
            Call index.AddTerm("A Hello World!", 223388888)
            Call index.AddTerm("A", 88)
            Call index.AddTerm("AB", -1111)
            Call index.AddTerm("ABC", 11)
            Call index.AddTerm("ABCD", 222)
            Call index.AddTerm("ABCDE", 3333)
            Call index.AddTerm("ABCDF", 666)

            Call index.AddTerm("XABCCC", 999)
            Call index.AddTerm("ABCCD", 555)
            Call index.AddTerm("ABCEE", 222)
            Call index.AddTerm("XABCDE", 777)
            Call index.AddTerm("TrieIndexWriter", 1234567)
        End Using

        Using dmp = testfile.ChangeSuffix("log").OpenWriter
            Try
                Call DumpView.IndexDumpView(testfile, dmp)
            Catch ex As Exception

            End Try

            Call dmp.WriteLine()
        End Using

        '  Pause()

        Call Console.WriteLine()

        Using index As New TrieIndexReader(testfile)
            Call Console.WriteLine(index.GetData("TrieIndexWriter"))
            Call Console.WriteLine(index.GetData("A Hello World!"))
            Call Console.WriteLine(index.GetData("A"))
            Call Console.WriteLine(index.GetData("AB"))
            Call Console.WriteLine(index.GetData("ABCCD"))
            Call Console.WriteLine(index.GetData("ABCEE"))
            Call Console.WriteLine(index.GetData("XABCCC"))
            Call Console.WriteLine(index.GetData("ABCDF"))
            Call Console.WriteLine(index.GetData("XABCDE"))
        End Using

        Pause()
    End Sub

End Module
