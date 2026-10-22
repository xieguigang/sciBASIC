Imports System.IO

Namespace Runtime

    ''' <summary>
    ''' Loads JavaScript modules for <c>import ... from './m.js'</c>.
    ''' <para>
    ''' A relative specifier resolves against the importing script's directory (the module
    ''' directory while a module body runs, so nested imports stay relative to their own
    ''' file) and then against the application base; without an explicit extension
    ''' <c>.js</c> and <c>index.js</c> are tried as well.
    ''' </para>
    ''' <para>
    ''' Modules are cached by absolute path: a module body runs exactly once, side effects
    ''' included, and a cycle is reported instead of recursing. The body is executed by
    ''' the interpreter running the importing script (<see cref="Executor"/>), so a module
    ''' sees the realm globals but not the importer's script-level variables; translated
    ''' code has no interpreter in scope and falls back to a fresh engine.
    ''' </para>
    ''' </summary>
    Friend Module ModuleLoader

        <ThreadStatic>
        Private _cache As Dictionary(Of String, Dictionary(Of String, Object))

        <ThreadStatic>
        Private _loading As HashSet(Of String)

        <ThreadStatic>
        Private _executor As Func(Of String, Dictionary(Of String, Object))

        ''' <summary>
        ''' Executes one module file and returns its top-level declarations. Installed by
        ''' <see cref="Interpreter.Run"/> so modules share the running realm.
        ''' </summary>
        Friend Property Executor As Func(Of String, Dictionary(Of String, Object))
            Get
                Return _executor
            End Get
            Set(value As Func(Of String, Dictionary(Of String, Object)))
                _executor = value
            End Set
        End Property

        ''' <summary>Resolve a specifier to an absolute file path, or throw a script error.</summary>
        Friend Function ResolvePath(specifier As String, baseDirectory As String) As String
            Dim spec = If(specifier, "").Trim()
            If spec.Length = 0 Then Throw Failure("an import needs a non-empty 'from' specifier")

            Dim roots As New List(Of String)
            If Path.IsPathRooted(spec) Then
                roots.Add("")
            Else
                If Not String.IsNullOrEmpty(baseDirectory) Then roots.Add(baseDirectory)
                Dim application = AppContext.BaseDirectory
                If Not String.Equals(application, baseDirectory, StringComparison.OrdinalIgnoreCase) Then
                    roots.Add(application)
                End If
            End If

            Dim tried As New List(Of String)
            For Each root In roots
                Dim basePath = If(root.Length = 0, spec, Path.Combine(root, spec))
                For Each candidate In Candidates(basePath)
                    tried.Add(candidate)
                    If File.Exists(candidate) Then Return Path.GetFullPath(candidate)
                Next
            Next

            Throw Failure("cannot find module '" & spec & "'; tried: " & String.Join(", ", tried))
        End Function

        ''' <summary>
        ''' Load a module (once per absolute path) and return its top-level bindings.
        ''' </summary>
        Friend Function Load(specifier As String, baseDirectory As String) As Dictionary(Of String, Object)
            Dim path = ResolvePath(specifier, baseDirectory)

            If _cache Is Nothing Then
                _cache = New Dictionary(Of String, Dictionary(Of String, Object))(StringComparer.OrdinalIgnoreCase)
            End If
            Dim loaded As Dictionary(Of String, Object) = Nothing
            If _cache.TryGetValue(path, loaded) Then Return loaded

            If _loading Is Nothing Then
                _loading = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            End If
            If Not _loading.Add(path) Then
                Throw Failure("circular import detected while loading '" & path & "'")
            End If

            Dim savedBase = JsRuntime.ScriptBaseDirectory
            Dim savedExecutor = _executor
            Try
                ' the module's own imports resolve against its folder
                JsRuntime.ScriptBaseDirectory = Path.GetDirectoryName(path)
                Dim exports = Execute(path)
                _cache(path) = exports
                Return exports
            Catch ex As JsRuntimeException
                Throw
            Catch ex As Exception
                Throw Failure("cannot execute module '" & path & "': " & ex.Message)
            Finally
                JsRuntime.ScriptBaseDirectory = savedBase
                _executor = savedExecutor
                _loading.Remove(path)
            End Try
        End Function

        Private Function Execute(path As String) As Dictionary(Of String, Object)
            If _executor IsNot Nothing Then Return _executor.Invoke(path)

            ' translated code: no interpreter is running, so run the module on a fresh one
            ' (sharing the console sink); its own nested imports reuse that engine
            Dim engine As New Interpreter(JsRuntime.Io)
            _executor = AddressOf engine.LoadModuleExports
            Return engine.LoadModuleExports(path)
        End Function

        ''' <summary>Candidate files for a specifier: exact, <c>.js</c>, <c>index.js</c>.</summary>
        Private Function Candidates(path As String) As List(Of String)
            Dim list As New List(Of String) From {path}
            If Not path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) AndAlso
               Not path.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase) Then
                list.Add(path & ".js")
                list.Add(Path.Combine(path, "index.js"))
            End If
            Return list
        End Function

        ''' <summary>Drop the module cache and the in-flight set.</summary>
        Friend Sub Clear()
            _cache = Nothing
            _loading = Nothing
        End Sub

        Private Function Failure(message As String) As JsRuntimeException
            Return JsRuntimeException.Error_("Error", message)
        End Function

    End Module

End Namespace
