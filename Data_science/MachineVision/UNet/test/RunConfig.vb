Imports System.IO

''' <summary>
''' demo / selftest 共用的运行配置（命令行解析后的结果）。
''' </summary>
Public Class RunConfig

    ''' <summary>数据集根目录（含 stage1_train / stage1_test 的那一层）。</summary>
    Public Property DataRoot As String = "C:\Users\Administrator\Downloads\unet-dataset"
    ''' <summary>模型输入边长。</summary>
    Public Property ImageSize As Integer = 64
    ''' <summary>下采样层数。</summary>
    Public Property Levels As Integer = 3
    ''' <summary>第一层通道数。</summary>
    Public Property BaseFilters As Integer = 8
    ''' <summary>训练轮数。</summary>
    Public Property Epochs As Integer = 6
    ''' <summary>训练样本上限；0 表示全量 670 张。</summary>
    Public Property Limit As Integer = 40
    ''' <summary>测试样本上限；0 表示全部 65 张。</summary>
    Public Property TestLimit As Integer = 12
    ''' <summary>学习率。</summary>
    Public Property LearningRate As Double = 0.005
    ''' <summary>小批量大小。</summary>
    Public Property BatchSize As Integer = 4
    ''' <summary>每轮学习率衰减。</summary>
    Public Property LrDecay As Double = 0.15
    ''' <summary>RLE 展开方向：True = 列主序。</summary>
    Public Property RleColumnMajor As Boolean = True
    ''' <summary>是否使用数据集解码缓存。</summary>
    Public Property UseCache As Boolean = True
    ''' <summary>是否启用混合精度（卷积热点降 fp32）。</summary>
    Public Property MixedPrecision As Boolean = True
    ''' <summary>落盘目录。</summary>
    Public Property OutputDir As String = ".\unet-out"
    ''' <summary>可视化输出的样本数。</summary>
    Public Property PreviewCount As Integer = 4

    ''' <summary>确保输出目录存在。</summary>
    Public Sub EnsureOutputDir()
        If Not Directory.Exists(OutputDir) Then
            Call Directory.CreateDirectory(OutputDir)
        End If
    End Sub

    ''' <summary>单行配置描述。</summary>
    Public Overrides Function ToString() As String
        ' VB 的插值字符串里不能嵌双引号，所以先把这段文字取出来
        Dim rleText = If(RleColumnMajor, "column-major", "row-major")

        Return $"data={DataRoot}, size={ImageSize}, levels={Levels}, filters={BaseFilters}, " &
               $"epochs={Epochs}, train={Limit}, test={TestLimit}, lr={LearningRate}, batch={BatchSize}, " &
               $"mixed={MixedPrecision}, rle={rleText}"
    End Function

End Class
