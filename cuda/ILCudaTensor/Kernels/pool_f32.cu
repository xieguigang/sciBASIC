// ---------------------------------------------------------------------------
// pool_f32.cu —— 单精度二维最大池化（正向取 argMax + 反向按 argMax 散射）
//
// 语义与 Compute\TensorComputeFBase.vb 的 MaxPool2D / MaxPool2DBackward 对齐：
//   * 越界的窗口位置直接跳过（等价于 -inf），而不是按零填充参与比较 ——
//     否则在输入为负数时"填充出来的 0"会错误胜出；
//   * 整个窗口都落在填充区时输出 0、argMax 记 0（CPU 参考实现同处理）；
//   * argMax 记录的是**输入张量中的扁平下标**，反向据此回填。
//
// 与卷积不同，池化的反向必须用 atomicAdd：窗口重叠时同一个输入位置会被多个
// 输出位置选中（CPU 参考实现是串行 `dst[target] += g[i]`）。
// 这是本算子**唯一**一处 GPU 端无法复刻 CPU 求和次序的地方，
// 因此池化反差的差异会略大于其它算子 —— 属于浮点结合律的正常范围。
//
// 布局约定同样是 channel-last：(N, H, W, C)
// ---------------------------------------------------------------------------

#define TENSOR_F32_POOL_NEG_INF (-3.402823466e+38f)

// ---------------------------------------------------------------------------
// 1. 前向：一个线程一个输出位置，顺便把获胜输入下标写进 argMax
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorMaxPool2DFp32Kernel(const float* __restrict__ x,
                                                    float* __restrict__ y,
                                                    float* __restrict__ argMax,
                                                    int inH, int inW, int inC,
                                                    int outH, int outW,
                                                    int batch, int size,
                                                    int stride, int padding) {
    const long long total = (long long)batch * outH * outW * inC;

    for (long long idx = (long long)blockIdx.x * blockDim.x + threadIdx.x;
         idx < total;
         idx += (long long)gridDim.x * blockDim.x) {

        long long rest = idx;

        const int c  = (int)(rest % inC);  rest /= inC;
        const int ow = (int)(rest % outW); rest /= outW;
        const int oh = (int)(rest % outH); rest /= outH;
        const int n  = (int)rest;

        float best = TENSOR_F32_POOL_NEG_INF;
        long long bestIdx = -1;

        for (int kh = 0; kh < size; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= inH) continue;

            for (int kw = 0; kw < size; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= inW) continue;

                const long long flat = (((long long)n * inH + ih) * inW + iw) * inC + c;
                const float v = x[flat];

                if (v > best) {
                    best = v;
                    bestIdx = flat;
                }
            }
        }

        if (bestIdx < 0) {
            // 整个窗口都落在填充区（极小输入 + 大 padding 才会出现）
            y[idx] = 0.0f;
            argMax[idx] = 0.0f;
        } else {
            y[idx] = best;
            argMax[idx] = (float)bestIdx;
        }
    }
}

// ---------------------------------------------------------------------------
// 2. 反向：按 argMax 把上游梯度回填到输入位置
//
//    每个输出位置唯一对应一个输入位置，但反过来不成立（窗口重叠），
//    因此这里用 atomicAdd 累加。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorMaxPool2DBackwardFp32Kernel(const float* __restrict__ gradOutput,
                                                            const float* __restrict__ argMax,
                                                            float* __restrict__ dx,
                                                            int total, int inputLength) {
    const int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx >= total) return;

    const int target = (int)argMax[idx];
    if (target >= 0 && target < inputLength) {
        atomicAdd(&dx[target], gradOutput[idx]);
    }
}
