// ---------------------------------------------------------------------------
// 通用归约内核（sum / max / min）
//
// 采用"两阶段"结构，避免主机端同步：
//   1) reduceXxxKernel    —— 每个 block 用共享内存树形归约出一个部分结果，
//                            内部用 grid-stride 循环，因此可以处理任意长度的输入；
//   2) reduceFinalXxxKernel —— 单独一个 block 把部分结果汇总成最终值。
//
// 需要的动态共享内存：blockDim.x * sizeof(float)
// ---------------------------------------------------------------------------

#define REDUCE_ADD(a, b) ((a) + (b))
#define REDUCE_MAX(a, b) fmaxf((a), (b))
#define REDUCE_MIN(a, b) fminf((a), (b))

#define REDUCE_POS_INF 3.402823466e+38f
#define REDUCE_NEG_INF (-3.402823466e+38f)

// 第一阶段：每个 block 归约出 1 个部分结果
#define DEFINE_REDUCE_STAGE(NAME, INIT, OP)                                     \
extern "C" __global__ void NAME(const float* __restrict__ x,                    \
                                float* __restrict__ partial, int n) {           \
    extern __shared__ float cache[];                                            \
                                                                                \
    const unsigned int tid = threadIdx.x;                                       \
    const unsigned int stride = (unsigned int)blockDim.x * gridDim.x;           \
                                                                                \
    float acc = INIT;                                                           \
    for (unsigned int i = (unsigned int)blockIdx.x * blockDim.x + tid;          \
         i < (unsigned int)n; i += stride) {                                    \
        acc = OP(acc, x[i]);                                                    \
    }                                                                           \
                                                                                \
    cache[tid] = acc;                                                           \
    __syncthreads();                                                            \
                                                                                \
    for (unsigned int s = blockDim.x >> 1; s > 0; s >>= 1) {                    \
        if (tid < s) {                                                          \
            cache[tid] = OP(cache[tid], cache[tid + s]);                        \
        }                                                                       \
        __syncthreads();                                                        \
    }                                                                           \
                                                                                \
    if (tid == 0) {                                                             \
        partial[blockIdx.x] = cache[0];                                         \
    }                                                                           \
}

// 第二阶段：把 count 个部分结果汇总成最终值（用 1 个 block 启动）
#define DEFINE_REDUCE_FINAL(NAME, INIT, OP)                                     \
extern "C" __global__ void NAME(const float* __restrict__ partial,              \
                                float* __restrict__ out, int count) {           \
    extern __shared__ float cache[];                                            \
                                                                                \
    const unsigned int tid = threadIdx.x;                                       \
                                                                                \
    float acc = INIT;                                                           \
    for (int i = (int)tid; i < count; i += (int)blockDim.x) {                   \
        acc = OP(acc, partial[i]);                                              \
    }                                                                           \
                                                                                \
    cache[tid] = acc;                                                           \
    __syncthreads();                                                            \
                                                                                \
    for (unsigned int s = blockDim.x >> 1; s > 0; s >>= 1) {                    \
        if (tid < s) {                                                          \
            cache[tid] = OP(cache[tid], cache[tid + s]);                        \
        }                                                                       \
        __syncthreads();                                                        \
    }                                                                           \
                                                                                \
    if (tid == 0) {                                                             \
        out[0] = cache[0];                                                      \
    }                                                                           \
}

DEFINE_REDUCE_STAGE(reduceSumKernel, 0.0f, REDUCE_ADD)
DEFINE_REDUCE_STAGE(reduceMaxKernel, REDUCE_NEG_INF, REDUCE_MAX)
DEFINE_REDUCE_STAGE(reduceMinKernel, REDUCE_POS_INF, REDUCE_MIN)

DEFINE_REDUCE_FINAL(reduceFinalSumKernel, 0.0f, REDUCE_ADD)
DEFINE_REDUCE_FINAL(reduceFinalMaxKernel, REDUCE_NEG_INF, REDUCE_MAX)
DEFINE_REDUCE_FINAL(reduceFinalMinKernel, REDUCE_POS_INF, REDUCE_MIN)
