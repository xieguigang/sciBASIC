// ---------------------------------------------------------------------------
// 手写双精度矩阵乘内核（行主序）
//
//   C(m x n) = A(m x k) * B(k x n)
//
// 与框架自带的 blas.cu（float 版）结构一致：16x16 分块 + 静态共享内存，
// 越界位置补 0，因此任意尺寸都能直接算。
//
// 之所以不用 IL2Cuda 自动生成：分块 GEMM 依赖共享内存与 __syncthreads 的
// block 内协作，这些语义无法用“纯标量反编译”表达。
//
// 启动形状：grid = (ceil(n/16), ceil(m/16))，block = (16, 16)，动态共享内存 = 0。
// ---------------------------------------------------------------------------

#define GEMM_D_TILE 16

extern "C" __global__ void tensorGemmDoubleKernel(const double* __restrict__ A,
                                                  const double* __restrict__ B,
                                                  double* __restrict__ C,
                                                  int m, int n, int k) {
    __shared__ double aTile[GEMM_D_TILE][GEMM_D_TILE];
    __shared__ double bTile[GEMM_D_TILE][GEMM_D_TILE];

    // blockIdx.x -> 列方向（j），blockIdx.y -> 行方向（i）
    const int row = blockIdx.y * GEMM_D_TILE + threadIdx.y;
    const int col = blockIdx.x * GEMM_D_TILE + threadIdx.x;

    double acc = 0.0;
    const int nTiles = (k + GEMM_D_TILE - 1) / GEMM_D_TILE;

    for (int t = 0; t < nTiles; ++t) {
        const int pa = t * GEMM_D_TILE + threadIdx.x;   // A 的列
        const int pb = t * GEMM_D_TILE + threadIdx.y;   // B 的行

        aTile[threadIdx.y][threadIdx.x] =
            (row < m && pa < k) ? A[(size_t)row * (size_t)k + (size_t)pa] : 0.0;
        bTile[threadIdx.y][threadIdx.x] =
            (pb < k && col < n) ? B[(size_t)pb * (size_t)n + (size_t)col] : 0.0;

        __syncthreads();

#pragma unroll
        for (int q = 0; q < GEMM_D_TILE; ++q) {
            acc += aTile[threadIdx.y][q] * bTile[q][threadIdx.x];
        }

        __syncthreads();
    }

    if (row < m && col < n) {
        C[(size_t)row * (size_t)n + (size_t)col] = acc;
    }
}
