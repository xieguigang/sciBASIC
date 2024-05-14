#Region "Microsoft.VisualBasic::d0399b73b1b111eb4b2aec4184a6cba2, gr\Microsoft.VisualBasic.Imaging\GifEncoder.vb"

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

    '   Total Lines: 205
    '    Code Lines: 122
    ' Comment Lines: 59
    '   Blank Lines: 24
    '     File Size: 8.02 KB


    ' Class GifEncoder
    ' 
    '     Properties: FrameDelay
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: AddFrame, Dispose, InitHeader, WriteByte, WriteColorTable
    '          WriteGraphicControlBlock, WriteImageBlock, WriteShort, WriteString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

''' <summary>
''' Encodes multiple images as an animated gif to a stream. <br />
''' ALWAYS ALWAYS ALWAYS wire this up   in a using block <br />
''' Disposing the encoder will complete the file. <br />
''' Uses default .net GIF encoding and adds animation headers.
''' </summary>
''' <remarks>
''' https://github.com/DataDink/Bumpkit
''' </remarks>
Public Class GifEncoder : Implements IDisposable

#Region "Header Constants"
    Private Const FileType As String = "GIF"
    Private Const FileVersion As String = "89a"
    Private Const FileTrailer As Byte = &H3B

    Private Const ApplicationExtensionBlockIdentifier As Integer = &HFF21
    Private Const ApplicationBlockSize As Byte = &HB
    Private Const ApplicationIdentification As String = "NETSCAPE2.0"

    Private Const GraphicControlExtensionBlockIdentifier As Integer = &HF921
    Private Const GraphicControlExtensionBlockSize As Byte = &H4

    Private Const SourceGlobalColorInfoPosition As Long = 10
    Private Const SourceGraphicControlExtensionPosition As Long = 781
    Private Const SourceGraphicControlExtensionLength As Long = 8
    Private Const SourceImageBlockPosition As Long = 789
    Private Const SourceImageBlockHeaderLength As Long = 11
    Private Const SourceColorBlockPosition As Long = 13
    Private Const SourceColorBlockLength As Long = 768
#End Region

    Private _isFirstImage As Boolean = True
    Private _width As Integer?
    Private _height As Integer?
    Private _repeatCount As Integer?

    ReadOnly _stream As Stream

    ' Public Accessors
    Public Property FrameDelay() As TimeSpan

    ''' <summary>
    ''' Encodes multiple images as an animated gif to a stream. <br />
    ''' ALWAYS ALWAYS ALWAYS wire this in a using block <br />
    ''' Disposing the encoder will complete the file. <br />
    ''' Uses default .net GIF encoding and adds animation headers.
    ''' </summary>
    ''' <param name="stream">The stream that will be written to.</param>
    ''' <param name="width">Sets the width for this gif or null to use the first frame's width.</param>
    ''' <param name="height">Sets the height for this gif or null to use the first frame's height.</param>
    Public Sub New(stream As Stream, Optional width As Integer? = Nothing, Optional height As Integer? = Nothing, Optional repeatCount As Integer? = Nothing)
        _stream = stream
        _width = width
        _height = height
        _repeatCount = repeatCount
    End Sub

    ''' <summary>
    ''' Adds a frame to this animation.
    ''' </summary>
    ''' <param name="img">The image to add</param>
    ''' <param name="x">The positioning x offset this image should be displayed at.</param>
    ''' <param name="y">The positioning y offset this image should be displayed at.</param>
    Public Sub AddFrame(img As Image, Optional x As Integer = 0, Optional y As Integer = 0, Optional frameDelay As TimeSpan? = Nothing)
        Using gifStream = New MemoryStream()
            img.Save(gifStream, ImageFormat.Gif)
            If _isFirstImage Then
                ' Steal the global color table info
                InitHeader(gifStream, img.Width, img.Height)
            End If
            WriteGraphicControlBlock(gifStream, frameDelay.GetValueOrDefault(Me.FrameDelay))
            WriteImageBlock(gifStream, Not _isFirstImage, x, y, img.Width, img.Height)
        End Using
        _isFirstImage = False
    End Sub

    Private Sub InitHeader(sourceGif As Stream, w As Integer, h As Integer)
        ' File Header
        WriteString(FileType)
        WriteString(FileVersion)
        WriteShort(_width.GetValueOrDefault(w))
        ' Initial Logical Width
        WriteShort(_height.GetValueOrDefault(h))
        ' Initial Logical Height
        sourceGif.Position = SourceGlobalColorInfoPosition
        WriteByte(sourceGif.ReadByte())
        ' Global Color Table Info
        WriteByte(0)
        ' Background Color Index
        WriteByte(0)
        ' Pixel aspect ratio
        WriteColorTable(sourceGif)

        ' App Extension Header
        WriteShort(ApplicationExtensionBlockIdentifier)
        WriteByte(ApplicationBlockSize)
        WriteString(ApplicationIdentification)
        WriteByte(3)
        ' Application block length
        WriteByte(1)
        WriteShort(_repeatCount.GetValueOrDefault(0))
        ' Repeat count for images.
        WriteByte(0)
        ' terminator
    End Sub

    Private Sub WriteColorTable(sourceGif As Stream)
        sourceGif.Position = SourceColorBlockPosition
        ' Locating the image color table
        Dim colorTable = New Byte(SourceColorBlockLength - 1) {}
        sourceGif.Read(colorTable, 0, colorTable.Length)
        _stream.Write(colorTable, 0, colorTable.Length)
    End Sub

    Private Sub WriteGraphicControlBlock(sourceGif As Stream, frameDelay As TimeSpan)
        sourceGif.Position = SourceGraphicControlExtensionPosition
        ' Locating the source GCE
        Dim blockhead = New Byte(SourceGraphicControlExtensionLength - 1) {}
        sourceGif.Read(blockhead, 0, blockhead.Length)
        ' Reading source GCE
        WriteShort(GraphicControlExtensionBlockIdentifier)
        ' Identifier
        WriteByte(GraphicControlExtensionBlockSize)
        ' Block Size
        WriteByte(blockhead(3) And &HF7 Or &H8)
        ' Setting disposal flag
        WriteShort(Convert.ToInt32(frameDelay.TotalMilliseconds / 10))
        ' Setting frame delay
        WriteByte(blockhead(6))
        ' Transparent color index
        WriteByte(0)
        ' Terminator
    End Sub

    Private Sub WriteImageBlock(sourceGif As Stream, includeColorTable As Boolean, x As Integer, y As Integer, h As Integer, w As Integer)
        sourceGif.Position = SourceImageBlockPosition
        ' Locating the image block
        Dim header = New Byte(SourceImageBlockHeaderLength - 1) {}
        sourceGif.Read(header, 0, header.Length)
        WriteByte(header(0))
        ' Separator
        WriteShort(x)
        ' Position X
        WriteShort(y)
        ' Position Y
        WriteShort(h)
        ' Height
        WriteShort(w)
        ' Width
        If includeColorTable Then
            ' If first frame, use global color table - else use local
            sourceGif.Position = SourceGlobalColorInfoPosition
            WriteByte(sourceGif.ReadByte() And &H3F Or &H80)
            ' Enabling local color table
            WriteColorTable(sourceGif)
        Else
            ' Disabling local color table
            WriteByte(header(9) And &H7 Or &H7)
        End If

        WriteByte(header(10))
        ' LZW Min Code Size
        ' Read/Write image data
        sourceGif.Position = SourceImageBlockPosition + SourceImageBlockHeaderLength

        Dim dataLength = sourceGif.ReadByte()
        While dataLength > 0
            Dim imgData = New Byte(dataLength - 1) {}
            sourceGif.Read(imgData, 0, dataLength)

            _stream.WriteByte(Convert.ToByte(dataLength))
            _stream.Write(imgData, 0, dataLength)
            dataLength = sourceGif.ReadByte()
        End While

        _stream.WriteByte(0)
        ' Terminator
    End Sub

    Private Sub WriteByte(value As Integer)
        _stream.WriteByte(Convert.ToByte(value))
    End Sub

    Private Sub WriteShort(value As Integer)
        _stream.WriteByte(Convert.ToByte(value And &HFF))
        _stream.WriteByte(Convert.ToByte((value >> 8) And &HFF))
    End Sub

    Private Sub WriteString(value As String)
        _stream.Write(value.ToArray().[Select](Function(c) CByte(AscW(c))).ToArray(), 0, value.Length)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Complete File
        WriteByte(FileTrailer)

        ' Pushing data
        _stream.Flush()
    End Sub
End Class
