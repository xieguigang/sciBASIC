Imports System.IO
Imports System.Reflection

Namespace Runtime

    ''' <summary>
    ''' Assembly and namespace resolution behind <c>import Foo.Bar as ns from 'x.dll'</c>.
    ''' <para>
    ''' The specifier is resolved in this order: an already loaded assembly with that
    ''' simple (file) name, an explicit path — absolute, or relative to the importing
    ''' script and then to the application base — <see cref="System.Reflection.Assembly.Load(AssemblyName)"/>
    ''' (which honours the runtime's own probing), the host search paths
    ''' (<see cref="ClrInterop.AddAssemblySearchPath"/>) and finally the host resolver
    ''' callback (<see cref="ClrInterop.AssemblyResolver"/>). A specifier that no step
    ''' accepts raises a script-catchable error listing what was tried.
    ''' </para>
    ''' <para>
    ''' A namespace is exposed as a FLAT map of simple type name → <see cref="ClrType"/>:
    ''' public types only, no nested types and no open generic definitions (a definition
    ''' such as <c>List(Of )</c> cannot be constructed, so it is left out). Scanning a
    ''' namespace happens once per (assembly, namespace) and is then cached.
    ''' </para>
    ''' </summary>
    Friend Module AssemblyResolver

        Private ReadOnly _assemblies As New Dictionary(Of String, Assembly)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _namespaces As New Dictionary(Of String, Dictionary(Of String, Object))(StringComparer.Ordinal)
        Private ReadOnly _gate As New Object

        ''' <summary>Load (or reuse) the assembly named by an import specifier.</summary>
        Friend Function Resolve(specifier As String, baseDirectory As String) As Assembly
            Dim spec = If(specifier, "").Trim()
            If spec.Length = 0 Then Throw Failure("an import needs a non-empty 'from' specifier")

            SyncLock _gate
                Dim cached As Assembly = Nothing
                If _assemblies.TryGetValue(spec, cached) Then Return cached
            End SyncLock

            Dim tried As New List(Of String)
            Dim found = FindLoaded(spec)

            If found Is Nothing Then
                ' an explicit dll path (absolute, or relative to the script / app base)
                Dim path = ResolveExistingPath(spec, baseDirectory)
                If path IsNot Nothing Then
                    tried.Add(path)
                    Try
                        found = Assembly.LoadFrom(path)
                    Catch ex As Exception
                        Throw Failure("cannot load assembly '" & spec & "' from '" & path & "': " & ex.Message)
                    End Try
                End If
            End If

            If found Is Nothing Then
                Dim bare = BareAssemblyName(spec)
                tried.Add("Assembly.Load(" & bare & ")")
                Try
                    found = Assembly.Load(New AssemblyName(bare))
                Catch ex As Exception
                    ' not resolvable by name: the host paths / callback below may still know it
                End Try
            End If

            If found Is Nothing Then
                For Each directory In ClrInterop.AssemblySearchPaths
                    Dim candidate = Path.Combine(directory, BareAssemblyName(spec) & ".dll")
                    tried.Add(candidate)
                    If Not File.Exists(candidate) Then Continue For
                    Try
                        found = Assembly.LoadFrom(candidate)
                    Catch ex As Exception
                        found = Nothing
                    End Try
                    If found IsNot Nothing Then Exit For
                Next
            End If

            If found Is Nothing Then
                Dim resolver = ClrInterop.AssemblyResolver
                If resolver IsNot Nothing Then
                    tried.Add("the host AssemblyResolver callback")
                    Try
                        found = resolver(spec)
                    Catch ex As Exception
                        found = Nothing
                    End Try
                End If
            End If

            If found Is Nothing Then
                Throw Failure("cannot load assembly '" & spec & "'; tried: " & String.Join(", ", tried))
            End If

            SyncLock _gate
                _assemblies(spec) = found
            End SyncLock
            Return found
        End Function

        ''' <summary>
        ''' The public types of <paramref name="namespaceName"/> as a script object:
        ''' simple type name → <see cref="ClrType"/>.
        ''' </summary>
        Friend Function ImportNamespace(namespaceName As String, specifier As String,
                                       baseDirectory As String) As Dictionary(Of String, Object)
            Dim ns = If(namespaceName, "").Trim()
            If ns.Length = 0 Then Throw Failure("an import needs a non-empty namespace name")

            Dim assembly = Resolve(specifier, baseDirectory)
            Dim key = assembly.FullName & "|" & ns

            SyncLock _gate
                Dim cached As Dictionary(Of String, Object) = Nothing
                If _namespaces.TryGetValue(key, cached) Then Return cached
            End SyncLock

            Dim exported As Type() = Nothing
            Try
                exported = assembly.GetExportedTypes()
            Catch ex As ReflectionTypeLoadException
                exported = ex.Types
            Catch ex As Exception
                Throw Failure("cannot read the types of assembly '" & assembly.GetName().Name & "': " & ex.Message)
            End Try

            Dim map As New Dictionary(Of String, Object)(StringComparer.Ordinal)
            For Each t In exported
                If t Is Nothing Then Continue For
                If t.IsNested Then Continue For
                If t.IsGenericTypeDefinition Then Continue For
                If Not String.Equals(t.Namespace, ns, StringComparison.Ordinal) Then Continue For
                If map.ContainsKey(t.Name) Then Continue For
                map(t.Name) = ClrInterop.WrapType(t)
            Next

            If map.Count = 0 Then
                Throw Failure("namespace '" & ns & "' has no public types in assembly '" &
                              assembly.GetName().Name & "'")
            End If

            SyncLock _gate
                _namespaces(key) = map
            End SyncLock
            Return map
        End Function

        ''' <summary>Drop the assembly / namespace caches.</summary>
        Friend Sub Clear()
            SyncLock _gate
                _assemblies.Clear()
                _namespaces.Clear()
            End SyncLock
        End Sub

        ' ---------------- helpers ----------------

        Private Function FindLoaded(spec As String) As Assembly
            Dim wanted = BareAssemblyName(spec)
            For Each assembly In AppDomain.CurrentDomain.GetAssemblies()
                If String.Equals(assembly.GetName().Name, wanted, StringComparison.OrdinalIgnoreCase) Then Return assembly
                Dim location As String = Nothing
                Try
                    location = assembly.Location
                Catch ex As Exception
                    ' dynamic / in-memory assemblies have no location
                End Try
                If String.IsNullOrEmpty(location) Then Continue For
                If String.Equals(Path.GetFileNameWithoutExtension(location), wanted, StringComparison.OrdinalIgnoreCase) Then
                    Return assembly
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>Existing file path for a specifier that names a dll, or Nothing.</summary>
        Private Function ResolveExistingPath(spec As String, baseDirectory As String) As String
            If Not spec.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) Then Return Nothing
            If Path.IsPathRooted(spec) Then
                Return If(File.Exists(spec), Path.GetFullPath(spec), Nothing)
            End If
            If Not String.IsNullOrEmpty(baseDirectory) Then
                Dim relative = Path.Combine(baseDirectory, spec)
                If File.Exists(relative) Then Return Path.GetFullPath(relative)
            End If
            Dim application = Path.Combine(AppContext.BaseDirectory, spec)
            If File.Exists(application) Then Return Path.GetFullPath(application)
            Return Nothing
        End Function

        ''' <summary>File name without directory and without the <c>.dll</c> extension.</summary>
        Private Function BareAssemblyName(spec As String) As String
            Dim name = Path.GetFileName(spec)
            If name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) Then
                name = name.Substring(0, name.Length - 4)
            End If
            Return name
        End Function

        Private Function Failure(message As String) As JsRuntimeException
            Return JsRuntimeException.Error_("Error", message)
        End Function

    End Module

End Namespace
