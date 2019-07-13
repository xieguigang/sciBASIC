Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' A simple VB.NET AVI encoder
''' 
''' > https://github.com/Sebmaster/avi.js
''' </summary>
Public Class Encoder

    Public ReadOnly settings As Settings
    Public ReadOnly Property streams As BinaryDataWriter

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

    Shared Function getVideoDataLength(frames As Array) As Integer
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

    Public Property fps As Double
    Public Property width As Integer
    Public Property height As Integer
    Public Property frames As New List(Of Byte())

    Dim stream As BinaryDataWriter

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

    Public Function writeHeaderBuffer(idx%, dataOffset%) As Integer
        Dim hexIdx = idx.ToHexString & "db"
        If (hexIdx.Length = 3) Then hexIdx = "0" & hexIdx

        Call stream.Write("LIST", BinaryStringFormat.NoPrefixOrTermination) '0
        Call stream.Write(148 + frames.Count * 4 * 2) '4
        Call stream.Write("strl", BinaryStringFormat.NoPrefixOrTermination) ' 8
        Call stream.Write("strh", BinaryStringFormat.NoPrefixOrTermination) ' 12
        Call stream.Write(56) ' 16
        Call stream.Write("vids") ' 20 // fourCC
        Call stream.Write("DIB ") ' 24 // Uncompressed
        Call stream.Write(0) '28 // Flags
        writeShort(buf, 32, 1); // Priority
		writeShort(buf, 34, 0); // Language
		writeInt(buf, 36, 0); // Initial frames
		writeInt(buf, 40, 1); // Scale
		writeInt(buf, 44, this.fps); // Rate
		writeInt(buf, 48, 0); // Startdelay
		writeInt(buf, 52, this.frames.length); // Length
		writeInt(buf, 56, this.width * this.height * 4 + 8); // suggested buffer size
		writeInt(buf, 60, -1); // quality
		writeInt(buf, 64, 0); // sampleSize
		writeShort(buf, 68, 0); // Rect left
		writeShort(buf, 70, 0); // Rect top
		writeShort(buf, 72, this.width); // Rect width
		writeShort(buf, 74, this.height); // Rect height
		
		writeString(buf, 76, 'strf');
        writeInt(buf, 80, 40);
        writeInt(buf, 84, 40); // struct size
		writeInt(buf, 88, this.width); // width
		writeInt(buf, 92, -this.height); // height
		writeShort(buf, 96, 1); // planes
		writeShort(buf, 98, 32); // bits per pixel
		writeInt(buf, 100, 0); // compression
		writeInt(buf, 104, 0); // image size
		writeInt(buf, 108, 0); // x pixels per meter
		writeInt(buf, 112, 0); // y pixels per meter
		writeInt(buf, 116, 0); // colortable used
		writeInt(buf, 120, 0); // colortable important
		
		writeString(buf, 124, 'indx');
        writeInt(buf, 128, 24 + this.frames.length * 4 * 2); // size
        writeShort(buf, 132, 2); // LongsPerEntry
		writeBytes(buf, 134, [0, 0x01]); // indexSubType + indexType
		writeInt(buf, 136, this.frames.length); // numIndexEntries
		writeString(buf, 140, hexIdx); // chunkID
		writeLong(buf, 144, dataOffset); // data offset
		writeInt(buf, 152, 0); // reserved
    End Function
End Class