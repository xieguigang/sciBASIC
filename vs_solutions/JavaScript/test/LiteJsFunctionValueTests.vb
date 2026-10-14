#Region "Microsoft.VisualBasic::c9e4b7a1f0d3486293c15e7ab40f68d1, vs_solutions\JavaScript\test\LiteJsFunctionValueTests.vb"

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

    '     Class LiteJsFunctionValueTests
    '
    '         Function: OneLine, RunJs
    '
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Function values as script visible objects: <c>String(fn)</c> (the source),
''' <c>console.log(fn)</c> (node's <c>[Function: name]</c>), <c>fn.name</c> /
''' <c>fn.length</c>, and the safe rendering of values that used to hit the invalid
''' string conversion (<c>CStr</c>) fallback — the crash reported from the REPL when
''' typing a function name.
''' </summary>
Public Class LiteJsFunctionValueTests

    Private Shared Function RunJs(source As String) As List(Of String)
        Dim io As ScriptIO = New LogTextIO
        Dim engine As New Interpreter(io)
        engine.Run(Parser.Parse(source))
        Return DirectCast(io, LogTextIO).Lines
    End Function

    Private Shared Function OneLine(source As String) As String
        Dim lines = RunJs(source)
        Assert.True(lines.Count >= 1, "no output")
        Return lines(0)
    End Function

    ' ------------------------------------------------ String(fn) / fn.toString()

    <Fact>
    Public Sub String_OfFunctionIsTheSource()
        Const header = "function add(a, b) { return a + b; } "

        ' the reported crash: String(fn) threw InvalidCastException before this fix
        Assert.Equal("function add(a, b) { return a + b; }", OneLine(header & "console.log(String(add));"))
        Assert.Equal("function add(a, b) { return a + b; }", OneLine(header & "console.log('' + add);"))
        Assert.Equal("function add(a, b) { return a + b; }", OneLine(header & "console.log(add.toString());"))
        ' %s uses the string form, like node
        Assert.Equal("fn=function add(a, b) { return a + b; }",
                     OneLine(header & "console.log('fn=%s', add);"))
    End Sub

    <Fact>
    Public Sub String_OfArrowAndMethodSources()
        Assert.Equal("(x) => x + 1", OneLine("console.log(String((x) => x + 1));"))
        Assert.Equal("function (x) { return x; }", OneLine("console.log(String(function (x) { return x; }));"))
        ' a method / shorthand body is rendered with the function keyword (the parameter
        ' list and the body text are the ones that were written)
        Assert.Equal("function m() { return 1; }", OneLine("var o = { m() { return 1; } }; console.log(String(o.m));"))
    End Sub

    <Fact>
    Public Sub String_KeepsTheWrittenBodyTextAcrossLines()
        Dim lines = RunJs("function multi(a) {" & vbLf &
                          "  return a;" & vbLf &
                          "}" & vbLf &
                          "console.log(String(multi));")

        ' the echoed source is a single log entry that still spans the original lines
        Assert.Equal(1, lines.Count)
        Assert.Contains("function multi(a) {", lines(0))
        Assert.Contains("return a;", lines(0))
        Assert.Contains(vbLf, lines(0))
    End Sub

    ' ------------------------------------------------ console.log(fn)

    <Fact>
    Public Sub Display_OfFunctionUsesNodeInspectForm()
        ' console.log(fn) is node's inspect form, while String(fn) is the source
        Assert.Equal("[Function: add]", OneLine("function add(a, b) { return a + b; } console.log(add);"))
        Assert.Equal("[Function: log]", OneLine("console.log(console.log);"))
        Assert.Equal("[Function: add]", OneLine("function add() {} console.log('%o', add);"))
        Assert.Equal("add [Function: add]", OneLine("function add() {} console.log(add.name, add);"))
        ' an anonymous function has no name to show
        Assert.Equal("[Function]", OneLine("console.log(function (x) { return x; });"))
    End Sub

    ' ------------------------------------------------ fn.name / fn.length

    <Fact>
    Public Sub Name_And_Length_FollowNode()
        Assert.Equal("add 2", OneLine("function add(a, b) { return a + b; } console.log(add.name, add.length);"))
        ' the name is inferred from the declaration / assignment position (V8 inference)
        Assert.Equal("named", OneLine("var named = function (x) { return x; }; console.log(named.name);"))
        Assert.Equal("arrow", OneLine("var arrow = (x) => x + 1; console.log(arrow.name);"))
        ' object and class methods take the key as their name
        Assert.Equal("m", OneLine("var o = { m() { return 1; } }; console.log(o.m.name);"))
        Assert.Equal("m", OneLine("class C { m() { return 1; } } console.log((new C()).m.name);"))
    End Sub

    <Fact>
    Public Sub Length_StopsAtDefaultOrRestParameter()
        Assert.Equal("1", OneLine("function f(a, b = 2, ...rest) { return a; } console.log(f.length);"))
        Assert.Equal("2", OneLine("function f(a, b, c = 1) { return a; } console.log(f.length);"))
        Assert.Equal("2", OneLine("function f(a, b, ...rest) { return a; } console.log(f.length);"))
        Assert.Equal("0", OneLine("function f(...rest) { return 1; } console.log(f.length);"))
    End Sub

    ' ------------------------------------------------ safe degradation

    <Fact>
    Public Sub FunctionValue_WithoutSourceDegradesSafely()
        Dim io As ScriptIO = New LogTextIO
        Dim engine As New Interpreter(io)

        ' a host delegate has no AST node / no source: rendering must never throw
        engine.DefineGlobal("hosted", CType(Function(args As Object()) CObj(args.Length), Func(Of Object(), Object)))
        engine.Run(Parser.Parse("console.log(String(hosted), hosted(1, 2, 3), hosted.name.length, hosted.toString());"))

        Dim lines = DirectCast(io, LogTextIO).Lines
        Assert.Equal("[Function] 3 0 [Function]", lines(0))
    End Sub

    <Fact>
    Public Sub OpaqueValues_NoLongerHitTheInvalidConversion()
        ' every one of these used to reach `Return CStr(x)` and throw
        Assert.Equal("[object Promise]", OneLine("console.log(String(Promise.resolve(1)));"))
        Assert.Equal("[object Promise]", OneLine("console.log(Promise.resolve(1));"))
        ' a promise is still a thenable object
        Assert.Equal("true", OneLine("console.log(typeof Promise.resolve(1).then === 'function');"))
    End Sub

    ' ------------------------------------------------ guards for existing behaviour

    <Fact>
    Public Sub ExistingDisplayBehaviour_IsUnchanged()
        ' the error / class / object forms other tests rely on
        Assert.Equal("TypeError: t", OneLine("console.log(String(new TypeError('t')));"))
        Assert.Equal("function Point", OneLine("class Point { } console.log(typeof Point, Point.name);"))
        Assert.Equal("class Point { ... }", OneLine("class Point { } console.log(String(Point));"))
        Assert.Equal("[object Object]", OneLine("console.log(String({a: 1}));"))
        Assert.Equal("function log() { [native code] }", OneLine("console.log(String(console.log));"))
        ' functions stay invisible to json, exactly like js
        Assert.Equal("undefined", OneLine("function add() {} console.log(String(JSON.stringify(add)));"))
        Assert.Equal("{}", OneLine("function add() {} console.log(JSON.stringify({f: add}));"))
        ' and a function value is still callable / typed as a function
        Assert.Equal("3 function", OneLine("function add(a, b) { return a + b; } console.log(add(1, 2), typeof add);"))
    End Sub

End Class
