Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
' the AST root node shares its name with this entry point module
Imports JsProgram = Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Program

''' <summary>
''' Node-like host for the LiteJs engine.
''' <para>
''' <c>njs script.js [args...]</c> runs a file in a Node style environment
''' (process.cwd / __dirname / __filename / process.argv plus the fs / path / process /
''' Buffer modules), designed so its stdout can be diffed against
''' <c>node &lt;script.js&gt;</c>.
''' </para>
''' <para>
''' <c>njs</c> without a script starts an interactive REPL: every input is evaluated in a
''' persistent global scope and the value of a trailing expression is echoed.
''' </para>
''' <para>
''' Output backends (script mode): <c>--ansi</c> (default) — <see cref="ConsoleIO"/> with
''' <c>%c</c> CSS styles rendered as ANSI escape sequences; <c>--plain</c> —
''' <see cref="LogTextIO"/>, buffered and echoed plainly for byte level comparison;
''' <c>--html &lt;file.html&gt;</c> — <see cref="HtmlIO"/>, a styled standalone page.
''' </para>
''' </summary>
Module Program

    ' ===================== interactive session =====================

    ''' <summary>Prompt shown when a new input starts.</summary>
    Private Const PrimaryPrompt As String = "> "

    ''' <summary>Prompt shown while an input is continued on the next line.</summary>
    Private Const ContinuePrompt As String = "... "

    ''' <summary>Source name reported by JS stack frames while no script file is running.</summary>
    Private Const ReplSource As String = "[repl]"

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

        ' no script and no script arguments → interactive session, like `node` without a file
        If scriptArg Is Nothing AndAlso userArgs.Count = 0 Then
            If backend = "html" Then
                Usage("--html requires a script file")
                System.Environment.Exit(1)
                Return
            End If

            Call Repl(backend)
            Return
        End If

        If scriptArg Is Nothing Then
            Usage(Nothing)
            System.Environment.Exit(1)
            Return
        End If

        Call RunScript(Path.GetFullPath(scriptArg), userArgs, backend, htmlPath)
    End Sub

    ' ===================== script mode =====================

    ''' <summary>Run one script file with the selected output backend (see the module docs).</summary>
    Private Sub RunScript(scriptPath As String, userArgs As List(Of String), backend As String, htmlPath As String)
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
        Catch ex As Exception
            ReportUncaught(ex, scriptPath)
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

    ''' <summary>
    ''' Report a failed script / REPL input the way node does: the error message followed by
    ''' the JS frames (a syntax error carries the position of the offending token).
    ''' </summary>
    Private Sub ReportUncaught(ex As Exception, sourceFile As String)
        Dim runtimeError = TryCast(ex, JsRuntimeException)

        If runtimeError IsNot Nothing Then
            System.Console.Error.WriteLine("Uncaught " & runtimeError.Stack)
            Return
        End If

        Dim syntaxError = TryCast(ex, ParseException)

        If syntaxError IsNot Nothing Then
            System.Console.Error.WriteLine("Uncaught " & JsStackTrace.FormatWithHeader(
                "SyntaxError", syntaxError.Message,
                JsStackTrace.SingleFrame(JsStackFrame.AnonymousFunction, sourceFile, syntaxError.Line, syntaxError.Column)))
            Return
        End If

        System.Console.Error.WriteLine("Uncaught " & ex.Message)
    End Sub

    ' ===================== REPL mode =====================

    ''' <summary>
    ''' Interactive JavaScript session: an input is accumulated until it parses (so multi line
    ''' declarations work), evaluated in a persistent global scope — variables, functions and
    ''' classes survive from one input to the next — and its completion value is echoed,
    ''' <c>undefined</c> included, exactly like the node REPL.
    ''' <para>
    ''' <c>.help</c> lists the dot commands; <c>.exit</c>, Ctrl+D and Ctrl+C leave the session.
    ''' When stdin is a pipe there is no banner and no prompt, so a session can be scripted
    ''' (and diffed) just like a script file.
    ''' </para>
    ''' </summary>
    Private Sub Repl(backend As String)
        Dim interactive = Not System.Console.IsInputRedirected

        ProcessModule.SetContext(System.Environment.CurrentDirectory, System.Environment.CurrentDirectory,
                                 ReplSource, New List(Of Object) From {"njs"})

        Dim io As ScriptIO = New ConsoleIO()
        Dim engine = NewEngine(io)

        If backend <> "ansi" Then
            System.Console.Error.WriteLine("note: the REPL always writes live console output, --" & backend & " is ignored")
        End If

        If interactive Then
            Banner()
            ' Ctrl+C leaves the session; without this the CLR would kill the process with the
            ' default handler (the REPL holds no buffered output, so there is nothing to flush)
            AddHandler System.Console.CancelKeyPress,
                Sub(sender, e)
                    e.Cancel = True
                    System.Console.WriteLine()
                    System.Environment.Exit(0)
                End Sub
        End If

        ' A redirected session is decoded as UTF-8 (with BOM detection) instead of trusting
        ' Console.InputEncoding: the console code page may be a legacy one, which would mangle
        ' the BOM and any non ASCII text of a piped session.
        Dim stdin As TextReader = Nothing

        If Not interactive Then
            stdin = New StreamReader(System.Console.OpenStandardInput(), New UTF8Encoding(False), True)
        End If

        Dim pending As New StringBuilder()

        Try
            While True
                If interactive Then
                    System.Console.Write(If(pending.Length = 0, PrimaryPrompt, ContinuePrompt))
                End If

                Dim line = ReadInput(stdin)

                If line Is Nothing Then
                    ' Ctrl+D, or the end of a piped session
                    If interactive Then System.Console.WriteLine()
                    Return
                End If

                ' a stray BOM is never part of an input
                If line.Length > 0 AndAlso line(0) = ChrW(&HFEFF) Then line = line.Substring(1)


                ' dot commands only count when no input is pending
                If pending.Length = 0 Then
                    Dim command = line.Trim()

                    If command.StartsWith("."c) Then
                        Select Case command.ToLowerInvariant()
                            Case ".exit", ".quit", ".q"
                                Return
                            Case ".help", ".h", ".?"
                                Help()
                            Case ".clear", ".cls"
                                io.Clear()
                            Case ".version"
                                System.Console.WriteLine(EngineVersion())
                            Case ".reset"
                                engine = NewEngine(io)
                                System.Console.WriteLine("(globals cleared)")
                            Case Else
                                System.Console.Error.WriteLine("(unknown command '" & command & "' — .help lists the commands)")
                        End Select

                        Continue While
                    End If
                End If

                If pending.Length = 0 AndAlso line.Trim().Length = 0 Then
                    Continue While      ' an empty input only gives a fresh prompt, like node
                End If

                pending.AppendLine(line)

                Dim source = pending.ToString()
                Dim program As JsProgram = Nothing

                ' like node / devtools, a leading '{' is an object literal whenever it can be one:
                ' a bare `{ a: 1 }` would otherwise parse as a block statement with a label
                If source.TrimStart().StartsWith("{"c) Then
                    Try
                        program = Parser.Parse("(" & source & ")")
                    Catch ex As ParseException
                        program = Nothing
                    End Try
                End If

                If program Is Nothing Then
                    Try
                        program = Parser.Parse(source)
                    Catch ex As ParseException
                        ' an open block / unterminated literal is not an error yet: keep reading
                        If IsIncomplete(ex) Then Continue While

                        ReportUncaught(ex, ReplSource)
                        pending.Clear()
                        Continue While
                    End Try
                End If

                pending.Clear()

                Try
                    ' the completion value is echoed even when it is undefined, matching node
                    System.Console.WriteLine(DisplayResult(engine.Evaluate(program), interactive))
                Catch ex As Exception
                    ReportUncaught(ex, ReplSource)
                End Try
            End While
        Finally
            If stdin IsNot Nothing Then stdin.Dispose()
        End Try
    End Sub

    ''' <summary>Next input line: the console when interactive, the UTF-8 pipe otherwise.</summary>
    Private Function ReadInput(stdin As TextReader) As String
        If stdin IsNot Nothing Then Return stdin.ReadLine()

        Return System.Console.ReadLine()
    End Function

    ''' <summary>A fresh engine bound to <paramref name="io"/> with the REPL source name.</summary>
    Private Function NewEngine(io As ScriptIO) As Interpreter
        Dim engine As New Interpreter(io)

        engine.SourceFile = ReplSource
        Return engine
    End Function

    ''' <summary>
    ''' True when the parser stopped because the input is incomplete (unclosed block, bracket
    ''' or literal) rather than because it is wrong: the REPL then reads another line instead
    ''' of reporting a syntax error.
    ''' </summary>
    Private Function IsIncomplete(ex As ParseException) As Boolean
        Dim message = ex.Message

        Return message.IndexOf("unexpected end of input", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               message.IndexOf("<eof>", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               message.IndexOf("unterminated", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>
    ''' REPL echo format: strings are quoted (like node), primitives are colourised when the
    ''' session is interactive, everything else reuses the engine's JSON style display.
    ''' </summary>
    Private Function DisplayResult(value As Object, colorize As Boolean) As String
        If value Is Nothing Then Return Paint("null", AnsiColor.BrightBlack, colorize)
        If value Is JsRuntime.Undef Then Return Paint("undefined", AnsiColor.BrightBlack, colorize)

        If TypeOf value Is String Then
            Dim text = CStr(value)

            Return Paint("'" & text.Replace("\"c, "\\").Replace("'"c, "\'") & "'",
                         AnsiColor.BrightGreen, colorize)
        End If

        If TypeOf value Is Double OrElse TypeOf value Is Boolean Then
            Return Paint(JsRuntime.JsDisplay(value), AnsiColor.BrightYellow, colorize)
        End If

        ' a JS function echoes its source, devtools style: ƒ add(a, b) { throw ... }
        ' (no source for host delegates / generated code → the node inspect form)
        If TypeOf value Is Func(Of Object(), Object) Then
            Return Paint(If(JsFunctions.DevToolsEcho(value), JsFunctions.Inspect(value)),
                         AnsiColor.BrightCyan, colorize)
        End If

        ' a native (host) function: ƒ log() { [native code] }
        Dim builtin = TryCast(value, BuiltinMethod)

        If builtin IsNot Nothing Then
            Return Paint("ƒ " & builtin.Name & "() { [native code] }", AnsiColor.BrightCyan, colorize)
        End If

        Return JsRuntime.JsDisplay(value)
    End Function

    ''' <summary>Wrap <paramref name="text"/> into an ANSI foreground colour.</summary>
    Private Function Paint(text As String, color As AnsiColor, enabled As Boolean) As String
        If Not enabled Then Return text

        Return color.GetEscapeSequence() & text & AnsiEscapeCodes.Reset
    End Function

    Private Sub Banner()
        System.Console.WriteLine("njs — JavaScript running on .NET CLR")
        System.Console.WriteLine("LiteJs " & EngineVersion() & ", .NET " & System.Environment.Version.ToString())
        System.Console.WriteLine()
        System.Console.WriteLine("Type "".help"" for more information.")
    End Sub

    Private Sub Help()
        System.Console.WriteLine("  .help      show this help")
        System.Console.WriteLine("  .clear     clear the console")
        System.Console.WriteLine("  .reset     drop every global binding (fresh engine)")
        System.Console.WriteLine("  .version   engine version")
        System.Console.WriteLine("  .exit      leave the session (Ctrl+D does the same)")
        System.Console.WriteLine()
        System.Console.WriteLine("An input is evaluated as soon as it parses, so multi line declarations")
        System.Console.WriteLine("(function / if / class with an open brace) continue on the next prompt.")
    End Sub

    Private Function EngineVersion() As String
        Dim version = GetType(Interpreter).Assembly.GetName().Version

        Return If(version Is Nothing, "unknown", version.ToString())
    End Function

    Private Sub Usage(Optional [error] As String = Nothing)
        If Not String.IsNullOrEmpty([error]) Then
            System.Console.Error.WriteLine("error: " & [error])
        End If

        System.Console.Error.WriteLine("JavaScript Running on .NET CLR Environment")
        System.Console.Error.WriteLine()
        System.Console.Error.WriteLine("Usage: njs                      start an interactive REPL")
        System.Console.Error.WriteLine("       njs <script.js> [args...] [--ansi|--plain|--html <out.html>]")
        System.Console.Error.WriteLine("  --ansi         render %c CSS styles and log levels as ANSI colours (default)")
        System.Console.Error.WriteLine("  --plain        write the collected plain text output to stdout")
        System.Console.Error.WriteLine("  --html <file>  render a styled standalone HTML terminal page")
    End Sub

End Module
