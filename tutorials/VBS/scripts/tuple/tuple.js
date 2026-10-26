import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ============================================================================
//  Tuple / array destructuring tutorial — the njs twin of tuple.vb
//
//  The language syntax differs (JS array destructuring instead of VB tuple
//  deconstruction) but the data flow is the same: the tuples are turned into a
//  standard library table and exported.
// ============================================================================

const here = (name) => path.join(__dirname, name);
const join = (values, sep) => [...values].join(sep === undefined ? ',' : sep);

// Deconstruct the pair [1, 2] into two variables in a single line.
const [a, b] = [1, 2];

console.log('demo test result of variable tuple deconstruct in vb:');
console.log(`a := ${a}`);
console.log(`b := ${b}`);
console.log(`(a+b) := ${a + b}`);

// An array literal whose elements are (key, value) pairs.
const tuples = [['a', 123], ['b', 456], ['c', 789]];

// Each item is deconstructed while iterating.
for (const [str, int] of tuples) {
    console.log(`${str} => ${int.toFixed(4)}`);
}

// ---------------------------------------------------------------------------
// From language values to a standard library table
// ---------------------------------------------------------------------------
const rowNames = [];
const values = [];

for (const [name, value] of tuples) {
    rowNames.push(name);
    values.push(value);
}

const dataset = stdlib.table.fromColumns(['value'], [values], rowNames);

console.log(`table: ${dataset.nsamples} samples x ${dataset.nfeatures} feature, ` +
            `names = ${join(stdlib.table.rowNames(dataset), ', ')}`);
console.log(`stats: sum = ${stdlib.stats.sum(values)}, mean = ${stdlib.stats.mean(values)}, ` +
            `median = ${stdlib.stats.median(values)}`);

console.log('csv : ' + stdio.write.csv(dataset, here('tuple.csv')));

console.log('done: tuple');
