' ---------------------------------------------------------------------------
' Double <-> Single 精度桥接
'
' Tensor 的数据是 Double，而 ILCuda 框架自带的内核全部是 Single（float）。
' P1 阶段采用“float 快速通道”：把主机 Double 数组降为 Single 上传到显存计算，
' 再把结果升回 Double。精度损失约 1e-7 量级（后续可用 IL2Cuda 生成 double 内核补齐）。
' ---------------------------------------------------------------------------

Namespace GPUTensor

    Friend Module FloatBridge

        ''' <summary>把主机 Double 数组降精度为 Single 数组</summary>
        Public Function ToSingle(src As Double()) As Single()
            Dim dst(src.Length - 1) As Single

            For i As Integer = 0 To src.Length - 1
                dst(i) = CSng(src(i))
            Next

            Return dst
        End Function

        ''' <summary>把显存回读的 Single 数组升精度为 Double 数组</summary>
        Public Function ToDouble(src As Single()) As Double()
            Dim dst(src.Length - 1) As Double

            For i As Integer = 0 To src.Length - 1
                dst(i) = CDbl(src(i))
            Next

            Return dst
        End Function

    End Module

End Namespace
