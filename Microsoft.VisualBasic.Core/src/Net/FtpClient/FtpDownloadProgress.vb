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