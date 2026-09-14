' ============================================================================
' Surrogate.vb — 替代梯度函数（Surrogate Gradient）
'
' Heaviside 阶跃函数 S = Θ(u) 的导数是 delta 函数，无法直接反向传播。
' 替代梯度法（Neftci et al., 2019）在反向传播时用一条光滑曲线的
' 导数 σ'(u) 代替 delta：前向保持二值脉冲的稀疏性，反向获得可用的梯度。
'
' 本模块实现 readme 中描述的三种替代函数：
'   FastSigmoid : σ'(u) = 1 / (α|u| + 1)²
'   ATan        : σ'(u) = α/(2π) / (1 + (παu/2)²)
'   STE         : σ'(u) = 1（straight-through estimator）
' ============================================================================

Imports std = System.Math

''' <summary>替代梯度函数类型</summary>
Public Enum SurrogateKind
    ''' <summary>fast sigmoid：σ'(u) = 1/(α|u|+1)²（snnTorch 默认之一）</summary>
    FastSigmoid
    ''' <summary>反正切替代：σ'(u) = α/(2π) / (1 + (παu/2)²)</summary>
    ATan
    ''' <summary>直通估计器：反向恒 1</summary>
    STE
End Enum

''' <summary>替代梯度函数库</summary>
Public Module Surrogate

    ''' <summary>
    ''' 前向脉冲生成：Heaviside 阶跃 S = Θ(u)。
    ''' （u >= 0 时发放；u 恰为 0 在连续仿真中测度为零，归入不发放）
    ''' </summary>
    Public Function Spike(u As Double) As Double
        Return If(u > 0.0, 1.0, 0.0)
    End Function

    ''' <summary>
    ''' 替代导数：反向传播时代替 Θ'(u)=δ(u) 使用。
    ''' </summary>
    ''' <param name="u">膜电位与阈值之差 U − U_thr</param>
    Public Function Derivative(u As Double, kind As SurrogateKind, alpha As Double) As Double
        Select Case kind
            Case SurrogateKind.FastSigmoid
                Return 1.0 / std.Pow(alpha * std.Abs(u) + 1.0, 2.0)
            Case SurrogateKind.ATan
                Dim s = (std.PI / 2.0) * alpha * u
                Return (alpha / (2.0 * std.PI)) / (1.0 + s * s)
            Case Else ' STE
                Return 1.0
        End Select
    End Function

    ''' <summary>
    ''' 光滑前向（仅供梯度数值自检）：替代导数的原函数。
    '''   FastSigmoid: f(u) = u / (α|u|+1)，恰好 f'(u) = 1/(α|u|+1)²
    '''   ATan       : f(u) = (1/π)·atan(παu/2) + 1/2，恰好等于 ATan 替代导数
    ''' 把前向的阶跃换成 f 后，loss 关于权重的"解析梯度"（用 σ' 反传）
    ''' 与"数值差分梯度"应一致，从而可验证 BPTT 实现的正确性。
    ''' </summary>
    Public Function SmoothSpike(u As Double, kind As SurrogateKind, alpha As Double) As Double
        Select Case kind
            Case SurrogateKind.ATan
                Return (1.0 / std.PI) * std.Atan((std.PI / 2.0) * alpha * u) + 0.5
            Case SurrogateKind.STE
                Return Spike(u)  ' STE 无光滑原函数，自检时不使用
            Case Else
                Return u / (alpha * std.Abs(u) + 1.0)
        End Select
    End Function

End Module


