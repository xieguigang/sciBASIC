#Region "Microsoft.VisualBasic::a988317fa83de740a1ce599b2fac17b9, mime\application%json\test\simple_jsonParserTest.vb"

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

    '   Total Lines: 171
    '    Code Lines: 106 (61.99%)
    ' Comment Lines: 34 (19.88%)
    '    - Xml Docs: 29.41%
    ' 
    '   Blank Lines: 31 (18.13%)
    '     File Size: 7.05 KB


    ' Module simple_jsonParserTest
    ' 
    '     Sub: Main11, test1, test2, test3, test4
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.application.json.LenientJson

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

    ''' <summary>
    ''' Strategy 16 regression sample: the closing quote of a value string is
    ''' dropped, so the value swallows the separator and the next key.
    ''' Correct form would be: {"a": "v1", "b": "v2", "c": "v3"}
    ''' </summary>
    ReadOnly missing_quote_test As String = "{""a"": ""v1,b"": ""v2,c"": ""v3""}"

    Sub Main11()
        ' Call test4()
        ' Call test3()
        ' Call test2()
        Call test1()
    End Sub

    ''' <summary>
    ''' Verifies Strategy 16 (missing closing quote recovery) against the
    ''' production document <c>test_error_json.json</c>, in which the closing
    ''' quote of nearly every string value was dropped by the LLM.
    ''' </summary>
    Sub test4()
        ' --- Minimal synthetic case -------------------------------------------
        Dim simple As JsonObject = LenientJsonParser.ParseJSON(missing_quote_test)

        Call Console.WriteLine("[Strategy 16] minimal case keys: " & simple.ObjectKeys.JoinBy(", "))

        For Each key As String In simple.ObjectKeys
            Call Console.WriteLine($"  {key} = {simple(key).AsString(True)}")
        Next

        ' --- The real production document -------------------------------------
        Dim file As String = "G:\pixelArtist\src\framework\mime\application%json\test\test_error_json.json"

        If Not file.FileExists Then
            Call Console.WriteLine($"test data file not found: {file.GetFullPath}")
            Call Pause()
            Return
        End If

        Dim doc As JsonObject = LenientJsonParser.Open(file)

        Call Console.WriteLine()
        Call Console.WriteLine("[Strategy 16] test_error_json.json top-level keys:")
        Call Console.WriteLine("  " & doc.ObjectKeys.JoinBy(", "))

        For Each key As String In {"module_index", "module_name", "xlsx_file"}
            Dim val As JsonElement = doc(key)
            Call Console.WriteLine($"  {key} = {If(val Is Nothing, "<missing>", val.AsString(True))}")
        Next

        Dim goal As JsonElement = doc("goal")

        If goal Is Nothing Then
            Call Console.WriteLine("  goal = <missing>")
        Else
            Dim text As String = goal.AsString(True)
            Call Console.WriteLine($"  goal ({text.Length} chars) = {Mid(text, 1, 40)}...")
        End If

        Dim sheets As JsonArray = TryCast(doc("sheets"), JsonArray)

        If sheets Is Nothing Then
            Call Console.WriteLine("  sheets = <missing or not an array>")
        Else
            Call Console.WriteLine($"  sheets = {sheets.length} element(s)")

            For i As Integer = 0 To sheets.length - 1
                Dim sheet As JsonObject = TryCast(sheets(i), JsonObject)

                If sheet Is Nothing Then
                    Call Console.WriteLine($"    [{i}] <not an object>")
                    Continue For
                End If

                Call Console.WriteLine($"    [{i}] keys: {sheet.ObjectKeys.JoinBy(", ")}")

                Dim name As JsonElement = sheet("sheet_name")
                Dim csv As JsonElement = sheet("csv")

                Call Console.WriteLine($"         sheet_name = {If(name Is Nothing, "<missing>", name.AsString(True))}")
                Call Console.WriteLine($"         csv        = {If(csv Is Nothing, "<missing>", csv.AsString(True))}")
            Next
        End If

        Pause()
    End Sub

    Sub test3()
        ' Regression: LLM_test2 contains internal unescaped quotes that are NOT
        ' followed by ':', so Strategy 15 must still keep them inside the string.
        Dim parsed = LenientJsonParser.ParseJSON(LLM_test2)

        parsed = LenientJsonParser.ParseJSON(LLM_test3)

        ' Pause()
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
