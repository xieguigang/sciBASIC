Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' Minimal host that runs a JavaScript file with the LiteJs engine, exposing a Node.js-like
''' environment (process.cwd / __dirname / __filename / process.argv). Designed so its stdout
''' can be diffed against <c>node &lt;script.js&gt;</c> output for fidelity checks.
''' </summary>
Module Program

    Sub Main()
        Dim raw = System.Environment.GetCommandLineArgs().ToList()
        ' raw(0) is the host executable; locate the first *.js argument as the script
        Dim idx = -1
        For i = 0 To raw.Count - 1
            If raw(i).EndsWith(".js", System.StringComparison.OrdinalIgnoreCase) Then
                idx = i
                Exit For
            End If
        Next
        If idx < 0 Then
            System.Console.Error.WriteLine("Usage: LiteJs.Runner <script.js> [args...]")
            System.Environment.Exit(1)
            Return
        End If

        Dim scriptPath = Path.GetFullPath(raw(idx))
        Dim userArgs = raw.GetRange(idx + 1, raw.Count - idx - 1)

        Dim argv As New System.Collections.Generic.List(Of Object)()
        argv.Add("node")                 ' execPath  (argv[0] in Node)
        argv.Add(scriptPath)             ' script path (argv[1] in Node)
        For Each a In userArgs : argv.Add(a) : Next

        Dim cwd = System.Environment.CurrentDirectory
        Dim scriptDir = Path.GetDirectoryName(scriptPath)
        ProcessModule.SetContext(cwd, scriptDir, scriptPath, argv)

        Dim io As ScriptIO = New ConsoleIO()
        Dim engine As New Interpreter(io)
        Try
            engine.Run(Parser.Parse(File.ReadAllText(scriptPath)))
        Catch ex As JsRuntimeException
            System.Console.Error.WriteLine("Uncaught " & ex.Message)
            System.Environment.Exit(1)
        End Try
    End Sub

End Module
