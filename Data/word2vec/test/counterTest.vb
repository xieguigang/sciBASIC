Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Module counterTest

    Public Sub Main()
        Dim strKeys = New String() {"1", "2", "3", "1", "2", "1", "3", "3", "3", "1", "2"}
        Dim counter As Counter(Of String) = New Counter(Of String)()

        For Each strKey In strKeys
            counter.add(strKey)
        Next

        For Each strKey As String In counter.keySet
            Console.WriteLine(strKey & " : " & counter.get(strKey))
        Next

        Console.WriteLine(counter.get("9"))
        '        System.out.println(Long.MAX_VALUE);

        Pause()
    End Sub
End Module
