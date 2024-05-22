#Region "Microsoft.VisualBasic::5570781cb8fb7cf13ce91a40685ef86d, Data_science\Mathematica\SignalProcessing\SignalProcessing\FFT\DFT.vb"

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

    '   Total Lines: 135
    '    Code Lines: 56 (41.48%)
    ' Comment Lines: 59 (43.70%)
    '    - Xml Docs: 79.66%
    ' 
    '   Blank Lines: 20 (14.81%)
    '     File Size: 5.74 KB


    '     Class TFftAlgorithm
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: FourierTransformation, InvDFT
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace FFT

    ''' <summary>
    ''' Quick Fourier Transformation. 
    ''' Some ideas to make the Discrete Fourier Transformation a bit quicker and implemented a lean version of the DFT algorithm.
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/590638/Quick-Fourier-Transformation
    ''' 
    ''' 离散傅里叶变换(discrete Fourier transform) 傅里叶分析方法是信号分析的最基本方法，傅里叶变换是傅里叶分析的核心，
    ''' 通过它把信号从时间域变换到频率域，进而研究信号的频谱结构和变化规律。
    ''' 在形式上，变换两端（时域和频域上）的序列是有限长的，而实际上这两组序列都应当被认为是离散周期信号的主值序列。
    ''' 即使对有限长的离散信号作DFT，也应当将其看作其周期延拓的变换。在实际应用中通常采用快速傅里叶变换计算DFT。
    ''' 
    ''' 下面给出离散傅里叶变换的变换对： 对于N点序列，它的离散傅里叶变换（DFT）为 其中是自然对数的底数，是虚数单位单位。
    ''' 通常以符号表示这一变换，即 离散傅里叶变换的逆变换（IDFT）为： 可以记为： 实际上，DFT和IDFT变换式中和式前面的
    ''' 归一化系数并不重要。有时会将这两个系数都改成。
    ''' 
    ''' 
    ''' 
    ''' The FFT produce frequency samples (or spectral bin). A frequency sample is a complex number with real and imaginary part. 
    ''' The imaginary part give the phase and the real part give the amplitude. We have to compute the magnitude in dB from this 
    ''' to produce a nice spectrogram. The magnitude of a spectral bin is simply the amount of energy for the corresponding 
    ''' frequency.
    ''' (FFT产生对波形的频率的采样，一个频率采样是一个复数集合，虚数部分记录了相位，实数部分则记录了振幅。我们必须计算声贝的大小从而产生一个比较不错的分析数据)
    ''' </remarks>
    Public Class TFftAlgorithm

        Dim N As Integer

        Public y As Double()
        Public xw As Double()

        ''' <summary>
        ''' The real value is the cosinus part
        ''' </summary>
        ''' <remarks>
        ''' Compute magnitudes
        ''' 
        ''' Now we can compute the magnitude from complex values. This is done with the good old Pythagorean theorem. 
        ''' Each complex number can be represented in a two-dimensional space. 
        ''' 
        ''' The real part is a, and the imaginary part is b.
        ''' 
        ''' Magnitudes are stored in a two dimentional array magnitudes[x,y] where x is the nth FFT performed by 
        ''' SampleTagger and y is the nth magnitude in range [0,fft_size/2]
        ''' </remarks>
        Public a As Double()
        ''' <summary>
        ''' The imag value is the sinus part
        ''' </summary>
        ''' <remarks></remarks>
        Public b As Double()
        Public sine As Double()
        Public cosine As Double()

        ''' <summary>
        ''' 使用本构造函数所创建的FFT对象，需要在后续的代码之中手动设置<see cref="y"></see>的值
        ''' </summary>
        ''' <param name="order"><see cref="y"></see>的值的数目</param>
        ''' <remarks></remarks>
        Public Sub New(order As Integer)
            Dim k As Integer
            N = order
            y = New Double(N) {}
            a = New Double(N) {}
            b = New Double(N) {}
            xw = New Double(N) {}
            sine = New Double(N) {}
            cosine = New Double(N) {}

            cosine(0) = 1.0
            ' we don't have to calculate cos(0) = 1
            sine(0) = 0
            '                        and sin(0) = 0
            For k = 1 To N - 1
                '  init vectors of unit circle
                cosine(k) = std.Cos((2.0 * std.PI * CDbl(k) / CDbl(N)))
                sine(k) = std.Sin((2.0 * std.PI * CDbl(k) / CDbl(N)))
            Next
        End Sub

        Sub New(WaveFormData As Double())
            Call Me.New(WaveFormData.Length)

            Me.y = DirectCast(WaveFormData.Clone, Double())
        End Sub

        ''' <summary>
        ''' Fourier transformation calculation of the Fourier components
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub FourierTransformation()

            If N > 0 Then

                For k As Integer = 0 To N - 1
                    a(k) = 0
                    b(k) = 0

                    For n__1 As Integer = 0 To (N - 1) - 1
                        a(k) = a(k) + ((cosine((k * n__1) Mod N) * y(n__1)))
                        b(k) = b(k) + ((sine((k * n__1) Mod N) * y(n__1)))
                    Next

                    a(k) = a(k) / N * 2
                    b(k) = b(k) / N * 2
                Next

                a(0) = a(0) / 2
                b(0) = b(0) / 2
            End If

        End Sub

        ''' <summary>
        ''' invers Fourier transformation, rebuild the signal in real numbers
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub InvDFT()

            For k As Integer = 0 To N - 1

                xw(k) = 0

                For i As Integer = 0 To 29
                    ' we only take the first 30 fourier components
                    xw(k) = xw(k) + (a(i) * std.Cos(2.0 * std.PI * CDbl(i * k) / CDbl(N)) + b(i) * std.Sin(2.0 * std.PI * CDbl(i * k) / CDbl(N)))
                Next
            Next
        End Sub
    End Class
End Namespace
