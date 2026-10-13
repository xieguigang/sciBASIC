Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Xunit

''' <summary>
''' Keeps <c>test/artifacts/GeneratedScript.vb</c> — the checked-in translation of
''' <c>test/artifacts/demo.js</c> — byte-identical with the current
''' <see cref="CodeGenerator"/> output.
''' <para>
''' The artifact is compiled into the test assembly (SDK globbing), so it also
''' acts as a build-time COMPILE check for generated code: a code generator change
''' that emits uncompilable VB breaks the build, not just a test.
''' </para>
''' <para>
''' Regenerate after intentional code-generator changes:
''' <c>set LITEJS_REGEN_ARTIFACTS=1</c> then run this test.
''' </para>
''' </summary>
Public Class ArtifactSyncTests

    ''' <summary>Feature demonstration script that the artifact is generated from.</summary>
    Public Const DemoFile As String = "demo.js"
    Public Const ArtifactFile As String = "GeneratedScript.vb"
    Public Const ArtifactClassName As String = "GeneratedScript"

    <Fact>
    Public Sub ArtifactMatchesGeneratorOutput()
        Dim dir = ArtifactDirectory()
        Dim source = File.ReadAllText(System.IO.Path.Combine(dir, DemoFile))
        Dim generated = New CodeGenerator().Generate(Parser.Parse(source), ArtifactClassName)
        Dim target = System.IO.Path.Combine(dir, ArtifactFile)

        If Environment.GetEnvironmentVariable("LITEJS_REGEN_ARTIFACTS") = "1" Then
            File.WriteAllText(target, generated)
            Return
        End If

        Dim committed = File.ReadAllText(target)
        Assert.True(committed = generated,
                    $"artifacts/{ArtifactFile} is stale — regenerate it with LITEJS_REGEN_ARTIFACTS=1 " &
                    "after code-generator changes")
    End Sub

    ''' <summary>The demo script must also run cleanly on the interpreter.</summary>
    <Fact>
    Public Sub DemoScriptRunsOnInterpreter()
        Dim source = File.ReadAllText(System.IO.Path.Combine(ArtifactDirectory(), DemoFile))
        Dim io As ScriptIO = New LogTextIO
        Dim engine As New Interpreter(io)
        engine.Run(Parser.Parse(source))
        Dim lines = DirectCast(io, LogTextIO).Lines
        Assert.True(lines.Count > 0, "the demo script produced no output")
    End Sub

    Private Shared Function ArtifactDirectory() As String
        ' walk up from the test binaries to the project source folder
        Dim dir = New DirectoryInfo(AppContext.BaseDirectory)
        While dir IsNot Nothing AndAlso Not Directory.Exists(System.IO.Path.Combine(dir.FullName, "artifacts"))
            dir = dir.Parent
        End While
        Assert.True(dir IsNot Nothing, "artifacts folder not found")
        Return System.IO.Path.Combine(dir.FullName, "artifacts")
    End Function

End Class
