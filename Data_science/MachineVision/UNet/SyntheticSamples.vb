Namespace UNet

    ''' <summary>
    ''' 确定性合成样本生成器。
    ''' </summary>
    ''' <remarks>
    ''' 真实数据（DSB2018）用于端到端的训练与评估，而**数值类**断言（梯度校验、
    ''' 结构自检）需要"小而可控、不依赖外部文件"的数据，故保留这个生成器：
    ''' 背景是缓慢变化的光照 + 噪声，前景是若干实心圆盘，标签就是圆盘内部。
    ''' 同一个种子必然产生同一批数据，因此自检可复现。
    ''' </remarks>
    Public Module SyntheticSamples

        ''' <summary>
        ''' 生成一批合成样本。
        ''' </summary>
        ''' <param name="count">样本数。</param>
        ''' <param name="size">图像边长。</param>
        ''' <param name="seed">随机种子。</param>
        ''' <param name="noise">背景噪声幅度（0~1）。</param>
        ''' <returns>合成样本数组。</returns>
        Public Function Generate(count As Integer, size As Integer,
                                 Optional seed As Integer = 2015,
                                 Optional noise As Double = 0.12) As SegmentationSample()
            Dim rnd As New Random(seed)
            Dim result(count - 1) As SegmentationSample

            For n As Integer = 0 To count - 1
                Dim pixels(size * size - 1) As Double
                Dim labels(size * size - 1) As Integer

                ' 背景：低频光照 + 噪声
                Dim gx = rnd.NextDouble() * 0.4
                Dim gy = rnd.NextDouble() * 0.4
                Dim gx1 = rnd.NextDouble() * 0.4
                Dim gy1 = rnd.NextDouble() * 0.4

                For y As Integer = 0 To size - 1
                    For x As Integer = 0 To size - 1
                        Dim u = x / CDbl(size)
                        Dim v = y / CDbl(size)
                        Dim background = 0.25 + gx * u + gy * v + gx1 * u * v + gy1 * (1 - v)

                        background += (rnd.NextDouble() - 0.5) * noise

                        If background < 0 Then background = 0
                        If background > 1 Then background = 1

                        pixels(y * size + x) = background
                    Next
                Next

                ' 前景：1~3 个实心圆盘
                Dim shapes = 1 + rnd.Next(3)

                For s As Integer = 1 To shapes
                    Dim cx = rnd.Next(size)
                    Dim cy = rnd.Next(size)
                    Dim radius = size * (0.08 + rnd.NextDouble() * 0.16)
                    Dim r2 = radius * radius
                    Dim bright = 0.75 + rnd.NextDouble() * 0.25

                    For y As Integer = 0 To size - 1
                        Dim dy = y - cy

                        For x As Integer = 0 To size - 1
                            Dim dx = x - cx

                            If dx * dx + dy * dy <= r2 Then
                                Dim idx = y * size + x

                                pixels(idx) = bright + (rnd.NextDouble() - 0.5) * noise * 0.5
                                labels(idx) = 1
                            End If
                        Next
                    Next
                Next

                result(n) = New SegmentationSample With {
                    .Id = $"synth_{n:000}",
                    .Width = size,
                    .Height = size,
                    .Channels = 1,
                    .Pixels = pixels,
                    .Labels = labels
                }
            Next

            Return result
        End Function

    End Module

End Namespace
