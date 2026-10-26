import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ============================================================================
//  Dynamic type tutorial — the njs twin of dynamic_type.vb
//
//  JavaScript variables are dynamic by nature, so the "let" part of the VB
//  script maps to plain reassignment here; the second half is identical: the
//  dynamic values are handed to the standard library.
// ============================================================================

const here = (name) => path.join(__dirname, name);

// a strongly typed CLR integer (the VB ``Dim a As Integer`` counterpart)
let a = 123;
console.log(`a := ${a}`);

// a dynamic variable: the runtime type follows the assigned value
let b = 456;
console.log(`b := ${b}`);

b = { a: 123, b: 456 };
console.log(`b.a := ${b.a}`);
console.log(`b.b := ${b.b}`);

b = false;
console.log(`check boolean := ${b}`);

// ---------------------------------------------------------------------------
// the dynamic values are ordinary numbers, so the standard library can use
// them directly
// ---------------------------------------------------------------------------
const values = [a, 456, 789];

console.log(`stats: count = ${values.length}, sum = ${stdlib.stats.sum(values)}, ` +
            `mean = ${stdlib.stats.mean(values).toFixed(4)}`);

const dataset = stdlib.table.fromColumns(['value'], [values], stdlib.table.names('row', 3));

console.log(`table: ${dataset.nsamples} samples x ${dataset.nfeatures} feature`);
console.log('csv : ' + stdio.write.csv(dataset, here('dynamic_type.csv')));

console.log('done: dynamic_type');
