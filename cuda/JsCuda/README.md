# JsCuda — JavaScript → CUDA

Translates **JavaScript functions into CUDA kernels** at run time, the JavaScript counterpart of
`cuda/ILCuda`'s "IL → AST → CUDA" pipeline:

```
JS function  ──►  JavaScript AST  ──►  typed AST (IL-style)  ──►  .cu source
              (LiteJs Parser)        (JsCudaDecompiler)      (CudaEmitter, reused)
                                                                    │
                                            NVRTC JIT ◄─────────────┘
                                                 │
                                          cuLaunchKernel  ──►  matrix on the GPU
```

The typed AST is the very same node model the IL pipeline produces, so `CudaEmitter` and
`CudaTypeMap` (including the `System.Math` → `sqrt/fabs/pow/...` table) are reused unchanged and
both pipelines generate device code in the same style.

Assembly: `Microsoft.VisualBasic.Computing.JSCuda` → `.nuget\net10.0\`.

---

## 1. Writing a kernel

A kernel is an ordinary JavaScript function. There are no CUDA keywords and no annotations.

### 1.1 Name convention (default)

```javascript
// i is the thread index; x is a buffer (indexed), cols is a scalar
function rowSum(i, x, cols) {
    let s = 0;

    for (let k = 0; k < cols; k++) {
        s = s + x[i * cols + k];
    }

    return s;
}
```

* parameters named **`i`** (and **`j`**) are the thread indices — they are *not* kernel arguments
* every other parameter that is indexed (`x[...]`) becomes a device buffer (`const double*`)
* every other parameter becomes a double scalar
* the **returned value** is written to the output array/matrix

### 1.2 Context sugar

```javascript
// one parameter: the context object; the parameters are derived from its usage
function distanceCell(ctx) {
    const d = ctx.rowSumSq[ctx.i] + ctx.rowSumSq[ctx.j] - 2 * ctx.dot[ctx.i * ctx.rows + ctx.j];

    if (ctx.i === ctx.j) return 0;
    if (d < 0) return 0;

    return Math.sqrt(d);
}
```

`ctx.i` / `ctx.j` are the thread indices, `ctx.<name>[k]` is a buffer, any other `ctx.<name>` is a
scalar. The kernel argument order is **buffers first, then scalars, each in order of appearance** —
`JsCudaHost.parameterNames(kernel)` prints the exact contract for a translated kernel.

### 1.3 Supported JavaScript subset

| supported | |
| --- | --- |
| statements | `var/let/const`, `if/else`, `while`, `do/while`, `for(;;)`, `return`, blocks, expression statements |
| operators | `+ - * / % **`, comparisons (`== != === !== < <= > >=`), `&& || !`, unary `-`, ternary `?:` |
| math | `Math.sqrt/abs/pow/exp/log/log10/min/max/floor/ceil/round/trunc/sin/cos/tan/asin/acos/atan/atan2/sinh/cosh/tanh` |
| buffers | `arr[index]` read and write |

| not supported (fails with a clear `JsCudaException`) | why |
| --- | --- |
| `arr.length` | a device pointer has no length — pass the length as a parameter |
| `%` on non-integer operands | C has no floating point modulo |
| calling another script function | only the `Math.*` functions are device callable |
| `for..of`, `switch`, `try/catch`, `break/continue` | not implemented in the device emitter |
| closures over outer variables | a kernel only sees its parameters |

Failures are explicit by design: a kernel that compiles but computes something else costs far more
than a translation error naming the offending node and source position.

**Numbers are doubles.** Every local stays `double` even when it is initialised with an integral
literal, so `let s = 0; s = s + x[k];` accumulates fractional values correctly. Subscripts are
cast to `int` automatically. Thread indices and for-loop counters are `int`.

---

## 2. Running it from a script

```javascript
import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '.../Microsoft.VisualBasic.Computing.JSCuda.dll';

const kernel = jscuda.JsCudaHost.translate(rowSum);        // .cu source + typed AST
console.log(jscuda.JsCudaHost.cudaSource(kernel));         // the generated device code
console.log(jscuda.JsCudaHost.pseudoCode(kernel));         // the recovered typed AST

jscuda.JsCudaHost.register(kernel);                        // before the engine is created

const engine = jscuda.JsCudaHost.createEngine();
const input = jscuda.JsCudaHost.buffer([1, 2, 3, 4]);      // double buffer
const output = jscuda.JsCudaHost.bufferSize(4);
const sink = jscuda.JsCudaHost.buffer([10, 20, 30, 40]);

jscuda.JsCudaHost.launch1D(kernel, engine, 4, 256, sink, input, 4);
jscuda.JsCudaHost.synchronize(engine);

const result = jscuda.JsCudaHost.read(sink);               // double[]

output.Dispose(); input.Dispose(); sink.Dispose(); engine.Dispose();
```

Entry points (all static, all lowercase):

| member | purpose |
| --- | --- |
| `translate(fn, options?)` | JS function value → kernel |
| `translateSource(text, options?)` | JavaScript source text → kernel (tests, docs) |
| `register(kernel)` | add the source to the NVRTC compilation unit |
| `cudaSource(kernel)` / `pseudoCode(kernel)` / `indexMode(kernel)` / `parameterNames(kernel)` | inspect the translation |
| `available()` / `deviceName()` / `diagnostics()` | probe the device |
| `createEngine()` / `synchronize(engine)` | engine life cycle |
| `buffer(values)` / `bufferSize(n)` / `read(buffer)` | double precision device memory |
| `launch1D(kernel, engine, n, blockSize, output, ...args)` | element-wise / 1D launch |
| `launch2D(kernel, engine, rows, cols, tile, output, ...args)` | matrix launch |
| `maxError(gpu, cpu)` / `preview(values, rows, n)` | comparison helpers |

The host must pass **exactly** the arguments `parameterNames()` lists (thread indices excluded);
JavaScript numbers are converted to the declared `int` / `double` slot automatically.

CLR member names are case sensitive from a script: use `JsCudaHost.translate`, not
`JsCudaHost.Translate`.

---

## 3. Tests

```bat
dotnet test cuda\JsCuda\test\JsCuda.Tests.vbproj
```

The suite is translation-only (no GPU needed): it asserts the generated `.cu` text, the index mode
inference, the `ctx` sugar equivalence, the parameter kinds, and that every unsupported construct
fails with the documented message.

The GPU path is exercised by `tutorials\VBS\scripts\cuda\cuda.js`, which compares the kernel output
with the same JavaScript function running on the host (double precision, so the error is ~0).
