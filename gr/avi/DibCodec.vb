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
''' 未压缩的 32 位 top-down DIB 编码器（fourCC <c>'DIB '</c>）。
''' </summary>
''' <remarks>
''' 这是改造前 <see cref="AVIStream"/> 的原始输出方式，保留下来作为兼容性回退：
''' 选择该方式即可得到与旧版本完全一致的未压缩画面。
''' </remarks>
Public Class DibCodec : Inherits AviVideoCodec

    Sub New(fps As Integer, width As Integer, height As Integer)
        MyBase.New(fps, width, height)
    End Sub

    Public Overrides ReadOnly Property FourCC As String
        Get
            Return "DIB "
        End Get
    End Property

    Public Overrides ReadOnly Property BitCount As Short
        Get
            Return 32
        End Get
    End Property

    ''' <summary>
    ''' 未压缩 DIB 使用负高度表示 top-down 扫描顺序。
    ''' </summary>
    Public Overrides ReadOnly Property HeightSign As Integer
        Get
            Return -1
        End Get
    End Property

    Public Overrides ReadOnly Property SuggestedBufferSize As Integer
        Get
            Return width * height * 4 + 8
        End Get
    End Property

    Public Overrides Function Encode(bitmap As Bitmap) As Byte()
        Call validateFrameSize(bitmap)

        Return PixelData.ToBgraBytes(bitmap)
    End Function

End Class
