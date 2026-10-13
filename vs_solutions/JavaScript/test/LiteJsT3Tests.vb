#Region "Microsoft.VisualBasic::0197abc76ff5dc6effd8d864cae02b6b, vs_solutions\JavaScript\test\LiteJsT3Tests.vb"

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

    '   Total Lines: 182
    '    Code Lines: 138 (75.82%)
    ' Comment Lines: 21 (11.54%)
    '    - Xml Docs: 23.81%
    ' 
    '   Blank Lines: 23 (12.64%)
    '     File Size: 10.48 KB


    ' Class LiteJsT3Tests
    ' 
    '     Function: Generate, OneLine, RunJs
    ' 
    '     Sub: ArrayLibrary, CodeGen_RegexAndBuiltins, Date_Basics, Errors_ConstructionAndProperties, ObjectMathNumberJsonLibrary
    '          Regex_LiteralsAndDivisionDisambiguation, Regex_StringInterop, StringLibrary
    ' 
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Xunit

''' <summary>
''' Tier 3 coverage: regular-expression literals and RegExp interop, the Error
''' hierarchy, Date and the String/Array/Object/Math/Number/JSON library
''' completion.
''' </summary>
Public Class LiteJsT3Tests

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

    ' ------------------------------------------------------------- regex

    <Fact>
    Public Sub Regex_LiteralsAndDivisionDisambiguation()
        Assert.Equal("true", OneLine("console.log(/ab+c/.test('abbbc'));"))
        Assert.Equal("1", OneLine("console.log(6 / 2 / 3);"))                    ' division still works
        Assert.Equal("12", OneLine("console.log(1 + /2/.source);"))              ' literal after '+'
        Assert.Equal("object", OneLine("console.log(typeof /x/);"))
        ' a regex right after ')' is treated as division (no statement here), after
        ' 'return' as a regex
        Assert.Equal("true", OneLine("function f() { return /\d+/.test('42'); } console.log(f());"))
        Assert.Equal("a+ gi", OneLine("var re = /a+/gi; console.log(re.source, re.flags);"))
        Assert.Equal("true", OneLine("console.log(/a/i.test('A'));"))
        ' a global regex is stateful: lastIndex advances between calls
        Assert.Equal("1 true 2", OneLine("var re = /a/g; re.test('aaa'); console.log(re.lastIndex, re.test('aaa'), re.lastIndex);"))
    End Sub

    <Fact>
    Public Sub Regex_StringInterop()
        Assert.Equal("1,2", OneLine("console.log('a1b2'.match(/\d/g).join(','));"))
        Assert.Equal("123", OneLine("console.log(/\d+/.exec('ab123')[0]);"))
        Assert.Equal("null", OneLine("console.log(String(/\d+/.exec('abc')));"))
        Assert.Equal("aXbX", OneLine("console.log('a1b2'.replace(/\d/g, 'X'));"))
        Assert.Equal("14/10/2026", OneLine(
            "console.log('2026-10-14'.replace(/(\d+)-(\d+)-(\d+)/, '$3/$2/$1'));"))
        ' only the first match is replaced without the g flag
        Assert.Equal("aXb2", OneLine("console.log('a1b2'.replace(/\d/, 'X'));"))
        ' function replacement doubles each digit
        Assert.Equal("a2b6", OneLine(
            "console.log('a1b3'.replace(/\d/g, function (m) { return Number(m) * 2; }));"))
        Assert.Equal("1", OneLine("console.log('abc'.search(/b/));"))
        ' a trailing separator produces a trailing empty element (JS semantics)
        Assert.Equal("4", OneLine("console.log('a1b2c3'.split(/\d/).length);"))
        Assert.Equal("a|b|c|", OneLine("console.log('a1b2c3'.split(/\d/).join('|'));"))
        Assert.Equal("2", OneLine("console.log('a1b2'.matchAll(/\d/g).length);"))
        Assert.Equal("true", OneLine("console.log(new RegExp('a+', 'i').test('AAA'));"))
        ' both exec and match return the matched text at index 0
        Assert.Equal("b b", OneLine("console.log(/b/.exec('ab')[0], 'ab'.match(/b/)[0]);"))
    End Sub

    ' ------------------------------------------------------------- errors

    <Fact>
    Public Sub Errors_ConstructionAndProperties()
        Assert.Equal("boom Error", OneLine("var e = new Error('boom'); console.log(e.message, e.name);"))
        Assert.Equal("TypeError: t", OneLine("console.log(String(new TypeError('t')));"))
        Assert.Equal("true true", OneLine(
            "console.log(new TypeError('x') instanceof TypeError, new TypeError('x') instanceof Error);"))
        Assert.Equal("true", OneLine("console.log(new Error().message === '');"))
        Assert.Equal("function function", OneLine("console.log(typeof Error, typeof Array);"))
        ' an engine error carries an Error object, so .name / .message work
        Assert.Equal("TypeError true", OneLine(
            "try { null.x; } catch (e) { console.log(e.name, e instanceof TypeError); }"))
        Assert.Equal("ReferenceError", OneLine(
            "try { notDefinedAnywhere; } catch (e) { console.log(e.name); }"))
        Assert.Equal("custom", OneLine(
            "try { throw new Error('custom'); } catch (e) { console.log(e.message); }"))
    End Sub

    ' --------------------------------------------------------------- date

    <Fact>
    Public Sub Date_Basics()
        Assert.Equal("number", OneLine("console.log(typeof Date.now());"))
        Assert.Equal("0", OneLine("console.log(new Date(0).getTime());"))
        Assert.Equal("1970", OneLine("console.log(new Date(0).getFullYear());"))
        Assert.Equal("0", OneLine("console.log(new Date(0).getMonth());"))
        Assert.Equal("true", OneLine("console.log(Date.now() > new Date(0).getTime());"))
        Assert.Equal("number", OneLine("console.log(typeof new Date().getTimezoneOffset());"))
        Assert.Equal("true", OneLine("console.log(/^\d{4}-/.test(new Date(0).toISOString()));"))
        Assert.Equal("number", OneLine("console.log(typeof Date.parse('2026-10-14'));"))
    End Sub

    ' -------------------------------------------------- array / string lib

    <Fact>
    Public Sub ArrayLibrary()
        Assert.Equal("3", OneLine("console.log([1,2,3,4].find(x => x > 2));"))
        Assert.Equal("2", OneLine("console.log([1,2,3].findIndex(x => x === 3));"))
        Assert.Equal("3", OneLine("console.log([1,2,3].findLast(x => x < 4));"))
        Assert.Equal("true false", OneLine("console.log([1,2,3].some(x => x > 2), [1,2,3].every(x => x > 1));"))
        Assert.Equal("3", OneLine("console.log([[1,2],[3]].flat().length);"))
        Assert.Equal("2", OneLine("console.log([[1,[2]]].flat(2).length);"))
        Assert.Equal("6", OneLine("console.log([1,2,3].flatMap(x => [x, x]).length);"))
        Assert.Equal("3 2", OneLine("console.log([1,2,3].at(-1), [1,2,3].at(-1) - 1);"))
        Assert.Equal("2", OneLine("console.log([1,2,3].lastIndexOf(3));"))
        Assert.Equal("0", OneLine("console.log([1,2,3].reduceRight((a, b) => a - b));"))
        Assert.Equal("1,0,0", OneLine("console.log([1,2,3].fill(0, 1).join(','));"))
        Assert.Equal("3", OneLine("console.log([1,2,3].keys().length);"))
        ' splice removes in place and returns the removed elements
        Assert.Equal("1,4 2 2", OneLine("var a = [1,2,3,4]; var r = a.splice(1, 2); console.log(a.join(','), a.length, r.length);"))
        Assert.Equal("1,9,4", OneLine("var a = [1,2,3,4]; a.splice(1, 2, 9); console.log(a.join(','));"))
    End Sub

    <Fact>
    Public Sub StringLibrary()
        Assert.Equal("c", OneLine("console.log('abc'.at(-1));"))
        Assert.Equal("ab...", OneLine("console.log('ab'.padEnd(5, '.'));"))
        Assert.Equal("3", OneLine("console.log('  x  '.trimStart().length);"))
        Assert.Equal("bbb", OneLine("console.log('aaa'.replaceAll('a', 'b'));"))
        Assert.Equal("a-b-c", OneLine("console.log('a-b'.concat('-c'));"))
        Assert.Equal("97", OneLine("console.log('abc'.codePointAt(0));"))
        Assert.Equal("5", OneLine("console.log('abcabc'.lastIndexOf('c'));"))
        Assert.Equal("xxx", OneLine("console.log('x'.repeat(3));"))
        Assert.Equal("65", OneLine("console.log(String.fromCharCode(65) === 'A' ? 65 : 0);"))
    End Sub

    ' ------------------------------------------- object / math / number / json

    <Fact>
    Public Sub ObjectMathNumberJsonLibrary()
        Assert.Equal("2", OneLine("console.log(Object.assign({}, {a: 1}, {b: 2}).b);"))
        Assert.Equal("1", OneLine("console.log(Object.entries({a: 1}).length);"))
        Assert.Equal("a", OneLine("console.log(Object.entries({a: 1})[0][0]);"))
        Assert.Equal("v", OneLine("console.log(Object.fromEntries([['k', 'v']]).k);"))
        Assert.Equal("2", OneLine("console.log(Object.getOwnPropertyNames({a: 1, b: 2}).length);"))
        Assert.Equal("2", OneLine("console.log(Object.values({a: 1, b: 2}).length);"))
        Assert.Equal("2", OneLine("console.log(Object.keys({a: 1, b: 2}).length);"))

        Assert.Equal("12", OneLine("console.log(Math.imul(3, 4));"))
        Assert.Equal("31", OneLine("console.log(Math.clz32(1));"))
        Assert.Equal("-1", OneLine("console.log(Math.trunc(-1.7));"))
        Assert.Equal("3", OneLine("console.log(Math.cbrt(27));"))
        Assert.Equal("3", OneLine("console.log(Math.log2(8));"))
        Assert.Equal("0", OneLine("console.log(Math.atan2(0, 1));"))
        Assert.Equal("1.5", OneLine("console.log(Math.fround(1.5));"))
        Assert.Equal("3", OneLine("console.log(Math.max(1, 2, 3));"))
        Assert.Equal("1", OneLine("console.log(Math.min(1, 2, 3));"))

        Assert.Equal("true false", OneLine("console.log(Number.isInteger(5), Number.isInteger(5.5));"))
        Assert.Equal("true false", OneLine("console.log(Number.isNaN(NaN), Number.isNaN('x'));"))
        Assert.Equal("1.5", OneLine("console.log(Number.parseFloat('1.5x'));"))
        Assert.Equal("ff", OneLine("console.log((255).toString(16));"))
        Assert.Equal("9007199254740991", OneLine("console.log(Number.MAX_SAFE_INTEGER);"))

        ' the optional third argument switches to indented output
        Assert.Equal("true", OneLine("console.log(JSON.stringify({a: 1}, null, 2).indexOf('\n') > 0);"))
        Assert.Equal("false", OneLine("console.log(JSON.stringify({a: 1}).indexOf('\n') > 0);"))
    End Sub

    ' ============================================== code generator

    Private Shared Function Generate(source As String) As String
        Return New CodeGenerator().Generate(Parser.Parse(source))
    End Function

    <Fact>
    Public Sub CodeGen_RegexAndBuiltins()
        Assert.Contains("New LiteJsRuntime.JsRegExp(", Generate("var re = /a+/g;"))
        Assert.Contains("JsRuntime.JsInvokeMethod(", Generate("'x'.replace(/a/, 'b');"))
        Assert.Contains("JsRuntime.JsNew(", Generate("var e = new Error('x');"))
        Assert.Contains("JsRuntime.JsNew(", Generate("var d = new Date();"))
    End Sub

End Class

