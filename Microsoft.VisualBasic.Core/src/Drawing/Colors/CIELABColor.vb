Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging

    Public Class CIELABColor

        ' 定义CIELAB颜色的三个分量
        Public Property L As Double
        Public Property a As Double
        Public Property b As Double

        ' 构造函数
        Public Sub New(L As Double, a As Double, b As Double)
            _L = L
            _a = a
            _b = b
        End Sub

        ' 计算与另一个CIELAB颜色的ΔE*ab差异
        Public Function CalculateDeltaE(other As CIELABColor) As Double
            Dim deltaL As Double = other.L - Me.L
            Dim deltaA As Double = other.a - Me.a
            Dim deltaB As Double = other.b - Me.b

            ' 计算ΔE*ab
            Dim deltaE As Double = std.Sqrt(deltaL ^ 2 + deltaA ^ 2 + deltaB ^ 2)
            Return deltaE
        End Function

        ' 重写ToString方法，以便更好地显示CIELAB颜色值
        Public Overrides Function ToString() As String
            Return String.Format("CIELABColor(L={0}, a={1}, b={2})", _L, _a, _b)
        End Function

        ' 定义参考白点D65的XYZ值
        Public Const referenceX As Double = 95.047
        Public Const referenceY As Double = 100.0
        Public Const referenceZ As Double = 108.883

        ' RGB到CIELAB的转换函数
        Public Shared Function RGBToCIELAB(rgbColor As Color) As CIELABColor
            ' 将RGB值转换为0到1的范围
            Dim rNorm As Double = rgbColor.R / 255.0
            Dim gNorm As Double = rgbColor.G / 255.0
            Dim bNorm As Double = rgbColor.B / 255.0

            ' 应用gamma校正
            rNorm = If(rNorm > 0.04045, std.Pow((rNorm + 0.055) / 1.055, 2.4), rNorm / 12.92)
            gNorm = If(gNorm > 0.04045, std.Pow((gNorm + 0.055) / 1.055, 2.4), gNorm / 12.92)
            bNorm = If(bNorm > 0.04045, std.Pow((bNorm + 0.055) / 1.055, 2.4), bNorm / 12.92)

            ' 将RGB值转换为XYZ值
            Dim x As Double = (rNorm * 0.4124564 + gNorm * 0.3575761 + bNorm * 0.1804375) * 100
            Dim y As Double = (rNorm * 0.2126729 + gNorm * 0.7151522 + bNorm * 0.072175) * 100
            Dim z As Double = (rNorm * 0.0193339 + gNorm * 0.119192 + bNorm * 0.9503041) * 100

            ' 将XYZ值转换为CIELAB值
            Dim l As Double = If(y / referenceY > 0.008856, std.Pow(y / referenceY, 1 / 3.0) * 116 - 16, y / referenceY * 903.3)
            Dim a As Double = 500 * (Fxyz(x / referenceX) - Fxyz(y / referenceY))
            Dim b As Double = 200 * (Fxyz(y / referenceY) - Fxyz(z / referenceZ))

            Return New CIELABColor(l, a, b)
        End Function

        ' 辅助函数用于XYZ到CIELAB的转换
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function Fxyz(t As Double) As Double
            Return If(t > 0.008856, std.Pow(t, 1 / 3.0), 7.787 * t + 16 / 116.0)
        End Function

        ' CIELAB到RGB的转换函数
        Public Shared Function CIELABToRGB(labColor As CIELABColor) As Color
            ' 将CIELAB值转换为XYZ值
            Dim y As Double = If(labColor.L > 8, std.Pow((labColor.L + 16) / 116, 3) * referenceY, labColor.L / 903.3 * referenceY)
            Dim x As Double = referenceX * FxyzInv((labColor.L + 16) / 116 + labColor.a / 500)
            Dim z As Double = referenceZ * FxyzInv((labColor.L + 16) / 116 - labColor.b / 200)

            ' 将XYZ值转换为RGB值
            Dim rNorm As Double = (x * 0.032406 + y * 0.022886 - z * 0.009688) / 100
            Dim gNorm As Double = (-x * 0.015372 - y * 0.065627 + z * 0.061765) / 100
            Dim bNorm As Double = (x * 0.004102 - y * 0.003105 + z * 0.030483) / 100

            ' 应用gamma校正的逆过程
            rNorm = If(rNorm > 0.0031308, 1.055 * std.Pow(rNorm, 1 / 2.4) - 0.055, 12.92 * rNorm)
            gNorm = If(gNorm > 0.0031308, 1.055 * std.Pow(gNorm, 1 / 2.4) - 0.055, 12.92 * gNorm)
            bNorm = If(bNorm > 0.0031308, 1.055 * std.Pow(bNorm, 1 / 2.4) - 0.055, 12.92 * bNorm)

            ' 将RGB值转换回0到255的范围
            Dim r As Integer = Math.Clamp(CType(rNorm * 255, Integer), 0, 255)
            Dim g As Integer = Math.Clamp(CType(gNorm * 255, Integer), 0, 255)
            Dim b As Integer = Math.Clamp(CType(bNorm * 255, Integer), 0, 255)

            Return Color.FromArgb(r, g, b)
        End Function

        ' 辅助函数用于CIELAB到XYZ的转换
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function FxyzInv(t As Double) As Double
            Return If(t > 0.206896, std.Pow(t, 3), 0.128418 * (t - 0.137931))
        End Function
    End Class
End Namespace