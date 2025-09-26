#Region "Microsoft.VisualBasic::9ecf8f46145459fb43bb95d4ddd82522, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\ReinhardColorNormalization.vb"

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

    '   Total Lines: 193
    '    Code Lines: 115 (59.59%)
    ' Comment Lines: 51 (26.42%)
    '    - Xml Docs: 50.98%
    ' 
    '   Blank Lines: 27 (13.99%)
    '     File Size: 10.22 KB


    '     Class ReinhardColorNormalization
    ' 
    '         Function: RGBToLab
    ' 
    '         Sub: ApplyTransformation, CalculateStats, LabToRGB, Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports std = System.Math

Namespace Drawing2D.HeatMap

    ''' <summary>
    ''' The Reinhard Color Normalization algorithm, developed by Reinhard et al., is a classical method 
    ''' for color transfer between images. Its core principles and implementation are as follows:
    ''' 
    ''' ### 1 Fundamental Principle  
    ''' 
    ''' The algorithm leverages the **LAB color space**, where the L (lightness), A (green-red), and B 
    ''' (blue-yellow) channels are statistically uncorrelated. This independence allows for separate processing 
    ''' of each channel. The core idea involves deriving a **linear transformation** based on statistical 
    ''' analysis (mean and standard deviation) of the source and target images. By aligning the color 
    ''' distributions of the source image to match those of the target image, effective color migration 
    ''' is achieved .
    ''' 
    ''' ### 2 Implementation Approach  
    ''' 
    ''' In practical applications (e.g., MATLAB), the algorithm follows these steps:  
    ''' 
    ''' 1. **Color Space Conversion**: Convert both source and target images from RGB to LAB using functions like `rgb2lab` .  
    ''' 2. **Statistical Calculation**: Compute the mean (μ) and standard deviation (σ) for each LAB channel in both images.  
    ''' 3. **Linear Transformation**: For each channel, apply the transformation:  
    ''' 
    ''' \[
    ''' \text{output\_channel} = \left( \frac{\text{source\_channel} - \mu_{\text{source}}}{\sigma_{\text{source}}} \right) \times \sigma_{\text{target}} + \mu_{\text{target}}
    ''' \]  
    ''' 
    ''' This scales and shifts the source channel's distribution to match the target's statistics.  
    ''' 
    ''' 4. **Reconversion**: Convert the transformed LAB image back to RGB using `lab2rgb` to produce the final color-normalized result .  
    ''' 
    ''' ### 3 Key Advantages  
    ''' 
    ''' - **Channel Independence**: Processing LAB channels separately avoids cross-channel interference, enhancing accuracy .  
    ''' - **Simplicity**: The linear transformation is computationally efficient and easy to implement.  
    ''' - **Effectiveness**: Preserves structural details while adapting colors, making it suitable for 
    '''                      applications like photo stylization and medical imaging.  
    '''
    ''' </summary>
    Public Class ReinhardColorNormalization

        Public Shared Sub Normalize(ByRef targetImage As BitmapBuffer, ByRef sourceImage As BitmapBuffer)
            ' 将图像转换为LAB颜色空间
            Dim targetLab As Double(,) = RGBToLab(targetImage)
            Dim sourceLab As Double(,) = RGBToLab(sourceImage)

            ' 计算统计量：均值(mean)和标准差(std)
            Dim tarMean(2) As Double, tarStd(2) As Double
            Dim srcMean(2) As Double, srcStd(2) As Double
            CalculateStats(targetLab, tarMean, tarStd)
            CalculateStats(sourceLab, srcMean, srcStd)

            ' 应用Reinhard颜色变换
            ApplyTransformation(targetLab, tarMean, tarStd, srcMean, srcStd)

            ' 将LAB转换回RGB并更新目标图像
            LabToRGB(targetImage, targetLab)
        End Sub

        ' RGB转LAB颜色空间
        Private Shared Function RGBToLab(bmp As BitmapBuffer) As Double(,)
            Dim width As Integer = bmp.Width
            Dim height As Integer = bmp.Height
            Dim labPixels(width * height - 1, 2) As Double
            Dim bytes As Integer = bmp.Length
            Dim rgbValues() As Byte = bmp.RawBuffer
            Dim index As Integer = 0

            For yi As Integer = 0 To height - 1
                For xi As Integer = 0 To width - 1
                    Dim B As Double = rgbValues(index) / 255.0
                    Dim G As Double = rgbValues(index + 1) / 255.0
                    Dim R As Double = rgbValues(index + 2) / 255.0
                    index += 3

                    ' 将sRGB转换到CIE XYZ空间（假设sRGB，D65白点）
                    Dim rLinear As Double = If(R <= 0.04045, R / 12.92, std.Pow((R + 0.055) / 1.055, 2.4))
                    Dim gLinear As Double = If(G <= 0.04045, G / 12.92, std.Pow((G + 0.055) / 1.055, 2.4))
                    Dim bLinear As Double = If(B <= 0.04045, B / 12.92, std.Pow((B + 0.055) / 1.055, 2.4))

                    Dim X As Double = rLinear * 0.4124 + gLinear * 0.3576 + bLinear * 0.1805
                    Dim Y As Double = rLinear * 0.2126 + gLinear * 0.7152 + bLinear * 0.0722
                    Dim Z As Double = rLinear * 0.0193 + gLinear * 0.1192 + bLinear * 0.9505

                    ' 将XYZ转换到CIE L*a*b*空间（参考白点为D65）
                    X /= 0.95047
                    Z /= 1.08883

                    Dim fx As Double = If(X > 0.008856, std.Pow(X, 1.0 / 3.0), (7.787 * X) + (16.0 / 116.0))
                    Dim fy As Double = If(Y > 0.008856, std.Pow(Y, 1.0 / 3.0), (7.787 * Y) + (16.0 / 116.0))
                    Dim fz As Double = If(Z > 0.008856, std.Pow(Z, 1.0 / 3.0), (7.787 * Z) + (16.0 / 116.0))

                    Dim Lc As Double = If(Y > 0.008856, (116.0 * std.Pow(Y, 1.0 / 3.0)) - 16.0, 903.3 * Y)
                    Dim ac As Double = 500.0 * (fx - fy)
                    Dim bc As Double = 200.0 * (fy - fz)

                    labPixels(yi * width + xi, 0) = Lc
                    labPixels(yi * width + xi, 1) = ac
                    labPixels(yi * width + xi, 2) = bc
                Next
            Next

            Return labPixels
        End Function

        ' 计算均值(Mean)和标准差(Standard Deviation)
        Private Shared Sub CalculateStats(labPixels As Double(,), ByRef means As Double(), ByRef stds As Double())
            Dim count As Integer = labPixels.GetLength(0)
            Dim sumL As Double = 0, sumA As Double = 0, sumB As Double = 0
            For i As Integer = 0 To count - 1
                sumL += labPixels(i, 0)
                sumA += labPixels(i, 1)
                sumB += labPixels(i, 2)
            Next
            means(0) = sumL / count
            means(1) = sumA / count
            means(2) = sumB / count

            Dim sumVarL As Double = 0, sumVarA As Double = 0, sumVarB As Double = 0
            For i As Integer = 0 To count - 1
                sumVarL += std.Pow(labPixels(i, 0) - means(0), 2)
                sumVarA += std.Pow(labPixels(i, 1) - means(1), 2)
                sumVarB += std.Pow(labPixels(i, 2) - means(2), 2)
            Next
            stds(0) = std.Sqrt(sumVarL / count)
            stds(1) = std.Sqrt(sumVarA / count)
            stds(2) = std.Sqrt(sumVarB / count)
        End Sub

        ' 应用颜色变换：调整目标图像的统计量以匹配源图像
        Private Shared Sub ApplyTransformation(ByRef targetLab As Double(,), tarMean() As Double, tarStd() As Double, srcMean() As Double, srcStd() As Double)
            Dim count As Integer = targetLab.GetLength(0)
            For i As Integer = 0 To count - 1
                targetLab(i, 0) = (targetLab(i, 0) - tarMean(0)) * (srcStd(0) / tarStd(0)) + srcMean(0) ' L通道
                targetLab(i, 1) = (targetLab(i, 1) - tarMean(1)) * (srcStd(1) / tarStd(1)) + srcMean(1) ' A通道
                targetLab(i, 2) = (targetLab(i, 2) - tarMean(2)) * (srcStd(2) / tarStd(2)) + srcMean(2) ' B通道
            Next
        End Sub

        ' LAB转RGB颜色空间
        Private Shared Sub LabToRGB(ByRef bmp As BitmapBuffer, labPixels As Double(,))
            Dim width As Integer = bmp.Width
            Dim height As Integer = bmp.Height
            Dim bytes As Integer = bmp.Length
            Dim rgbValues() As Byte = bmp.RawBuffer
            Dim index As Integer = 0

            For yi As Integer = 0 To height - 1
                For xi As Integer = 0 To width - 1
                    Dim L As Double = labPixels(yi * width + xi, 0)
                    Dim a As Double = labPixels(yi * width + xi, 1)
                    Dim b As Double = labPixels(yi * width + xi, 2)

                    ' 将L*a*b*转换回XYZ
                    Dim fy As Double = (L + 16.0) / 116.0
                    Dim fx As Double = a / 500.0 + fy
                    Dim fz As Double = fy - b / 200.0

                    Dim x3 As Double = fx * fx * fx
                    Dim y3 As Double = fy * fy * fy
                    Dim z3 As Double = fz * fz * fz

                    Dim X As Double = If(x3 > 0.008856, x3, (fx - 16.0 / 116.0) / 7.787) * 0.95047
                    Dim Y As Double = If(y3 > 0.008856, y3, (fy - 16.0 / 116.0) / 7.787)
                    Dim Z As Double = If(z3 > 0.008856, z3, (fz - 16.0 / 116.0) / 7.787) * 1.08883

                    ' 将XYZ转换回线性RGB
                    Dim rLinear As Double = X * 3.2406 + Y * -1.5372 + Z * -0.4986
                    Dim gLinear As Double = X * -0.9689 + Y * 1.8758 + Z * 0.0415
                    Dim bLinear As Double = X * 0.0557 + Y * -0.204 + Z * 1.057

                    ' 将线性RGB转换回sRGB（伽马校正）
                    rLinear = If(rLinear <= 0.0031308, 12.92 * rLinear, (1.055 * std.Pow(rLinear, 1.0 / 2.4)) - 0.055)
                    gLinear = If(gLinear <= 0.0031308, 12.92 * gLinear, (1.055 * std.Pow(gLinear, 1.0 / 2.4)) - 0.055)
                    bLinear = If(bLinear <= 0.0031308, 12.92 * bLinear, (1.055 * std.Pow(bLinear, 1.0 / 2.4)) - 0.055)

                    ' 确保RGB值在0-1范围内，然后转换为0-255
                    Dim Rb As Byte = CByte(std.Max(0, std.Min(255, rLinear * 255)))
                    Dim Gb As Byte = CByte(std.Max(0, std.Min(255, gLinear * 255)))
                    Dim Bb As Byte = CByte(std.Max(0, std.Min(255, bLinear * 255)))

                    rgbValues(index) = Bb
                    rgbValues(index + 1) = Gb
                    rgbValues(index + 2) = Rb
                    index += 3
                Next
            Next
        End Sub
    End Class
End Namespace
