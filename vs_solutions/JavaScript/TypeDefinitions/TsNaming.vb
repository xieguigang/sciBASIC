#Region "Microsoft.VisualBasic::9d1f4a82c6b05e3f70a2d9c8b41e5f63, vs_solutions\JavaScript\TypeDefinitions\TsNaming.vb"

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
    ' GNU GENERAL PUBLIC License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Module TsNaming
    ' 
    '         Function: QualifiedName, Sanitize, StripArity

    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace TypeDefinitions

    ''' <summary>
    ''' Identifier and name helpers shared by the scanner, the type mapper and the emitter.
    ''' </summary>
    Friend Module TsNaming

        ''' <summary>
        ''' Words that are illegal as a bare identifier in a TypeScript declaration slot
        ''' (type parameter names, type names). Only these slots are sanitised — member
        ''' names are emitted verbatim because the bridge matches them exactly.
        ''' </summary>
        Private ReadOnly reserved As New HashSet(Of String)(StringComparer.Ordinal) From {
            "abstract", "any", "as", "async", "await", "bigint", "boolean", "break", "case",
            "catch", "class", "const", "constructor", "continue", "debugger", "declare",
            "default", "delete", "do", "else", "enum", "export", "extends", "false", "finally",
            "for", "from", "function", "get", "if", "implements", "import", "in", "infer",
            "instanceof", "interface", "is", "keyof", "let", "module", "namespace", "never",
            "new", "null", "number", "object", "of", "private", "protected", "public",
            "readonly", "return", "set", "static", "string", "super", "switch", "symbol",
            "this", "throw", "true", "try", "type", "typeof", "undefined", "unique", "unknown",
            "var", "void", "while", "with", "yield"
        }

        ''' <summary>Strip the CLR generic arity suffix (``Foo`1`` → ``Foo``).</summary>
        Friend Function StripArity(name As String) As String
            If name Is Nothing Then Return ""
            Dim tick As Integer = name.IndexOf("`"c)
            If tick > 0 Then Return name.Substring(0, tick)
            Return name
        End Function

        ''' <summary>Make a name safe for a TypeScript declaration slot.</summary>
        Friend Function Sanitize(name As String) As String
            If String.IsNullOrEmpty(name) Then Return "_"
            If reserved.Contains(name) Then Return name & "_"
            Return name
        End Function

        ''' <summary>
        ''' The TypeScript declaration path of a CLR type: dotted namespace plus the
        ''' declaring-type chain (nested types become ``Ns.Outer.Inner``), generic arity
        ''' stripped, with ``&lt;args&gt;`` appended for constructed generic types.
        ''' </summary>
        Friend Function QualifiedName(t As Type) As String
            If t Is Nothing Then Return "any"

            Dim definition As Type = t
            Dim constructed As Boolean = t.IsGenericType AndAlso Not t.IsGenericTypeDefinition

            If constructed Then definition = t.GetGenericTypeDefinition()

            Dim path As String = PathOf(definition)

            If constructed Then
                Dim args = t.GetGenericArguments().Select(Function(a) QualifiedName(a))
                Return path & "<" & String.Join(", ", args) & ">"
            End If

            Return path
        End Function

        ''' <summary>The dotted path of a (possibly nested, possibly generic) type definition.</summary>
        Private Function PathOf(t As Type) As String
            Dim parts As New List(Of String)
            Dim cur As Type = t

            While cur IsNot Nothing
                parts.Insert(0, StripArity(cur.Name))
                cur = cur.DeclaringType
            End While

            Dim ns As String = t.Namespace

            If Not String.IsNullOrEmpty(ns) Then parts.Insert(0, ns)

            Return String.Join(".", parts)
        End Function

    End Module

End Namespace
