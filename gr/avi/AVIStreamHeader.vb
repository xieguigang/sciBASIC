#Region "Microsoft.VisualBasic::9fec9d842fa471035a4913826dec0a0b, gr\avi\AVIStreamHeader.vb"

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

    '   Total Lines: 104
    '    Code Lines: 35
    ' Comment Lines: 60
    '   Blank Lines: 9
    '     File Size: 2.95 KB


    ' Class AVIStreamHeader
    ' 
    '     Properties: bottom, cb, dwFlags, dwInitialFrames, dwLength
    '                 dwQuality, dwRate, dwSampleSize, dwScale, dwStart
    '                 dwSuggestedBufferSize, fccHandler, fccType, left, right
    '                 top, wLanguage, wPriority
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Write
    ' 
    ' Enum StreamTypes
    ' 
    '     auds, mids, txts, vids
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Public Class AVIStreamHeader

    Public Const Magic As String = "strh"

    ''' <summary>
    ''' 本数据结构的大小，不包括最初的8个字节（fcc和cb两个域）
    ''' </summary>
    ''' <returns></returns>
    Public Property cb As Integer
    Public Property fccType As StreamTypes = StreamTypes.vids
    ''' <summary>
    ''' 指定流的处理者，对于音视频来说就是解码器
    ''' </summary>
    ''' <returns></returns>
    Public Property fccHandler As String
    ''' <summary>
    ''' 标记：是否允许这个流输出？调色板是否变化？
    ''' </summary>
    ''' <returns></returns>
    Public Property dwFlags As Integer
    ''' <summary>
    ''' 流的优先级（当有多个相同类型的流时优先级最高的为默认流）
    ''' </summary>
    ''' <returns></returns>
    Public Property wPriority As Short
    Public Property wLanguage As Short
    ''' <summary>
    ''' 为交互格式指定初始帧数
    ''' </summary>
    ''' <returns></returns>
    Public Property dwInitialFrames As Integer
    ''' <summary>
    ''' 这个流使用的时间尺度
    ''' </summary>
    ''' <returns></returns>
    Public Property dwScale As Integer
    Public Property dwRate As Integer
    ''' <summary>
    ''' 流的开始时间
    ''' </summary>
    ''' <returns></returns>
    Public Property dwStart As Integer
    ''' <summary>
    ''' 流的长度（单位与dwScale和dwRate的定义有关）
    ''' </summary>
    ''' <returns></returns>
    Public Property dwLength As Integer
    ''' <summary>
    ''' 读取这个流数据建议使用的缓存大小
    ''' </summary>
    ''' <returns></returns>
    Public Property dwSuggestedBufferSize As Integer
    ''' <summary>
    ''' 流数据的质量指标（0 ~ 10,000）
    ''' </summary>
    ''' <returns></returns>
    Public Property dwQuality As Integer
    ''' <summary>
    ''' Sample的大小
    ''' </summary>
    ''' <returns></returns>
    Public Property dwSampleSize As Integer

#Region "指定这个流（视频流或文字流）在视频主窗口中的显示位置"
    ' 视频主窗口由AVIMAINHEADER结构中的dwWidth和dwHeight决定
    Public Property left As Short
    Public Property top As Short
    Public Property right As Short
    Public Property bottom As Short
#End Region

    Dim stream As AVIStream

    Sub New(stream As AVIStream)
        Me.stream = stream
    End Sub

    Public Sub Write(stream As UInt8Array)

    End Sub

End Class

''' <summary>
''' 流的类型
''' </summary>
Public Enum StreamTypes
    ''' <summary>
    ''' （音频流）
    ''' </summary>
    auds
    ''' <summary>
    ''' （视频流）
    ''' </summary>
    vids
    ''' <summary>
    ''' （MIDI流）
    ''' </summary>
    mids
    ''' <summary>
    ''' （文字流）
    ''' </summary>
    txts
End Enum
