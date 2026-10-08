Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq
Imports LiteJs
Imports LiteJs.Runtime
Imports Xunit

''' <summary>Lexer / parser / interpreter / code-generator tests.</summary>
Public Class LiteJsTests

    ' ------------------------------------------------------------- helpers

    Private Shared Function RunJs(source As String) As List(Of String)
        Dim io As New ScriptIO()
        Dim engine As New Interpreter(io)
        engine.Run(Parser.Parse(source))
        Return io.Lines
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
        Assert.Equal(TokenType.Keyword, ts(0).Type)
        Assert.Equal("var", ts(0).Text)
        Assert.Equal(TokenType.Number, ts(3).Type)
        Assert.Equal(1.0, CDbl(ts(3).Value))
        Assert.False(ts(4).LineBreakBefore)   ' ';'
        Assert.True(ts(5).LineBreakBefore)    ' 'y'
        Assert.Equal(2, ts(5).Line)
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
        Assert.Equal(TokenType.Number, ts(3).Type)
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

    ' --------------------------------------------------------- codegen (text)

    <Fact>
    Public Sub CodeGen_EscapesVbKeywords()
        Dim code = New CodeGenerator().Generate(Parser.Parse("var string = 1; var next = 2;"))
        Assert.Contains("Public NotInheritable Class GeneratedScript", code)
        Assert.Contains("JsRuntime", code)                ' dynamic dispatch
        Assert.DoesNotContain("Dim string As", code)      ' VB keyword escaped
    End Sub

    ' ------------------------------------------------------------- CLI demo

    <Fact>
    Public Sub Cli_Demo_NoE2E_Passes()
        Dim rc = LiteJs.Cli.Program.Main({"/demo", "/no-e2e"})
        Assert.Equal(0, rc)
    End Sub

    <Fact>
    Public Sub Cli_Run_Translate_Smoke()
        Dim dir = Path.Combine(Path.GetTempPath(), "litejs_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(dir)
        Try
            Dim js = Path.Combine(dir, "t.js")
            File.WriteAllText(js, "function add(a,b){return a+b;} console.log(add(2,40));")
            Assert.Equal(0, LiteJs.Cli.Program.Main({"/run", js}))
            Dim outVb = Path.Combine(dir, "t.vb")
            Assert.Equal(0, LiteJs.Cli.Program.Main({"/translate", js, outVb}))
            Assert.True(File.ReadAllText(outVb).Contains("JsRuntime.JsAdd"))
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

End Class
