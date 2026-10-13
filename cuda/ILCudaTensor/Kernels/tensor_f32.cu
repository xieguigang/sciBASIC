// ---------------------------------------------------------------------------
// tensor_f32.cu —— 单精度（float32）张量栈的手写 CUDA 内核
//
// 与 tensor.cu（双精度栈）的分工完全相同：只写 IL2Cuda 表达不了的那部分。
// 单精度栈的逐元素 / 标量算子全部由 FloatKernels.vb 经 IL2Cuda 自动生成，
// 这里只提供：
//
//   1. 两段式全局归约（需要块内共享内存树形归约，IL2Cuda 无法表达）
//   2. CFD 四个扩展原语（三维网格索引 + 掩膜 + ping-pong 迭代）
//
// 矩阵乘不在本文件：直接复用 ILCuda 自带的 gemmKernel（Kernels\blas.cu，单精度）。
//
// 命名约定：全部以 tensorF32 开头，避免与双精度栈的 tensor* 内核在同一
// NVRTC 编译单元里撞名。
//
// 网格布局约定（与 Compute\TensorComputeFBase.vb 的 CPU 参考实现严格一致）：
//       idx = (i * ny + j) * nz + k
//   * (i±1, j, k) -> idx ± plane  (plane = ny * nz)
//   * (i, j±1, k) -> idx ± nz
//   * (i, j, k±1) -> idx ± 1
// 网格间距恒为 1（cell 单位）：CPU 参考实现里没有任何 h / spacing / 1/h^2，
// 因此这里也绝不能引入 —— 速度场的单位是"格/单位时间"，dt·u 即"格数"。
// ---------------------------------------------------------------------------

#define TENSOR_F32_REDUCE_BLOCK 256
#define TENSOR_F32_NEG_BIG (-3.402823466e+38f)
#define TENSOR_F32_POS_BIG (3.402823466e+38f)

// ---------------------------------------------------------------------------
// 1. 两段式全局归约
//
// 启动（与双精度栈的 ReduceGlobal 同构）：
//   阶段一 grid = (blocks, 1)，block = (TENSOR_F32_REDUCE_BLOCK, 1)
//   阶段二 grid = (1, 1)，     block = (TENSOR_F32_REDUCE_BLOCK, 1)
//
// 求和用 double 累加：ITensorComputeF.SumAll 的契约明确要求"累加使用双精度，
// 避免大数组单精度累加的舍入误差"，返回类型也是 Double。
// 极值没有这个问题，全程 float（MinAll / MaxAll 返回 Single）。
// ---------------------------------------------------------------------------

#define DEFINE_F32_PARTIAL_SUM(NAME)                                              \
extern "C" __global__ void NAME(const float* __restrict__ x,                      \
                                double* __restrict__ partials, int n) {           \
    __shared__ double cache[TENSOR_F32_REDUCE_BLOCK];                             \
    const int tid = threadIdx.x;                                                  \
    double acc = 0.0;                                                             \
    for (int i = blockIdx.x * blockDim.x + tid; i < n; i += blockDim.x * gridDim.x) \
        acc += (double)x[i];                                                      \
    cache[tid] = acc;                                                             \
    __syncthreads();                                                              \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                               \
        if (tid < s) cache[tid] += cache[tid + s];                                \
        __syncthreads();                                                          \
    }                                                                             \
    if (tid == 0) partials[blockIdx.x] = cache[0];                                \
}

#define DEFINE_F32_FINAL_SUM(NAME)                                                \
extern "C" __global__ void NAME(const double* __restrict__ partials,              \
                                double* __restrict__ out, int count) {            \
    __shared__ double cache[TENSOR_F32_REDUCE_BLOCK];                             \
    const int tid = threadIdx.x;                                                  \
    double acc = 0.0;                                                             \
    for (int i = tid; i < count; i += blockDim.x) acc += partials[i];             \
    cache[tid] = acc;                                                             \
    __syncthreads();                                                              \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                               \
        if (tid < s) cache[tid] += cache[tid + s];                                \
        __syncthreads();                                                          \
    }                                                                             \
    if (tid == 0) out[0] = cache[0];                                              \
}

#define DEFINE_F32_PARTIAL_EXTREME(NAME, INIT, COMBINE)                           \
extern "C" __global__ void NAME(const float* __restrict__ x,                      \
                                float* __restrict__ partials, int n) {            \
    __shared__ float cache[TENSOR_F32_REDUCE_BLOCK];                              \
    const int tid = threadIdx.x;                                                  \
    float acc = INIT;                                                             \
    for (int i = blockIdx.x * blockDim.x + tid; i < n; i += blockDim.x * gridDim.x) \
        acc = COMBINE(acc, x[i]);                                                 \
    cache[tid] = acc;                                                             \
    __syncthreads();                                                              \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                               \
        if (tid < s) cache[tid] = COMBINE(cache[tid], cache[tid + s]);            \
        __syncthreads();                                                          \
    }                                                                             \
    if (tid == 0) partials[blockIdx.x] = cache[0];                                \
}

#define DEFINE_F32_FINAL_EXTREME(NAME, INIT, COMBINE)                             \
extern "C" __global__ void NAME(const float* __restrict__ partials,               \
                                float* __restrict__ out, int count) {             \
    __shared__ float cache[TENSOR_F32_REDUCE_BLOCK];                              \
    const int tid = threadIdx.x;                                                  \
    float acc = INIT;                                                             \
    for (int i = tid; i < count; i += blockDim.x) acc = COMBINE(acc, partials[i]);\
    cache[tid] = acc;                                                             \
    __syncthreads();                                                              \
    for (int s = blockDim.x >> 1; s > 0; s >>= 1) {                               \
        if (tid < s) cache[tid] = COMBINE(cache[tid], cache[tid + s]);            \
        __syncthreads();                                                          \
    }                                                                             \
    if (tid == 0) out[0] = cache[0];                                              \
}

DEFINE_F32_PARTIAL_SUM(tensorF32ReducePartialSumKernel)
DEFINE_F32_FINAL_SUM(tensorF32ReduceFinalSumKernel)
DEFINE_F32_PARTIAL_EXTREME(tensorF32ReducePartialMaxKernel, TENSOR_F32_NEG_BIG, fmaxf)
DEFINE_F32_FINAL_EXTREME(tensorF32ReduceFinalMaxKernel, TENSOR_F32_NEG_BIG, fmaxf)
DEFINE_F32_PARTIAL_EXTREME(tensorF32ReducePartialMinKernel, TENSOR_F32_POS_BIG, fminf)
DEFINE_F32_FINAL_EXTREME(tensorF32ReduceFinalMinKernel, TENSOR_F32_POS_BIG, fminf)

// ---------------------------------------------------------------------------
// 2. CFD 扩展原语
//
// 三个 stencil 原语共用同一组约定：
//   * mask / nFluid 可以为 NULL：mask == NULL 表示全流体，nFluid == NULL 时
//     邻居数退化为常数 6（与 CPU 参考实现的 `If(nFluid Is Nothing, 6.0, ...)` 一致）
//   * 掩膜语义：0 = 固体 / 空腔，非 0 = 流体（判据是 `!= 0` 而不是 `== 1`）
//   * 邻居求和用 double 累加、顺序严格为 -i, +i, -j, +j, -k, +k，
//     末尾只做一次 (float) 舍入 —— CPU 参考实现就是这么写的，要逐点一致就必须照抄
//
// 由 idx 反解 (i, j, k)（CPU 侧是三重循环，GPU 只能反解）：
//       i = idx / plane;  rem = idx % plane;  j = rem / nz;  k = rem % nz
// ---------------------------------------------------------------------------

// 固体单元 / 非内部单元写 0。Laplacian 的输出缓冲必须是"全 0 初始化"的
// （CPU 侧 r 由 New Single(...) 得到），这里改为让内核覆盖整个 idx 范围并
// 显式写 0，从而不依赖缓冲的初始内容。
extern "C" __global__ void tensorLaplacian7Fp32Kernel(const float* __restrict__ src,
                                                      const unsigned char* __restrict__ mask,
                                                      const unsigned char* __restrict__ nFluid,
                                                      float* __restrict__ out,
                                                      int nx, int ny, int nz, int plane, int n) {
    const int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx >= n) return;

    const int i = idx / plane;
    const int rem = idx - i * plane;
    const int j = rem / nz;
    const int k = rem - j * nz;

    // 只算内部，留一层 ghost 壳（与 CPU 的 For i = 1 To nx-2 完全对应）
    if (i < 1 || i > nx - 2 || j < 1 || j > ny - 2 || k < 1 || k > nz - 2) {
        out[idx] = 0.0f;
        return;
    }

    if (mask != NULL && mask[idx] == 0) {
        out[idx] = 0.0f;
        return;
    }

    double s = 0.0;
    if (mask == NULL || mask[idx - plane] != 0) s += (double)src[idx - plane];
    if (mask == NULL || mask[idx + plane] != 0) s += (double)src[idx + plane];
    if (mask == NULL || mask[idx - nz]    != 0) s += (double)src[idx - nz];
    if (mask == NULL || mask[idx + nz]    != 0) s += (double)src[idx + nz];
    if (mask == NULL || mask[idx - 1]     != 0) s += (double)src[idx - 1];
    if (mask == NULL || mask[idx + 1]     != 0) s += (double)src[idx + 1];

    const double div = (nFluid == NULL) ? 6.0 : (double)nFluid[idx];

    out[idx] = (float)(s - div * (double)src[idx]);
}

// Jacobi 迭代的一步：读 prev 写 cur（真 Jacobi，不是 Gauss-Seidel）。
//
// 主机侧负责 ping-pong：两块缓冲交替作为 prev / cur，共启动 iterations 次。
// 绝不能在同一块缓冲上就地写 —— CPU 参考实现每轮开头都做 Array.Copy(cur, prev)。
//
// 关键：非内部单元**不写**（保留初值）。这与 Laplacian7 的"边界恒 0"不同：
// CPU 侧 cur 由 initial 拷贝而来，边界永远不被触碰，因此带 initial 调用时
// 边界会保留 initial 的值（包括 mask=0 的边界格也不会被置零）。
extern "C" __global__ void tensorJacobi7Fp32Kernel(const float* __restrict__ prev,
                                                   float* __restrict__ cur,
                                                   const float* __restrict__ rhs,
                                                   const unsigned char* __restrict__ mask,
                                                   const unsigned char* __restrict__ nFluid,
                                                   float alpha, float beta,
                                                   int nx, int ny, int nz, int plane, int n) {
    const int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx >= n) return;

    const int i = idx / plane;
    const int rem = idx - i * plane;
    const int j = rem / nz;
    const int k = rem - j * nz;

    if (i < 1 || i > nx - 2 || j < 1 || j > ny - 2 || k < 1 || k > nz - 2) return;

    if (mask != NULL && mask[idx] == 0) {
        cur[idx] = 0.0f;
        return;
    }

    double s = 0.0;
    if (mask == NULL || mask[idx - plane] != 0) s += (double)prev[idx - plane];
    if (mask == NULL || mask[idx + plane] != 0) s += (double)prev[idx + plane];
    if (mask == NULL || mask[idx - nz]    != 0) s += (double)prev[idx - nz];
    if (mask == NULL || mask[idx + nz]    != 0) s += (double)prev[idx + nz];
    if (mask == NULL || mask[idx - 1]     != 0) s += (double)prev[idx - 1];
    if (mask == NULL || mask[idx + 1]     != 0) s += (double)prev[idx + 1];

    // 条件是严格的 beta > 0：beta = 0 与 beta < 0 都走 nFluid 分支
    double denom = (beta > 0.0f) ? (double)beta
                                 : ((nFluid == NULL) ? 6.0 : (double)nFluid[idx]);
    if (denom == 0.0) denom = 1.0;

    cur[idx] = (float)(((double)alpha * s + (double)rhs[idx]) / denom);
}

// 半拉格朗日平流：沿速度场反向追踪 dt，在回溯位置对源场做三线性插值。
// 纯 gather（只读源场、只写自己的 idx），每个 idx 完全独立。
//
// 位移必须先用 float32 算：CPU 侧 `i - dt * uu(idx)` 中 dt 与 uu(idx) 都是 Single，
// VB 的数值提升规则让整个表达式留在 Single 域；若这里改成 double 会差 1 ulp。
// 之后的钳制 [0.5, n-1.5] 与插值则用 double（CPU 侧 std.Min/std.Max 的形参是 Double）。
extern "C" __global__ void tensorAdvectTrilinearFp32Kernel(const float* __restrict__ src,
                                                           const float* __restrict__ u,
                                                           const float* __restrict__ v,
                                                           const float* __restrict__ w,
                                                           const unsigned char* __restrict__ mask,
                                                           float dt,
                                                           float* __restrict__ out,
                                                           int nx, int ny, int nz, int plane, int n) {
    const int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx >= n) return;

    if (mask != NULL && mask[idx] == 0) {
        out[idx] = 0.0f;
        return;
    }

    const int i = idx / plane;
    const int rem = idx - i * plane;
    const int j = rem / nz;
    const int k = rem - j * nz;

    // 全网格（与 Laplacian / Jacobi 的"只算内部"不同）
    const float dispX = (float)i - dt * u[idx];
    const float dispY = (float)j - dt * v[idx];
    const float dispZ = (float)k - dt * w[idx];

    const double px = fmin(fmax((double)dispX, 0.5), (double)nx - 1.5);
    const double py = fmin(fmax((double)dispY, 0.5), (double)ny - 1.5);
    const double pz = fmin(fmax((double)dispZ, 0.5), (double)nz - 1.5);

    // ---- TrilinearAt（逐行对应 TensorComputeFBase.TrilinearAt）----
    //
    // 注意 fx/fy/fz 用**未钳制**的 floor 计算，且 i1 先于 i0 的钳制求值。
    // 这条顺序不能改：在越界路径上换顺序会得到不同结果。
    const int fi0 = (int)floor(px);
    const int fj0 = (int)floor(py);
    const int fk0 = (int)floor(pz);

    const double fx = px - (double)fi0;
    const double fy = py - (double)fj0;
    const double fz = pz - (double)fk0;

    const int i1 = (fi0 + 1 < nx - 1) ? (fi0 + 1) : (nx - 1);
    const int j1 = (fj0 + 1 < ny - 1) ? (fj0 + 1) : (ny - 1);
    const int k1 = (fk0 + 1 < nz - 1) ? (fk0 + 1) : (nz - 1);

    const int i0 = (fi0 < 0) ? 0 : ((fi0 > nx - 1) ? (nx - 1) : fi0);
    const int j0 = (fj0 < 0) ? 0 : ((fj0 > ny - 1) ? (ny - 1) : fj0);
    const int k0 = (fk0 < 0) ? 0 : ((fk0 > nz - 1) ? (nz - 1) : fk0);

    const int a = i0 * plane + j0 * nz;
    const int b = i0 * plane + j1 * nz;
    const int c = i1 * plane + j0 * nz;
    const int d = i1 * plane + j1 * nz;

    const double c000 = (double)src[a + k0];
    const double c001 = (double)src[a + k1];
    const double c010 = (double)src[b + k0];
    const double c011 = (double)src[b + k1];
    const double c100 = (double)src[c + k0];
    const double c101 = (double)src[c + k1];
    const double c110 = (double)src[d + k0];
    const double c111 = (double)src[d + k1];

    // 混合顺序：先 x(i) -> 再 y(j) -> 最后 z(k)，全程 double
    const double c00 = c000 * (1.0 - fx) + c100 * fx;
    const double c01 = c001 * (1.0 - fx) + c101 * fx;
    const double c10 = c010 * (1.0 - fx) + c110 * fx;
    const double c11 = c011 * (1.0 - fx) + c111 * fx;

    const double c0 = c00 * (1.0 - fy) + c10 * fy;
    const double c1 = c01 * (1.0 - fy) + c11 * fy;

    out[idx] = (float)(c0 * (1.0 - fz) + c1 * fz);
}

// 按掩膜把固体单元置零。与另外三个原语不同，ApplyMask 是**就地**语义：
// CPU 参考实现直接改写调用方张量的底层数组，因此主机侧需要"上传 -> 内核 ->
// 回写主机数组"，回写之后必须让设备缓存失效（TensorF.MarkHostModified）。
extern "C" __global__ void tensorApplyMaskFp32Kernel(const unsigned char* __restrict__ mask,
                                                     float* __restrict__ field, int n) {
    const int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx >= n) return;

    if (mask[idx] == 0) field[idx] = 0.0f;
}
