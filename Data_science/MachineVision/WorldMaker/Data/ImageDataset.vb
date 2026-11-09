' ---------------------------------------------------------------------------
' 训练数据集构建：JPEG 解码 → 盒式降采样 → Tensor(H, W, 3)
'
' 三个必须做对的细节：
'   1. COLMAP 标定的图像尺寸（本例 1959×1090）与磁盘上的 JPEG（980×545）不一致，
'      所以内参必须按“COLMAP 尺寸 → 目标尺寸”整体缩放，而不是按 JPEG 尺寸猜；
'   2. 降采样必须是面积平均（盒式滤波），不能最近邻——最近邻会让训练目标产生
'      高频混叠，高斯在优化时会去拟合混叠噪声，重建表面出现颗粒感；
'   3. Tensor 是 Double 类型：980×545×3 单视图 12.8 MB，301 视图约 3.9 GB 会 OOM，
'      因此按 --width 降分辨率并按 --views 抽帧，另有像素预算兜底。
' ---------------------------------------------------------------------------

Imports System.IO
Imports GS = Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports TF = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Photos

    ''' <summary>
    ''' 训练用图像 + 相机集合。
    ''' </summary>
    Public Class ImageDataset

        ''' <summary>训练目标图像（Tensor(H, W, 3)，值域 0..1）。</summary>
        Public Property Images As New List(Of TF.Tensor)()

        ''' <summary>与 <see cref="Images"/> 一一对应的针孔相机。</summary>
        Public Property Cameras As New List(Of GS.Camera)()

        ''' <summary>与 <see cref="Images"/> 一一对应的 COLMAP 视图。</summary>
        Public Property Views As New List(Of Colmap.ColmapImage)()

        ''' <summary>缩放后的共享内参（所有视图共用同一个相机）。</summary>
        Public Property Intrinsics As GS.CameraIntrinsics

        Public Property SourceWidth As Integer
        Public Property SourceHeight As Integer
        Public Property TargetWidth As Integer
        Public Property TargetHeight As Integer

        Public ReadOnly Property Count As Integer
            Get
                Return Images.Count
            End Get
        End Property

        Public ReadOnly Property TotalPixels As Long
            Get
                Return CLng(TargetWidth) * TargetHeight * Count
            End Get
        End Property

        ''' <summary>
        ''' 把某张训练图另存为 PNG（用于人工核对采样/解码是否正常）。
        ''' </summary>
        Public Sub SavePreview(index As Integer, path As String)
            If index < 0 OrElse index >= Images.Count Then Return

            Call GS.ImageLoader.Save(Images(index), path)
        End Sub

    End Class

    ''' <summary>
    ''' 从 COLMAP 场景构建训练数据集。
    ''' </summary>
    Public NotInheritable Class ImageDatasetBuilder

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 构建训练集：按预算抽帧、解码、降采样、同步缩放内参。
        ''' </summary>
        ''' <param name="scene">COLMAP 稀疏场景</param>
        ''' <param name="imagesDir">JPEG 所在目录</param>
        ''' <param name="options">预算参数</param>
        ''' <param name="log">日志回调</param>
        Public Shared Function Build(scene As Colmap.ColmapScene,
                                     imagesDir As String,
                                     options As Cli.PipelineOptions,
                                     Optional log As Action(Of String) = Nothing) As ImageDataset
            If scene Is Nothing Then Throw New ArgumentNullException(NameOf(scene))
            If scene.Images.Count = 0 Then
                Throw New InvalidOperationException("COLMAP 稀疏模型里没有任何注册成功的图像，无法训练。")
            End If

            ' 1) 只保留磁盘上真实存在的图像（避免用错数据集时到处是 FileNotFound）
            Dim available As New List(Of Colmap.ColmapImage)()
            Dim missing As Integer = 0

            For Each view As Colmap.ColmapImage In scene.Images
                Dim imagePath As String = Path.Combine(imagesDir, view.Name)

                If File.Exists(imagePath) Then
                    Call available.Add(view)
                Else
                    missing += 1
                End If
            Next

            If missing > 0 Then
                Call Report(log, $"[warn] COLMAP 中有 {missing} 张图像在 {imagesDir} 下找不到，已跳过")
            End If

            If available.Count = 0 Then
                Throw New InvalidOperationException(
                    $"COLMAP 里的图像名与 {imagesDir} 下的文件对不上（示例: {scene.Images(0).Name}）。")
            End If

            ' 2) 抽帧：均匀抽稀（确定性，不引入随机性）
            Dim targetWidth As Integer = options.TargetWidth
            Dim sample As Colmap.ColmapImage = available(0)
            Dim sourceWidth As Integer
            Dim sourceHeight As Integer

            Call ProbeSize(Path.Combine(imagesDir, sample.Name), sourceWidth, sourceHeight)

            ' 用第一张图确定“COLMAP 尺寸 → 目标尺寸”的缩放系数
            Dim primaryCamera As Colmap.ColmapCamera = GetCamera(scene, sample)

            Call Report(log, $"第一张图像实际尺寸: {sourceWidth}×{sourceHeight}；" &
                             $"COLMAP 标定尺寸: {primaryCamera.Width}×{primaryCamera.Height}")

            ' 目标高度按 COLMAP 标定的宽高比推导：标定尺寸定义的是投影几何，
            ' 而磁盘上的 JPEG 可能是它的等比缩小版（本例正好是 1/2），
            ' 用标定宽高比可以避免“图像尺寸读取异常”直接毁掉整个训练集。
            Dim targetHeight As Integer = std.Max(2, CInt(std.Round(targetWidth * primaryCamera.Height / CDbl(primaryCamera.Width))))
            Dim viewCount As Integer = std.Min(options.MaxViews, available.Count)

            ' 像素预算兜底
            Dim pixelsPerView As Long = CLng(targetWidth) * targetHeight
            Dim maxByBudget As Integer = CInt(std.Max(1L, options.PixelBudget \ std.Max(1L, pixelsPerView)))

            If viewCount > maxByBudget Then
                Call Report(log, $"[warn] 像素预算 {options.PixelBudget:N0} 只够 {maxByBudget} 个视图，视图数从 {viewCount} 降到 {maxByBudget}")
                viewCount = maxByBudget
            End If

            Dim selected As New List(Of Colmap.ColmapImage)(viewCount)

            For i As Integer = 0 To viewCount - 1
                Dim index As Integer = CInt(CDbl(i) * available.Count / viewCount)

                index = std.Min(index, available.Count - 1)
                Call selected.Add(available(index))
            Next

            Call Report(log, $"抽帧: {available.Count} → {viewCount}（均匀抽稀），渲染尺寸 {targetWidth}×{targetHeight}")

            ' 3) 用第一张图确定“COLMAP 尺寸 → 目标尺寸”的缩放系数
            Dim camera As Colmap.ColmapCamera = GetCamera(scene, selected(0))
            Dim sx As Double = targetWidth / CDbl(camera.Width)
            Dim sy As Double = targetHeight / CDbl(camera.Height)
            Dim intrinsics As New GS.CameraIntrinsics(
                camera.Fx * sx, camera.Fy * sy,
                (camera.Cx + 0.5) * sx - 0.5,
                (camera.Cy + 0.5) * sy - 0.5,
                targetWidth, targetHeight)

            Call Report(log, $"内参缩放: COLMAP {camera.Width}×{camera.Height} (fx={camera.Fx:F2}) → " &
                             $"渲染 {targetWidth}×{targetHeight} (fx={intrinsics.Fx:F2}, fy={intrinsics.Fy:F2}, " &
                             $"cx={intrinsics.Cx:F2}, cy={intrinsics.Cy:F2})")

            ' 4) 逐视图解码 + 降采样 + 组装相机
            Dim dataset As New ImageDataset With {
                .Intrinsics = intrinsics,
                .SourceWidth = camera.Width,
                .SourceHeight = camera.Height,
                .TargetWidth = targetWidth,
                .TargetHeight = targetHeight
            }

            Dim watch = System.Diagnostics.Stopwatch.StartNew()

            For i As Integer = 0 To selected.Count - 1
                Dim view As Colmap.ColmapImage = selected(i)
                Dim imagePath As String = Path.Combine(imagesDir, view.Name)
                Dim tensor As TF.Tensor = LoadAndDownscale(imagePath, targetWidth, targetHeight)

                Call dataset.Images.Add(tensor)
                Call dataset.Views.Add(view)

                Dim cameraOfView As Colmap.ColmapCamera = GetCamera(scene, view)
                Dim viewSx As Double = targetWidth / CDbl(cameraOfView.Width)
                Dim viewSy As Double = targetHeight / CDbl(cameraOfView.Height)
                Dim viewIntrinsics As New GS.CameraIntrinsics(
                    cameraOfView.Fx * viewSx, cameraOfView.Fy * viewSy,
                    (cameraOfView.Cx + 0.5) * viewSx - 0.5,
                    (cameraOfView.Cy + 0.5) * viewSy - 0.5,
                    targetWidth, targetHeight)

                Dim extrinsics As New GS.CameraExtrinsics(CloneMatrix(view.Rotation),
                                                          New Double() {view.Tx, view.Ty, view.Tz})

                Call dataset.Cameras.Add(New GS.Camera(viewIntrinsics, extrinsics, i))

                If (i + 1) Mod 10 = 0 OrElse i = selected.Count - 1 Then
                    Call Report(log, $"  解码 {i + 1}/{selected.Count} ... ({watch.Elapsed.TotalSeconds:F1}s)")
                End If
            Next

            watch.Stop()

            Call Report(log, $"数据集就绪: {dataset.Count} 视图 × {targetWidth}×{targetHeight} " &
                             $"= {dataset.TotalPixels:N0} 像素（{dataset.TotalPixels * 24.0 / 1024 / 1024:F1} MB），" &
                             $"耗时 {watch.Elapsed.TotalSeconds:F1}s")

            Return dataset
        End Function

        ''' <summary>
        ''' 解码 JPEG：先剥掉 APPn/COM 段再用 Core 的依赖无关解码器解码。
        ''' </summary>
        ''' <remarks>
        ''' Tanks&amp;Temples 的照片带 EXIF 缩略图（APP1）。Core 的 <c>JpegReader</c> 在扫描
        ''' 标记时会进入 APP1 的载荷内部，把缩略图的 SOF 当成主图尺寸（实测得到 215×35 的
        ''' 一条蓝色渐变带）。这里在解码前把 APPn/COM 段整段删掉，让解码器只看主图；
        ''' 段结构（长度字段、SOS 之后到 EOI 的熵编码数据）保持原样。
        ''' </remarks>
        Public Shared Function DecodeJpeg(path As String) As Microsoft.VisualBasic.Imaging.BitmapImage.BitmapBuffer
            Dim bytes As Byte() = File.ReadAllBytes(path)
            Dim cleaned As Byte() = StripMetadataSegments(bytes)
            Dim jpeg As Microsoft.VisualBasic.Imaging.JpegImage = Microsoft.VisualBasic.Imaging.JpegImage.Decode(cleaned)

            Return jpeg.GetMemoryBitmap()
        End Function

        ''' <summary>
        ''' 删除 JPEG 中的 APPn（0xE0..0xEF）与 COM（0xFE）段。
        ''' </summary>
        Private Shared Function StripMetadataSegments(bytes As Byte()) As Byte()
            If bytes Is Nothing OrElse bytes.Length < 4 Then Return bytes
            If bytes(0) <> &HFF OrElse bytes(1) <> &HD8 Then Return bytes

            Using output As New MemoryStream(bytes.Length)
                ' SOI
                Call output.WriteByte(&HFF)
                Call output.WriteByte(&HD8)

                Dim pos As Integer = 2

                While pos + 1 < bytes.Length
                    If bytes(pos) <> &HFF Then
                        ' 不是标记：说明已经进入熵编码数据（理论上不会走到这里）
                        Call output.Write(bytes, pos, bytes.Length - pos)

                        Exit While
                    End If

                    Dim marker As Integer = bytes(pos + 1)

                    ' 填充字节（FF FF ...）
                    If marker = &HFF Then
                        pos += 1

                        Continue While
                    End If

                    ' 无载荷的独立标记
                    If marker = &H01 OrElse (marker >= &HD0 AndAlso marker <= &HD7) Then
                        Call output.WriteByte(&HFF)
                        Call output.WriteByte(CByte(marker))

                        pos += 2

                        Continue While
                    End If

                    If pos + 3 >= bytes.Length Then Exit While

                    Dim length As Integer = (CInt(bytes(pos + 2)) << 8) Or CInt(bytes(pos + 3))
                    Dim endPos As Integer = pos + 2 + length

                    If length < 2 OrElse endPos > bytes.Length Then Exit While

                    Dim isMetadata As Boolean = (marker >= &HE0 AndAlso marker <= &HEF) OrElse marker = &HFE

                    If marker = &HDA Then
                        ' SOS：之后是熵编码数据，一直拷贝到文件尾（含 EOI）
                        Call output.Write(bytes, pos, bytes.Length - pos)

                        Exit While
                    End If

                    If Not isMetadata Then
                        Call output.Write(bytes, pos, endPos - pos)
                    End If

                    pos = endPos
                End While

                Return output.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' 解码 JPEG 并用盒式滤波降到目标尺寸。
        ''' </summary>
        Public Shared Function LoadAndDownscale(path As String, targetWidth As Integer, targetHeight As Integer) As TF.Tensor
            Dim buffer = DecodeJpeg(path)

            Dim sourceWidth As Integer = buffer.Width
            Dim sourceHeight As Integer = buffer.Height

            Dim tensor As New TF.Tensor(targetHeight, targetWidth, 3)
            Dim data As Double() = tensor.Data

            ' 盒式滤波：源图的每个像素只被访问一次，天然并行安全（按目标行切分）
            System.Threading.Tasks.Parallel.For(0, targetHeight,
                Sub(dy)
                    Dim y0 As Integer = dy * sourceHeight \ targetHeight
                    Dim y1 As Integer = std.Max(y0 + 1, (dy + 1) * sourceHeight \ targetHeight)

                    If y1 > sourceHeight Then y1 = sourceHeight

                    For dx As Integer = 0 To targetWidth - 1
                        Dim x0 As Integer = dx * sourceWidth \ targetWidth
                        Dim x1 As Integer = std.Max(x0 + 1, (dx + 1) * sourceWidth \ targetWidth)

                        If x1 > sourceWidth Then x1 = sourceWidth

                        Dim sumR As Integer = 0, sumG As Integer = 0, sumB As Integer = 0

                        For sy As Integer = y0 To y1 - 1
                            For sx As Integer = x0 To x1 - 1
                                sumR += buffer.GetRed(sx, sy)
                                sumG += buffer.GetGreen(sx, sy)
                                sumB += buffer.GetBlue(sx, sy)
                            Next
                        Next

                        Dim n As Integer = std.Max(1, (x1 - x0) * (y1 - y0))
                        Dim o As Integer = (dy * targetWidth + dx) * 3

                        data(o) = sumR / (255.0 * n)
                        data(o + 1) = sumG / (255.0 * n)
                        data(o + 2) = sumB / (255.0 * n)
                    Next
                End Sub)

            Return tensor
        End Function

        Private Shared Sub ProbeSize(path As String, ByRef width As Integer, ByRef height As Integer)
            Dim buffer = DecodeJpeg(path)

            width = buffer.Width
            height = buffer.Height
        End Sub

        Private Shared Function GetCamera(scene As Colmap.ColmapScene, view As Colmap.ColmapImage) As Colmap.ColmapCamera
            Dim camera As Colmap.ColmapCamera

            If Not scene.Cameras.TryGetValue(view.CameraId, camera) Then
                Throw New InvalidOperationException($"视图 {view.Name} 引用了不存在的相机 #{view.CameraId}")
            End If

            Return camera
        End Function

        Private Shared Function CloneMatrix(source As Double(,)) As Double(,)
            Dim rows As Integer = source.GetLength(0)
            Dim cols As Integer = source.GetLength(1)
            Dim clone(rows - 1, cols - 1) As Double

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    clone(i, j) = source(i, j)
                Next
            Next

            Return clone
        End Function

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
