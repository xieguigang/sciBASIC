// ---------------------------------------------------------------------------
// 手写卷积内核（double 精度，channel-last / NHWC 布局）
//
// 布局约定（与 ITensorCompute 的声明、以及 TensorComputeBase 的 CPU 参考实现一致）:
//     输入   x          : (N, H, W, C)
//     卷积核 filters    : (KH, KW, C, OutC)
//     偏置   bias       : (OutC)（可为 0，表示不加偏置）
//     输出   y          : (N, OH, OW, OutC)
//
//     OH = (H + 2*padding - KH) / stride + 1
//     OW = (W + 2*padding - KW) / stride + 1
//
// 选择 channel-last 是为了与 CNN 既有的 DataBlock（索引 (y*SX + x)*Depth + c）
// 完全同构，迁移时不需要任何转置/重排。
//
// 线程组织：一个线程负责一个输出元素（grid-stride），实现简单、语义与 CPU 参考
// 实现逐项对应，便于做 CPU-vs-GPU 数值一致性断言。
//
// 反向传播的 gradInput / gradFilter 存在"多个输出位置累加到同一个输入位置"的
// 情况（重叠窗口），因此使用 double 的 atomicAdd（需要 sm_60 及以上，
// 本框架的目标架构 sm_86 满足）。调用方必须先把输出缓冲清零。
//
// 所有符号统一带 tensorConv2D 前缀，避免与 tensor.cu / gemm.cu 以及框架
// 自带内核撞名（所有 .cu 会被合并进同一个 NVRTC 编译单元）。
// ---------------------------------------------------------------------------

#define TENSOR_CONV_BLOCK 256

// ---------------------------------------------------------------------------
// 前向：y[n,oh,ow,oc] = bias[oc] + Σ_{kh,kw,c} x[n,oh*stride+kh-padding, ow*stride+kw-padding, c] * filters[kh,kw,c,oc]
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DForwardKernel(const double* __restrict__ x,
                                                     const double* __restrict__ filters,
                                                     const double* __restrict__ bias,
                                                     double* __restrict__ out,
                                                     int N, int H, int W, int C,
                                                     int KH, int KW, int OutC,
                                                     int OH, int OW,
                                                     int stride, int padding) {
    const long long total = (long long)N * OH * OW * OutC;
    const long long step = (long long)blockDim.x * gridDim.x;
    const long long start = (long long)blockIdx.x * blockDim.x + threadIdx.x;

    for (long long idx = start; idx < total; idx += step) {
        int oc = (int)(idx % OutC);
        long long t = idx / OutC;
        int ow = (int)(t % OW); t /= OW;
        int oh = (int)(t % OH);
        int n = (int)(t / OH);

        double sum = (bias != 0) ? bias[oc] : 0.0;

        for (int kh = 0; kh < KH; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= H) continue;

            for (int kw = 0; kw < KW; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= W) continue;

                const double* xp = x + (((long long)n * H + ih) * W + iw) * C;
                const double* fp = filters + ((long long)(kh * KW + kw) * C) * OutC + oc;

                for (int c = 0; c < C; ++c) {
                    sum += xp[c] * fp[(long long)c * OutC];
                }
            }
        }

        out[idx] = sum;
    }
}

// ---------------------------------------------------------------------------
// 反向：对输入的梯度（输出缓冲必须已清零）
//   gradIn[n,h,w,c] += gradOut[n,oh,ow,oc] * filters[kh,kw,c,oc]
//   其中 h = oh*stride + kh - padding, w = ow*stride + kw - padding
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardInputKernel(const double* __restrict__ gradOut,
                                                           const double* __restrict__ filters,
                                                           double* __restrict__ gradIn,
                                                           int N, int H, int W, int C,
                                                           int KH, int KW, int OutC,
                                                           int OH, int OW,
                                                           int stride, int padding) {
    const long long total = (long long)N * OH * OW * OutC;
    const long long step = (long long)blockDim.x * gridDim.x;
    const long long start = (long long)blockIdx.x * blockDim.x + threadIdx.x;

    for (long long idx = start; idx < total; idx += step) {
        const double gv = gradOut[idx];

        if (gv == 0.0) continue;

        int oc = (int)(idx % OutC);
        long long t = idx / OutC;
        int ow = (int)(t % OW); t /= OW;
        int oh = (int)(t % OH);
        int n = (int)(t / OH);

        for (int kh = 0; kh < KH; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= H) continue;

            for (int kw = 0; kw < KW; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= W) continue;

                double* gp = gradIn + (((long long)n * H + ih) * W + iw) * C;
                const double* fp = filters + ((long long)(kh * KW + kw) * C) * OutC + oc;

                for (int c = 0; c < C; ++c) {
                    atomicAdd(&gp[c], gv * fp[(long long)c * OutC]);
                }
            }
        }
    }
}

// ---------------------------------------------------------------------------
// 反向：对卷积核的梯度（输出缓冲必须已清零）
//   gradFilters[kh,kw,c,oc] += gradOut[n,oh,ow,oc] * x[n,oh*stride+kh-padding, ow*stride+kw-padding, c]
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardFilterKernel(const double* __restrict__ gradOut,
                                                            const double* __restrict__ x,
                                                            double* __restrict__ gradFilters,
                                                            int N, int H, int W, int C,
                                                            int KH, int KW, int OutC,
                                                            int OH, int OW,
                                                            int stride, int padding) {
    const long long total = (long long)N * OH * OW * OutC;
    const long long step = (long long)blockDim.x * gridDim.x;
    const long long start = (long long)blockIdx.x * blockDim.x + threadIdx.x;

    for (long long idx = start; idx < total; idx += step) {
        const double gv = gradOut[idx];

        if (gv == 0.0) continue;

        int oc = (int)(idx % OutC);
        long long t = idx / OutC;
        int ow = (int)(t % OW); t /= OW;
        int oh = (int)(t % OH);
        int n = (int)(t / OH);

        for (int kh = 0; kh < KH; ++kh) {
            const int ih = oh * stride + kh - padding;
            if (ih < 0 || ih >= H) continue;

            for (int kw = 0; kw < KW; ++kw) {
                const int iw = ow * stride + kw - padding;
                if (iw < 0 || iw >= W) continue;

                const double* xp = x + (((long long)n * H + ih) * W + iw) * C;
                double* fp = gradFilters + ((long long)(kh * KW + kw) * C) * OutC + oc;

                for (int c = 0; c < C; ++c) {
                    atomicAdd(&fp[(long long)c * OutC], gv * xp[c]);
                }
            }
        }
    }
}

// ---------------------------------------------------------------------------
// 反向：对偏置的梯度
//   gradBias[oc] = Σ_{n,oh,ow} gradOut[n,oh,ow,oc]
// 一个线程负责一个输出通道，因此不需要原子操作。
// ---------------------------------------------------------------------------
extern "C" __global__ void tensorConv2DBackwardBiasKernel(const double* __restrict__ gradOut,
                                                          double* __restrict__ gradBias,
                                                          int N, int OH, int OW, int OutC) {
    const int oc = blockIdx.x * blockDim.x + threadIdx.x;

    if (oc >= OutC) return;

    double sum = 0.0;

    for (int n = 0; n < N; ++n) {
        for (int oh = 0; oh < OH; ++oh) {
            for (int ow = 0; ow < OW; ++ow) {
                sum += gradOut[(((long long)n * OH + oh) * OW + ow) * OutC + oc];
            }
        }
    }

    gradBias[oc] = sum;
}
