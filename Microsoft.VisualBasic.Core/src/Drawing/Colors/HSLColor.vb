#Region "Microsoft.VisualBasic::7d9144fb3824ee02288f4622969dcac9, Microsoft.VisualBasic.Core\src\Drawing\Colors\HSLColor.vb"

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

    '   Total Lines: 147
    '    Code Lines: 88 (59.86%)
    ' Comment Lines: 38 (25.85%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 21 (14.29%)
    '     File Size: 4.91 KB


    '     Structure HSLColor
    ' 
    '         Properties: H, L, S
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetHSL, hue2rgb, Lighten, ToRGB, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports std = System.Math

Namespace Imaging

    ''' <summary>
    ''' Describes a RGB color in Hue, Saturation, and Luminance values.
    ''' 
    ''' HSL (hue, saturation, lightness)
    ''' SL (hue, saturation, lightness) and HSV (hue, saturation, value) 
    ''' are alternative representations of the RGB color model
    ''' https://en.wikipedia.org/wiki/HSL_and_HSV
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
            Return ToRGB.ToHtmlColor
        End Function

        ''' <summary>
        ''' Lighten target color composition.
        ''' </summary>
        ''' <param name="percentage"></param>
        ''' <param name="lightColor"></param>
        ''' <returns></returns>
        Public Function Lighten(percentage As Double, lightColor As Color) As Color
            Dim base As Color = ToRGB()
            Dim newColor As Color = Color.FromArgb(
                base.A,
                (lightColor.R / 255.0) * base.R,
                (lightColor.G / 255.0) * base.G,
                (lightColor.B / 255.0) * base.B)
            Dim hsl As HSLColor = HSLColor.GetHSL(newColor)
            Dim l = std.Min(hsl.L + percentage, 1)

            newColor = New HSLColor(hsl.H, hsl.S, l).ToRGB
            Return newColor
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
            Dim max, min As Double

            max = std.Max(r, g)
            max = std.Max(max, b)
            min = std.Min(r, g)
            min = std.Min(min, b)

            l = (min + max) / 2.0

            If max = min Then
                s = 0
                h = s
            Else
                Dim d As Double = max - min

                s = If(l > 0.5, d / (2.0 - max - min), d / (max + min))

                If max = r Then
                    h = (g - b) / d + (If(g < b, 6.0, 0.0))
                ElseIf max = g Then
                    h = (b - r) / d + 2.0
                ElseIf max = b Then
                    h = (r - g) / d + 4.0
                End If

                h /= 6.0
            End If

            Return New HSLColor(h, s, l)
        End Function
    End Structure
End Namespace
