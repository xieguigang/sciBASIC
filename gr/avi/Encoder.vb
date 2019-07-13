Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage

''' <summary>
''' A simple VB.NET AVI encoder
''' 
''' > https://github.com/Sebmaster/avi.js
''' </summary>
Public Class Encoder

    Public ReadOnly settings As Settings
    Public ReadOnly Property streams As New List(Of AviStream)

    Sub New(settings As Settings)
        Me.settings = settings
    End Sub

    Public Sub WriteBuffer(path As String)
        Dim dataOffset As Integer() = New Integer(Me.streams.Count - 1) {}
        Dim Offset = 0
        Dim frames = 0
        Dim streamHeaderLength = 0
        For i As Integer = 0 To Me.streams.Count - 1
            frames += Me.streams(i).frames.Count
            streamHeaderLength += getVideoHeaderLength(Me.streams(i).frames.Count)
            dataOffset(i) = Offset
            Offset += getVideoDataLength(Me.streams(i).frames.ToArray)
        Next
        Dim moviOffset = streamHeaderLength + 12 + ' /* RIFF */ 
            12 + '/* hdrl */ 
            8 +' /* avih */ 
            56 +'/* struct */ 
            12 '/* movi */;

        Dim buffer As New UInt8Array(path, moviOffset + Offset)

        buffer.writeString(0, "RIFF") ' 0
        buffer.writeString(8, "AVI ") ' 8

        buffer.writeString(12, "LIST")
        buffer.writeInt(16, 68 + streamHeaderLength)
        buffer.writeString(20, "hdrl") '; // hdrl list
        buffer.writeString(24, "avih") '; // avih chunk
        buffer.writeInt(28, 56) '; // avih size

        buffer.writeInt(32, 66665) ';
        buffer.writeInt(36, 0) '; // MaxBytesPerSec
        buffer.writeInt(40, 2) '; // Padding (In bytes)
        buffer.writeInt(44, 0) '; // Flags
        buffer.writeInt(48, frames) '; // Total Frames
        buffer.writeInt(52, 0) '; // Initial Frames
        buffer.writeInt(56, streams.Count) '; // Total Streams
        buffer.writeInt(60, 0) '; // Suggested Buffer size
        buffer.writeInt(64, settings.width) '; // pixel width
        buffer.writeInt(68, settings.height) '; // pixel height
        buffer.writeInt(72, 0) '; // Reserved int[4]
        buffer.writeInt(76, 0) ';
        buffer.writeInt(80, 0) ';
        buffer.writeInt(84, 0) ';

        Dim len = 88
        Dim dataOffsetValue%
        Dim subChunk As UInt8Array
        Offset = 0
        For i As Integer = 0 To Me.streams.Count - 1
            dataOffsetValue = moviOffset + dataOffset(i)
            subChunk = buffer.subarray(88 + Offset)
            len += Me.streams(i).writeHeaderBuffer(subChunk, i, dataOffsetValue)
            buffer.Flush(subChunk)
        Next

        buffer.writeString(len, "LIST")
        buffer.writeString(len + 8, "movi")

        Dim moviLen = 4
        For i As Integer = 0 To Me.streams.Count - 1
            dataOffsetValue = len + 8 + moviLen
            subChunk = buffer.subarray(dataOffsetValue)
            moviLen += streams(i).writeDataBuffer(subChunk, i)
            buffer.Flush(subChunk)
        Next

        buffer.writeInt(len + 4, moviLen)
        buffer.writeInt(4, len + moviLen)

        Call buffer.Dispose()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function getVideoHeaderLength(frameLen As Integer) As Integer
        Return 12 +' strl 
            8 +' strh 
            56 +' struct  
            8 +' strf 
            40 + ' struct  
            8 +' indx 
            24 +' struct 
            frameLen * 4 * 2
    End Function

    Public Shared Function getVideoDataLength(frames As Byte()()) As Integer
        Dim len = 0
        For i As Integer = 0 To frames.Length - 1
            len += 8 + frames(i).Length + If(frames(i).Length Mod 2 = 0, 0, 1) ' Pad if chunk Not in word boundary
        Next
        Return len
    End Function
End Class

Public Class Settings

    Public Property width As Integer
    Public Property height As Integer

End Class

Public Class AviStream

    Public Property fps As Integer
    Public Property width As Short
    Public Property height As Short
    Public Property frames As New List(Of Byte())

    Sub New(fps%, width As Short, height As Short)
        Me.fps = fps
        Me.width = width
        Me.height = height
    End Sub

    Public Sub addFrame(image As Bitmap)
        Using bitmap As BitmapBuffer = BitmapBuffer.FromBitmap(image, Imaging.ImageLockMode.ReadOnly)
            Call addFrame(bitmap.ToArray)
        End Using
    End Sub

    Public Sub addFrame(imagePixels As Color())
        Dim bytes As New List(Of Byte)

        For Each pixel In imagePixels
            bytes.AddRange({pixel.R, pixel.G, pixel.B, pixel.A})
        Next

        Call addRGBFrame(bytes.ToArray)
    End Sub

    ''' <summary>
    ''' Adds a frame-array to the frame list.
    ''' </summary>
    ''' <param name="imgData">
    ''' the data of an image; a flat array containing (r, g, b, a) values.
    ''' </param>
    Public Sub addRGBFrame(imgData As Byte())
        Dim frame As Byte() = New Byte(imgData.Length - 1) {}
        For i As Integer = 0 To frame.Length - 1 Step 4
            frame(i) = imgData(i + 2)
            frame(i + 1) = imgData(i + 1)
            frame(i + 2) = imgData(i)
        Next

        frames.Add(frame)
    End Sub

    ''' <summary>
    ''' Writes the avi header to a buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <param name="dataOffset">the offset of the stream data from the beginning of the file</param>
    ''' <returns></returns>
    Public Function writeHeaderBuffer(stream As UInt8Array, idx%, dataOffset%) As Integer
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        stream.writeString(0, "LIST") ';
        stream.writeInt(4, 148 + frames.Count * 4 * 2) ';
        stream.writeString(8, "strl") ';
        stream.writeString(12, "strh") ';
        stream.writeInt(16, 56) ';
        stream.writeString(20, "vids") '; // fourCC
        stream.writeString(24, "DIB ") '; // Uncompressed
        stream.writeInt(28, 0) '; // Flags
        stream.writeShort(32, 1) '; // Priority
        stream.writeShort(34, 0) '; // Language
        stream.writeInt(36, 0) '; // Initial frames
        stream.writeInt(40, 1) '; // Scale
        stream.writeInt(44, fps) '; // Rate
        stream.writeInt(48, 0) '; // Startdelay
        stream.writeInt(52, frames.Count) '; // Length
        stream.writeInt(56, CInt(width) * CInt(height) * 4 + 8) '; // suggested buffer size
        stream.writeInt(60, -1) '; // quality
        stream.writeInt(64, 0) '; // sampleSize
        stream.writeShort(68, 0) '; // Rect left
        stream.writeShort(70, 0) '; // Rect top
        stream.writeShort(72, width) '; // Rect width
        stream.writeShort(74, height) '; // Rect height

        stream.writeString(76, "strf") ';
        stream.writeInt(80, 40) ';
        stream.writeInt(84, 40) '; // struct size
        stream.writeInt(88, width) '; // width
        stream.writeInt(92, -height) '; // height
        stream.writeShort(96, 1) '; // planes
        stream.writeShort(98, 32) '; // bits per pixel
        stream.writeInt(100, 0) '; // compression
        stream.writeInt(104, 0) '; // image size
        stream.writeInt(108, 0) '; // x pixels per meter
        stream.writeInt(112, 0) '; // y pixels per meter
        stream.writeInt(116, 0) '; // colortable used
        stream.writeInt(120, 0) '; // colortable important

        stream.writeString(124, "indx") ';
        stream.writeInt(128, 24 + frames.Count * 4 * 2) '; // size
        stream.writeShort(132, 2) '; // LongsPerEntry
        stream.writeBytes(134, {0, &H1}) '; // indexSubType + indexType
        stream.writeInt(136, frames.Count) '; // numIndexEntries
        stream.writeString(140, hexIdx) '; // chunkID
        stream.writeLong(144, dataOffset) '; // data offset
        stream.writeInt(152, 0) '; // reserved

        Dim Offset = 0
        For i As Integer = 0 To Me.frames.Count - 1 ' // index entries
            stream.writeInt(156 + i * 8, Offset) '; // offset
            stream.writeInt(160 + i * 8, frames(i).Length + 8) '; // size
            Offset += Me.frames(i).Length + 8
        Next

        Return 156 + Me.frames.Count * 4 * 2
    End Function

    ''' <summary>
    ''' Writes the frame data of a stream to the buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <returns></returns>
    Public Function writeDataBuffer(buf As UInt8Array, idx As Integer) As Integer
        Dim Len = 0
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        For i As Integer = 0 To Me.frames.Count - 1
            buf.writeString(Len, hexIdx)
            buf.writeInt(Len + 4, frames(i).Length)
            buf.writeBytes(Len + 8, frames(i))
            Len += Me.frames(i).Length + 8
        Next

        Return Len
    End Function
End Class