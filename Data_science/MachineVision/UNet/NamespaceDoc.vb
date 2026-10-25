Namespace UNet

    ''' <summary>
    ''' U-Net 语义分割（U-Net: Convolutional Networks for Biomedical Image Segmentation）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本命名空间在 sciBASIC# 已有的 CNN 层（卷积 / 池化 / ReLU）之上补齐了 U-Net 需要的
    ''' 两块拼图（转置卷积与跳跃连接拼接层位于 <c>MachineLearning\DeepLearning\CNN</c>），
    ''' 并用一个显式计算图执行器把它们组织成编码器—解码器结构。
    ''' </para>
    ''' <para>
    ''' 混合精度：主权重、偏置、优化器状态、激活、拼接、损失与指标全部是 fp64（
    ''' <c>Tensor</c>/<c>Double</c>）；卷积与转置卷积的前向及三个反向作为算力热点，
    ''' 按规模判据临时降为 fp32（<c>TensorF</c>）算完再升回 fp64。
    ''' 后端（CPU SIMD 或 CUDA）由 <c>Tensor.computeKernel</c> / <c>TensorF.computeKernelF</c>
    ''' 统一派发，本库自身不含任何 CUDA 依赖。
    ''' </para>
    ''' </remarks>
    Public Module NamespaceDoc
    End Module

End Namespace
