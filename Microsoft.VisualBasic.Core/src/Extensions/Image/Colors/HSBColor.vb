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
        ''' <paramname="hue">Input <paramrefname="hue"/> value range [0.0; 1.0]</param>
        ''' <paramname="saturation">Input <paramrefname="saturation"/> value range [0.0; 1.0]</param>
        ''' <paramname="brightness">Input <paramrefname="brightness"/> value range [0.0; 1.0]</param>
        ''' <paramname="red">Output <paramrefname="red"/> channel value range [0.0; 1.0]</param>
        ''' <paramname="green">Output <paramrefname="green"/> channel value range [0.0; 1.0]</param>
        ''' <paramname="blue">Output <paramrefname="blue"/> channel value range [0.0; 1.0]</param>
        ''' <exception cref="ArgumentException"><inheritdoccref="Rgb.IsBright(Double,Double,Double)"/></exception>
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
        ''' <paramname="red">Input <paramrefname="red"/> channel value range [0.0; 1.0]</param>
        ''' <paramname="green">Input <paramrefname="green"/> channel value range [0.0; 1.0]</param>
        ''' <paramname="blue">Input <paramrefname="blue"/> channel value range [0.0; 1.0]</param>
        ''' <paramname="hue">Output <paramrefname="hue"/> value range [0.0; 1.0]</param>
        ''' <paramname="saturation">Output <paramrefname="saturation"/> value range [0.0; 1.0]</param>
        ''' <paramname="brightness">Output <paramrefname="brightness"/> value range [0.0; 1.0]</param>
        ''' <exception cref="ArgumentException"><inheritdoccref="Rgb.IsBright(Double,Double,Double)"/></exception>
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
