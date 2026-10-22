#Region "Microsoft.VisualBasic::4e6b8c13f7a25d09b3c4e8f1a60d5729, vs_solutions\JavaScript\TypeDefinitions\AssemblyScanner.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public LICENSE for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Module AssemblyScanner
    ' 
    '         Function: Scan

    '     Class ScanResult
    ' 
    '         Properties: ExportNames, Types

    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

Namespace TypeDefinitions

    ''' <summary>The outcome of one assembly scan.</summary>
    Friend Class ScanResult

        ''' <summary>The scanned type models, sorted by full name.</summary>
        Friend Property Types As List(Of TsType)
        ''' <summary>Full names of every exported type, for the type mapper.</summary>
        Friend Property ExportNames As HashSet(Of String)

    End Class

    ''' <summary>
    ''' Reflects one CLR assembly into the <see cref="TsType"/> model.
    ''' <para>
    ''' Member visibility is taken from the very same <see cref="Runtime.ClrMembers"/>
    ''' views the script bridge uses (instance view, static view and constructors),
    ''' so <c>[JsName]</c> renames, <c>[JsIgnore]</c> hiding, <c>[JsExpose]</c>
    ''' whitelisting, special-name skipping and indexer separation can never drift
    ''' from runtime behaviour. Only the type-level enumeration is done here.
    ''' </para>
    ''' </summary>
    Friend Module AssemblyScanner

        Friend Function Scan(asm As Assembly, options As TypeDefinitionOptions) As ScanResult
            Dim candidates As List(Of Type) = CollectTypes(asm, options)
            Dim exports As New HashSet(Of String)(StringComparer.Ordinal)

            For Each t As Type In candidates
                If t.FullName IsNot Nothing Then exports.Add(t.FullName)
            Next

            ' ClrMembers caches per (assembly-qualified name | policy), so temporarily
            ' borrowing the global switch cannot pollute a previously built cache.
            Dim previous As Boolean = ClrInterop.ExplicitOnly
            ClrInterop.ExplicitOnly = options.ExplicitOnly

            Try
                Dim models As New List(Of TsType)

                For Each t As Type In candidates
                    Dim model As TsType = Nothing

                    Try
                        model = BuildType(t, exports)
                    Catch ex As Exception
                        ' a single unbuildable type must not abort the whole file
                        model = Nothing
                    End Try

                    If model IsNot Nothing Then models.Add(model)
                Next

                Call LinkNested(models)

                Return New ScanResult With {.Types = models, .ExportNames = exports}
            Finally
                ClrInterop.ExplicitOnly = previous
            End Try
        End Function

        ' ======================================================== type level

        Private Function CollectTypes(asm As Assembly, options As TypeDefinitionOptions) As List(Of Type)
            Dim all As Type()

            Try
                all = asm.GetTypes()
            Catch ex As ReflectionTypeLoadException
                ' keep whatever could be loaded from an assembly with missing dependencies
                all = ex.Types.Where(Function(t) t IsNot Nothing).ToArray()
            End Try

            Return all.Where(Function(t) IncludeType(t, options)) _
                      .OrderBy(Function(t) t.FullName, StringComparer.Ordinal) _
                      .ToList()
        End Function

        Private Function IncludeType(t As Type, options As TypeDefinitionOptions) As Boolean
            If t Is Nothing Then Return False
            If t.FullName Is Nothing Then Return False
            If t.IsGenericParameter Then Return False
            If Not options.IncludeNonPublic AndAlso Not t.IsVisible Then Return False
            If HasAttribute(t, GetType(CompilerGeneratedAttribute)) Then Return False
            ' VB anonymous types, lambda closures and iterator display classes
            If t.Name.IndexOf("<"c) >= 0 Then Return False

            Return True
        End Function

        Private Function BuildType(t As Type, exports As HashSet(Of String)) As TsType
            Dim model As New TsType With {
                .ClrType = t,
                .TsNamespace = If(t.Namespace, ""),
                .ShortName = TsNaming.StripArity(t.Name),
                .Kind = DetectKind(t),
                .IsAbstract = t.IsAbstract,
                .IsSealed = t.IsSealed,
                .IsObsolete = IsObsolete(t)
            }

            If t.IsGenericType Then
                model.GenericParameters = t.GetGenericArguments() _
                    .Where(Function(g) g.IsGenericParameter) _
                    .Select(Function(g) TsNaming.Sanitize(g.Name)) _
                    .ToList()
            End If

            Select Case model.Kind
                Case TsTypeKind.EnumType
                    model.EnumMembers = ReadEnumMembers(t)
                    Return model
                Case TsTypeKind.DelegateType
                    model.Invoke = ReadDelegateInvoke(t)
                    Return model
            End Select

            ' base class: only a same-assembly base becomes a TypeScript extends
            If t.BaseType IsNot Nothing Then
                Select Case t.BaseType.FullName
                    Case "System.Object", "System.ValueType", "System.Enum",
                         "System.MulticastDelegate", "System.Delegate"
                        ' no TypeScript extends for these
                    Case Else
                        If exports.Contains(t.BaseType.FullName) Then model.BaseType = t.BaseType
                End Select
            End If

            ' interfaces: drop the ones the base type already implements
            If model.Kind = TsTypeKind.InterfaceType Then
                model.Interfaces = t.GetInterfaces().ToList()
            Else
                Dim inherited As IEnumerable(Of Type) =
                    If(t.BaseType Is Nothing, New Type() {}, t.BaseType.GetInterfaces())

                model.Interfaces = t.GetInterfaces() _
                    .Where(Function(i) Not inherited.Contains(i)) _
                    .ToList()
            End If

            Call AddMembers(model, t)

            Return model
        End Function

        Private Sub LinkNested(all As List(Of TsType))
            Dim byFullName As New Dictionary(Of String, TsType)(StringComparer.Ordinal)

            For Each model In all
                If model.ClrType.FullName IsNot Nothing Then
                    byFullName(model.ClrType.FullName) = model
                End If
            Next

            For Each model In all
                Dim declaring As Type = model.ClrType.DeclaringType

                If declaring Is Nothing OrElse declaring.FullName Is Nothing Then Continue For

                Dim parent As TsType = Nothing

                If byFullName.TryGetValue(declaring.FullName, parent) Then
                    model.Parent = parent
                    parent.Nested.Add(model)
                End If
            Next
        End Sub

        Private Function DetectKind(t As Type) As TsTypeKind
            If t.IsEnum Then Return TsTypeKind.EnumType
            If t.IsInterface Then Return TsTypeKind.InterfaceType
            If GetType(System.Delegate).IsAssignableFrom(t) Then Return TsTypeKind.DelegateType
            If t.IsValueType Then Return TsTypeKind.StructType
            ' VB Module and C# static class: abstract + sealed, all members static
            If t.IsAbstract AndAlso t.IsSealed Then Return TsTypeKind.ModuleType

            Return TsTypeKind.ClassType
        End Function

        Private Function ReadEnumMembers(t As Type) As List(Of KeyValuePair(Of String, Long))
            Dim names As String() = [Enum].GetNames(t)
            Dim values As Array = [Enum].GetValues(t)
            Dim list As New List(Of KeyValuePair(Of String, Long))

            For i As Integer = 0 To names.Length - 1
                list.Add(New KeyValuePair(Of String, Long)(
                    names(i), Convert.ToInt64(values.GetValue(i), System.Globalization.CultureInfo.InvariantCulture)))
            Next

            Return list
        End Function

        Private Function ReadDelegateInvoke(t As Type) As TsMethod
            Dim invoke As MethodInfo = t.GetMethod("Invoke")

            If invoke Is Nothing Then Return Nothing

            Return New TsMethod With {
                .Name = "Invoke",
                .ReturnType = invoke.ReturnType,
                .Parameters = ReadParameters(invoke),
                .Info = invoke
            }
        End Function

        ' ======================================================== member level

        Private Sub AddMembers(model As TsType, t As Type)
            Dim instance As ClrMemberSet = ClrMembers.InstanceSet(t)
            Dim [shared] As ClrMemberSet = ClrMembers.StaticSet(t)

            ' ---- properties: instance view first, then statics that do not collide
            For Each kv In instance.Properties.Where(Function(x) UsableName(x.Key))
                model.Properties.Add(New TsProperty With {
                    .Name = kv.Key,
                    .PropertyType = kv.Value.PropertyType,
                    .CanRead = kv.Value.CanRead,
                    .CanWrite = kv.Value.CanWrite,
                    .IsStatic = False,
                    .Info = kv.Value
                })
            Next

            For Each kv In [shared].Properties.Where(Function(x) UsableName(x.Key))
                If model.Properties.Any(Function(p) String.Equals(p.Name, kv.Key, StringComparison.Ordinal)) Then
                    Continue For
                End If

                model.Properties.Add(New TsProperty With {
                    .Name = kv.Key,
                    .PropertyType = kv.Value.PropertyType,
                    .CanRead = kv.Value.CanRead,
                    .CanWrite = kv.Value.CanWrite,
                    .IsStatic = True,
                    .Info = kv.Value
                })
            Next

            model.Properties = model.Properties _
                .OrderBy(Function(p) p.Name, StringComparer.OrdinalIgnoreCase) _
                .ToList()

            ' ---- fields (a field never shadows a same-named property, matching the bridge)
            For Each kv In instance.Fields.Where(Function(x) UsableName(x.Key))
                AddField(model, kv.Key, kv.Value, isStatic:=False)
            Next

            For Each kv In [shared].Fields.Where(Function(x) UsableName(x.Key))
                AddField(model, kv.Key, kv.Value, isStatic:=True)
            Next

            model.Fields = model.Fields _
                .OrderBy(Function(f) f.Name, StringComparer.OrdinalIgnoreCase) _
                .ToList()

            ' ---- methods (every overload becomes one signature)
            For Each kv In instance.Methods.Where(Function(x) UsableName(x.Key))
                For Each m As MethodInfo In kv.Value
                    model.Methods.Add(ReadMethod(kv.Key, m, isStatic:=False))
                Next
            Next

            For Each kv In [shared].Methods.Where(Function(x) UsableName(x.Key))
                For Each m As MethodInfo In kv.Value
                    model.Methods.Add(ReadMethod(kv.Key, m, isStatic:=True))
                Next
            Next

            ' ---- constructors
            For Each ci As ConstructorInfo In ClrMembers.Constructors(t)
                model.Constructors.Add(New TsConstructor With {
                    .Parameters = ReadParameters(ci),
                    .Info = ci
                })
            Next

            ' ---- single-parameter indexers (the only form the bridge reaches)
            For Each p As PropertyInfo In instance.Indexers
                Dim ps As ParameterInfo() = p.GetIndexParameters()

                If ps.Length <> 1 Then Continue For
                If model.Indexers.Any(Function(x) x.KeyType Is ps(0).ParameterType) Then Continue For

                model.Indexers.Add(New TsIndexer With {
                    .KeyType = ps(0).ParameterType,
                    .ValueType = p.PropertyType,
                    .CanRead = p.CanRead,
                    .CanWrite = p.CanWrite
                })
            Next
        End Sub

        Private Sub AddField(model As TsType, name As String, f As FieldInfo, isStatic As Boolean)
            If model.Properties.Any(Function(p) String.Equals(p.Name, name, StringComparison.Ordinal)) Then
                Return
            End If

            If model.Fields.Any(Function(x) String.Equals(x.Name, name, StringComparison.Ordinal)) Then
                Return
            End If

            model.Fields.Add(New TsField With {
                .Name = name,
                .FieldType = f.FieldType,
                .IsStatic = isStatic,
                .IsReadOnly = f.IsInitOnly,
                .IsLiteral = f.IsLiteral,
                .Info = f
            })
        End Sub

        Private Function ReadMethod(name As String, m As MethodInfo, isStatic As Boolean) As TsMethod
            Dim generics As List(Of String) = Nothing

            If m.IsGenericMethod Then
                generics = m.GetGenericArguments() _
                    .Select(Function(g) TsNaming.Sanitize(g.Name)) _
                    .ToList()
            End If

            Return New TsMethod With {
                .Name = name,
                .ReturnType = m.ReturnType,
                .Parameters = ReadParameters(m),
                .IsStatic = isStatic,
                .GenericArguments = generics,
                .Info = m
            }
        End Function

        Private Function ReadParameters(m As MethodBase) As List(Of TsParameter)
            Dim ps As ParameterInfo() = m.GetParameters()
            Dim list As New List(Of TsParameter)

            For i As Integer = 0 To ps.Length - 1
                Dim p As ParameterInfo = ps(i)

                list.Add(New TsParameter With {
                    .Name = If(String.IsNullOrEmpty(p.Name), "arg" & i.ToString(System.Globalization.CultureInfo.InvariantCulture), p.Name),
                    .Type = p.ParameterType,
                    .IsOptional = p.IsOptional,
                    .IsParamArray = p.GetCustomAttributes(GetType(ParamArrayAttribute), False).Length > 0,
                    .IsByRef = p.ParameterType.IsByRef,
                    .HasDefault = p.HasDefaultValue,
                    .DefaultValue = If(p.HasDefaultValue, p.DefaultValue, Nothing)
                })
            Next

            Return list
        End Function

        ' ======================================================== helpers

        ''' <summary>
        ''' A member name is expressible with TypeScript dot syntax only when it carries
        ''' no namespace/nesting/generic punctuation.
        ''' </summary>
        Private Function UsableName(name As String) As Boolean
            If String.IsNullOrEmpty(name) Then Return False

            Return name.IndexOf("."c) < 0 AndAlso
                   name.IndexOf("+"c) < 0 AndAlso
                   name.IndexOf("<"c) < 0
        End Function

        Private Function HasAttribute(mi As MemberInfo, attrType As Type) As Boolean
            Return mi.GetCustomAttributes(attrType, True).Length > 0
        End Function

        Private Function IsObsolete(mi As MemberInfo) As Boolean
            Return HasAttribute(mi, GetType(ObsoleteAttribute))
        End Function

    End Module

End Namespace
