import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '../../../../.nuget/net10.0/Microsoft.VisualBasic.Computing.JSCuda.dll';

function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
    }

    return s;
}

const k = jscuda.JsCudaHost.translate(rowSum);

console.log(jscuda.JsCudaHost.cudaSource(k));
console.log('done: t11');