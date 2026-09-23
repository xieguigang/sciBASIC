// ---------------------------------------------------------------------------
// Cella 批量细胞管线融合内核（LNN 液态网络 + GNN 图卷积）
//
// 为什么需要它：
//   类器官仿真里每个细胞每步都要跑一次「液态代谢网络 RK4 积分」与「先验调控图
//   消息传递」。逐细胞调用时矩阵只有 1×122 / 339×34，规模低于 GPU 后端的
//   MinGemmElements 阈值，全部被判为"小算子"回退 CPU；即使强行上 GPU，
//   逐算子写法也会为每步产生几十次显存往返（每次几十微秒），把计算收益吃光。
//
//   把「同一步的全部细胞」看成一个 batch（batch = 存活细胞数，峰值数千），
//   每个子网络压成一到两次内核启动、并且状态在显存里原地更新之后：
//     * 单次算子的元素数从 1×122 涨到 B×122（百倍量级），越过回退阈值；
//     * 每次内核启动摊到全部细胞上，主机往返次数从"每细胞几十次"降到"每批几次"。
//
// 精度约定：
//   状态与权重全程 double（主机数组也是 double），没有精度转换，
//   与 CPU 参考实现的差异仅来自浮点结合律（GPU 端归约顺序不同）。
//
// 命名约定：符号一律带 tcCell 前缀，宏一律带 TCX_ 前缀。
//   —— 所有 .cu 会被 KernelSources.CombinedSource() 拼进同一个 NVRTC 编译单元，
//      函数名与宏名都不能与 tensor.cu / gemm.cu / spmm.cu / conv.cu / pool.cu /
//      train.cu / lif.cu / elementwise.cu 冲突。
// ---------------------------------------------------------------------------

// 激活函数编码：0 = 线性，1 = tanh，2 = sigmoid，3 = relu
#define TCX_ACT_LINEAR   0
#define TCX_ACT_TANH     1
#define TCX_ACT_SIGMOID  2
#define TCX_ACT_RELU     3

__device__ __forceinline__ double tcCellAct(double x, int code) {
    if (code == TCX_ACT_TANH)    return tanh(x);
    if (code == TCX_ACT_SIGMOID) return 1.0 / (1.0 + exp(-x));
    if (code == TCX_ACT_RELU)    return x > 0.0 ? x : 0.0;

    return x;
}

__device__ __forceinline__ double tcCellSigmoid(double x) {
    return 1.0 / (1.0 + exp(-x));
}

// ---------------------------------------------------------------------------
// LTC（液态时间常数网络）批量 RK4 积分：状态 [B, m] 就地推进 substeps 个子步
//
//   A     = act(h·Wrec + u·Win + b)
//   f     = σ(h·Wgate + u·WgateIn + bgate)        （无门控时 f ≡ 0）
//   dh/dt = (1/τ_eff + f) ⊙ (A − h)
//
// 启动：grid = (B, 1)，block = (m, 1)，动态共享内存 = 6·m·double
//   [sw(m) | sy(m) | sk(4m)] —— sy 为子步起始状态，sw 为各阶段输入，sk 为斜率。
// 线程 i 负责第 i 个单元：它需要整条状态向量，因此状态必须放在共享内存里，
// 每个阶段之间用 __syncthreads() 对齐（阶段斜率只被自己的线程消费，
// 但阶段输入是全体线程共享的，所以每次改写 sw 之前都要同步).
// ---------------------------------------------------------------------------
extern "C" __global__ void tcCellLtcRk4DoubleKernel(
    double* __restrict__ h,              // [B, m] 状态（就地更新）
    const double* __restrict__ u,        // [B, nIn] 输入（酶水平 ‖ 边界浓度）
    const double* __restrict__ wRec,     // [m, m]
    const double* __restrict__ wIn,      // [nIn, m]
    const double* __restrict__ bias,     // [m]（可为空）
    const double* __restrict__ wGate,    // [m, m]（可为空 → 无门控）
    const double* __restrict__ wGateIn,  // [nIn, m]（可为空）
    const double* __restrict__ biasGate, // [m]（可为空）
    const double* __restrict__ tauEff,   // [m] 有效时间常数
    int m,
    int nIn,
    double dt,
    int substeps,
    int hasGate,
    int actCode) {

    extern __shared__ double smem[];

    double* sw = smem;             // [m] 阶段输入
    double* sy = smem + m;         // [m] 子步起始状态
    double* sk = smem + 2 * m;     // [4m] RK4 斜率

    const int b = blockIdx.x;
    const int i = threadIdx.x;
    const long long row = (long long)b * m;
    const double* ub = u + (long long)b * nIn;
    const double invTau = (tauEff != 0) ? 1.0 / (tauEff[i]) : 0.0;
    const double base = (bias != 0) ? bias[i] : 0.0;
    const double baseGate = (hasGate && biasGate != 0) ? biasGate[i] : 0.0;

    const double step = dt / (double)substeps;
    const double half = 0.5 * step;

    sy[i] = h[row + i];
    sw[i] = sy[i];
    __syncthreads();

    for (int s = 0; s < substeps; ++s) {
        for (int stage = 0; stage < 4; ++stage) {
            double z = base;
            double zg = baseGate;

            for (int k = 0; k < m; ++k) {
                const double hv = sw[k];

                z += hv * wRec[(long long)k * m + i];

                if (hasGate) {
                    zg += hv * wGate[(long long)k * m + i];
                }
            }

            for (int j = 0; j < nIn; ++j) {
                const double uv = ub[j];

                z += uv * wIn[(long long)j * m + i];

                if (hasGate) {
                    zg += uv * wGateIn[(long long)j * m + i];
                }
            }

            const double a = tcCellAct(z, actCode);
            double decay = invTau;

            if (hasGate) {
                decay += tcCellSigmoid(zg);
            }

            sk[stage * m + i] = decay * (a - sw[i]);
            __syncthreads();

            // 组合下一阶段的输入：k1/k2 用半步、k3 用整步
            if (stage < 3) {
                const double scale = (stage == 2) ? step : half;

                sw[i] = sy[i] + scale * sk[stage * m + i];
                __syncthreads();
            }
        }

        // y += step/6 · (k1 + 2k2 + 2k3 + k4)
        sy[i] = sy[i] + (step / 6.0) * (sk[i] + 2.0 * sk[m + i] + 2.0 * sk[2 * m + i] + sk[3 * m + i]);
        __syncthreads();
        sw[i] = sy[i];
        __syncthreads();
    }

    h[row + i] = sy[i];
}

// ---------------------------------------------------------------------------
// 图卷积批量层（GEARS 消息传递 / 解码器复用同一内核）
//
//   out[row, j] = act( selfW[i] · Σ_k x[row, k]·wSelf[k, j] + bias[j]
//                      + Σ_e coeff[e] · Σ_k x[src_e, k]·wRel[k, j] )
//
//   row = b·n + i（批内第 b 个细胞的第 i 个节点），src_e 取自 CSR 的列索引，
//   因此同一个 CSR（拓扑与系数对全部细胞共享）可以服务整个 batch。
//   解码器等"无邻居"的层只需把 rowPtr 传空指针，退化为普通全连接层。
//
// 启动：grid = (B·n, 1)，block = (32, 1)（outF ≤ 32，用步长循环覆盖）
// ---------------------------------------------------------------------------
extern "C" __global__ void tcCellGraphLayerDoubleKernel(
    const double* __restrict__ x,       // [B·n, inF]
    const double* __restrict__ wSelf,   // [inF, outF]
    const double* __restrict__ wRel,    // [inF, outF]（可为空 → 忽略邻居项）
    const double* __restrict__ selfW,   // [n]（可为空 → 视为 1）
    const double* __restrict__ bias,    // [outF]（可为空 → 视为 0）
    const int* __restrict__ rowPtr,     // [n+1]（可为空 → 无邻居）
    const int* __restrict__ colIdx,     // [nnz]
    const double* __restrict__ coeff,   // [nnz]
    double* __restrict__ out,           // [B·n, outF]
    int n,
    int inF,
    int outF,
    int actCode) {

    const int row = blockIdx.x;
    const int i = row % n;
    const double* xi = x + (long long)row * inF;
    const double sw = (selfW != 0) ? selfW[i] : 1.0;
    const int e0 = (rowPtr != 0) ? rowPtr[i] : 0;
    const int e1 = (rowPtr != 0) ? rowPtr[i + 1] : 0;
    double* orow = out + (long long)row * outF;

    for (int j = threadIdx.x; j < outF; j += blockDim.x) {
        double acc = (bias != 0) ? bias[j] : 0.0;

        if (wSelf != 0) {
            double s = 0.0;

            for (int k = 0; k < inF; ++k) {
                s += xi[k] * wSelf[(long long)k * outF + j];
            }

            acc += sw * s;
        }

        if (rowPtr != 0 && wRel != 0) {
            for (int e = e0; e < e1; ++e) {
                const double c = coeff[e];

                if (c == 0.0) {
                    continue;
                }

                const double* xs = x + (long long)(row - i + colIdx[e]) * inF;
                double s = 0.0;

                for (int k = 0; k < inF; ++k) {
                    s += xs[k] * wRel[(long long)k * outF + j];
                }

                acc += c * s;
            }
        }

        orow[j] = tcCellAct(acc, actCode);
    }
}

// ---------------------------------------------------------------------------
// 通量读取头批量计算：v = e ⊙ gsat( [h ‖ u] · Wv + bv )
//
//   gsat = σ(·)        （不可逆反应，通量恒为正）
//   gsat = 2σ(·) − 1   （可逆反应，允许负通量表示逆向流动）
//   e    = u 的前 r 项 （逐反应的酶水平）
//   预激活先按 ±30 夹断，与 CPU 侧的 Clamp 一致（避免 exp 溢出）
//
// 启动：grid = (B, 1)，block = (64, 1)
// ---------------------------------------------------------------------------
extern "C" __global__ void tcCellFluxHeadDoubleKernel(
    const double* __restrict__ h,           // [B, m]
    const double* __restrict__ u,           // [B, nIn]（前 r 项为酶水平）
    const double* __restrict__ wFlux,       // [m+nIn, r]
    const double* __restrict__ bias,        // [r]（可为空）
    const double* __restrict__ reversible,  // [r]（可为空 → 全部按不可逆处理）
    double* __restrict__ out,               // [B, r]
    int m,
    int nIn,
    int r) {

    const int b = blockIdx.x;
    const double* hb = h + (long long)b * m;
    const double* ub = u + (long long)b * nIn;
    double* ob = out + (long long)b * r;

    for (int j = threadIdx.x; j < r; j += blockDim.x) {
        double z = (bias != 0) ? bias[j] : 0.0;

        for (int i = 0; i < m; ++i) {
            z += hb[i] * wFlux[(long long)i * r + j];
        }

        for (int i = 0; i < nIn; ++i) {
            z += ub[i] * wFlux[(long long)(m + i) * r + j];
        }

        if (z > 30.0) {
            z = 30.0;
        } else if (z < -30.0) {
            z = -30.0;
        }

        const double sat = 1.0 / (1.0 + exp(-z));
        const double e = ub[j];
        const int rev = (reversible != 0) ? (reversible[j] != 0.0 ? 1 : 0) : 0;

        ob[j] = e * (rev == 1 ? (2.0 * sat - 1.0) : sat);
    }
}

// ---------------------------------------------------------------------------
// 系统时间常数批量计算：τ^sys = 1 / (1/τ_eff + f)，
//   f = σ(h·Wgate + u·WgateIn + bgate)（无门控时 f ≡ 0，退化为 τ_eff）
//
// 启动：grid = (B, 1)，block = (64, 1)
// ---------------------------------------------------------------------------
extern "C" __global__ void tcCellSysTauDoubleKernel(
    const double* __restrict__ h,            // [B, m]
    const double* __restrict__ u,            // [B, nIn]
    const double* __restrict__ wGate,        // [m, m]（可为空）
    const double* __restrict__ wGateIn,      // [nIn, m]（可为空）
    const double* __restrict__ biasGate,     // [m]（可为空）
    const double* __restrict__ tauEff,       // [m]
    double* __restrict__ out,                // [B, m]
    int m,
    int nIn,
    int hasGate) {

    const int b = blockIdx.x;
    const double* hb = h + (long long)b * m;
    const double* ub = u + (long long)b * nIn;
    double* ob = out + (long long)b * m;

    for (int i = threadIdx.x; i < m; i += blockDim.x) {
        double decay = 1.0 / tauEff[i];

        if (hasGate) {
            double zg = (biasGate != 0) ? biasGate[i] : 0.0;

            for (int k = 0; k < m; ++k) {
                zg += hb[k] * wGate[(long long)k * m + i];
            }

            for (int j = 0; j < nIn; ++j) {
                zg += ub[j] * wGateIn[(long long)j * m + i];
            }

            decay += 1.0 / (1.0 + exp(-zg));
        }

        ob[i] = 1.0 / decay;
    }
}

// ---------------------------------------------------------------------------
// 节点特征拼装：[B·n, 2+2d] ← [x̄(b,i) ‖ p(b,i) ‖ e_i ‖ z_pert(b)]
//
// 扰动集合编码器（Deep Sets 均值池化）在主机侧已折算成一次 GEMM
// （z_pert = scale/count · Σ p·e），这里只做通道拼接。
//
// 启动：grid = (ceil(B·n·dims / 256), 1)，block = (256, 1)
// ---------------------------------------------------------------------------
extern "C" __global__ void tcCellGraphFeatureDoubleKernel(
    const double* __restrict__ xNorm,   // [rows]（= [B·n]，行优先）
    const double* __restrict__ flag,    // [rows]
    const double* __restrict__ embed,   // [n, d]
    const double* __restrict__ zPert,   // [B, d]
    double* __restrict__ out,           // [rows, 2+2d]
    int rows,
    int n,
    int d) {

    const int dims = 2 + 2 * d;
    const long long total = (long long)rows * dims;

    for (long long t = (long long)blockIdx.x * blockDim.x + threadIdx.x;
         t < total;
         t += (long long)gridDim.x * blockDim.x) {
        const int k = (int)(t % dims);
        const long long row = t / dims;
        const int i = (int)(row % n);
        const int b = (int)(row / n);

        double v;

        if (k == 0) {
            v = xNorm[row];
        } else if (k == 1) {
            v = flag[row];
        } else if (k < 2 + d) {
            v = embed[(long long)i * d + (k - 2)];
        } else {
            v = zPert[(long long)b * d + (k - 2 - d)];
        }

        out[t] = v;
    }
}
