# TFftAlgorithm
_namespace: [Microsoft.VisualBasic.DataMining](./index.md)_

Quick Fourier Transformation. 
 Some ideas to make the Discrete Fourier Transformation a bit quicker and implemented a lean version of the DFT algorithm.

> 
>  http://www.codeproject.com/Articles/590638/Quick-Fourier-Transformation
>  
>  离散傅里叶变换(discrete Fourier transform) 傅里叶分析方法是信号分析的最基本方法，傅里叶变换是傅里叶分析的核心，
>  通过它把信号从时间域变换到频率域，进而研究信号的频谱结构和变化规律。
>  在形式上，变换两端（时域和频域上）的序列是有限长的，而实际上这两组序列都应当被认为是离散周期信号的主值序列。
>  即使对有限长的离散信号作DFT，也应当将其看作其周期延拓的变换。在实际应用中通常采用快速傅里叶变换计算DFT。
>  
>  下面给出离散傅里叶变换的变换对： 对于N点序列，它的离散傅里叶变换（DFT）为 其中是自然对数的底数，是虚数单位单位。
>  通常以符号表示这一变换，即 离散傅里叶变换的逆变换（IDFT）为： 可以记为： 实际上，DFT和IDFT变换式中和式前面的
>  归一化系数并不重要。有时会将这两个系数都改成。
>  
>  
>  
>  The FFT produce frequency samples (or spectral bin). A frequency sample is a complex number with real and imaginary part. 
>  The imaginary part give the phase and the real part give the amplitude. We have to compute the magnitude in dB from this 
>  to produce a nice spectrogram. The magnitude of a spectral bin is simply the amount of energy for the corresponding 
>  frequency.
>  (FFT产生对波形的频率的采样，一个频率采样是一个复数集合，虚数部分记录了相位，实数部分则记录了振幅。我们必须计算声贝的大小从而产生一个比较不错的分析数据)
>  


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.TFftAlgorithm.#ctor(System.Int32)
```
使用本构造函数所创建的FFT对象，需要在后续的代码之中手动设置@``F:Microsoft.VisualBasic.DataMining.TFftAlgorithm.y``的值

|Parameter Name|Remarks|
|--------------|-------|
|order|@``F:Microsoft.VisualBasic.DataMining.TFftAlgorithm.y``的值的数目|


#### FourierTransformation
```csharp
Microsoft.VisualBasic.DataMining.TFftAlgorithm.FourierTransformation
```
Fourier transformation calculation of the Fourier components

#### InvDFT
```csharp
Microsoft.VisualBasic.DataMining.TFftAlgorithm.InvDFT
```
invers Fourier transformation, rebuild the signal in real numbers


### Properties

#### a
The real value is the cosinus part
#### b
The imag value is the sinus part
