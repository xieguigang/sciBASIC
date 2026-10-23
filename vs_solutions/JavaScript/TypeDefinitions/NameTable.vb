#Region "Microsoft.VisualBasic::f1c2d34e5a6b7089c1d2e3f4a5b6c7d8, vs_solutions\JavaScript\TypeDefinitions\NameTable.vb"

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
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Class NameTable
    ' 
    '         Properties: (none)
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: IsExported, Path, SimpleName
    ' 
    '     Module (helpers): ScopeKey, Compose
    ' 
    '     Class EmitContext
    ' 
    '         Properties: Names
    ' 
    '         Function: ModelOf
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Reflection

Namespace TypeDefinitions

    ''' <summary>
    ''' The emitted TypeScript names of the scanned types.
    ''' <para>
    ''' .NET lets one namespace hold ``Foo`` and ``Foo(Of T)`` at the same time
    ''' (different generic arity), which TypeScript cannot declare — the second
    ''' declaration of a name block gets a numeric suffix here. Every reference in
    ''' the generated file (signatures, extends/implements clauses, global aliases)
    ''' reads its name from this table, so a renamed type stays consistent.
    ''' </para>
    ''' </summary>
    Friend NotInheritable Class NameTable

        Private ReadOnly _simple As New Dictionary(Of Type, String)
        Private ReadOnly _paths As New Dictionary(Of Type, String)
        Private ReadOnly _exported As New HashSet(Of String)(StringComparer.Ordinal)

        Friend Sub New(types As List(Of TsType))
            For Each model As TsType In types
                If model.ClrType.FullName IsNot Nothing Then
                    _exported.Add(model.ClrType.FullName)
                End If
            Next

            ' types sharing one declaration block: a namespace or an outer type
            Dim scopes As IEnumerable(Of IGrouping(Of String, TsType)) =
                types.GroupBy(Function(m) ScopeKey(m.ClrType), StringComparer.Ordinal)

            For Each scope In scopes
                Dim ordered As List(Of TsType) = scope _
                    .OrderBy(Function(m) m.ClrType.FullName, StringComparer.Ordinal) _
                    .ToList()

                For i As Integer = 0 To ordered.Count - 1
                    Dim t As Type = ordered(i).ClrType
                    Dim baseName As String = TsNaming.Sanitize(TsNaming.StripArity(t.Name))

                    _simple(t) = If(i = 0, baseName, baseName & "_" & i.ToString(CultureInfo.InvariantCulture))
                Next
            Next

            For Each model As TsType In types
                _paths(model.ClrType) = Compose(model.ClrType)
            Next
        End Sub

        ''' <summary>
        ''' The name block a declaration is uniquified inside: the declaring scope
        ''' (a namespace or an outer type) plus the bare CLR name, arity stripped.
        ''' </summary>
        Private Shared Function ScopeKey(t As Type) As String
            Dim name As String = TsNaming.StripArity(t.Name)

            If t.DeclaringType Is Nothing Then
                Return "ns:" & If(t.Namespace, "") & "|" & name
            End If

            Return "type:" & t.DeclaringType.FullName & "|" & name
        End Function

        ''' <summary>Is the type (or its generic definition) part of the scanned assembly?</summary>
        Friend Function IsExported(t As Type) As Boolean
            If t Is Nothing Then Return False

            Dim key As Type = t

            If t.IsGenericType AndAlso Not t.IsGenericTypeDefinition Then
                ' a constructed generic whose arguments are generic parameters carries
                ' no FullName, so always test through its definition
                key = t.GetGenericTypeDefinition()
            End If

            Return key.FullName IsNot Nothing AndAlso _exported.Contains(key.FullName)
        End Function

        ''' <summary>The dotted declaration path (namespace plus the declaring-type chain).</summary>
        Friend Function Path(t As Type) As String
            If t Is Nothing Then Return "any"

            Dim cached As String = Nothing

            If _paths.TryGetValue(t, cached) Then Return cached

            Return Compose(t)
        End Function

        ''' <summary>The declaration name inside its block (arity stripped, disambiguated).</summary>
        Friend Function SimpleName(t As Type) As String
            Dim name As String = Nothing

            If _simple.TryGetValue(t, name) Then Return name

            Return TsNaming.Sanitize(TsNaming.StripArity(t.Name))
        End Function

        Private Function Compose(t As Type) As String
            Dim parts As New List(Of String)
            Dim cur As Type = t

            While cur IsNot Nothing
                parts.Insert(0, SimpleName(cur))
                cur = cur.DeclaringType
            End While

            If Not String.IsNullOrEmpty(t.Namespace) Then parts.Insert(0, t.Namespace)

            Return String.Join(".", parts)
        End Function

    End Class

End Namespace
