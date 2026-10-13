Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Tier 1 syntax coverage: switch / for-of / labels / template literals /
''' bitwise / ?? / optional chaining / spread / rest &amp; default parameters /
''' destructuring / object-literal enhancements / comma / in / instanceof /
''' delete / void / numeric separators.
''' <para>
''' Interpreter expectations follow real JavaScript behaviour; the code
''' generator is checked structurally (no NotSupportedException, expected
''' JsRuntime dispatch) because the generated VB cannot be compiled in-process.
''' </para>
''' </summary>
Public Class LiteJsT1Tests

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

    ' ---------------------------------------------------------------- switch

    <Fact>
    Public Sub Switch_MatchDefaultFallThroughBreak()
        Assert.Equal("2", OneLine("switch (2) { case 1: console.log(1); break; case 2: console.log(2); break; }"))
        ' fall-through: case 2 runs into case 3 until break
        Assert.Equal("23", OneLine("var r = ''; switch (2) { case 1: r += '1'; case 2: r += '2'; case 3: r += '3'; break; default: r += 'd'; } console.log(r);"))
        Assert.Equal("d", OneLine("var r = ''; switch (9) { case 1: r += '1'; break; default: r += 'd'; } console.log(r);"))
        ' matching is STRICT (1 !== '1')
        Assert.Equal("d", OneLine("var r = ''; switch ('1') { case 1: r += 'num'; break; default: r += 'd'; } console.log(r);"))
        ' no match and no default → nothing happens
        Assert.Equal("after", OneLine("switch (9) { case 1: console.log('no'); } console.log('after');"))
        ' default in the middle still falls through to later cases
        Assert.Equal("d3", OneLine("var r = ''; switch (9) { case 1: r += '1'; break; default: r += 'd'; case 3: r += '3'; } console.log(r);"))
    End Sub

    ' ---------------------------------------------------------------- for-of

    <Fact>
    Public Sub ForOf_ArraysStringsAndDestructuring()
        Assert.Equal("123", OneLine("var s = ''; for (var x of [1, 2, 3]) s += x; console.log(s);"))
        Assert.Equal("abc", OneLine("var s = ''; for (const c of 'abc') s += c; console.log(s);"))
        ' break / continue
        Assert.Equal("4", OneLine("var t = 0; for (const x of [1, 2, 3, 4]) { if (x == 2) continue; if (x == 4) break; t += x; } console.log(t);"))
        ' destructuring target
        Assert.Equal("1:2 3:4", OneLine("var s = ''; for (const [k, v] of [[1, 2], [3, 4]]) s += k + ':' + v + ' '; console.log(s.trim());"))
        ' let binding is per-iteration
        Assert.Equal("0 1 2", OneLine("var fns = []; for (const x of [0, 1, 2]) fns.push(function () { return x; }); console.log(fns[0](), fns[1](), fns[2]());"))
        ' non-iterable → TypeError (catchable)
        Assert.Equal("caught", OneLine("try { for (var x of 5) { } } catch (e) { console.log('caught'); }"))
    End Sub

    ' ---------------------------------------------------------------- labels

    <Fact>
    Public Sub Labels_BreakAndContinue()
        Assert.Equal("001020", OneLine(
            "var log = '';" &
            "outer: for (var i = 0; i < 3; i++) {" &
            "  for (var j = 0; j < 3; j++) {" &
            "    if (j == 1) continue outer;" &
            "    log += i + '' + j;" &
            "  }" &
            "}" &
            "console.log(log);"))
        Assert.Equal("00010210", OneLine(
            "var log = '';" &
            "outer: for (var i = 0; i < 3; i++) {" &
            "  for (var j = 0; j < 3; j++) {" &
            "    if (i == 1 && j == 1) break outer;" &
            "    log += i + '' + j;" &
            "  }" &
            "}" &
            "console.log(log);"))
        ' labeled block + break
        Assert.Equal("a", OneLine("var r = ''; blk: { r += 'a'; if (true) break blk; r += 'b'; } console.log(r);"))
        ' continue to a non-loop label is rejected at parse time
        Assert.Throws(Of ParseException)(Function() Parser.Parse("foo: { continue foo; }"))
    End Sub

    ' ------------------------------------------------------ template literals

    <Fact>
    Public Sub TemplateLiterals_InterpolationAndEscapes()
        Assert.Equal("a2b", OneLine("console.log(`a${1 + 1}b`);"))
        Assert.Equal("hello world", OneLine("var n = 'world'; console.log(`hello ${n}`);"))
        ' nested template inside an interpolation
        Assert.Equal("ab1cd", OneLine("console.log(`a${`b${1}c`}d`);"))
        ' braces inside the placeholder
        Assert.Equal("x[object Object]y", OneLine("console.log(`x${ {a: 1} }y`);"))
        ' escapes and literal newlines
        Assert.Equal("a" & vbLf & "b", OneLine("console.log(`a` + '\n' + `b`);"))
        Assert.Equal("a`b", OneLine("console.log(`a\`b`);"))
        Assert.Equal("$x", OneLine("console.log(`\$x`);"))
        ' multiline template literal (real line break inside the backticks)
        Assert.Equal(2, RunJs("console.log(`line1" & vbLf & "line2`);").Count)
    End Sub

    ' --------------------------------------------------------------- bitwise

    <Fact>
    Public Sub Bitwise_Int32Semantics()
        Assert.Equal("1", OneLine("console.log(5 & 3);"))
        Assert.Equal("7", OneLine("console.log(5 | 3);"))
        Assert.Equal("6", OneLine("console.log(5 ^ 3);"))
        Assert.Equal("-1", OneLine("console.log(~0);"))
        Assert.Equal("8", OneLine("console.log(1 << 3);"))
        Assert.Equal("-4", OneLine("console.log(-8 >> 1);"))
        Assert.Equal("2147483647", OneLine("console.log(-1 >>> 1);"))
        ' >>> produces an unsigned value
        Assert.Equal("4294967295", OneLine("console.log(-1 >>> 0);"))
        ' shift count is masked to 5 bits
        Assert.Equal("2", OneLine("console.log(1 << 33);"))
        ' non-numeric operands go through ToInt32
        Assert.Equal("6", OneLine("console.log('5' & 3);"))
        Assert.Equal("0", OneLine("console.log(NaN & 5);"))
        ' precedence: & binds tighter than | but looser than ==
        Assert.Equal("1", OneLine("console.log(1 | 2 & 0);"))
        Assert.Equal("true", OneLine("console.log((1 | 0) == 1 & 3);"))
    End Sub

    ' ------------------------------------------------------------ nullish ??

    <Fact>
    Public Sub NullishCoalescing()
        Assert.Equal("5", OneLine("console.log(null ?? 5);"))
        Assert.Equal("5", OneLine("console.log(undefined ?? 5);"))
        Assert.Equal("0", OneLine("console.log(0 ?? 5);"))
        Assert.Equal("", OneLine("console.log('' ?? 5);"))
        Assert.Equal("false", OneLine("console.log(false ?? true);"))
        ' short-circuit: the right side is not evaluated
        Assert.Equal("1", OneLine("var n = 0; var v = 1 ?? (n = 9); console.log(n);"))
    End Sub

    ' ------------------------------------------------------- optional chain

    <Fact>
    Public Sub OptionalChaining_MemberIndexCallAndShortCircuit()
        Assert.Equal("undefined", OneLine("var o = null; console.log(o?.x);"))
        Assert.Equal("2", OneLine("var o = {a: {b: 2}}; console.log(o?.a?.b);"))
        ' a nullish link short-circuits the WHOLE remaining chain
        Assert.Equal("undefined", OneLine("var o = {}; console.log(o.x?.y.z);"))
        Assert.Equal("1", OneLine("var o = {a: 1}; console.log(o?.['a']);"))
        ' optional call
        Assert.Equal("undefined", OneLine("var f = null; console.log(f?.());"))
        Assert.Equal("7", OneLine("var o = {m: function () { return 7; }}; console.log(o.m?.());"))
        ' non-optional access on nullish still throws
        Assert.Equal("caught", OneLine("try { var o = null; o.x; } catch (e) { console.log('caught'); }"))
        ' mixing: normal access after a present optional link
        Assert.Equal("3", OneLine("var o = {a: {b: {c: 3}}}; console.log(o?.a.b.c);"))
    End Sub

    ' ----------------------------------------------------------------- spread

    <Fact>
    Public Sub Spread_InCallsArraysAndObjects()
        Assert.Equal("6", OneLine("function f(a, b, c) { return a + b + c; } var xs = [1, 2, 3]; console.log(f(...xs));"))
        Assert.Equal("6", OneLine("function f(a, b, c) { return a + b + c; } console.log(f(1, ...[2, 3]));"))
        Assert.Equal("3", OneLine("console.log([...[1, 2], 3].length);"))
        Assert.Equal("a,b", OneLine("console.log([...'ab'].join(','));"))
        Assert.Equal("3", OneLine("var o = {...{a: 1}, b: 2}; console.log(o.a + o.b);"))
        ' later properties win
        Assert.Equal("2", OneLine("var o = {...{a: 1}, a: 2}; console.log(o.a);"))
    End Sub

    ' ------------------------------------------------- rest / default params

    <Fact>
    Public Sub RestAndDefaultParameters()
        Assert.Equal("2", OneLine("function f(a, ...r) { return r.length; } console.log(f(1, 2, 3));"))
        Assert.Equal("b,c", OneLine("function f(a, ...r) { return r.join(','); } console.log(f('a', 'b', 'c'));"))
        Assert.Equal("11", OneLine("function f(a, b = 10) { return a + b; } console.log(f(1));"))
        Assert.Equal("3", OneLine("function f(a, b = 10) { return a + b; } console.log(f(1, 2));"))
        ' an explicitly-undefined argument also triggers the default
        Assert.Equal("11", OneLine("function f(a, b = 10) { return a + b; } console.log(f(1, undefined));"))
        ' defaults may reference earlier parameters
        Assert.Equal("3", OneLine("function f(a, b = a + 1) { return b; } console.log(f(2));"))
        ' arrow functions support both
        Assert.Equal("4", OneLine("var f = (a = 2) => a * 2; console.log(f());"))
        Assert.Equal("3", OneLine("var f = (...r) => r.length; console.log(f(1, 2, 3));"))
        ' hoisted declarations keep their defaults
        Assert.Equal("5", OneLine("console.log(f()); function f(a = 5) { return a; }"))
    End Sub

    ' ----------------------------------------------------------- destructuring

    <Fact>
    Public Sub Destructuring_ArrayAndObject()
        Assert.Equal("3", OneLine("var [a, b] = [1, 2]; console.log(a + b);"))
        Assert.Equal("2", OneLine("var [x, ...r] = [1, 2, 3]; console.log(r.length);"))
        Assert.Equal("2", OneLine("var [, second] = [1, 2]; console.log(second);"))
        Assert.Equal("t", OneLine("var [s = 't'] = []; console.log(s);"))
        Assert.Equal("9", OneLine("var {u = 9} = {}; console.log(u);"))
        Assert.Equal("2", OneLine("var {p, q: z} = {p: 1, q: 2}; console.log(z);"))
        Assert.Equal("5", OneLine("var {a: {b}} = {a: {b: 5}}; console.log(b);"))
        Assert.Equal("2", OneLine("var {c, ...rest} = {c: 1, d: 2, e: 3}; console.log(rest.d);"))
        ' assignment form (not a declaration)
        Assert.Equal("2 1", OneLine("var a = 1, b = 2; [a, b] = [b, a]; console.log(a, b);"))
        Assert.Equal("7", OneLine("var o = {v: 7}; var v; ({v} = o); console.log(v);"))
        ' nested array in object
        Assert.Equal("4", OneLine("var {list: [first]} = {list: [4, 5]}; console.log(first);"))
    End Sub

    ' -------------------------------------------------- object literal extras

    <Fact>
    Public Sub ObjectLiteral_ShorthandMethodsAccessorsComputedSpread()
        Assert.Equal("1", OneLine("var a = 1; var o = {a}; console.log(o.a);"))
        Assert.Equal("6", OneLine("var o = {f: function (x) { return x * 2; }}; console.log(o.f(3));"))
        Assert.Equal("6", OneLine("var o = {f(x) { return x * 2; }}; console.log(o.f(3));"))
        Assert.Equal("kk", OneLine("var k = 'kk'; var o = {[k]: 1}; console.log(Object.keys(o)[0]);"))
        ' getter / setter through a closure slot (this arrives in tier 2)
        Assert.Equal("42", OneLine("var o = {get x() { return 42; }}; console.log(o.x);"))
        Assert.Equal("5", OneLine(
            "var stored = 0; var o = {set v(n) { stored = n; }, get v() { return stored; }};" &
            "o.v = 5; console.log(o.v);"))
        ' property shorthand with a reserved-word key is still a plain key
        Assert.Equal("1", OneLine("var o = {get: 1}; console.log(o.get);"))
        ' accessors are not own data properties → JSON.stringify skips them
        Assert.Equal("{}", OneLine("console.log(JSON.stringify({get x() { return 1; }}));"))
    End Sub

    ' -------------------------------------------------- comma / in / instanceof

    <Fact>
    Public Sub CommaOperatorAndRelationalOperators()
        Assert.Equal("3", OneLine("var x = (1, 2, 3); console.log(x);"))
        Assert.Equal("true", OneLine("console.log('a' in {a: 1});"))
        Assert.Equal("false", OneLine("console.log('b' in {a: 1});"))
        Assert.Equal("true", OneLine("console.log(0 in [1, 2]);"))
        Assert.Equal("false", OneLine("console.log(2 in [1, 2]);"))
        Assert.Equal("true", OneLine("console.log([] instanceof Array);"))
        Assert.Equal("false", OneLine("console.log({} instanceof Array);"))
        Assert.Equal("true", OneLine("console.log(function () { } instanceof Function);"))
        ' primitives are not instances of their wrapper constructors
        Assert.Equal("false", OneLine("console.log('a' instanceof String);"))
    End Sub

    ' -------------------------------------------------------- delete / void

    <Fact>
    Public Sub DeleteAndVoid()
        Assert.Equal("undefined", OneLine("var o = {a: 1}; console.log(delete o.a, o.a);"))
        Assert.Equal("true", OneLine("var o = {a: 1}; console.log(delete o.a);"))
        Assert.Equal("true", OneLine("var a = 1; console.log(delete a);"))
        Assert.Equal("undefined", OneLine("console.log(void 0);"))
        ' the operand is still evaluated
        Assert.Equal("1", OneLine("var n = 0; void (n = 1); console.log(n);"))
    End Sub

    ' ---------------------------------------------------- numeric separators

    <Fact>
    Public Sub NumericSeparators()
        Assert.Equal("1000000", OneLine("console.log(1_000_000);"))
        Assert.Equal("8191", OneLine("console.log(0x1F_FF);"))
        Assert.Equal("1000.5", OneLine("console.log(1_000.5);"))
        Assert.Equal("1000", OneLine("console.log(1e1_0 / 1e7);"))
        ' malformed separators are rejected by the lexer
        Assert.Throws(Of ParseException)(Function() Lexer.Lex("var x = 1__0;"))
        Assert.Throws(Of ParseException)(Function() Lexer.Lex("var x = 1_;"))
    End Sub

    ' =========================================================== code generator

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    <Fact>
    Public Sub CodeGen_SwitchUsesIfGotoChain()
        Dim code = Generate("switch (x) { case 1: console.log(1); break; default: console.log('d'); }")
        Assert.Contains("JsRuntime.JsStrictEq", code)
        Assert.Contains("__case", code)
        Assert.Contains("__swend", code)
        Assert.DoesNotContain("Select Case", code)      ' loose comparison + no fall-through
    End Sub

    <Fact>
    Public Sub CodeGen_LoopsAndLabels()
        Dim forOf = Generate("for (const x of xs) { console.log(x); }")
        Assert.Contains("JsRuntime.JsIterableValues(", forOf)

        Dim labeled = Generate("outer: for (var i = 0; i < 3; i++) { continue outer; }")
        Assert.Contains("__lcont", labeled)
        Assert.Contains("GoTo", labeled)

        Dim labeledBreak = Generate("outer: while (true) { break outer; }")
        Assert.Contains("__lexit", labeledBreak)
    End Sub

    <Fact>
    Public Sub CodeGen_TemplateSpreadAndParameters()
        Dim tpl = Generate("console.log(`a${1 + 1}b`);")
        Assert.Contains("JsRuntime.JsStr(", tpl)

        Dim spreadCall = Generate("f(...xs);")
        Assert.Contains("JsRuntime.JsSpread(", spreadCall)
        Assert.Contains("JsRuntime.JsBuildArgs(", spreadCall)

        Dim spreadArr = Generate("var a = [...xs, 1];")
        Assert.Contains("JsRuntime.JsArrayFrom(", spreadArr)

        Dim spreadObj = Generate("var o = {...base, a: 1};")
        Assert.Contains("JsRuntime.JsObjFrom(", spreadObj)

        Dim rest = Generate("function f(a, ...r) { return r; }")
        Assert.Contains("JsRuntime.JsRestArgs(", rest)

        Dim dflt = Generate("function f(a = 1) { return a; }")
        Assert.Contains("IsNot JsRuntime.Undef", dflt)
    End Sub

    <Fact>
    Public Sub CodeGen_DestructuringAndOptionalChain()
        Dim arr = Generate("var [a, b] = pair;")
        Assert.Contains("JsRuntime.JsIndex(", arr)
        Assert.Contains("JsRuntime.JsArrayRest(", Generate("var [a, ...r] = pair;"))

        Dim obj = Generate("var {a, ...rest} = o;")
        Assert.Contains("JsRuntime.JsGet(", obj)
        Assert.Contains("JsRuntime.JsObjectRest(", obj)

        Dim oc = Generate("console.log(a?.b.c);")
        Assert.Contains("JsRuntime.JsChainGet(", oc)
        Assert.Contains("OptionalChainShortCircuit", oc)
        Assert.Contains("JsRuntime.JsChainInvoke(", Generate("f?.();"))
    End Sub

    <Fact>
    Public Sub CodeGen_OperatorsAndBuiltins()
        Assert.Contains("JsRuntime.JsBitAnd(", Generate("var x = a & b;"))
        Assert.Contains("JsRuntime.JsBitNot(", Generate("var x = ~a;"))
        Assert.Contains("JsRuntime.JsNullish(", Generate("var x = a ?? b;"))
        Assert.Contains("JsRuntime.JsIn(", Generate("var x = k in o;"))
        Assert.Contains("JsRuntime.JsInstanceOf(", Generate("var x = a instanceof B;"))
        Assert.Contains("JsRuntime.JsDelete(", Generate("delete o.a;"))
        Assert.Contains("JsRuntime.JsVoid(", Generate("void 0;"))
        Assert.Contains("JsRuntime.JsAccessor(", Generate("var o = {get x() { return 1; }};"))
    End Sub

End Class
