' ============================================================================
' WavVoicePrintReader.vb - WAV 文件声纹提取辅助类
'
' 封装基于上传的 WAV 模块读取指定时间窗口、指定声道音频数据，
' 并调用 VoicePrintExtractor 提取声纹向量的完整流程。
'
' 依赖：
'   - Microsoft.VisualBasic.Data.IO.BinaryDataReader（来自 sciBASIC 框架）
'   - Microsoft.VisualBasic.Data.Wave.WaveFile（来自上传的 WAV 模块）
'   - VoicePrintExtractor（本项目的核心提取器）
' ============================================================================

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports std = System.Math

''' <summary>
''' 基于 WAV 文件的声纹提取辅助类。
''' 提供一次性调用接口，自动处理文件打开、声道选取、时间窗口裁剪。
''' </summary>
Public Class WavVoicePrintReader

    ''' <summary>
    ''' 从 WAV 文件中提取指定时间窗口内指定声道的声纹向量。
    ''' </summary>
    ''' <param name="wavPath">WAV 文件路径。</param>
    ''' <param name="startTimeSec">起始时间（秒），例如 60.0 表示从第 1 分钟开始。</param>
    ''' <param name="endTimeSec">结束时间（秒），例如 120.0 表示到第 2 分钟结束。传 -1 表示到文件末尾。</param>
    ''' <param name="channelIndex">声道索引：0=左声道，1=右声道。</param>
    ''' <param name="options">声纹提取参数，为 Nothing 时使用默认值（192 维）。</param>
    ''' <returns>声纹向量。</returns>
    Public Shared Function Extract(
        wavPath As String,
        startTimeSec As Double,
        endTimeSec As Double,
        channelIndex As Integer,
        Optional options As VoicePrintOptions = Nothing) As Double()

        If options Is Nothing Then options = New VoicePrintOptions()
        If Not File.Exists(wavPath) Then
            Throw New FileNotFoundException("WAV file not found: " & wavPath)
        End If

        ' 使用懒加载模式打开 WAV 文件（仅读取所需区段，节省内存）
        Using reader As New BinaryDataReader(File.OpenRead(wavPath))
            Dim wav As WaveFile = WaveFile.Open(reader, lazy:=True)

            Dim sampleRate As Integer = wav.fmt.SampleRate
            Dim numChannels As Integer = wav.fmt.channels

            ' 验证声道索引
            If channelIndex < 0 OrElse channelIndex >= numChannels Then
                Throw New ArgumentException(
                    $"Channel index {channelIndex} is out of range. File has {numChannels} channel(s).")
            End If

            ' 计算总采样数（data chunk 字节数 / 每帧字节数）
            Dim totalSamples As Integer = wav.data.chunkSize \ wav.fmt.sampleSizeBytes

            ' 计算时间窗口对应的采样范围
            Dim startSample As Integer = CInt(std.Max(0, startTimeSec * sampleRate))
            Dim endSample As Integer

            If endTimeSec < 0 Then
                endSample = totalSamples
            Else
                endSample = CInt(std.Min(totalSamples, endTimeSec * sampleRate))
            End If

            If endSample <= startSample Then
                Throw New ArgumentException(
                    $"Invalid time window: {startTimeSec}s ~ {endTimeSec}s. " &
                    $"Audio duration is {totalSamples / CDbl(sampleRate):F2}s.")
            End If

            Dim numSamples As Integer = endSample - startSample

            ' 从 WAV 文件加载指定范围的采样数据
            Dim samples As IEnumerable(Of Sample) =
                wav.data.LoadSamples(startSample, numSamples)

            ' 提取目标声道数据
            Dim channelData As New List(Of Single)(numSamples)
            For Each s As Sample In samples
                channelData.Add(s.channels(channelIndex))
            Next

            ' 调用提取器
            Dim extractor As New VoicePrintExtractor()
            Return extractor.Extract(channelData.ToArray(), sampleRate, options)
        End Using
    End Function

    ''' <summary>
    ''' 从 WAV 文件中提取指定时间窗口内指定声道的声纹向量（返回详细结果）。
    ''' </summary>
    Public Shared Function ExtractDetailed(
        wavPath As String,
        startTimeSec As Double,
        endTimeSec As Double,
        channelIndex As Integer,
        Optional options As VoicePrintOptions = Nothing) As VoicePrintResult

        If options Is Nothing Then options = New VoicePrintOptions()
        If Not File.Exists(wavPath) Then
            Throw New FileNotFoundException("WAV file not found: " & wavPath)
        End If

        Using reader As New BinaryDataReader(File.OpenRead(wavPath))
            Dim wav As WaveFile = WaveFile.Open(reader, lazy:=True)

            Dim sampleRate As Integer = wav.fmt.SampleRate
            Dim numChannels As Integer = wav.fmt.channels

            If channelIndex < 0 OrElse channelIndex >= numChannels Then
                Throw New ArgumentException(
                    $"Channel index {channelIndex} is out of range. File has {numChannels} channel(s).")
            End If

            Dim totalSamples As Integer = wav.data.chunkSize \ wav.fmt.sampleSizeBytes

            Dim startSample As Integer = CInt(std.Max(0, startTimeSec * sampleRate))
            Dim endSample As Integer

            If endTimeSec < 0 Then
                endSample = totalSamples
            Else
                endSample = CInt(std.Min(totalSamples, endTimeSec * sampleRate))
            End If

            If endSample <= startSample Then
                Throw New ArgumentException(
                    $"Invalid time window: {startTimeSec}s ~ {endTimeSec}s. " &
                    $"Audio duration is {totalSamples / CDbl(sampleRate):F2}s.")
            End If

            Dim numSamples As Integer = endSample - startSample

            Dim samples As IEnumerable(Of Sample) =
                wav.data.LoadSamples(startSample, numSamples)

            Dim channelData As New List(Of Single)(numSamples)
            For Each s As Sample In samples
                channelData.Add(s.channels(channelIndex))
            Next

            Dim extractor As New VoicePrintExtractor()
            Return extractor.ExtractDetailed(channelData.ToArray(), sampleRate, options)
        End Using
    End Function

    ''' <summary>
    ''' 获取 WAV 文件的格式信息（不提取声纹）。
    ''' </summary>
    Public Shared Function GetWavInfo(wavPath As String) As (SampleRate As Integer, Channels As Integer,
                                                              BitsPerSample As Integer, DurationSec As Double,
                                                              AudioFormat As String)
        Using reader As New BinaryDataReader(File.OpenRead(wavPath))
            Dim wav As WaveFile = WaveFile.Open(reader, lazy:=True)
            Dim totalSamples As Integer = wav.data.chunkSize \ wav.fmt.sampleSizeBytes
            Return (wav.fmt.SampleRate, wav.fmt.channels, wav.fmt.BitsPerSample,
                    totalSamples / CDbl(wav.fmt.SampleRate),
                    wav.fmt.effectiveAudioFormat.ToString())
        End Using
    End Function

End Class
