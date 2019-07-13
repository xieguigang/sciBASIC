Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Text

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

        Dim Buffer As New BinaryDataWriter(path.Open(, doClear:=True), Encodings.ASCII)
        Buffer.Write("RIFF", BinaryStringFormat.NoPrefixOrTermination) ' 0
        Buffer.Seek(8, IO.SeekOrigin.Begin)
        Buffer.Write("AVI ", BinaryStringFormat.NoPrefixOrTermination) ' 8

        Buffer.Write("LIST", BinaryStringFormat.NoPrefixOrTermination) ' 12
        Buffer.Write(68 + streamHeaderLength) ' 16;
        Buffer.Write("hdrl", BinaryStringFormat.NoPrefixOrTermination) ' 20; // hdrl list
        Buffer.Write("avih", BinaryStringFormat.NoPrefixOrTermination) ' 24; // avih chunk
        Buffer.Write(56) ' 28; // avih size

        Buffer.Write(66665) ' 32;
        Buffer.Write(0) ' 36; // MaxBytesPerSec
        Buffer.Write(2) '40; // Padding (In bytes)
        Buffer.Write(0) '44; // Flags
        Buffer.Write(frames) ' 48; // Total Frames
        Buffer.Write(0) '52; // Initial Frames
        Buffer.Write(Me.streams.Count) ' 56; // Total Streams
        Buffer.Write(0) ' 60; // Suggested Buffer size
        Buffer.Write(Me.settings.width) ' 64; // pixel width
        Buffer.Write(Me.settings.height) ' 68; // pixel height
        Buffer.Write(0) ' 72; // Reserved int[4]
        Buffer.Write(0) ' 76;
        Buffer.Write(0) ' 80;
        Buffer.Write(0) ' 84;

        Dim Len = 88
        Offset = 0
        For i As Integer = 0 To Me.streams.Count - 1
            Buffer.Seek(88 + Offset, IO.SeekOrigin.Begin)
            Len += Me.streams(i).writeHeaderBuffer(i, moviOffset + dataOffset(i), Buffer)
        Next

        Buffer.Seek(Len, IO.SeekOrigin.Begin)
        Buffer.Write("LIST", BinaryStringFormat.NoPrefixOrTermination)
        Buffer.Seek(Len + 8, IO.SeekOrigin.Begin)
        Buffer.Write("movi", BinaryStringFormat.NoPrefixOrTermination)

        Dim moviLen = 4
        For i As Integer = 0 To Me.streams.Count - 1
            Buffer.Seek(Len + 8 + moviLen, IO.SeekOrigin.Begin)
            moviLen += Me.streams(i).writeDataBuffer(i, Buffer)
        Next

        Buffer.Seek(Len + 4, IO.SeekOrigin.Begin)
        Buffer.Write(moviLen)
        Buffer.Seek(4, IO.SeekOrigin.Begin)
        Buffer.Write(Len + moviLen)

        Call Buffer.Flush()
        Call Buffer.Close()
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

    Public Shared Function getVideoDataLength(frames As Array) As Integer
        Dim Len = 0
        For i As Integer = 0 To frames.Length - 1
            Len += 8 + frames(i).length + If(frames(i).length Mod 2 = 0, 0, 1) ' Pad if chunk Not in word boundary
        Next
        Return Len
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
    Public Function writeHeaderBuffer(idx%, dataOffset%, stream As BinaryDataWriter) As Integer
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        Call stream.Write("LIST", BinaryStringFormat.NoPrefixOrTermination) '0
        Call stream.Write(148 + frames.Count * 4 * 2) '4
        Call stream.Write("strl", BinaryStringFormat.NoPrefixOrTermination) ' 8
        Call stream.Write("strh", BinaryStringFormat.NoPrefixOrTermination) ' 12
        Call stream.Write(56) ' 16
        Call stream.Write("vids", BinaryStringFormat.NoPrefixOrTermination) ' 20 // fourCC
        Call stream.Write("DIB ", BinaryStringFormat.NoPrefixOrTermination) ' 24 // Uncompressed
        Call stream.Write(0) '28 // Flags
        Call stream.Write(1S) ' 32 // Priority
        Call stream.Write(0S) '34 // Language
        Call stream.Write(0) ' 36; // Initial frames
        Call stream.Write(1) ' 40; // Scale
        Call stream.Write(Me.fps) '44; // Rate
        Call stream.Write(0) '48 // Startdelay
        Call stream.Write(Me.frames.Count) '52; // Length
        Call stream.Write(CInt(Me.width) * CInt(Me.height) * 4 + 8) ' 56; // suggested buffer size
        Call stream.Write(-1) ' 60; // quality
        Call stream.Write(0) ' 64; // sampleSize
        Call stream.Write(0S) ' 68; // Rect left
        Call stream.Write(0S) ' 70; // Rect top
        Call stream.Write(Me.width) ' 72; // Rect width
        Call stream.Write(Me.height) '74 // Rect height

        Call stream.Write("strf", BinaryStringFormat.NoPrefixOrTermination) ' 76;
        Call stream.Write(40) ' 80;
        Call stream.Write(40) ' 84; // struct size
        Call stream.Write(Me.width) ' 88; // width
        Call stream.Write(-Me.height) ' 92; // height
        Call stream.Write(1S) ' 96; // planes
        Call stream.Write(32S) ' 98; // bits per pixel
        Call stream.Write(0) ' 100; // compression
        Call stream.Write(0) ' 104; // image size
        Call stream.Write(0) ' 108; // x pixels per meter
        Call stream.Write(0) ' 112; // y pixels per meter
        Call stream.Write(0) ' 116; // colortable used
        Call stream.Write(0) ' 120; // colortable important

        Call stream.Write("indx", BinaryStringFormat.NoPrefixOrTermination) ' 124;
        Call stream.Write(24 + Me.frames.Count * 4 * 2) ' 128; // size
        Call stream.Write(2S) ' 132; // LongsPerEntry
        Call stream.Write(New Byte() {0, &H1}) ' 134; // indexSubType + indexType
        Call stream.Write(Me.frames.Count) ' 136; // numIndexEntries
        Call stream.Write(hexIdx, BinaryStringFormat.NoPrefixOrTermination) ' 140; // chunkID
        Call stream.Write(dataOffset) ' 144; // data offset
        Call stream.Write(0) ' 152; // reserved

        Dim Offset = 0
        For i As Integer = 0 To Me.frames.Count - 1 ' // index entries
            stream.Seek(156 + i * 8, IO.SeekOrigin.Begin)
            stream.Write(Offset) ' ; // offset
            stream.Seek(160 + i * 8, IO.SeekOrigin.Begin)
            stream.Write(Me.frames(i).Length + 8) '; // size
            Offset += Me.frames(i).Length + 8
        Next

        Return 156 + Me.frames.Count * 4 * 2
    End Function

    ''' <summary>
    ''' Writes the frame data of a stream to the buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <returns></returns>
    Public Function writeDataBuffer(idx As Integer, stream As BinaryDataWriter) As Integer
        Dim Len = 0
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        For i As Integer = 0 To Me.frames.Count - 1
            Call stream.Write(hexIdx, BinaryStringFormat.NoPrefixOrTermination) ' 0
            Call stream.Write(Me.frames(i).Length) ' 4
            Call stream.Write(Me.frames(i)) ' 8
            Len += Me.frames(i).Length + 8
        Next

        Return Len
    End Function
End Class