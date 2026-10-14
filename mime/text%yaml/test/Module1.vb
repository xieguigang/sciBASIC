#Region "Microsoft.VisualBasic::5a4d1f6d0cdb1f1307a812a683d643ae, mime\text%yaml\test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigigang (xie.guigang@live.com)
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
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not see <http://www.gnu.org/licenses/>.

    ' /********************************************************************************/

    ' Module Module1
    ' 
    '     Sub: Main, TestYamlParserOnNatureSkills, TestStructuredParsing,
    '          TestYamlFrontMatterBlockScalar, TestYamlParserBlockScalar, Assert
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.text.yaml

Module Module1

    Private failCount As Integer = 0

    ''' <summary>
    ''' Convert a scalar JsonElement (JsonValue) to its string representation.
    ''' </summary>
    Private Function AsString(e As JsonElement) As String
        If e Is Nothing Then Return Nothing
        Return CType(DirectCast(e, JsonValue), String)
    End Function


    Sub Main()
        Console.WriteLine("=== YAML parser self-test ===")

        Call TestYamlFrontMatterBlockScalar()
        Call TestYamlParserBlockScalar()
        Call TestYamlParserOnNatureSkills()
        Call TestStructuredParsing()

        If failCount > 0 Then
            Console.WriteLine($"SELF-TEST FAILED: {failCount} assertion(s) failed.")
            End
        Else
            Console.WriteLine("SELF-TEST PASSED: all assertions succeeded.")
        End If

        Pause()
    End Sub

    ''' <summary>
    ''' Lightweight assertion: throws on failure so the demo exits non-zero
    ''' when an expectation is not met.
    ''' </summary>
    Private Sub Assert(condition As Boolean, msg As String)
        If condition Then
            Console.WriteLine("  ok  - " & msg)
        Else
            failCount += 1
            Console.WriteLine("  FAIL- " & msg)
            Throw New Exception("Assertion failed: " & msg)
        End If
    End Sub

    ' ----------------------------------------------------------------------
    ' YamlFrontMatterParser block scalar support
    ' ----------------------------------------------------------------------
    Private Sub TestYamlFrontMatterBlockScalar()
        Console.WriteLine(vbCrLf & "[YamlFrontMatterParser] block scalar support")

        ' Folded, strip chomping: description: >-  must NOT become the literal ">-"
        Dim md As String = "---" & vbCrLf &
            "name: pdf_extract_text" & vbCrLf &
            "description: >-" & vbCrLf &
            "  Extract text content from PDF documents" & vbCrLf &
            "  and convert it to markdown." & vbCrLf &
            "tags:" & vbCrLf &
            "  - pdf" & vbCrLf &
            "  - text-extraction" & vbCrLf &
            "---" & vbCrLf

        Dim fm = YamlFrontMatterParser.Parse(md)
        Assert(fm.ContainsKey("name"), "front matter exposes name")
        Assert(fm("name") = "pdf_extract_text", "name value parsed")
        ' The bug: description used to be the literal string ">-"
        Assert(fm("description") <> ">-", "description is not the literal '>-'")
        Assert(fm("description").Contains("Extract text content from PDF documents"),
               "folded description keeps first line")
        Assert(fm("description").Contains("convert it to markdown"),
               "folded description keeps second line")
        Assert(fm("description").Contains(" "),
               "folded description joins lines with a space")
        ' strip chomping ('-') => no trailing newline
        Assert(Not fm("description").EndsWith(ControlChars.Lf),
               "strip chomping '-' drops the trailing newline")

        ' Literal block scalar: description: |  keeps newlines
        Dim md2 As String = "---" & vbCrLf &
            "name: literal_demo" & vbCrLf &
            "body: |" & vbCrLf &
            "  line one" & vbCrLf &
            "  line two" & vbCrLf &
            "---" & vbCrLf
        Dim fm2 = YamlFrontMatterParser.Parse(md2)
        Assert(fm2("body").Contains("line one" & ControlChars.Lf & "line two"),
               "literal block scalar preserves newlines")

        ' Standalone .yaml (no '---' delimiters) should still parse, including
        ' the folded description used by the nature-skills manifest files.
        Dim yaml As String = "name: nature-writing" & vbCrLf &
            "version: 1.5.0" & vbCrLf &
            "description: >" & vbCrLf &
            "  Declarative manifest for the static/dynamic split. SKILL.md" & vbCrLf &
            "  uses this to decide which fragments to load." & vbCrLf &
            vbCrLf &
            "always_load:" & vbCrLf &
            "  - a.md" & vbCrLf &
            "  - b.md" & vbCrLf
        Dim fm3 = YamlFrontMatterParser.Parse(yaml)
        Assert(fm3("name") = "nature-writing", "standalone yaml: name")
        Assert(fm3("description").Contains("static/dynamic split"),
               "standalone yaml: folded description")
        Assert(fm3("always_load") = "a.md, b.md", "standalone yaml: bulleted list")
    End Sub

    ' ----------------------------------------------------------------------
    ' YamlParser block scalar support
    ' ----------------------------------------------------------------------
    Private Sub TestYamlParserBlockScalar()
        Console.WriteLine(vbCrLf & "[YamlParser] block scalar support")

        Dim doc As String =
            "name: demo" & vbCrLf &
            "summary: >" & vbCrLf &
            "  This is a folded" & vbCrLf &
            "  multi-line summary." & vbCrLf &
            vbCrLf &
            "notes: |" & vbCrLf &
            "  keep my" & vbCrLf &
            "  line breaks" & vbCrLf &
            "items:" & vbCrLf &
            "  - one" & vbCrLf &
            "  - two" & vbCrLf

        Dim root = DirectCast(YamlParser.Parse(doc), JsonObject)
        Dim summary = AsString(root("summary"))
        Assert(summary.Contains("This is a folded multi-line summary."),
               "YamlParser folds '>' into a single spaced line")
        Assert(Not summary.Contains(ControlChars.Lf),
               "YamlParser folded scalar has no embedded newline")

        Dim notes = AsString(root("notes"))
        Assert(notes.Contains("keep my" & ControlChars.Lf & "line breaks"),
               "YamlParser literal '|' keeps newlines")

        Dim items = DirectCast(root("items"), JsonArray)
        Assert(items.Length = 2, "YamlParser parses the item list")
        Assert(AsString(items(0)) = "one", "YamlParser first item")
    End Sub

    ' ----------------------------------------------------------------------
    ' Parse every *.yaml under the nature-skills folder
    ' ----------------------------------------------------------------------
    Private Sub TestYamlParserOnNatureSkills()
        Console.WriteLine(vbCrLf & "[YamlParser] parse all nature-skills yaml files")

        Dim root = "C:\Users\Administrator\Downloads\nature-skills"
        If Not Directory.Exists(root) Then
            Console.WriteLine("  SKIP: folder not found: " & root)
            Return
        End If

        Dim files = Directory.GetFiles(root, "*.yaml", SearchOption.AllDirectories)
        Assert(files.Length > 0, $"found {files.Length} yaml files under nature-skills")

        Dim parsed As Integer = 0
        For Each f In files
            Try
                Dim doc = YamlParser.ParseFile(f)
                Assert(TypeOf doc Is JsonObject, $"document is a JsonObject: {f}")
                Dim obj = DirectCast(doc, JsonObject)

                ' description must never be left as the literal block indicator
                Dim desc = obj.GetString("description")
                If desc IsNot Nothing Then
                    Assert(desc <> ">-" AndAlso desc <> ">" AndAlso desc <> "|" AndAlso desc <> "|-",
                           $"description not a literal block indicator: {Path.GetFileName(f)}")
                    Assert(desc.Length > 0, $"description non-empty: {Path.GetFileName(f)}")
                End If

                ' every skill manifest exposes a name
                If Path.GetFileName(f) = "manifest.yaml" Then
                    Assert(obj.HasObjectKey("name"), $"manifest has name: {f}")
                End If

                parsed += 1
                Console.WriteLine($"  parsed {files.Length} file(s); last ok: {Path.GetFileName(f)}")
            Catch ex As Exception
                failCount += 1
                Console.WriteLine($"  FAIL {f}: {ex.Message}")
            End Try
        Next

        Assert(parsed = files.Length, $"all {files.Length} files parsed without error")
    End Sub

    ' ----------------------------------------------------------------------
    ' Deep structural checks on a representative manifest
    ' ----------------------------------------------------------------------
    Private Sub TestStructuredParsing()
        Console.WriteLine(vbCrLf & "[YamlParser] structural parsing of nature-writing/manifest.yaml")

        Dim path = "C:\Users\Administrator\Downloads\nature-skills\skills\nature-writing\manifest.yaml"
        If Not File.Exists(path) Then
            Console.WriteLine("  SKIP: " & path)
            Return
        End If

        Dim root = DirectCast(YamlParser.ParseFile(path), JsonObject)
        Assert(AsString(root("name")) = "nature-writing", "top-level name")
        Assert(AsString(root("version")) = "1.5.0", "top-level version")

        ' Folded description must contain the full multi-line text, not just the first line.
        Dim desc = AsString(root("description"))
        Assert(desc.Contains("Declarative manifest for the static/dynamic split"),
               "description keeps the full folded text (first line)")
        Assert(desc.Contains("decide which fragments to load"),
               "description keeps the full folded text (second line)")

        ' always_load is a list of 7 entries
        Dim alwaysLoad = DirectCast(root("always_load"), JsonArray)
        Assert(alwaysLoad.Length = 7, $"always_load has 7 entries (got {alwaysLoad.Length})")

        ' axes -> task -> detect is a multi-line literal block scalar
        Dim axes = DirectCast(root("axes"), JsonObject)
        Dim task = DirectCast(axes("task"), JsonObject)
        Dim detect = AsString(task("detect"))
        Assert(detect.Contains("Use submission-package when the user requests"),
               "axes.task.detect literal block scalar keeps text")
        Assert(detect.Contains("belongs to nature-response, not this skill"),
               "axes.task.detect literal block scalar keeps later lines")

        ' axes.task.values is a nested mapping
        Dim values = DirectCast(task("values"), JsonObject)
        Assert(values.HasObjectKey("manuscript"), "axes.task.values.manuscript exists")
        Assert(AsString(values("manuscript")).EndsWith("task/manuscript.md"),
               "axes.task.values.manuscript maps to a fragment path")

        ' references.on_demand is a list of mapping items
        Dim references = DirectCast(root("references"), JsonObject)
        Dim onDemand = DirectCast(references("on_demand"), JsonArray)
        Assert(onDemand.Length > 10, $"references.on_demand has many entries (got {onDemand.Length})")
        Dim first = DirectCast(DirectCast(onDemand(0), JsonObject), JsonObject)
        Assert(first.HasObjectKey("condition"), "first on_demand item has condition")
        Assert(first.HasObjectKey("path"), "first on_demand item has path")
    End Sub

End Module
