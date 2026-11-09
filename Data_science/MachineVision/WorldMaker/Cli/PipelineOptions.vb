' ---------------------------------------------------------------------------
' 流水线预算参数与预设档位
'
' 设计要点：
'   1. 所有“会显著改变耗时/内存”的量都做成参数（视图数、渲染宽度、迭代数、
'      每步视图数、泊松分辨率、BPA 点数上限），换档只改命令行，不改代码；
'   2. 三套预设（fast/standard/high）只是参数组合的别名，任何参数都能单独覆盖；
'   3. 额外提供 --budget-minutes 作为“硬时限”：训练阶段超时即停止并继续后续
'      阶段，保证这条流水线在生产环境里不会因为某一段而无限期挂住。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Text
Imports std = System.Math

Namespace Cli

    ''' <summary>
    ''' 端到端流水线的全部可调参数。
    ''' </summary>
    Public Class PipelineOptions

        Public Const DefaultImagesDir As String =
            "C:\Users\Administrator\Downloads\tandt_db\tandt\train\images"

        ''' <summary>输入照片目录（JPEG 序列）。</summary>
        Public Property ImagesDir As String = DefaultImagesDir

        ''' <summary>COLMAP 稀疏重建目录（含 cameras.bin/images.bin/points3D.bin）。</summary>
        Public Property SparseDir As String = ""

        ''' <summary>产物根目录。</summary>
        Public Property OutputRoot As String = ""

        ''' <summary>本次运行的标签（默认时间戳），用于 output/&lt;runTag&gt; 归档。</summary>
        Public Property RunTag As String = ""

        ''' <summary>预设档位名：fast / standard / high。</summary>
        Public Property Preset As String = "fast"

        ''' <summary>参与训练的视图数量上限。</summary>
        Public Property MaxViews As Integer = 48

        ''' <summary>训练图像的渲染宽度（高度按原始宽高比缩放）。</summary>
        Public Property TargetWidth As Integer = 400

        ''' <summary>训练迭代次数（高斯优化步数）。</summary>
        Public Property Iterations As Integer = 500

        ''' <summary>每步参与优化的视图数（0 = 全部视图；默认取 min(8, MaxViews)）。</summary>
        Public Property ViewsPerStep As Integer = 4

        ''' <summary>训练阶段墙钟时限（分钟，0 = 不限）。到点后停止训练但保留当前模型继续后续阶段。</summary>
        Public Property BudgetMinutes As Double = 25.0

        ''' <summary>自适应致密化间隔（0 = 关闭；真实数据下 182k 初始点已足够稠密，快速档默认关闭）。</summary>
        Public Property DensifyInterval As Integer = 0

        ''' <summary>泊松重建的八叉树分辨率。</summary>
        Public Property PoissonResolution As Integer = 128

        ''' <summary>BPA 重建前点云采样上限（控制滚球法耗时）。</summary>
        Public Property BpaMaxPoints As Integer = 300000

        ''' <summary>网格简化目标三角面数（0 = 不简化）。</summary>
        Public Property SimplifyTarget As Integer = 0

        ''' <summary>是否用 RANSAC 移除最大的平面（地面/背景），Poisson 会把它们一并建模。</summary>
        Public Property RemoveGround As Boolean = False

        ''' <summary>是否执行 BPA 备选重建。</summary>
        Public Property EnableBpa As Boolean = True

        ''' <summary>表面点云过滤：高斯不透明度下限。</summary>
        Public Property MinOpacity As Double = 0.05

        ''' <summary>表面点云过滤：高斯尺度上限倍数（相对中位尺度）。</summary>
        Public Property MaxScaleFactor As Double = 3.0

        ''' <summary>训练结束后渲染多少个视图做对照。</summary>
        Public Property RenderCount As Integer = 4

        ''' <summary>随机种子（视图采样/兜底初始化）。</summary>
        Public Property Seed As Integer = 20260101

        ''' <summary>
        ''' 复用已有的表面点云 PLY，跳过“读取 COLMAP + 训练 + 导出”三个阶段，直接做重建。
        ''' 调整泊松分辨率/简化预算时用它，可以把一轮迭代从十几分钟压到一分钟。
        ''' </summary>
        Public Property ReusePly As String = ""

        ''' <summary>训练张量总像素预算，超出时自动减少视图数（默认 4 亿像素 ≈ 3.2 GB）。</summary>
        Public Property PixelBudget As Long = 400000000L

        ''' <summary>
        ''' 由命令行构造参数（先应用预设，再逐项覆盖）。
        ''' </summary>
        Public Shared Function FromCommandLine(cli As CommandLine) As PipelineOptions
            Dim options As New PipelineOptions()

            options.Preset = If(cli.GetValue("preset"), "fast")
            Call options.ApplyPreset(options.Preset)

            If Not String.IsNullOrEmpty(cli.GetValue("images")) Then options.ImagesDir = cli.GetValue("images")
            If Not String.IsNullOrEmpty(cli.GetValue("sparse")) Then options.SparseDir = cli.GetValue("sparse")
            If Not String.IsNullOrEmpty(cli.GetValue("out")) Then options.OutputRoot = cli.GetValue("out")
            If Not String.IsNullOrEmpty(cli.GetValue("tag")) Then options.RunTag = cli.GetValue("tag")

            options.MaxViews = cli.GetInt("views", options.MaxViews)
            options.TargetWidth = cli.GetInt("width", options.TargetWidth)
            options.Iterations = cli.GetInt("iterations", options.Iterations)
            options.ViewsPerStep = cli.GetInt("views-per-step", options.ViewsPerStep)
            options.BudgetMinutes = cli.GetDouble("budget-minutes", options.BudgetMinutes)
            options.DensifyInterval = cli.GetInt("densify", options.DensifyInterval)
            options.PoissonResolution = cli.GetInt("poisson-res", options.PoissonResolution)
            options.BpaMaxPoints = cli.GetInt("bpa-max-points", options.BpaMaxPoints)
            options.SimplifyTarget = cli.GetInt("simplify", options.SimplifyTarget)
            options.MinOpacity = cli.GetDouble("min-opacity", options.MinOpacity)
            options.MaxScaleFactor = cli.GetDouble("max-scale-factor", options.MaxScaleFactor)
            options.RenderCount = cli.GetInt("render-count", options.RenderCount)
            options.Seed = cli.GetInt("seed", options.Seed)
            options.PixelBudget = CLng(cli.GetDouble("pixel-budget", CDbl(options.PixelBudget)))

            options.ReusePly = If(cli.GetValue("reuse-ply"), "")

            If cli.Has("remove-ground") Then options.RemoveGround = True
            If cli.Has("no-bpa") Then options.EnableBpa = False
            If cli.Has("with-bpa") Then options.EnableBpa = True

            If options.ViewsPerStep <= 0 Then
                options.ViewsPerStep = std.Min(8, std.Max(1, options.MaxViews))
            End If

            Return options
        End Function

        ''' <summary>
        ''' 应用预设档位。
        ''' </summary>
        Public Sub ApplyPreset(name As String)
            Select Case If(name, "fast").Trim().ToLowerInvariant()
                Case "fast"
                    Me.Preset = "fast"
                    Me.MaxViews = 48
                    Me.TargetWidth = 400
                    Me.Iterations = 500
                    Me.ViewsPerStep = 4
                    Me.BudgetMinutes = 25.0
                    Me.PoissonResolution = 160
                    Me.DensifyInterval = 0
                Case "standard"
                    Me.Preset = "standard"
                    Me.MaxViews = 120
                    Me.TargetWidth = 640
                    Me.Iterations = 2000
                    Me.ViewsPerStep = 8
                    Me.BudgetMinutes = 180.0
                    Me.PoissonResolution = 192
                    Me.DensifyInterval = 100
                Case "high"
                    Me.Preset = "high"
                    Me.MaxViews = 301
                    Me.TargetWidth = 980
                    Me.Iterations = 5000
                    Me.ViewsPerStep = 16
                    Me.BudgetMinutes = 0.0
                    Me.PoissonResolution = 256
                    Me.DensifyInterval = 100
                Case Else
                    Throw New ArgumentException($"未知的预设档位: {name}（可选 fast / standard / high）")
            End Select
        End Sub

        ''' <summary>
        ''' 补齐派生默认值并做合法性校验。
        ''' </summary>
        Public Sub Validate()
            If String.IsNullOrEmpty(ImagesDir) OrElse Not Directory.Exists(ImagesDir) Then
                Throw New DirectoryNotFoundException(
                    $"照片目录不存在: {ImagesDir}{vbCrLf}" &
                    "请用 --images <目录> 指定包含 JPEG 序列的文件夹。")
            End If

            If String.IsNullOrEmpty(SparseDir) Then
                ' 约定：<images>\..\sparse\0（Tanks&Temples / COLMAP 默认布局）
                Dim parent As String = Path.GetDirectoryName(Path.GetFullPath(ImagesDir).TrimEnd("\"c))

                SparseDir = Path.Combine(parent, "sparse", "0")

                If Not Directory.Exists(SparseDir) Then
                    SparseDir = Path.Combine(parent, "sparse")
                End If
            End If

            If Not Directory.Exists(SparseDir) Then
                Throw New DirectoryNotFoundException(
                    $"COLMAP 稀疏目录不存在: {SparseDir}{vbCrLf}" &
                    "请用 --sparse <目录> 指定包含 cameras.bin / images.bin / points3D.bin 的文件夹。")
            End If

            If String.IsNullOrEmpty(OutputRoot) Then
                OutputRoot = Path.Combine(Directory.GetCurrentDirectory(), "output")
            End If

            If String.IsNullOrEmpty(RunTag) Then
                RunTag = Date.Now.ToString("yyyyMMdd_HHmmss")
            End If

            If MaxViews < 1 Then Throw New ArgumentOutOfRangeException(NameOf(MaxViews), "视图数量必须 ≥ 1")
            If TargetWidth < 64 Then Throw New ArgumentOutOfRangeException(NameOf(TargetWidth), "渲染宽度必须 ≥ 64")
            If Iterations < 1 Then Throw New ArgumentOutOfRangeException(NameOf(Iterations), "迭代次数必须 ≥ 1")
            If PoissonResolution < 32 OrElse PoissonResolution > 512 Then
                Throw New ArgumentOutOfRangeException(NameOf(PoissonResolution), "泊松分辨率必须在 32..512 之间（内存随分辨率立方增长）")
            End If
            If MinOpacity < 0.0 OrElse MinOpacity > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(MinOpacity), "不透明度阈值必须在 0..1 之间")
            End If
        End Sub

        ''' <summary>
        ''' 参数快照（写入报告，便于复现同一结果）。
        ''' </summary>
        Public Function ToText() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine($"preset            = {Preset}")
            Call sb.AppendLine($"images            = {ImagesDir}")
            Call sb.AppendLine($"sparse            = {SparseDir}")
            Call sb.AppendLine($"output            = {Path.Combine(OutputRoot, RunTag)}")
            Call sb.AppendLine($"views             = {MaxViews}")
            Call sb.AppendLine($"width             = {TargetWidth}")
            Call sb.AppendLine($"iterations        = {Iterations}")
            Call sb.AppendLine($"views-per-step    = {ViewsPerStep}")
            Call sb.AppendLine($"budget-minutes    = {If(BudgetMinutes <= 0, "unlimited", BudgetMinutes.ToString("F1"))}")
            Call sb.AppendLine($"densify-interval  = {DensifyInterval}")
            Call sb.AppendLine($"poisson-res       = {PoissonResolution}")
            Call sb.AppendLine($"bpa-max-points    = {BpaMaxPoints}")
            Call sb.AppendLine($"simplify          = {If(SimplifyTarget <= 0, "off", SimplifyTarget.ToString())}")
            Call sb.AppendLine($"remove-ground     = {RemoveGround}")
            Call sb.AppendLine($"enable-bpa        = {EnableBpa}")
            Call sb.AppendLine($"min-opacity       = {MinOpacity:F3}")
            Call sb.AppendLine($"max-scale-factor  = {MaxScaleFactor:F2}")
            Call sb.AppendLine($"seed              = {Seed}")

            Return sb.ToString()
        End Function

        ''' <summary>命令行帮助文本。</summary>
        Public Shared Function Usage() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine("WorldMaker —— 照片 → 3D 高斯溅射点云 → 三维场景模型")
            Call sb.AppendLine()
            Call sb.AppendLine("用法:")
            Call sb.AppendLine("  WorldMaker [--images <dir>] [--sparse <dir>] [--out <dir>] [选项...]")
            Call sb.AppendLine()
            Call sb.AppendLine("数据与产物:")
            Call sb.AppendLine("  --images <dir>         JPEG 照片目录（默认 Tanks&Temples train/images）")
            Call sb.AppendLine("  --sparse <dir>         COLMAP sparse/0 目录（默认 <images>\..\sparse\0）")
            Call sb.AppendLine("  --out <dir>            产物根目录（默认当前目录下的 output）")
            Call sb.AppendLine("  --tag <name>           运行标签（默认时间戳），产物落在 <out>\<tag>\")
            Call sb.AppendLine()
            Call sb.AppendLine("训练预算:")
            Call sb.AppendLine("  --preset <name>        fast | standard | high（默认 fast）")
            Call sb.AppendLine("  --views <n>            参与训练的视图数上限")
            Call sb.AppendLine("  --width <px>           训练渲染宽度（高度按原比例）")
            Call sb.AppendLine("  --iterations <n>       训练迭代次数")
            Call sb.AppendLine("  --views-per-step <n>   每步使用多少个视图")
            Call sb.AppendLine("  --budget-minutes <m>   训练墙钟时限（0 = 不限；到点保留当前模型继续）")
            Call sb.AppendLine("  --densify <n>          致密化间隔（0 = 关闭）")
            Call sb.AppendLine()
            Call sb.AppendLine("重建与导出:")
            Call sb.AppendLine("  --poisson-res <n>      泊松八叉树分辨率（32..512）")
            Call sb.AppendLine("  --bpa-max-points <n>   BPA 采样上限")
            Call sb.AppendLine("  --no-bpa               跳过 BPA 备选重建")
            Call sb.AppendLine("  --simplify <n>         网格简化目标三角面数（0 = 不简化）")
            Call sb.AppendLine("  --remove-ground        用 RANSAC 移除最大平面（地面/背景）")
            Call sb.AppendLine("  --min-opacity <v>      表面点云不透明度下限（默认 0.1）")
            Call sb.AppendLine("  --max-scale-factor <v> 表面点云尺度上限倍数（默认 3.0）")
            Call sb.AppendLine("  --render-count <n>     训练后渲染对照视图数量")
            Call sb.AppendLine("  --reuse-ply <path>     复用已有表面点云，跳过训练直接重建")
            Call sb.AppendLine("  --seed <n>             随机种子")
            Call sb.AppendLine("  --help                 显示本帮助")

            Return sb.ToString()
        End Function

    End Class

End Namespace
