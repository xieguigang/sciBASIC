Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace UNet

    ''' <summary>
    ''' 一个分割样本：归一化后的图像 + 每像素类别标签。
    ''' </summary>
    ''' <remarks>
    ''' 图像像素按 <c>(y * Width + x) * Channels + c</c> 排列（与 <see cref="DataBlock"/> 的
    ''' <c>(SX*y + x)*Depth + depth</c> 索引公式一致），取值落在 [0,1]；
    ''' 标签按 <c>y * Width + x</c> 排列。
    ''' </remarks>
    Public Class SegmentationSample

        ''' <summary>样本标识（通常是数据集里的目录名）。</summary>
        Public Property Id As String
        ''' <summary>图像宽度。</summary>
        Public Property Width As Integer
        ''' <summary>图像高度。</summary>
        Public Property Height As Integer
        ''' <summary>通道数。</summary>
        Public Property Channels As Integer
        ''' <summary>归一化到 [0,1] 的像素数据。</summary>
        Public Property Pixels As Double()
        ''' <summary>每像素类别下标。</summary>
        Public Property Labels As Integer()

        ''' <summary>像素总数。</summary>
        Public ReadOnly Property PixelCount As Integer
            Get
                Return Width * Height
            End Get
        End Property

        ''' <summary>
        ''' 把像素写进给定的数据块。
        ''' </summary>
        ''' <param name="block">目标数据块，形状必须与本样本一致。</param>
        ''' <remarks>
        ''' 走 <c>addImageData(values, 1.0)</c>：它一次写完并统一声明设备端缓存失效，
        ''' 比逐元素 <c>setWeight</c> 快得多，同时把 [0,1] 平移到 [-0.5, 0.5]。
        ''' </remarks>
        Public Function WriteTo(block As DataBlock) As DataBlock
            Call block.addImageData(Pixels, 1.0)
            Return block
        End Function

        ''' <summary>浅拷贝（用于增广时派生新样本）。</summary>
        Public Function Clone() As SegmentationSample
            Return New SegmentationSample With {
                .Id = Id,
                .Width = Width,
                .Height = Height,
                .Channels = Channels,
                .Pixels = CType(Pixels.Clone(), Double()),
                .Labels = CType(Labels.Clone(), Integer())
            }
        End Function

        ''' <summary>
        ''' 随机增广：水平/垂直翻转与 90° 旋转，图像与标签同步变换。
        ''' </summary>
        ''' <param name="rng">随机数发生器。</param>
        ''' <returns>增广后的新样本；<paramref name="rng"/> 为 Nothing 时返回原对象。</returns>
        Public Function Augment(rng As Random) As SegmentationSample
            If rng Is Nothing Then Return Me

            Dim w = Width, h = Height, c = Channels
            Dim copy = Clone()
            Dim flipH = rng.Next(2) = 1
            Dim flipV = rng.Next(2) = 1
            Dim rot90 = rng.Next(2) = 1

            If Not flipH AndAlso Not flipV AndAlso Not rot90 Then Return copy

            Dim srcPixels = Pixels
            Dim srcLabels = Labels
            Dim dstPixels(srcPixels.Length - 1) As Double
            Dim dstLabels(srcLabels.Length - 1) As Integer

            ' 先把目标坐标 (dy, dx) 映射回源坐标 (sy, sx)
            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    Dim sy = y, sx = x

                    If rot90 AndAlso w = h Then
                        ' 旋转 90°：dst(y, x) = src(w - 1 - x, y)
                        Dim t = sy
                        sy = w - 1 - sx
                        sx = t
                    End If
                    If flipV Then sy = h - 1 - sy
                    If flipH Then sx = w - 1 - sx

                    Dim sBase = (sy * w + sx) * c
                    Dim dBase = (y * w + x) * c

                    For k As Integer = 0 To c - 1
                        dstPixels(dBase + k) = srcPixels(sBase + k)
                    Next

                    dstLabels(y * w + x) = srcLabels(sy * w + sx)
                Next
            Next

            copy.Pixels = dstPixels
            copy.Labels = dstLabels

            Return copy
        End Function

        ''' <summary>单行概述。</summary>
        Public Overrides Function ToString() As String
            Return $"{Id} [{Width}x{Height}x{Channels}]"
        End Function

    End Class

End Namespace
