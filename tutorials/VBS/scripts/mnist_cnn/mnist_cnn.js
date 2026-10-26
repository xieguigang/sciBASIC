import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ============================================================================
//  MNIST CNN demo — the njs twin of mnist_cnn.vb
//
//  Run:
//      njs mnist_cnn.js --mnist-data=<MNIST dir>
//
//  Without a real MNIST folder the deterministic synthetic digit set of
//  ml.syntheticDigits is used, so the tutorial always runs.
// ============================================================================

const here = (name) => path.join(__dirname, name);
const padEnd = (value, width) => String(value).padEnd(width);
const fixed = (value, digits) => value.toFixed(digits);
const percent = (value) => (value * 100).toFixed(2) + '%';

const argValue = (name) => {
    const prefix = name + '=';

    for (let i = 0; i < process.argv.length; i++) {
        const arg = String(process.argv[i]);

        if (arg.indexOf(prefix) === 0) return arg.substring(prefix.length);
    }

    return null;
};

const mnistDir = argValue('--mnist-data');
const passes = 2;
const samples = 120;
const seed = 12345;

// 0) dataset
let dataset;
let imagesFile = null;
let labelsFile = null;

if (mnistDir) {
    imagesFile = path.join(mnistDir, 'train-images-idx3-ubyte');
    labelsFile = path.join(mnistDir, 'train-labels-idx1-ubyte');
}

if (imagesFile && fs.existsSync(imagesFile) && fs.existsSync(labelsFile)) {
    console.log(`[data] mnist: ${imagesFile}`);
    dataset = stdio.read.mnist(imagesFile, labelsFile, samples);
} else {
    console.log('[data] no mnist dataset given (--mnist-data=<dir>), using the synthetic digit set');
    dataset = stdlib.ml.syntheticDigits(12, seed);
}

// 1) environment probe
console.log('');
console.log('=== 1) environment probe ===');
console.log(`    default backend = ${stdlib.ml.backendName()}`);
console.log(`    random seed     = ${seed} (weight init)`);
console.log(`    dataset         = ${dataset.nsamples} samples x ${dataset.nfeatures} pixels`);
console.log(`    training config = ${passes} passes x ${samples} images`);

// 2) CPU (SIMD) baseline
console.log('');
console.log('=== 2) CPU(SIMD) training + evaluation ===');

const cpuResult = stdlib.ml.cnnTrainTable(dataset, 'class', seed, passes, samples, false);

console.log(`    backend=${cpuResult.backend}  loss=${cpuResult.loss}  ` +
            `correct=${cpuResult.correct}/${cpuResult.total} (${percent(cpuResult.accuracy)})  ` +
            `elapsed=${fixed(cpuResult.seconds, 3)}s`);

// 3) register the CUDA compute engine
console.log('');
console.log('=== 3) register the CUDA compute engine ===');

const gpuEnabled = stdlib.ml.useCuda();

if (gpuEnabled) {
    console.log(`    [OK] current backend = ${stdlib.ml.backendName()}`);
    console.log(`    thresholds  : MinGpuElements=${stdlib.ml.cudaMinGpuElements()}, MinGemmElements=${stdlib.ml.cudaMinGemmElements()}`);

    const failures = stdlib.ml.cudaKernelFailures();

    if (failures.length === 0) {
        console.log('    IL2Cuda double-precision kernels: all translated and registered successfully');
    } else {
        for (const failure of failures) {
            console.log(`    [kernel translation failed] ${failure}`);
        }
    }
} else {
    console.log(`    [fallback] registration failed, using the CPU this run: ${stdlib.ml.cudaLastError()}`);
}

// 4) GPU pass, operator backend table and the CPU vs GPU comparison
if (gpuEnabled) {
    console.log('');
    console.log('=== 4) CUDA(GPU) training + evaluation ===');

    const gpuResult = stdlib.ml.cnnTrainTable(dataset, 'class', seed, passes, samples, false);

    console.log(`    backend=${gpuResult.backend}  loss=${gpuResult.loss}  ` +
                `correct=${gpuResult.correct}/${gpuResult.total} (${percent(gpuResult.accuracy)})  ` +
                `elapsed=${fixed(gpuResult.seconds, 3)}s`);

    console.log('');
    console.log('=== 5) actual execution backend of every tensor operator ===');

    for (const opLine of stdlib.ml.cnnOperatorTable()) {
        console.log(opLine);
    }

    console.log('');
    console.log('=== 6) CPU(SIMD) vs CUDA(GPU) ===');

    const lossError = Math.abs(cpuResult.loss - gpuResult.loss);

    console.log(`    ${padEnd('backend', 14)}${padEnd('loss', 24)}${padEnd('accuracy', 18)}elapsed(s)`);
    console.log(`    ${padEnd('SIMD(CPU)', 14)}${padEnd(String(cpuResult.loss), 24)}${padEnd(percent(cpuResult.accuracy), 18)}${fixed(cpuResult.seconds, 3)}`);
    console.log(`    ${padEnd('CUDA(GPU)', 14)}${padEnd(String(gpuResult.loss), 24)}${padEnd(percent(gpuResult.accuracy), 18)}${fixed(gpuResult.seconds, 3)}`);
    console.log('');
    console.log(`    loss max abs error = ${fixed(lossError, 9)}   ${lossError < 1.0e-6 ? 'OK' : 'FAIL'} (tolerance 1.0E-6)`);
    console.log(`    correct count      = CPU ${cpuResult.correct}/${cpuResult.total}  GPU ${gpuResult.correct}/${gpuResult.total}   ` +
                `${cpuResult.correct === gpuResult.correct ? 'OK' : 'difference see above'}`);
    console.log(`    elapsed ratio      = CPU ${fixed(cpuResult.seconds, 3)}s  GPU ${fixed(gpuResult.seconds, 3)}s  ` +
                `ratio=${fixed(cpuResult.seconds / gpuResult.seconds, 2)}x`);
} else {
    console.log('');
    console.log('=== 4-6) skipped ===');
    console.log(`    GPU not enabled, CPU result = loss ${cpuResult.loss}, ` +
                `${cpuResult.correct}/${cpuResult.total} (${percent(cpuResult.accuracy)})`);
}

// 7) a small markdown report about the run
const doc = stdlib.report.document('MNIST CNN backend comparison', 'sciBASIC standard library');

doc.add(stdlib.report.heading('1. Configuration'));
doc.add(stdlib.report.list([
    `dataset: ${dataset.nsamples} samples x ${dataset.nfeatures} pixels`,
    `${passes} training passes x ${samples} images, weight seed ${seed}`,
    `CUDA backend: ${gpuEnabled ? 'enabled' : 'not available'}`
], false));
doc.add(stdlib.report.heading('2. CPU baseline'));
doc.add(stdlib.report.paragraph(
    `loss = ${cpuResult.loss}, accuracy = ${percent(cpuResult.accuracy)}, elapsed = ${fixed(cpuResult.seconds, 3)}s`));

console.log('');
console.log('md  : ' + doc.save(here('mnist-cnn.md')));
console.log('done: mnist-cnn');
