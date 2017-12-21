Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()

        Dim path = "./sdfsdfsdf.dat"

        Using write As New StringWriter(path)
            Call write.Append("633333333333336")
            Call write.Append({"ASDF", "12345", "00000"})
        End Using

        Dim reader As New StringReader(path)

        Console.WriteLine(reader.ReadString)
        Console.WriteLine(reader.ReadStringArray.GetJson)

        Pause()
    End Sub

End Module
