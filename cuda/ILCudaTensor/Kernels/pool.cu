// ---------------------------------------------------------------------------
// 手写最大池化内核（double 精度，channel-last / NHWC 布局）
//
// 布局约定（与 ITensorCompute 的声明、以及 TensorComputeBase 的 CPU 参考实现一致）:
//     输入   x       : (N, H, W, C)
//     输出   y       : (N, OH, OW, C)
//     argMax         : (N, OH, OW, C)，每个元素是获胜元素在**输入张量**之中的
//                      扁平下标 n*H*W*C + h*W*C + w*C + c
//
//     OH = (H + 2*padding - size) / stride + 1
//     OW = (W + 2*padding - size) / stride + 1
//
// 越界的窗口位置直接跳过（等价于 -inf），这样零填充不会在负数输入时错误地胜出；
// 这与 TensorComputeBase 的 CPU 参考实现完全一致。
//
// 前向内核一次同时产出 values 与 argMax：反向只需要按 argMax 做 scatter-add，
// 不需要重新计算窗口，也不需要保存中间张量。
//
// 所有符号统一带 tensorMaxPool2D 前缀，避免与 tensor.cu / gemm.cu 以及框架
// 自带内核撞名（所有 .cu 会被合并进同一个 NVRTC 编译单元）。
// ---------------------------------------------------------------------------

#define TENSOR_POOL_BLOCK 256
#define TENSOR_POOL_NEG_BIG (-1.0e308)

// ---------------------------------------------------------------------------
// 前向：同时输出 values 与 argMax（输入扁平下标）
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorMaxPool2DForwardKernel(const double* __restrict__ x,
                                                        double* __restrict__ out,
                                                        int* __restrict__ argMax,
                                                        int N, int H, int W, int C,
                                                        int size, int OH, int OW,
                                                        int stride, int padding) {
    const long long total = (long long)N * OH * OW * C;
    const long long step = (long long)blockDim.x * gridDim.x;
    const long long start = (long long)blockIdx.x * blockDim.x + threadIdx.x;

    for (long long idx = start; idx < total; idx += step) {
        const int c = (int)(idx % C);
        long long t = idx / C;
        const int ow = (int)(t % OW); t /= OW;
        const int oh = (int)(t % OH);
        const int n = (int)(t / OH);

        double best = TENSOR_POOL_NEG_BIG;
        int bestIdx = 0;

        for (int kh = 0; kh < size; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= H) continue;

            for (int kw = 0; kw < size; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= W) continue;

                const long long flat = (((long long)n * H + ih) * W + iw) * C + c;
                const double v = x[flat];

                if (v > best) {
                    best = v;
                    bestIdx = (int)flat;
                }
            }
        }

        out[idx] = (best == TENSOR_POOL_NEG_BIG) ? 0.0 : best;
        argMax[idx] = bestIdx;
    }
}

// ---------------------------------------------------------------------------
// 反向：把梯度散射回输入位置（输出缓冲必须已清零）
//   gradIn[argMax[i]] += gradOut[i]
// 窗口重叠时同一个输入位置可能被多个输出位置选中，因此使用 atomicAdd。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorMaxPool2DBackwardKernel(const double* __restrict__ gradOut,
                                                         const int* __restrict__ argMax,
                                                         double* __restrict__ gradIn,
                                                         long long total) {
    const long long step = (long long)blockDim.x * gridDim.x;
    const long long start = (long long)blockIdx.x * blockDim.x + threadIdx.x;

    for (long long i = start; i < total; i += step) {
        atomicAdd(&gradIn[argMax[i]], gradOut[i]);
    }
}
