#Region "Microsoft.VisualBasic::node_api_codegen, vs_solutions\JavaScript\test\NodeApiCodegenTests.vb"

Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Guard that the Node.js-compatible modules (fs / Buffer / path / process) are reachable
''' from BOTH execution paths of the engine:
''' <list type="bullet">
'''   <item>the CodeGenerator must emit references to the shared <c>JsRuntime.*Builtin</c>
'''   instances (which <c>GlobalIdentifiers()</c> now (re)builds in the generated Run);</item>
'''   <item>the interpreter must actually perform real file I/O through them.</item>
''' </list>
''' This backs the plan's requirement that generated code and the interpreter agree for the
''' new APIs.
''' </summary>
Public Class NodeApiCodegenTests

    <Fact>
    Public Sub CodegenWiresUpNodeModules()
        Dim src = "var p = path.join('a', 'b');" &
                  "fs.writeFileSync(path.join('d', 'f.txt'), 'hi');" &
                  "var t = fs.readFileSync(path.join('d', 'f.txt'), 'utf8');"
        Dim generated = New CodeGenerator().Generate(Parser.Parse(src), "GenNodeApi")
        Assert.Contains("JsRuntime.GlobalIdentifiers()", generated)
        Assert.Contains("JsRuntime.PathBuiltin", generated)
        Assert.Contains("JsRuntime.FsBuiltin", generated)
    End Sub

    <Fact>
    Public Sub InterpreterRunsFsAndPath()
        Dim dir = System.IO.Path.GetTempPath()
        ' path.join on an absolute Windows dir + fs read/write/unlink on a real temp file.
        Dim src = "var f = path.join(" & Quote(dir) & ", 'litejs_codegen_test.txt');" &
                  "fs.writeFileSync(f, 'hello');" &
                  "console.log('read:', fs.readFileSync(f, 'utf8'));" &
                  "console.log('exists:', fs.existsSync(f));" &
                  "fs.unlinkSync(f);" &
                  "console.log('removed:', !fs.existsSync(f));"
        Dim io As ScriptIO = New LogTextIO()
        Dim engine As New Interpreter(io)
        engine.Run(Parser.Parse(src))
        Dim lines = DirectCast(io, LogTextIO).Lines
        Assert.True(lines.Any(Function(l) l.Contains("read: hello")), "expected read output")
        Assert.True(lines.Any(Function(l) l.Contains("exists: true")), "expected exists true")
        Assert.True(lines.Any(Function(l) l.Contains("removed: true")), "expected removed true")
    End Sub

    ' Build a single-quoted JS string literal, escaping backslashes for Windows paths.
    Private Shared Function Quote(s As String) As String
        Return "'" & s.Replace("\", "\\") & "'"
    End Function

End Class

#End Region
