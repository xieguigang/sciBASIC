Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module simple_jsonParserTest

    Sub Main()
        Call test1()
    End Sub

    Sub test1()
        ' Dim str As JsonValue = JsonParser.Parse("'abc'")
        ' Dim null As JsonValue = JsonParser.Parse("null")
        ' Dim vec1 As JsonArray = JsonParser.Parse("[-1,1,2,3,4,5]")
        ' Dim obj As JsonObject = JsonParser.Parse("{'a': true, b: [3,3,4]}")
        ' Dim literal As JsonValue = JsonParser.Parse("false//a scalar boolean value")
        ' Dim empty_array As JsonArray = JsonParser.Parse("[]")
        ' Dim empty_obj As JsonObject = JsonParser.Parse("{}")
        ' Dim escape_str As JsonValue = JsonParser.Parse("'this is \'string\', another ""string block"".'")
        Dim escpae_strVal As JsonArray = JsonParser.Parse("['this is \'string\', \nanother ""string block"".']")
        Dim obj_no_comment = JsonParser.Parse("
        {
            'a': true,
            // is an integer vector
            'v': [1,1,1,3,4,5],
            'empty_array': [],
            // is a string
            'str': 'hello ""world""!',
            // string in multiple lines
            ""text"": '
                line1
                line2
                line3
            ',
            'nest_object': {
                'empty': {},
                scalar: false
            },
            'flag': false// is a single comment line
        }

        ")

        Pause()
    End Sub
End Module
