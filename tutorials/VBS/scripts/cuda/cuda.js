import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';
import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

// ============================================================================
//  JavaScript -> CUDA tutorial  (the njs twin of cuda.vb)
//
//  The maths lives here, in the script: plain JavaScript functions with no CUDA
//  concept at all. JsCuda decompiles them into a typed AST, emits the .cu
//  source, and the same function is still callable on the host (which is what
//  the self check and the CPU reference use):
//
//      JS function -> AST -> .cu source
//                 -> NVRTC just-in-time compile -> cuLaunchKernel -> matrix
//
//  Kernel conventions (identical to the IL pipeline):
//    * parameters named i / j are the thread indices
//    * the other parameters are buffers (indexed with [..]) or scalars
//    * the returned value is written to the output matrix
//    * a function with a single `ctx` parameter uses the context sugar:
//      ctx.i / ctx.j are the indices, ctx.dot[k] is a buffer cell
// ============================================================================

const rows = 512;
const cols = 512;
const seed = 42;
const previewSize = 6;

// ---------------------------------------------------------------------------
// The CUDA kernels: plain JavaScript mathematics.
// ---------------------------------------------------------------------------

/** Sum of one row of the input matrix. */
function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
    }

    return s;
}

/** Sum of squares of one row of the input matrix. */
function rowSumSq(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        const v = x[i * cols + k];

        s = s + v * v;
    }

    return s;
}

/** Gram matrix cell: dot product of the rows i and j. */
function gramDot(i, j, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k] * x[j * cols + k];
    }

    return s;
}

/** Pearson correlation between the rows i and j, from the precomputed sums. */
function correlationCell(i, j, dot, rowSum, rowSumSq, rows, cols) {
    const n = cols;
    const covariance = dot[i * rows + j] - n * (rowSum[i] / n) * (rowSum[j] / n);
    const varianceI = rowSumSq[i] - n * (rowSum[i] / n) * (rowSum[i] / n);
    const varianceJ = rowSumSq[j] - n * (rowSum[j] / n) * (rowSum[j] / n);
    const denominator = Math.sqrt(varianceI) * Math.sqrt(varianceJ);

    if (denominator <= 0) {
        return 0;
    }

    return covariance / denominator;
}

/**
 * Euclidean distance between the rows i and j — written with the `ctx` sugar.
 * The parameter list is derived from the context members the body uses:
 * ctx.i / ctx.j are the thread indices, ctx.dot / ctx.rowSumSq are buffers.
 */
function distanceCell(ctx) {
    const d = ctx.rowSumSq[ctx.i] + ctx.rowSumSq[ctx.j] - 2 * ctx.dot[ctx.i * ctx.rows + ctx.j];

    if (ctx.i === ctx.j) {
        return 0;
    }
    if (d < 0) {
        return 0;
    }

    return Math.sqrt(d);
}

/** A pure scalar kernel: clamps a value from below (element wise). */
const clamp = (value, minimum) => (value < minimum ? minimum : value);

// ---------------------------------------------------------------------------
// The double precision host reference implementations.
// ---------------------------------------------------------------------------

function referencePearson(m, rows, cols) {
    const mean = [];
    const sd = [];

    for (let i = 0; i < rows; i++) {
        let sum = 0;
        let sumSq = 0;

        for (let k = 0; k < cols; k++) {
            const v = m[i * cols + k];

            sum += v;
            sumSq += v * v;
        }

        mean.push(sum / cols);
        sd.push(Math.sqrt(Math.max(sumSq - cols * mean[i] * mean[i], 0)));
    }

    const out = [];

    for (let i = 0; i < rows; i++) {
        for (let j = 0; j < rows; j++) {
            let dot = 0;

            for (let k = 0; k < cols; k++) {
                dot += m[i * cols + k] * m[j * cols + k];
            }

            const covariance = dot - cols * mean[i] * mean[j];
            const denominator = sd[i] * sd[j];
            const c = denominator > 0 ? covariance / denominator : 0;

            out.push(i === j ? 1 : Math.min(1, Math.max(-1, c)));
        }
    }

    return out;
}

function referenceDistance(m, rows, cols) {
    const sumSq = [];

    for (let i = 0; i < rows; i++) {
        let sum = 0;

        for (let k = 0; k < cols; k++) {
            const v = m[i * cols + k];

            sum += v * v;
        }

        sumSq.push(sum);
    }

    const out = [];

    for (let i = 0; i < rows; i++) {
        for (let j = 0; j < rows; j++) {
            let dot = 0;

            for (let k = 0; k < cols; k++) {
                dot += m[i * cols + k] * m[j * cols + k];
            }

            out.push(i === j ? 0 : Math.sqrt(Math.max(sumSq[i] + sumSq[j] - 2 * dot, 0)));
        }
    }

    return out;
}

/** Deterministic pseudo random matrix in [-1, 1] (same formula as stdlib.cuda.makeMatrix). */
function makeMatrix(rows, cols, seed) {
    let state = seed;
    const data = [];

    for (let i = 0; i < rows * cols; i++) {
        // a small linear congruential generator: reproducible on every engine
        state = (state * 1103515245 + 12345) % 2147483648;
        data.push((state / 2147483648) * 2 - 1);
    }

    return data;
}

// ===========================================================================
// 1) translation
// ===========================================================================
console.log('=== 1) decompile the JavaScript functions into CUDA kernels ===');

const kRowSum = jscuda.JsCudaHost.translate(rowSum);
const kRowSumSq = jscuda.JsCudaHost.translate(rowSumSq);
const kGramDot = jscuda.JsCudaHost.translate(gramDot);
const kCorrelation = jscuda.JsCudaHost.translate(correlationCell);
const kDistance = jscuda.JsCudaHost.translate(distanceCell);
const kClamp = jscuda.JsCudaHost.translate(clamp);

const translated = [
    ['rowSum', kRowSum], ['rowSumSq', kRowSumSq], ['gramDot', kGramDot],
    ['correlationCell', kCorrelation], ['distanceCell', kDistance], ['clamp', kClamp]
];

for (const [label, kernel] of translated) {
    console.log(`  ${label.padEnd(22)} index mode = ${jscuda.JsCudaHost.indexMode(kernel)}`);
}

console.log('');
console.log('  ---- correlationCell pseudo code ----');
console.log(jscuda.JsCudaHost.pseudoCode(kCorrelation));
console.log('');
console.log('  ---- distanceCell cuda source (ctx style) ----');
console.log(jscuda.JsCudaHost.cudaSource(kDistance));

// ===========================================================================
// 2) self check: the very same JavaScript function runs on the host
// ===========================================================================
console.log('');
console.log('=== 2) interpreter self check ===');

const probe = makeMatrix(64, 64, seed);
const probeRowSum = [];
const probeRowSumSq = [];
const probeGram = [];

for (let i = 0; i < 64; i++) {
    probeRowSum.push(rowSum(i, probe, 64));
    probeRowSumSq.push(rowSumSq(i, probe, 64));
}

for (let i = 0; i < 64; i++) {
    for (let j = 0; j < 64; j++) {
        probeGram.push(gramDot(i, j, probe, 64));
    }
}

// the GPU result is double precision, so the host comparison keeps the same numbers
console.log(`  rowSum: host value = ${rowSum(0, probe, 64)}`);
console.log(`  rowSumSq: host value = ${rowSumSq(0, probe, 64)}`);
console.log(`  gramDot: host value = ${gramDot(0, 1, probe, 64)}`);
console.log(`  correlationCell: host value = ${correlationCell(0, 1, probeGram, probeRowSum, probeRowSumSq, 64, 64)}`);
console.log(`  distanceCell: host value = ${distanceCell({
    i: 0, j: 1, rows: 64, dot: probeGram, rowSumSq: probeRowSumSq
})}`);
console.log(`  clamp: host value = ${clamp(0.25, 0.5)}`);

// ===========================================================================
// 3) GPU computation and 4) CPU reference comparison
// ===========================================================================
console.log('');
console.log(`=== 3) GPU computation (${rows} x ${cols} input matrix) ===`);

const data = makeMatrix(rows, cols, seed);
const referenceCorrelation = referencePearson(data, rows, cols);
const referenceDistance = referenceDistance(data, rows, cols);

if (jscuda.JsCudaHost.available()) {
    // kernels must be registered before the engine compiles them
    for (const k of [kRowSum, kRowSumSq, kGramDot, kCorrelation, kDistance]) {
        jscuda.JsCudaHost.register(k);
    }

    const engine = jscuda.JsCudaHost.createEngine();
    const cells = rows * rows;
    const bufX = jscuda.JsCudaHost.buffer(data);
    const bufSum = jscuda.JsCudaHost.bufferSize(rows);
    const bufSumSq = jscuda.JsCudaHost.bufferSize(rows);
    const bufDot = jscuda.JsCudaHost.bufferSize(cells);
    const bufCorr = jscuda.JsCudaHost.bufferSize(cells);
    const bufDist = jscuda.JsCudaHost.bufferSize(cells);
    const started = new Date();

    jscuda.JsCudaHost.launch1D(kRowSum, engine, rows, 256, bufSum, bufX, cols);
    jscuda.JsCudaHost.launch1D(kRowSumSq, engine, rows, 256, bufSumSq, bufX, cols);
    jscuda.JsCudaHost.launch2D(kGramDot, engine, rows, rows, 16, bufDot, bufX, cols);
    jscuda.JsCudaHost.launch2D(kCorrelation, engine, rows, rows, 16, bufCorr, bufDot, bufSum, bufSumSq, rows, cols);
    jscuda.JsCudaHost.launch2D(kDistance, engine, rows, rows, 16, bufDist, bufSumSq, bufDot, rows);
    jscuda.JsCudaHost.synchronize(engine);

    const elapsed = new Date().getTime() - started.getTime();
    const gpuCorrelation = jscuda.JsCudaHost.read(bufCorr);
    const gpuDistance = jscuda.JsCudaHost.read(bufDist);

    console.log(`  device           : ${jscuda.JsCudaHost.deviceName()}`);
    console.log(`  elapsed          : ${elapsed.toFixed(4)} ms`);
    console.log(`  correlation error: ${jscuda.JsCudaHost.maxError(gpuCorrelation, referenceCorrelation).toFixed(9)} (tolerance 1.0E-3)`);
    console.log(`  distance error   : ${jscuda.JsCudaHost.maxError(gpuDistance, referenceDistance).toFixed(9)} (tolerance 1.0E-2)`);
    console.log('');
    console.log(`  Pearson correlation matrix (GPU, top-left ${previewSize} x ${previewSize}):`);
    console.log(jscuda.JsCudaHost.preview(gpuCorrelation, rows, previewSize));
    console.log('');
    console.log(`  Euclidean distance matrix (GPU, top-left ${previewSize} x ${previewSize}):`);
    console.log(jscuda.JsCudaHost.preview(gpuDistance, rows, previewSize));

    // 16x16 of the GPU result goes to a csv table
    const corner = [];

    for (let i = 0; i < 16; i++) {
        const line = [];

        for (let j = 0; j < 16; j++) {
            line.push(gpuCorrelation[i * rows + j]);
        }

        corner.push(line);
    }

    const correlationTable = stdlib.table.fromMatrix(corner, stdlib.table.names('row', 16), stdlib.table.names('col', 16));

    console.log('');
    console.log('csv : ' + stdio.write.csv(correlationTable, here('cuda-correlation-16x16.csv')));

    bufX.Dispose();
    bufSum.Dispose();
    bufSumSq.Dispose();
    bufDot.Dispose();
    bufCorr.Dispose();
    bufDist.Dispose();
    engine.Dispose();
} else {
    console.log('  GPU unavailable, the host reference results are shown below.');

    for (const diagnostic of jscuda.JsCudaHost.diagnostics()) {
        console.log('  ' + diagnostic);
    }

    console.log('');
    console.log(`  Pearson correlation matrix (host, top-left ${previewSize} x ${previewSize}):`);
    console.log(jscuda.JsCudaHost.preview(referenceCorrelation, rows, previewSize));
    console.log('');
    console.log(`  Euclidean distance matrix (host, top-left ${previewSize} x ${previewSize}):`);
    console.log(jscuda.JsCudaHost.preview(referenceDistance, rows, previewSize));
}

console.log('done: cuda');
