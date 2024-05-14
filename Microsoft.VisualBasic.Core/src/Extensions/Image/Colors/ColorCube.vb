#Region "Microsoft.VisualBasic::826a36c440bdd1fc8bd797127bef65fe, Microsoft.VisualBasic.Core\src\Extensions\Image\Colors\ColorCube.vb"

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

    '   Total Lines: 273
    '    Code Lines: 174
    ' Comment Lines: 73
    '   Blank Lines: 26
    '     File Size: 13.23 KB


    '     Module ColorCube
    ' 
    '         Function: Compare, CompareGreater, CompareLess, DegreesToRadians, GetAzimuthTo
    '                   GetBrightness, (+2 Overloads) GetColorFrom, GetColorsAround, GetColorSequence, GetColorSpectrum
    '                   GetElevationTo, RadiansToDegrees, WrapAngle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports stdNum = System.Math

Namespace Imaging

    ''' <summary>
    ''' Describes the RGB color space as a 3D cube with the origin at Black.
    ''' </summary>
    ''' <remarks>
    ''' http://social.technet.microsoft.com/wiki/contents/articles/20990.generate-color-sequences-using-rgb-color-cube-in-vb-net.aspx
    ''' </remarks>
    <Package("ColorCube",
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
            Dim delta1 As Double = Color.Black.EuclideanDistance(source)
            Dim delta2 As Double = Color.Black.EuclideanDistance(target)
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
        <Extension>
        Public Function GetBrightness(target As Color) As Integer
            Return CInt(stdNum.Sqrt(0.241 * target.R ^ 2 + 0.691 * target.G ^ 2 + 0.068 * target.B ^ 2))
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
        ''' Value must be between 0 and 90.
        ''' </summary>
        Const InvalidRange$ = "Value must be between 0 and 90."

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
        Public Function GetColorFrom(source As Color,
                                     azimuth As Double,
                                     elevation As Double,
                                     distance As Double,
                                     Optional isRadians As Boolean = False,
                                     Optional alpha% = 255) As Color

            If azimuth < 0 OrElse azimuth > 90 Then
                Throw New ArgumentException("azimuth", InvalidRange)
            End If
            If elevation < 0 OrElse elevation > 90 Then
                Throw New ArgumentException("elevation", InvalidRange)
            End If

            Dim a, e, r, g, b As Double

            If isRadians Then
                a = azimuth
                e = elevation
            Else
                a = DegreesToRadians(azimuth)
                e = DegreesToRadians(elevation)
            End If

            r = distance * stdNum.Cos(a) * stdNum.Cos(e)
            b = distance * stdNum.Sin(a) * stdNum.Cos(e)
            g = distance * stdNum.Sin(e)

            If Double.IsNaN(r) Then r = 0
            If Double.IsNaN(g) Then g = 0
            If Double.IsNaN(b) Then b = 0

            Return Color.FromArgb(
                alpha,
                stdNum.Max(stdNum.Min(source.R + r, 255), 0),
                stdNum.Max(stdNum.Min(source.G + g, 255), 0),
                stdNum.Max(stdNum.Min(source.B + b, 255), 0))
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
        Public Function GetColorSequence(source As Color, target As Color, increment As Integer, Optional alpha% = 255) As Color()
            Dim a As Double = GetAzimuthTo(source, target)
            Dim e As Double = GetElevationTo(source, target)
            Dim d As Double = source.EuclideanDistance(target)
            Dim result As New List(Of Color)

            For i As Integer = 0 To d Step increment
                result += GetColorFrom(
                    source,
                    a, e, i,
                    isRadians:=True,
                    alpha:=alpha%)
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

        <ExportAPI("CompareLess")>
        Public Function CompareLess(value As Integer, inc As Integer) As Boolean
            Return value < 255 - stdNum.Abs(inc)
        End Function

        <ExportAPI("CompareGreater")>
        Public Function CompareGreater(value As Integer, inc As Integer) As Boolean
            Return value > 0 + stdNum.Abs(inc)
        End Function

        <ExportAPI("Radians")>
        Public Function DegreesToRadians(degrees As Double) As Double
            Return degrees * (stdNum.PI / 180.0)
        End Function

        <ExportAPI("Degrees")>
        Public Function RadiansToDegrees(radians As Double) As Double
            Return CSng(radians * (180.0 / stdNum.PI))
        End Function

        <ExportAPI("Azimuth")>
        Public Function GetAzimuthTo(source As Color, target As Color) As Double
            Return WrapAngle(stdNum.Atan2(CDbl(target.B) - CDbl(source.B), CDbl(target.R) - CDbl(source.R)))
        End Function

        <ExportAPI("Elevation")>
        Public Function GetElevationTo(source As Color, target As Color) As Double
            Return WrapAngle(stdNum.Atan2(CDbl(target.G) - CDbl(source.G), 255))
        End Function

        <ExportAPI("WrapAngle")>
        Public Function WrapAngle(radians As Double) As Double
            While radians < -stdNum.PI
                radians += stdNum.PI * 2
            End While
            While radians > stdNum.PI
                radians -= stdNum.PI * 2
            End While
            Return radians
        End Function
    End Module
End Namespace
