// ---------------------------------------------------------------------------
// 行间相似度度量内核（皮尔逊相关系数 + 欧氏距离）
//
// 两种度量都可以归约成"行向量之间的点积"：
//
//   记第 i 行的 sum_i = Σ x_ik, sumSq_i = Σ x_ik^2, dot_ij = Σ x_ik * x_jk
//   mean_i = sum_i / n
//   var_i  = sumSq_i - n * mean_i^2                 (Σ (x_ik - mean_i)^2)
//
//   皮尔逊: corr_ij = (dot_ij - n * mean_i * mean_j) / sqrt(var_i * var_j)
//   欧氏  : dist_ij = sqrt(sumSq_i + sumSq_j - 2 * dot_ij)
//
// 因此整条流水线由三个内核组成：
//   1) rowStatsKernel  —— 每个 block 负责一行，用共享内存做树形归约
//   2) gramKernel      —— 16x16 分块的点积矩阵，利用对称性只算上三角
//   3) finalizeKernel  —— 用点积与行统计量同时产出相关矩阵与距离矩阵
//
// 本文件属于 demo（test 工程的内嵌资源），运行时注册进 ILCuda 框架，
// 与框架自带的内核一起编译进同一个模块。
// ---------------------------------------------------------------------------

#define METRICS_TILE 16

extern "C" __global__ void rowStatsKernel(const float* __restrict__ x, int rows, int cols,
                                          float* __restrict__ rowSum, float* __restrict__ rowSumSq) {
    extern __shared__ float cache[];

    float* sSum = cache;
    float* sSumSq = cache + blockDim.x;

    const int row = blockIdx.x;
    const unsigned int tid = threadIdx.x;
    const float* rowPtr = x + (size_t)row * (size_t)cols;

    float localSum = 0.0f;
    float localSumSq = 0.0f;

    for (int c = (int)tid; c < cols; c += (int)blockDim.x) {
        float v = rowPtr[c];
        localSum += v;
        localSumSq += v * v;
    }

    sSum[tid] = localSum;
    sSumSq[tid] = localSumSq;
    __syncthreads();

    for (unsigned int stride = blockDim.x >> 1; stride > 0; stride >>= 1) {
        if (tid < stride) {
            sSum[tid] += sSum[tid + stride];
            sSumSq[tid] += sSumSq[tid + stride];
        }
        __syncthreads();
    }

    if (tid == 0) {
        rowSum[row] = sSum[0];
        rowSumSq[row] = sSumSq[0];
    }
}

extern "C" __global__ void gramKernel(const float* __restrict__ x, int rows, int cols,
                                      float* __restrict__ dot) {
    __shared__ float aTile[METRICS_TILE][METRICS_TILE];
    __shared__ float bTile[METRICS_TILE][METRICS_TILE];

    const int bx = blockIdx.x;   // 列方向（j）
    const int by = blockIdx.y;   // 行方向（i）

    // dot 矩阵是对称的，下三角的 block 直接退出，计算量减半
    if (bx > by) {
        return;
    }

    const int tx = threadIdx.x;
    const int ty = threadIdx.y;
    const int i = by * METRICS_TILE + ty;
    const int j = bx * METRICS_TILE + tx;

    float acc = 0.0f;
    const int nTiles = (cols + METRICS_TILE - 1) / METRICS_TILE;

    for (int t = 0; t < nTiles; ++t) {
        const int ca = t * METRICS_TILE + tx;
        const int cb = t * METRICS_TILE + ty;

        aTile[ty][tx] = (i < rows && ca < cols) ? x[(size_t)i * (size_t)cols + (size_t)ca] : 0.0f;
        bTile[ty][tx] = (j < rows && cb < cols) ? x[(size_t)j * (size_t)cols + (size_t)cb] : 0.0f;

        __syncthreads();

#pragma unroll
        for (int k = 0; k < METRICS_TILE; ++k) {
            acc += aTile[ty][k] * bTile[k][tx];
        }

        __syncthreads();
    }

    if (i < rows && j < rows) {
        dot[(size_t)i * (size_t)rows + (size_t)j] = acc;
        dot[(size_t)j * (size_t)rows + (size_t)i] = acc;
    }
}

extern "C" __global__ void finalizeKernel(const float* __restrict__ dot,
                                          const float* __restrict__ rowSum,
                                          const float* __restrict__ rowSumSq,
                                          int rows, int cols,
                                          float* __restrict__ corr,
                                          float* __restrict__ dist) {
    const int j = blockIdx.x * blockDim.x + threadIdx.x;
    const int i = blockIdx.y * blockDim.y + threadIdx.y;

    if (i >= rows || j >= rows) {
        return;
    }

    const float n = (float)cols;
    const float meanI = rowSum[i] / n;
    const float meanJ = rowSum[j] / n;
    const float varI = rowSumSq[i] - n * meanI * meanI;
    const float varJ = rowSumSq[j] - n * meanJ * meanJ;
    const size_t index = (size_t)i * (size_t)rows + (size_t)j;

    // 对角线：数学上 corr = 1、dist = 0。
    // 这里必须直接赋值——若走通用公式，dist 会变成
    // sqrt(sumSq_i + sumSq_i - 2*dot_ii)，两个相近的大数相减后开根号，
    // 浮点舍入误差会被 sqrt 放大（实测可达 1e-2 量级）。
    if (i == j) {
        corr[index] = 1.0f;
        dist[index] = 0.0f;
        return;
    }

    const float d = dot[index];
    const float cov = d - n * meanI * meanJ;
    const float denom = sqrtf(fmaxf(varI, 0.0f)) * sqrtf(fmaxf(varJ, 0.0f));
    const float c = (denom > 1.0e-12f) ? (cov / denom) : 0.0f;

    corr[index] = fminf(1.0f, fmaxf(-1.0f, c));

    const float d2 = fmaxf(rowSumSq[i] + rowSumSq[j] - 2.0f * d, 0.0f);
    dist[index] = sqrtf(d2);
}
