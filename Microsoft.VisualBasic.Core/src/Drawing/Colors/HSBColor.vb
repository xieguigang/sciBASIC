#Region "Microsoft.VisualBasic::7119a6a1b3c0dff8ddb496381ec0d0eb, Microsoft.VisualBasic.Core\src\Extensions\Image\Colors\HSBColor.vb"

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

    '   Total Lines: 113
    '    Code Lines: 73 (64.60%)
    ' Comment Lines: 22 (19.47%)
    '    - Xml Docs: 77.27%
    ' 
    '   Blank Lines: 18 (15.93%)
    '     File Size: 5.04 KB


    '     Class HSBColor
    ' 
    '         Properties: Brightness, Hue, Saturation
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: ToRgb, ToString
    ' 
    '         Sub: FromRgb, ToRgb
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Imaging

    ''' <summary>
    ''' Provides methods for conversion between RGB and HSB color models
    ''' </summary>
    Public Class HSBColor

        Public Property Hue As Double
        Public Property Saturation As Double
        Public Property Brightness As Double

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(color As Color)
            Call FromRgb(color.R / 255, color.G / 255, color.B / 255, Hue, Saturation, Brightness)
        End Sub

        Sub New()
        End Sub

        Sub New(h As Double, s As Double, b As Double)
            Hue = h
            Saturation = s
            Brightness = b
        End Sub

        Public Function ToRgb() As Color
            Dim r, g, b As Double
            Call ToRgb(Hue, Saturation, Brightness, r, g, b)
            Return Color.FromArgb(r * 255, g * 255, b * 255)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToRgb.ToHtmlColor
        End Function

        ''' <summary>Converts HSB color model values to RGB values</summary>
        ''' <param name="hue">Input <paramref name="hue"/> value range [0.0; 1.0]</param>
        ''' <param name="saturation">Input <paramref name="saturation"/> value range [0.0; 1.0]</param>
        ''' <param name="brightness">Input <paramref name="brightness"/> value range [0.0; 1.0]</param>
        ''' <param name="red">Output <paramref name="red"/> channel value range [0.0; 1.0]</param>
        ''' <param name="green">Output <paramref name="green"/> channel value range [0.0; 1.0]</param>
        ''' <param name="blue">Output <paramref name="blue"/> channel value range [0.0; 1.0]</param>
        Public Shared Sub ToRgb(hue As Double, saturation As Double, brightness As Double,
                                <Out> ByRef red As Double,
                                <Out> ByRef green As Double,
                                <Out> ByRef blue As Double)

            Dim hueSector = hue * 6
            Dim hueSectorIntegerPart As Integer = hueSector
            Dim hueSectorFractionalPart = hueSector - hueSectorIntegerPart

            Dim p = brightness * (1 - saturation),
                q = brightness * (1 - hueSectorFractionalPart * saturation),
                t = brightness * (1 - (1 - hueSectorFractionalPart) * saturation)

            Select Case hueSectorIntegerPart
                Case 1 : red = q : green = brightness : blue = p
                Case 2 : red = p : green = brightness : blue = t
                Case 3 : red = p : green = q : blue = brightness
                Case 4 : red = t : green = p : blue = brightness
                Case 5 : red = brightness : green = p : blue = q
                Case Else
                    red = brightness
                    green = t
                    blue = p
            End Select
        End Sub

        ''' <summary>Converts RGB color model values to HSB values</summary>
        ''' <param name="red">Input <paramref name="red"/> channel value range [0.0; 1.0]</param>
        ''' <param name="green">Input <paramref name="green"/> channel value range [0.0; 1.0]</param>
        ''' <param name="blue">Input <paramref name="blue"/> channel value range [0.0; 1.0]</param>
        ''' <param name="hue">Output <paramref name="hue"/> value range [0.0; 1.0]</param>
        ''' <param name="saturation">Output <paramref name="saturation"/> value range [0.0; 1.0]</param>
        ''' <param name="brightness">Output <paramref name="brightness"/> value range [0.0; 1.0]</param>
        Public Shared Sub FromRgb(red As Double, green As Double, blue As Double,
                                  <Out> ByRef hue As Double,
                                  <Out> ByRef saturation As Double,
                                  <Out> ByRef brightness As Double)
            hue = 0
            ' Max of r/g/b
            brightness = If(red > green, If(red > blue, red, blue), If(green > blue, green, blue))
            ' Max - mof r/g/b
            Dim delta = brightness - If(red < green, If(red < blue, red, blue), If(green < blue, green, blue))

            saturation = If(brightness = 0, 0, delta / brightness)

            If saturation = 0 Then
                Return
            End If

            ' Determining hue sector
            If red = brightness Then
                hue = (green - blue) / delta
            ElseIf green = brightness Then
                hue = 2 + (blue - red) / delta
            ElseIf blue = brightness Then
                hue = 4 + (red - green) / delta
            End If

            ' Sector to hue
            hue *= 1.0R / 6.0R

            ' For cases like R = MAX & B > G
            If hue < 0 Then hue += 1.0R
        End Sub
    End Class
End Namespace
