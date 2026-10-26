Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' The script introspection magics of the JavaScript engine — the counterpart of the VBS
''' <c>Magics.Build</c> snippets: <c>here(relpath)</c>, <c>scriptDir()</c>,
''' <c>scriptFile()</c>, <c>scriptName(withExtension?)</c>, <c>scriptText()</c> and
''' <c>scriptLines()</c>.
''' <para>
''' <c>here()</c> anchors relative paths at the folder of the running script, exactly like
''' the VBS <c>Here(relpath)</c> magic function; the anchor is published through
''' <see cref="Interpreter.SourceFile"/>, so the tests point the engine at a throw-away js
''' file on disk.
''' </para>
''' </summary>
Public Class LiteJsMagicTests

    ' ======================================================== helpers

    ''' <summary>Write a throw-away script + optional data files; the caller deletes the folder.</summary>
    Private Shared Function CreateScript(source As String,
                                         ParamArray files As (Name As String, Text As String)()) As String
        Dim dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                         "litejs-magic-" & Guid.NewGuid().ToString("N"))
        System.IO.Directory.CreateDirectory(dir)

        For Each f In files
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, f.Name), f.Text)
        Next

        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "main.js"), source)

        Return dir
    End Function

    Private Shared Sub Remove(dir As String)
        Try
            System.IO.Directory.Delete(dir, True)
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>Run a script that lives in a temp folder (the magic functions need a file).</summary>
    Private Shared Function RunIn(dir As String) As List(Of String)
        Dim io As New LogTextIO
        Dim engine As New Interpreter(io)
        engine.SourceFile = System.IO.Path.Combine(dir, "main.js")
        engine.Run(Parser.Parse(System.IO.File.ReadAllText(engine.SourceFile)))
        Return io.Lines
    End Function

    Private Shared Function OneLineIn(dir As String, source As String) As String
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "main.js"), source)
        Return RunIn(dir)(0)
    End Function

    Private Shared Function LinesIn(dir As String, source As String) As List(Of String)
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "main.js"), source)
        Return RunIn(dir)
    End Function

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    Private Shared Function FullPath(dir As String, name As String) As String
        Return System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, name))
    End Function

    ''' <summary>A JavaScript string literal holding a Windows path (backslashes escaped).</summary>
    Private Shared Function Quote(text As String) As String
        Return """" & text.Replace("\", "\\") & """"
    End Function

    ' ======================================================== here()

    <Fact>
    Public Sub Here_AnchorsRelativePathsAtTheScriptFolder()
        Dim dir = CreateScript("", ("data.txt", "hello"))

        Try
            Assert.Equal(FullPath(dir, "data.txt"),
                         OneLineIn(dir, "console.log(here('data.txt'));"))
        Finally
            Remove(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Here_EmptyArgumentIsTheScriptFolder()
        Dim dir = CreateScript("")

        Try
            Assert.Equal(System.IO.Path.GetFullPath(dir),
                         OneLineIn(dir, "console.log(here(''));"))
            ' the same value as scriptDir()
            Assert.Equal(OneLineIn(dir, "console.log(scriptDir());"),
                         OneLineIn(dir, "console.log(here(''));"))
        Finally
            Remove(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Here_AbsolutePathIsNormalisedButNotRebased()
        Dim dir = CreateScript("")
        Dim absolute = System.IO.Path.Combine(dir, "sub", "..", "data.txt")

        Try
            Assert.Equal(FullPath(dir, "data.txt"),
                         OneLineIn(dir, $"console.log(here({Quote(absolute)}));"))
        Finally
            Remove(dir)
        End Try
    End Sub

    <Fact>
    Public Sub Here_WorksFromInsideAFunction()
        Dim dir = CreateScript("")

        Try
            ' the magic is a normal global: it must resolve inside nested scopes too
            Assert.Equal(FullPath(dir, "figure.png"), OneLineIn(dir,
                "function save(name) { return here(name); }" &
                "console.log(save('figure.png'));"))
        Finally
            Remove(dir)
        End Try
    End Sub

    ' ======================================================== script introspection

    <Fact>
    Public Sub ScriptDir_ScriptFile_ScriptName()
        Dim dir = CreateScript("")

        Try
            Assert.Equal(System.IO.Path.GetFullPath(dir),
                         OneLineIn(dir, "console.log(scriptDir());"))
            Assert.Equal(FullPath(dir, "main.js"),
                         OneLineIn(dir, "console.log(scriptFile());"))
            Assert.Equal("main.js",
                         OneLineIn(dir, "console.log(scriptName());"))
            Assert.Equal("main",
                         OneLineIn(dir, "console.log(scriptName(false));"))
        Finally
            Remove(dir)
        End Try
    End Sub

    <Fact>
    Public Sub ScriptText_And_ScriptLines_ReadTheRunningScript()
        Dim dir = CreateScript("")

        Try
            ' the script is written back to main.js before it runs, so both read that text
            Dim source = "console.log(scriptLines().length);" & vbCrLf &
                         "console.log(scriptText().indexOf('scriptLines') >= 0);"
            Dim lines = LinesIn(dir, source)

            Assert.Equal("2", lines(0))
            Assert.Equal("true", lines(1))
        Finally
            Remove(dir)
        End Try
    End Sub

    <Fact>
    Public Sub ScriptText_WithoutASourceFile_IsCatchable()
        ' no SourceFile: the engine has no script on disk, so the read must fail catchably
        Dim io As New LogTextIO
        Dim engine As New Interpreter(io)

        engine.Run(Parser.Parse(
            "try { scriptText(); console.log('no-error'); }" &
            " catch (e) { console.log('caught'); }"))

        Assert.Equal("caught", io.Lines(0))
    End Sub

    <Fact>
    Public Sub ScriptDir_WithoutASourceFile_DoesNotThrow()
        ' an in-memory script still answers with a stable (possibly empty) folder
        Dim io As New LogTextIO
        Dim engine As New Interpreter(io)

        engine.Run(Parser.Parse("console.log(typeof scriptDir(), typeof here(''));"))

        Assert.Equal("string string", io.Lines(0))
    End Sub

    ' ======================================================== magic table

    <Fact>
    Public Sub ScriptMagic_NamesAreStable()
        ' the public surface is what the code generator and the documentation rely on
        Dim expected As String() = {"here", "scriptDir", "scriptFile", "scriptName", "scriptText", "scriptLines"}

        Assert.Equal(String.Join(",", expected), String.Join(",", ScriptMagic.Names))
        Assert.True(ScriptMagic.IsMagic("here"))
        Assert.False(ScriptMagic.IsMagic("here2"))
    End Sub

    ' ======================================================== generated code

    <Fact>
    Public Sub CodeGen_ResolvesTheMagicsThroughTheGlobalObject()
        Dim code = Generate(
            "console.log(here('a.txt'), scriptDir(), scriptFile(), scriptName(), scriptText(), scriptLines());")

        For Each name As String In ScriptMagic.Names
            Assert.Contains($"JsRuntime.JsGet(JsRuntime.GlobalObj, ""{name}"")", code)
        Next
    End Sub

    <Fact>
    Public Sub CodeGen_DoesNotEmitAMagicAsAHostGlobal()
        ' a magic must never degrade to ResolveHostGlobal: that would 404 at run time
        Dim code = Generate("console.log(here('a.txt'));")

        Assert.DoesNotContain("ResolveHostGlobal(""here"")", code)
    End Sub

End Class
