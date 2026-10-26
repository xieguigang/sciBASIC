Imports System.IO

Namespace Runtime

    ''' <summary>
    ''' Script introspection builtins ("magic functions"), the JavaScript counterpart of the
    ''' <c>Magics.Build</c> snippets of the VBS engine
    ''' (<c>vs_solutions\VBS\src\VBScript\Magics.vb</c>):
    ''' <c>here(relpath)</c>, <c>scriptDir()</c>, <c>scriptFile()</c>,
    ''' <c>scriptName(withExtension?)</c>, <c>scriptText()</c> and <c>scriptLines()</c>.
    ''' <para>
    ''' The directory is anchored at <see cref="JsRuntime.ScriptBaseDirectory"/> — which
    ''' <c>Interpreter.Run</c> narrows to the folder of the script it is running — so a
    ''' relative path passed to <c>here</c> resolves exactly like the VBS
    ''' <c>Here(relpath)</c> magic function does. Absolute paths are returned unchanged
    ''' (only normalised) and an empty argument yields the script folder itself.
    ''' </para>
    ''' <para>
    ''' These names are dispatched through the <c>global</c> native object
    ''' (see <c>JsRuntime.GlobalIdentifiers</c> / <c>BuiltinGlobalFn</c>) rather than through a
    ''' new module: the code generator resolves known globals with
    ''' <c>JsGet(GlobalObj, name)</c>, so interpreted and translated scripts behave the same.
    ''' </para>
    ''' </summary>
    Public Module ScriptMagic

        ''' <summary>The names exposed as script globals (used by the engine and the code generator).</summary>
        Public ReadOnly Names As String() = {
            "here", "scriptDir", "scriptFile", "scriptName", "scriptText", "scriptLines"
        }

        ''' <summary>Is <paramref name="name"/> one of the script magic functions?</summary>
        Public Function IsMagic(name As String) As Boolean
            If String.IsNullOrEmpty(name) Then Return False

            For Each item As String In Names
                If String.Equals(item, name, StringComparison.Ordinal) Then Return True
            Next

            Return False
        End Function

        ' ===================== the magic functions =====================

        ''' <summary>
        ''' Resolve a path relative to the folder of the script that is currently running;
        ''' an absolute path is returned normalised and <c>""</c> yields the script folder.
        ''' </summary>
        Public Function Here(relpath As String) As String
            Dim folder As String = ScriptDir()

            If String.IsNullOrEmpty(relpath) Then
                Return folder
            End If

            Try
                If Path.IsPathRooted(relpath) Then
                    Return Path.GetFullPath(relpath)
                End If

                If String.IsNullOrEmpty(folder) Then
                    Return Path.GetFullPath(relpath)
                End If

                Return Path.GetFullPath(Path.Combine(folder, relpath))
            Catch ex As Exception
                Throw JsRuntimeException.Error_("Error",
                    "here('" & relpath & "'): cannot resolve the path: " & ex.Message)
            End Try
        End Function

        ''' <summary>The folder of the currently running script (no trailing separator).</summary>
        Public Function ScriptDir() As String
            Dim folder = JsRuntime.ScriptBaseDirectory

            If String.IsNullOrEmpty(folder) Then Return ""

            Try
                Return Path.GetFullPath(folder)
            Catch ex As Exception
                Return folder
            End Try
        End Function

        ''' <summary>
        ''' The absolute path of the script currently running, or <c>""</c> when the source has
        ''' no file behind it (a <c>-e</c> snippet, a REPL line, a unit test that only parses text).
        ''' </summary>
        Public Function ScriptFile() As String
            Dim file = JsRuntime.CurrentScriptFile

            If String.IsNullOrEmpty(file) Then Return ""

            Try
                Return Path.GetFullPath(file)
            Catch ex As Exception
                Return file
            End Try
        End Function

        ''' <summary>
        ''' File name of the running script; <paramref name="withExtension"/> = False strips the
        ''' extension (<c>demo.js</c> → <c>demo</c>).
        ''' </summary>
        Public Function ScriptName(Optional withExtension As Boolean = True) As String
            Dim file = ScriptFile()

            If String.IsNullOrEmpty(file) Then Return ""

            Return If(withExtension, Path.GetFileName(file), Path.GetFileNameWithoutExtension(file))
        End Function

        ''' <summary>The source text of the running script.</summary>
        Public Function ScriptText() As String
            Return File.ReadAllText(ExistingScriptFile("scriptText"))
        End Function

        ''' <summary>The source lines of the running script.</summary>
        Public Function ScriptLines() As String()
            Return File.ReadAllLines(ExistingScriptFile("scriptLines"))
        End Function

        ' ===================== dispatch =====================

        ''' <summary>
        ''' Invoke one of the magic functions for the builtin dispatch of the
        ''' <c>global</c> object; an unknown name is a normal JS <c>TypeError</c>.
        ''' </summary>
        Friend Function Dispatch(name As String, args As Object()) As Object
            Select Case name
                Case "here"
                    Return Here(JsRuntime.JsStr(Arg(args, 0)))
                Case "scriptDir"
                    Return ScriptDir()
                Case "scriptFile"
                    Return ScriptFile()
                Case "scriptName"
                    If args.Length = 0 Then Return ScriptName()
                    Return ScriptName(JsRuntime.JsTruthy(Arg(args, 0)))
                Case "scriptText"
                    Return ScriptText()
                Case "scriptLines"
                    Dim lines As New List(Of Object)()

                    For Each line As String In ScriptLines()
                        Call lines.Add(line)
                    Next

                    Return lines
                Case Else
                    Throw JsRuntimeException.TypeError(name & " is not a function")
            End Select
        End Function

        Private Function ExistingScriptFile(caller As String) As String
            ' note: the local must not be named 'file' — VB is case insensitive and it would
            ' shadow the System.IO.File type used right below
            Dim sourcePath = ScriptFile()

            If String.IsNullOrEmpty(sourcePath) OrElse Not File.Exists(sourcePath) Then
                Throw JsRuntimeException.Error_("Error",
                    caller & "(): the running script has no source file on disk")
            End If

            Return sourcePath
        End Function

        Private Function Arg(args As Object(), index As Integer) As Object
            If args Is Nothing OrElse index >= args.Length Then Return JsRuntime.Undef

            Return args(index)
        End Function

    End Module

End Namespace
