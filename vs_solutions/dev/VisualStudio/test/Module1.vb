Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.SourceMap
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Call sourceMapDecodeTest()
        Call vlqtest()
    End Sub

    Sub sourceMapDecodeTest()
        Dim map As sourceMap = "D:\biodeep\biodeep_v2\biodeep\cdn.biodeep.cn\typescripts\build\linq.js.map".LoadJsonFile(Of sourceMap)
        Dim ref = map.decodeMappings.ToArray

        For Each n In ref
            Call Console.WriteLine(n.GetStackFrame(map).ToString)
        Next

        Pause()
    End Sub

    Sub vlqtest()
        Console.WriteLine(base64VLQ.base64VLQ_encode(16))
        Console.WriteLine(base64VLQ.base64VLQ_decode("gB"))
        Console.WriteLine(base64VLQ.getIntegers("AAAAA").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("BBBBB").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("CCCCC").ToArray.GetJson)

        Console.WriteLine(base64VLQ.getIntegers("AAgBC").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("SAAQ").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("CAAEA").ToArray.GetJson)

        Pause()
    End Sub
End Module
