import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
    }

    return s;
}

const kernel = jscuda.JsCudaHost.translate(rowSum);

jscuda.JsCudaHost.register(kernel);

function probe(label, rows, cols, data, blockSize) {
    const engine = jscuda.JsCudaHost.createEngine();
    const bufX = jscuda.JsCudaHost.buffer(data);
    const bufSum = jscuda.JsCudaHost.bufferSize(rows);

    jscuda.JsCudaHost.launch1D(kernel, engine, rows, blockSize, bufSum, bufX, cols);
    jscuda.JsCudaHost.synchronize(engine);

    const sums = jscuda.JsCudaHost.read(bufSum);
    const expected = rowSum(0, data, cols);

    console.log(`${label}: gpu=${sums[0]} host=${expected} ${sums[0] === expected ? 'OK' : 'MISMATCH'}`);

    bufX.Dispose();
    bufSum.Dispose();
    engine.Dispose();
}

const ints = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
const fractions = [];

for (let i = 0; i < 512; i++) {
    fractions.push((i % 17) / 17 - 0.5);
}

const frac12 = [-0.5, -0.4, -0.3, -0.2, -0.1, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6];

probe('1 frac12   ', 4, 3, frac12, 4);
probe('2 ints     ', 4, 3, ints, 4);
probe('3 fractions', 4, 3, fractions, 4);
probe('4 ints     ', 4, 3, ints, 4);

console.log('done: t9');
