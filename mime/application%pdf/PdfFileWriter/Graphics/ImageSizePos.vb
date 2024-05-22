#Region "Microsoft.VisualBasic::fd59f6fbfab53fe3270199fcc52ba29e, mime\application%pdf\PdfFileWriter\Graphics\ImageSizePos.vb"

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

    '   Total Lines: 118
    '    Code Lines: 45 (38.14%)
    ' Comment Lines: 61 (51.69%)
    '    - Xml Docs: 55.74%
    ' 
    '   Blank Lines: 12 (10.17%)
    '     File Size: 5.38 KB


    ' Module ImageSizePos
    ' 
    '     Function: (+2 Overloads) ImageArea, ImageSize
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	ImageSizePoos
'	Support class for image aspect ratio calculations.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System.Drawing

''' <summary>
''' Image size and position class
''' </summary>
''' <remarks>
''' Delta X and Y are the adjustments to image position to
''' meet the content alignment request.
''' </remarks>
Public Module ImageSizePos

    ''' <summary>
    ''' Adjust image drawing area for both aspect ratio and content alignment
    ''' </summary>
    ''' <param name="ImageWidthPix">Image width in pixels.</param>
    ''' <param name="ImageHeightPix">Image height in pixels.</param>
    ''' <param name="DrawArea">Drawing area rectangle</param>
    ''' <param name="Alignment">Content alignment.</param>
    ''' <returns>Adjusted drawing area rectangle</returns>
    Public Function ImageArea(ImageWidthPix As Integer, ImageHeightPix As Integer, DrawArea As PdfRectangle, Alignment As ContentAlignment) As PdfRectangle
        Return ImageArea(ImageWidthPix, ImageHeightPix, DrawArea.Left, DrawArea.Bottom, DrawArea.Width, DrawArea.Height, Alignment)
    End Function

    ''' <summary>
    ''' Adjust image drawing area for both aspect ratio and content alignment
    ''' </summary>
    ''' <param name="ImageWidthPix">Image width in pixels.</param>
    ''' <param name="ImageHeightPix">Image height in pixels.</param>
    ''' <param name="DrawAreaLeft">Drawing area left side.</param>
    ''' <param name="DrawAreaBottom">Drawing area bottom side.</param>
    ''' <param name="DrawAreaWidth">Drawing area width.</param>
    ''' <param name="DrawAreaHeight">Drawing area height.</param>
    ''' <param name="Alignment">Content alignment.</param>
    ''' <returns>Adjusted drawing area rectangle</returns>
    Public Function ImageArea(ImageWidthPix As Integer, ImageHeightPix As Integer, DrawAreaLeft As Double, DrawAreaBottom As Double, DrawAreaWidth As Double, DrawAreaHeight As Double, Alignment As ContentAlignment) As PdfRectangle
        Dim DeltaX As Double = 0
        Dim DeltaY As Double = 0
        Dim Width As Double
        Dim Height As Double

        ' calculate height to fit aspect ratio
        Height = DrawAreaWidth * ImageHeightPix / ImageWidthPix

        If Height <= DrawAreaHeight Then
            Width = DrawAreaWidth

            If Height < DrawAreaHeight Then
                If Alignment = ContentAlignment.MiddleLeft OrElse Alignment = ContentAlignment.MiddleCenter OrElse Alignment = ContentAlignment.MiddleRight Then
                    DeltaY = 0.5 * (DrawAreaHeight - Height)
                ElseIf Alignment = ContentAlignment.TopLeft OrElse Alignment = ContentAlignment.TopCenter OrElse Alignment = ContentAlignment.TopRight Then
                    DeltaY = DrawAreaHeight - Height
                End If
                ' calculate width to fit aspect ratio
            End If
        Else
            Width = DrawAreaHeight * ImageWidthPix / ImageHeightPix
            Height = DrawAreaHeight

            If Width < DrawAreaWidth Then
                If Alignment = ContentAlignment.TopCenter OrElse Alignment = ContentAlignment.MiddleCenter OrElse Alignment = ContentAlignment.BottomCenter Then
                    DeltaX = 0.5 * (DrawAreaWidth - Width)
                ElseIf Alignment = ContentAlignment.TopRight OrElse Alignment = ContentAlignment.MiddleRight OrElse Alignment = ContentAlignment.BottomRight Then
                    DeltaX = DrawAreaWidth - Width
                End If
            End If
        End If

        ' position rectangle
        Return New PdfRectangle(DrawAreaLeft + DeltaX, DrawAreaBottom + DeltaY, DrawAreaLeft + DeltaX + Width, DrawAreaBottom + DeltaY + Height)
    End Function

    ''' <summary>
    ''' Calculate best fit to preserve aspect ratio
    ''' </summary>
    ''' <param name="ImageWidthPix">Image width in pixels.</param>
    ''' <param name="ImageHeightPix">Image height in pixels.</param>
    ''' <param name="DrawAreaWidth">Drawing area width.</param>
    ''' <param name="DrawAreaHeight">Drawing area height.</param>
    ''' <returns>Image size in user units.</returns>
    Public Function ImageSize(ImageWidthPix As Integer, ImageHeightPix As Integer, DrawAreaWidth As Double, DrawAreaHeight As Double) As SizeD
        Dim OutputSize As SizeD = New SizeD()
        OutputSize.Height = DrawAreaWidth * ImageHeightPix / ImageWidthPix

        If OutputSize.Height <= DrawAreaHeight Then
            OutputSize.Width = DrawAreaWidth
        Else
            OutputSize.Width = DrawAreaHeight * ImageWidthPix / ImageHeightPix
            OutputSize.Height = DrawAreaHeight
        End If

        Return OutputSize
    End Function
End Module
