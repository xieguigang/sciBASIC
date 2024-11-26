#Region "Microsoft.VisualBasic::92278d79f36264958ac5cde120a0bc8c, Data_science\Mathematica\SignalProcessing\SignalProcessing\Extensions.vb"

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

    '   Total Lines: 46
    '    Code Lines: 18 (39.13%)
    ' Comment Lines: 24 (52.17%)
    '    - Xml Docs: 79.17%
    ' 
    '   Blank Lines: 4 (8.70%)
    '     File Size: 2.17 KB


    ' Module Extensions
    ' 
    '     Function: AsSignal, ShannonTransferRate, SNRatio
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

<HideModuleName>
Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsSignal(signals As IEnumerable(Of TimeSignal)) As Signal
        Return New Signal(signals)
    End Function

    ''' <summary>
    ''' 信噪比就是信号的平均功率和噪声的平均功率之比，即：S/N。
    ''' 用分贝（dB）作为度量单位，即信噪比（dB）= 10 * log10(S/N) (dB)
    ''' 例如：当S/N=10时，信噪比为10dB；当S/N=1000时，信噪比为30dB。
    ''' </summary>
    ''' <param name="signal"></param>
    ''' <param name="noise"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function SNRatio(signal As Double, noise As Double) As Double
        Return 10 * std.Log10(If(noise <= 0.0, Double.MaxValue, signal / noise))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetNoise(signalPower As Double, snrDb As Double) As Double
        Return signalPower / std.Pow(10, snrDb / 10)
    End Function

    ''' <summary>
    ''' ### 香农公式
    ''' 
    ''' 香农(Shannon)用信息论的理论推导出了带宽受限且有高斯白噪声干扰的信道的极限、无差错的信息传输速率。
    ''' 信道的极限信息传输速率 C 可表达为
    ''' 
    ''' ``C = W log2(1+S/N) b/s``
    '''
    ''' 信道的带宽或信道中的信噪比越大，则信息的极限传输速率就越高。 只要信息传输速率低于信道的极限信息传输速率，就一定可以找到某种办法来实现无差错的传输。 实际信道上能够达到的信息传输速率要比香农的极限传输速率低不少。
    ''' </summary>
    ''' <param name="bandWidth">为信道的带宽（以 Hz 为单位）</param>
    ''' <param name="signal">为信道内所传信号的平均功率</param>
    ''' <param name="noise">为信道内部的高斯噪声功率</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ShannonTransferRate(bandWidth As Double, signal As Double, noise As Double) As Double
        Return bandWidth * std.Log(1 + signal / noise, 2)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="analogSignal"></param>
    ''' <param name="samplingRate"></param>
    ''' <param name="bitDepth">
    ''' `bitDepth`在ADC（模数转换）中指的是用于量化模拟信号的位数。它决定了ADC的分辨率，即能够区分不同模拟信号强度的能力。
    ''' `bitDepth`的取值范围通常取决于特定的应用和所需的精度。
    ''' 
    ''' 常见的`bitDepth`取值包括：
    ''' 
    ''' - **8位（8-bit）**：提供256个不同的量化级别。这在一些低端或对精度要求不高的应用中使用，例如早期的计算机声卡。
    ''' - **10位（10-bit）**：提供1024个量化级别。这种精度在消费级和专业音频设备中越来越常见。
    ''' - **12位（12-bit）**：提供4096个量化级别。常用于医疗设备和一些高端音频设备。
    ''' - **16位（16-bit）**：提供65536个量化级别。这是CD音频的标准，也被广泛用于专业音频和数字音频工作站。
    ''' - **24位（24-bit）**：提供16777216个量化级别。这种高精度常用于专业音频录音和制作，因为它提供了更高的动态范围和更低的噪声水平。
    ''' - **32位（32-bit）**：提供超过43亿个量化级别。这种超高精度通常用于专业音频处理和科学测量。
    ''' 
    ''' 在音频应用中，16位和24位是最常见的`bitDepth`设置。更高的`bitDepth`可以提供更好的声音质量和更大的动态范围，
    ''' 但也意味着更大的数据文件和处理需求。选择合适的`bitDepth`取决于具体的应用场景和所需的音频质量。
    ''' </param>
    ''' <returns></returns>
    Public Iterator Function AnalogDigitConvert(analogSignal As List(Of Single),
                                                samplingRate As Integer,
                                                Optional bitDepth As Integer = 8) As IEnumerable(Of Integer)
        ' 量化级别
        Dim levels = std.Pow(2, bitDepth)
        ' 假设模拟信号以固定的时间间隔产生
        Dim timeInterval = 1.0 / samplingRate

        ' 对模拟信号进行采样和量化
        For i = 0 To analogSignal.Count - 1 Step timeInterval
            ' 根据采样率选择采样点
            Dim sample = analogSignal(CInt(i))
            ' 量化过程
            Dim digitalSample = CInt(std.Round(sample * (levels - 1)))

            Yield digitalSample
        Next
    End Function
End Module
