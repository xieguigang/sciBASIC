#Region "Microsoft.VisualBasic::190b8cac841b1c0fe982114247883636, Data_science\Mathematica\SignalProcessing\wav\wav\VoicePrint\VoicePrint.vb"

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

    '   Total Lines: 139
    '    Code Lines: 93 (66.91%)
    ' Comment Lines: 31 (22.30%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (10.79%)
    '     File Size: 5.92 KB


    ' Module VoicePrint
    ' 
    '     Function: Extract
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Globalization

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

Public Module VoicePrint

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

    Public Function Extract(wavPath As String, Optional startTime As Double = 0.0,
        Optional endTime As Double = -1.0,
        Optional channelIndex As Integer = 0,
        Optional targetDim As Integer = 192,
        Optional useDelta As Boolean = True,
        Optional useDeltaDelta As Boolean = True,
        Optional applyCMVN As Boolean = True,
        Optional l2Normalize As Boolean = True,
        Optional numMelFilters As Integer = 26,
        Optional frameSizeMs As Double = 25.0,
        Optional frameHopMs As Double = 10.0) As VoicePrintResult

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
            Return Nothing
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

        Return result
    End Function
End Module

