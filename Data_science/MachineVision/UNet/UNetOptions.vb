Namespace UNet

    ''' <summary>
    ''' U-Net 网络的结构超参数。
    ''' </summary>
    ''' <remarks>
    ''' 网络是严格对称的：
    ''' <list type="bullet">
    ''' <item>编码器共 <see cref="Levels"/> 层，第 l 层通道数 = <see cref="BaseFilters"/> * 2^l，每层两次 3×3(same) 卷积 + ReLU，之后 2×2 最大池化；</item>
    ''' <item>瓶颈层再做两次卷积，通道数 = <see cref="BaseFilters"/> * 2^<see cref="Levels"/>；</item>
    ''' <item>解码器按相反顺序上采样：2×2 转置卷积(stride=2) → 与对应编码器层拼接 → 两次卷积；</item>
    ''' <item>末端 1×1 卷积把通道数压到 <see cref="NumClasses"/>，得到每像素的类别 logits。</item>
    ''' </list>
    ''' </remarks>
    Public Class UNetOptions

        ''' <summary>输入图像宽度（必须与训练数据实际喂入的尺寸一致）。</summary>
        Public Property InputWidth As Integer = 64
        ''' <summary>输入图像高度。</summary>
        Public Property InputHeight As Integer = 64
        ''' <summary>输入通道数；灰度图为 1，RGB 为 3。</summary>
        Public Property InputChannels As Integer = 1
        ''' <summary>分割类别数（含背景）。</summary>
        Public Property NumClasses As Integer = 2
        ''' <summary>下采样次数（同时也是跳跃连接的条数）。</summary>
        Public Property Levels As Integer = 3
        ''' <summary>第 1 层的通道数，之后每层翻倍。</summary>
        Public Property BaseFilters As Integer = 8
        ''' <summary>编码器卷积核边长（配合 <see cref="ConvPadding"/>=1 得到 same 卷积）。</summary>
        Public Property ConvKernel As Integer = 3
        ''' <summary>编码器卷积的零填充；1 表示 same 卷积。</summary>
        Public Property ConvPadding As Integer = 1
        ''' <summary>解码器转置卷积的核边长；2 配合 stride=2 恰好把尺寸翻倍。</summary>
        Public Property UpKernel As Integer = 2
        ''' <summary>卷积层是否带可学习偏置。</summary>
        Public Property UseBias As Boolean = True

        ''' <summary>
        ''' 校验参数：宽高必须能被 2^<see cref="Levels"/> 整除（否则上采样回来拼接时尺寸对不上）。
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">参数非法时抛出。</exception>
        Public Sub Validate()
            If InputWidth <= 0 OrElse InputHeight <= 0 Then
                Throw New ArgumentOutOfRangeException("InputWidth", "输入尺寸必须为正数")
            End If
            If InputChannels <= 0 Then
                Throw New ArgumentOutOfRangeException("InputChannels", "输入通道数必须为正数")
            End If
            If NumClasses < 2 Then
                Throw New ArgumentOutOfRangeException("NumClasses", "分割类别数至少为 2（背景 + 前景）")
            End If
            If Levels < 1 Then
                Throw New ArgumentOutOfRangeException("Levels", "下采样层数至少为 1")
            End If
            If BaseFilters < 1 Then
                Throw New ArgumentOutOfRangeException("BaseFilters", "基础通道数至少为 1")
            End If
            If ConvKernel <= 0 OrElse UpKernel <= 0 Then
                Throw New ArgumentOutOfRangeException("ConvKernel", "卷积核边长必须为正数")
            End If
            If ConvPadding < 0 OrElse ConvPadding > ConvKernel - 1 Then
                Throw New ArgumentOutOfRangeException("ConvPadding", "零填充宽度必须落在 [0, kernel-1]")
            End If

            Dim factor = CInt(System.Math.Pow(2, Levels))

            If InputWidth Mod factor <> 0 OrElse InputHeight Mod factor <> 0 Then
                Throw New ArgumentOutOfRangeException(
                    "Levels",
                    $"输入尺寸 {InputWidth}x{InputHeight} 必须能被 2^{Levels}={factor} 整除")
            End If
        End Sub

        ''' <summary>拷贝一份独立的配置对象。</summary>
        Public Function Clone() As UNetOptions
            Return DirectCast(MemberwiseClone(), UNetOptions)
        End Function

        ''' <summary>第 <paramref name="level"/> 层（0 起）的通道数。</summary>
        Public Function ChannelsOf(level As Integer) As Integer
            Return BaseFilters * CInt(System.Math.Pow(2, level))
        End Function

        ''' <summary>瓶颈层的通道数。</summary>
        Public ReadOnly Property BottleneckChannels As Integer
            Get
                Return ChannelsOf(Levels)
            End Get
        End Property

        ''' <summary>单行结构描述。</summary>
        Public Overrides Function ToString() As String
            Return $"U-Net({InputWidth}x{InputHeight}x{InputChannels}, levels:{Levels}, filters:{BaseFilters}, classes:{NumClasses})"
        End Function

    End Class

End Namespace
