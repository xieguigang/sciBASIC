// ---------------------------------------------------------------------------
// conv_f32.cu —— 单精度二维卷积的正向与三个反向
//
// 与双精度栈 conv.cu 的关系：语义**逐行对齐** Compute\TensorComputeFBase.vb
// 里新加的标量参考实现，差别只有两处：
//   * 数据落到 float；
//   * 累加一律在 double 里进行，末尾只舍入一次 —— 这与 CPU 参考实现的口径一致，
//     保证 CPU / GPU 对拍时差异只有 Tensile LP 量级。
//
// 布局约定（channel-last，与主机侧严格一致）:
//     输入   x       : (N, H, W, C)
//     卷积核 filters  : (KH, KW, C, OutC)
//     偏置   bias    : (OutC)
//     输出   y       : (N, OH, OW, OutC)
//
// 实现取向：**一个线程负责一个输出元素**（而不是 im2col + GEMM）。
// im2col 的访存更规整但需要额外开辟 [N*OH*OW, KH*KW*C] 的巨型中间缓冲，
// 在 KEN batch 下这个中间体往往比输入还大；而每个输出元素的累加长度是
// KH*KW*C，并行度足够把带宽吃满。先保证正确、再谈 GEMM 化。
//
// 遍历顺序刻意保持与 CPU 参考实现完全一致（kh -> kw -> c），
// 这样"不同的并行归约次序"不会引入超出舍入范围的偏差。
//
// 启动：grid-stride，block = (256, 1)
// ---------------------------------------------------------------------------

// 统一的 grid-stride 宏：所有内核都是"一个线程一个元素"，只是元素总数不同
#define CONV_F32_STRIDE(TOTAL)                                              \
    for (long long idx = (long long)blockIdx.x * blockDim.x + threadIdx.x;  \
         idx < (TOTAL);                                                     \
         idx += (long long)gridDim.x * blockDim.x)

// ---------------------------------------------------------------------------
// 1. 前向
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DFp32Kernel(const float* __restrict__ x,
                                                 const float* __restrict__ filters,
                                                 const float* __restrict__ bias,
                                                 float* __restrict__ y,
                                                 int batch, int inH, int inW, int inC,
                                                 int filtH, int filtW, int outC,
                                                 int outH, int outW,
                                                 int stride, int padding, int hasBias) {
    const long long total = (long long)batch * outH * outW * outC;

    CONV_F32_STRIDE(total) {
        long long rest = idx;

        const int oc = (int)(rest % outC); rest /= outC;
        const int ow = (int)(rest % outW); rest /= outW;
        const int oh = (int)(rest % outH); rest /= outH;
        const int n  = (int)rest;

        double sum = 0.0;
        if (hasBias != 0 && bias != 0) sum = (double)bias[oc];

        for (int kh = 0; kh < filtH; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= inH) continue;

            for (int kw = 0; kw < filtW; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= inW) continue;

                const long long xBase = (((long long)n * inH + ih) * inW + iw) * inC;
                const long long kBase = (((long long)kh * filtW + kw) * inC) * outC + oc;

                for (int c = 0; c < inC; ++c) {
                    sum += (double)x[xBase + c] * (double)filters[kBase + (long long)c * outC];
                }
            }
        }

        y[idx] = (float)sum;
    }
}

// ---------------------------------------------------------------------------
// 2. 反向：对输入的梯度
//    与 CPU 参考实现的循环嵌套一致 —— 遍历输入位置，反向找哪些卷积核元素踩到它。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardInputFp32Kernel(const float* __restrict__ gradOutput,
                                                              const float* __restrict__ filters,
                                                              float* __restrict__ dx,
                                                              int batch, int inH, int inW, int inC,
                                                              int filtH, int filtW, int outC,
                                                              int outH, int outW,
                                                              int stride, int padding) {
    const long long total = (long long)batch * inH * inW * inC;

    CONV_F32_STRIDE(total) {
        long long rest = idx;

        const int ic = (int)(rest % inC); rest /= inC;
        const int iw = (int)(rest % inW); rest /= inW;
        const int ih = (int)(rest % inH); rest /= inH;
        const int n  = (int)rest;

        double sum = 0.0;

        for (int oh = 0; oh < outH; ++oh) {
            const int kh = ih - oh * stride + padding;
            if (kh < 0 || kh >= filtH) continue;

            for (int ow = 0; ow < outW; ++ow) {
                const int kw = iw - ow * stride + padding;
                if (kw < 0 || kw >= filtW) continue;

                const long long gBase = (((long long)n * outH + oh) * outW + ow) * outC;

                for (int oc = 0; oc < outC; ++oc) {
                    sum += (double)gradOutput[gBase + oc] *
                           (double)filters[(((long long)kh * filtW + kw) * inC + ic) * outC + oc];
                }
            }
        }

        dx[idx] = (float)sum;
    }
}

// ---------------------------------------------------------------------------
// 3. 反向：对卷积核的梯度
//    一个线程一个卷积核元素，遍历所有 batch / 输出位置做累加。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardFilterFp32Kernel(const float* __restrict__ gradOutput,
                                                               const float* __restrict__ x,
                                                               float* __restrict__ dw,
                                                               int batch, int inH, int inW, int inC,
                                                               int filtH, int filtW, int outC,
                                                               int outH, int outW,
                                                               int stride, int padding) {
    const long long total = (long long)filtH * filtW * inC * outC;

    CONV_F32_STRIDE(total) {
        long long rest = idx;

        const int oc = (int)(rest % outC); rest /= outC;
        const int ic = (int)(rest % inC);  rest /= inC;
        const int kw = (int)(rest % filtW); rest /= filtW;
        const int kh = (int)rest;

        double sum = 0.0;

        for (int n = 0; n < batch; ++n) {
            for (int oh = 0; oh < outH; ++oh) {
                const int ih = oh * stride + kh - padding;
                if (ih < 0 || ih >= inH) continue;

                for (int ow = 0; ow < outW; ++ow) {
                    const int iw = ow * stride + kw - padding;
                    if (iw < 0 || iw >= inW) continue;

                    sum += (double)gradOutput[(((long long)n * outH + oh) * outW + ow) * outC + oc] *
                           (double)x[(((long long)n * inH + ih) * inW + iw) * inC + ic];
                }
            }
        }

        dw[idx] = (float)sum;
    }
}

// ---------------------------------------------------------------------------
// 4. 反向：对偏置的梯度（沿 N / OH / OW 求和）
//
//    与前向/另两个反向不同，这里**一个线程负责一个输出通道**并在线程内串行累加，
//    而不是用 atomicAdd：
//      * outC 通常只有几十到几千，并行度已经足够；
//      * 串行累加的次序与 CPU 参考实现一致，结果可逐位对照，不需要放宽容差。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardBiasFp32Kernel(const float* __restrict__ gradOutput,
                                                             float* __restrict__ db,
                                                             int outC, long long perChannel) {
    const int oc = blockIdx.x * blockDim.x + threadIdx.x;
    if (oc >= outC) return;

    double sum = 0.0;

    for (long long i = 0; i < perChannel; ++i) {
        sum += (double)gradOutput[(long long)oc + i * outC];
    }

    db[oc] = (float)sum;
}
