import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function twice(i, x) {
    return x[i] * 2;
}

const k = jscuda.JsCudaHost.translate(twice);

console.log('indexMode = ' + jscuda.JsCudaHost.indexMode(k));
jscuda.JsCudaHost.register(k);
console.log('registered');

if (jscuda.JsCudaHost.available()) {
    const engine = jscuda.JsCudaHost.createEngine();
    const input = jscuda.JsCudaHost.buffer([1, 2, 3, 4]);
    const output = jscuda.JsCudaHost.bufferSize(4);

    jscuda.JsCudaHost.launch1D(k, engine, 4, 4, output, input);
    jscuda.JsCudaHost.synchronize(engine);

    const result = jscuda.JsCudaHost.read(output);

    console.log('gpu = ' + jscuda.JsCudaHost.preview(result, 2, 2));

    output.Dispose();
    input.Dispose();
    engine.Dispose();
} else {
    console.log('no gpu');
}

console.log('done: t6');
