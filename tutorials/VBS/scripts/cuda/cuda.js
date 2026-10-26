import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ============================================================================
//  IL -> CUDA tutorial — the njs twin of cuda.vb
//
//  The decompiled functions live in the standard library assembly
//  (stdlib.PearsonMetrics) because a JS function is not a CLR method and cannot
//  be handed to the IL decompiler; every other step of the pipeline is shared
//  with the VBS script through the same stdlib.cuda api.
// ============================================================================

const here = (name) => path.join(__dirname, name);

const rows = 512;
const cols = 512;
const seed = 42;
const preview = 6;

console.log('=== 1) decompile the VB.NET functions into an AST and emit .cu source ===');

for (const kernel of stdlib.cuda.kernelNames()) {
    console.log(`  ${kernel} index mode = ${stdlib.cuda.kernelIndexMode(kernel)}`);
}

console.log('');
console.log('  ---- CorrelationCell pseudo code ----');
console.log(stdlib.cuda.kernelPseudoCode('CorrelationCell'));
console.log('');
console.log('  ---- DistanceCell cuda source ----');
console.log(stdlib.cuda.kernelSource('DistanceCell'));

// 2) interpreter self check
console.log('');
console.log('=== 2) interpreter self check ===');

const probe = stdlib.cuda.makeMatrix(64, 64, seed);

for (const check of stdlib.cuda.selfCheck(probe, 64, 64)) {
    console.log('  ' + check);
}

// 3) GPU computation and 4) CPU reference comparison
console.log('');
console.log(`=== 3) GPU computation (${rows} x ${cols} input matrix) ===`);

const data = stdlib.cuda.makeMatrix(rows, cols, seed);
const cpuCorrelation = stdlib.cuda.pearsonCpu(data, rows, cols);
const cpuDistance = stdlib.cuda.distanceCpu(data, rows, cols);

if (stdlib.cuda.available()) {
    const result = stdlib.cuda.metrics(data, rows, cols);

    console.log(`  device           : ${result.device}`);
    console.log(`  elapsed          : ${result.elapsedMs} ms`);
    console.log(`  correlation error: ${stdlib.cuda.maxError(result.correlation, cpuCorrelation).toFixed(9)} (tolerance 1.0E-3)`);
    console.log(`  distance error   : ${stdlib.cuda.maxError(result.distance, cpuDistance).toFixed(9)} (tolerance 1.0E-2)`);
    console.log('');
    console.log(`  Pearson correlation matrix (GPU, top-left ${preview} x ${preview}):`);
    console.log(result.previewCorrelation(preview));
    console.log('');
    console.log(`  Euclidean distance matrix (GPU, top-left ${preview} x ${preview}):`);
    console.log(result.previewDistance(preview));
} else {
    console.log('  GPU unavailable, the CPU reference results are shown below.');

    for (const diagnostic of stdlib.cuda.diagnostics()) {
        console.log('  ' + diagnostic);
    }

    console.log('');
    console.log(`  Pearson correlation matrix (CPU, top-left ${preview} x ${preview}):`);
    console.log(stdlib.cuda.preview(cpuCorrelation, rows, preview));
    console.log('');
    console.log(`  Euclidean distance matrix (CPU, top-left ${preview} x ${preview}):`);
    console.log(stdlib.cuda.preview(cpuDistance, rows, preview));
}

// 5) export both matrices through the standard table writer
const correlationTable = stdlib.table.fromMatrix(
    stdlib.cuda.toMatrix(stdlib.cuda.pearsonCpu(data, 16, cols), 16, 16),
    stdlib.table.names('row', 16),
    stdlib.table.names('col', 16));

console.log('');
console.log('csv : ' + stdio.write.csv(correlationTable, here('cuda-correlation-16x16.csv')));
console.log('done: cuda');
