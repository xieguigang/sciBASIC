Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' Minimal host that runs a JavaScript file with the LiteJs engine, exposing a Node.js-like
''' environment (process.cwd / __dirname / __filename / process.argv). Designed so its stdout
''' can be diffed against <c>node &lt;script.js&gt;</c> output for fidelity checks.
''' <para>
''' Output backends:
''' <list type="bullet">
''' <item><c>--ansi</c> (default) — <see cref="ConsoleIO"/>: <c>%c</c> CSS styles are
''' rendered as ANSI escape sequences and log levels are colourised.</item>
''' <item><c>--plain</c> — <see cref="LogTextIO"/>: collect the output and echo it
''' plainly to stdout, byte for byte comparable with Node.</item>
''' <item><c>--html &lt;file.html&gt;</c> — <see cref="HtmlIO"/>: render a styled
''' standalone HTML terminal page, saved when the script finishes.</item>
''' </list>
''' </para>
''' </summary>
Module Program

    Sub Main()
        Dim raw = System.Environment.GetCommandLineArgs().ToList()
        Dim backend As String = "ansi"
        Dim htmlPath As String = Nothing
        Dim scriptArg As String = Nothing
        Dim userArgs As New List(Of String)()

        Dim i As Integer = 1

        While i < raw.Count
            Dim arg = raw(i)

            Select Case arg
                Case "--ansi"
                    backend = "ansi"
                    i += 1
                    Continue While
                Case "--plain"
                    backend = "plain"
                    i += 1
                    Continue While
                Case "--html"
                    If i + 1 >= raw.Count Then
                        Usage("--html requires an output file path")
                        System.Environment.Exit(1)
                        Return
                    End If

                    backend = "html"
                    htmlPath = raw(i + 1)
                    i += 2
                    Continue While
                Case "-h", "--help", "/?"
                    Usage(Nothing)
                    Return
            End Select

            If scriptArg Is Nothing AndAlso arg.EndsWith(".js", System.StringComparison.OrdinalIgnoreCase) Then
                scriptArg = arg
            Else
                userArgs.Add(arg)
            End If

            i += 1
        End While

        If scriptArg Is Nothing Then
            Usage(Nothing)
            System.Environment.Exit(1)
            Return
        End If

        Dim scriptPath = Path.GetFullPath(scriptArg)

        Dim argv As New System.Collections.Generic.List(Of Object)()
        argv.Add("node")                 ' execPath  (argv[0] in Node)
        argv.Add(scriptPath)             ' script path (argv[1] in Node)
        For Each a In userArgs : argv.Add(a) : Next

        Dim cwd = System.Environment.CurrentDirectory
        Dim scriptDir = Path.GetDirectoryName(scriptPath)
        ProcessModule.SetContext(cwd, scriptDir, scriptPath, argv)

        Dim plain As LogTextIO = Nothing
        Dim html As HtmlIO = Nothing
        Dim io As ScriptIO

        Select Case backend
            Case "plain"
                plain = New LogTextIO()
                io = plain
            Case "html"
                html = New HtmlIO(Path.GetFileName(scriptPath))
                html.Meta = scriptPath
                io = html
            Case Else
                io = New ConsoleIO()
        End Select

        Dim engine As New Interpreter(io)
        Dim failed As Boolean = False

        ' stack frames report the script path, like Node does
        engine.SourceFile = scriptPath

        Try
            engine.Run(Parser.Parse(File.ReadAllText(scriptPath)))
        Catch ex As JsRuntimeException
            ' Node style uncaught report: "TypeError: msg" plus the JS frames
            System.Console.Error.WriteLine("Uncaught " & ex.Stack)
            failed = True
        Catch ex As ParseException
            ' a syntax error carries the failing token position: render it as a JS frame
            System.Console.Error.WriteLine("Uncaught " & JsStackTrace.FormatWithHeader(
                "SyntaxError", ex.Message,
                JsStackTrace.SingleFrame(JsStackFrame.AnonymousFunction, scriptPath, ex.Line, ex.Column)))
            failed = True
        Finally
            ' flush the buffering backends even when the script failed, so the partial
            ' output stays observable
            If plain IsNot Nothing Then
                For Each line In plain.Lines
                    System.Console.WriteLine(line)
                Next
            End If

            If html IsNot Nothing Then
                html.SaveHtml(htmlPath)
                System.Console.Error.WriteLine("HtmlIO: wrote " & Path.GetFullPath(htmlPath))
            End If
        End Try

        If failed Then System.Environment.Exit(1)
    End Sub

    Private Sub Usage(Optional [error] As String = Nothing)
        If Not String.IsNullOrEmpty([error]) Then
            System.Console.Error.WriteLine("error: " & [error])
        End If

        System.Console.Error.WriteLine("Usage: LiteJs.Runner <script.js> [args...] [--ansi|--plain|--html <out.html>]")
        System.Console.Error.WriteLine("  --ansi         render %c CSS styles and log levels as ANSI colours (default)")
        System.Console.Error.WriteLine("  --plain        write the collected plain text output to stdout")
        System.Console.Error.WriteLine("  --html <file>  render a styled standalone HTML terminal page")
    End Sub

End Module
