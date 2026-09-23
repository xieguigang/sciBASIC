' Author:
' 
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

''' <summary>
''' MP4 文件写入器，调用形状对齐 <c>Microsoft.VisualBasic.Imaging.AVIMedia.Encoder</c>。
''' </summary>
''' <remarks>
''' 典型用法（与 <c>avi/test/Module1.vb</c> 的写法一致）：
''' <code>
''' Dim mp4 As New MP4Encoder(New MP4Settings With {.width = frame.Width, .height = frame.Height})
''' Dim stream As New MP4Stream(24, frame.Width, frame.Height, quality:=90)
''' 
''' For i As Integer = 0 To frames
'''     Call stream.addFrame(frame)
''' Next
''' 
''' Call mp4.streams.Add(stream)
''' Call mp4.WriteBuffer("X:\animation.mp4")
''' </code>
''' </remarks>
Public Class MP4Encoder

    Public ReadOnly Property settings As MP4Settings
    Public ReadOnly Property streams As New List(Of MP4Stream)

    ''' <summary>
    ''' 是否把索引（moov）前置，便于边下边播；默认开启。
    ''' </summary>
    Public Property fastStart As Boolean = True

    Sub New(settings As MP4Settings)
        If settings Is Nothing Then
            Throw New ArgumentNullException(NameOf(settings))
        End If

        Me.settings = settings
    End Sub

    ''' <summary>
    ''' 把所有视频轨写出为 mp4 文件
    ''' </summary>
    Public Sub WriteBuffer(path As String)
        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentException("output path could not be empty!", NameOf(path))
        End If
        If streams.Count = 0 Then
            Throw New InvalidOperationException("at least one video stream is required for writing an mp4 file!")
        End If

        Dim tracks As New List(Of MP4TrackSource)

        For Each stream As MP4Stream In streams
            tracks.Add(New MP4TrackSource(stream.fps, stream.width, stream.height, stream.codec.codec, stream.samples))
        Next

        Using muxer As New MP4Muxer(settings.width, settings.height)
            muxer.fastStart = Me.fastStart

            Call muxer.writeFile(path, tracks)
        End Using

        ' 采样数据已经全部写入文件，释放临时文件
        For Each stream As MP4Stream In streams
            Call stream.Dispose()
        Next
    End Sub

End Class
