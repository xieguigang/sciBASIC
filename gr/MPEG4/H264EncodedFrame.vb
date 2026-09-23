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
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

''' <summary>
''' 一个已编码的采样：AVCC 格式（4 字节大端长度前缀）的 NAL 数据，以及它相对解码时间的显示时间偏移。
''' </summary>
''' <remarks>
''' B 帧的存在使<b>解码顺序</b>与<b>显示顺序</b>分离：为使用「未来锚点」作为参考，锚点必须先于它前面的
''' B 帧被编码，因此编码器按解码顺序输出采样，而播放顺序由本类的
''' <see cref="compositionOffset"/>（显示时间 − 解码时间，单位为帧）在容器层
''' 通过 <c>ctts</c> 还原。
''' </remarks>
Friend Class H264EncodedFrame

    ''' <summary>AVCC 格式的采样数据（4 字节大端长度前缀 + NAL）</summary>
    Friend ReadOnly Property data As Byte()

    ''' <summary>
    ''' 合成时间偏移（显示时间 − 解码时间，单位为帧）。
    ''' </summary>
    ''' <remarks>
    ''' 编码器整体把显示时间平移了 1 帧，使该偏移恒为非负——<c>ctts</c> 的 0 版本条目是无符号的，
    ''' 出现负值需要改用带符号的版本，兼容性更差。平移只是给整条视频轨加了一个固定延迟，不影响播放。
    ''' </remarks>
    Friend ReadOnly Property compositionOffset As Integer

    Friend Sub New(data As Byte(), compositionOffset As Integer)
        Me.data = data
        Me.compositionOffset = compositionOffset
    End Sub

End Class
