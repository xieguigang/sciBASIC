// ---------------------------------------------------------------------------
// 手写张量内核（double 精度）
//
// 覆盖 IL2Cuda 无法表达、且框架自带 float 内核也不支持的“按行协作”算子：
//   * softmax / log-softmax（沿最后一维）
//   * 沿最后一维的归约：sum / mean / max / min
//   * 沿最后一维的极值下标：argmax / argmin
//
// 约定：
//   * 输入按行主序展平，共 outerSize 行，每行 axisSize 个**连续**元素（沿最后一维，
//     stride = 1）。这正是 softmax / 交叉熵 / 逐样本归约在深度学习里的常见形态。
//   * 每个 block 负责一行，块内用共享内存做树形归约。
//
// 启动形状：grid = (outerSize, 1)，block = (TENSOR_BLOCK, 1)，动态共享内存 = 0。
//   注意：block 的线程数必须等于 TENSOR_BLOCK（共享内存数组是静态声明的）。
//
// 说明：这里刻意使用**静态**共享内存（而不是 extern __shared__），
//       因为 NVRTC 对“同一个编译单元里多个内核各自声明 extern __shared__”
//       支持不佳。同理不使用 INFINITY 宏，改用显式的极大值作为单位元。
//
// 所有符号统一带 tensorRow 前缀，避免与框架自带内核及 IL 生成内核撞名。
// ---------------------------------------------------------------------------

#define TENSOR_BLOCK 256
#define TENSOR_NEG_BIG (-1.0e308)
#define TENSOR_POS_BIG (1.0e308)

// ---------------------------------------------------------------------------
// softmax：out(i) = exp(x(i) - max) / sum(exp(x - max))
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorRowSoftmaxKernel(const double* __restrict__ x,
                                                  double* __restrict__ out,
                                                  int outerSize, int axisSize) {
    __shared__ double cache[TENSOR_BLOCK];

    const int row = blockIdx.x;
    if (row >= outerSize) return;

    const double* p = x + (long long)row * axisSize;
    double* q = out + (long long)row * axisSize;
    const int tid = threadIdx.x;

    // 阶段 1：行内最大值（数值稳定）
    double m = TENSOR_NEG_BIG;
    for (int k = tid; k < axisSize; k += blockDim.x) m = fmax(m, p[k]);
    cache[tid] = m;
    __syncthreads();
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {
        if (tid < s) cache[tid] = fmax(cache[tid], cache[tid + s]);
        __syncthreads();
    }
    m = cache[0];
    __syncthreads();

    // 阶段 2：sum(exp(x - m))
    double sum = 0.0;
    for (int k = tid; k < axisSize; k += blockDim.x) sum += exp(p[k] - m);
    cache[tid] = sum;
    __syncthreads();
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {
        if (tid < s) cache[tid] += cache[tid + s];
        __syncthreads();
    }
    sum = cache[0];

    // 阶段 3：归一化写回
    for (int k = tid; k < axisSize; k += blockDim.x) q[k] = exp(p[k] - m) / sum;
}

// ---------------------------------------------------------------------------
// log-softmax：out(i) = x(i) - (max + log(sum(exp(x - max))))
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorRowLogSoftmaxKernel(const double* __restrict__ x,
                                                     double* __restrict__ out,
                                                     int outerSize, int axisSize) {
    __shared__ double cache[TENSOR_BLOCK];

    const int row = blockIdx.x;
    if (row >= outerSize) return;

    const double* p = x + (long long)row * axisSize;
    double* q = out + (long long)row * axisSize;
    const int tid = threadIdx.x;

    double m = TENSOR_NEG_BIG;
    for (int k = tid; k < axisSize; k += blockDim.x) m = fmax(m, p[k]);
    cache[tid] = m;
    __syncthreads();
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {
        if (tid < s) cache[tid] = fmax(cache[tid], cache[tid + s]);
        __syncthreads();
    }
    m = cache[0];
    __syncthreads();

    double sum = 0.0;
    for (int k = tid; k < axisSize; k += blockDim.x) sum += exp(p[k] - m);
    cache[tid] = sum;
    __syncthreads();
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {
        if (tid < s) cache[tid] += cache[tid + s];
        __syncthreads();
    }

    const double lse = m + log(cache[0]);

    for (int k = tid; k < axisSize; k += blockDim.x) q[k] = p[k] - lse;
}

// ---------------------------------------------------------------------------
// 归约：sum / mean / max / min（每个 block 一行，结果写 out[row]）
// ---------------------------------------------------------------------------
#define DEFINE_ROW_REDUCE(NAME, INIT, COMBINE, FINALIZE)                         \
extern "C" __global__ void NAME(const double* __restrict__ x,                    \
                                double* __restrict__ out,                        \
                                int outerSize, int axisSize) {                   \
    __shared__ double cache[TENSOR_BLOCK];                                       \
    const int row = blockIdx.x;                                                  \
    if (row >= outerSize) return;                                                \
    const double* p = x + (long long)row * axisSize;                             \
    const int tid = threadIdx.x;                                                 \
                                                                                 \
    double acc = INIT;                                                           \
    for (int k = tid; k < axisSize; k += blockDim.x) acc = COMBINE(acc, p[k]);   \
    cache[tid] = acc;                                                            \
    __syncthreads();                                                             \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                              \
        if (tid < s) cache[tid] = COMBINE(cache[tid], cache[tid + s]);           \
        __syncthreads();                                                         \
    }                                                                            \
    if (tid == 0) out[row] = FINALIZE(cache[0], axisSize);                       \
}

#define ROW_ADD(a, b) ((a) + (b))
#define ROW_MAX(a, b) fmax(a, b)
#define ROW_MIN(a, b) fmin(a, b)
#define ROW_SAME(v, n) (v)
#define ROW_MEAN(v, n) ((v) / (double)(n))

DEFINE_ROW_REDUCE(tensorRowReduceSumKernel, 0.0, ROW_ADD, ROW_SAME)
DEFINE_ROW_REDUCE(tensorRowReduceMeanKernel, 0.0, ROW_ADD, ROW_MEAN)
DEFINE_ROW_REDUCE(tensorRowReduceMaxKernel, TENSOR_NEG_BIG, ROW_MAX, ROW_SAME)
DEFINE_ROW_REDUCE(tensorRowReduceMinKernel, TENSOR_POS_BIG, ROW_MIN, ROW_SAME)

// ---------------------------------------------------------------------------
// 极值下标：argmax / argmin（返回该行的下标，以 double 写回 out[row]）
// ---------------------------------------------------------------------------
#define DEFINE_ROW_ARG(NAME, INIT, COMBINE, PICK)                                \
extern "C" __global__ void NAME(const double* __restrict__ x,                    \
                                double* __restrict__ out,                        \
                                int outerSize, int axisSize) {                   \
    __shared__ double cache[TENSOR_BLOCK];                                       \
    const int row = blockIdx.x;                                                  \
    if (row >= outerSize) return;                                                \
    const double* p = x + (long long)row * axisSize;                             \
    const int tid = threadIdx.x;                                                 \
                                                                                 \
    double best = INIT;                                                          \
    for (int k = tid; k < axisSize; k += blockDim.x) best = COMBINE(best, p[k]); \
    cache[tid] = best;                                                           \
    __syncthreads();                                                             \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                              \
        if (tid < s) cache[tid] = COMBINE(cache[tid], cache[tid + s]);           \
        __syncthreads();                                                         \
    }                                                                            \
    best = cache[0];                                                             \
    __syncthreads();                                                             \
                                                                                 \
    int idx = axisSize;                                                          \
    for (int k = tid; k < axisSize; k += blockDim.x) {                           \
        if (PICK(p[k], best)) idx = min(idx, k);                                 \
    }                                                                            \
    cache[tid] = (double)idx;                                                    \
    __syncthreads();                                                             \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                              \
        if (tid < s) cache[tid] = fmin(cache[tid], cache[tid + s]);              \
        __syncthreads();                                                         \
    }                                                                            \
    if (tid == 0) out[row] = cache[0];                                           \
}

#define PICK_EQ(v, ref) ((v) == (ref))

DEFINE_ROW_ARG(tensorRowArgMaxKernel, TENSOR_NEG_BIG, ROW_MAX, PICK_EQ)
DEFINE_ROW_ARG(tensorRowArgMinKernel, TENSOR_POS_BIG, ROW_MIN, PICK_EQ)
