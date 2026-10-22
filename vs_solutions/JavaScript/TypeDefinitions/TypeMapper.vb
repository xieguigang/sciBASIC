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

Namespace TypeDefinitions

    ''' <summary>
    ''' Renders a CLR type as a TypeScript type, item by item aligned with the
    ''' <see cref="Runtime.ClrConvert"/> conversion semantics of the script bridge:
    ''' <para>
    ''' numerics → ``number`` · String/Char/Guid → ``string`` · Boolean → ``boolean`` ·
    ''' Date/DateTimeOffset → ``Date`` · Task(Of T) → ``Promise&lt;T&gt;`` ·
    ''' Nullable(Of T) → ``T | null`` · arrays and ``List(Of T)`` → ``T[]`` ·
    ''' ``IEnumerable(Of T)`` → ``Iterable&lt;T&gt;`` · delegates → the declared function
    ''' type · types of the scanned assembly → their declaration name · everything
    ''' else (BCL) → ``any``.
    ''' </para>
    ''' </summary>
    Friend Module TypeMapper

        Friend Function TsTypeName(t As Type, exports As HashSet(Of String)) As String
            If t Is Nothing Then Return "any"
            If t.IsByRef Then Return TsTypeName(t.GetElementType(), exports)
            If t.IsPointer Then Return "any"
            If t.IsGenericParameter Then Return TsNaming.Sanitize(t.Name)

            If t.IsArray Then
                ' the bridge rejects multi-dimensional arrays, so does the definition
                If t.GetArrayRank() = 1 Then
                    Return TsTypeName(t.GetElementType(), exports) & "[]"
                End If

                Return "any"
            End If

            Dim nullable As Type = Nullable.GetUnderlyingType(t)

            If nullable IsNot Nothing Then
                Return TsTypeName(nullable, exports) & " | null"
            End If

            If t Is GetType(Void) Then Return "void"
            If t Is GetType(Boolean) Then Return "boolean"
            If t Is GetType(String) OrElse t Is GetType(Char) OrElse t Is GetType(Guid) Then Return "string"
            If ClrConvert.IsNumericType(t) Then Return "number"
            If t Is GetType(Date) OrElse t Is GetType(DateTimeOffset) Then Return "Date"
            If t Is GetType(Object) OrElse t Is GetType(DBNull) Then Return "any"
            If t Is GetType(Task) Then Return "Promise<void>"

            If t.IsGenericType Then
                Dim definition As String = t.GetGenericTypeDefinition().FullName

                If definition = "System.Threading.Tasks.Task`1" Then
                    Return "Promise<" & TsTypeName(t.GetGenericArguments()(0), exports) & ">"
                End If

                If ClrConvert.IsGenericList(t) Then
                    Return TsTypeName(t.GetGenericArguments()(0), exports) & "[]"
                End If

                Select Case definition
                    Case "System.Collections.Generic.IList`1",
                         "System.Collections.Generic.ICollection`1",
                         "System.Collections.Generic.IReadOnlyList`1",
                         "System.Collections.Generic.IReadOnlyCollection`1"
                        Return TsTypeName(t.GetGenericArguments()(0), exports) & "[]"
                    Case "System.Collections.Generic.IEnumerable`1"
                        Return "Iterable<" & TsTypeName(t.GetGenericArguments()(0), exports) & ">"
                End Select
            End If

            If t.IsEnum Then
                Return If(IsExported(t, exports), TsNaming.QualifiedName(t), "number")
            End If

            If ClrConvert.IsDelegate(t) Then
                Return If(IsExported(t, exports), TsNaming.QualifiedName(t), "any")
            End If

            If IsExported(t, exports) Then Return TsNaming.QualifiedName(t)

            ' BCL types outside the mapping table degrade to any
            Return "any"
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
