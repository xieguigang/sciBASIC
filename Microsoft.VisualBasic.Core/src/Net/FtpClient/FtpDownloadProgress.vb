#Region "Microsoft.VisualBasic::39057b932da2859ee01f689b46d0a705, Microsoft.VisualBasic.Core\src\Net\FtpClient\FtpDownloadProgress.vb"

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

    '   Total Lines: 44
    '    Code Lines: 25 (56.82%)
    ' Comment Lines: 9 (20.45%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.53 KB


    '     Structure FtpDownloadProgress
    ' 
    '         Properties: BytesTransferred, Elapsed, ProgressPercentage, TotalBytes, TransferRateBytesPerSecond
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Net.FTP

    ' ===================== 下载进度 =====================

    ''' <summary>
    ''' 文件下载进度信息。
    ''' </summary>
    Public Structure FtpDownloadProgress

        ''' <summary>已传输字节数。</summary>
        Public ReadOnly Property BytesTransferred As Long

        ''' <summary>文件总字节数，-1 表示未知 (服务器不支持 SIZE 命令)。</summary>
        Public ReadOnly Property TotalBytes As Long

        ''' <summary>已耗时。</summary>
        Public ReadOnly Property Elapsed As TimeSpan

        Public Sub New(bytesTransferred As Long, totalBytes As Long, elapsed As TimeSpan)
            _BytesTransferred = bytesTransferred
            _TotalBytes = totalBytes
            _Elapsed = elapsed
        End Sub

        ''' <summary>进度百分比 (0-100)，TotalBytes 未知时返回 -1。</summary>
        Public ReadOnly Property ProgressPercentage As Double
            Get
                If TotalBytes <= 0 Then Return -1
                Return std.Round(BytesTransferred * 100.0 / TotalBytes, 1)
            End Get
        End Property

        ''' <summary>传输速率 (字节/秒)。</summary>
        Public ReadOnly Property TransferRateBytesPerSecond As Double
            Get
                If Elapsed.TotalSeconds <= 0 Then Return 0
                Return BytesTransferred / Elapsed.TotalSeconds
            End Get
        End Property

    End Structure
End Namespace
