Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Imaging

    ''' <summary>
    ''' Describes a RGB color in Hue, Saturation, and Luminance values.
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure HSLColor

        ''' <summary>
        ''' The color hue.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property H As Double
        ''' <summary>
        ''' The color saturation.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property S As Double
        ''' <summary>
        ''' The color luminance.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property L As Double

        Public Sub New(hValue As Double, sValue As Double, lValue As Double)
            H = hValue
            S = sValue
            L = lValue
        End Sub

        Public Overrides Function ToString() As String
            Return ToRGB.RGB2Hexadecimal
        End Function

        Public Function ToRGB() As Color
            Dim r, g, b As Double
            Dim h As Double = Me.H
            Dim s As Double = Me.S
            Dim l As Double = Me.L

            If s = 0 Then
                b = l
                g = b
                r = g
            Else
                Dim q As Double = If(l < 0.5, l * (1 + s), l + s - l * s)
                Dim p As Double = 2.0 * l - q

                r = HSLColor.hue2rgb(p, q, h + 1 / 3.0)
                g = HSLColor.hue2rgb(p, q, h)
                b = HSLColor.hue2rgb(p, q, h - 1 / 3.0)
            End If

            r = r * 255.0
            g = g * 255.0
            b = b * 255.0

            Return Color.FromArgb(r, g, b)
        End Function

        Private Shared Function hue2rgb(p As Double, q As Double, t As Double) As Double
            If t < 0 Then t += 1
            If t > 1 Then t -= 1
            If t < 1 / 6.0 Then Return p + (q - p) * 6.0 * t
            If t < 1 / 2.0 Then Return q
            If t < 2 / 3.0 Then Return p + (q - p) * (2.0 / 3.0 - t) * 6.0
            Return p
        End Function

        ''' <summary>
        ''' Converts a RGB color into its Hue, Saturation, and Luminance (HSL) values.
        ''' </summary>
        ''' <param name="rgb">The color to convert.</param>
        ''' <returns>The HSL representation of the color.</returns>
        ''' <remarks>
        ''' Source algorithm found using web search at:
        ''' http://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm This link is external to TechNet Wiki. It will open in a new window.
        ''' (Adapted to VB)
        ''' </remarks>
        ''' 
        <ExportAPI("Color.HSL")>
        Public Shared Function GetHSL(rgb As Color) As HSLColor
            Dim h, s, l As Double
            Dim r As Double = rgb.R / 255.0
            Dim g As Double = rgb.G / 255.0
            Dim b As Double = rgb.B / 255.0
            Dim v, m, vm As Double
            Dim r2, g2, b2 As Double

            h = 0
            s = 0
            l = 0
            v = Math.Max(r, g)
            v = Math.Max(v, b)
            m = Math.Min(r, g)
            m = Math.Min(m, b)
            l = (m + v) / 2.0
            If l <= 0.0 Then
                Exit Function
            End If

            vm = v - m
            s = vm
            If s > 0.0 Then
                s /= If((l <= 0.5), (v + m), (2.0 - v - m))
            Else
                Exit Function
            End If

            r2 = (v - r) / vm
            g2 = (v - g) / vm
            b2 = (v - b) / vm

            If (r = v) Then
                h = If(g = m, 5.0 + b2, 1.0 - g2)
            ElseIf (g = v) Then
                h = If(b = m, 1.0 + r2, 3.0 - b2)
            Else
                h = If(r = m, 3.0 + g2, 5.0 - r2)
            End If

            h /= 6.0
            Return New HSLColor(h, s, l)
        End Function
    End Structure
End Namespace