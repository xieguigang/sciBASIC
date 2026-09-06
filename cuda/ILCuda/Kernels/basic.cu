// ---------------------------------------------------------------------------
// 基础数学加速内核
//
// 这些源码作为内嵌资源随程序分发，运行时由 NVRTC 即时编译成 PTX，
// 再通过 cuModuleLoadData 载入显卡执行；项目本身不依赖任何 NuGet 程序包。
// ---------------------------------------------------------------------------

extern "C" __global__ void vecAddKernel(const float* a, const float* b, float* c, int n) {
    int i = blockDim.x * blockIdx.x + threadIdx.x;

    if (i < n) {
        c[i] = a[i] + b[i];
    }
}

// out = alpha * x + y
extern "C" __global__ void saxpyKernel(float alpha, const float* x, const float* y, float* out, int n) {
    int i = blockDim.x * blockIdx.x + threadIdx.x;

    if (i < n) {
        out[i] = alpha * x[i] + y[i];
    }
}
