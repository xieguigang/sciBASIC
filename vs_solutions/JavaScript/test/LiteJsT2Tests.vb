Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Tier 2 (object orientation) coverage: class declarations and expressions,
''' constructors, instance/static members, getters/setters, fields, inheritance,
''' super, this binding and instanceof.
''' </summary>
Public Class LiteJsT2Tests

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

    ' ------------------------------------------------------------- basics

    <Fact>
    Public Sub Class_ConstructorAndMethods()
        Assert.Equal("3 1 2", OneLine(
            "class Point {" &
            "  constructor(x, y) { this.x = x; this.y = y; }" &
            "  sum() { return this.x + this.y; }" &
            "}" &
            "var p = new Point(1, 2);" &
            "console.log(p.sum(), p.x, p.y);"))
        ' a class is a function value with a name
        Assert.Equal("function Point", OneLine("class Point { } console.log(typeof Point, Point.name);"))
        ' chaining methods through 'this'
        Assert.Equal("2", OneLine(
            "class C { constructor() { this.n = 0; } inc() { this.n = this.n + 1; return this; } }" &
            "var c = new C(); c.inc().inc(); console.log(c.n);"))
        ' class expression (anonymous and named)
        Assert.Equal("k", OneLine("var K = class { hello() { return 'k'; } }; console.log(new K().hello());"))
    End Sub

    <Fact>
    Public Sub Class_StaticMembers()
        Assert.Equal("s 5", OneLine(
            "class M { static make() { return 's'; } static n = 5; }" &
            "console.log(M.make(), M.n);"))
        ' static members are inherited by subclasses
        Assert.Equal("hi", OneLine(
            "class A { static hi() { return 'hi'; } }" &
            "class B extends A { }" &
            "console.log(B.hi());"))
    End Sub

    <Fact>
    Public Sub Class_AccessorsAndFields()
        Assert.Equal("10", OneLine(
            "class T { constructor() { this._v = 0; }" &
            "  get v() { return this._v; }" &
            "  set v(n) { this._v = n * 2; } }" &
            "var t = new T(); t.v = 5; console.log(t.v);"))
        Assert.Equal("3", OneLine(
            "class F { n = 1; m = 2; total() { return this.n + this.m; } }" &
            "console.log(new F().total());"))
        ' field initialisers see 'this'
        Assert.Equal("2", OneLine("class G { a = 1; b = this.a + 1; } console.log(new G().b);"))
    End Sub

    ' -------------------------------------------------------------- this

    <Fact>
    Public Sub This_BindingRules()
        ' method call binds this to the receiver
        Assert.Equal("1", OneLine("var g = {v: 1, m: function () { return this.v; }}; console.log(g.m());"))
        ' a detached method loses the receiver (this = global, no 'v')
        Assert.Equal("undefined", OneLine(
            "var g = {v: 1, m: function () { return this.v; }}; var m2 = g.m; console.log(m2());"))
        ' plain calls see the global object (non-strict)
        Assert.Equal("undefined", OneLine("function f() { return this.missing; } console.log(f());"))
        ' arrow functions keep the enclosing 'this'
        Assert.Equal("7", OneLine(
            "class A { constructor() { this.n = 7; } get() { var f = () => this.n; return f(); } }" &
            "console.log(new A().get());"))
        ' indexed method call also binds the receiver
        Assert.Equal("9", OneLine("var o = {m: function () { return 9; }}; console.log(o['m']());"))
    End Sub

    ' -------------------------------------------------------- inheritance

    <Fact>
    Public Sub Inheritance_SuperAndInstanceOf()
        Dim src =
            "class Animal {" &
            "  constructor(name) { this.name = name; }" &
            "  speak() { return this.name + ' makes a sound'; }" &
            "}" &
            "class Dog extends Animal {" &
            "  constructor(name) { super(name); this.kind = 'dog'; }" &
            "  speak() { return super.speak() + ' (woof)'; }" &
            "}"

        Assert.Equal("Rex makes a sound (woof)", OneLine(src & "console.log(new Dog('Rex').speak());"))
        Assert.Equal("Rex dog", OneLine(src & "var d = new Dog('Rex'); console.log(d.name, d.kind);"))
        Assert.Equal("true true", OneLine(src & "var d = new Dog('Rex'); console.log(d instanceof Dog, d instanceof Animal);"))
        Assert.Equal("false", OneLine(src & "var a = new Animal('x'); console.log(a instanceof Dog);"))
        Assert.Equal("true", OneLine(src & "class P { } console.log(new P() instanceof Object);"))
        ' multi-level super chain
        Assert.Equal("ABC", OneLine(
            "class A { f() { return 'A'; } }" &
            "class B extends A { f() { return super.f() + 'B'; } }" &
            "class C extends B { f() { return super.f() + 'C'; } }" &
            "console.log(new C().f());"))
        ' inherited prototypes are visible on the instance
        Assert.Equal("1", OneLine(
            "class Animal { constructor() { this.n = 1; } }" &
            "class Dog extends Animal { }" &
            "console.log(new Dog().n);"))
    End Sub

    <Fact>
    Public Sub Class_Errors()
        ' calling a class without new is a TypeError
        Assert.Equal("caught", OneLine("class C { } try { C(); } catch (e) { console.log('caught'); }"))
        ' extending a non-constructor is a TypeError
        Assert.Equal("caught", OneLine("try { class X extends 5 { } } catch (e) { console.log('caught'); }"))
        ' new on a non-callable is a TypeError
        Assert.Equal("caught", OneLine("try { new 5(); } catch (e) { console.log('caught'); }"))
        ' super is only valid in a derived class
        Assert.Equal("caught", OneLine(
            "class A { f() { return super.f(); } }" &
            "try { new A().f(); } catch (e) { console.log('caught'); }"))
        ' builtin constructors still work alongside classes
        Assert.Equal("3 0", OneLine("var a = new Array(3); console.log(a.length, (new Array(1, 2)).length - 2);"))
    End Sub

    ' ============================================== code generator

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    <Fact>
    Public Sub CodeGen_ClassAndConstructors()
        Dim code = Generate("class P { constructor(x) { this.x = x; } m() { return this.x; } static s() { return 1; } }")
        Assert.Contains("JsRuntime.JsDefineClass(", code)
        Assert.Contains("""i"", ""m""", code)                 ' instance member
        Assert.Contains("""s"", ""s""", code)                 ' static member
        Assert.Contains("JsRuntime.CurrentThis", code)       ' 'this' binding
        Assert.Contains("Dim this As JsValue = JsRuntime.CurrentThis", code)

        Assert.Contains("JsRuntime.JsNew(", Generate("class P { } var p = new P();"))
    End Sub

    <Fact>
    Public Sub CodeGen_MethodCallsAndSuper()
        Dim mc = Generate("class P { m() { return 1; } } var p = new P(); p.m();")
        Assert.Contains("JsRuntime.JsInvokeMethod(", mc)

        Dim inherit = Generate(
            "class A { f() { return 1; } }" &
            "class B extends A { constructor() { super(); } f() { return super.f(); } }")
        Assert.Contains("JsRuntime.JsSuperConstructor(", inherit)
        Assert.Contains("JsRuntime.JsSuperInvoke(", inherit)

        Dim idx = Generate("var o = {m: function () { }}; o['m']();")
        Assert.Contains("JsRuntime.JsInvokeIndex(", idx)
    End Sub

End Class
