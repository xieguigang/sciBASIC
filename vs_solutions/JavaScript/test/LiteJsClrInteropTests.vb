#Region "Microsoft.VisualBasic::d0ce149f5cde93df33718b6fe0794968, vs_solutions\JavaScript\test\LiteJsClrInteropTests.vb"

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

    '   Total Lines: 358
    '    Code Lines: 273 (76.26%)
    ' Comment Lines: 33 (9.22%)
    '    - Xml Docs: 45.45%
    ' 
    '   Blank Lines: 52 (14.53%)
    '     File Size: 15.80 KB


    ' Class LiteJsClrInteropTests
    ' 
    '     Function: ErrorName, Fixture, OneLine, RunJs
    ' 
    '     Sub: Clr_CollectionLengthIndexLoopAndSpread, Clr_DelegateParameterIsRejectedWithReadableError, Clr_DeleteIsRejected, Clr_EnumAndDateAndGuidConversion, Clr_ExceptionIsCatchableFromScript
    '          Clr_ExplicitOnlyForcesWhitelistModeForEveryType, Clr_FaultedTaskBecomesRejection, Clr_IdentityIsStable, Clr_IndexerIsReachedWithBrackets, Clr_InOperatorAndKeys
    '          Clr_InstallHostBatchBindsAllObjects, Clr_JsExposeSwitchesTheTypeToWhitelistMode, Clr_JsIgnoreAndJsNameAreHonoured, Clr_NestedObjectStaysReachable, Clr_NewCreatesAnInstanceAndInstanceofWorks
    '          Clr_NoMatchingOverloadReportsCandidates, Clr_OverloadResolutionPicksByArgumentType, Clr_ParamsAndOptionalParameters, Clr_PlainEnumerableIteratesWithoutIndexer, Clr_ReadOnlyPropertyWriteIsTypeError
    '          Clr_ReadsPropertiesAndFields, Clr_RegisteredHostGlobalIsVisibleToNewEngines, Clr_RethrownErrorKeepsTheOriginalException, Clr_SpreadCopiesDataMembers, Clr_StaticMembersAndConstruction
    '          Clr_StructWritesDoNotPersist, Clr_TaskBecomesPromise, Clr_TypeCannotBeCalledWithoutNew, Clr_TypeofDisplayAndJson, Clr_UnknownMemberReadIsUndefinedAndWriteIsTypeError
    '          Clr_UnknownMethodCallIsTypeError, Clr_UnregisteredGlobalIsAReferenceError, Clr_WriteOnlyPropertyReadIsTypeError, Clr_WritesPropertiesAndFields, CodeGen_EntryPointTakesTheOptionalHostArgument
    '          CodeGen_HostIdentifiersResolveAtRuntime
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' The CLR reflection bridge: host objects (see <see cref="ClrHostFixture"/>) injected
''' with <c>Interpreter.DefineGlobal</c> / <c>ClrHost</c> behave like JS objects —
''' property and field access, method calls with overload resolution, indexers,
''' collections, statics, <c>new</c>, <c>instanceof</c>, Tasks and error boundaries.
''' <para>
''' Code-generator coverage is structural (the generated VB cannot be compiled
''' in-process): the translation of a script using a host object must resolve the name
''' through <c>JsRuntime.ResolveHostGlobal</c> and the entry point must carry the
''' optional <see cref="ClrHost"/> argument.
''' </para>
''' </summary>
Public Class LiteJsClrInteropTests

    ' ======================================================== helpers

    ''' <summary>Run a script with host objects bound as globals; returns the console lines.</summary>
    Private Shared Function RunJs(source As String,
                                  ParamArray globals As (Name As String, Value As Object)()) As List(Of String)
        Dim io As New LogTextIO
        Dim engine As New Interpreter(io)
        For Each g In globals
            engine.DefineGlobal(g.Name, g.Value)
        Next
        engine.Run(Parser.Parse(source))
        Return io.Lines
    End Function

    Private Shared Function OneLine(source As String,
                                    ParamArray globals As (Name As String, Value As Object)()) As String
        Return RunJs(source, globals)(0)
    End Function

    ''' <summary>Assert a script error and return its JS error name (e.g. "TypeError").</summary>
    Private Shared Function ErrorName(source As String,
                                      ParamArray globals As (Name As String, Value As Object)()) As String
        Dim ex = Assert.Throws(Of JsRuntimeException)(Function() RunJs(source, globals))
        Dim err = TryCast(ex.Payload, JsError)
        Assert.NotNull(err)
        Return err.Name
    End Function

    ''' <summary>A fresh host object per call ('Host' would collide with the 'host' variable).</summary>
    Private Shared Function Fixture() As ClrHostFixture
        Return New ClrHostFixture()
    End Function

    ' ======================================================== members

    <Fact>
    Public Sub Clr_ReadsPropertiesAndFields()
        ' member names match exactly: Title / Counter (use [JsName] for other spellings)
        Assert.Equal("hello 0", OneLine("console.log(host.Title, host.Counter);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_WritesPropertiesAndFields()
        Assert.Equal("x 7 true true", OneLine(
            "host.Title = 'x'; host.Counter = 7;" &
            "console.log(host.Title, host.Counter, host.Title === 'x', host.Counter === 7);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_ReadOnlyPropertyWriteIsTypeError()
        Assert.Equal("TypeError", ErrorName("host.Id = 1;", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_WriteOnlyPropertyReadIsTypeError()
        Assert.Equal("TypeError", ErrorName("console.log(host.WriteOnlyValue);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_UnknownMemberReadIsUndefinedAndWriteIsTypeError()
        Assert.Equal("undefined false", OneLine("console.log(host.Nope, 'Nope' in host);", ("host", Fixture())))
        Assert.Equal("TypeError", ErrorName("host.Nope = 1;", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_InOperatorAndKeys()
        ' data members are enumerable properties; methods are not own properties (JS semantics)
        Assert.Equal("true false true false", OneLine(
            "var k = Object.keys(host);" &
            "console.log('Title' in host, 'Nope' in host, k.indexOf('Title') >= 0, k.indexOf('Add') >= 0);",
            ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_TypeofDisplayAndJson()
        Assert.Equal("object true", OneLine(
            "console.log(typeof host, JSON.stringify(host).indexOf('hello') >= 0);", ("host", Fixture())))
        Assert.Equal("true", OneLine(
            "console.log(host.toString().indexOf('ClrHostFixture') >= 0);", ("host", Fixture())))
        Assert.Equal("true", OneLine(
            "console.log(String(host) === '[object Object]');", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_IdentityIsStable()
        ' wrapping is identity-stable, so === works like it does for JS objects
        Assert.Equal("true true false", OneLine(
            "console.log(host === host, host.Child === host.Child, host.Child === host);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_DeleteIsRejected()
        Assert.Equal("TypeError", ErrorName("delete host.Title;", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_NestedObjectStaysReachable()
        Assert.Equal("child 5", OneLine("console.log(host.Child.Label, host.Child.Value);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_SpreadCopiesDataMembers()
        Assert.Equal("hello 1 undefined", OneLine(
            "var copy = { ...host, extra: 1 }; console.log(copy.Title, copy.extra, typeof copy.Add);",
            ("host", Fixture())))
    End Sub

    ' ======================================================== methods

    <Fact>
    Public Sub Clr_OverloadResolutionPicksByArgumentType()
        ' JS numbers are Doubles, strings are strings: the matching overload wins
        Assert.Equal("3 4 ab", OneLine(
            "console.log(host.Add(1, 2), host.Add(1.5, 2.5), host.Add('a', 'b'));", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_ParamsAndOptionalParameters()
        Assert.Equal("6 0", OneLine("console.log(host.SumAll(1, 2, 3), host.SumAll());", ("host", Fixture())))
        Assert.Equal("hi world hi x", OneLine("console.log(host.Greet(), host.Greet('x'));", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_NoMatchingOverloadReportsCandidates()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs("host.Add(true);", ("host", Fixture())))
        Dim err = TryCast(ex.Payload, JsError)
        Assert.NotNull(err)
        Assert.Equal("TypeError", err.Name)
        Assert.Contains("no overload of 'Add'", err.Message)
        Assert.Contains("Add(Double, Double)", err.Message)
    End Sub

    <Fact>
    Public Sub Clr_DelegateParameterIsRejectedWithReadableError()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs("host.MapIt(function(x) { return x; });", ("host", Fixture())))
        Dim err = TryCast(ex.Payload, JsError)
        Assert.NotNull(err)
        Assert.Contains("delegate parameters are not supported", err.Message)
    End Sub

    <Fact>
    Public Sub Clr_UnknownMethodCallIsTypeError()
        Assert.Equal("TypeError", ErrorName("host.Nope();", ("host", Fixture())))
    End Sub

    ' ======================================================== indexers and collections

    <Fact>
    Public Sub Clr_IndexerIsReachedWithBrackets()
        Assert.Equal("item-1 item-9", OneLine("console.log(host[1], host[9]);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_CollectionLengthIndexLoopAndSpread()
        Assert.Equal("3 1.5", OneLine("console.log(host.Items.length, host.Items[0]);", ("host", Fixture())))
        Assert.Equal("7", OneLine(
            "var total = 0; for (const v of host.Items) { total += v; } console.log(total);", ("host", Fixture())))
        Assert.Equal("[1.5,2.5,3]", OneLine(
            "console.log(JSON.stringify([...host.Items]));", ("host", Fixture())))
        Assert.Equal("0,1,2", OneLine(
            "console.log(Object.keys(host.Items).join(','));", ("host", Fixture())))
        Assert.Equal("[1.5,2.5,3]", OneLine("console.log(JSON.stringify(host.Items));", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_PlainEnumerableIteratesWithoutIndexer()
        Assert.Equal("30 2", OneLine(
            "var total = 0; for (const v of host.Sequence) { total += v; }" &
            "console.log(total, host.Sequence.length);", ("host", Fixture())))
        Assert.Equal("undefined", OneLine("console.log(host.Sequence[0]);", ("host", Fixture())))
    End Sub

    ' ======================================================== statics, new, instanceof

    <Fact>
    Public Sub Clr_StaticMembersAndConstruction()
        Assert.Equal("5 static", OneLine(
            "console.log(types.StaticSum(2, 3), types.StaticLabel);", ("types", GetType(ClrHostFixture))))
        Assert.Equal("function static", OneLine(
            "types.StaticLabel = 'static'; console.log(typeof types, types.StaticLabel);",
            ("types", GetType(ClrHostFixture))))
    End Sub

    <Fact>
    Public Sub Clr_NewCreatesAnInstanceAndInstanceofWorks()
        Assert.Equal("made true true", OneLine(
            "var inst = new types(); inst.Title = 'made';" &
            "console.log(inst.Title, inst instanceof types, inst instanceof Object);",
            ("types", GetType(ClrHostFixture))))
    End Sub

    <Fact>
    Public Sub Clr_TypeCannotBeCalledWithoutNew()
        Assert.Equal("TypeError", ErrorName("types();", ("types", GetType(ClrHostFixture))))
    End Sub

    ' ======================================================== conversions

    <Fact>
    Public Sub Clr_EnumAndDateAndGuidConversion()
        Dim modes = RunJs("console.log(host.Mode); host.Mode = 'Active'; console.log(host.Mode);",
                          ("host", Fixture()))
        Assert.Equal("0 1", String.Join(" ", modes))       ' numeric, assignable by name
        Assert.Equal("2020 true", OneLine(
            "console.log(host.Stamp.getFullYear(), host.Stamp.getTime() > 0);", ("host", Fixture())))
        Assert.Equal("11111111-2222-3333-4444-555555555555", OneLine("console.log(host.Token);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_StructWritesDoNotPersist()
        ' a struct property hands out a boxed copy: writing it changes only that copy
        Dim points = RunJs(
            "console.log(host.Point.X); host.Point.X = 99; console.log(host.Point.X);", ("host", Fixture()))
        Assert.Equal("1 1", String.Join(" ", points))
    End Sub

    ' ======================================================== tasks and errors

    <Fact>
    Public Sub Clr_TaskBecomesPromise()
        Assert.Equal("then 41 await 41", String.Join(" ", RunJs(
            "host.FetchAsync().then(v => console.log('then', v));" &
            "(async function() { const v = await host.FetchAsync(); console.log('await', v); })();",
            ("host", Fixture()))))
    End Sub

    <Fact>
    Public Sub Clr_FaultedTaskBecomesRejection()
        Assert.Equal("caught async boom", OneLine(
            "host.FailsAsync().catch(e => console.log('caught', e.message));", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_ExceptionIsCatchableFromScript()
        Assert.Equal("InvalidOperationException boom", OneLine(
            "try { host.Explode(); } catch (e) { console.log(e.name, e.message); }", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_RethrownErrorKeepsTheOriginalException()
        Dim ex = Assert.Throws(Of JsRuntimeException)(
            Function() RunJs("try { host.Explode(); } catch (e) { throw e; }", ("host", Fixture())))
        Dim original = ClrInterop.ToHostException(ex)
        Dim typed = TryCast(original, InvalidOperationException)
        Assert.NotNull(typed)
        Assert.Equal("boom", typed.Message)
    End Sub

    ' ======================================================== visibility control

    <Fact>
    Public Sub Clr_JsIgnoreAndJsNameAreHonoured()
        Assert.Equal("undefined alias undefined", OneLine(
            "console.log(typeof host.Hidden, host.renamed, typeof host.OriginalName);", ("host", Fixture())))
    End Sub

    <Fact>
    Public Sub Clr_ExplicitOnlyForcesWhitelistModeForEveryType()
        ClrInterop.ExplicitOnly = True
        Try
            ' a type without any [JsExpose] member exposes nothing while the switch is on
            Assert.Equal("undefined undefined", OneLine(
                "console.log(typeof host.Title, typeof host.Add);", ("host", Fixture())))
            Assert.Equal("yes undefined", OneLine(
                "console.log(w.Visible, typeof w.Invisible);", ("w", New ClrWhitelistFixture())))
        Finally
            ClrInterop.ExplicitOnly = False
        End Try
    End Sub

    <Fact>
    Public Sub Clr_JsExposeSwitchesTheTypeToWhitelistMode()
        Assert.Equal("yes undefined function undefined", OneLine(
            "console.log(w.Visible, typeof w.Invisible, typeof w.VisibleMethod, typeof w.InvisibleMethod);",
            ("w", New ClrWhitelistFixture())))
        Assert.Equal("yes undefined", OneLine(
            "console.log(w.Visible, w.Invisible);", ("w", New ClrWhitelistFixture())))
    End Sub

    ' ======================================================== host injection channels

    <Fact>
    Public Sub Clr_RegisteredHostGlobalIsVisibleToNewEngines()
        ClrInterop.Register("host", Fixture())
        Try
            Assert.Equal("hello", OneLine("console.log(host.Title);"))
            Dim stored As Object = Nothing
            Assert.True(ClrInterop.TryGetHostGlobal("host", stored))
            Assert.IsType(Of ClrObject)(stored)      ' the registry holds the wrapped proxy
        Finally
            ClrInterop.Clear()
        End Try
    End Sub

    <Fact>
    Public Sub Clr_InstallHostBatchBindsAllObjects()
        Dim host As New ClrHost()
        host.Add("host", Fixture())
        host.Add("types", GetType(ClrHostFixture))
        Try
            Dim io As New LogTextIO
            Dim engine As New Interpreter(io)
            engine.InstallHost(host)
            engine.Run(Parser.Parse("console.log(host.Title, types.name);"))
            Assert.Equal("hello ClrHostFixture", io.Lines(0))
        Finally
            ClrInterop.Clear()
        End Try
    End Sub

    <Fact>
    Public Sub Clr_UnregisteredGlobalIsAReferenceError()
        ' the bridge widens global resolution to host objects only; anything else is a
        ' normal JS ReferenceError at run time
        Dim lines = RunJs("try { definitelyNotRegistered; } catch (e) { console.log(e.name); }")
        Assert.Equal("ReferenceError", lines(0))
    End Sub

    ' ======================================================== code generator parity

    <Fact>
    Public Sub CodeGen_HostIdentifiersResolveAtRuntime()
        Dim code = New CodeGenerator().Generate(
            Parser.Parse("console.log(host.Title); host.Title = 'x'; console.log(host.Add(1, 2));"))
        Assert.Contains("JsRuntime.ResolveHostGlobal(""host"")", code)
        Assert.Contains("JsRuntime.JsInvokeMethod", code)
        Assert.Contains("JsRuntime.JsSet(", code)
    End Sub

    <Fact>
    Public Sub CodeGen_EntryPointTakesTheOptionalHostArgument()
        Dim code = New CodeGenerator().Generate(Parser.Parse("var x = 1;"))
        Assert.Contains(
            "Public Shared Sub Run(io As LiteJsRuntime.ScriptIO, Optional host As LiteJsRuntime.ClrHost = Nothing)", code)
        Assert.Contains("If host IsNot Nothing Then host.Install()", code)
    End Sub

End Class

