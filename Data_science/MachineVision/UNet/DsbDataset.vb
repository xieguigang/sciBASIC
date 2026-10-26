' ---------------------------------------------------------------------------
' DSB2018（Kaggle 细胞核分割）数据集加载
'
' 目录形态：
'   stage1_train/<id>/images/<id>.png      一张原图
'   stage1_train/<id>/masks/*.png          若干个实例掩膜（合并成二值前景）
'   stage1_test/<id>/images/<id>.png       一张原图，**没有 masks**
'   stage1_solution.csv                    测试集的真值（RLE）
'   stage1_train_labels.csv                训练集的真值（RLE，与上面同格式）
'
' 处理：解码 PNG → 转灰度 → 按每图对比度归一化到 [0,1] → 双线性缩放到模型输入尺寸；
'      掩膜用最近邻缩放 + 0.5 阈值（避免插值把二值标签糊掉）。
'
' 缓存：670 张图 × 若干实例掩膜，首次解码要几十秒，
'      因此把"解码 + 缩放"之后的结果落成二进制缓存，之后直接读取。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Png
Imports std = System.Math

Namespace UNet

    ''' <summary>
    ''' DSB2018 数据集的加载与缓存。
    ''' </summary>
    Public Module DsbDataset

        ''' <summary>训练/验证目录名。</summary>
        Public Const TrainFolder As String = "stage1_train"
        ''' <summary>测试目录名（无 masks，真值来自 solution csv）。</summary>
        Public Const TestFolder As String = "stage1_test"
        ''' <summary>测试集真值文件。</summary>
        Public Const SolutionFile As String = "stage1_solution.csv"
        ''' <summary>训练集真值文件（用于校对 RLE 展开方向）。</summary>
        Public Const TrainLabelFile As String = "stage1_train_labels.csv"

        Private ReadOnly CacheMagic As Byte() = New Byte() {&H55, &H4E, &H44, &H53}

        ''' <summary>
        ''' 加载训练集。
        ''' </summary>
        ''' <param name="root">数据集根目录（含 stage1_train 的那一层）。</param>
        ''' <param name="imageSize">缩放到的边长。</param>
        ''' <param name="limit">最多加载多少张；0 表示全部。</param>
        ''' <param name="useCache">是否使用/写入二进制缓存。</param>
        Public Function LoadTrain(root As String, imageSize As Integer,
                                  Optional limit As Integer = 0,
                                  Optional useCache As Boolean = True) As SegmentationSample()
            Dim cacheFile = CachePath(root, "train", imageSize, limit)

            If useCache Then
                Dim cached = TryReadCache(cacheFile)

                If cached IsNot Nothing Then Return cached
            End If

            Dim samples = LoadSplit(System.IO.Path.Combine(root, TrainFolder), imageSize, limit, Nothing)
            Dim result = samples.ToArray()

            If useCache Then Call WriteCache(cacheFile, result)

            Return result
        End Function

        ''' <summary>
        ''' 加载测试集（真值来自 stage1_solution.csv 的 RLE）。
        ''' </summary>
        ''' <param name="root">数据集根目录。</param>
        ''' <param name="imageSize">缩放到的边长。</param>
        ''' <param name="limit">最多加载多少张；0 表示全部。</param>
        ''' <param name="columnMajor">RLE 的展开方向；默认为列主序（由 selftest 交叉验证锁定）。</param>
        ''' <param name="useCache">是否使用/写入二进制缓存。</param>
        Public Function LoadTest(root As String, imageSize As Integer,
                                 Optional limit As Integer = 0,
                                 Optional columnMajor As Boolean = True,
                                 Optional useCache As Boolean = True) As SegmentationSample()
            Dim cacheFile = CachePath(root, $"test_{If(columnMajor, "col", "row")}", imageSize, limit)

            If useCache Then
                Dim cached = TryReadCache(cacheFile)

                If cached IsNot Nothing Then Return cached
            End If

            Dim solutionPath = System.IO.Path.Combine(root, SolutionFile)

            If Not System.IO.File.Exists(solutionPath) Then
                Throw New FileNotFoundException("找不到测试集真值文件, 无法对 stage1_test 做评估", solutionPath)
            End If

            Dim truth = ReadSolution(solutionPath)
            Dim samples = LoadSplit(System.IO.Path.Combine(root, TestFolder), imageSize, limit, truth, columnMajor)
            Dim result = samples.ToArray()

            If useCache Then Call WriteCache(cacheFile, result)

            Return result
        End Function

        ''' <summary>
        ''' 读取一个 split 目录下的全部样本。
        ''' </summary>
        ''' <param name="dir">split 目录（stage1_train / stage1_test）。</param>
        ''' <param name="imageSize">目标边长。</param>
        ''' <param name="limit">样本上限。</param>
        ''' <param name="truth">
        ''' 外部真值表；为 Nothing 时从 <c>masks/*.png</c> 读取（训练集）。
        ''' </param>
        ''' <param name="columnMajor">RLE 展开方向。</param>
        Private Function LoadSplit(dir As String, imageSize As Integer, limit As Integer,
                                   truth As Dictionary(Of String, List(Of String)),
                                   Optional columnMajor As Boolean = True) As List(Of SegmentationSample)
            If Not Directory.Exists(dir) Then
                Throw New DirectoryNotFoundException($"找不到数据集目录: {dir}")
            End If

            Dim dirs = Directory.GetDirectories(dir)
            Array.Sort(dirs)

            If limit > 0 AndAlso limit < dirs.Length Then
                dirs = dirs.Take(limit).ToArray()
            End If

            Dim samples As New List(Of SegmentationSample)

            For Each d In dirs
                Dim id = System.IO.Path.GetFileName(d)
                Dim imgDir = System.IO.Path.Combine(d, "images")
                Dim images = Directory.GetFiles(imgDir, "*.png")

                If images.Length = 0 Then Continue For

                Dim image = PngReader.Decode(images(0), toGray:=True)

                If image.IsBlank Then
                    ' 解码出全 0 通常意味着位深/颜色类型没处理对，属于"假通过"的典型来源
                    Throw New InvalidDataException($"解码出的图像全为 0, 请检查解码器: {images(0)}")
                End If

                Dim mask As Boolean() = Nothing

                If truth Is Nothing Then
                    mask = LoadMaskFromPng(System.IO.Path.Combine(d, "masks"), image.Width, image.Height)
                ElseIf truth.ContainsKey(id) Then
                    mask = RleCodec.Merge(truth(id), image.Height, image.Width, columnMajor)
                End If

                If mask Is Nothing Then Continue For

                Dim pixels = ResizeImage(image.Pixels, image.Width, image.Height, imageSize)
                Dim labels = RleCodec.ToLabels(ResizeMask(mask, image.Width, image.Height, imageSize))

                Call NormalizeContrast(pixels)

                samples.Add(New SegmentationSample With {
                    .Id = id,
                    .Width = imageSize,
                    .Height = imageSize,
                    .Channels = 1,
                    .Pixels = pixels,
                    .Labels = labels
                })
            Next

            Return samples
        End Function

        ''' <summary>把 masks 目录下的多个实例掩膜合并成一张二值前景掩膜。</summary>
        Public Function LoadMaskFromPng(maskDir As String, width As Integer, height As Integer) As Boolean()
            Dim mask(width * height - 1) As Boolean

            If Not Directory.Exists(maskDir) Then Return Nothing

            For Each file In Directory.GetFiles(maskDir, "*.png")
                Dim m = PngReader.Decode(file, toGray:=True)

                If m.Width <> width OrElse m.Height <> height Then
                    Throw New InvalidDataException(
                        $"掩膜尺寸 {m.Width}x{m.Height} 与原图 {width}x{height} 不一致: {file}")
                End If

                For i As Integer = 0 To mask.Length - 1
                    If m.Pixels(i) > 0.5 Then mask(i) = True
                Next
            Next

            Return mask
        End Function

        ''' <summary>
        ''' 读取 solution / labels CSV，返回 ImageId → RLE 列表。
        ''' </summary>
        Public Function ReadSolution(path As String) As Dictionary(Of String, List(Of String))
            Dim table As New Dictionary(Of String, List(Of String))

            For Each raw In System.IO.File.ReadLines(path).Skip(1)
                If raw.Length = 0 Then Continue For

                ' EncodedPixels 内部只有空格，不含逗号，所以按逗号切分是安全的
                Dim parts = raw.Split(","c)

                If parts.Length < 2 Then Continue For

                Dim id = parts(0).Trim()
                Dim encoded = parts(1).Trim()

                If Not table.ContainsKey(id) Then
                    table(id) = New List(Of String)()
                End If

                If encoded.Length > 0 Then
                    table(id).Add(encoded)
                End If
            Next

            Return table
        End Function

        ''' <summary>读取 solution CSV 中每张图的 (height, width)。</summary>
        Public Function ReadSolutionSizes(path As String) As Dictionary(Of String, Integer())
            Dim table As New Dictionary(Of String, Integer())

            For Each raw In System.IO.File.ReadLines(path).Skip(1)
                Dim parts = raw.Split(","c)

                If parts.Length < 4 Then Continue For

                Dim id = parts(0).Trim()
                Dim h As Integer, w As Integer

                If Integer.TryParse(parts(2), h) AndAlso Integer.TryParse(parts(3), w) Then
                    table(id) = New Integer() {h, w}
                End If
            Next

            Return table
        End Function

        ' ---------------- 缩放与归一化 ----------------

        ''' <summary>双线性缩放单通道图像。</summary>
        Public Function ResizeImage(src As Double(), w As Integer, h As Integer, size As Integer) As Double()
            Dim dst(size * size - 1) As Double
            Dim scaleX = w / CDbl(size)
            Dim scaleY = h / CDbl(size)

            For y As Integer = 0 To size - 1
                Dim sy = (y + 0.5) * scaleY - 0.5

                If sy < 0 Then sy = 0
                If sy > h - 1 Then sy = h - 1

                Dim y0 = CInt(std.Floor(sy))
                Dim y1 = std.Min(y0 + 1, h - 1)
                Dim fy = sy - y0

                For x As Integer = 0 To size - 1
                    Dim sx = (x + 0.5) * scaleX - 0.5

                    If sx < 0 Then sx = 0
                    If sx > w - 1 Then sx = w - 1

                    Dim x0 = CInt(std.Floor(sx))
                    Dim x1 = std.Min(x0 + 1, w - 1)
                    Dim fx = sx - x0

                    Dim v00 = src(y0 * w + x0)
                    Dim v01 = src(y0 * w + x1)
                    Dim v10 = src(y1 * w + x0)
                    Dim v11 = src(y1 * w + x1)

                    Dim top = v00 + (v01 - v00) * fx
                    Dim bottom = v10 + (v11 - v10) * fx

                    dst(y * size + x) = top + (bottom - top) * fy
                Next
            Next

            Return dst
        End Function

        ''' <summary>
        ''' 最近邻缩放二值掩膜（不做插值，避免把标签糊成小数后用阈值又丢掉细结构）。
        ''' </summary>
        Public Function ResizeMask(src As Boolean(), w As Integer, h As Integer, size As Integer) As Boolean()
            Dim dst(size * size - 1) As Boolean

            For y As Integer = 0 To size - 1
                Dim sy = CInt((y + 0.5) * h / size)

                If sy < 0 Then sy = 0
                If sy > h - 1 Then sy = h - 1

                For x As Integer = 0 To size - 1
                    Dim sx = CInt((x + 0.5) * w / size)

                    If sx < 0 Then sx = 0
                    If sx > w - 1 Then sx = w - 1

                    dst(y * size + x) = src(sy * w + sx)
                Next
            Next

            Return dst
        End Function

        ''' <summary>按每张图自身的对比度归一化到 [0,1]（显微图像的染色强度差异很大）。</summary>
        Public Sub NormalizeContrast(pixels As Double())
            Dim min As Double = Double.MaxValue
            Dim max As Double = Double.MinValue

            For Each v In pixels
                If v < min Then min = v
                If v > max Then max = v
            Next

            Dim range = max - min

            If range <= 0 Then
                For i As Integer = 0 To pixels.Length - 1
                    pixels(i) = 0.5
                Next

                Return
            End If

            For i As Integer = 0 To pixels.Length - 1
                pixels(i) = (pixels(i) - min) / range
            Next
        End Sub

        ' ---------------- 缓存 ----------------

        Private Function CachePath(root As String, split As String, imageSize As Integer, limit As Integer) As String
            Dim dir = System.IO.Path.Combine(root, ".unet-cache")

            If Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            Return System.IO.Path.Combine(dir, $"{split}_{imageSize}_{limit}.bin")
        End Function

        Private Function TryReadCache(path As String) As SegmentationSample()
            If Not System.IO.File.Exists(path) Then Return Nothing

            Try
                Using fs As New FileStream(path, FileMode.Open, FileAccess.Read)
                    Using br As New BinaryReader(fs)
                        Dim head = br.ReadBytes(4)

                        If head.Length <> 4 OrElse head(0) <> CacheMagic(0) OrElse head(1) <> CacheMagic(1) OrElse
                           head(2) <> CacheMagic(2) OrElse head(3) <> CacheMagic(3) Then
                            Return Nothing
                        End If

                        If br.ReadInt32() <> 1 Then Return Nothing

                        Dim count = br.ReadInt32()
                        Dim result(count - 1) As SegmentationSample

                        For i As Integer = 0 To count - 1
                            Dim sample As New SegmentationSample With {.Id = br.ReadString()}

                            sample.Width = br.ReadInt32()
                            sample.Height = br.ReadInt32()
                            sample.Channels = br.ReadInt32()

                            Dim pixels(sample.Width * sample.Height * sample.Channels - 1) As Double

                            For k As Integer = 0 To pixels.Length - 1
                                pixels(k) = br.ReadDouble()
                            Next

                            sample.Pixels = pixels

                            Dim labels(sample.Width * sample.Height - 1) As Integer

                            For k As Integer = 0 To labels.Length - 1
                                labels(k) = br.ReadInt32()
                            Next

                            sample.Labels = labels

                            result(i) = sample
                        Next

                        Return result
                    End Using
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Private Sub WriteCache(path As String, samples As SegmentationSample())
            Try
                Using fs As New FileStream(path, FileMode.Create, FileAccess.Write)
                    Using bw As New BinaryWriter(fs)
                        Call bw.Write(CacheMagic)
                        Call bw.Write(1I)
                        Call bw.Write(samples.Length)

                        For Each sample In samples
                            Call bw.Write(sample.Id)
                            Call bw.Write(sample.Width)
                            Call bw.Write(sample.Height)
                            Call bw.Write(sample.Channels)

                            For Each v In sample.Pixels
                                Call bw.Write(v)
                            Next
                            For Each v In sample.Labels
                                Call bw.Write(v)
                            Next
                        Next
                    End Using
                End Using
            Catch
                ' 缓存写失败不影响主流程
            End Try
        End Sub

    End Module

End Namespace
