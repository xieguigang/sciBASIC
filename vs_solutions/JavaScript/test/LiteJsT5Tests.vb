#Region "Microsoft.VisualBasic::4ecea98ce5373300baa2b007be90feab, vs_solutions\JavaScript\test\LiteJsT5Tests.vb"

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

    '   Total Lines: 92
    '    Code Lines: 63 (68.48%)
    ' Comment Lines: 18 (19.57%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 11 (11.96%)
    '     File Size: 4.16 KB


    ' Class LiteJsT5Tests
    ' 
    '     Function: Generate, OneLine, RunJs
    ' 
    '     Sub: CodeGen_Generators, Generators_BasicIteration, Generators_LoopsReturnAndMethods, Generators_ValueAndArguments
    ' 
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Tier 5 coverage: generator functions (<c>function*</c> / <c>yield</c>).
''' <para>
''' Generators are materialised EAGERLY — the body runs to completion on the first
''' call and the yielded values are collected. That matches the observable
''' behaviour of for-of / spread / next() consumption, but an infinite generator
''' never returns (a documented limitation of the tree-walking interpreter).
''' </para>
''' </summary>
Public Class LiteJsT5Tests

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

    <Fact>
    Public Sub Generators_BasicIteration()
        Assert.Equal("1 2", OneLine(
            "function* g() { yield 1; yield 2; }" &
            "var it = g(); console.log(it.next().value, it.next().value);"))
        ' the first next() consumes the only value; every later call reports done
        Assert.Equal("false true false true", OneLine(
            "function* g() { yield 1; }" &
            "var it = g();" &
            "var a = it.next(); console.log(a.done, it.next().done, a.done, it.next().done);"))
        ' spread and for-of consume the whole iterator
        Assert.Equal("1,2", OneLine("function* g() { yield 1; yield 2; } console.log([...g()].join(','));"))
        Assert.Equal("012", OneLine(
            "function* range(n) { for (var i = 0; i < n; i++) yield i; }" &
            "var s = ''; for (const x of range(3)) s += x; console.log(s);"))
        Assert.Equal("function", OneLine("function* g() { } console.log(typeof g);"))
    End Sub

    <Fact>
    Public Sub Generators_LoopsReturnAndMethods()
        ' a loop body produces one value per iteration
        Assert.Equal("024", OneLine(
            "function* evens(n) { for (var i = 0; i < n; i++) { if (i % 2 == 0) yield i; } }" &
            "console.log([...evens(5)].join(''));"))
        ' 'return' ends the collection
        Assert.Equal("1", OneLine(
            "function* g() { yield 1; return 9; yield 2; } console.log([...g()].join(','));"))
        ' generator methods on object literals and classes
        Assert.Equal("a", OneLine("var o = { *g() { yield 'a'; } }; console.log([...o.g()].join(','));"))
        Assert.Equal("b", OneLine("class A { *g() { yield 'b'; } } console.log([...new A().g()].join(','));"))
        ' generators interoperate with the array library
        Assert.Equal("3", OneLine("function* g() { yield 1; yield 2; yield 3; } console.log([...g()].length);"))
    End Sub

    <Fact>
    Public Sub Generators_ValueAndArguments()
        ' yield evaluates to its argument (eager mode cannot resume with a sent value)
        Assert.Equal("5", OneLine(
            "function* g() { var x = yield 5; console.log(x); }" &
            "var it = g(); it.next();"))
        ' parameters and closures work as usual
        Assert.Equal("abab", OneLine(
            "function* rep(s, n) { for (var i = 0; i < n; i++) yield s; }" &
            "console.log([...rep('ab', 2)].join(''));"))
    End Sub

    ' ============================================== code generator

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    <Fact>
    Public Sub CodeGen_Generators()
        Dim code = Generate("function* g() { yield 1; }")
        Assert.Contains("New LiteJsRuntime.JsYieldCollector()", code)
        Assert.Contains(".AddValue(", code)
        Assert.Contains("JsRuntime.JsGeneratorFrom(", code)
    End Sub

End Class

