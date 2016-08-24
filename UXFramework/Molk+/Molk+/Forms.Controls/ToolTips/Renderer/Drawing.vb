#Region "Microsoft.VisualBasic::070b0f5a849a7cc49205c3ebf154f897, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ToolTips\Renderer\Drawing.vb"

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
Imports System.Drawing.Drawing2D
Imports System.Globalization

Namespace Renderer

    Public Class Drawing
        Public Shared en_us_ci As CultureInfo = New CultureInfo("en-US")
#Region "Enumerations"
        ''' <summary>
        ''' Color theme used for rendering objects.
        ''' </summary>
        Public Enum ColorTheme
            Blue = 0
            BlackBlue = 1
        End Enum
        ''' <summary>
        ''' Enumeration used to determine contents of a given tooltip parameter.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum ToolTipContent
            TitleOnly
            TitleAndText
            TitleAndImage
            All
            ImageOnly
            ImageAndText
            TextOnly
            Empty
        End Enum
        ''' <summary>
        ''' Enumeration used to determine starting point of glowing light.
        ''' </summary>
        ''' <remarks><seelaso cref="getGlowingPath"/></remarks>
        Public Enum LightingGlowPoint
            TopLeft
            TopCenter
            TopRight
            MiddleLeft
            MiddleCenter
            MiddleRight
            BottomLeft
            BottomCenter
            BottomRight
            Custom
        End Enum
        ''' <summary>
        ''' Enumeration used to determine the shadow location.
        ''' </summary>
        ''' <remarks><seealso cref="getInnerShadowPath"/></remarks>
        Public Enum ShadowPoint
            Top
            TopLeft
            TopRight
            Left
            Right
            Bottom
            BottomLeft
            BottomRight
        End Enum
        ''' <summary>
        ''' Enumeration used to determine the direction of a triangle.
        ''' </summary>
        ''' <remarks><seealso cref="drawTriangle"/></remarks>
        Public Enum TriangleDirection
            Up
            Left
            Right
            Down
            UpLeft
            UpRight
            DownLeft
            DownRight
        End Enum
        Public Enum GripMode
            Left
            Right
        End Enum
#End Region
#Region "Drawing Path"
        ''' <summary>
        ''' Create a rounded corner rectangle.
        ''' </summary>
        ''' <param name="rect">The rectangle to be rounded.</param>
        ''' <param name="topLeft">Range of the top left corner from the rectangle to be rounded.</param>
        ''' <param name="topRight">Range of the top right corner from the rectangle to be rounded.</param>
        ''' <param name="bottomLeft">Range of the bottom left corner from the rectangle to be rounded.</param>
        ''' <param name="bottomRight">Range of the bottom right corner from the rectangle to be rounded.</param>
        ''' <returns>A GraphicsPath object that represent a rectangle that have its corners rounded.</returns>
        ''' <remarks>The <c>range</c> must be greater than or equal with zero, and must be less then or equal with a half of its rectangle's width or height.
        ''' If the <c>range</c> value less than zero, then its return the rect parameter.
        ''' If rectangle width greater than its height, then maximum value of <c>range</c> is a half of rectangle height.
        ''' There are optionally rounded on its four corner.</remarks>
        Public Shared Function roundedRectangle( rect As Rectangle,
        Optional  topLeft As Integer = 0,
        Optional  topRight As Integer = 0,
        Optional  bottomLeft As Integer = 0,
        Optional  bottomRight As Integer = 0) As GraphicsPath
            Dim result As GraphicsPath = New GraphicsPath
            If rect.Width > 0 And rect.Height > 0 Then
                Dim maxAllowed As Integer
                If rect.Height < rect.Width Then
                    maxAllowed = Math.Floor(rect.Height / 2)
                Else
                    maxAllowed = Math.Floor(rect.Width / 2)
                End If
                Dim startPoint As PointF, endPoint As PointF
                With rect
                    If topLeft > 0 And topLeft < maxAllowed Then
                        result.AddArc(.X, .Y, topLeft * 2, topLeft * 2, 180, 90)
                        startPoint = New PointF(.X + topLeft, .Y)
                        endPoint = New PointF(.X, .Y + topLeft)
                    Else
                        startPoint = New PointF(.X, .Y)
                        endPoint = New PointF(.X, .Y)
                    End If
                    If topRight > 0 And topRight < maxAllowed Then
                        result.AddLine(startPoint.X, startPoint.Y, .Right - (topRight + 1), .Y)
                        result.AddArc(.Right - ((topRight * 2) + 1), .Y, topRight * 2, topRight * 2, 270, 90)
                        startPoint = New Point(.Right - 1, .Y + topRight)
                    Else
                        result.AddLine(startPoint.X, startPoint.Y, .Right - 1, .Y)
                        startPoint = New Point(.Right - 1, .Y)
                    End If
                    If bottomRight > 0 And bottomRight < maxAllowed Then
                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Bottom - (bottomRight + 1))
                        result.AddArc(.Right - ((bottomRight * 2) + 1), .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 0, 90)
                        startPoint = New Point(.Right - (bottomRight + 1), .Bottom - 1)
                    Else
                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Bottom - 1)
                        startPoint = New Point(.Right - 1, .Bottom - 1)
                    End If
                    If bottomLeft > 0 And bottomLeft < maxAllowed Then
                        result.AddLine(startPoint.X, startPoint.Y, .X + bottomLeft, startPoint.Y)
                        result.AddArc(.X, .Bottom - ((bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 90, 90)
                        startPoint = New Point(.X, .Bottom - (bottomLeft + 1))
                    Else
                        result.AddLine(startPoint.X, startPoint.Y, .X, startPoint.Y)
                        startPoint = New Point(.X, startPoint.Y)
                    End If
                    result.AddLine(startPoint, endPoint)
                    result.CloseFigure()
                    Return result
                End With
            End If
            ' Return the rect param.
            result.AddRectangle(rect)
            Return result
        End Function
        ''' <summary>
        ''' Create a lighting glow path from a rectangle.
        ''' </summary>
        ''' <returns>A GraphicsPath object that represent a lighting glow.</returns>
        ''' <param name="rect">The rectangle where lighting glow path to be created.</param>
        ''' <param name="glowPoint">One of <see cref="LightingGlowPoint">LightingGlowPoint</see> enumeration value.  Determine where the light starts.</param>
        ''' <param name="percentWidth">Percentage of rectangle's width used to create the path.</param>
        ''' <param name="percentHeight">Percentage of rectangle's height used to create the path.</param>
        ''' <param name="customX">X location where the light starts.  Used when glowPoint value is LightingGlowPoint.Custom.</param>
        ''' <param name="customY">Y location where the light starts.  Used when glowPoint value is LightingGlowPoint.Custom.</param>
        Public Shared Function getGlowingPath( rect As Rectangle,
        Optional  glowPoint As LightingGlowPoint = LightingGlowPoint.BottomCenter,
        Optional  percentWidth As Integer = 100,
        Optional  percentHeight As Integer = 100,
        Optional  customX As Integer = 0,
        Optional  customY As Integer = 0) As GraphicsPath
            Dim arcRect As Rectangle
            Dim ePath As GraphicsPath = New GraphicsPath
            Select Case glowPoint
                Case LightingGlowPoint.TopLeft
                    arcRect = New Rectangle(
                    rect.X - (rect.Width * percentWidth / 100),
                    rect.Y - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddLine(rect.X, rect.Y, CInt(rect.X + (rect.Width * percentWidth / 100)), rect.Y)
                    ePath.AddArc(arcRect, 0, 90)
                    ePath.AddLine(rect.X, rect.Y + CInt(rect.Height * percentHeight / 100), rect.X, rect.Y)
                Case LightingGlowPoint.TopCenter
                    arcRect = New Rectangle(
                    (rect.X + (rect.Width / 2)) - (rect.Width * percentWidth / 200),
                    rect.Y - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddLine(rect.X + CInt(rect.Width * (100 - percentWidth) / 200),
                    rect.Y, rect.Right - CInt(rect.Width * (100 - percentWidth) / 200),
                    rect.Y)
                    ePath.AddArc(arcRect, 0, 180)
                Case LightingGlowPoint.TopRight
                    arcRect = New Rectangle(
                    rect.Right - (rect.Width * percentWidth / 100),
                    rect.Y - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddLine(rect.Right - CInt(rect.Width * percentWidth / 100),
                    rect.Y, rect.Right, rect.Y)
                    ePath.AddLine(rect.Right, rect.Y, rect.Right,
                    rect.Y + CInt(rect.Height * percentHeight / 100))
                    ePath.AddArc(arcRect, 90, 90)
                Case LightingGlowPoint.MiddleLeft
                    arcRect = New Rectangle(
                    rect.X - (rect.Width * percentWidth / 100),
                    (rect.Y + (rect.Height / 2)) - (rect.Height * percentHeight / 200),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight / 100)
                    ePath.AddArc(arcRect, 270, 180)
                    ePath.AddLine(rect.X,
                    rect.Bottom - CInt(rect.Height * (100 - percentHeight) / 200),
                    rect.X,
                    rect.Y + CInt(rect.Height * (100 - percentHeight) / 200))
                Case LightingGlowPoint.MiddleCenter
                    arcRect = New Rectangle(
                    (rect.X + (rect.Width / 2)) - (rect.Width * percentWidth / 200),
                    (rect.Y + (rect.Height / 2)) - (rect.Height * percentHeight / 200),
                    rect.Width * percentWidth / 100,
                    rect.Height * percentHeight / 100)
                    ePath.AddEllipse(arcRect)
                Case LightingGlowPoint.MiddleRight
                    arcRect = New Rectangle(
                    rect.Right - (rect.Width * percentWidth / 100),
                    (rect.Y + (rect.Height / 2)) - (rect.Height * percentHeight / 200),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight / 100)
                    ePath.AddLine(rect.Right,
                    rect.Bottom - CInt(rect.Height * (100 - percentHeight) / 200),
                    rect.Right,
                    rect.Y + CInt(rect.Height * (100 - percentHeight) / 200))
                    ePath.AddArc(arcRect, 90, 180)
                Case LightingGlowPoint.BottomLeft
                    arcRect = New Rectangle(
                    rect.X - (rect.Width * percentWidth / 100),
                    rect.Bottom - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddArc(arcRect, 270, 90)
                    ePath.AddLine(CInt(rect.X + (rect.Width * percentWidth / 100)), rect.Bottom, rect.X, rect.Bottom)
                    ePath.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom - CInt(rect.Height * percentHeight / 100))
                Case LightingGlowPoint.BottomCenter
                    arcRect = New Rectangle(
                    (rect.X + (rect.Width / 2)) - (rect.Width * percentWidth / 200),
                    rect.Bottom - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddArc(arcRect, 180, 180)
                    ePath.AddLine(rect.X + CInt(rect.Width * (100 - percentWidth) / 200),
                    rect.Bottom, rect.Right - CInt(rect.Width * (100 - percentWidth) / 200),
                    rect.Bottom)
                Case LightingGlowPoint.BottomRight
                    arcRect = New Rectangle(
                    rect.Right - (rect.Width * percentWidth / 100),
                    rect.Bottom - (rect.Height * percentHeight / 100),
                    rect.Width * percentWidth * 2 / 100,
                    rect.Height * percentHeight * 2 / 100)
                    ePath.AddArc(arcRect, 180, 90)
                    ePath.AddLine(rect.Right,
                    rect.Bottom - CInt(rect.Height * percentHeight / 100),
                    rect.Right, rect.Bottom)
                    ePath.AddLine(rect.Right, rect.Bottom,
                    rect.Right - CInt(rect.Width * percentWidth / 100),
                    rect.Bottom)
                Case LightingGlowPoint.Custom
                    arcRect = New Rectangle(
                    (rect.X + customX) - (rect.Width * percentWidth / 200),
                    (rect.Y + customY) - (rect.Height * percentHeight / 200),
                    rect.Width * percentWidth / 100,
                    rect.Height * percentHeight / 100)
                    ePath.AddEllipse(arcRect)
            End Select
            ePath.CloseFigure()
            Return ePath
        End Function
        ''' <summary>
        ''' Create a GraphicsPath object represent an inner shadow of a rectangle.
        ''' </summary>
        ''' <returns>A GraphicsPath object that represent an inner shadow.</returns>
        ''' <param name="rect">The rectangle where shadow path to be created.</param>
        ''' <param name="shadow">One of <see cref="ShadowPoint">ShadowPoint</see> enumeration value.  Determine the place of the shadow inside the rectangle.</param>
        ''' <param name="verticalRange">Shadow height, calculated from top or bottom of the rectange.</param>
        ''' <param name="horizontalRange">Shadow width, calculated from left or right of the rectangle.</param>
        ''' <param name="topLeft">Rounded range of the rectangle's top left corner.</param>
        ''' <param name="topRight">Rounded range of the rectangle's top right corner.</param>
        ''' <param name="bottomLeft">Rounded range of the rectangle's bottom left corner.</param>
        ''' <param name="bottomRight">Rounded range of the rectangle's bottom right corner.</param>
        ''' <remarks><seealso cref="ShadowPoint"/></remarks>
        Public Shared Function getInnerShadowPath( rect As Rectangle,
        Optional  shadow As ShadowPoint = ShadowPoint.Top,
        Optional  verticalRange As Integer = 2,
        Optional  horizontalRange As Integer = 2,
        Optional  topLeft As Integer = 0,
        Optional  topRight As Integer = 0,
        Optional  bottomLeft As Integer = 0,
        Optional  bottomRight As Integer = 0) As GraphicsPath
            Dim result As GraphicsPath = New GraphicsPath
            If rect.Width > 0 And rect.Height > 0 Then
                Dim maxAllowed As Integer
                If rect.Height < rect.Width Then
                    maxAllowed = Math.Floor(rect.Height / 2)
                Else
                    maxAllowed = Math.Floor(rect.Width / 2)
                End If
                If verticalRange < Math.Floor(rect.Height / 2) And horizontalRange < Math.Floor(rect.Width / 2) Then
                    ' Building shadow
                    With rect
                        Select Case shadow
                            Case ShadowPoint.Top, ShadowPoint.TopLeft, ShadowPoint.TopRight
                                ' Shadow from top
                                Dim startPoint As PointF, endPoint As PointF
                                If topLeft > 0 And topLeft < maxAllowed Then
                                    result.AddArc(.X, .Y, topLeft * 2, topLeft * 2, 180, 90)
                                    startPoint = New PointF(.X + topLeft, .Y)
                                    endPoint = New PointF(.X, .Y + topLeft)
                                Else
                                    startPoint = New PointF(.X, .Y)
                                    endPoint = New PointF(.X, .Y)
                                End If
                                If topRight > 0 And topRight < maxAllowed Then
                                    result.AddLine(startPoint.X, startPoint.Y, .Right - (topRight + 1), .Y)
                                    result.AddArc(.Right - ((topRight * 2) + 1), .Y, topRight * 2, topRight * 2, 270, 90)
                                    startPoint = New PointF(.Right - 1, .Y + topRight)
                                Else
                                    result.AddLine(startPoint.X, startPoint.Y, .Right - 1, .Y)
                                    startPoint = New PointF(.Right - 1, .Y)
                                End If
                                If shadow = ShadowPoint.TopRight Then
                                    If bottomRight > 0 And bottomRight < maxAllowed Then
                                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Bottom - (bottomRight + 1))
                                        result.AddArc(.Right - ((bottomRight * 2) + 1), .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 0, 90)
                                        startPoint = New PointF(.Right - (bottomRight + 1), .Bottom - 1)
                                    Else
                                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Bottom - 1)
                                        startPoint = New PointF(startPoint.X, .Bottom - 1)
                                    End If
                                    result.AddLine(startPoint.X, startPoint.Y, startPoint.X - horizontalRange, startPoint.Y)
                                    startPoint = New PointF(startPoint.X - horizontalRange, startPoint.Y)
                                    If bottomRight > 0 And bottomRight < maxAllowed Then
                                        result.AddArc(startPoint.X - bottomRight, .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 90, -90)
                                        startPoint = New PointF(startPoint.X + bottomRight, .Bottom - (bottomRight + 1))
                                    End If
                                    If topRight > 0 And topRight < maxAllowed Then
                                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Y + topRight + verticalRange)
                                        result.AddArc(.Right - (horizontalRange + (topRight * 2) + 1), .Y + verticalRange, topRight * 2, topRight * 2, 0, -90)
                                        startPoint = New PointF(.Right - (horizontalRange + topRight + 1), .Y + verticalRange)
                                    Else
                                        result.AddLine(startPoint.X, startPoint.Y, startPoint.X, .Y + verticalRange)
                                        startPoint = New PointF(.Right - (horizontalRange + 1), .Y + verticalRange)
                                    End If
                                Else
                                    result.AddLine(startPoint.X, startPoint.Y, startPoint.X, startPoint.Y + verticalRange)
                                    startPoint = New PointF(startPoint.X, startPoint.Y + verticalRange)
                                    If topRight > 0 And topRight < maxAllowed Then
                                        result.AddArc(.Right - ((topRight * 2) + 1), startPoint.Y - topRight, topRight * 2, topRight * 2, 0, -90)
                                        startPoint = New PointF(.Right - 1, startPoint.Y - topRight)
                                    End If
                                End If
                                If shadow = ShadowPoint.TopLeft Then
                                    If topLeft > 0 And topLeft < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(.X + horizontalRange + topLeft, startPoint.Y))
                                        result.AddArc(.X + horizontalRange, .Y + verticalRange, topLeft * 2, topLeft * 2, 270, -90)
                                        startPoint = New PointF(.X + horizontalRange, .Y + verticalRange + topLeft)
                                    Else
                                        result.AddLine(startPoint, New PointF(.X + horizontalRange, startPoint.Y))
                                        startPoint = New PointF(.X + horizontalRange, .Y + verticalRange)
                                    End If
                                    If bottomLeft > 0 And bottomLeft < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Bottom - (bottomLeft + 1)))
                                        result.AddArc(.X + horizontalRange, .Bottom - ((bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 180, -90)
                                        result.AddLine(.X + horizontalRange + bottomLeft, .Bottom - 1, .X + bottomLeft, .Bottom - 1)
                                        result.AddArc(.X, .Bottom - ((bottomLeft * 2) - 1), bottomLeft * 2, bottomLeft * 2, 90, 90)
                                        startPoint = New PointF(.X, .Bottom - (bottomLeft + 1))
                                    Else
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Bottom - 1))
                                        result.AddLine(startPoint.X, .Bottom - 1, .X, .Bottom - 1)
                                        startPoint = New PointF(.X, .Bottom - 1)
                                    End If
                                Else
                                    If topLeft > 0 And topLeft < maxAllowed Then
                                        result.AddLine(startPoint.X, startPoint.Y, .X + topLeft, startPoint.Y)
                                        result.AddArc(.X, startPoint.Y, topLeft * 2, topLeft * 2, 270, -90)
                                        startPoint = New PointF(.X, startPoint.Y + topLeft)
                                    Else
                                        result.AddLine(startPoint.X, startPoint.Y, .X, startPoint.Y)
                                        startPoint = New PointF(.X, startPoint.Y)
                                    End If
                                End If
                                result.AddLine(startPoint, endPoint)
                                result.CloseFigure()
                                Return result
                            Case ShadowPoint.Bottom, ShadowPoint.BottomLeft, ShadowPoint.BottomRight
                                ' Shadow from bottom
                                Dim startPoint As PointF, endPoint As PointF
                                If bottomLeft > 0 And bottomLeft < maxAllowed Then
                                    result.AddArc(.X, .Bottom - ((bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 180, -90)
                                    startPoint = New PointF(.X + bottomLeft, .Bottom - 1)
                                    endPoint = New PointF(.X, .Bottom - (bottomLeft + 1))
                                Else
                                    startPoint = New PointF(.X, .Bottom - 1)
                                    endPoint = New PointF(.X, .Bottom - 1)
                                End If
                                If bottomRight > 0 And bottomRight < maxAllowed Then
                                    result.AddLine(startPoint, New PointF(.Right - (bottomRight + 1), .Bottom - 1))
                                    result.AddArc(.Right - ((bottomRight * 2) + 1), .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 90, -90)
                                    startPoint = New PointF(.Right - 1, .Bottom - (bottomRight + 1))
                                Else
                                    result.AddLine(startPoint, New PointF(.Right - 1, .Bottom - 1))
                                    startPoint = New PointF(.Right - 1, .Bottom - 1)
                                End If
                                If shadow = ShadowPoint.BottomRight Then
                                    If topRight > 0 And topRight < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Y + topRight + 1))
                                        result.AddArc(.Right - ((topRight * 2) + 1), .Y, topRight * 2, topRight * 2, 0, -90)
                                        startPoint = New PointF(.Right - (topRight + 1), .Y)
                                    Else
                                        result.AddLine(startPoint, New PointF(.Right - 1, .Y))
                                        startPoint = New PointF(.Right - 1, .Y)
                                    End If
                                    result.AddLine(startPoint, New PointF(startPoint.X - horizontalRange, .Y))
                                    startPoint = New PointF(startPoint.X - horizontalRange, .Y)
                                    If topRight > 0 And topRight < maxAllowed Then
                                        result.AddArc(startPoint.X - topRight, .Y, topRight * 2, topRight * 2, 270, 90)
                                        startPoint = New PointF(startPoint.X + topRight, .Y + topRight)
                                    End If
                                    If bottomRight > 0 And bottomRight < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Bottom - (bottomRight + verticalRange + 1)))
                                        result.AddArc(.Right - (horizontalRange + (bottomRight * 2) + 1), .Bottom - (verticalRange + (bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 0, 90)
                                        startPoint = New PointF(.Right - (horizontalRange + bottomRight + 1), .Bottom - (verticalRange + 1))
                                    Else
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Bottom - (verticalRange + 1)))
                                        startPoint = New PointF(startPoint.X, .Bottom - (verticalRange + 1))
                                    End If
                                Else
                                    result.AddLine(startPoint, New PointF(startPoint.X, startPoint.Y - verticalRange))
                                    startPoint = New PointF(startPoint.X, startPoint.Y - verticalRange)
                                    If bottomRight > 0 And bottomRight < maxAllowed Then
                                        result.AddArc(.Right - ((bottomRight * 2) + 1), startPoint.Y - bottomRight, bottomRight * 2, bottomRight * 2, 0, 90)
                                        startPoint = New PointF(.Right - (bottomRight + 1), startPoint.Y + bottomRight)
                                    End If
                                End If
                                If shadow = ShadowPoint.BottomLeft Then
                                    If bottomLeft > 0 And bottomLeft < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(.X + horizontalRange + bottomLeft, startPoint.Y))
                                        result.AddArc(.X + horizontalRange, .Bottom - (verticalRange + (bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 90, 90)
                                        startPoint = New PointF(.X + horizontalRange, .Bottom - (verticalRange + bottomLeft + 1))
                                    Else
                                        result.AddLine(startPoint, New PointF(.X + horizontalRange, startPoint.Y))
                                        startPoint = New PointF(.X + horizontalRange, .Bottom - (verticalRange + 1))
                                    End If
                                    If topLeft > 0 And topLeft < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Y + topLeft))
                                        result.AddArc(.X + horizontalRange, .Y, topLeft * 2, topLeft * 2, 180, 90)
                                        result.AddLine(.X + horizontalRange + topLeft, .Y, .X + topLeft, .Y)
                                        result.AddArc(.X, .Y, topLeft * 2, topLeft * 2, 270, -90)
                                        startPoint = New PointF(.X, .Y + topLeft)
                                    Else
                                        result.AddLine(startPoint, New PointF(startPoint.X, .Y))
                                        result.AddLine(startPoint.X, .Y, .X, .Y)
                                        startPoint = New PointF(.X, .Y)
                                    End If
                                Else
                                    If bottomLeft > 0 And bottomLeft < maxAllowed Then
                                        result.AddLine(startPoint, New PointF(.X + bottomLeft, startPoint.Y))
                                        result.AddArc(.X, .Bottom - (verticalRange + (bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 90, 90)
                                        startPoint = New PointF(.X, .Bottom - (verticalRange + bottomLeft + 1))
                                    Else
                                        result.AddLine(startPoint, New PointF(.X, startPoint.Y))
                                        startPoint = New PointF(.X, startPoint.Y)
                                    End If
                                End If
                                result.AddLine(startPoint, endPoint)
                                result.CloseFigure()
                                Return result
                            Case ShadowPoint.Left
                                ' Left only shadow
                                Dim startPoint As PointF, endPoint As PointF
                                If topLeft > 0 And topLeft < maxAllowed Then
                                    endPoint = New PointF(.X, .Y + topLeft)
                                    result.AddArc(.X, .Y, topLeft * 2, topLeft * 2, 180, 90)
                                    result.AddLine(.X + topLeft, .Y, .X + horizontalRange + topLeft, .Y)
                                    result.AddArc(.X + horizontalRange, .Y, topLeft * 2, topLeft * 2, 270, -90)
                                    startPoint = New PointF(.X + horizontalRange, .Y + topLeft)
                                Else
                                    endPoint = New PointF(.X, .Y)
                                    result.AddLine(.X, .Y, .X + horizontalRange, .Y)
                                    startPoint = New PointF(.X + horizontalRange, .Y)
                                End If
                                If bottomLeft > 0 And bottomLeft < maxAllowed Then
                                    result.AddLine(startPoint, New PointF(.X + horizontalRange, .Bottom - (bottomLeft + 1)))
                                    result.AddArc(.X + horizontalRange, .Bottom - ((bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 180, -90)
                                    result.AddLine(.X + horizontalRange + bottomLeft, .Bottom - 1, .X + bottomLeft, .Bottom - 1)
                                    result.AddArc(.X, .Bottom - ((bottomLeft * 2) + 1), bottomLeft * 2, bottomLeft * 2, 90, -90)
                                    startPoint = New PointF(.X, .Bottom - (bottomLeft + 1))
                                Else
                                    result.AddLine(startPoint, New PointF(.X + horizontalRange, .Bottom - 1))
                                    result.AddLine(.X + horizontalRange, .Bottom - 1, .X, .Bottom - 1)
                                    startPoint = New PointF(.X, .Bottom - 1)
                                End If
                                result.AddLine(startPoint, endPoint)
                                result.CloseFigure()
                                Return result
                            Case ShadowPoint.Right
                                ' Right only shadow
                                Dim startPoint As PointF, endPoint As PointF
                                If topRight > 0 And topRight < maxAllowed Then
                                    endPoint = New PointF(.Right - (horizontalRange + 1), .Y + topLeft)
                                    result.AddArc(.Right - ((topRight * 2) + horizontalRange + 1), .Y, topRight * 2, topRight * 2, 0, -90)
                                    result.AddLine(.Right - (topRight + horizontalRange + 1), .Y, .Right - (topRight + 1), .Y)
                                    result.AddArc(.Right - ((topRight * 2) + 1), .Y, topRight * 2, topRight * 2, 270, 90)
                                    startPoint = New PointF(.Right - 1, .Y + topRight)
                                Else
                                    endPoint = New PointF(.Right - (horizontalRange + 1), .Y)
                                    result.AddLine(endPoint, New PointF(.Right - 1, .Y))
                                    startPoint = New PointF(.Right - 1, .Y)
                                End If
                                If bottomRight > 0 And bottomRight < maxAllowed Then
                                    result.AddLine(startPoint, New PointF(.Right - 1, .Bottom - (bottomRight + 1)))
                                    result.AddArc(.Right - ((bottomRight * 2) + 1), .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 0, 90)
                                    result.AddLine(.Right - (bottomRight + 1), .Bottom - 1, .Right - (bottomRight + horizontalRange + 1), .Bottom - 1)
                                    result.AddArc(.Right - ((bottomRight * 2) + horizontalRange + 1), .Bottom - ((bottomRight * 2) + 1), bottomRight * 2, bottomRight * 2, 90, -90)
                                    startPoint = New PointF(.Right - (horizontalRange + 1), .Bottom - (bottomRight + 1))
                                Else
                                    result.AddLine(startPoint, New PointF(.Right - 1, .Bottom - 1))
                                    result.AddLine(.Right - 1, .Bottom - 1, .Right - (horizontalRange + 1), .Bottom - 1)
                                    startPoint = New PointF(.Right - (horizontalRange + 1), .Bottom - 1)
                                End If
                                result.AddLine(startPoint, endPoint)
                                result.CloseFigure()
                                Return result
                        End Select
                    End With
                End If
            End If
            result.AddRectangle(rect)
            Return result
        End Function
#End Region
#Region "Triangle"
        ''' <summary>
        ''' Draw a shadowed triangle specified by x and y location, overloaded.
        ''' </summary>
        ''' <param name="g">Graphics object where the triangle to be drawn.</param>
        ''' <param name="x">X location of the triangle.</param>
        ''' <param name="y">Y location of the triangle.</param>
        ''' <param name="color">Color of the triangle.</param>
        ''' <param name="shadowColor">Shadow color of the triangle.</param>
        ''' <param name="direction"><see cref="TriangleDirection">TriangleDirection</see>, direction of the triangle.</param>
        ''' <param name="size">Size of the triangle.</param>
        ''' <remarks></remarks>
        Public Shared Sub drawTriangle( g As Graphics,  x As Integer,  y As Integer,
         color As Color,  shadowColor As Color,
        Optional  direction As TriangleDirection = TriangleDirection.Down,
        Optional  size As Integer = 6)
            If size > 0 Then
                Dim points(0 To 3) As PointF
                Dim shadowPoints(0 To 3) As PointF
                Dim i As Integer
                Select Case direction
                    Case TriangleDirection.Up
                        points(0) = New PointF(x, y + (size - 1))
                        points(1) = New PointF(x + size, y + (size - 1))
                        points(2) = New PointF(x + (size / 2), y)
                    Case TriangleDirection.Down
                        points(0) = New PointF(x, y)
                        points(1) = New PointF(x + size, y)
                        points(2) = New PointF(x + (size / 2), y + (size - 1))
                    Case TriangleDirection.Left
                        points(0) = New PointF(x + (size - 1), y)
                        points(1) = New PointF(x + (size - 1), y + size)
                        points(2) = New PointF(x, y + (size / 2))
                    Case TriangleDirection.Right
                        points(0) = New PointF(x, y)
                        points(1) = New PointF(x, y + size)
                        points(2) = New PointF(x + (size - 1), y + (size / 2))
                    Case TriangleDirection.UpLeft, TriangleDirection.UpRight
                        points(0) = New PointF(x, y)
                        points(1) = New PointF(x + size, y)
                        If direction = TriangleDirection.UpLeft Then
                            points(2) = New PointF(x, y + size)
                        Else
                            points(2) = New PointF(x + size, y + size)
                        End If
                    Case TriangleDirection.DownLeft, TriangleDirection.DownRight
                        points(0) = New PointF(x, y + size)
                        points(1) = New PointF(x + size, y + size)
                        If direction = TriangleDirection.DownLeft Then
                            points(2) = New PointF(x, y)
                        Else
                            points(2) = New PointF(x + size, y)
                        End If
                End Select
                points(3) = points(0)
                ' Draw its shadow first
                If direction = TriangleDirection.Down Or direction = TriangleDirection.Up Or direction = TriangleDirection.Left _
                Or direction = TriangleDirection.Right Then
                    i = 0
                    While i < 4
                        shadowPoints(i) = New PointF(points(i).X, points(i).Y + 2)
                        i = i + 1
                    End While
                Else
                    i = 0
                    While i < 4
                        shadowPoints(i) = New PointF(points(i).X + 1, points(i).Y + 2)
                        i = i + 1
                    End While
                End If
                g.FillPolygon(New SolidBrush(shadowColor), shadowPoints)
                g.FillPolygon(New SolidBrush(color), points)
            End If
        End Sub
        ''' <summary>
        ''' Draw a shadowed triangle specified by p point location, overloaded.
        ''' </summary>
        ''' <param name="g">Graphics object where the triangle to be drawn.</param>
        ''' <param name="p">Point, location of the triangle.</param>
        ''' <param name="color">Color of the triangle.</param>
        ''' <param name="shadowColor">Shadow color of the triangle.</param>
        ''' <param name="direction"><see cref="TriangleDirection">TriangleDirection</see>, direction of the triangle.</param>
        ''' <param name="size">Size of the triangle.</param>
        ''' <remarks></remarks>
        Public Shared Sub drawTriangle( g As Graphics,  p As Point,
         color As Color,  shadowColor As Color,
        Optional  direction As TriangleDirection = TriangleDirection.Down,
        Optional  size As Integer = 6)
            drawTriangle(g, p.X, p.Y, color, shadowColor, direction, size)
        End Sub
        ''' <summary>
        ''' Draw a shadowed triangle in the center of a rectangle, overloaded.
        ''' </summary>
        ''' <param name="g">Graphics object where the triangle to be drawn.</param>
        ''' <param name="rect">Rectangle where the triangle to be drawn.</param>
        ''' <param name="color">Color of the triangle.</param>
        ''' <param name="shadowColor">Shadow color of the triangle.</param>
        ''' <param name="direction"><see cref="TriangleDirection">TriangleDirection</see>, direction of the triangle.</param>
        ''' <param name="size">Size of the triangle.</param>
        ''' <remarks></remarks>
        Public Shared Sub drawTriangle( g As Graphics,  rect As Rectangle,  color As Color,
         shadowColor As Color, Optional  direction As TriangleDirection = TriangleDirection.Down,
        Optional  size As Integer = 6)
            Dim x As Integer = rect.X + ((rect.Width - size) / 2)
            Dim y As Integer = rect.Y + ((rect.Height - size) / 2)
            drawTriangle(g, x, y, color, shadowColor, direction, size)
        End Sub
#End Region
#Region "Imaging"
        ''' <summary>
        ''' Get a resized bounding rectangle of an image specified by maxSize.
        ''' </summary>
        ''' <param name="img">Image to measure.</param>
        ''' <param name="maxSize">Maximum width or height of the result.</param>
        ''' <returns>A rectangle represent resized bounding of an image.</returns>
        ''' <remarks>If image is nothing, a (0, 0, 0, 0) rectangle returned.</remarks>
        Public Shared Function getImageRectangle( img As Image,
         maxSize As Integer) As Rectangle
            Dim iRect As Rectangle = New Rectangle(0, 0, 0, 0)
            If img IsNot Nothing Then
                If img.Width <= maxSize And img.Height <= maxSize Then
                    iRect.Width = img.Width
                    iRect.Height = img.Height
                Else
                    If img.Width = img.Height Then
                        iRect.Width = maxSize
                        iRect.Height = maxSize
                    Else
                        If img.Width > img.Height Then
                            iRect.Width = maxSize
                            iRect.Height = img.Height * maxSize / img.Width
                        Else
                            iRect.Height = maxSize
                            iRect.Width = img.Width * maxSize / img.Height
                        End If
                    End If
                End If
            End If
            Return iRect
        End Function
        ''' <summary>
        ''' Get a resized bounding rectangle of an image specified by maxSize inside of a rectangle.
        ''' </summary>
        ''' <param name="img">Image to measure.</param>
        ''' <param name="rect">Rectangle where the result to be placed.</param>
        ''' <param name="maxSize">Maximum width or height of the result.</param>
        ''' <returns>A rectangle represent resized bounding of an image.</returns>
        ''' <remarks>If image is nothing, rect parameter returned.</remarks>
        Public Shared Function getImageRectangle( img As Image,
         rect As Rectangle,  maxSize As Integer) As Rectangle
            If img IsNot Nothing Then
                Dim iRect As Rectangle
                If img.Width <= maxSize And img.Height <= maxSize Then
                    iRect.Width = img.Width
                    iRect.Height = img.Height
                Else
                    If img.Width = img.Height Then
                        iRect.Width = maxSize
                        iRect.Height = maxSize
                    Else
                        If img.Width > img.Height Then
                            iRect.Width = maxSize
                            iRect.Height = img.Height * maxSize / img.Width
                        Else
                            iRect.Height = maxSize
                            iRect.Width = img.Width * maxSize / img.Height
                        End If
                    End If
                End If
                iRect.X = rect.X + ((rect.Width - iRect.Width) / 2)
                iRect.Y = rect.Y + ((rect.Height - iRect.Height) / 2)
                Return iRect
            End If
            Return rect
        End Function
        ''' <summary>
        ''' Get a resized bounding rectangle of an image specified by maximum width and maximum height inside of a rectangle.
        ''' </summary>
        ''' <param name="img">Image to measure.</param>
        ''' <param name="rect">Rectangle where the result to be placed.</param>
        ''' <param name="maxWidth">Maximum width of the result.</param>
        ''' <param name="maxHeight">Maximum height of the result.</param>
        ''' <returns>A rectangle represent resized bounding of an image.</returns>
        ''' <remarks>If image is nothing, rect parameter returned.</remarks>
        Public Shared Function getImageRectangle( img As Image,
         rect As Rectangle,  maxWidth As Integer,
         maxHeight As Integer) As Rectangle
            If img IsNot Nothing Then
                Dim iRect As Rectangle
                If img.Width <= maxWidth And img.Height <= maxHeight Then
                    iRect.Width = img.Width
                    iRect.Height = img.Height
                Else
                    If img.Width = img.Height Then
                        iRect.Width = IIf(maxWidth > maxHeight, maxHeight, maxWidth)
                        iRect.Height = IIf(maxWidth > maxHeight, maxHeight, maxWidth)
                    Else
                        If img.Width / img.Height > maxWidth / maxHeight Then
                            iRect.Width = maxWidth
                            iRect.Height = img.Height * maxWidth / img.Width
                        Else
                            iRect.Height = maxHeight
                            iRect.Width = img.Width * maxHeight / img.Height
                        End If
                    End If
                End If
                iRect.X = rect.X + ((rect.Width - iRect.Width) / 2)
                iRect.Y = rect.Y + ((rect.Height - iRect.Height) / 2)
                Return iRect
            End If
            Return rect
        End Function
        ''' <summary>
        ''' Resize an image to fit maximum value.
        ''' </summary>
        ''' <param name="image">Image to measure.</param>
        ''' <param name="max">Maximum width or height of the result.</param>
        ''' <returns>A size represent resized image size.</returns>
        ''' <remarks>If image is nothing, a (0, 0) size returned.</remarks>
        Public Shared Function scaleImage( image As Image,  max As Integer) As Size
            Dim result As Size = New Size(0, 0)
            If image IsNot Nothing Then
                If image.Width = image.Height Then
                    result = New Size(max, max)
                Else
                    If image.Width > image.Height Then
                        result = New Size(max, max * image.Height / image.Width)
                    Else
                        result = New Size(max * image.Width / image.Height, max)
                    End If
                End If
            End If
            Return result
        End Function
        ''' <summary>
        ''' Draw a grayscaled image from an image inside a rectangle.
        ''' </summary>
        ''' <param name="image">Image to be drawn.</param>
        ''' <param name="rect">Rectangle where a grayscaled image to be drawn.</param>
        ''' <param name="g">Graphics object where the grayscaled image to be drawn.</param>
        Public Shared Sub grayscaledImage( image As Image,
         rect As Rectangle,  g As Graphics)
            If image IsNot Nothing Then
                Dim grayMatrix As System.Drawing.Imaging.ColorMatrix
                Dim i As Integer, j As Integer
                Dim imgAttr As System.Drawing.Imaging.ImageAttributes
                grayMatrix = New System.Drawing.Imaging.ColorMatrix
                i = 0
                While i < 5
                    j = 0
                    While j < 5
                        grayMatrix.Item(i, j) = 0.0F
                        j = j + 1
                    End While
                    i = i + 1
                End While
                grayMatrix.Item(0, 0) = 0.299F
                grayMatrix.Item(0, 1) = 0.299F
                grayMatrix.Item(0, 2) = 0.299F
                grayMatrix.Item(1, 0) = 0.587F
                grayMatrix.Item(1, 1) = 0.587F
                grayMatrix.Item(1, 2) = 0.587F
                grayMatrix.Item(2, 0) = 0.114F
                grayMatrix.Item(2, 1) = 0.114F
                grayMatrix.Item(2, 2) = 0.114F
                grayMatrix.Item(3, 3) = 1.0F
                grayMatrix.Item(4, 4) = 1.0F
                imgAttr = New System.Drawing.Imaging.ImageAttributes
                imgAttr.SetColorMatrix(grayMatrix)
                g.DrawImage(image, rect, 0, 0, image.Width,
                image.Height, GraphicsUnit.Pixel, imgAttr)
            End If
        End Sub
#End Region
#Region "Miscellaneous"
        ''' <summary>
        ''' Get a color structure from an AHSB value
        ''' </summary>
        ''' <param name="a">Alpha value from the color.</param>
        ''' <param name="h">Hue value from the color.</param>
        ''' <param name="s">Saturation value from the color.</param>
        ''' <param name="b">Brightness value from the color.</param>
        ''' <returns>A color structure represent AHSB value.</returns>
        Public Shared Function colorFromAHSB( a As Integer,  h As Single,
         s As Single,  b As Single) As Color
            ' http://130.113.54.154/~monger/hsl-rgb.html
            If h < 0.0F Or h > 360.0F Or s < 0.0F Or s > 1.0F Or b < 0.0F Or b > 1.0F Then Return Color.Black
            If s = 0.0F Then Return Color.FromArgb(a, 255 * b, 255 * b, 255 * b)
            Dim temp1 As Single, temp2 As Single
            Dim hConv As Single = h / 360
            Dim tmps(0 To 2) As Single
            Dim i As Integer
            If b < 0.5F Then
                temp2 = b * (1 + s)
            Else
                temp2 = (b + s) - (b * s)
            End If
            temp1 = (2 * b) - temp2
            tmps(0) = hConv + (1 / 3)
            tmps(1) = hConv
            tmps(2) = hConv - (1 / 3)
            i = 0
            While i < 3
                If tmps(i) < 0 Then tmps(i) = tmps(i) + 1.0F
                If tmps(i) > 1 Then tmps(i) = tmps(i) - 1.0F
                If 6.0F * tmps(i) < 1 Then
                    tmps(i) = temp1 + (temp2 - temp1) * 6.0F * tmps(i)
                ElseIf 2.0F * tmps(i) < 1 Then
                    tmps(i) = temp2
                ElseIf 3.0F * tmps(i) < 2 Then
                    tmps(i) = temp1 + (temp2 - temp1) * ((2.0F / 3.0F) - tmps(i)) * 6.0F
                End If
                i = i + 1
            End While
            Return Color.FromArgb(a, 255 * tmps(0), 255 * tmps(1), 255 * tmps(2))
        End Function
        Public Shared ReadOnly Property SizingGripBlend() As ColorBlend
            Get
                Dim aBlend As ColorBlend
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                aBlend = New ColorBlend
                colors(0) = Color.White
                colors(1) = Color.FromArgb(223, 233, 239)
                pos(0) = 0
                pos(1) = 1
                aBlend.Colors = colors
                aBlend.Positions = pos
                Return aBlend
            End Get
        End Property
        Public Shared ReadOnly Property GripBorderPen() As Pen
            Get
                Return New Pen(Color.FromArgb(221, 231, 238))
            End Get
        End Property
        Public Shared ReadOnly Property GripDotBrush() As Brush
            Get
                Return New SolidBrush(Color.FromArgb(82, 116, 167))
            End Get
        End Property
        Public Shared Sub drawGrip( p As Point,  g As Graphics, Optional  mode As GripMode = GripMode.Right)
            If mode = GripMode.Right Then
                drawRightBottomGrid(g, p.X, p.Y)
            Else
                drawLeftBottomGrid(g, p.X, p.Y)
            End If
        End Sub
        Public Shared Sub drawGrip( x As Integer,  y As Integer,  g As Graphics, Optional  mode As GripMode = GripMode.Right)
            If mode = GripMode.Right Then
                drawRightBottomGrid(g, x, y)
            Else
                drawLeftBottomGrid(g, x, y)
            End If
        End Sub
        Public Shared Sub drawVGrip( rect As Rectangle,  g As Graphics)
            Dim aRect As Rectangle = New Rectangle(
            rect.X + ((rect.Width - 20) / 2), rect.Y + 1, 20, 7)
            drawBottomGrid(g, aRect.X, aRect.Y)
        End Sub
        Public Shared Sub drawRightBottomGrid( g As Graphics,  x As Integer,  y As Integer)
            Dim rectDot As Rectangle = New Rectangle(0, 0, 2, 2)
            rectDot.X = x + 5
            rectDot.Y = y + 4
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
            rectDot.X = x + 5
            rectDot.Y = y + 7
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
            rectDot.X = x + 1
            rectDot.Y = y + 7
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
        End Sub
        Public Shared Sub drawLeftBottomGrid( g As Graphics,  x As Integer,  y As Integer)
            Dim rectDot As Rectangle = New Rectangle(0, 0, 2, 2)
            rectDot.X = x + 1
            rectDot.Y = y + 4
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
            rectDot.X = x + 5
            rectDot.Y = y + 7
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
            rectDot.X = x + 1
            rectDot.Y = y + 7
            g.FillEllipse(Brushes.White, rectDot)
            rectDot.X = rectDot.X + 1
            rectDot.Y = rectDot.Y - 1
            g.FillEllipse(GripDotBrush, rectDot)
        End Sub
        Public Shared Sub drawBottomGrid( g As Graphics,  x As Integer,  y As Integer)
            Dim rectDot As Rectangle = New Rectangle(0, 0, 2, 2)
            Dim i As Integer
            rectDot.X = x + 3
            rectDot.Y = y + 3
            i = 0
            While i < 4
                g.FillEllipse(Brushes.White, rectDot)
                rectDot.X = rectDot.X - 1
                rectDot.Y = rectDot.Y - 1
                g.FillEllipse(GripDotBrush, rectDot)
                rectDot.X = rectDot.X + 6
                rectDot.Y = rectDot.Y + 1
                i = i + 1
            End While
        End Sub
        Public Shared ReadOnly Property NormalTextBrush(Optional  theme As ColorTheme = ColorTheme.Blue) As SolidBrush
            Get
                Select Case theme
                    Case ColorTheme.Blue
                        Return New SolidBrush(Color.Black)
                    Case ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(255, 255, 255))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property DisabledTextBrush(Optional  theme As ColorTheme = ColorTheme.Blue) As SolidBrush
            Get
                Select Case theme
                    Case ColorTheme.Blue, ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(118, 118, 118))
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property
        Public Shared Function compareColor( clr1 As Color,  clr2 As Color) As Boolean
            Return clr1.A = clr2.A And clr1.R = clr2.R And clr1.G = clr2.G And clr1.B = clr2.B
        End Function
#End Region
    End Class
End Namespace
