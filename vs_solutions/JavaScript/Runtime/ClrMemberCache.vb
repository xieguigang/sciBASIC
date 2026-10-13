#Region "Microsoft.VisualBasic::47aab6b9fdf169f6befa58488400b18a, vs_solutions\JavaScript\Runtime\ClrMemberCache.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
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
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 353
    '    Code Lines: 264 (74.79%)
    ' Comment Lines: 42 (11.90%)
    '    - Xml Docs: 80.95%
    ' 
    '   Blank Lines: 47 (13.31%)
    '     File Size: 16.10 KB


    '     Class ClrMemberSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: HasMember
    ' 
    '     Class ClrCall
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Module ClrMembers
    ' 
    '         Function: Accepts, BindArgs, Build, CacheKey, Constructors
    '                   HasAttribute, HasParamArray, IncludeMember, InstanceSet, IsEnumerableType
    '                   Iterate, Resolve, ResolveConstructor, ResolveOverload, Score
    '                   ScriptName, StaticSet, (+2 Overloads) ToBases, TryCount, WhitelistFor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Runtime

    ''' <summary>Cached reflection view of one CLR type: the members a script may reach.</summary>
    Friend NotInheritable Class ClrMemberSet

        ''' <summary>Readable/writable properties by script name (indexers excluded).</summary>
        Friend ReadOnly [Properties] As New Dictionary(Of String, PropertyInfo)
        ''' <summary>Public fields by script name.</summary>
        Friend ReadOnly Fields As New Dictionary(Of String, FieldInfo)
        ''' <summary>Method overloads by script name (property accessors excluded).</summary>
        Friend ReadOnly Methods As New Dictionary(Of String, List(Of MethodInfo))
        ''' <summary>Indexer properties (<c>Item</c> with parameters).</summary>
        Friend ReadOnly Indexers As New List(Of PropertyInfo)
        ''' <summary>Data-member names in declaration order — for-in / Object.keys.</summary>
        Friend ReadOnly Names As New List(Of String)
        ''' <summary>True when the wrapped type implements <see cref="System.Collections.IEnumerable"/>.</summary>
        Friend ReadOnly IsEnumerable As Boolean

        Friend Sub New(isEnumerable As Boolean)
            Me.IsEnumerable = isEnumerable
        End Sub

        Friend Function HasMember(name As String) As Boolean
            Return [Properties].ContainsKey(name) OrElse Fields.ContainsKey(name) OrElse Methods.ContainsKey(name)
        End Function

    End Class

    ''' <summary>A resolved call: the chosen method/constructor plus its bound argument array.</summary>
    Friend NotInheritable Class ClrCall

        Friend ReadOnly Info As MethodBase
        Friend ReadOnly Args As Object()

        Friend Sub New(info As MethodBase, args As Object())
            Me.Info = info
            Me.Args = args
        End Sub

    End Class

    ''' <summary>
    ''' Reflection scan, member cache and overload resolution behind the CLR bridge.
    ''' <para>
    ''' One scan per type (and per static/instance view) builds the member dictionaries
    ''' once, so the hot path is a dictionary lookup instead of a
    ''' <c>GetProperties()</c>/<c>GetMethods()</c> sweep. Visibility rules are applied
    ''' while scanning: <see cref="JsIgnoreAttribute"/> hides a member,
    ''' <see cref="JsNameAttribute"/> renames it, and a type carrying at least one
    ''' <see cref="JsExposeAttribute"/> member switches to whitelist mode (only marked
    ''' members stay visible).
    ''' </para>
    ''' </summary>
    Friend Module ClrMembers

        Private ReadOnly _instanceSets As New Dictionary(Of String, ClrMemberSet)
        Private ReadOnly _staticSets As New Dictionary(Of String, ClrMemberSet)
        Private ReadOnly _constructors As New Dictionary(Of String, List(Of ConstructorInfo))
        Private ReadOnly _gate As New Object

        ' ======================================================== cache

        ''' <summary>
        ''' Cache key of one reflection view. <see cref="ClrInterop.ExplicitOnly"/> takes
        ''' part in it, so flipping the global whitelist switch never serves a view that
        ''' was scanned under the previous policy.
        ''' </summary>
        Private Function CacheKey(t As Type) As String
            Return t.AssemblyQualifiedName & "|" & If(ClrInterop.ExplicitOnly, "explicit", "default")
        End Function

        Friend Function InstanceSet(t As Type) As ClrMemberSet
            Dim key = CacheKey(t)
            SyncLock _gate
                Dim cached As ClrMemberSet = Nothing
                If _instanceSets.TryGetValue(key, cached) Then Return cached
                Dim built = Build(t, isStatic:=False)
                _instanceSets(key) = built
                Return built
            End SyncLock
        End Function

        Friend Function StaticSet(t As Type) As ClrMemberSet
            Dim key = CacheKey(t)
            SyncLock _gate
                Dim cached As ClrMemberSet = Nothing
                If _staticSets.TryGetValue(key, cached) Then Return cached
                Dim built = Build(t, isStatic:=True)
                _staticSets(key) = built
                Return built
            End SyncLock
        End Function

        Friend Function Constructors(t As Type) As List(Of ConstructorInfo)
            Dim key = CacheKey(t)
            SyncLock _gate
                Dim cached As List(Of ConstructorInfo) = Nothing
                If _constructors.TryGetValue(key, cached) Then Return cached
                Dim whitelist = WhitelistFor(t)
                Dim list As New List(Of ConstructorInfo)
                For Each ci In t.GetConstructors(BindingFlags.Public Or BindingFlags.Instance)
                    If IncludeMember(ci, whitelist) Then list.Add(ci)
                Next
                _constructors(key) = list
                Return list
            End SyncLock
        End Function

        ' ======================================================== scan

        Private Function Build(t As Type, isStatic As Boolean) As ClrMemberSet
            Dim members As New ClrMemberSet(IsEnumerableType(t))
            Dim flags As BindingFlags = BindingFlags.Public Or
                                        If(isStatic, BindingFlags.Static, BindingFlags.Instance)
            Dim whitelist = WhitelistFor(t)

            For Each p In t.GetProperties(flags)
                If p.GetIndexParameters().Length > 0 Then
                    members.Indexers.Add(p)
                    Continue For
                End If
                If Not IncludeMember(p, whitelist) Then Continue For
                Dim name = ScriptName(p, p.Name)
                If members.Properties.ContainsKey(name) Then Continue For
                members.Properties(name) = p
                members.Names.Add(name)
            Next

            For Each f In t.GetFields(flags)
                If Not IncludeMember(f, whitelist) Then Continue For
                Dim name = ScriptName(f, f.Name)
                If members.Properties.ContainsKey(name) OrElse members.Fields.ContainsKey(name) Then Continue For
                members.Fields(name) = f
                members.Names.Add(name)
            Next

            For Each m In t.GetMethods(flags)
                ' special names are property/event accessors and operators: not script members
                If m.IsSpecialName Then Continue For
                If Not IncludeMember(m, whitelist) Then Continue For
                Dim name = ScriptName(m, m.Name)
                Dim candidates As List(Of MethodInfo) = Nothing
                If Not members.Methods.TryGetValue(name, candidates) Then
                    candidates = New List(Of MethodInfo)
                    members.Methods(name) = candidates
                End If
                candidates.Add(m)
            Next

            Return members
        End Function

        Friend Function IsEnumerableType(t As Type) As Boolean
            If t Is GetType(String) Then Return False
            Return GetType(System.Collections.IEnumerable).IsAssignableFrom(t)
        End Function

        ''' <summary>True when the type must be exposed selectively (whitelist mode).</summary>
        Private Function WhitelistFor(t As Type) As Boolean
            If ClrInterop.ExplicitOnly Then Return True
            For Each m In t.GetMembers(BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.Static)
                If HasAttribute(m, GetType(JsExposeAttribute)) Then Return True
            Next
            Return False
        End Function

        Private Function IncludeMember(mi As MemberInfo, whitelist As Boolean) As Boolean
            If HasAttribute(mi, GetType(JsIgnoreAttribute)) Then Return False
            If whitelist Then Return HasAttribute(mi, GetType(JsExposeAttribute))
            Return True
        End Function

        Private Function HasAttribute(mi As MemberInfo, attrType As Type) As Boolean
            Return mi.GetCustomAttributes(attrType, True).Length > 0
        End Function

        ''' <summary>The script-visible name of a member ([JsName] wins, exact match otherwise).</summary>
        Private Function ScriptName(mi As MemberInfo, fallback As String) As String
            For Each a In mi.GetCustomAttributes(GetType(JsNameAttribute), True)
                Dim n = TryCast(a, JsNameAttribute)
                If n IsNot Nothing AndAlso Not String.IsNullOrEmpty(n.Name) Then Return n.Name
            Next
            Return fallback
        End Function

        ' ======================================================== enumeration

        ''' <summary>Element count of an enumerable/array target, if it has one.</summary>
        Friend Function TryCount(target As Object, ByRef count As Double) As Boolean
            Dim arr = TryCast(target, Array)
            If arr IsNot Nothing Then
                count = CDbl(arr.Length)
                Return True
            End If
            Dim collection = TryCast(target, System.Collections.ICollection)
            If collection IsNot Nothing Then
                count = CDbl(collection.Count)
                Return True
            End If
            If Not IsEnumerableType(target.GetType()) Then Return False

            ' ICollection(Of T).Count — looked up by reflection so no Linq dependency
            Try
                Dim p = target.GetType().GetProperty("Count", BindingFlags.Public Or BindingFlags.Instance)
                If p IsNot Nothing AndAlso ClrConvert.IsNumericType(p.PropertyType) AndAlso p.GetIndexParameters().Length = 0 Then
                    count = CDbl(p.GetValue(target))
                    Return True
                End If
            Catch ex As Exception
                ' fall through to a manual count
            End Try

            Dim n = 0
            For Each el In DirectCast(target, System.Collections.IEnumerable)
                n += 1
            Next
            count = CDbl(n)
            Return True
        End Function

        ''' <summary>Materialise the elements of an enumerable target as script values.</summary>
        Friend Function Iterate(target As Object) As List(Of Object)
            Dim result As New List(Of Object)
            Dim seq = TryCast(target, System.Collections.IEnumerable)
            If seq Is Nothing Then Return result
            For Each el In seq
                result.Add(ClrConvert.ToJs(el))
            Next
            Return result
        End Function

        ' ======================================================== overload resolution

        ''' <summary>Pick the best method overload for the actual argument values.</summary>
        Friend Function ResolveOverload(candidates As List(Of MethodInfo), args As Object(), name As String) As ClrCall
            Return Resolve(ToBases(candidates), args, name)
        End Function

        ''' <summary>Pick the best constructor overload for the actual argument values.</summary>
        Friend Function ResolveConstructor(candidates As List(Of ConstructorInfo), args As Object(), typeName As String) As ClrCall
            Return Resolve(ToBases(candidates), args, typeName)
        End Function

        Private Function Resolve(candidates As List(Of MethodBase), args As Object(), name As String) As ClrCall
            Dim best As MethodBase = Nothing
            Dim bestScore = Integer.MaxValue
            For Each m In candidates
                If Not Accepts(m, args.Length) Then Continue For
                Dim s = Score(m, args)
                If s < 0 Then Continue For
                ' ties keep declaration order (most-derived overload first)
                If s < bestScore Then
                    bestScore = s
                    best = m
                End If
            Next
            If best Is Nothing Then Throw ClrConvert.NoOverload(name, candidates, args)
            Return New ClrCall(best, BindArgs(best, args))
        End Function

        Private Function ToBases(methods As List(Of MethodInfo)) As List(Of MethodBase)
            Dim list As New List(Of MethodBase)
            For Each m In methods
                list.Add(m)
            Next
            Return list
        End Function

        Private Function ToBases(ctors As List(Of ConstructorInfo)) As List(Of MethodBase)
            Dim list As New List(Of MethodBase)
            For Each c In ctors
                list.Add(c)
            Next
            Return list
        End Function

        ''' <summary>Can this member be called with <paramref name="argCount"/> arguments?</summary>
        Private Function Accepts(m As MethodBase, argCount As Integer) As Boolean
            Dim ps = m.GetParameters()
            If ps.Length = 0 Then Return argCount = 0
            If HasParamArray(ps(ps.Length - 1)) Then Return argCount >= ps.Length - 1
            Dim required = 0
            For Each p In ps
                If Not p.IsOptional Then required += 1
            Next
            Return argCount >= required AndAlso argCount <= ps.Length
        End Function

        ''' <summary>Total conversion cost of the call, or −1 when some argument cannot convert.</summary>
        Private Function Score(m As MethodBase, args As Object()) As Integer
            Dim ps = m.GetParameters()
            Dim isParamArray = ps.Length > 0 AndAlso HasParamArray(ps(ps.Length - 1))
            Dim total = 0
            For i = 0 To args.Length - 1
                Dim target As Type
                If isParamArray AndAlso i >= ps.Length - 1 Then
                    target = ps(ps.Length - 1).ParameterType.GetElementType()
                Else
                    target = ps(i).ParameterType
                End If
                Dim s = ClrConvert.ConvertScore(args(i), target)
                If s < 0 Then Return -1
                total += s
            Next
            Return total
        End Function

        ''' <summary>Convert the script arguments into the parameter array the member expects.</summary>
        Private Function BindArgs(m As MethodBase, args As Object()) As Object()
            Dim ps = m.GetParameters()
            If ps.Length = 0 Then Return New Object() {}

            Dim bound(ps.Length - 1) As Object
            For i = 0 To ps.Length - 1
                If i < args.Length Then
                    bound(i) = ClrConvert.ToClr(args(i), ps(i).ParameterType)
                ElseIf ps(i).IsOptional Then
                    Dim dv = ps(i).DefaultValue
                    If dv Is DBNull.Value OrElse dv Is Missing.Value OrElse
                       (dv Is Nothing AndAlso ps(i).ParameterType.IsValueType) Then
                        dv = ClrConvert.DefaultValue(ps(i).ParameterType)
                    End If
                    bound(i) = dv
                Else
                    bound(i) = ClrConvert.DefaultValue(ps(i).ParameterType)
                End If
            Next

            Dim last = ps(ps.Length - 1)
            If HasParamArray(last) Then
                Dim elem = last.ParameterType.GetElementType()
                Dim extra As New List(Of Object)
                For i = ps.Length - 1 To args.Length - 1
                    extra.Add(ClrConvert.ToClr(args(i), elem))
                Next
                Dim arr = Array.CreateInstance(elem, extra.Count)
                For i = 0 To extra.Count - 1
                    arr.SetValue(extra(i), i)
                Next
                bound(ps.Length - 1) = arr
            End If
            Return bound
        End Function

        Private Function HasParamArray(p As ParameterInfo) As Boolean
            Return p.GetCustomAttributes(GetType(ParamArrayAttribute), False).Length > 0
        End Function

    End Module

End Namespace

