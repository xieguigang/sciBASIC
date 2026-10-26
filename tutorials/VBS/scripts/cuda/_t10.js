import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function first(i, x) {
    return x[i];
}

function five(i, x) {
    return 5;
}

const kFirst = jscuda.JsCudaHost.translate(first);
const kFive = jscuda.JsCudaHost.translate(five);

jscuda.JsCudaHost.register(kFirst);
jscuda.JsCudaHost.register(kFive);

const engine = jscuda.JsCudaHost.createEngine();

function run(label, kernel, data) {
    const bufX = jscuda.JsCudaHost.buffer(data);
    const out = jscuda.JsCudaHost.bufferSize(4);

    jscuda.JsCudaHost.launch1D(kernel, engine, 4, 4, out, bufX);
    jscuda.JsCudaHost.synchronize(engine);

    const result = jscuda.JsCudaHost.read(out);

    console.log(`${label}: ${result[0]}, ${result[1]}, ${result[2]}, ${result[3]}`);

    bufX.Dispose();
    out.Dispose();
}

run('first(ints)   ', kFirst, [1, 2, 3, 4]);
run('first(fracs)  ', kFirst, [0.5, 0.25, 0.125, 0.0625]);
run('five(ints)    ', kFive, [1, 2, 3, 4]);
run('five(fracs)   ', kFive, [0.5, 0.25, 0.125, 0.0625]);

engine.Dispose();

console.log('done: t10');
