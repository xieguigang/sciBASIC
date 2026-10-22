// LiteJs feature demonstration — the source of artifacts/GeneratedScript.vb.
// The translation of this file is compiled into the test assembly, so it doubles
// as a build-time compile check of the JS-to-VB.NET code generator.

// ---- import: CLR assembly types through reflection ----
// 'from' names an assembly; the namespace becomes an object of CLR types, so
// msil.MethodDecompiler is the VB Module and its members are called statically.
import Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL as msil from 'Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.dll';

// ---- variables, hoisting, closures ----
var total = 0;
if (true) { var leaked = 7; }
function makeCounter() { var n = 0; return function () { n = n + 1; return n; }; }
var counter = makeCounter();
console.log('counter:', counter(), counter(), 'leaked:', leaked);

// ---- arithmetic, bitwise, comparison, ternary, nullish ----
var bits = (5 & 3) | (1 << 4);
var fallback = null ?? 'default';
console.log('bits:', bits, 'fallback:', fallback, 7 % 3 === 1);

// ---- template literals ----
var name = 'world';
console.log(`hello ${name}, total=${total + 1}`);

// ---- arrays and the array library ----
var xs = [3, 1, 2];
xs.sort();
console.log('joined:', xs.join(','), 'sum:', xs.reduce((a, b) => a + b, 0));
console.log('found:', xs.find(x => x > 1), 'flat:', [[1, 2], [3]].flat().length);

// ---- objects, destructuring, spread, accessors ----
var base = { a: 1, b: 2 };
var merged = { ...base, c: 3, [name]: 4 };
var { a, ...rest } = merged;
var [first, ...others] = xs;
var withAccessor = {
    stored: 0,
    get value() { return this.stored; },
    set value(v) { this.stored = v; }
};
withAccessor.value = a + first;
console.log('a:', a, 'rest keys:', Object.keys(rest).length, 'others:', others.length);
console.log('accessor:', withAccessor.value, 'computed key:', merged[name]);

// ---- default and rest parameters ----
function describe(label, count = 1, ...extra) { return label + count + extra.length; }
console.log('describe:', describe('n', undefined, 'x', 'y'));
console.log('spread call:', describe(...['z', 2]));

// ---- control flow: switch, for-of, labels, try/catch ----
var grade = '';
switch (xs.length) {
    case 2: grade += 'two';
    case 3: grade += 'three'; break;
    default: grade += 'other';
}
var walked = '';
outer: for (var i = 0; i < 3; i++) {
    for (const ch of 'ab') {
        if (ch === 'b') continue outer;
        walked += i + ch;
    }
}
try { JSON.parse('{'); } catch (e) { console.log('json error:', e.name); }
console.log('grade:', grade, 'walked:', walked);

// ---- regular expressions ----
var date = '2026-10-14';
console.log('iso:', date.replace(/(\d+)-(\d+)-(\d+)/, '$3/$2/$1'), /\d+/g.test(date));

// ---- classes ----
class Shape {
    static made = 0;
    constructor(area) { this.area = area; Shape.made++; }
    describe() { return 'shape(' + this.area + ')'; }
}
class Square extends Shape {
    constructor(side) { super(side * side); this.side = side; }
    describe() { return 'square(' + this.side + ') -> ' + super.describe(); }
}
var sq = new Square(3);
console.log(sq.describe(), sq instanceof Square, sq instanceof Shape, Shape.made);

// ---- optional chaining ----
var maybe = { nested: { value: 5 } };
console.log('chain:', maybe?.nested?.value, (null)?.missing);

// ---- generators ----
function* naturals(n) { for (var k = 1; k <= n; k++) yield k; }
console.log('generated:', [...naturals(4)].join('-'));

// ---- promises and async/await ----
async function fetchValue() { return 42; }
async function main() {
    var value = await fetchValue();
    console.log('awaited:', value);
}
main().then(() => console.log('done'));
Promise.all([Promise.resolve(1), Promise.resolve(2)])
    .then(values => console.log('all:', values.join('+')));

// ---- CLR reflection bridge ----
// 'bridge' is a host-injected .NET object: the engine resolves its properties,
// fields and methods through reflection, so it reads like a plain JS object.
// (Interpreted runs get it from Interpreter.DefineGlobal; translated runs get it
// from the optional ClrHost argument of Run.)
console.log('clr:', bridge.Title, bridge.Add(1, 2), bridge.Items.length);
bridge.Title = 'updated';
var clrSum = 0;
for (const item of bridge.Items) { clrSum += item; }
console.log('clr sum:', clrSum, Object.keys(bridge).indexOf('Title') >= 0, bridge[1]);
try { bridge.Explode(); } catch (e) { console.log('clr error:', e.name); }

// ---- imported CLR types ----
console.log('import:', typeof msil.MethodDecompiler, Object.keys(msil).indexOf('MethodDecompiler') >= 0);
