// ---------------------------------------------------------------------------
// 融合的稀疏递归 LIF 单步内核（脉冲神经网络）
//
// 为什么需要它：
//   SparseLIFLayer 的逐算子写法是
//       I = W · S_prev + I_ext;  U = β·H + I;  S = Θ(U − U_thr);  H = 复位(U, S)
//   在十万级神经元规模下，这套写法每步要分配 6 个 [1, N] 中间张量（每个约 1.1 MB），
//   并且在 GPU 后端下每一跳都是一次显存上传 + 一次回读。跨步复用的状态只有 [1, N]，
//   而 PCIe 往返开销与内核计算时间同阶，于是"上 GPU"反而更慢。
//
//   把「稀疏输入 + 泄漏积分 + 阈值触发 + 复位 + 计数累加」压成一次调用之后，
//   状态张量可以一直留在显存（见 DeviceResidentStore / DeviceResidentStore64），
//   每步只剩两次内核启动、零次主机往返。
//
// 与 spmm.cu 的分工（刻意不把 SpMM 也融进本文件）：
//   tensorSpmmCsrKernel 是按 (batch, row) 行并行 + atomicAdd 的散射内核，按行写多个输出列；
//   本文件的内核是逐元素内核，每个线程只碰一个神经元。两者放在同一次调用里串行启动，
//   中间结果写进调用方复用的 I 缓冲 —— 这样既不用重写已经被验证过的稀疏内核，
//   也不需要一个"先散射再逐元素"的跨线程同步（block 之间无法同步）。
//
// 精度约定：
//   * Double 版本：整条链路（I / H / S / counts）都是双精度，与 CPU 实现逐位一致；
//   * Fp32 版本：H 与 counts 用单精度（省显存、省带宽），但 S 仍写双精度 —— 脉冲是
//     0/1，单精度下精确可表示，这样下一步的 tensorSpmmCsrKernel（dense 为 double）
//     可以原样复用；
//   * 复位减法用的 thrSub、衰减用的 beta 都是"主机侧已按单精度舍入"的值，
//     与 Tensor * CSng(x) 的语义保持一致。
//
// 命名约定：符号一律带 Lif 前缀，宏一律带 TLF_ 前缀。
//   —— 所有 .cu 会被 KernelSources.CombinedSource() 拼进**同一个 NVRTC 编译单元**，
//      因此函数名与宏名都不能与 tensor.cu / gemm.cu / spmm.cu / conv.cu / pool.cu /
//      train.cu / elementwise.cu 冲突（参考 train.cu 的 TAW_ / TCE_ 约定）。
// ---------------------------------------------------------------------------

#define TLF_STRIDE_LOOP for (int idx = blockDim.x * blockIdx.x + threadIdx.x; idx < n; idx += blockDim.x * gridDim.x)

// ---------------------------------------------------------------------------
// 双精度融合 LIF（状态全部为 double，与 CPU 标量实现逐位一致）
//
//   pre = I[idx] + I_ext[idx]              （I_ext 可为空指针 → 视为 0）
//   U   = pre + β·H[idx]
//   S   = (U − U_thr > 0) ? 1 : 0
//   H   = subtract ? U − S·U_thr : U·(1 − S)
//   counts += S
//
// 启动：grid = (min(ceil(n/256), 4096), 1)，block = (256, 1)
// 调用方责任：I 必须已由 tensorSpmmCsrKernel 填好（且已清零后再散射）；
//             counts 必须在整个仿真窗开始时清零。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorLifUpdateDoubleKernel(const double* __restrict__ i,
                                                       const double* __restrict__ ext,
                                                       double* __restrict__ h,
                                                       double* __restrict__ s,
                                                       double* __restrict__ counts,
                                                       int n,
                                                       double beta,
                                                       double threshold,
                                                       double thrSub,
                                                       int subtract) {
    TLF_STRIDE_LOOP {
        const double pre = i[idx] + (ext ? ext[idx] : 0.0);
        const double u = pre + h[idx] * beta;
        const double sp = (u - threshold > 0.0) ? 1.0 : 0.0;

        s[idx] = sp;
        h[idx] = subtract ? (u - sp * thrSub) : (u * (1.0 - sp));
        counts[idx] += sp;
    }
}

// ---------------------------------------------------------------------------
// 单精度状态融合 LIF（H / counts / I_ext 为 float，S 仍为 double）
//
// 适用场景：显存或带宽受限、且能接受膜电位单精度舍入的仿真（"最快档"）。
// 注意 S 保持双精度不是随手为之：它是下一步稀疏乘法的 dense 输入，
// 保持 double 才能复用 tensorSpmmCsrKernel，而 0/1 在两种精度下都是精确值。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorLifUpdateFp32Kernel(const double* __restrict__ i,
                                                     const float* __restrict__ ext,
                                                     float* __restrict__ h,
                                                     double* __restrict__ s,
                                                     float* __restrict__ counts,
                                                     int n,
                                                     float beta,
                                                     float threshold,
                                                     float thrSub,
                                                     int subtract) {
    TLF_STRIDE_LOOP {
        const float pre = (float)i[idx] + (ext ? ext[idx] : 0.0f);
        const float u = pre + h[idx] * beta;
        const float sp = (u - threshold > 0.0f) ? 1.0f : 0.0f;

        s[idx] = (double)sp;
        h[idx] = subtract ? (u - sp * thrSub) : (u * (1.0f - sp));
        counts[idx] += sp;
    }
}
