#Region "Microsoft.VisualBasic::419aabf1c38f6e9c7a898c7b95daba63, Data_science\Mathematica\SignalProcessing\SignalProcessing\Extensions.vb"

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
End Module
