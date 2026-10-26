import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
    }

    return s;
}

function rowSumSq(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        const v = x[i * cols + k];

        s = s + v * v;
    }

    return s;
}

function gramDot(i, j, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k] * x[j * cols + k];
    }

    return s;
}

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

const rows = 64;
const cols = 8;
const data = [];

for (let i = 0; i < rows * cols; i++) {
    data.push((i % 17) / 17 - 0.5);
}

const kRowSum = jscuda.JsCudaHost.translate(rowSum);
const kRowSumSq = jscuda.JsCudaHost.translate(rowSumSq);
const kGramDot = jscuda.JsCudaHost.translate(gramDot);
const kCorrelation = jscuda.JsCudaHost.translate(correlationCell);

jscuda.JsCudaHost.register(kRowSum);

const engine = jscuda.JsCudaHost.createEngine();
const cells = rows * rows;
const bufX = jscuda.JsCudaHost.buffer(data);
const bufSum = jscuda.JsCudaHost.bufferSize(rows);
const bufSumSq = jscuda.JsCudaHost.bufferSize(rows);
const bufDot = jscuda.JsCudaHost.bufferSize(cells);
const bufCorr = jscuda.JsCudaHost.bufferSize(cells);

jscuda.JsCudaHost.launch1D(kRowSum, engine, rows, 64, bufSum, bufX, cols);
jscuda.JsCudaHost.synchronize(engine);

console.log('upload[5] = ' + jscuda.JsCudaHost.read(bufX)[5] + ' (expected ' + data[5] + ')');

const sums = jscuda.JsCudaHost.read(bufSum);
const sumsSq = jscuda.JsCudaHost.read(bufSumSq);
const dot = jscuda.JsCudaHost.read(bufDot);
const corr = jscuda.JsCudaHost.read(bufCorr);

console.log('sum     gpu/host = ' + sums[0] + ' / ' + rowSum(0, data, cols));

bufX.Dispose();
bufSum.Dispose();
bufSumSq.Dispose();
bufDot.Dispose();
bufCorr.Dispose();
engine.Dispose();

console.log('done: t8');
