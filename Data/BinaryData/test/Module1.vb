#Region "Microsoft.VisualBasic::3a312545447d8bf372d830480bd473fa, Data\BinaryData\test\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Sub: IOtest, Main, reflectionTest, writeTest2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub writeTest2()
        Using out = New BinaryDataWriter("./sssss.txt".Open)
            Dim buffer As New IO.MemoryStream(Encoding.UTF8.GetBytes("Hello world!!! 撒比大师大师框架汇顶科技安徽科技案发后看见爱上" & RandomASCIIString(8192) & "writerBuffer test success;"))

            Call out.Write(buffer)
        End Using

        Pause()
    End Sub

    Sub Main()

        Call writeTest2()

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
