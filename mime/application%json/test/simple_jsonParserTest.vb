#Region "Microsoft.VisualBasic::bc88ea83c1c1346c6195722a26af5b35, mime\application%json\JSONtest\simple_jsonParserTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 45
    '    Code Lines: 24 (53.33%)
    ' Comment Lines: 16 (35.56%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (11.11%)
    '     File Size: 1.56 KB


    ' Module simple_jsonParserTest
    ' 
    '     Sub: Main, test1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module simple_jsonParserTest

    ReadOnly failureTestExample As String = <json>{
    "id":"921a4932-0d77-490e-bb15-3bd9c3596fd1",
    "object":"chat.completion.chunk",
    "created":1784698240,
    "model":"deepseek-v4-flash",
    "system_fingerprint":"fp_8b330d02d0_prod0820_fp8_kvcache_20260402",
    "choices":[
        {"index":0,"delta":{"tool_calls":[{"index":0,"function":{"arguments":"C:\\\\Windows\\cmd.exe"}}]},"logprobs":null,"finish_reason":null}]}</json>

    ReadOnly LLM_test2 As String = <json>
                                       {
            "mechanism": "HIF-1α stabilization under hypoxia transcriptionally upregulates GLUT1, glycolytic enzymes, LDHA, and PDK1 (which inhibits PDH), shifting metabolism from OXPHOS to glycolysis.",
            "evidence": "Multiple papers: "HIF-1 induces GLUT1, glycolytic enzymes, LDHA" and "HIF-1 increases PDK1 to inhibit PDH" in breast cancer cells."
        }
                                   </json>

    ReadOnly LLM_test3 As String = <json>
                                        {"name": "Alice", "age": 
                                   </json>

    Sub Main()
        Call test3()
        Call test2()
        Call test1()
    End Sub

    Sub test3()
        Dim parsed = LenientJsonParser.ParseJSON(LLM_test2)

        parsed = LenientJsonParser.ParseJSON(LLM_test3)

        Pause()
    End Sub

    Sub test2()
        Dim escpae_strVal As JsonObject = JsonParser.Parse(failureTestExample)
        Dim choices As JsonArray = escpae_strVal!choices
        Dim opt As JsonObject = choices(0)
        Dim tool_call = opt!delta
        Dim args As JsonArray = DirectCast(tool_call, JsonObject)!tool_calls
        Dim firstVal As JsonObject = args(0)
        Dim firstFunc As JsonObject = firstVal!function
        Dim arg = firstFunc!arguments
        Dim str As String = DirectCast(arg, JsonValue).AsString(True)
        Pause()
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
