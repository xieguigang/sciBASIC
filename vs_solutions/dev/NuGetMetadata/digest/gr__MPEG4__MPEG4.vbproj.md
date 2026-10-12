# gr/MPEG4/MPEG4.vbproj

- RootNamespace : Microsoft.VisualBasic.Imaging.MPEG4
- AssemblyName  : Microsoft.VisualBasic.Imaging.MPEG4
- TargetFramework: net10.0
- Source files  : 19
- Existing Title: 
- Existing Desc : 
- Existing Tags : 

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Imaging.MPEG4)

## Public types
- Class BitStreamWriter (BitStreamWriter.vb) - MSB-first 位流写入器，负责 H.264 的位级语法写出与 RBSP 防竞争字节插入。 H.264 的语法元素是按"高位在前"的顺序串起来的，因此这里统一按 MSB-first 累积到字节。 除了常规的无符号/有符号指数哥伦布编码之外，还提供 CABAC 切片所需的
- Class H264Cabac (H264Cabac.vb) - 算术编码部分按 ISO/IEC 14496-10 的 CABAC 编码流程实现（低位 low 以 9 位定点累积、 区间 range 归一化到 [0x100, 0x1FF]、进位链用 outstanding 计数处理）； 状态转移与区间查表则完全镜像 ffmpeg 解码器的用法
- Class H264CabacDebugReader (H264CabacDebugReader.vb) - 用途只有一个：把编码器写出的码流反解回 bin 序列，从而定位「编码器与解码器在哪一个 bin 上分叉」。 它不参与编码/解码流程，也不会被任何正式 API 引用；一旦码流通过验收即可删除本文件。 实现严格对照 libavcodec/cabac.c 的 <c>ff_init_cabac_decoder</c>、cabac_functions.h 的
- Class H264EncodedFrame (H264EncodedFrame.vb) - 一个已编码的采样：AVCC 格式（4 字节大端长度前缀）的 NAL 数据，以及它相对解码时间的显示时间偏移。 B 帧的存在使<b>解码顺序</b>与<b>显示顺序</b>分离：为使用「未来锚点」作为参考，锚点必须先于它前面的 B 帧被编码，因此编码器按解码顺序输出采样，而播放顺序由本类的
- Enum H264IntraMode (H264IntraPrediction.vb) - 帧内预测的模式编号（与 H.264 规范一致）。 亮度 I_16x16 与色度的 4 种模式编号相同（<c>Vertical=0, Horizontal=1, DC=2, Plane=3</c>）， 写入码流时：I_16x16 的模式直接进 mb_type；色度的模式则用 unary 编码单独传输。
- Class H264MotionEstimation (H264MotionEstimation.vb) - 整像素运动估计与运动补偿 首版只做整像素：有界搜索（半径 <see cref="searchRadius"/>）遍历候选位移， 以 SAD 为代价函数取最优。参考平面越界时由 <see cref="H264Plane.at"/> 做边缘复制，
- Class H264Plane (H264Plane.vb) - 单个 8bit 图像平面（亮度或色度），带独立步长。 编码器需要频繁按 4x4 块读写样点，加上重建帧还要作为后续预测的参考， 因此这里用连续的一维数组加步长来承载，避免二维数组的边界检查开销。
- Class H264Yuv420 (H264Plane.vb) - 一幅 4:2:0 YUV 图像（亮度 + 两个色度平面，均已按宏块对齐）。 由 RGBA 帧转换而来：BT.601 的 studio range 整数公式，色度用 2x2 平均下采样。 转换结果同时作为「原始帧」参与残差计算与「重建帧」被逐块覆盖，
- Enum H264SliceType (H264SliceHeader.vb) - H.264 的 slice_type 取值（写入码流的是该值，读到时大于 4 表示整幅图像统一类型）。
- Class H264MbInfo (H264VideoCodec.vb) - 一个宏块在解码器中留下的邻接状态：后续宏块推导上下文时要用到。 三样东西必须与解码器完全一致，否则 CABAC 上下文会跑偏导致码流无法解码： + <c>cbp</c>：完整编码块模式，其中 bit8 表示亮度 DC 块是否有系数、bit6/bit7 表示色度 Cb/Cr 的 DC 块是否有系数
- Class H264VideoCodec (H264VideoCodec.vb) - + **仅 I 帧**，宏块统一使用 <b>I_16x16</b>（含 4 种亮度预测模式的选择） + CABAC 熵编码、Main profile、4:2:0、8bit、关闭去环滤波 + 色度预测模式目前固定为 DC（0）——这样它的上下文恒为 0，规避 ffmpeg 对
- Class Mp4BoxWriter (Mp4BoxWriter.vb) - ISO BMFF（MP4）的 box 写入器：所有字段都是<b>大端</b>序，box 尺寸在写出过程中自动回填。 结构对照 ffmpeg 的 libavformat/movenc.c（例如 <c>mov_write_mvhd_tag</c> @4551、 <c>mov_write_tkhd_tag</c> @4041、<c>mov_write_stbl_tag</c> @3439）。
- Class MP4Encoder (MP4Encoder.vb) - 典型用法（与 <c>avi/test/Module1.vb</c> 的写法一致）： <code> Dim mp4 As New MP4Encoder(New MP4Settings With {.width = frame.Width, .height = frame.Height})
- Structure H264Sample (MP4Muxer.vb) - 一个编码后的 MP4 采样（AVCC 长度前缀形式的 NAL 序列）。
- Class MP4SampleStore (MP4Muxer.vb) - 把编码后的采样落盘到临时文件，并记录尺寸索引。 与 AVI 侧的帧存储思路一致：图像数据始终留在磁盘上，内存里只保留每个采样几字节的索引， 因此导出长时间动画时内存占用不随帧数增长。
- Class MP4TrackSource (MP4Muxer.vb) - 写出容器时一条视频轨所需的信息：帧率、画布尺寸、SPS/PPS 与采样索引。
- Class MP4Muxer (MP4Muxer.vb) - MP4（ISO BMFF）容器封装：写出 ftyp / moov / mdat，并支持 faststart（moov 前置）。 采样数据格式为 AVC 的 <c>lengthSizeMinusOne = 3</c>，即每个 NAL 前面是 4 字节大端长度， SPS/PPS 放在 <c>avcC</c> 中（关键帧采样里也会重复携带，便于任意位置起播）。
- Class MP4Settings (MP4Settings.vb) - MP4 输出的画布设置，形状对齐 <c>Microsoft.VisualBasic.Imaging.AVIMedia.Settings</c>。 使用 With 初始化器即可： <code>New MP4Settings With {.width = 800, .height = 600}</code>
- Class MP4Stream (MP4Stream.vb) - MP4 中的一条视频轨，形状对齐 <c>Microsoft.VisualBasic.Imaging.AVIMedia.AVIStream</c>。 三条帧入口与 AVI 侧保持一致： + <see cref="addFrame(CoreBitmap)"/>：跨平台内存位图

## Notable public members
- Friend Property skipFirstBit As Boolean
- Friend Sub New(Optional capacity As Integer = 4096)
- Friend ReadOnly Property length As Integer
- Friend ReadOnly Property bitLength As Long
- Friend Sub writeBit(b As Integer)
- Friend Sub writeBits(value As Long, bits As Integer)
- Friend Sub writeUe(value As Long)
- Friend Sub writeSe(value As Long)
- Friend Sub alignWithOnes()
- Friend Sub alignWithZeros()
- Friend Sub writeRbspTrailingBits()
- Friend Function toArray() As Byte()
- Friend Shared Function escapeRbsp(rbsp As Byte()) As Byte()
- Friend Shared Function nalUnit(rbsp As Byte(), nalRefIdc As Integer, nalType As Integer, Optional annexB As Boolean = True) As Byte()
- Friend Sub New(bits As BitStreamWriter)
- Friend Sub initStates(isI As Boolean, cabacInitIdc As Integer, sliceQp As Integer)
- Friend Sub begin()
- Friend Sub encodeBin(ctx As Integer, bin As Integer)
- Friend Sub encodeBypass(bit As Integer)
- Friend Sub encodeBypassSign(value As Integer)
- Friend Sub encodeTerminate(bit As Integer)
- Public ReadOnly Property range As Integer
- Public Function readBin(ctx As Integer) As Integer
- Public Function readTerminate() As Integer
- Public Shared Property Enabled As Boolean
- Public Shared ReadOnly Property Bins As List(Of String) = New List(Of String)
- Public Shared ReadOnly Property Ranges As List(Of Integer) = New List(Of Integer)
- Public Shared ReadOnly Property Values As List(Of String) = New List(Of String)
- Public Shared Sub note(name As String, value As Integer)
- Friend ReadOnly Property data As Byte()
- Friend ReadOnly Property compositionOffset As Integer
- Friend Sub New(data As Byte(), compositionOffset As Integer)
- Friend Shared Function allowedModes(leftAvailable As Boolean, topAvailable As Boolean) As Boolean()
- Friend Shared Sub predict16x16(plane As H264Plane, x0 As Integer, y0 As Integer,
- Friend Shared Sub predict8x8(plane As H264Plane, x0 As Integer, y0 As Integer,
- Friend Const searchRadius As Integer = 16
- Friend Shared Function searchInteger(reference As H264Plane, source As H264Plane,
- Friend Shared Function sample(plane As H264Plane, x As Integer, y As Integer) As Integer
- Friend Shared Function sad(reference As H264Plane, source As H264Plane,
- Friend Shared Sub buildPrediction(reference As H264Plane, x0 As Integer, y0 As Integer,
- Friend ReadOnly Property width As Integer
- Friend ReadOnly Property height As Integer
- Friend ReadOnly Property stride As Integer
- Friend ReadOnly Property data As Byte()
- Friend Sub New(width As Integer, height As Integer)
- Friend Sub fill(value As Integer)
- Friend Function at(x As Integer, y As Integer) As Integer
- Friend Sub setAt(x As Integer, y As Integer, value As Integer)
- Friend Sub padEdges(visibleWidth As Integer, visibleHeight As Integer)
- Friend ReadOnly Property codedWidth As Integer
- Friend ReadOnly Property codedHeight As Integer
- Friend ReadOnly Property width As Integer
- Friend ReadOnly Property height As Integer
- Friend ReadOnly Property y As H264Plane
- Friend ReadOnly Property u As H264Plane
- Friend ReadOnly Property v As H264Plane
- Friend Sub New(codedWidth As Integer, codedHeight As Integer, width As Integer, height As Integer)
- Friend Function clone() As H264Yuv420
- Friend Sub loadBgra(bgra As Byte(), pixelWidth As Integer)
- Friend Shared Function writeBlock(cabac As H264Cabac,
- ... and 92 more

## Imports
- Color = System.Drawing.Color
- CoreBitmap = Microsoft.VisualBasic.Imaging.Bitmap
- Microsoft.VisualBasic.Imaging.AVIMedia
- std = System.Math
- System.IO
- System.Runtime.CompilerServices
- System.Text

## File tree
- BitStreamWriter.vb
- H264Cabac.vb
- H264CabacDebugReader.vb
- H264CabacTables.vb
- H264Debug.vb
- H264EncodedFrame.vb
- H264IntraPrediction.vb
- H264MotionEstimation.vb
- H264Plane.vb
- H264Residual.vb
- H264SliceHeader.vb
- H264SpsPps.vb
- H264Transform.vb
- H264VideoCodec.vb
- Mp4BoxWriter.vb
- MP4Encoder.vb
- MP4Muxer.vb
- MP4Settings.vb
- MP4Stream.vb

