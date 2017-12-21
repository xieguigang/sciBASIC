Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Module Module1

    Sub Main()

        Call reflectionTest()

        Call IOtest()

    End Sub

    Sub IOtest()
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


    Sub reflectionTest()
        Dim path$ = "./fffffffffffffffffffff.dat"
        Dim p1$
        Dim p2%

        Using writer As New StringWriter(path)

            Dim write = writer.CreateWriter(Of int)

            p1 = write(New int(-9876))
            p2 = write(New int(88888888))

        End Using

        Dim reader As New StringReader(path)
        Dim read = reader.CreateReader(Of int)

        Console.WriteLine(read(0))
        Console.WriteLine(read(p1))

        Pause()
    End Sub
End Module
