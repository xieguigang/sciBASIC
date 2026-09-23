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

' 父命名空间 Microsoft.VisualBasic.Imaging 下存在同名的 Bitmap 类型，这里使用显式别名
Imports Bitmap = System.Drawing.Bitmap

''' <summary>
''' AVI 视频流编解码器契约。
''' </summary>
''' <remarks>
''' 由 <see cref="AVIStream"/> 按流持有一个实例。容器侧只依赖本契约暴露的
''' fourCC、位深、高度符号、建议缓冲大小以及编解码器私有数据，因此新增一种压缩
''' 方式只需要实现本类，不需要改动任何容器写入逻辑。
''' </remarks>
Public MustInherit Class AviVideoCodec

    ''' <summary>
    ''' 流的帧率，部分压缩方式（如 MPEG-4）需要用它推导码流的时间基准。
    ''' </summary>
    Public ReadOnly Property fps As Integer

    ''' <summary>
    ''' 画面宽度（像素）
    ''' </summary>
    Public ReadOnly Property width As Integer

    ''' <summary>
    ''' 画面高度（像素）
    ''' </summary>
    Public ReadOnly Property height As Integer

    Sub New(fps As Integer, width As Integer, height As Integer)
        Me.fps = fps
        Me.width = width
        Me.height = height
    End Sub

    ''' <summary>
    ''' 同时写入 <c>strh.fccHandler</c> 与 <c>strf.biCompression</c> 的四字符编码。
    ''' </summary>
    Public MustOverride ReadOnly Property FourCC As String

    ''' <summary>
    ''' <c>strf.biBitCount</c>
    ''' </summary>
    Public MustOverride ReadOnly Property BitCount As Short

    ''' <summary>
    ''' <c>strf.biHeight</c> 的符号：未压缩 DIB 为 -1（top-down），压缩格式统一为 +1。
    ''' </summary>
    Public Overridable ReadOnly Property HeightSign As Integer
        Get
            Return 1
        End Get
    End Property

    ''' <summary>
    ''' 建议的帧数据缓冲大小，同时用于 <c>strh.dwSuggestedBufferSize</c> 与 <c>strf.biSizeImage</c>。
    ''' </summary>
    Public MustOverride ReadOnly Property SuggestedBufferSize As Integer

    ''' <summary>
    ''' 编解码器私有数据：需要在首个视频帧之前写入 <c>movi</c> 的字节流（MPEG-4 为 VOL 头）。
    ''' 不需要私有数据的压缩方式返回 Nothing。
    ''' </summary>
    Public Overridable ReadOnly Property CodecPrivate As Byte()
        Get
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' 把一帧画面编码为写入 <c>movi</c> 的原始载荷字节。
    ''' </summary>
    Public MustOverride Function Encode(bitmap As Bitmap) As Byte()

End Class
