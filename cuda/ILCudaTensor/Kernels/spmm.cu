// ---------------------------------------------------------------------------
// 手写 CSR 稀疏 × 稠密内核（double 精度）
//
// 计算：out[batch x Columns] = dense[batch x Rows] · W[Rows x Columns]
// 其中 W 以 CSR（行 = 突触前，列 = 突触后）存储：
//   rowPtr  长度 Rows+1；第 r 行的非零列位于 colIdx[rowPtr[r] .. rowPtr[r+1])
//   colIdx / values 等长 = nnz
//
// 并行划分：一个线程负责 (batch, row) 中的一个输入行 —— 读出 dense 的该元素，
// 按该行的非零列散布累加到输出。不同输入行可能命中同一输出列，因此对输出使用
// double 的 atomicAdd（需要 sm_60 及以上，与 conv.cu / pool.cu 的做法一致）。
// 调用方在启动前必须把输出缓冲清零（Fill(0)）。
//
// 启动形状：LaunchPlanner.For1D(batch * rows, 256)（block 数会被限制在 MaxBlocks1D，
//         因此这里使用 grid-stride 循环覆盖全部元素）。
//
// 稀疏性优化：脉冲输入高度稀疏（绝大多数元素为 0），对 xv == 0 直接跳过，
//             等价于 CSR-SpMV 的快速路径。
//
// 用途：脉冲神经网络加载真实突触连接组（如 FlyWire，十万级神经元 / 千万级突触）
//       时的核心算子 —— 稠密矩阵在该规模下完全不可行。
// ---------------------------------------------------------------------------

extern "C" __global__ void tensorSpmmCsrKernel(const int* __restrict__ rowPtr,
                                               const int* __restrict__ colIdx,
                                               const double* __restrict__ values,
                                               const double* __restrict__ dense,
                                               double* __restrict__ out,
                                               int rows, int columns, int batch) {
    const long long total = (long long)rows * (long long)batch;
    const long long stride = (long long)gridDim.x * blockDim.x;

    for (long long idx = (long long)blockIdx.x * blockDim.x + threadIdx.x;
         idx < total;
         idx += stride) {

        // dense 为 [batch, rows] 行主序：第 idx 个元素即第 (b, r) 个
        const int b = (int)(idx / rows);
        const int r = (int)(idx - (long long)b * rows);

        const double xv = dense[idx];
        if (xv == 0.0) continue;                       // 脉冲稀疏：跳过零源

        const int start = rowPtr[r];
        const int end = rowPtr[r + 1];
        double* rowOut = out + (size_t)b * (size_t)columns;

        for (int k = start; k < end; ++k) {
            atomicAdd(&rowOut[colIdx[k]], xv * values[k]);
        }
    }
}
