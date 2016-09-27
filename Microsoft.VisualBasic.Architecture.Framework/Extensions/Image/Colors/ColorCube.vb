#Region "Microsoft.VisualBasic::21d70a6abd9dc838c9f83b14db95177c, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Colors\ColorCube.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Imaging

    ''' <summary>
    ''' Describes the RGB color space as a 3D cube with the origin at Black.
    ''' </summary>
    ''' <remarks>
    ''' http://social.technet.microsoft.com/wiki/contents/articles/20990.generate-color-sequences-using-rgb-color-cube-in-vb-net.aspx
    ''' </remarks>
    <PackageNamespace("ColorCube",
                  Publisher:="Reed Kimble",
                  Category:=APICategories.UtilityTools,
                  Description:="Sometimes when you are designing a form, or creating some other kind of visual output, you'd like to generate an array of colors which may be shades of a single color or a discernible sequence of individual colors such as the spectrum of a rainbow.  This can be useful for coloring bars in a graph or generating a gradient around some specified color.  Unfortunately the .Net framework does not give us any sophisticated solution for this.
                  <br />
While any number of complex code solutions could be created to attempt to address this problem, if we think of the RGB color space spatially, we can construct a three-dimensional cube which represents all possible colors and can easily be traversed mathematically.",
                  Url:="http://social.technet.microsoft.com/wiki/contents/articles/20990.generate-color-sequences-using-rgb-color-cube-in-vb-net.aspx")>
    Public Module ColorCube

        ''' <summary>
        ''' Compares two colors according to their distance from the origin of the cube (black).
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Compare")>
        Public Function Compare(source As Color, target As Color) As Integer
            Dim delta1 As Double = GetDistance(Color.Black, source)
            Dim delta2 As Double = GetDistance(Color.Black, target)
            Return delta1.CompareTo(delta2)
        End Function

        ''' <summary>
        ''' Returns an integer between 0 and 255 indicating the perceived brightness of the color.
        ''' </summary>
        ''' <param name="target">A System.Drawing.Color instance.</param>
        ''' <returns>An integer indicating the brightness with 0 being dark and 255 being bright.</returns>
        ''' <remarks>
        ''' Formula found using web search at:
        ''' http://www.nbdtech.com/Blog/archive/2008/04/27/Calculating-the-Perceived-Brightness-of-a-Color.aspx This link is external to TechNet Wiki. It will open in a new window.
        ''' with reference to : http://alienryderflex.com/hsp.html This link is external to TechNet Wiki. It will open in a new window.
        ''' Effectively the same as measuring a color's distance from black, but constrained to a 0-255 range.
        ''' </remarks>
        ''' 
        <ExportAPI("Brightness")>
        Public Function GetBrightness(target As Color) As Integer
            Return CInt(Math.Sqrt(0.241 * target.R ^ 2 + 0.691 * target.G ^ 2 + 0.068 * target.B ^ 2))
        End Function

        ''' <summary>
        ''' Gets a color from within the cube starting at the origin and moving a given distance in the specified direction.
        ''' </summary>
        ''' <param name="azimuth">The side-to-side angle in degrees; 0 points toward red and 90 points toward blue.</param>
        ''' <param name="elevation">The top-to-bottom angle in degrees; 0 is no green and 90 points toward full green.</param>
        ''' <param name="distance">The distance to travel within the cube; 500 is max.</param>
        ''' <returns>The color within the cube at the given distance in the specified direction.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("CreateColor")>
        Public Function GetColorFrom(azimuth As Integer, elevation As Integer, distance As Integer) As Color
            Return GetColorFrom(Color.Black, azimuth, elevation, distance)
        End Function

        ''' <summary>
        ''' Gets a color from within the cube starting at the specified location and moving a given distance in the specified direction.
        ''' </summary>
        ''' <param name="source">The source location within the cube from which to start moving.</param>
        ''' <param name="azimuth">The side-to-side angle in degrees; 0 points toward red and 90 points toward blue.</param>
        ''' <param name="elevation">The top-to-bottom angle in degrees; 0 is no green and 90 points toward full green.</param>
        ''' <param name="distance">The distance to travel within the cube; the approximate distance from black to white is 500.</param>
        ''' <returns>The color within the cube at the given distance in the specified direction.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("CreateColor")>
        Public Function GetColorFrom(source As Color, azimuth As Double, elevation As Double, distance As Double, Optional isRadians As Boolean = False) As Color
            If azimuth < 0 OrElse azimuth > 90 Then Throw New ArgumentException("azimuth", "Value must be between 0 and 90.")
            If elevation < 0 OrElse elevation > 90 Then Throw New ArgumentException("elevation", "Value must be between 0 and 90.")
            Dim a, e, r, g, b As Double
            If isRadians Then
                a = azimuth
                e = elevation
            Else
                a = DegreesToRadians(azimuth)
                e = DegreesToRadians(elevation)
            End If
            r = distance * Math.Cos(a) * Math.Cos(e)
            b = distance * Math.Sin(a) * Math.Cos(e)
            g = distance * Math.Sin(e)
            If Double.IsNaN(r) Then r = 0
            If Double.IsNaN(g) Then g = 0
            If Double.IsNaN(b) Then b = 0
            Return Color.FromArgb(Math.Max(Math.Min(source.R + r, 255), 0), Math.Max(Math.Min(source.G + g, 255), 0), Math.Max(Math.Min(source.B + b, 255), 0))
        End Function

        ''' <summary>
        ''' Creates an array of colors from a selection within a sphere around the specified color.
        ''' </summary>
        ''' <param name="target">The color to select around.</param>
        ''' <param name="distance">The radius of the selection sphere.</param>
        ''' <param name="increment">The increment within the sphere at which a selection is taken; larger numbers result in smaller selection sets.</param>
        ''' <returns>An array of colors located around the specified color within the cube.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("GetColors")>
        Public Function GetColorsAround(target As Color, distance As Integer, increment As Integer) As Color()
            Dim result As New List(Of Color)
            For a As Integer = 0 To 359 Step increment
                For e As Integer = 0 To 359 Step increment
                    Dim c As Color = GetColorFrom(target, a, e, distance)
                    If Not result.Contains(c) Then
                        result.Add(c)
                    End If
                Next
            Next
            result.Sort(AddressOf Compare)
            Return result.ToArray
        End Function

        ''' <summary>
        ''' Creates an array of colors in a gradient sequence between two specified colors.
        ''' </summary>
        ''' <param name="source">The starting color in the sequence.</param>
        ''' <param name="target">The end color in the sequence.</param>
        ''' <param name="increment">The increment between colors.</param>
        ''' <returns>A gradient array of colors.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Gradients")>
        Public Function GetColorSequence(source As Color, target As Color, increment As Integer) As Color()
            Dim result As New List(Of Color)
            Dim a As Double = GetAzimuthTo(source, target)
            Dim e As Double = GetElevationTo(source, target)
            Dim d As Double = GetDistance(source, target)
            For i As Integer = 0 To d Step increment
                result.Add(GetColorFrom(source, a, e, i, True))
            Next
            Return result.ToArray
        End Function

        ''' <summary>
        ''' Creates a rainbow array of colors by selecting from the edges of the cube in ROYGBIV order at the specified increment.
        ''' </summary>
        ''' <param name="increment">The increment along the edges at which a selection is taken; larger numbers result in smaller selection sets.</param>
        ''' <returns>An array of colors in ROYGBIV order at the given increment.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("ColorSpectrum")>
        Public Function GetColorSpectrum(increment As Integer) As Color()
            Dim result As New List(Of Color)
            Dim rgb(2) As Integer
            Dim idx As Integer = 1
            Dim inc As Integer = increment
            Dim cmp As Func(Of Integer, Integer, Boolean)

            rgb(0) = 255
            cmp = AddressOf CompareLess
            Do
                result.Add(Color.FromArgb(rgb(0), rgb(1), rgb(2)))
                If cmp(rgb(idx), inc) Then
                    rgb(idx) += inc
                Else
                    Select Case idx
                        Case 1
                            If rgb(2) < 255 Then
                                rgb(idx) = 255
                                idx = 0
                                cmp = AddressOf CompareGreater
                            Else
                                rgb(idx) = 0
                                idx = 0
                                cmp = AddressOf CompareLess
                            End If
                        Case 2
                            rgb(idx) = 255
                            idx = 1
                            cmp = AddressOf CompareGreater
                        Case 0
                            If rgb(2) < 255 Then
                                rgb(idx) = 0
                                idx = 2
                                cmp = AddressOf CompareLess
                            Else
                                rgb(idx) = 255
                                Exit Do
                            End If
                    End Select
                    inc *= -1
                End If
            Loop
            result.Add(Color.FromArgb(rgb(0), rgb(1), rgb(2)))
            Return result.ToArray
        End Function

        ''' <summary>
        ''' Gets the distance between two colors within the cube.
        ''' </summary>
        ''' <param name="source">The source color in the cube.</param>
        ''' <param name="target">The target color in the cube.</param>
        ''' <returns>The distance between the source and target colors.</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Distance")>
        Public Function GetDistance(source As Color, target As Color) As Double
            Dim squareR As Double = CDbl(target.R) - CDbl(source.R)
            squareR *= squareR
            Dim squareG As Double = CDbl(target.G) - CDbl(source.G)
            squareG *= squareG
            Dim squareB As Double = CDbl(target.B) - CDbl(source.B)
            squareB *= squareB
            Return System.Math.Sqrt(squareR + squareG + squareB)
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
        Public Function GetHSL(rgb As Color) As HSLColor
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

        <ExportAPI("CompareLess")>
        Public Function CompareLess(value As Integer, inc As Integer) As Boolean
            Return value < 255 - Math.Abs(inc)
        End Function

        <ExportAPI("CompareGreater")>
        Public Function CompareGreater(value As Integer, inc As Integer) As Boolean
            Return value > 0 + Math.Abs(inc)
        End Function

        <ExportAPI("Radians")>
        Public Function DegreesToRadians(degrees As Double) As Double
            Return degrees * (Math.PI / 180.0)
        End Function

        <ExportAPI("Degrees")>
        Public Function RadiansToDegrees(radians As Double) As Double
            Return CSng(radians * (180.0 / Math.PI))
        End Function

        <ExportAPI("Azimuth")>
        Public Function GetAzimuthTo(source As Color, target As Color) As Double
            Return WrapAngle(Math.Atan2(CDbl(target.B) - CDbl(source.B), CDbl(target.R) - CDbl(source.R)))
        End Function

        <ExportAPI("Elevation")>
        Public Function GetElevationTo(source As Color, target As Color) As Double
            Return WrapAngle(Math.Atan2(CDbl(target.G) - CDbl(source.G), 255))
        End Function

        <ExportAPI("WrapAngle")>
        Public Function WrapAngle(radians As Double) As Double
            While radians < -Math.PI
                radians += Math.PI * 2
            End While
            While radians > Math.PI
                radians -= Math.PI * 2
            End While
            Return radians
        End Function
    End Module

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
    End Structure
End Namespace
