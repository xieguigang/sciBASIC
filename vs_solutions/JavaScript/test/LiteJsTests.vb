#Region "Microsoft.VisualBasic::eb3a88f15b5a545a9569d1f2058791a0, vs_solutions\JavaScript\test\LiteJsTests.vb"

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

    '   Total Lines: 339
    '    Code Lines: 266 (78.47%)
    ' Comment Lines: 27 (7.96%)
    '    - Xml Docs: 3.70%
    ' 
    '   Blank Lines: 46 (13.57%)
    '     File Size: 15.11 KB


    ' Class LiteJsTests
    ' 
    '     Function: OneLine, RunJs
    ' 
    '     Sub: CodeGen_ArrowFunction_NoOwnArguments, CodeGen_EscapesVbKeywords, CodeGen_NeAndStrictNe_RuntimeHelpers, CodeGen_VarHoisting_SingleTopDeclaration, Console_JsonStyleDisplay
    '          Interp_ArraysAndMethods, Interp_ArrowArguments_Lexical, Interp_ControlFlow, Interp_FnWithNoParameters, Interp_FunctionsRecursionClosures
    '          Interp_LetLoop_PerIterationBinding, Interp_NeAndStrictNe, Interp_ObjectsAndForIn, Interp_ReadUndeclared_Throws, Interp_TryCatchFinallyThrow
    '          Interp_TryCatchSwallowsOnlyJsErrors, Interp_TypeofAndNullUndefined, Interp_TypeofNull_IsObject, Interp_UpdateUndeclared_ImplicitGlobal, Interp_VariablesAndArithmetic
    '          Interp_VarIsFunctionScoped, Interp_WeakTypingAndTruthiness, Json_Parse_Composites, Json_Parse_Errors, Json_Parse_Scalars
    '          Json_Stringify, Lexer_CommentsAndNumbers, Lexer_StringEscapes, Lexer_TokensAndLineBreakFlag, Lexer_UnclosedString_Throws
    '          Parser_Asi_MissingSemicolonBeforeNewline, Parser_BreakOutsideLoop_Throws, Parser_ErrorCarriesPosition, Parser_Precedence_BuildsRightTree
    ' 
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>Lexer / parser / interpreter / code-generator tests.</summary>
Public Class LiteJsTests

    ' ------------------------------------------------------------- helpers

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

    ' --------------------------------------------------------------- lexer

    <Fact>
    Public Sub Lexer_TokensAndLineBreakFlag()
        Dim ts = Lexer.Lex("var x = 1;" & vbLf & "y")
        Assert.Equal(TokenType.Keyword, ts(0).name)
        Assert.Equal("var", ts(0).Text)
        Assert.Equal(TokenType.Number, ts(3).name)
        Assert.Equal(1.0, CDbl(ts(3).Value))
        Assert.False(ts(4).LineBreakBefore)   ' ';'
        Assert.True(ts(5).LineBreakBefore)    ' 'y'
        Assert.Equal(2, ts(5).span.line)
    End Sub

    <Fact>
    Public Sub Lexer_StringEscapes()
        Dim ts = Lexer.Lex("var s = 'a\tb\nc';")
        Dim s = DirectCast(ts(3).Value, String)
        Assert.Equal("a" & vbTab & "b" & vbLf & "c", s)
    End Sub

    <Fact>
    Public Sub Lexer_UnclosedString_Throws()
        Assert.Throws(Of ParseException)(Function() Lexer.Lex("var s = 'oops"))
    End Sub

    <Fact>
    Public Sub Lexer_CommentsAndNumbers()
        Dim ts = Lexer.Lex("// c" & vbLf & "var x = 0.5e2;")
        Assert.Equal(TokenType.Number, ts(3).name)
        Assert.Equal(50.0, CDbl(ts(3).Value))
    End Sub

    ' -------------------------------------------------------------- parser

    <Fact>
    Public Sub Parser_Precedence_BuildsRightTree()
        Dim p = Parser.Parse("var r = 1 + 2 * 3;")
        Dim v = DirectCast(DirectCast(p.Body(0), VarStmt).Declarations(0).Init, BinaryExpr)
        Assert.Equal("+", v.Op)                        ' (1) + (2*3)
        Assert.Equal("*", DirectCast(v.Right, BinaryExpr).Op)
    End Sub

    <Fact>
    Public Sub Parser_Asi_MissingSemicolonBeforeNewline()
        Dim p = Parser.Parse("var a = 1" & vbLf & "var b = 2")
        Assert.Equal(2, p.Body.Count)
    End Sub

    <Fact>
    Public Sub Parser_ErrorCarriesPosition()
        Dim ex = Assert.Throws(Of ParseException)(Function() Parser.Parse("var = 3;"))
        Assert.Contains("1:5", ex.Message)
    End Sub

    <Fact>
    Public Sub Parser_BreakOutsideLoop_Throws()
        Assert.Throws(Of ParseException)(Function() Parser.Parse("break;"))
        Assert.Throws(Of ParseException)(Function() Parser.Parse("function f() { continue; }"))
    End Sub

    ' --------------------------------------------------------- interpreter

    <Fact>
    Public Sub Interp_VariablesAndArithmetic()
        Assert.Equal("8", OneLine("var a = 3, b = 5; console.log(a + b);"))
        Assert.Equal("15", OneLine("console.log(2 * 7.5);"))
        Assert.Equal("3", OneLine("console.log(7 % 4);"))
        Assert.Equal("8", OneLine("console.log(2 ** 3);"))
    End Sub

    <Fact>
    Public Sub Interp_WeakTypingAndTruthiness()
        Assert.Equal("true", OneLine("console.log(0 == '0');"))          ' loose
        Assert.Equal("false", OneLine("console.log(0 === '0');"))        ' strict
        Assert.Equal("11", OneLine("console.log('1' + 1);"))             ' string concat
        Assert.Equal("2", OneLine("console.log('1' - 0 + 1);"))          ' numeric coercion
        Assert.Equal("1", OneLine("console.log(!!'x' + !!'');"))         ' true + false
    End Sub

    <Fact>
    Public Sub Interp_FunctionsRecursionClosures()
        Assert.Equal("120", OneLine("function f(n){return n<=1?1:n*f(n-1);} console.log(f(5));"))
        ' two independent closure instances
        Dim lines = RunJs("function make() { var c = 0; return function(){ c = c + 1; return c; }; }" &
                          "var a = make(); var b = make(); a(); a(); console.log(a(), b());")
        Assert.Equal("3 1", lines(0))
    End Sub

    <Fact>
    Public Sub Interp_ControlFlow()
        Assert.Equal("16", OneLine("var s=0; for(var i=1;i<10;i++){ if(i%2==0) continue; if(i>7) break; s+=i; } console.log(s);"))
        ' do-while body runs at least once even with a false condition
        Assert.Equal("5", OneLine("var x=0; do { x = x + 5; } while (x < 0); console.log(x);"))
        Assert.Equal("ab", OneLine("var o={a:1,b:2}, k=''; for (var key in o) k += key; console.log(k);"))
    End Sub

    <Fact>
    Public Sub Interp_TryCatchFinallyThrow()
        Dim lines = RunJs("var log = '';" &
                          "try {" &
                          "    try { throw 'inner'; } catch (e) { log += 'C' + e; throw 'outer'; }" &
                          "} catch (e2) {" &
                          "    log += '!' + e2;" &
                          "} finally {" &
                          "    log += 'F';" &
                          "}" &
                          "console.log(log);")
        Assert.Equal("Cinner!outerF", lines(0))
    End Sub

    <Fact>
    Public Sub Interp_TryCatchSwallowsOnlyJsErrors()
        ' control-flow signals are NOT catchable, matching JS semantics
        Dim lines = RunJs("var out = 0;" &
                          "for (var i = 0; i < 5; i++) {" &
                          "    try { if (i == 2) continue; } catch (e) { }" &
                          "    out += i;" &
                          "}" &
                          "console.log(out);")
        Assert.Equal("8", lines(0))        ' 0+1+3+4 — continue still skipped i==2
    End Sub

    <Fact>
    Public Sub Interp_ArraysAndMethods()
        Assert.Equal("6", OneLine("console.log([1,2,3].reduce(function(a,b){return a+b;}));"))
        Assert.Equal("4 2", OneLine("var a=[3,1,2]; a.push(9); a.sort(); console.log(a.length, a[1]);"))
        Assert.Equal("1,2,3", OneLine("console.log([1,2,3].join(','));"))
        Assert.Equal("1 9", OneLine("var m=[1,2,3].map(function(x){return x*x;}); console.log(m[0], m[2]);"))
    End Sub

    <Fact>
    Public Sub Interp_ObjectsAndForIn()
        Dim lines = RunJs("var gene = { name: 'TP53', score: 0.5 };" &
                          "gene.score = gene.score * 2;" &
                          "console.log(gene.name, gene['score']);")
        Assert.Equal("TP53 1", lines(0))
    End Sub

    <Fact>
    Public Sub Interp_TypeofAndNullUndefined()
        Assert.Equal("number string boolean undefined function object",
                     OneLine("console.log(typeof 1, typeof 'x', typeof true, typeof undefined, typeof console.log, typeof {});"))
        Assert.Equal("true", OneLine("console.log(null == undefined);"))
        Assert.Equal("false", OneLine("console.log(null === undefined);"))
    End Sub

    <Fact>
    Public Sub Interp_ReadUndeclared_Throws()
        Assert.Throws(Of JsRuntimeException)(Function() RunJs("console.log(nope);"))
    End Sub

    <Fact>
    Public Sub Interp_FnWithNoParameters()
        Assert.Equal("ok", OneLine("function f() { return 'ok'; } console.log(f());"))
    End Sub

    ' -------------------------------------------- tier 0: semantic fixes

    <Fact>
    Public Sub Interp_TypeofNull_IsObject()
        Assert.Equal("object", OneLine("console.log(typeof null);"))
        Assert.Equal("object", OneLine("var n = null; console.log(typeof n);"))
        Assert.Equal("undefined", OneLine("console.log(typeof undefined);"))
        Assert.Equal("undefined", OneLine("var u; console.log(typeof u);"))
    End Sub

    <Fact>
    Public Sub Interp_VarIsFunctionScoped()
        ' var escapes an if-block (JS function scoping, not block scoping)
        Assert.Equal("1", OneLine("if (true) { var x = 1; } console.log(x);"))
        ' var escapes a for-loop header
        Assert.Equal("3", OneLine("for (var i = 0; i < 3; i++) {} console.log(i);"))
        ' var inside nested blocks of a function stays function-visible
        Assert.Equal("7", OneLine("function f() { { var y = 7; } return y; } console.log(f());"))
        ' let stays block-scoped
        Assert.Throws(Of JsRuntimeException)(Function() RunJs("if (true) { let z = 1; } console.log(z);"))
    End Sub

    <Fact>
    Public Sub Interp_LetLoop_PerIterationBinding()
        ' each closure captures its own let binding (0 1 2), unlike var (3 3 3)
        Assert.Equal("0 1 2", OneLine(
            "var fns = []; for (let i = 0; i < 3; i++) { fns.push(function() { return i; }); }" &
            "console.log(fns[0](), fns[1](), fns[2]());"))
        Assert.Equal("3 3 3", OneLine(
            "var fns = []; for (var i = 0; i < 3; i++) { fns.push(function() { return i; }); }" &
            "console.log(fns[0](), fns[1](), fns[2]());"))
    End Sub

    <Fact>
    Public Sub Interp_ArrowArguments_Lexical()
        ' arrow functions inherit 'arguments' from the enclosing function
        Dim lines = RunJs(
            "function outer() {" &
            "  var arrow = () => arguments[0];" &
            "  return arrow('x', 'y');" &
            "}" &
            "console.log(outer('a', 'b'));")
        Assert.Equal("a", lines(0))
        ' regular functions get their own arguments object
        Assert.Equal("x", OneLine("function f() { return arguments[0]; } console.log(f('x', 'y'));"))
    End Sub

    <Fact>
    Public Sub Interp_UpdateUndeclared_ImplicitGlobal()
        ' non-strict JS: x++ on an undeclared x assigns NaN to the implicit global
        Assert.Equal("NaN", OneLine("x++; console.log(x);"))
        Assert.Equal("NaN", OneLine("y--; console.log(y);"))
        Assert.Equal("NaN", OneLine("console.log(++undeclaredCounter);"))
    End Sub

    <Fact>
    Public Sub Interp_NeAndStrictNe()
        Assert.Equal("true false", OneLine("console.log(1 != 2, 1 !== 1);"))
        Assert.Equal("true", OneLine("console.log('a' != 'b');"))
        Assert.Equal("true", OneLine("console.log(NaN != NaN);"))     ' NaN equals nothing
        Assert.Equal("false", OneLine("console.log(NaN == NaN);"))
    End Sub

    ' --------------------------------------------------------- codegen (text)

    <Fact>
    Public Sub CodeGen_EscapesVbKeywords()
        Dim code = New CodeGenerator().Generate(Parser.Parse("var string = 1; var next = 2;"))
        Assert.Contains("Public NotInheritable Class GeneratedScript", code)
        Assert.Contains("JsRuntime", code)                ' dynamic dispatch
        Assert.DoesNotContain("Dim string As", code)      ' VB keyword escaped
    End Sub

    <Fact>
    Public Sub CodeGen_VarHoisting_SingleTopDeclaration()
        ' VB Dim is block-scoped but JS var is function-scoped: the var must be
        ' declared once at the top of Run and only assigned inside the block
        Dim code = New CodeGenerator().Generate(Parser.Parse("if (true) { var x = 1; } console.log(x);"))
        Assert.Contains("Dim x As JsValue = JsRuntime.Undef", code)
        Assert.DoesNotContain("Dim x As JsValue = 1R", code)
    End Sub

    <Fact>
    Public Sub CodeGen_NeAndStrictNe_RuntimeHelpers()
        ' != / !== translate to the JsRuntime helpers that actually exist
        Dim code = New CodeGenerator().Generate(Parser.Parse("console.log(1 != 2, 1 !== 2);"))
        Assert.Contains("JsRuntime.JsNe(", code)
        Assert.Contains("JsRuntime.JsStrictNe(", code)
    End Sub

    <Fact>
    Public Sub CodeGen_ArrowFunction_NoOwnArguments()
        ' arrows inherit 'arguments' lexically; only real functions bind their own
        Dim arrowCode = New CodeGenerator().Generate(Parser.Parse("var f = () => 1;"))
        Assert.DoesNotContain("Dim arguments", arrowCode)
        Dim fnCode = New CodeGenerator().Generate(Parser.Parse("var g = function() { return 1; };"))
        Assert.Contains("Dim arguments As JsValue = New List(Of Object)(args__", fnCode)
    End Sub

    ' ----------------------------------------------------------------- json

    <Fact>
    Public Sub Json_Parse_Scalars()
        Assert.Equal("1.5", OneLine("console.log(JSON.parse('1.5'));"))
        Assert.Equal("42", OneLine("console.log(JSON.parse('42'));"))
        Assert.Equal("true", OneLine("console.log(JSON.parse('true'));"))
        Assert.Equal("null", OneLine("console.log(JSON.parse('null'));"))
        Assert.Equal("hi there", OneLine("console.log(JSON.parse('""hi there""'));"))
    End Sub

    <Fact>
    Public Sub Json_Parse_Composites()
        Assert.Equal("TP53 2", OneLine(
            "var o = JSON.parse('{""name"":""TP53"",""score"":2}');" &
            "console.log(o.name, o.score);"))
        Assert.Equal("3 2", OneLine(
            "var a = JSON.parse('[1,2,3]'); console.log(a.length, a[1]);"))
        Assert.Equal("1 1", OneLine(
            "var o = JSON.parse('{""nested"":{""deep"":{""x"":1}}}');" &
            "console.log(o.nested.deep.x, 1);"))
    End Sub

    <Fact>
    Public Sub Json_Parse_Errors()
        Assert.Throws(Of JsRuntimeException)(Function() RunJs("JSON.parse('{');"))
        Assert.Throws(Of JsRuntimeException)(Function() RunJs("JSON.parse('');"))
        ' parse errors are catchable by JS try/catch
        Assert.Equal("caught", OneLine(
            "try { JSON.parse('{bad'); } catch (e) { console.log('caught'); }"))
    End Sub

    <Fact>
    Public Sub Json_Stringify()
        Assert.Equal("1", OneLine(
            "console.log(JSON.parse(JSON.stringify({a: 1, b: [1, 2]})).b[0]);"))
        Assert.Equal("""x""", OneLine("console.log(JSON.stringify('x'));"))
        Assert.Equal("[1,2,3]", OneLine("console.log(JSON.stringify([1,2,3]));"))
        ' undefined members are omitted, undefined in arrays → null
        Assert.Equal("{""b"": 1}", OneLine(
            "console.log(JSON.stringify({a: undefined, b: 1}));"))
        Assert.Equal("[null]", OneLine("console.log(JSON.stringify([NaN]));"))
        ' top-level undefined → the value undefined itself
        Assert.Equal("undefined", OneLine("console.log(JSON.stringify(undefined));"))
    End Sub

    <Fact>
    Public Sub Console_JsonStyleDisplay()
        Assert.Equal("[1,2,3]", OneLine("console.log([1,2,3]);"))
        Assert.Equal("{""a"": 1}", OneLine("console.log({a: 1});"))
        Assert.Equal("{""name"": ""TP53"",""score"": 2}", OneLine(
            "console.log({name: 'TP53', score: 2});"))
        ' scalars keep the existing display behaviour
        Assert.Equal("abc", OneLine("console.log('abc');"))
        Assert.Equal("1.5", OneLine("console.log(1.5);"))
    End Sub

End Class

