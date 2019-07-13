Imports System.Runtime.CompilerServices

''' <summary>
''' A simple VB.NET AVI encoder
''' 
''' > https://github.com/Sebmaster/avi.js
''' </summary>
Public Class Encoder

    Public ReadOnly Property settings As Settings
    Public ReadOnly Property streams As New List(Of AVIStream)

    Sub New(settings As Settings)
        Me.settings = settings
    End Sub

    Public Sub WriteBuffer(path As String)
        Dim dataOffset As Integer() = New Integer(Me.streams.Count - 1) {}
        Dim offset = 0
        Dim frames = 0
        Dim streamHeaderLength = 0

        For i As Integer = 0 To Me.streams.Count - 1
            frames += Me.streams(i).frames.Count
            streamHeaderLength += getVideoHeaderLength(Me.streams(i).frames.Count)
            dataOffset(i) = offset
            offset += getVideoDataLength(streams(i))
        Next

        Dim moviOffset = streamHeaderLength + 12 + ' RIFF 
            12 + ' hdrl 
            8 +  ' avih  
            56 + ' struct  
            12   ' movi 

        Dim buffer As New UInt8Array(path, moviOffset + offset)

        buffer.writeString(0, "RIFF") ' 0
        buffer.writeString(8, "AVI ") ' 8

        buffer.writeString(12, "LIST")
        buffer.writeInt(16, 68 + streamHeaderLength)
        buffer.writeString(20, "hdrl") ' hdrl list
        buffer.writeString(24, "avih") ' avih chunk
        buffer.writeInt(28, 56)        ' avih size

        buffer.writeInt(32, 66665)
        buffer.writeInt(36, 0)         ' MaxBytesPerSec
        buffer.writeInt(40, 2)         ' Padding (In bytes)
        buffer.writeInt(44, 0)         ' Flags
        buffer.writeInt(48, frames)    ' Total Frames
        buffer.writeInt(52, 0)         ' Initial Frames
        buffer.writeInt(56, streams.Count)   ' Total Streams
        buffer.writeInt(60, 0)               ' Suggested Buffer size
        buffer.writeInt(64, settings.width)  ' pixel width
        buffer.writeInt(68, settings.height) ' pixel height
        buffer.writeInt(72, 0) '; Reserved int[4]
        buffer.writeInt(76, 0) ';
        buffer.writeInt(80, 0) ';
        buffer.writeInt(84, 0) ';

        Dim len = 88
        Dim dataOffsetValue%
        Dim subChunk As UInt8Array

        offset = Scan0

        For i As Integer = 0 To Me.streams.Count - 1
            dataOffsetValue = moviOffset + dataOffset(i)
            subChunk = buffer.subarray(88 + offset)
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
        Return 12 + ' strl 
            8 +     ' strh 
            56 +    ' struct  
            8 +     ' strf 
            40 +    ' struct  
            8 +     ' indx 
            24 +    ' struct 
            frameLen * 4 * 2
    End Function

    Public Shared Function getVideoDataLength(stream As AVIStream) As Integer
        Dim len = 0
        Dim frames = stream.frames

        For i As Integer = 0 To frames.Count - 1
            ' Pad if chunk Not in word boundary
            len += 8 + frames(i).length + If(frames(i).length Mod 2 = 0, 0, 1)
        Next

        Return len
    End Function
End Class


