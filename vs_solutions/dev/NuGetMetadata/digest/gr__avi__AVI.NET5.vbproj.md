# gr/avi/AVI.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.Imaging.AVIMedia
- AssemblyName  : Microsoft.VisualBasic.Imaging.AVIMedia
- TargetFramework: net10.0
- Source files  : 13
- Existing Title: AVI Video Encoder with MJPEG and Uncompressed DIB Codecs
- Existing Desc : A dependency-free AVI (RIFF) video writer for sciBASIC#: assembles main and stream headers to export rendered animation frames as MJPEG or uncompressed 32-bit DIB streams, with per-stream codec selection and configurable JPEG quality and no external codec dependency.
- Existing Tags : scibasic;avi;video-encoder;riff;mjpeg;animation;frame-stream

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Imaging.AVIMedia)

## Public types
- Enum AviCodec (AviCodec.vb) - AVI 视频流的压缩方式。
- Class AVIMainHeader (AVIMainHeader.vb)
- Class AVIStream (AVIStream.vb)
- Class AVIStreamHeader (AVIStreamHeader.vb)
- Enum StreamTypes (AVIStreamHeader.vb) - 流的类型
- Class DibCodec (DibCodec.vb) - 未压缩的 32 位 top-down DIB 编码器（fourCC <c>'DIB '</c>）。 这是改造前 <see cref="AVIStream"/> 的原始输出方式，保留下来作为兼容性回退： 选择该方式即可得到与旧版本完全一致的未压缩画面。
- Class Encoder (Encoder.vb) - A simple VB.NET AVI encoder > https://github.com/Sebmaster/avi.js
- Class FrameStream (FrameStream.vb)
- Class MjpegCodec (MjpegCodec.vb) - Motion JPEG 编码器（fourCC <c>'MJPG'</c>）：每一帧都是一张标准的 JPEG 图片。 帧间不共享信息，因此任意一帧都可以独立解码，容错性最好；压缩比则由 JPEG 画质参数控制。 视频流中的 MJPEG 帧必须是"裸"的 JPEG 字节流，这里直接使用 GDI+ 自带的 JPEG 编码器，
- Class NamespaceDoc (NamespaceDoc.vb) - An AVI (RIFF) video encoder: assembles the main and stream headers and appends uncompressed RGB bitmap frames into a playable video file.
- Class Settings (Settings.vb)
- Class UInt8Array (UInt8Array.vb)

## Notable public members
- Public Const DefaultQuality As Integer = 90
- Public Shared Function Create(type As AviCodec,
- Public Shared Function FourCC(value As String) As Integer
- Public Const Magic As String = "avih"
- Public Property cb As Integer = 56
- Public Const AVIF_HASINDEX As Integer = &H10
- Public ReadOnly Property dwMicroSecPerFrame As Integer
- Public ReadOnly Property dwMaxBytesPerSec As Integer
- Public Property dwPaddingGranularity As Integer = 2
- Public Property dwFlags As Integer = AVIF_HASINDEX
- Public ReadOnly Property dwTotalFrames As Integer
- Public Property dwInitialFrames As Integer = 0
- Public ReadOnly Property dwStreams As Integer
- Public ReadOnly Property dwSuggestedBufferSize As Integer
- Public ReadOnly Property dwWidth As Integer
- Public ReadOnly Property dwHeight As Integer
- Public Property dwReserved As Integer()
- Public Sub Write(buffer As UInt8Array)
- Public Property fps As Integer
- Public Property width As Short
- Public Property height As Short
- Public Property frames As New List(Of FrameStream)
- Public ReadOnly Property codecType As AviCodec
- Public ReadOnly Property codec As AviVideoCodec
- Public Sub addFrame(image As Bitmap)
- Public Sub addFrame(imagePixels As Color())
- Public Sub addRGBFrame(imgData As Byte())
- Public Function writeHeaderBuffer(stream As UInt8Array, idx%, dataOffset As Long) As Integer
- Public Function writeDataBuffer(buf As UInt8Array, idx As Integer) As Long
- Public Const Magic As String = "strh"
- Public Property cb As Integer
- Public Property fccType As StreamTypes = StreamTypes.vids
- Public Property fccHandler As String
- Public Property dwFlags As Integer
- Public Property wPriority As Short
- Public Property wLanguage As Short
- Public Property dwInitialFrames As Integer
- Public Property dwScale As Integer
- Public Property dwRate As Integer
- Public Property dwStart As Integer
- Public Property dwLength As Integer
- Public Property dwSuggestedBufferSize As Integer
- Public Property dwQuality As Integer
- Public Property dwSampleSize As Integer
- Public Property left As Short
- Public Property top As Short
- Public Property right As Short
- Public Property bottom As Short
- Public Sub Write(stream As UInt8Array)
- Public ReadOnly Property fps As Integer
- Public ReadOnly Property width As Integer
- Public ReadOnly Property height As Integer
- Public MustOverride ReadOnly Property FourCC As String
- Public MustOverride ReadOnly Property BitCount As Short
- Public Overridable ReadOnly Property HeightSign As Integer
- Public MustOverride ReadOnly Property SuggestedBufferSize As Integer
- Public Overridable ReadOnly Property CodecPrivate As Byte()
- Public MustOverride Function Encode(bitmap As Bitmap) As Byte()
- Protected Sub validateFrameSize(bitmap As Bitmap)
- Public Overrides ReadOnly Property FourCC As String
- ... and 33 more

## Imports
- Color = System.Drawing.Color
- GdiDrawing = System.Drawing
- GdiImaging = System.Drawing.Imaging
- Microsoft.VisualBasic.ApplicationServices
- Microsoft.VisualBasic.Imaging.BitmapImage
- Microsoft.VisualBasic.Linq
- std = System.Math
- System.Drawing
- System.IO
- System.Runtime.CompilerServices
- System.Runtime.InteropServices

## File tree
- AviCodec.vb
- AVIMainHeader.vb
- AVIStream.vb
- AVIStreamHeader.vb
- AviVideoCodec.vb
- DibCodec.vb
- Encoder.vb
- FrameStream.vb
- MjpegCodec.vb
- NamespaceDoc.vb
- PixelData.vb
- Settings.vb
- UInt8Array.vb

