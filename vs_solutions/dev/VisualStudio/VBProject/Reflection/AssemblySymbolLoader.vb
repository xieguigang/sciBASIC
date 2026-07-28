Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports TypeInfo = Microsoft.VisualBasic.Scripting.MetaData.TypeInfo

''' <summary>
''' Loads a .NET assembly (dll) via read-only metadata reflection and maps
''' every type and member into the VBLang symbol tree model. The whole
''' assembly is hosted inside a single virtual <see cref="VBDocument"/> whose
''' <see cref="VBDocument.Types"/> dictionary holds the symbol tree (namespaces
''' -> types -> nested types / members). No source code is executed.
''' </summary>
Namespace Reflection

    ''' <summary>
    ''' Loads a .NET assembly (dll) via read-only metadata reflection and maps
    ''' every type and member into the VBLang symbol tree model. The whole
    ''' assembly is hosted inside a single virtual <see cref="VBDocument"/> whose
    ''' <see cref="VBDocument.Types"/> dictionary holds the symbol tree (namespaces
    ''' -> types -> nested types / members). No source code is executed.
    ''' </summary>
    Public Module AssemblySymbolLoader

    ''' <summary>
    ''' Read-only load a .NET dll and build its symbol tree wrapped in a
    ''' <see cref="VBProject"/>. The loaded assembly metadata is never executed.
    ''' </summary>
    ''' <param name="dllPath">absolute or relative path to the target dll.</param>
    Public Function LoadAssembly(dllPath As String) As VBProject
        If String.IsNullOrWhiteSpace(dllPath) Then
            Throw New ArgumentNullException(NameOf(dllPath))
        End If

        dllPath = Path.GetFullPath(dllPath)

        If Not File.Exists(dllPath) Then
            Throw New FileNotFoundException("The target assembly file does not exist.", dllPath)
        End If

        ' Resolve dependency assemblies: the current runtime assemblies plus the
        ' target dll itself. This lets MetadataLoadContext resolve base types,
        ' interfaces and generic arguments without loading the assembly for real.
        Dim runtimeDir As String = RuntimeEnvironment.GetRuntimeDirectory()
        Dim paths As New List(Of String)(Directory.GetFiles(runtimeDir, "*.dll"))
        If Not paths.Contains(dllPath, StringComparer.OrdinalIgnoreCase) Then
            paths.Add(dllPath)
        End If

        Dim resolver As New PathAssemblyResolver(paths)

        Using context As New MetadataLoadContext(resolver)
            Dim assembly As Assembly = context.LoadFromAssemblyPath(dllPath)
            Return BuildProject(assembly, dllPath)
        End Using
    End Function

    ' ------------------------------------------------------------------ build

    Private Function BuildProject(assembly As Assembly, dllPath As String) As VBProject
        ' Synthetic root namespace container, mirrors VBParser.Parse output: the
        ' root's InternalNested dictionary becomes the virtual document's Types.
        Dim root As New NamespaceSymbol()
        root.Name = ""

        Dim nsCache As New Dictionary(Of String, TypeContainerSymbol)(StringComparer.OrdinalIgnoreCase)
        Dim typeSymbols As New Dictionary(Of String, LanguageSymbolType)()

        Dim types As Type() = GetAllTypes(assembly)

        ' Pass 1: create the type symbol (without members) for every type.
        For Each t In types
            If t.FullName Is Nothing Then Continue For
            Dim sym As LanguageSymbolType = Nothing
            Try
                sym = CreateTypeSymbol(t)
            Catch
                sym = Nothing
            End Try
            If sym IsNot Nothing Then
                typeSymbols(t.FullName) = sym
            End If
        Next

        ' Pass 2: attach each type under its parent namespace or declaring type.
        For Each t In types
            If t.FullName Is Nothing Then Continue For
            Dim sym As LanguageSymbolType = Nothing
            If Not typeSymbols.TryGetValue(t.FullName, sym) Then Continue For

            Dim container As TypeContainerSymbol = ResolveParent(t, root, nsCache, typeSymbols)
            If container Is Nothing Then Continue For

            If container.InternalNested Is Nothing Then
                container.InternalNested = New Dictionary(Of String, LanguageSymbolType)()
            End If
            container.InternalNested(sym.Name) = sym
        Next

        ' Pass 3: map members into each container type (tree preserved).
        For Each t In types
            If t.FullName Is Nothing Then Continue For
            Dim sym As LanguageSymbolType = Nothing
            If Not typeSymbols.TryGetValue(t.FullName, sym) Then Continue For
            Dim ct As TypeContainerSymbol = TryCast(sym, TypeContainerSymbol)
            If ct Is Nothing Then Continue For
            Try
                MapMembers(t, ct)
            Catch
            End Try
        Next

        Dim vbdoc As New VBDocument()
        vbdoc.FileName = dllPath
        vbdoc.Imports = New String() {}
        vbdoc.Types = If(root.InternalNested, New Dictionary(Of String, LanguageSymbolType)())

        Dim proj As New VBProject()
        proj.AssemblyName = If(assembly.GetName()?.Name, Path.GetFileNameWithoutExtension(dllPath))
        proj.OutputType = "Library"
        proj.RootNamespace = ""
        proj.CompileFiles = New VBDocument() {vbdoc}

        Return proj
    End Function

    Private Function ResolveParent(type As Type,
                                    root As TypeContainerSymbol,
                                    nsCache As Dictionary(Of String, TypeContainerSymbol),
                                    typeSymbols As Dictionary(Of String, LanguageSymbolType)) As TypeContainerSymbol
        If type.DeclaringType IsNot Nothing Then
            Dim parentSym As LanguageSymbolType = Nothing
            If typeSymbols.TryGetValue(type.DeclaringType.FullName, parentSym) Then
                Return TryCast(parentSym, TypeContainerSymbol)
            End If
            Return Nothing
        End If

        Dim ns As String = If(type.Namespace, "")
        If ns.Length = 0 Then Return root
        Return GetNamespace(ns, root, nsCache)
    End Function

    Private Function GetNamespace(ns As String,
                                  root As TypeContainerSymbol,
                                  nsCache As Dictionary(Of String, TypeContainerSymbol)) As TypeContainerSymbol
        If nsCache.ContainsKey(ns) Then Return nsCache(ns)

        Dim segments As String() = ns.Split(New Char() {"."c}, StringSplitOptions.RemoveEmptyEntries)
        Dim current As TypeContainerSymbol = root
        Dim accumulated As String = ""

        For Each seg In segments
            accumulated = If(accumulated.Length = 0, seg, accumulated & "." & seg)

            If nsCache.ContainsKey(accumulated) Then
                current = nsCache(accumulated)
                Continue For
            End If

            Dim nsContainer As New NamespaceSymbol()
            nsContainer.Name = seg

            If current.InternalNested Is Nothing Then
                current.InternalNested = New Dictionary(Of String, LanguageSymbolType)()
            End If
            current.InternalNested(seg) = nsContainer
            nsCache(accumulated) = nsContainer
            current = nsContainer
        Next

        Return current
    End Function

    ' ------------------------------------------------------------- type symbol

    Private Function CreateTypeSymbol(type As Type) As LanguageSymbolType
        Dim sym As LanguageSymbolType

        If type.IsEnum Then
            Dim ct As New EnumSymbol()
            ct.Name = type.Name
            ct.EnumBaseType = MapEnumUnderlyingType(type)
            sym = ct

        ElseIf IsDelegateType(type) Then
            sym = MapDelegate(type)

        ElseIf type.IsInterface Then
            Dim ct As New InterfaceSymbol()
            ct.Name = type.Name
            sym = ct

        ElseIf IsModule(type) Then
            Dim ct As New ModuleSymbol()
            ct.Name = type.Name
            sym = ct

        ElseIf type.IsValueType Then
            Dim ct As New StructureSymbol()
            ct.Name = type.Name
            sym = ct

        Else
            Dim ct As New ClassSymbol()
            ct.Name = type.Name
            sym = ct
        End If

        If TypeOf sym Is TypeContainerSymbol Then
            Dim ct As TypeContainerSymbol = DirectCast(sym, TypeContainerSymbol)
            ct.Modifiers = BuildTypeModifiers(type)
            ct.Attributes = MapAttributes(type.GetCustomAttributesData())
            ct.GenericTypeArguments = MapGenericArgs(type)

            If ct.Type <> SymbolType.Enum Then
                Dim baseType As Type = Nothing
                Try : baseType = type.BaseType : Catch : baseType = Nothing : End Try
                ct.InheritsType = MapBaseType(baseType)
                ct.ImplementsInterfaces = MapInterfaces(type, baseType)
            End If
        ElseIf TypeOf sym Is DelegateSymbol Then
            Dim dt As DelegateSymbol = DirectCast(sym, DelegateSymbol)
            dt.Modifiers = BuildTypeModifiers(type)
            dt.Attributes = MapAttributes(type.GetCustomAttributesData())
            dt.GenericTypeArguments = MapGenericArgs(type)
        End If

        Return sym
    End Function

    Private Function IsModule(type As Type) As Boolean
        If Not (type.IsSealed AndAlso type.IsAbstract) Then Return False
        ' VB modules compile to abstract sealed classes flagged with
        ' StandardModuleAttribute; static C# classes look the same, so both map
        ' to a VB Module.
        For Each a In type.GetCustomAttributesData()
            If a.AttributeType IsNot Nothing AndAlso a.AttributeType.Name = "StandardModuleAttribute" Then
                Return True
            End If
        Next
        Return True
    End Function

    Private Function IsDelegateType(type As Type) As Boolean
        If type.IsInterface Then Return False
        If type.FullName = "System.Delegate" OrElse type.FullName = "System.MulticastDelegate" Then
            Return False
        End If
        Dim base As Type = Nothing
        Try : base = type.BaseType : Catch : base = Nothing : End Try
        Return base IsNot Nothing AndAlso base.FullName = "System.MulticastDelegate"
    End Function

    Private Function MapDelegate(type As Type) As DelegateSymbol
        Dim dt As New DelegateSymbol()
        dt.Name = type.Name

        Dim invoke As MethodInfo = Nothing
        Try
            invoke = type.GetMethod("Invoke", BindingFlags.Public Or BindingFlags.Instance)
        Catch
            invoke = Nothing
        End Try

        If invoke IsNot Nothing Then
            dt.Parameters = MapParameters(invoke.GetParameters())
            Dim ret As Type = Nothing
            Try : ret = invoke.ReturnType : Catch : ret = Nothing : End Try
            If ret IsNot Nothing AndAlso ret.FullName <> "System.Void" Then
                dt.ValueType = ToVBTypeInfo(ret)
            End If
        Else
            dt.Parameters = New Dictionary(Of String, TypeInfo)()
        End If

        Return dt
    End Function

    Private Function MapEnumUnderlyingType(type As Type) As TypeInfo
        Dim underlying As Type = Nothing
        Try
            underlying = [Enum].GetUnderlyingType(type)
        Catch
            underlying = Nothing
        End Try

        If underlying Is Nothing Then
            Try
                For Each f In type.GetFields(BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
                    If Not f.IsStatic AndAlso f.Name = "value__" Then
                        underlying = f.FieldType
                        Exit For
                    End If
                Next
            Catch
            End Try
        End If

        Return ToVBTypeInfo(underlying)
    End Function

    ' ---------------------------------------------------------------- members

    Private Sub MapMembers(type As Type, ct As TypeContainerSymbol)
        If ct.Members Is Nothing Then
            ct.Members = New Dictionary(Of String, LanguageSymbolType)()
        End If

        ' methods / constructors / operators
        Dim methods As MethodInfo() = Nothing
        Try
            methods = type.GetMethods(BindingFlags.Public Or BindingFlags.NonPublic Or
                                      BindingFlags.Instance Or BindingFlags.Static Or
                                      BindingFlags.DeclaredOnly)
        Catch
            methods = Nothing
        End Try

        If methods IsNot Nothing Then
            For Each m In methods
                If m.IsSpecialName Then
                    Dim n As String = m.Name
                    If n.StartsWith("op_", StringComparison.OrdinalIgnoreCase) Then
                        ' operator, handled below
                    ElseIf n.StartsWith("get_", StringComparison.OrdinalIgnoreCase) OrElse
                           n.StartsWith("set_", StringComparison.OrdinalIgnoreCase) OrElse
                           n.StartsWith("add_", StringComparison.OrdinalIgnoreCase) OrElse
                           n.StartsWith("remove_", StringComparison.OrdinalIgnoreCase) OrElse
                           n.StartsWith("raise_", StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    Else
                        Continue For
                    End If
                End If

                Dim sym As MethodSymbol
                If m.IsConstructor Then
                    sym = New MethodSymbol(SymbolType.New)
                    sym.Name = "New"
                ElseIf m.IsSpecialName AndAlso m.Name.StartsWith("op_", StringComparison.OrdinalIgnoreCase) Then
                    sym = New MethodSymbol(SymbolType.Operator)
                    sym.Name = m.Name
                ElseIf m.ReturnType IsNot Nothing AndAlso m.ReturnType.FullName = "System.Void" Then
                    sym = New MethodSymbol(SymbolType.Sub)
                    sym.Name = m.Name
                Else
                    sym = New MethodSymbol(SymbolType.Function)
                    sym.Name = m.Name
                    If m.ReturnType IsNot Nothing Then
                        sym.ReturnType = ToVBTypeInfo(m.ReturnType)
                    End If
                End If

                sym.Parameters = MapParameters(m.GetParameters())
                sym.Modifiers = BuildMethodModifiers(m)
                sym.Attributes = MapAttributes(m.GetCustomAttributesData())
                sym.GenericTypeArguments = MapGenericArgs(m)
                ct.Members(sym.Name) = sym
            Next
        End If

        ' properties
        Dim props As PropertyInfo() = Nothing
        Try
            props = type.GetProperties(BindingFlags.Public Or BindingFlags.NonPublic Or
                                       BindingFlags.Instance Or BindingFlags.Static Or
                                       BindingFlags.DeclaredOnly)
        Catch
            props = Nothing
        End Try

        If props IsNot Nothing Then
            For Each p In props
                Dim sym As New PropertySymbol()
                sym.Name = p.Name
                Try
                    sym.Parameters = MapParameters(p.GetIndexParameters())
                Catch
                    sym.Parameters = New Dictionary(Of String, TypeInfo)()
                End Try
                Dim rt As Type = Nothing
                Try : rt = p.PropertyType : Catch : rt = Nothing : End Try
                sym.ReturnType = ToVBTypeInfo(rt)
                sym.Modifiers = BuildPropertyModifiers(p)
                sym.Attributes = MapAttributes(p.GetCustomAttributesData())
                ct.Members(sym.Name) = sym
            Next
        End If

        ' fields (enum values are literal static fields and are kept)
        Dim fields As FieldInfo() = Nothing
        Try
            fields = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or
                                    BindingFlags.Instance Or BindingFlags.Static Or
                                    BindingFlags.DeclaredOnly)
        Catch
            fields = Nothing
        End Try

        If fields IsNot Nothing Then
            For Each f In fields
                If f.IsSpecialName Then Continue For
                If f.Name = "value__" Then Continue For

                Dim sym As New VariableSymbol()
                sym.Name = f.Name
                Dim ft As Type = Nothing
                Try : ft = f.FieldType : Catch : ft = Nothing : End Try
                sym.ValueType = ToVBTypeInfo(ft)
                sym.Modifiers = BuildFieldModifiers(f)
                sym.Attributes = MapAttributes(f.GetCustomAttributesData())
                ct.Members(sym.Name) = sym
            Next
        End If

        ' events
        Dim events As EventInfo() = Nothing
        Try
            events = type.GetEvents(BindingFlags.Public Or BindingFlags.NonPublic Or
                                    BindingFlags.Instance Or BindingFlags.Static Or
                                    BindingFlags.DeclaredOnly)
        Catch
            events = Nothing
        End Try

        If events IsNot Nothing Then
            For Each e In events
                Dim sym As New EventSymbol()
                sym.Name = e.Name
                Dim et As Type = Nothing
                Try : et = e.EventHandlerType : Catch : et = Nothing : End Try
                sym.DelegateType = ToVBTypeInfo(et)
                sym.Modifiers = BuildEventModifiers(e)
                sym.Attributes = MapAttributes(e.GetCustomAttributesData())
                ct.Members(sym.Name) = sym
            Next
        End If
    End Sub

    ' ------------------------------------------------------------- modifiers

    Private Function BuildTypeModifiers(type As Type) As String
        Dim mods As New List(Of String)
        mods.Add(Visibility(type))

        If type.IsAbstract AndAlso Not type.IsSealed Then
            mods.Add("MustInherit")
        ElseIf type.IsSealed AndAlso Not type.IsAbstract Then
            mods.Add("NotInheritable")
        End If

        Return String.Join(" ", mods.ToArray()).Trim()
    End Function

    Private Function BuildMethodModifiers(m As MethodBase) As String
        Dim mods As New List(Of String)
        mods.Add(Visibility(m))

        If m.DeclaringType IsNot Nothing AndAlso m.DeclaringType.IsInterface Then
            Return mods(0)
        End If

        If m.IsStatic Then mods.Add("Shared")

        If m.IsAbstract AndAlso m.IsVirtual Then
            mods.Add("MustOverride")
        ElseIf m.IsVirtual Then
            If m.IsFinal Then
                mods.Add("NotOverridable")
            Else
                Dim baseDef As MethodInfo = Nothing
                Try : baseDef = DirectCast(m, MethodInfo).GetBaseDefinition() : Catch : baseDef = Nothing : End Try
                If baseDef IsNot Nothing AndAlso baseDef.DeclaringType IsNot Nothing AndAlso
                   m.DeclaringType IsNot Nothing AndAlso Not baseDef.DeclaringType.Equals(m.DeclaringType) Then
                    mods.Add("Overrides")
                Else
                    mods.Add("Overridable")
                End If
            End If
        End If

        Return String.Join(" ", mods.ToArray()).Trim()
    End Function

    Private Function BuildPropertyModifiers(p As PropertyInfo) As String
        Dim mods As New List(Of String)
        mods.Add(Visibility(p))
        If p.DeclaringType IsNot Nothing AndAlso p.DeclaringType.IsInterface Then
            Return mods(0)
        End If

        Dim acc As MethodInfo() = Nothing
        Try : acc = p.GetAccessors(True) : Catch : acc = Nothing : End Try
        If acc IsNot Nothing AndAlso acc.Length > 0 AndAlso acc(0).IsStatic Then
            mods.Add("Shared")
        End If

        Return String.Join(" ", mods.ToArray()).Trim()
    End Function

    Private Function BuildFieldModifiers(f As FieldInfo) As String
        Dim mods As New List(Of String)
        mods.Add(Visibility(f))

        If f.IsLiteral Then
            mods.Add("Const")
        Else
            If f.IsStatic Then mods.Add("Shared")
            If f.IsInitOnly Then mods.Add("ReadOnly")
        End If

        Return String.Join(" ", mods.ToArray()).Trim()
    End Function

    Private Function BuildEventModifiers(e As EventInfo) As String
        Dim mods As New List(Of String)
        mods.Add(Visibility(e))
        If e.DeclaringType IsNot Nothing AndAlso e.DeclaringType.IsInterface Then
            Return mods(0)
        End If

        Dim add As MethodInfo = Nothing
        Try : add = e.GetAddMethod(True) : Catch : add = Nothing : End Try
        If add IsNot Nothing AndAlso add.IsStatic Then
            mods.Add("Shared")
        End If

        Return String.Join(" ", mods.ToArray()).Trim()
    End Function

    ' ------------------------------------------------------------- visibility

    Private Function Visibility(type As Type) As String
        If type.IsPublic OrElse type.IsNestedPublic Then Return "Public"
        If type.IsNestedPrivate Then Return "Private"
        If type.IsNestedFamily Then Return "Protected"
        If type.IsNestedFamORAssem Then Return "Protected Friend"
        If type.IsNestedFamANDAssem Then Return "Protected Friend"
        Return "Friend"
    End Function

    Private Function Visibility(m As MethodBase) As String
        If m.IsPublic Then Return "Public"
        If m.IsPrivate Then Return "Private"
        If m.IsFamily Then Return "Protected"
        If m.IsFamilyOrAssembly Then Return "Protected Friend"
        If m.IsFamilyAndAssembly Then Return "Protected Friend"
        Return "Friend"
    End Function

    Private Function Visibility(f As FieldInfo) As String
        If f.IsPublic Then Return "Public"
        If f.IsPrivate Then Return "Private"
        If f.IsFamily Then Return "Protected"
        If f.IsFamilyOrAssembly Then Return "Protected Friend"
        If f.IsFamilyAndAssembly Then Return "Protected Friend"
        Return "Friend"
    End Function

    Private Function Visibility(p As PropertyInfo) As String
        Dim acc As MethodInfo() = Nothing
        Try : acc = p.GetAccessors(True) : Catch : acc = Nothing : End Try
        If acc IsNot Nothing AndAlso acc.Length > 0 Then Return Visibility(DirectCast(acc(0), MethodBase))
        Return "Public"
    End Function

    Private Function Visibility(e As EventInfo) As String
        Dim add As MethodInfo = Nothing
        Try : add = e.GetAddMethod(True) : Catch : add = Nothing : End Try
        If add IsNot Nothing Then Return Visibility(DirectCast(add, MethodBase))
        Return "Public"
    End Function

    ' ------------------------------------------------------------- type info

    Private Function ToVBTypeInfo(type As Type) As TypeInfo
        Dim info As New TypeInfo()
        If type Is Nothing Then
            Return info
        End If
        info.fullName = ToVBFullName(type)
        Try
            If type.Assembly IsNot Nothing Then
                info.assembly = type.Assembly.GetName().Name
            End If
        Catch
        End Try
        Return info
    End Function

    Private Function ToVBFullName(type As Type) As String
        If type Is Nothing Then Return ""
        If type.IsGenericParameter Then Return type.Name

        If Not type.IsGenericType Then
            Return If(type.FullName, type.Name)
        End If

        ' Rebuild a VB-style name: X(Of T, U) from the CLR generic form.
        Dim defName As String = type.GetGenericTypeDefinition().FullName
        Dim tick As Integer = defName.IndexOf("`"c)
        If tick > 0 Then defName = defName.Substring(0, tick)

        Dim args As Type() = Nothing
        Try : args = type.GetGenericArguments() : Catch : args = Nothing : End Try
        If args Is Nothing OrElse args.Length = 0 Then
            Return defName
        End If

        Dim parts As New List(Of String)
        For Each a In args
            parts.Add(ToVBFullName(a))
        Next
        Return defName & "(Of " & String.Join(", ", parts.ToArray()) & ")"
    End Function

    Private Function MapParameters(parameters As ParameterInfo()) As Dictionary(Of String, TypeInfo)
        Dim dict As New Dictionary(Of String, TypeInfo)()
        If parameters Is Nothing Then Return dict
        For Each p In parameters
            Dim name As String = If(p.Name, "arg" & dict.Count)
            dict(name) = ToVBTypeInfo(p.ParameterType)
        Next
        Return dict
    End Function

    Private Function MapGenericArgs(type As Type) As TypeInfo()
        If Not type.IsGenericType Then Return Nothing
        Dim args As Type() = Nothing
        Try : args = type.GetGenericArguments() : Catch : args = Nothing : End Try
        If args Is Nothing OrElse args.Length = 0 Then Return Nothing
        Return args.Select(Function(a) ToVBTypeInfo(a)).ToArray()
    End Function

    Private Function MapGenericArgs(method As MethodBase) As TypeInfo()
        If Not method.IsGenericMethod Then Return Nothing
        Dim args As Type() = Nothing
        Try : args = method.GetGenericArguments() : Catch : args = Nothing : End Try
        If args Is Nothing OrElse args.Length = 0 Then Return Nothing
        Return args.Select(Function(a) ToVBTypeInfo(a)).ToArray()
    End Function

    Private Function MapBaseType(baseType As Type) As TypeInfo
        If baseType Is Nothing Then Return Nothing
        Dim skip() As String = {"System.Object", "System.ValueType", "System.Enum", "System.MulticastDelegate", "System.Delegate"}
        If Array.IndexOf(skip, baseType.FullName) >= 0 Then Return Nothing
        Return ToVBTypeInfo(baseType)
    End Function

    Private Function MapInterfaces(type As Type, baseType As Type) As TypeInfo()
        Dim allIfaces As Type() = Nothing
        Try : allIfaces = type.GetInterfaces() : Catch : allIfaces = Nothing : End Try
        If allIfaces Is Nothing OrElse allIfaces.Length = 0 Then Return New TypeInfo() {}

        Dim baseIfaces As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If baseType IsNot Nothing Then
            Dim bi As Type() = Nothing
            Try : bi = baseType.GetInterfaces() : Catch : bi = Nothing : End Try
            If bi IsNot Nothing Then
                For Each b In bi
                    If b.FullName IsNot Nothing Then baseIfaces.Add(b.FullName)
                Next
            End If
        End If

        Dim result As New List(Of TypeInfo)
        For Each i In allIfaces
            If i.FullName IsNot Nothing AndAlso Not baseIfaces.Contains(i.FullName) Then
                result.Add(ToVBTypeInfo(i))
            End If
        Next
        Return result.ToArray()
    End Function

    Private Function MapAttributes(attrs As IEnumerable(Of CustomAttributeData)) As List(Of String)
        Dim list As New List(Of String)
        If attrs Is Nothing Then Return list
        For Each a In attrs
            If a.AttributeType Is Nothing Then Continue For
            Dim name As String = a.AttributeType.Name
            If name.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) AndAlso name.Length > 9 Then
                name = name.Substring(0, name.Length - 9)
            End If
            list.Add(name)
        Next
        Return list
    End Function

    ' -------------------------------------------------------------- type list

    Private Function GetAllTypes(assembly As Assembly) As Type()
        Try
            Return assembly.GetTypes()
        Catch ex As ReflectionTypeLoadException
            Return If(ex.Types, New Type() {})
        Catch
            Try
                Return assembly.DefinedTypes.Select(Function(t) DirectCast(t, Type)).ToArray()
            Catch
                Return New Type() {}
            End Try
        End Try
    End Function

End Module

End Namespace
