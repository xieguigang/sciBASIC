Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Tier 4 coverage: Promise and async/await driven by the synchronous microtask
''' queue (the engine has no event loop, so all promises settle synchronously and
''' the queue is drained at <c>await</c> and at the end of a script).
''' </summary>
Public Class LiteJsT4Tests

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

    ' ------------------------------------------------------------ promises

    <Fact>
    Public Sub Promise_ThenCatchChain()
        Assert.Equal("1", OneLine("Promise.resolve(1).then(v => console.log(v));"))
        Assert.Equal("2", OneLine("Promise.resolve(1).then(v => v + 1).then(v => console.log(v));"))
        Assert.Equal("boom", OneLine("Promise.reject('boom').catch(r => console.log(r));"))
        Assert.Equal("9", OneLine("new Promise(function (resolve) { resolve(9); }).then(v => console.log(v));"))
        Assert.Equal("10", OneLine(
            "new Promise(function (resolve, reject) { resolve(10); }).then(v => console.log(v));"))
        ' a throwing handler rejects the chained promise
        Assert.Equal("bad", OneLine(
            "Promise.resolve(1).then(v => { throw new Error('bad'); }).catch(e => console.log(e.message));"))
        Assert.Equal("function", OneLine("console.log(typeof Promise);"))
    End Sub

    <Fact>
    Public Sub Promise_OrderingAndStatics()
        ' microtasks run after the synchronous body, like JS
        Assert.Equal(2, RunJs("Promise.resolve(1).then(() => console.log('async')); console.log('sync');").Count)
        Dim lines = RunJs("Promise.resolve(1).then(() => console.log('async')); console.log('sync');")
        Assert.Equal("sync", lines(0))
        Assert.Equal("async", lines(1))

        Assert.Equal("1,2", OneLine("Promise.all([Promise.resolve(1), Promise.resolve(2)]).then(v => console.log(v.join(',')));"))
        Assert.Equal("nope", OneLine("Promise.all([Promise.resolve(1), Promise.reject('nope')]).catch(e => console.log(e));"))
        Assert.Equal("fulfilled rejected", OneLine(
            "Promise.allSettled([Promise.resolve(1), Promise.reject('x')])" &
            ".then(rs => console.log(rs[0].status, rs[1].status));"))
        Assert.Equal("3", OneLine(
            "Promise.race([Promise.resolve(3), Promise.resolve(4)]).then(v => console.log(v));"))
    End Sub

    ' -------------------------------------------------------------- async

    <Fact>
    Public Sub AsyncFunctionsAndAwait()
        ' an async function returns a promise settled by its return value
        Assert.Equal("1", OneLine("async function f() { return 1; } f().then(v => console.log(v));"))
        Assert.Equal("3", OneLine("var f = async () => 3; f().then(v => console.log(v));"))
        ' await unwraps a promise inside an async function
        Assert.Equal("got 5", OneLine(
            "async function f() { return 5; }" &
            "async function main() { var v = await f(); console.log('got ' + v); }" &
            "main();"))
        Assert.Equal("7", OneLine(
            "async function main() { console.log(await Promise.resolve(7)); }" &
            "main();"))
        ' a rejection inside an async function becomes a rejected promise
        Assert.Equal("caught x", OneLine(
            "async function f() { throw new Error('x'); }" &
            "f().catch(e => console.log('caught ' + e.message));"))
        ' await on a rejected promise is catchable with try/catch
        Assert.Equal("caught e", OneLine(
            "async function f() { try { await Promise.reject('e'); } catch (e) { console.log('caught ' + e); } }" &
            "f();"))
        ' await of a non-promise yields the value itself
        Assert.Equal("plain", OneLine(
            "async function main() { console.log(await 'plain'); } main();"))
        ' awaiting inside a loop keeps the body sequential
        Assert.Equal("123", OneLine(
            "async function main() {" &
            "  var s = '';" &
            "  for (var x of [1, 2, 3]) { s += await Promise.resolve(x); }" &
            "  console.log(s);" &
            "} main();"))
        ' async methods on classes
        Assert.Equal("1", OneLine(
            "class A { async m() { return 1; } } new A().m().then(v => console.log(v));"))
    End Sub

    ' ============================================== code generator

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    <Fact>
    Public Sub CodeGen_AsyncAndAwait()
        Dim code = Generate("async function f() { return 1; }")
        Assert.Contains("New LiteJs.Runtime.JsPromise()", code)
        Assert.Contains(".Fulfill(", code)
        Assert.Contains(".Reject(", code)

        Assert.Contains("JsRuntime.JsAwait(", Generate("function g() { return 1; } async function f() { return await g(); }"))
        Assert.Contains("JsRuntime.DrainMicrotasks()", Generate("console.log(1);"))
    End Sub

End Class
