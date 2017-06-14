#Region "Microsoft.VisualBasic::3de5825866652d97f3559642bbe6def1, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GDI+\GeomTransform.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq

Namespace Imaging

    <PackageNamespace("GDI.Transform")> Public Module GeomTransform

        <Extension> Public Function CalculateAngle(p1 As Point, p2 As Point) As Double
            Dim xDiff As Single = p2.X - p1.X
            Dim yDiff As Single = p2.Y - p1.Y
            Return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI
        End Function

        <Extension>
        Public Function GetBounds(points As IEnumerable(Of Point)) As RectangleF
            Return points.Select(Function(pt) pt.PointF).GetBounds
        End Function

        <Extension>
        Public Function GetBounds(points As IEnumerable(Of PointF)) As RectangleF
            Dim array = points.ToArray
            Dim xmin = array.Min(Function(pt) pt.X)
            Dim xmax = array.Max(Function(pt) pt.X)
            Dim ymin = array.Min(Function(pt) pt.Y)
            Dim ymax = array.Max(Function(pt) pt.Y)
            Dim topLeft As New PointF(xmin, ymin)
            Dim size As New SizeF(xmax - xmin, ymax - ymin)
            Return New RectangleF(topLeft, size)
        End Function

        <Extension> Public Function ToPoint(pf As PointF) As Point
            Return New Point(pf.X, pf.Y)
        End Function

        ''' <summary>
        ''' Gets the center location of the region rectangle.
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <ExportAPI("Center")>
        <Extension> Public Function Centre(rect As Rectangle) As Point
            Return New Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
        End Function

        ''' <summary>
        ''' Gets the center location of the region rectangle.
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <ExportAPI("Center")>
        <Extension> Public Function Centre(rect As RectangleF) As PointF
            Return New PointF(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
        End Function

        <Extension>
        Public Function CentralOffset(pts As IEnumerable(Of Point), frameSize As Size) As PointF
            Return pts.Select(Function(pt) pt.PointF).ToArray.CentralOffset(frameSize.SizeF)
        End Function

        <Extension>
        Public Function SizeF(size As Size) As SizeF
            Return New SizeF(size.Width, size.Height)
        End Function

        <Extension>
        Public Function CentralOffset(pts As IEnumerable(Of PointF), frameSize As SizeF) As PointF
            Dim xOffset!() = pts.ToArray(Function(x) x.X)
            Dim yOffset!() = pts.ToArray(Function(x) x.Y)
            Dim xo, yo As Single

            If xOffset.Length > 0 Then
                xo = xOffset.Min
            End If
            If yOffset.Length > 0 Then
                yo = yOffset.Min
            End If

            Dim size As New SizeF(xOffset.Max - xOffset.Min, yOffset.Max - yOffset.Min)
            Dim left! = (frameSize.Width - size.Width) / 2
            Dim top! = (frameSize.Height - size.Height) / 2

            Return New PointF(left - xo, top - yo)
        End Function

        Const pi2 As Double = Math.PI / 2.0

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
        <ExportAPI("Image.Rotate", Info:="Creates a new Image containing the same image only rotated.")>
        <Extension> Public Function RotateImage(image As Image, angle As Single) As Bitmap
            If image Is Nothing Then
                Throw New ArgumentNullException("image value is nothing!")
            End If

            Dim oldWidth As Double = CDbl(image.Width)
            Dim oldHeight As Double = CDbl(image.Height)

            ' Convert degrees to radians
            Dim theta As Double = CDbl(angle) * Math.PI / 180.0
            Dim locked_theta As Double = theta

            ' Ensure theta is now [0, 2pi)
            While locked_theta < 0.0
                locked_theta += 2 * Math.PI
            End While

            Dim newWidth As Double, newHeight As Double
            Dim nWidth As Integer, nHeight As Integer        ' The newWidth/newHeight expressed as ints

            Dim adjacentTop As Double, oppositeTop As Double
            Dim adjacentBottom As Double, oppositeBottom As Double

            ' We need to calculate the sides of the triangles based
            ' on how much rotation is being done to the bitmap.
            '   Refer to the first paragraph in the explaination above for 
            '   reasons why.
            If (locked_theta >= 0.0 AndAlso locked_theta < pi2) OrElse (locked_theta >= Math.PI AndAlso locked_theta < (Math.PI + pi2)) Then
                adjacentTop = Math.Abs(Math.Cos(locked_theta)) * oldWidth
                oppositeTop = Math.Abs(Math.Sin(locked_theta)) * oldWidth

                adjacentBottom = Math.Abs(Math.Cos(locked_theta)) * oldHeight
                oppositeBottom = Math.Abs(Math.Sin(locked_theta)) * oldHeight
            Else
                adjacentTop = Math.Abs(Math.Sin(locked_theta)) * oldHeight
                oppositeTop = Math.Abs(Math.Cos(locked_theta)) * oldHeight

                adjacentBottom = Math.Abs(Math.Sin(locked_theta)) * oldWidth
                oppositeBottom = Math.Abs(Math.Cos(locked_theta)) * oldWidth
            End If

            newWidth = adjacentTop + oppositeBottom
            newHeight = adjacentBottom + oppositeTop

            nWidth = CInt(Math.Truncate(Math.Ceiling(newWidth)))
            nHeight = CInt(Math.Truncate(Math.Ceiling(newHeight)))

            Dim rotatedBmp As New Bitmap(nWidth, nHeight)

            Using g As Graphics = Graphics.FromImage(rotatedBmp)
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

                If locked_theta >= 0.0 AndAlso locked_theta < pi2 Then
                    points = New Point() {
                    New Point(CInt(Math.Truncate(oppositeBottom)), 0),
                    New Point(nWidth, CInt(Math.Truncate(oppositeTop))),
                    New Point(0, CInt(Math.Truncate(adjacentBottom)))
                }
                ElseIf locked_theta >= pi2 AndAlso locked_theta < Math.PI Then
                    points = New Point() {
                    New Point(nWidth, CInt(Math.Truncate(oppositeTop))),
                    New Point(CInt(Math.Truncate(adjacentTop)), nHeight),
                    New Point(CInt(Math.Truncate(oppositeBottom)), 0)
                }
                ElseIf locked_theta >= Math.PI AndAlso locked_theta < (Math.PI + pi2) Then
                    points = New Point() {
                    New Point(CInt(Math.Truncate(adjacentTop)), nHeight),
                    New Point(0, CInt(Math.Truncate(adjacentBottom))),
                    New Point(nWidth, CInt(Math.Truncate(oppositeTop)))
                }
                Else
                    points = New Point() {
                    New Point(0, CInt(Math.Truncate(adjacentBottom))),
                    New Point(CInt(Math.Truncate(oppositeBottom)), 0),
                    New Point(CInt(Math.Truncate(adjacentTop)), nHeight)
                }
                End If

                Call g.DrawImage(image, points)
            End Using

            Return rotatedBmp
        End Function
    End Module
End Namespace
