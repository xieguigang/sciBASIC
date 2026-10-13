// ---------------------------------------------------------------------------
// spmm_f32.cu —— 单精度 CSR 稀疏 × 稠密乘法
//
// 语义与 Compute\TensorComputeFBase.vb 的 SpMM 对齐：
//     y[b, c] = Σ_r x[b, r] * W[r, c]    （W 以 CSR 存放：行 = 突触前 r）
// 也就是 y[b, :] += Σ_r x[b,r] · CSR 第 r 行的全部 (column, value)。
//
// 并行策略：**一个线程负责一个 (batch, 行) 对**。
// 选它而不是"一个线程负责一个输出元素"的理由：
//   * CSR 的第 r 行本身就是连续的一段 [rowPtr[r], rowPtr[r+1])，
//     一个线程走完这一段正好是合并访存；
//   * 脉冲输入的高度稀疏特性可以让整行直接跳过（x[b,r] == 0 时不进内层循环），
//     这与 CPU 参考实现里的 `If xv = 0.0 Then Continue For` 是同一件事。
//
// 代价是同一个输出列可能被多个线程同时命中，因此必须用 atomicAdd。
// 这也意味着 GPU 侧的求和次序与 CPU 侧不同 —— 差异落在浮点结合律的量级内；
// 由于 y 是 float 而累加在 CPU 侧是 double，这里的中间累加同样用 double 寄存器，
// 只在写回时用 atomicAdd(float*)。
//
// 启动：grid-stride，block = (256, 1)，总任务数 = batch * rows
// ---------------------------------------------------------------------------

extern "C" __global__ void tensorCsrSpmmFp32Kernel(const float* __restrict__ x,
                                                  const int* __restrict__ rowPointers,
                                                  const int* __restrict__ columnIndices,
                                                  const float* __restrict__ values,
                                                  float* __restrict__ y,
                                                  int batch, int rows, int cols) {
    const long long total = (long long)batch * rows;

    for (long long task = (long long)blockIdx.x * blockDim.x + threadIdx.x;
         task < total;
         task += (long long)gridDim.x * blockDim.x) {

        const int b = (int)(task / rows);
        const int r = (int)(task - (long long)b * rows);

        const double xv = (double)x[(long long)b * rows + r];

        // 与 CPU 参考实现一致：零源直接跳过整行
        if (xv == 0.0) continue;

        const int kBegin = rowPointers[r];
        const int kEnd = rowPointers[r + 1];
        const long long yBase = (long long)b * cols;

        for (int k = kBegin; k < kEnd; ++k) {
            atomicAdd(&y[yBase + columnIndices[k]], (float)(xv * (double)values[k]));
        }
    }
}
