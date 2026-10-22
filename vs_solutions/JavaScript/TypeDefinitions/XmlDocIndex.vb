#Region "Microsoft.VisualBasic::1f5d8b03c7e94a26d0b8f5c3a71e9420, vs_solutions\JavaScript\TypeDefinitions\XmlDocIndex.vb"

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

    '     Class XmlDocIndex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: MemberDoc, TypeDoc

    '     Module XmlDocLoader
    ' 
    '         Function: TryLoad

    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection

Imports doc = Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Namespace TypeDefinitions

    ''' <summary>
    ''' Indexed view of one assembly XML documentation file, loaded through the
    ''' <c>ApplicationServices.Development.XmlDoc</c> model
    ''' (<see cref="doc.ProjectSpace.CreateDocProject"/>), so the very same parser the
    ''' rest of sciBASIC# uses produces the JSDoc source material.
    ''' <para>
    ''' Reflection members are matched against the compiler documentation naming
    ''' convention (<c>M:Ns.Type.Name(System.Int32,...)</c>, generic instances rendered
    ''' with curly braces, arrays with <c>[]</c>, ByRef with <c>&amp;</c>) with three
    ''' fallback levels: exact signature, name + parameter count, name only.
    ''' </para>
    ''' </summary>
    Friend Class XmlDocIndex

        Friend ReadOnly byFullName As New Dictionary(Of String, doc.ProjectType)(StringComparer.OrdinalIgnoreCase)
        Friend ReadOnly byShortName As New Dictionary(Of String, List(Of doc.ProjectType))(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Load and index one XML documentation file. Returns Nothing when the file is
        ''' missing, unreadable or not an assembly documentation — a broken document must
        ''' never abort the generation.
        ''' </summary>
        Friend Shared Function TryLoad(path As String) As XmlDocIndex
            If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing

            Try
                Dim project As doc.Project = doc.ProjectSpace.CreateDocProject(path, excludeVBSpecific:=True)

                If project Is Nothing Then Return Nothing

                Dim index As New XmlDocIndex

                For Each ns As doc.ProjectNamespace In project.Namespaces
                    For Each pt As doc.ProjectType In ns.Types
                        Dim key As String = If(String.IsNullOrEmpty(ns.fullName), pt.Name, ns.fullName & "." & pt.Name)

                        If Not index.byFullName.ContainsKey(key) Then
                            index.byFullName(key) = pt
                        End If

                        Dim shortName As String = pt.Name
                        Dim plus As Integer = shortName.IndexOf("+"c)

                        If plus > 0 Then shortName = shortName.Substring(0, plus)

                        Dim bucket As List(Of doc.ProjectType) = Nothing

                        If Not index.byShortName.TryGetValue(shortName, bucket) Then
                            bucket = New List(Of doc.ProjectType)
                            index.byShortName(shortName) = bucket
                        End If

                        bucket.Add(pt)
                    Next
                Next

                Return index
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>The documentation of one type, Nothing when undocumented.</summary>
        Friend Function TypeDoc(fullName As String) As doc.ProjectType
            If String.IsNullOrEmpty(fullName) Then Return Nothing

            Dim pt As doc.ProjectType = Nothing

            If byFullName.TryGetValue(fullName, pt) Then Return pt

            ' nested types and generic arity suffixes may not line up: fall back to
            ' the short name, but only when it is unambiguous
            Dim lastDot As Integer = fullName.LastIndexOf("."c)

            If lastDot > 0 Then
                Dim bucket As List(Of doc.ProjectType) = Nothing

                If byShortName.TryGetValue(fullName.Substring(lastDot + 1), bucket) AndAlso bucket.Count = 1 Then
                    Return bucket(0)
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>The documentation of one member, Nothing when undocumented.</summary>
        Friend Function MemberDoc(mi As MemberInfo) As doc.ProjectMember
            If mi Is Nothing Then Return Nothing

            Dim declaring As Type = mi.DeclaringType

            If declaring Is Nothing OrElse declaring.FullName Is Nothing Then Return Nothing

            Dim pt As doc.ProjectType = TypeDoc(declaring.FullName)

            If pt Is Nothing Then Return Nothing

            Dim key As String = MemberKey(mi)

            If key Is Nothing Then Return Nothing

            ' 1) exact signature match
            Dim exact As doc.ProjectMember = FindExact(pt, key)

            If exact IsNot Nothing Then Return exact

            ' 2) name + parameter count
            Dim name As String = MemberShortName(mi)
            Dim count As Integer = ParameterCount(mi)
            Dim table As IEnumerable(Of doc.ProjectMember) = MemberTable(pt, mi.MemberType)

            If table IsNot Nothing AndAlso count >= 0 Then
                Dim matched = table _
                    .Where(Function(pm) String.Equals(pm.Name, name, StringComparison.OrdinalIgnoreCase)) _
                    .Where(Function(pm) ParamCountOf(pm) = count) _
                    .FirstOrDefault()

                If matched IsNot Nothing Then Return matched
            End If

            ' 3) name only (first overload)
            If table IsNot Nothing Then
                Dim named = table _
                    .Where(Function(pm) String.Equals(pm.Name, name, StringComparison.OrdinalIgnoreCase)) _
                    .FirstOrDefault()

                If named IsNot Nothing Then Return named
            End If

            Return Nothing
        End Function

        ' ======================================================== matching

        Private Function FindExact(pt As doc.ProjectType, key As String) As doc.ProjectMember
            Dim tables As IEnumerable(Of doc.ProjectMember)() = {pt.AllMethods, pt.AllProperties, pt.AllFields}

            For Each table As IEnumerable(Of doc.ProjectMember) In tables
                If table Is Nothing Then Continue For

                For Each pm As doc.ProjectMember In table
                    If String.Equals(pm.[Declare], key, StringComparison.Ordinal) Then Return pm
                Next
            Next

            Return Nothing
        End Function

        Private Function MemberTable(pt As doc.ProjectType, kind As MemberTypes) As IEnumerable(Of doc.ProjectMember)
            Select Case kind
                Case MemberTypes.Method, MemberTypes.Constructor
                    Return pt.AllMethods
                Case MemberTypes.Property
                    Return pt.AllProperties
                Case MemberTypes.Field
                    Return pt.AllFields
                Case Else
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' The documentation name of a member minus its kind prefix, e.g.
        ''' ``Ns.Type.Add(System.Int32,System.String)``.
        ''' </summary>
        Private Function MemberKey(mi As MemberInfo) As String
            Dim dt As Type = mi.DeclaringType

            If dt Is Nothing OrElse dt.FullName Is Nothing Then Return Nothing

            Dim head As String = dt.FullName & "."

            Select Case mi.MemberType
                Case MemberTypes.Property
                    Return head & DirectCast(mi, PropertyInfo).Name
                Case MemberTypes.Field
                    Return head & DirectCast(mi, FieldInfo).Name
                Case MemberTypes.Constructor
                    Return head & "#ctor" & ParamList(DirectCast(mi, ConstructorInfo))
                Case MemberTypes.Method
                    Dim m As MethodInfo = DirectCast(mi, MethodInfo)
                    Dim name As String = m.Name

                    If m.IsGenericMethodDefinition Then
                        name &= "``" & m.GetGenericArguments().Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    End If

                    Return head & name & ParamList(m)
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function MemberShortName(mi As MemberInfo) As String
            Select Case mi.MemberType
                Case MemberTypes.Property
                    Return DirectCast(mi, PropertyInfo).Name
                Case MemberTypes.Field
                    Return DirectCast(mi, FieldInfo).Name
                Case MemberTypes.Method
                    Return DirectCast(mi, MethodInfo).Name
                Case MemberTypes.Constructor
                    Return "#ctor"
                Case Else
                    Return mi.Name
            End Select
        End Function

        Private Function ParameterCount(mi As MemberInfo) As Integer
            Select Case mi.MemberType
                Case MemberTypes.Property
                    Return DirectCast(mi, PropertyInfo).GetIndexParameters().Length
                Case MemberTypes.Constructor
                    Return DirectCast(mi, ConstructorInfo).GetParameters().Length
                Case MemberTypes.Method
                    Return DirectCast(mi, MethodInfo).GetParameters().Length
                Case Else
                    Return -1
            End Select
        End Function

        Private Function ParamList(m As MethodBase) As String
            Dim ps As ParameterInfo() = m.GetParameters()

            If ps.Length = 0 Then Return "()"

            Return "(" & String.Join(",", ps.Select(Function(p) DocParamName(p.ParameterType))) & ")"
        End Function

        ''' <summary>Render a type the way the compiler writes it into XML documents.</summary>
        Private Function DocParamName(t As Type) As String
            If t Is Nothing Then Return ""
            If t.IsByRef Then Return DocParamName(t.GetElementType()) & "&"
            If t.IsPointer Then Return DocParamName(t.GetElementType()) & "*"

            If t.IsArray Then
                Return DocParamName(t.GetElementType()) & New String(","c, t.GetArrayRank() - 1) & "[]"
            End If

            If t.IsGenericType Then
                Dim definition As Type = t.GetGenericTypeDefinition()

                If definition.FullName Is Nothing Then Return t.Name

                Dim args = t.GetGenericArguments().Select(Function(a) DocParamName(a))

                Return TsNaming.StripArity(definition.FullName) & "{" & String.Join(",", args) & "}"
            End If

            ' a generic parameter carries no full name
            If t.FullName Is Nothing Then Return t.Name

            Return t.FullName
        End Function

        ''' <summary>Parameter count of a documented member, read from its declared signature.</summary>
        Private Function ParamCountOf(pm As doc.ProjectMember) As Integer
            Dim [declare] As String = pm.[Declare]

            If String.IsNullOrEmpty([declare]) Then Return -1

            Dim open As Integer = [declare].IndexOf("("c)

            If open < 0 Then Return -1

            Dim close As Integer = [declare].LastIndexOf(")"c)

            If close <= open + 1 Then Return 0

            Dim inner As String = [declare].Substring(open + 1, close - open - 1)
            Dim depth As Integer = 0
            Dim count As Integer = 1

            For Each ch As Char In inner
                Select Case ch
                    Case "{"c, "["c
                        depth += 1
                    Case "}"c, "]"c
                        depth -= 1
                    Case ","c
                        If depth = 0 Then count += 1
                End Select
            Next

            Return count
        End Function

    End Class

End Namespace
