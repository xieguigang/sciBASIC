#Region "Microsoft.VisualBasic::a71f3d5c2e984b16bc0d5a7f81e39c42, vs_solutions\JavaScript\Runtime\JsFunctions.vb"

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

    '     Module JsFunctions
    '
    '         Function: Arity, DevToolsEcho, DisplayName, Inspect, IsArrow, JoinParameters
    '                   NativeSource, SignatureOf, SourceOf, TryGetNode
    '
    '         Sub: Register
    '
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' Links the runtime value of a JavaScript function — a
    ''' <see cref="Func(Of Object(), Object)"/> delegate created by the interpreter — to the
    ''' AST node it came from, and renders it the way Node does.
    ''' <para>
    ''' A delegate cannot carry extra fields, so the link lives in a
    ''' <see cref="ConditionalWeakTable(Of TKey, TValue)"/> keyed by the delegate itself: weak
    ''' keys mean a collected function value never keeps its AST alive, and the table is
    ''' thread safe, so no per thread state is needed.
    ''' </para>
    ''' <para>
    ''' Every accessor degrades safely — a function without a registered node (a host
    ''' delegate, a generated code lambda) reports <c>[Function: name]</c> instead of throwing.
    ''' </para>
    ''' </summary>
    Public Module JsFunctions

        Private ReadOnly _nodes As New ConditionalWeakTable(Of Object, FuncExpr)()

        ''' <summary>
        ''' Remember which AST node a runtime function value came from (called by
        ''' <c>Interpreter.MakeFunction</c>, the only place that creates them).
        ''' </summary>
        Public Sub Register(value As Object, node As FuncExpr)
            If value Is Nothing OrElse node Is Nothing Then Return

            _nodes.Remove(value)
            _nodes.Add(value, node)
        End Sub

        Public Function TryGetNode(value As Object, ByRef node As FuncExpr) As Boolean
            node = Nothing

            If value Is Nothing Then Return False

            Return _nodes.TryGetValue(value, node)
        End Function

        ' ===================== script visible properties =====================

        ''' <summary>
        ''' JS <c>fn.name</c>: the declared name, else the name inferred from the syntactic
        ''' position, else <c>""</c> for a truly anonymous function (like V8).
        ''' </summary>
        Public Function DisplayName(value As Object) As String
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return ""

            Return If(Not String.IsNullOrEmpty(node.Name), node.Name, If(node.InferredName, ""))
        End Function

        ''' <summary>
        ''' JS <c>fn.length</c>: the parameters before the first default value or rest
        ''' parameter (parameters with a default are not counted, matching the spec).
        ''' </summary>
        Public Function Arity(value As Object) As Integer
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return 0

            Dim count As Integer = 0

            For i = 0 To node.Parameters.Length - 1
                If node.Defaults IsNot Nothing AndAlso i < node.Defaults.Length AndAlso node.Defaults(i) IsNot Nothing Then Exit For

                count += 1
            Next

            Return count
        End Function

        Public Function IsArrow(value As Object) As Boolean
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return False

            Return node.IsArrow
        End Function

        ' ===================== rendering =====================

        ''' <summary>
        ''' Node's <c>util.inspect</c> form used by <c>console.log(fn)</c>:
        ''' <c>[Function: name]</c> (native / host functions use their own name).
        ''' </summary>
        Public Function Inspect(value As Object) As String
            Dim builtin = TryCast(value, BuiltinMethod)

            If builtin IsNot Nothing Then Return "[Function: " & builtin.Name & "]"

            Dim clrMethod = TryCast(value, ClrBoundMethod)

            If clrMethod IsNot Nothing Then Return "[Function: " & clrMethod.Name & "]"

            Dim name = DisplayName(value)

            If String.IsNullOrEmpty(name) Then Return "[Function]"

            Return "[Function: " & name & "]"
        End Function

        ''' <summary>
        ''' JS <c>String(fn)</c> (also <c>'x' + fn</c> and the <c>%s</c> specifier): the function
        ''' source. The body text is the original source slice kept by the parser; a function
        ''' without source (host delegate, generated code) degrades to the inspect form.
        ''' </summary>
        Public Function SourceOf(value As Object) As String
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return Inspect(value)
            If String.IsNullOrEmpty(node.BodySource) Then Return Inspect(value)

            If node.IsArrow Then Return AsyncPrefix(node) & "(" & JoinParameters(node) & ") => " & node.BodySource

            Dim name = DisplayName(value)
            Dim keyword = If(node.IsGenerator, "function*", "function")

            If String.IsNullOrEmpty(name) Then
                Return AsyncPrefix(node) & keyword & " (" & JoinParameters(node) & ") " & node.BodySource
            End If

            Return AsyncPrefix(node) & keyword & " " & name & "(" & JoinParameters(node) & ") " & node.BodySource
        End Function

        ''' <summary>The source form of a native (host) function: <c>function log() { [native code] }</c>.</summary>
        Public Function NativeSource(name As String) As String
            Return "function " & If(name, "") & "() { [native code] }"
        End Function

        ''' <summary>
        ''' DevTools style echo of a function value (<c>ƒ add(a, b) { ... }</c>) used by the
        ''' REPL; <c>Nothing</c> when the value has no known source.
        ''' </summary>
        Public Function DevToolsEcho(value As Object) As String
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return Nothing
            If String.IsNullOrEmpty(node.BodySource) Then Return Nothing

            Dim marker = If(node.IsGenerator, "ƒ* ", "ƒ ")
            Dim parameters = "(" & JoinParameters(node) & ")"

            If node.IsArrow Then Return AsyncPrefix(node) & marker & parameters & " => " & node.BodySource

            Dim name = DisplayName(value)
            Dim signature = If(String.IsNullOrEmpty(name), parameters, name & parameters)

            Return AsyncPrefix(node) & marker & signature & " " & node.BodySource
        End Function

        ''' <summary>Signature of a function value, e.g. <c>add(a, b)</c> (anonymous: <c>(a, b)</c>).</summary>
        Public Function SignatureOf(value As Object) As String
            Dim node As FuncExpr = Nothing

            If Not TryGetNode(value, node) Then Return "()"

            Dim name = DisplayName(value)
            Dim parameters = "(" & JoinParameters(node) & ")"

            If String.IsNullOrEmpty(name) Then Return parameters

            Return name & parameters
        End Function

        Private Function AsyncPrefix(node As FuncExpr) As String
            Return If(node.IsAsync, "async ", "")
        End Function

        ''' <summary>
        ''' The parameter list as written. Default values are not kept as source, so a
        ''' defaulted parameter is rendered by its name (<c>(a, b = 1)</c> shows as <c>(a, b)</c>).
        ''' </summary>
        Private Function JoinParameters(node As FuncExpr) As String
            Dim parts As New List(Of String)()

            For Each parameter In node.Parameters
                parts.Add(parameter)
            Next

            If Not String.IsNullOrEmpty(node.RestParam) Then parts.Add("..." & node.RestParam)

            Return String.Join(", ", parts)
        End Function

    End Module

End Namespace
