Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module simple_jsonParserTest

    Sub Main()
        Call test1()
    End Sub

    Sub test1()
        Dim str As JsonValue = JsonParser.Parse("'abc'")
        Dim null As JsonValue = JsonParser.Parse("null")

    End Sub
End Module
