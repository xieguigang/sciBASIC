Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript
Imports Xunit

''' <summary>
''' TEMPORARY helper: regenerates test/artifacts/GeneratedScript.vb from the demo
''' script so the checked-in translation stays byte-identical with the current
''' CodeGenerator output (the artifacts file is compiled into the test assembly
''' and therefore acts as a build-time compile check of generated code).
''' </summary>
Public Class _ArtifactGenerator

    ''' <summary>Representative script exercising every implemented language feature.</summary>
    Public Const DemoSource As String =
        "// ---- variables, hoisting, closures ----" & vbLf &
        "var total = 0;" & vbLf &
        "if (true) { var leaked = 7; }" & vbLf &
        "function makeCounter() { var n = 0; return function () { n = n + 1; return n; }; }" & vbLf &
        "var counter = makeCounter();" & vbLf &
        "console.log('counter:', counter(), counter(), 'leaked:', leaked);" & vbLf &
        "" & vbLf &
        "// ---- arithmetic, bitwise, comparison, ternary, nullish ----" & vbLf &
        "var bits = (5 & 3) | (1 << 4);" & vbLf &
        "var fallback = null ?? 'default';" & vbLf &
        "console.log('bits:', bits, 'fallback:', fallback, 7 % 3 === 1);" & vbLf &
        "" & vbLf &
        "// ---- template literals ----" & vbLf &
        "var name = 'world';" & vbLf &
        "console.log(`hello ${name}, total=${total + 1}`);" & vbLf &
        "" & vbLf &
        "// ---- arrays, libraries ----" & vbLf &
        "var xs = [3, 1, 2];" & vbLf &
        "xs.sort();" & vbLf &
        "console.log('joined:', xs.join(','), 'sum:', xs.reduce((a, b) => a + b, 0));" & vbLf &
        "console.log('found:', xs.find(x => x > 1), 'flat:', [[1, 2], [3]].flat().length);" & vbLf &
        "" & vbLf &
        "// ---- objects, destructuring, spread, accessors ----" & vbLf &
        "var base = { a: 1, b: 2 };" & vbLf &
        "var merged = { ...base, c: 3, [name]: 4 };" & vbLf &
        "var { a, ...rest } = merged;" & vbLf &
        "var [first, ...others] = xs;" & vbLf &
        "var withAccessor = {" & vbLf &
        "    stored: 0," & vbLf &
        "    get value() { return this.stored; }," & vbLf &
        "    set value(v) { this.stored = v; }" & vbLf &
        "};" & vbLf &
        "withAccessor.value = a + first;" & vbLf &
        "console.log('a:', a, 'rest keys:', Object.keys(rest).length, 'others:', others.length);" & vbLf &
        "console.log('accessor:', withAccessor.value, 'shortcut:', merged[name]);" & vbLf &
        "" & vbLf &
        "// ---- default & rest parameters ----" & vbLf &
        "function describe(label, count = 1, ...extra) { return label + count + extra.length; }" & vbLf &
        "console.log('describe:', describe('n', undefined, 'x', 'y'));" & vbLf &
        "console.log('spread call:', describe(...['z', 2]));" & vbLf &
        "" & vbLf &
        "// ---- control flow: switch, for-of, labels, try/catch ----" & vbLf &
        "var grade = '';" & vbLf &
        "switch (xs.length) {" & vbLf &
        "    case 2: grade += 'two';" & vbLf &
        "    case 3: grade += 'three'; break;" & vbLf &
        "    default: grade += 'other';" & vbLf &
        "}" & vbLf &
        "var walked = '';" & vbLf &
        "outer: for (var i = 0; i < 3; i++) {" & vbLf &
        "    for (const ch of 'ab') {" & vbLf &
        "        if (ch === 'b') continue outer;" & vbLf &
        "        walked += i + ch;" & vbLf &
        "    }" & vbLf &
        "}" & vbLf &
        "try { JSON.parse('{'); } catch (e) { console.log('json error:', e.name); }" & vbLf &
        "console.log('grade:', grade, 'walked:', walked);" & vbLf &
        "" & vbLf &
        "// ---- regex ----" & vbLf &
        "var date = '2026-10-14';" & vbLf &
        "console.log('iso:', date.replace(/(\d+)-(\d+)-(\d+)/, '$3/$2/$1'), /\\d+/g.test(date));" & vbLf &
        "" & vbLf &
        "// ---- classes ----" & vbLf &
        "class Shape {" & vbLf &
        "    static made = 0;" & vbLf &
        "    constructor(area) { this.area = area; Shape.made++; }" & vbLf &
        "    describe() { return 'shape(' + this.area + ')'; }" & vbLf &
        "}" & vbLf &
        "class Square extends Shape {" & vbLf &
        "    constructor(side) { super(side * side); this.side = side; }" & vbLf &
        "    describe() { return 'square(' + this.side + ') -> ' + super.describe(); }" & vbLf &
        "}" & vbLf &
        "var sq = new Square(3);" & vbLf &
        "console.log(sq.describe(), sq instanceof Square, sq instanceof Shape, Shape.made);" & vbLf &
        "" & vbLf &
        "// ---- optional chaining, logical assignment targets ----" & vbLf &
        "var maybe = { nested: { value: 5 } };" & vbLf &
        "console.log('chain:', maybe?.nested?.value, (null)?.missing);" & vbLf &
        "" & vbLf &
        "// ---- generators ----" & vbLf &
        "function* naturals(n) { for (var k = 1; k <= n; k++) yield k; }" & vbLf &
        "console.log('generated:', [...naturals(4)].join('-'));" & vbLf &
        "" & vbLf &
        "// ---- promises & async/await ----" & vbLf &
        "async function fetchValue() { return 42; }" & vbLf &
        "async function main() {" & vbLf &
        "    var value = await fetchValue();" & vbLf &
        "    console.log('awaited:', value);" & vbLf &
        "}" & vbLf &
        "main().then(() => console.log('done'));" & vbLf &
        "Promise.all([Promise.resolve(1), Promise.resolve(2)])" & vbLf &
        ".then(values => console.log('all:', values.join('+')));" & vbLf

    <Fact>
    Public Sub RegenerateArtifact()
        Dim code = New CodeGenerator().Generate(Parser.Parse(DemoSource), "GeneratedScript")
        Dim path = Path.Combine(ArtifactDirectory(), "GeneratedScript.vb")
        File.WriteAllText(path, code)
        Assert.Contains("JsRuntime", code)
    End Sub

    Private Shared Function ArtifactDirectory() As String
        ' walk up from the test binaries to the project source folder
        Dim dir = New DirectoryInfo(AppContext.BaseDirectory)
        While dir IsNot Nothing AndAlso Not Directory.Exists(Path.Combine(dir.FullName, "artifacts"))
            dir = dir.Parent
        End While
        Assert.True(dir IsNot Nothing, "artifacts folder not found")
        Return Path.Combine(dir.FullName, "artifacts")
    End Function

End Class
