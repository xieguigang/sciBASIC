#Region "Microsoft.VisualBasic::6adab80607980c691329a469db6cfc4b, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\Effects.vb"

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

    '   Total Lines: 221
    '    Code Lines: 98 (44.34%)
    ' Comment Lines: 87 (39.37%)
    '    - Xml Docs: 62.07%
    ' 
    '   Blank Lines: 36 (16.29%)
    '     File Size: 9.50 KB


    '     Module Effects
    ' 
    '         Function: RotateImage, Vignette
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports std = System.Math
Imports Microsoft.VisualBasic.Imaging

Namespace Imaging.BitmapImage

    Public Module Effects

        ''' <summary>
        ''' 羽化
        ''' </summary>
        ''' <param name="Image"></param>
        ''' <param name="y1"></param>
        ''' <param name="y2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function Vignette(image As Image, y1%, y2%, Optional renderColor As Color = Nothing) As Image
            Dim alpha As Integer = 0
            Dim delta = (std.PI / 2) / std.Abs(y1 - y2)
            Dim offset As Double = 0

            renderColor = renderColor Or Color.White.AsDefaultColor

            Using g As Graphics2D = image.CreateCanvas2D
                With g
                    Dim rect As New Rectangle With {
                        .Location = New Point(0, y2),
                        .Size = New Size(.Width, .Height - y2)
                    }

                    For y As Integer = y1 To y2
                        Dim color As Color = Color.FromArgb(alpha, renderColor.R, renderColor.G, renderColor.B)
                        Dim pen As New Pen(color)

                        .DrawLine(pen, New Point(0, y), New Point(.Width, y))
                        alpha = CInt(255 * std.Sin(offset) ^ 2)
                        offset += delta
                    Next

                    Call .FillRectangle(New SolidBrush(renderColor), rect)

                    Return .ImageResource
                End With
            End Using
        End Function

        Const pi2 As Double = PI / 2.0

        ''' <summary>
        ''' Creates a new Image containing the same image only rotated
        ''' </summary>
        ''' <param name="image">The <see cref="System.Drawing.Image"/> to rotate</param>
        ''' <param name="angle">The amount to rotate the image, clockwise, in degrees</param>
        ''' <returns>A new <see cref="System.Drawing.Bitmap"/> that is just large enough
        ''' to contain the rotated image without cutting any corners off.</returns>
        ''' <exception cref="System.ArgumentNullException">Thrown if <see cref="image"/> is null.</exception>
        ''' <remarks>
        ''' 
        ''' Explaination of the calculations
        '''
        ''' The trig involved in calculating the new width and height
        ''' is fairly simple; the hard part was remembering that when 
        ''' PI/2 &lt;= theta &lt;= PI and 3PI/2 &lt;= theta &lt; 2PI the width and 
        ''' height are switched.
        '''  
        ''' When you rotate a rectangle, r, the bounding box surrounding r
        ''' contains for right-triangles of empty space.  Each of the 
        ''' triangles hypotenuse's are a known length, either the width or
        ''' the height of r.  Because we know the length of the hypotenuse
        ''' and we have a known angle of rotation, we can use the trig
        ''' function identities to find the length of the other two sides.
        '''  
        ''' sine = opposite/hypotenuse
        ''' cosine = adjacent/hypotenuse
        '''  
        ''' solving for the unknown we get
        '''  
        ''' opposite = sine * hypotenuse
        ''' adjacent = cosine * hypotenuse
        '''  
        ''' Another interesting point about these triangles is that there
        ''' are only two different triangles. The proof for which is easy
        ''' to see, but its been too long since I've written a proof that
        ''' I can't explain it well enough to want to publish it.  
        '''  
        ''' Just trust me when I say the triangles formed by the lengths 
        ''' width are always the same (for a given theta) and the same 
        ''' goes for the height of r.
        '''  
        ''' Rather than associate the opposite/adjacent sides with the
        ''' width and height of the original bitmap, I'll associate them
        ''' based on their position.
        '''  
        ''' adjacent/oppositeTop will refer to the triangles making up the 
        ''' upper right and lower left corners
        '''  
        ''' adjacent/oppositeBottom will refer to the triangles making up 
        ''' the upper left and lower right corners
        '''  
        ''' The names are based on the right side corners, because thats 
        ''' where I did my work on paper (the right side).
        '''  
        ''' Now if you draw this out, you will see that the width of the 
        ''' bounding box is calculated by adding together adjacentTop and 
        ''' oppositeBottom while the height is calculate by adding 
        ''' together adjacentBottom and oppositeTop.
        ''' 
        ''' </remarks>
        <ExportAPI("Image.Rotate")>
        <Extension>
        Public Function RotateImage(image As Image, angle!) As Bitmap
            If image Is Nothing Then
                Throw New ArgumentNullException("image value is nothing!")
            End If

            Dim oldWidth As Double = CDbl(image.Width)
            Dim oldHeight As Double = CDbl(image.Height)

            ' Convert degrees to radians
            Dim theta As Double = CDbl(angle) * std.PI / 180.0
            Dim lockedTheta As Double = theta

            ' Ensure theta is now [0, 2pi)
            While lockedTheta < 0.0
                lockedTheta += 2 * std.PI
            End While

            Dim newWidth As Double, newHeight As Double
            ' The newWidth/newHeight expressed as ints
            Dim nWidth As Integer, nHeight As Integer

            Dim adjacentTop As Double, oppositeTop As Double
            Dim adjacentBottom As Double, oppositeBottom As Double

            ' We need to calculate the sides of the triangles based
            ' on how much rotation is being done to the bitmap.
            '   Refer to the first paragraph in the explaination above for 
            '   reasons why.
            If (lockedTheta >= 0.0 AndAlso lockedTheta < pi2) OrElse (lockedTheta >= std.PI AndAlso lockedTheta < (std.PI + pi2)) Then
                adjacentTop = std.Abs(Cos(lockedTheta)) * oldWidth
                oppositeTop = std.Abs(Sin(lockedTheta)) * oldWidth

                adjacentBottom = std.Abs(Cos(lockedTheta)) * oldHeight
                oppositeBottom = std.Abs(Sin(lockedTheta)) * oldHeight
            Else
                adjacentTop = std.Abs(Sin(lockedTheta)) * oldHeight
                oppositeTop = std.Abs(Cos(lockedTheta)) * oldHeight

                adjacentBottom = std.Abs(Sin(lockedTheta)) * oldWidth
                oppositeBottom = std.Abs(Cos(lockedTheta)) * oldWidth
            End If

            newWidth = adjacentTop + oppositeBottom
            newHeight = adjacentBottom + oppositeTop

            nWidth = CInt(Truncate(Ceiling(newWidth)))
            nHeight = CInt(Truncate(Ceiling(newHeight)))

            Dim rotatedBmp As New Bitmap(nWidth, nHeight)

            ' This array will be used to pass in the three points that 
            ' make up the rotated image
            Dim points As Point()

            ' The values of opposite/adjacentTop/Bottom are referring to 
            ' fixed locations instead of in relation to the
            ' rotating image so I need to change which values are used
            ' based on the how much the image is rotating.

            ' For each point, one of the coordinates will always be 0, 
            ' nWidth, or nHeight.  This because the Bitmap we are drawing on
            ' is the bounding box for the rotated bitmap.  If both of the 
            ' corrdinates for any of the given points wasn't in the set above
            ' then the bitmap we are drawing on WOULDN'T be the bounding box
            ' as required.

            If lockedTheta >= 0.0 AndAlso lockedTheta < pi2 Then

                points = {
                    New Point(CInt(Truncate(oppositeBottom)), 0),
                    New Point(nWidth, CInt(Truncate(oppositeTop))),
                    New Point(0, CInt(Truncate(adjacentBottom)))
                }

            ElseIf lockedTheta >= pi2 AndAlso lockedTheta < std.PI Then

                points = {
                    New Point(nWidth, CInt(Truncate(oppositeTop))),
                    New Point(CInt(Truncate(adjacentTop)), nHeight),
                    New Point(CInt(Truncate(oppositeBottom)), 0)
                }

            ElseIf lockedTheta >= std.PI AndAlso lockedTheta < (std.PI + pi2) Then

                points = {
                    New Point(CInt(Truncate(adjacentTop)), nHeight),
                    New Point(0, CInt(Truncate(adjacentBottom))),
                    New Point(nWidth, CInt(Truncate(oppositeTop)))
                }

            Else

                points = {
                    New Point(0, CInt(Truncate(adjacentBottom))),
                    New Point(CInt(Truncate(oppositeBottom)), 0),
                    New Point(CInt(Truncate(adjacentTop)), nHeight)
                }

            End If

            Using g As Graphics = Graphics.FromImage(rotatedBmp)
                Call g.DrawImage(image, points)
            End Using

            Return rotatedBmp
        End Function
    End Module
End Namespace
