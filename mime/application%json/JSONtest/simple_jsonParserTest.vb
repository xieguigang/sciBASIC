Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module simple_jsonParserTest

    Sub Main()
        Call test1()
    End Sub

    Sub test1()
        ' Dim str As JsonValue = JsonParser.Parse("'abc'")
        ' Dim null As JsonValue = JsonParser.Parse("null")
        Dim vec1 As JsonArray = JsonParser.Parse("[1,2,3,4,5]")
        Dim obj As JsonObject = JsonParser.Parse("{'a': true, b: [3,3,4]}")

        Pause()
    End Sub
End Module
