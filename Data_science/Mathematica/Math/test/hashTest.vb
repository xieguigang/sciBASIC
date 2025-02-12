Imports System.Text
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.HashMaps

Module hashTest

    Sub Main()
        Dim list = {"splash10-00ei-0900000000-22dd4bb393a82a83f8bd",
            "splash10-00ei-0900000000-245d9c908df7a0c94456",
            "splash10-00ei-0900000000-24dedfe93a91a232b6c2",
            "splash10-00ei-0900000000-253f6c22c8deea9cdad4",
            "splash10-00ei-0900000000-26dbfb87530f01e7ae8e",
            "splash10-00ei-0900000000-2b1d39e39c63a616bf41",
            "splash10-00ei-0900000000-2b20805f9d8296763102",
            "splash10-00ei-0900000000-2b238f40be5e61ec6512",
            "splash10-00ei-0900000000-2b3f0ac3ca9a14958dba",
            "splash10-00ei-0900000000-2eb14cff48e04abc660a",
            "splash10-00ei-0900000000-30f3f398ab274296074f",
            "splash10-00ei-0900000000-356548db80321d18935d",
            "splash10-00ei-0900000000-36152281406a11fbbc79",
            "splash10-00ei-0900000000-361dee3193514eddf43d"}

        Call hashtest2()

        For Each s As String In list
            Call Console.WriteLine($"{s}: {Crc32.CRC32Bytes(Encoding.ASCII.GetBytes(s))}")
        Next

        Pause()
    End Sub

    Sub hashtest2()
        Dim conflicts As Integer = 0
        Dim total As Integer = 10000000
        Dim hashSet As New System.Collections.Generic.HashSet(Of ULong)()

        For i As Integer = 0 To total - 1
            Dim a As Integer = i
            Dim b As Integer = i * 2
            Dim hash As ULong = HashMap.HashCodePair(a, b)
            If Not hashSet.Add(hash) Then
                conflicts += 1
            End If
        Next

        Console.WriteLine($"Total hashes: {total}")
        Console.WriteLine($"Total conflicts: {conflicts}")
        Console.WriteLine($"Conflict rate: {CDbl(conflicts) / total * 100}%")

        Pause()
    End Sub

    Sub stringKeys()
        Dim total As Integer = 10000000


    End Sub
End Module
