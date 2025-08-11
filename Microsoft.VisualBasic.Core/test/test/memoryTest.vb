Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Module memoryTest

    ReadOnly test As New TestData With {.bits = {2, 3, 4, 23, 53, 5, 34, 64, 5, 6}, .flag = True, .ints = {24342, 53, 453, 463, 456457, 5675}, .name = "[asdasdasfsdf+++++++++++++++++++++++++++++++++++++++++]"}

    Sub runTest()
        Dim bytes1 = test.StructureToByte
        Dim reload = bytes1.ByteToStructure(Of TestData)

        Call Console.WriteLine(test.bits.SequenceEqual(reload.bits))
        Call Console.WriteLine(test.ints.SequenceEqual(reload.ints))
        Call Console.WriteLine(test.flag = reload.flag)
        Call Console.WriteLine(test.name = reload.name)

        Pause()
    End Sub

End Module

Public Structure TestData

    Dim name As String
    Dim bits As Byte()
    Dim flag As Boolean
    Dim ints As Integer()

End Structure