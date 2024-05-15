#Region "Microsoft.VisualBasic::c2f1e1842f6cbd8e130158d5b9db2e75, gr\avi\AVIStream.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 148
    '    Code Lines: 106
    ' Comment Lines: 18
    '   Blank Lines: 24
    '     File Size: 5.79 KB


    ' Class AVIStream
    ' 
    '     Properties: fps, frames, height, width
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: writeDataBuffer, writeHeaderBuffer
    ' 
    '     Sub: (+2 Overloads) addFrame, addRGBFrame
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Public Class AVIStream

    Public Property fps As Integer
    Public Property width As Short
    Public Property height As Short
    Public Property frames As New List(Of FrameStream)

    ReadOnly temp$

    Sub New(fps%, width As Short, height As Short)
        Me.fps = fps
        Me.width = width
        Me.height = height
        Me.temp = TempFileSystem.GetAppSysTempFile(".rgb_frames", App.PID, prefix:=GetHashCode.ToHexString)
    End Sub

    Public Sub addFrame(image As Bitmap)
        Using bitmap As BitmapBuffer = BitmapBuffer.FromBitmap(image, ImageLockMode.ReadOnly)
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
    ''' the data of an image; a flat array containing ``(r, g, b, a)`` values.
    ''' </param>
    Public Sub addRGBFrame(imgData As Byte())
        Dim frame As Byte() = New Byte(imgData.Length - 1) {}
        For i As Integer = 0 To frame.Length - 1 Step 4
            frame(i) = imgData(i + 2)
            frame(i + 1) = imgData(i + 1)
            frame(i + 2) = imgData(i)
        Next

        frames.Add(New FrameStream(temp, frame))
    End Sub

    ''' <summary>
    ''' Writes the avi header to a buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <param name="dataOffset">the offset of the stream data from the beginning of the file</param>
    ''' <returns></returns>
    Public Function writeHeaderBuffer(stream As UInt8Array, idx%, dataOffset As Long) As Integer
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        stream.writeString(0, "LIST")
        stream.writeInt(4, 148 + frames.Count * 4 * 2)
        stream.writeString(8, "strl")
        stream.writeString(12, "strh")
        stream.writeInt(16, 56)
        stream.writeString(20, "vids")    ' fourCC
        stream.writeString(24, "DIB ")    ' Uncompressed
        stream.writeInt(28, 0)            ' Flags
        stream.writeShort(32, 1)          ' Priority
        stream.writeShort(34, 0)          ' Language
        stream.writeInt(36, 0)            ' Initial frames
        stream.writeInt(40, 1)            ' Scale
        stream.writeInt(44, fps)          ' Rate
        stream.writeInt(48, 0)            ' Startdelay
        stream.writeInt(52, frames.Count) ' Length
        stream.writeInt(56, CInt(width) * CInt(height) * 4 + 8) ' suggested buffer size
        stream.writeInt(60, -1)       ' quality
        stream.writeInt(64, 0)        ' sampleSize
        stream.writeShort(68, 0)      ' Rect left
        stream.writeShort(70, 0)      ' Rect top
        stream.writeShort(72, width)  ' Rect width
        stream.writeShort(74, height) ' Rect height

        stream.writeString(76, "strf")
        stream.writeInt(80, 40)
        stream.writeInt(84, 40)      ' struct size
        stream.writeInt(88, width)   ' width
        stream.writeInt(92, -height) ' height
        stream.writeShort(96, 1)     ' planes
        stream.writeShort(98, 32)    ' bits per pixel
        stream.writeInt(100, 0)      ' compression
        stream.writeInt(104, 0)      ' image size
        stream.writeInt(108, 0)      ' x pixels per meter
        stream.writeInt(112, 0)      ' y pixels per meter
        stream.writeInt(116, 0)      ' colortable used
        stream.writeInt(120, 0)      ' colortable important

        stream.writeString(124, "indx")
        stream.writeInt(128, 24 + frames.Count * 4 * 2) ' size
        stream.writeShort(132, 2)                       ' LongsPerEntry
        stream.writeBytes(134, {0, &H1})                ' indexSubType + indexType
        stream.writeInt(136, frames.Count)              ' numIndexEntries
        stream.writeString(140, hexIdx)                 ' chunkID
        stream.writeLong(144, dataOffset)               ' data offset
        stream.writeInt(152, 0)                         ' reserved

        Dim offset As Long = 0

        For i As Integer = 0 To Me.frames.Count - 1            ' index entries
            ' 原先这里是writeInt，但是大文件溢出了
            stream.writeLong(156 + i * 8, offset)              ' offset
            stream.writeInt(160 + i * 8, frames(i).length + 8) ' size

            offset += Me.frames(i).length + 8
        Next

        Return 156 + Me.frames.Count * 4 * 2
    End Function

    ''' <summary>
    ''' Writes the frame data of a stream to the buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <returns></returns>
    Public Function writeDataBuffer(buf As UInt8Array, idx As Integer) As Long
        Dim len& = 0
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        For i As Integer = 0 To Me.frames.Count - 1
            buf.writeString(len, hexIdx)
            buf.writeInt(len + 4, frames(i).length)
            buf.writeBytes(len + 8, frames(i))

            len += frames(i).length + 8
        Next

        Return len
    End Function
End Class
