#Region "Microsoft.VisualBasic::7a2c9f05e4b83d16c0a7f3e895b2d418, vs_solutions\JavaScript\TypeDefinitions\TypeMapper.vb"

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

    '     Module TypeMapper
    ' 
    '         Function: TsTypeName

    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Threading.Tasks

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

Namespace TypeDefinitions

    ''' <summary>
    ''' Renders a CLR type as a TypeScript type, item by item aligned with the
    ''' <see cref="Runtime.ClrConvert"/> conversion semantics of the script bridge:
    ''' <para>
    ''' numerics → ``number`` (enums report their underlying TypeCode, so they land
    ''' here too — the bridge hands a script the numeric value) ·
    ''' String/Char/Guid → ``string`` · Boolean → ``boolean`` ·
    ''' Date/DateTimeOffset → ``Date`` · Task(Of T) → ``Promise&lt;T&gt;`` ·
    ''' Nullable(Of T) → ``T | null`` · arrays and ``List(Of T)`` → ``T[]`` ·
    ''' ``IEnumerable(Of T)`` → ``Iterable&lt;T&gt;`` · delegates → the declared function
    ''' type · types of the scanned assembly → their declaration name · everything
    ''' else (BCL) → ``any``.
    ''' </para>
    ''' <paramref name="context"/> carries the generic parameter names of the
    ''' declaring type: Visual Basic emits a signature type that instantiates a
    ''' generic type with the enclosing type's own parameters (in order) as the
    ''' unbound generic definition, so those parameters are substituted back here.
    ''' </summary>
    Friend Module TypeMapper

        Friend Function TsTypeName(t As Type, exports As HashSet(Of String),
                                   Optional context As List(Of String) = Nothing) As String
            If t Is Nothing Then Return "any"
            If t.IsByRef Then Return TsTypeName(t.GetElementType(), exports, context)
            If t.IsPointer Then Return "any"
            If t.IsGenericParameter Then Return TsNaming.Sanitize(t.Name)

            If t.IsArray Then
                ' the bridge rejects multi-dimensional arrays, so does the definition
                If t.GetArrayRank() = 1 Then
                    Return TsTypeName(t.GetElementType(), exports, context) & "[]"
                End If

                Return "any"
            End If

            Dim nullable As Type = System.Nullable.GetUnderlyingType(t)

            If nullable IsNot Nothing Then
                Return TsTypeName(nullable, exports, context) & " | null"
            End If

            If t Is GetType(Void) Then Return "void"
            If t Is GetType(Boolean) Then Return "boolean"
            If t Is GetType(String) OrElse t Is GetType(Char) OrElse t Is GetType(Guid) Then Return "string"
            ' enums report their underlying TypeCode here, exactly like ClrConvert.ToJs
            ' converts them to a Double: the script sees the numeric value
            If ClrConvert.IsNumericType(t) Then Return "number"
            If t Is GetType(Date) OrElse t Is GetType(DateTimeOffset) Then Return "Date"
            If t Is GetType(Object) OrElse t Is GetType(DBNull) Then Return "any"
            If t Is GetType(Task) Then Return "Promise<void>"

            If t.IsGenericType Then
                Dim definition As String = t.GetGenericTypeDefinition().FullName

                If definition = "System.Threading.Tasks.Task`1" Then
                    Return "Promise<" & TsTypeName(t.GetGenericArguments()(0), exports, context) & ">"
                End If

                If ClrConvert.IsGenericList(t) Then
                    Return TsTypeName(t.GetGenericArguments()(0), exports, context) & "[]"
                End If

                Select Case definition
                    Case "System.Collections.Generic.IList`1",
                         "System.Collections.Generic.ICollection`1",
                         "System.Collections.Generic.IReadOnlyList`1",
                         "System.Collections.Generic.IReadOnlyCollection`1"
                        Return TsTypeName(t.GetGenericArguments()(0), exports, context) & "[]"
                    Case "System.Collections.Generic.IEnumerable`1"
                        Return "Iterable<" & TsTypeName(t.GetGenericArguments()(0), exports, context) & ">"
                End Select
            End If

            If ClrConvert.IsDelegate(t) Then
                Return If(IsExported(t, exports), TsNaming.QualifiedName(t), "any")
            End If

            If IsExported(t, exports) Then
                If t.IsGenericTypeDefinition Then
                    ' Visual Basic writes a signature that instantiates a generic type
                    ' with the enclosing type's own parameters as the unbound
                    ' definition; substitute those parameters back positionally
                    Dim args As List(Of String) = DefinitionArguments(t, context)

                    Return TsNaming.QualifiedName(t) & "<" & String.Join(", ", args) & ">"
                End If

                Return TsNaming.QualifiedName(t)
            End If

            ' BCL types outside the mapping table degrade to any
            Return "any"
        End Function

        ''' <summary>
        ''' The rendered generic arguments of an unbound definition signature: the
        ''' declaring type's parameters when they line up positionally (the only case
        ''' Visual Basic emits the definition for), ``any`` otherwise.
        ''' </summary>
        Private Function DefinitionArguments(t As Type, context As List(Of String)) As List(Of String)
            Dim count As Integer = t.GetGenericArguments().Length

            If context IsNot Nothing AndAlso context.Count = count Then
                Return context
            End If

            Return Enumerable.Repeat("any", count).ToList()
        End Function

        ''' <summary>Is the type (or its generic definition) part of the scanned assembly?</summary>
        Private Function IsExported(t As Type, exports As HashSet(Of String)) As Boolean
            If t.FullName Is Nothing Then Return False

            If t.IsGenericType AndAlso Not t.IsGenericTypeDefinition Then
                Return exports.Contains(t.GetGenericTypeDefinition().FullName)
            End If

            Return exports.Contains(t.FullName)
        End Function

    End Module

End Namespace
