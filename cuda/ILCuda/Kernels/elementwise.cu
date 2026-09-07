// ---------------------------------------------------------------------------
// 逐元素（elementwise）算子族
//
// 所有内核都用 grid-stride 循环，因此 block 数不必覆盖全部元素——
// 调用方可以用 LaunchPlanner.For1D 限制 block 上限，超大规模向量也能一趟跑完。
// ---------------------------------------------------------------------------

#define EW_STRIDE_LOOP for (int i = blockDim.x * blockIdx.x + threadIdx.x; i < n; i += blockDim.x * gridDim.x)

// 二元算子：out[i] = f(a[i], b[i])
#define DEFINE_BINARY_KERNEL(NAME, EXPR)                                        \
extern "C" __global__ void NAME(const float* __restrict__ a,                    \
                                const float* __restrict__ b,                    \
                                float* __restrict__ out, int n) {               \
    EW_STRIDE_LOOP {                                                            \
        out[i] = EXPR;                                                          \
    }                                                                           \
}

// 一元算子：out[i] = f(x[i])
#define DEFINE_UNARY_KERNEL(NAME, EXPR)                                         \
extern "C" __global__ void NAME(const float* __restrict__ x,                    \
                                float* __restrict__ out, int n) {               \
    EW_STRIDE_LOOP {                                                            \
        out[i] = EXPR;                                                          \
    }                                                                           \
}

DEFINE_BINARY_KERNEL(ewAddKernel, a[i] + b[i])
DEFINE_BINARY_KERNEL(ewSubKernel, a[i] - b[i])
DEFINE_BINARY_KERNEL(ewMulKernel, a[i] * b[i])
// 除零保护：分母为 0 时写 0，避免产生 inf/nan 污染后续计算
DEFINE_BINARY_KERNEL(ewDivKernel, (b[i] != 0.0f) ? (a[i] / b[i]) : 0.0f)

DEFINE_UNARY_KERNEL(ewReluKernel, fmaxf(x[i], 0.0f))
DEFINE_UNARY_KERNEL(ewExpKernel, __expf(x[i]))
DEFINE_UNARY_KERNEL(ewLogKernel, __logf(x[i]))
DEFINE_UNARY_KERNEL(ewSqrtKernel, sqrtf(fmaxf(x[i], 0.0f)))
DEFINE_UNARY_KERNEL(ewAbsKernel, fabsf(x[i]))

// 标量缩放：out[i] = alpha * x[i]
extern "C" __global__ void ewScaleKernel(const float* __restrict__ x, float alpha,
                                         float* __restrict__ out, int n) {
    EW_STRIDE_LOOP {
        out[i] = alpha * x[i];
    }
}

// 乘加：out[i] = a[i] + alpha * b[i]（axpy 的一般形式）
extern "C" __global__ void ewAxpyKernel(const float* __restrict__ a,
                                        const float* __restrict__ b, float alpha,
                                        float* __restrict__ out, int n) {
    EW_STRIDE_LOOP {
        out[i] = a[i] + alpha * b[i];
    }
}
