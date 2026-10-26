import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
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

const rows = 4;
const cols = 3;
const data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

const kRowSum = jscuda.JsCudaHost.translate(rowSum);
const kCorrelation = jscuda.JsCudaHost.translate(correlationCell);

jscuda.JsCudaHost.register(kRowSum);
jscuda.JsCudaHost.register(kCorrelation);

const engine = jscuda.JsCudaHost.createEngine();
const bufX = jscuda.JsCudaHost.buffer(data);
const bufSum = jscuda.JsCudaHost.bufferSize(rows);
const bufDot = jscuda.JsCudaHost.buffer([
    14, 28, 42, 56,
    28, 56, 84, 112,
    42, 84, 126, 168,
    56, 112, 168, 224
]);
const bufCorr = jscuda.JsCudaHost.bufferSize(rows * rows);

jscuda.JsCudaHost.launch1D(kRowSum, engine, rows, 4, bufSum, bufX, cols);
jscuda.JsCudaHost.launch2D(kCorrelation, engine, rows, rows, 2, bufCorr, bufDot, bufSum, bufSum, rows, cols);
jscuda.JsCudaHost.synchronize(engine);

const gpuSums = jscuda.JsCudaHost.read(bufSum);
const sums = [];

for (let i = 0; i < rows; i++) {
    sums.push(gpuSums[i]);
}

console.log('rowSum gpu  = ' + sums.join(', '));
console.log('rowSum host = ' + [rowSum(0, data, cols), rowSum(1, data, cols), rowSum(2, data, cols), rowSum(3, data, cols)].join(', '));
console.log('corr gpu    = ' + jscuda.JsCudaHost.preview(jscuda.JsCudaHost.read(bufCorr), rows, 4));
console.log('corr host00 = ' + correlationCell(0, 0, [14, 28, 42, 56, 28, 56, 84, 112, 42, 84, 126, 168, 56, 112, 168, 224], sums, sums, rows, cols));

bufX.Dispose();
bufSum.Dispose();
bufDot.Dispose();
bufCorr.Dispose();
engine.Dispose();

console.log('done: t7');
