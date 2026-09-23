Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 批量细胞管线融合算子使用的激活函数编码。
    ''' </summary>
    ''' <remarks>
    ''' 融合算子（<see cref="ITensorCompute.LtcRk4Batch"/> / <see cref="ITensorCompute.GraphLayerBatch"/>）
    ''' 的签名一律使用 <c>Integer</c> 编码而不是委托或字符串：
    ''' 委托无法跨越 GPU 内核边界，字符串比较会进入每个线程的内层循环。
    ''' 编码取值与 <c>Kernels\cellaccel.cu</c> 中的 <c>TCX_ACT_*</c> 宏一一对应，
    ''' 修改任何一侧都必须同步另一侧。
    ''' </remarks>
    Public Module CellActivation

        ''' <summary>恒等（线性输出，用于回归头）</summary>
        Public Const Linear As Integer = 0
        ''' <summary>双曲正切（液态网络与图卷积层的默认激活）</summary>
        Public Const Tanh As Integer = 1
        ''' <summary>逻辑斯蒂 S 形（门控与通量读取头）</summary>
        Public Const Sigmoid As Integer = 2
        ''' <summary>整流线性单元</summary>
        Public Const ReLU As Integer = 3

        ''' <summary>按编码应用激活函数。</summary>
        Public Function Apply(x As Double, code As Integer) As Double
            Select Case code
                Case Tanh
                    Return std.Tanh(x)
                Case Sigmoid
                    Return 1.0 / (1.0 + std.Exp(-x))
                Case ReLU
                    Return If(x > 0.0, x, 0.0)
                Case Else
                    Return x
            End Select
        End Function
    End Module

End Namespace
