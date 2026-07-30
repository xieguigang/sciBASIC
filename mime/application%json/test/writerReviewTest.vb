#Region "JSONWriter review tests"

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' code-review regression tests for the JSONWriter fixes:
'''   - invariant-culture number formatting (no locale decimal separator)
'''   - quoted ISO-8601 date when unixTimestamp = false
'''   - NaN / +Infinity / -Infinity literals
'''   - control-character escaping in both unicode and non-unicode paths
'''   - non-ASCII escaping only in the unicode path
'''   - no blank line before the closing bracket of an indented array
'''
''' run by renaming this Sub to `Main` (the test project allows a single entry
''' point) and executing the JSONtest project.
''' </summary>
Module writerReviewTest

    Sub MainWriterReview()
        Dim failures As New List(Of String)
        Dim originalCulture = Thread.CurrentThread.CurrentCulture

        Try
            ' 1. numbers must be formatted with the invariant culture ("." decimal separator)
            Thread.CurrentThread.CurrentCulture = New CultureInfo("de-DE")
            Dim numObj = New JsonObject
            numObj.Add("num", 1234.5)
            numObj.Add("neg", -0.0001)
            Dim numJson = numObj.BuildJsonString(indent:=False)
            If Not numJson.Contains("1234.5") Then failures.Add("number not invariant formatted: " & numJson)
            If numJson.Contains("1234,5") Then failures.Add("number used culture decimal separator: " & numJson)

            ' 2. NaN / +Infinity / -Infinity must be emitted as json string literals
            Dim spObj = New JsonObject
            spObj.Add("nan", Double.NaN)
            spObj.Add("pos", Double.PositiveInfinity)
            spObj.Add("neg", Double.NegativeInfinity)
            Dim spJson = spObj.BuildJsonString(indent:=False)
            If Not spJson.Contains("""NaN""") Then failures.Add("NaN not serialized as literal: " & spJson)
            If Not spJson.Contains("""Infinity""") Then failures.Add("+Infinity not serialized: " & spJson)
            If Not spJson.Contains("""-Infinity""") Then failures.Add("-Infinity not serialized: " & spJson)

            ' 3. non-unix date -> quoted ISO-8601 string
            Dim dateObj = New JsonObject
            dateObj.Add("d", New Date(2026, 7, 30, 12, 34, 56))
            Dim optsDate = New JSONSerializerOptions With {.unixTimestamp = False}
            Dim dateJson = dateObj.BuildJsonString(optsDate)
            If Not dateJson.Contains("""2026-07-30T12:34:56") Then failures.Add("date not quoted ISO string: " & dateJson)
            If JsonParser.Parse(dateJson) Is Nothing Then failures.Add("date json failed to parse: " & dateJson)

            ' 4. control chars must be escaped (never raw) in the non-unicode path
            Dim ctrlObj = New JsonObject
            ctrlObj.Add("s", "a" & vbCrLf & "b" & vbTab & "c" & vbCr & "d")
            Dim optsCtrl = New JSONSerializerOptions With {.unicodeEscape = False}
            Dim ctrlJson = ctrlObj.BuildJsonString(optsCtrl)
            If ctrlJson.Contains(vbCr) OrElse ctrlJson.Contains(vbLf) OrElse ctrlJson.Contains(vbTab) Then
                failures.Add("raw control characters leaked into non-unicode output: " & ctrlJson)
            End If
            If JsonParser.Parse(ctrlJson) Is Nothing Then failures.Add("ctrl json failed to parse: " & ctrlJson)

            ' 5. control chars must be escaped in the unicode path as well
            Dim optsUni = New JSONSerializerOptions With {.unicodeEscape = True}
            Dim uniJson = ctrlObj.BuildJsonString(optsUni)
            If JsonParser.Parse(uniJson) Is Nothing Then failures.Add("unicode json failed to parse: " & uniJson)

            ' 6. non-ASCII is escaped only in the unicode path
            Dim uniStrObj = New JsonObject
            uniStrObj.Add("s", "caf" & ChrW(&H00E9) & "测试")
            Dim uniEscJson = uniStrObj.BuildJsonString(optsUni)
            If uniEscJson.Contains("测试") OrElse uniEscJson.Contains("caf" & ChrW(&H00E9)) Then
                failures.Add("non-ASCII not escaped in unicode path: " & uniEscJson)
            End If
            If JsonParser.Parse(uniEscJson) Is Nothing Then failures.Add("unicode-escaped json failed to parse: " & uniEscJson)

            Dim rawEscJson = uniStrObj.BuildJsonString(optsCtrl)
            If JsonParser.Parse(rawEscJson) Is Nothing Then failures.Add("raw non-ASCII json failed to parse: " & rawEscJson)

            ' 7. indented array closing must not emit a blank line before ]
            Dim arrObj = New JsonObject
            Dim objArr As New JsonArray
            objArr.Add(New JsonObject From {{"x", 1}})
            objArr.Add(New JsonObject From {{"y", 2}})
            arrObj.Add("items", objArr)
            arrObj.Add("names", New JsonArray({"a", "b", "c"}))
            Dim indentJson = arrObj.BuildJsonString(indent:=True)
            If indentJson.Contains(Environment.NewLine & Environment.NewLine) Then
                failures.Add("blank line detected before closing bracket:" & vbCrLf & indentJson)
            End If
            If JsonParser.Parse(indentJson) Is Nothing Then failures.Add("indented json failed to parse: " & indentJson)

        ' 8. round-tripped json: an array that mixes objects and scalar values
        '    becomes an Object-typed array; its scalar elements must not produce
        '    leading commas or a blank line before the closing bracket.
        Dim rt = JsonParser.Parse("[1, {""x"": 1}, 2, 3]")
        Dim rtJson = rt.BuildJsonString(indent:=True)
        If rtJson.Contains(Environment.NewLine & Environment.NewLine) Then
            failures.Add("blank line before closing bracket in round-tripped array:" & vbCrLf & rtJson)
        End If
        If rtJson.Contains(Environment.NewLine & ",") Then
            failures.Add("leading comma in round-tripped indented array:" & vbCrLf & rtJson)
        End If
        Dim rtParsed = JsonParser.Parse(rtJson)
        If rtParsed Is Nothing Then
            failures.Add("round-tripped array failed to parse: " & rtJson)
        ElseIf DirectCast(rtParsed, JsonArray).Length <> 4 Then
            failures.Add("round-tripped array lost elements: " & rtJson)
        End If

        ' 9. non-numeric clr types (char, guid, timespan, enum) must be quoted strings
        Dim miscObj = New JsonObject
        miscObj.Add("c", ChrW(65))
        miscObj.Add("g", System.Guid.NewGuid())
        miscObj.Add("t", System.TimeSpan.FromMinutes(5))
        miscObj.Add("e", System.DayOfWeek.Monday)
        Dim miscJson = miscObj.BuildJsonString(indent:=False)
        If JsonParser.Parse(miscJson) Is Nothing Then
            failures.Add("non-numeric clr types produced invalid json: " & miscJson)
        End If
        If miscJson.Contains("Monday") AndAlso Not miscJson.Contains("""Monday""") Then
            failures.Add("enum serialized as bare token (not quoted): " & miscJson)
        End If

        ' 10. the clr string "null" must stay a json string, not become the null keyword
        Dim nullObj = New JsonObject
        nullObj.Add("n", "null")
        Dim nullJson = nullObj.BuildJsonString(indent:=False)
        If Not nullJson.Contains("""null""") Then
            failures.Add("clr string ""null"" not quoted: " & nullJson)
        End If
        If nullJson.Contains(": null") OrElse nullJson.Contains(":null") Then
            failures.Add("clr string ""null"" became the json null keyword: " & nullJson)
        End If
        Dim reparsedNull = DirectCast(JsonParser.Parse(nullJson), JsonObject)
        Dim nv = DirectCast(reparsedNull("n"), JsonValue)
        If nv.value Is Nothing Then
            failures.Add("clr string ""null"" parsed back as json null: " & nullJson)
        ElseIf Not CStr(nv.value) = "null" Then
            failures.Add("clr string ""null"" round-trip lost its value: " & nullJson)
        End If

        Catch ex As Exception
            failures.Add("exception: " & ex.Message & vbCrLf & ex.StackTrace)
        Finally
            Thread.CurrentThread.CurrentCulture = originalCulture
        End Try

        If failures.Count = 0 Then
            Console.WriteLine("ALL JSONWriter REVIEW TESTS PASSED")
        Else
            Console.WriteLine("JSONWriter REVIEW TESTS FAILED (" & failures.Count & "):")
            For Each f In failures
                Console.WriteLine(" - " & f)
            Next
        End If
    End Sub

End Module
#End Region
