// ---------------------------------------------------------------------------
// 基础线性代数内核（行主序 row-major）
//
// gemmKernel: C(m x n) = A(m x k) * B(k x n)
//             用 16x16 分块 + 共享内存，越界补 0，因此可以处理任意尺寸。
//             启动形状：grid = (ceil(n/16), ceil(m/16)), block = (16, 16)
//
// gemvKernel: y(m) = A(m x k) * x(k)
//             每个 block 负责一行，用共享内存树形归约完成点积。
//             启动形状：grid = (m, 1), block = (blockDim.x, 1, 1)
//             需要的动态共享内存：blockDim.x * sizeof(float)
// ---------------------------------------------------------------------------

#define GEMM_TILE 16

extern "C" __global__ void gemmKernel(const float* __restrict__ A,
                                      const float* __restrict__ B,
                                      float* __restrict__ C,
                                      int m, int n, int k) {
    __shared__ float aTile[GEMM_TILE][GEMM_TILE];
    __shared__ float bTile[GEMM_TILE][GEMM_TILE];

    // blockIdx.x -> 列方向（j），blockIdx.y -> 行方向（i）
    const int row = blockIdx.y * GEMM_TILE + threadIdx.y;
    const int col = blockIdx.x * GEMM_TILE + threadIdx.x;

    float acc = 0.0f;
    const int nTiles = (k + GEMM_TILE - 1) / GEMM_TILE;

    for (int t = 0; t < nTiles; ++t) {
        const int pa = t * GEMM_TILE + threadIdx.x;   // A 的列
        const int pb = t * GEMM_TILE + threadIdx.y;   // B 的行

        aTile[threadIdx.y][threadIdx.x] =
            (row < m && pa < k) ? A[(size_t)row * (size_t)k + (size_t)pa] : 0.0f;
        bTile[threadIdx.y][threadIdx.x] =
            (pb < k && col < n) ? B[(size_t)pb * (size_t)n + (size_t)col] : 0.0f;

        __syncthreads();

#pragma unroll
        for (int q = 0; q < GEMM_TILE; ++q) {
            acc += aTile[threadIdx.y][q] * bTile[q][threadIdx.x];
        }

        __syncthreads();
    }

    if (row < m && col < n) {
        C[(size_t)row * (size_t)n + (size_t)col] = acc;
    }
}

extern "C" __global__ void gemvKernel(const float* __restrict__ A,
                                      const float* __restrict__ x,
                                      float* __restrict__ y,
                                      int m, int k) {
    extern __shared__ float cache[];

    const int row = blockIdx.x;
    const unsigned int tid = threadIdx.x;

    if (row >= m) {
        return;
    }

    const float* rowPtr = A + (size_t)row * (size_t)k;

    float acc = 0.0f;
    for (int c = (int)tid; c < k; c += (int)blockDim.x) {
        acc += rowPtr[c] * x[c];
    }

    cache[tid] = acc;
    __syncthreads();

    for (unsigned int s = blockDim.x >> 1; s > 0; s >>= 1) {
        if (tid < s) {
            cache[tid] += cache[tid + s];
        }
        __syncthreads();
    }

    if (tid == 0) {
        y[row] = cache[0];
    }
}
