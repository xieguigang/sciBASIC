# gr/avi/AVI.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.Imaging.AVIMedia
- AssemblyName  : Microsoft.VisualBasic.Imaging.AVIMedia
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: AVI Video Encoder For Uncompressed RGB Frame Streams
- Existing Desc : A dependency-free AVI (RIFF) writer that assembles main/stream headers and appends bitmap frames into a playable video file, ideal for exporting rendered animation frames from the sciBASIC# imaging pipeline.
- Existing Tags : scibasic;avi;video-encoder;riff;animation

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Imaging.AVIMedia)

## Public types
- Class AVIMainHeader (AVIMainHeader.vb)
- Class AVIStream (AVIStream.vb)
- Class AVIStreamHeader (AVIStreamHeader.vb)
- Enum StreamTypes (AVIStreamHeader.vb) - 流的类型
- Class Encoder (Encoder.vb) - A simple VB.NET AVI encoder > https://github.com/Sebmaster/avi.js
- Class FrameStream (FrameStream.vb)
- Class Settings (Settings.vb)
- Class UInt8Array (UInt8Array.vb)

## Notable public members
- Public Const Magic As String = "avih"
- Public Property cb As Integer = 56
- Public Property dwMicroSecPerFrame As Integer = 66665
- Public Property dwMaxBytesPerSec As Integer = 0
- Public Property dwPaddingGranularity As Integer = 2
- Public Property dwFlags As Integer = 0
- Public ReadOnly Property dwTotalFrames As Integer
- Public Property dwInitialFrames As Integer = 0
- Public ReadOnly Property dwStreams As Integer
- Public Property dwSuggestedBufferSize As Integer = 0
- Public ReadOnly Property dwWidth As Integer
- Public ReadOnly Property dwHeight As Integer
- Public Property dwReserved As Integer()
- Public Sub Write(buffer As UInt8Array)
- Public Property fps As Integer
- Public Property width As Short
- Public Property height As Short
- Public Property frames As New List(Of FrameStream)
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
- Public ReadOnly Property settings As Settings
- Public ReadOnly Property streams As New List(Of AVIStream)
- Public ReadOnly Property main As AVIMainHeader
- Public Sub WriteBuffer(path As String)
- Public Shared Function getVideoHeaderLength(frameLen As Integer) As Integer
- Public Shared Function getVideoDataLength(stream As AVIStream) As Long
- Public ReadOnly Property length As Integer
- Public Overrides Function ToString() As String
- Public Property width As Integer
- Public Property height As Integer
- Public ReadOnly Property length As Long
- Public Function subarray(begin As Long) As UInt8Array
- Public Sub writeBytes(idx As Long, bytes As Byte())
- Public Sub writeShort(idx As Long, num As Short)
- Public Sub writeInt(idx As Long, num As Integer)
- Public Sub writeLong(idx As Long, num As Long)
- Public Sub writeString(idx As Long, str As String)
- ... and 3 more

## Imports
- Microsoft.VisualBasic.ApplicationServices
- Microsoft.VisualBasic.Imaging.BitmapImage
- Microsoft.VisualBasic.Linq
- System.Drawing
- System.IO
- System.Runtime.CompilerServices

## File tree
- AVIMainHeader.vb
- AVIStream.vb
- AVIStreamHeader.vb
- Encoder.vb
- FrameStream.vb
- Settings.vb
- UInt8Array.vb

