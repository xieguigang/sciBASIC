' ============================================================================
' Program.vb - 声纹提取器控制台入口
'
' 用法:
'   VoicePrint <wavfile> [options]
'
' 选项:
'   --start <sec>          起始时间（秒），默认 0
'   --end <sec>            结束时间（秒），默认 -1（到文件末尾）
'   --channel <n>          声道索引（0=左, 1=右），默认 0
'   --dim <n>              目标维度，默认 192
'   --no-delta             不使用 Delta 系数
'   --no-delta-delta       不使用 Delta-Delta 系数
'   --no-cmvn              禁用 CMVN 归一化
'   --no-l2norm            禁用 L2 归一化
'   --mel-filters <n>      Mel 滤波器数量，默认 26
'   --frame-size <ms>      帧长（毫秒），默认 25
'   --frame-hop <ms>       帧移（毫秒），默认 10
'   --output <file>        输出文件路径（JSON），默认仅控制台输出
'   --info                 仅显示 WAV 文件信息
'
' 示例:
'   VoicePrint audio.wav --start 60 --end 120 --channel 0 --dim 192
'   VoicePrint audio.wav --info
'   VoicePrint audio.wav --start 0 --end 30 --output voiceprint.json
' ============================================================================

Imports System.IO
Imports System.Globalization

Module Program

    Private Const HelpText As String =
"VoicePrint Extractor - WAV 音频声纹向量提取工具

用法: VoicePrint <wavfile> [options]

选项:
  --start <sec>          起始时间（秒），默认 0
  --end <sec>            结束时间（秒），默认 -1（到文件末尾）
  --channel <n>          声道索引（0=左, 1=右），默认 0
  --dim <n>              目标维度，默认 192
  --no-delta             不使用 Delta 系数
  --no-delta-delta       不使用 Delta-Delta 系数
  --no-cmvn              禁用 CMVN 归一化
  --no-l2norm            禁用 L2 归一化
  --mel-filters <n>      Mel 滤波器数量，默认 26
  --frame-size <ms>      帧长（毫秒），默认 25
  --frame-hop <ms>       帧移（毫秒），默认 10
  --output <file>        输出文件路径（JSON），默认仅控制台输出
  --info                 仅显示 WAV 文件信息
  --help, -h             显示帮助

示例:
  VoicePrint audio.wav --start 60 --end 120 --channel 0 --dim 192
  VoicePrint audio.wav --info
  VoicePrint audio.wav --start 0 --end 30 --output voiceprint.json
"

    Sub Main(args As String())
        If args.Length = 0 Then
            Console.WriteLine(HelpText)
            Return
        End If

        ' 检查 --help
        For Each a As String In args
            If a = "--help" OrElse a = "-h" Then
                Console.WriteLine(HelpText)
                Return
            End If
        Next

        ' 第一个参数是 WAV 文件路径
        Dim wavPath As String = args(0)
        If Not File.Exists(wavPath) Then
            Console.WriteLine("[ERROR] File not found: " & wavPath)
            Return
        End If

        ' 解析选项
        Dim startTime As Double = 0.0
        Dim endTime As Double = -1.0
        Dim channelIndex As Integer = 0
        Dim targetDim As Integer = 192
        Dim useDelta As Boolean = True
        Dim useDeltaDelta As Boolean = True
        Dim applyCMVN As Boolean = True
        Dim l2Normalize As Boolean = True
        Dim numMelFilters As Integer = 26
        Dim frameSizeMs As Double = 25.0
        Dim frameHopMs As Double = 10.0
        Dim outputPath As String = Nothing
        Dim infoOnly As Boolean = False

        Dim i As Integer = 1
        Do While i < args.Length
            Select Case args(i)
                Case "--start"
                    i += 1
                    If i < args.Length Then Double.TryParse(args(i), NumberStyles.Any, CultureInfo.InvariantCulture, startTime)

                Case "--end"
                    i += 1
                    If i < args.Length Then Double.TryParse(args(i), NumberStyles.Any, CultureInfo.InvariantCulture, endTime)

                Case "--channel"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), channelIndex)

                Case "--dim"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), targetDim)

                Case "--no-delta"
                    useDelta = False

                Case "--no-delta-delta"
                    useDeltaDelta = False

                Case "--no-cmvn"
                    applyCMVN = False

                Case "--no-l2norm"
                    l2Normalize = False

                Case "--mel-filters"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), numMelFilters)

                Case "--frame-size"
                    i += 1
                    If i < args.Length Then Double.TryParse(args(i), NumberStyles.Any, CultureInfo.InvariantCulture, frameSizeMs)

                Case "--frame-hop"
                    i += 1
                    If i < args.Length Then Double.TryParse(args(i), NumberStyles.Any, CultureInfo.InvariantCulture, frameHopMs)

                Case "--output"
                    i += 1
                    If i < args.Length Then outputPath = args(i)

                Case "--info"
                    infoOnly = True

                Case Else
                    Console.WriteLine($"[WARN] Unknown option: {args(i)}")
            End Select
            i += 1
        Loop

        ' 显示文件信息
        Console.WriteLine("=== WAV File Info ===")
        Dim info = WavVoicePrintReader.GetWavInfo(wavPath)
        Console.WriteLine($"  Path:          {wavPath}")
        Console.WriteLine($"  Sample Rate:   {info.SampleRate} Hz")
        Console.WriteLine($"  Channels:      {info.Channels}")
        Console.WriteLine($"  Bits/Sample:   {info.BitsPerSample}")
        Console.WriteLine($"  Duration:      {info.DurationSec:F3}s")
        Console.WriteLine($"  Format:        {info.AudioFormat}")
        Console.WriteLine()

        If infoOnly Then Return

        ' 配置提取参数
        Dim options As New VoicePrintOptions With {
            .TargetDimension = targetDim,
            .UseDelta = useDelta,
            .UseDeltaDelta = useDeltaDelta,
            .ApplyCMVN = applyCMVN,
            .L2Normalize = l2Normalize,
            .NumMelFilters = numMelFilters,
            .FrameSizeMs = frameSizeMs,
            .FrameHopMs = frameHopMs
        }

        ' 显示提取配置
        Dim channelName As String = If(channelIndex = 0, "Left", If(channelIndex = 1, "Right", $"Channel {channelIndex}"))
        Console.WriteLine("=== Extraction Config ===")
        Console.WriteLine($"  Time Window:   {startTime:F2}s ~ {If(endTime < 0, "end", endTime.ToString("F2") & "s")}")
        Console.WriteLine($"  Channel:       {channelName} (index {channelIndex})")
        Console.WriteLine($"  Target Dim:    {targetDim}")
        Console.WriteLine($"  Use Delta:     {useDelta}")
        Console.WriteLine($"  Use Delta2:    {useDeltaDelta}")
        Console.WriteLine($"  CMVN:          {applyCMVN}")
        Console.WriteLine($"  L2 Normalize:  {l2Normalize}")
        Console.WriteLine($"  Mel Filters:   {numMelFilters}")
        Console.WriteLine($"  Frame:         {frameSizeMs}ms / hop {frameHopMs}ms")
        Console.WriteLine()

        ' 提取声纹
        Console.WriteLine("Extracting voiceprint...")
        Dim watch As New System.Diagnostics.Stopwatch()
        watch.Start()

        Dim result As VoicePrintResult
        Try
            result = WavVoicePrintReader.ExtractDetailed(
                wavPath, startTime, endTime, channelIndex, options)
        Catch ex As Exception
            Console.WriteLine("[ERROR] " & ex.Message)
            Return
        End Try

        watch.Stop()
        Console.WriteLine($"Done in {watch.ElapsedMilliseconds}ms")
        Console.WriteLine()
        Console.WriteLine("=== Result ===")
        Console.WriteLine($"  {result.ToString()}")
        Console.WriteLine()

        ' 输出向量
        Console.WriteLine("=== Voiceprint Vector ===")
        Dim vector As Double() = result.Vector
        For k As Integer = 0 To vector.Length - 1
            Console.WriteLine($"  [{k,3}] {vector(k).ToString("F6", CultureInfo.InvariantCulture)}")
        Next

        ' 写入 JSON 文件
        If outputPath IsNot Nothing Then
            WriteJson(outputPath, result, wavPath, startTime, endTime, channelIndex, info)
            Console.WriteLine()
            Console.WriteLine($"Vector saved to: {outputPath}")
        End If
    End Sub

    ''' <summary>
    ''' 将声纹向量写入 JSON 文件。
    ''' </summary>
    Private Sub WriteJson(path As String, result As VoicePrintResult,
                          wavPath As String, startTime As Double, endTime As Double,
                          channelIndex As Integer,
                          info As (SampleRate As Integer, Channels As Integer, BitsPerSample As Integer,
                                   DurationSec As Double, AudioFormat As String))
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("{")
        sb.AppendLine($"  ""wavFile"": ""{wavPath.Replace("\", "\\")}"",")
        sb.AppendLine($"  ""sampleRate"": {info.SampleRate},")
        sb.AppendLine($"  ""channels"": {info.Channels},")
        sb.AppendLine($"  ""bitsPerSample"": {info.BitsPerSample},")
        sb.AppendLine($"  ""durationSec"": {info.DurationSec.ToString("F3", CultureInfo.InvariantCulture)},")
        sb.AppendLine($"  ""audioFormat"": ""{info.AudioFormat}"",")
        sb.AppendLine($"  ""startTimeSec"": {startTime.ToString("F2", CultureInfo.InvariantCulture)},")
        sb.AppendLine($"  ""endTimeSec"": {If(endTime < 0, info.DurationSec.ToString("F2", CultureInfo.InvariantCulture), endTime.ToString("F2", CultureInfo.InvariantCulture))},")
        sb.AppendLine($"  ""channelIndex"": {channelIndex},")
        sb.AppendLine($"  ""numFrames"": {result.NumFrames},")
        sb.AppendLine($"  ""numMfcc"": {result.NumMfcc},")
        sb.AppendLine($"  ""dimension"": {result.Vector.Length},")
        sb.Append("  ""vector"": [")
        For i As Integer = 0 To result.Vector.Length - 1
            If i > 0 Then sb.Append(", ")
            sb.Append(result.Vector(i).ToString("F6", CultureInfo.InvariantCulture))
        Next
        sb.AppendLine("]")
        sb.AppendLine("}")

        File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8)
    End Sub

End Module
