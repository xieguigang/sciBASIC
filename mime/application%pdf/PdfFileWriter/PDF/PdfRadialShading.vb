﻿#Region "Microsoft.VisualBasic::de12f037db6ea1670c77e8dc72c9d543, mime\application%pdf\PdfFileWriter\PDF\PdfRadialShading.vb"

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

    ' Class PdfRadialShading
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Sub: AntiAlias, ExtendShading, SetBoundingBox, SetGradientDirection, WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfRadialShading
'	PDF radial shading resource class.
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
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Imports System
Imports SysMedia = System.Windows.Media
Imports stdNum = System.Math

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
''' <summary>
''' PDF radial shading resource class
''' </summary>
''' <remarks>
''' Derived class from PdfObject
''' </remarks>
Public Class PdfRadialShading
    Inherits PdfObject

    Private BBoxLeft As Double
    Private BBoxBottom As Double
    Private BBoxRight As Double
    Private BBoxTop As Double
    Private Mapping As MappingMode
    Private StartCenterX As Double
    Private StartCenterY As Double
    Private StartRadius As Double
    Private EndCenterX As Double
    Private EndCenterY As Double
    Private EndRadius As Double
    Private ExtendShadingBefore As Boolean = True
    Private ExtendShadingAfter As Boolean = True

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' <summary>
    ''' PDF radial shading constructor.
    ''' </summary>
    ''' <param name="Document">Parent PDF document object</param>
    ''' <param name="BBoxLeft">Bounding box left position</param>
    ''' <param name="BBoxBottom">Bounding box bottom position</param>
    ''' <param name="BBoxWidth">Bounding box width</param>
    ''' <param name="BBoxHeight">Bounding box height</param>
    ''' <param name="ShadingFunction">Shading function</param>
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Sub New(ByVal Document As PdfDocument, ByVal BBoxLeft As Double, ByVal BBoxBottom As Double, ByVal BBoxWidth As Double, ByVal BBoxHeight As Double, ByVal ShadingFunction As PdfShadingFunction)
        MyBase.New(Document)
        ' create resource code
        ResourceCode = Document.GenerateResourceNumber("S"c)

        ' color space red, green and blue
        Dictionary.Add("/ColorSpace", "/DeviceRGB")

        ' shading type axial
        Dictionary.Add("/ShadingType", "3")

        ' add shading function to shading dictionary
        Dictionary.AddIndirectReference("/Function", ShadingFunction)

        ' bounding box
        Me.BBoxLeft = BBoxLeft
        Me.BBoxBottom = BBoxBottom
        BBoxRight = BBoxLeft + BBoxWidth
        BBoxTop = BBoxBottom + BBoxHeight

        ' assume the direction of color change is along x axis
        Mapping = MappingMode.Relative
        StartCenterX = 0.5
        StartCenterY = 0.5
        StartRadius = 0.0
        EndCenterX = 0.5
        EndCenterY = 0.5
        EndRadius = stdNum.Sqrt(0.5)
        Return
    End Sub

    ''' <summary>
    ''' PDF radial shading constructor for one unit bounding box
    ''' </summary>
    ''' <param name="Document">Parent PDF document object.</param>
    ''' <param name="ShadingFunction">Shading function.</param>
    Public Sub New(ByVal Document As PdfDocument, ByVal ShadingFunction As PdfShadingFunction)
        Me.New(Document, 0.0, 0.0, 1.0, 1.0, ShadingFunction)
    End Sub

    ''' <summary>
    ''' PDF radial shading constructor for one unit bounding box
    ''' </summary>
    ''' <param name="Document">Parent PDF document object.</param>
    ''' <param name="MediaBrush">System.Windows.Media brush</param>
    ''' <remarks>Support for WPF media</remarks>
    Public Sub New(ByVal Document As PdfDocument, ByVal MediaBrush As SysMedia.RadialGradientBrush)
        Me.New(Document, 0.0, 0.0, 1.0, 1.0, New PdfShadingFunction(Document, MediaBrush))
        SetGradientDirection(MediaBrush.Center.X, MediaBrush.Center.Y, 0.0, MediaBrush.GradientOrigin.X, MediaBrush.GradientOrigin.Y, 0.5 * (MediaBrush.RadiusX + MediaBrush.RadiusY), If(MediaBrush.MappingMode = SysMedia.BrushMappingMode.RelativeToBoundingBox, MappingMode.Relative, MappingMode.Absolute))
        Return
    End Sub

    ''' <summary>
    ''' Set bounding box
    ''' </summary>
    ''' <param name="BBoxLeft">Bounding box left</param>
    ''' <param name="BBoxBottom">Bounding box bottom</param>
    ''' <param name="BBoxWidth">Bounding box width</param>
    ''' <param name="BBoxHeight">Bounding box height</param>
    Public Sub SetBoundingBox(ByVal BBoxLeft As Double, ByVal BBoxBottom As Double, ByVal BBoxWidth As Double, ByVal BBoxHeight As Double)
        ' bounding box
        Me.BBoxLeft = BBoxLeft
        Me.BBoxBottom = BBoxBottom
        BBoxRight = BBoxLeft + BBoxWidth
        BBoxTop = BBoxBottom + BBoxHeight
        Return
    End Sub

    ''' <summary>
    ''' Set gradient direction
    ''' </summary>
    ''' <param name="StartCenterX">Start circle center x position</param>
    ''' <param name="StartCenterY">Start circle center y position</param>
    ''' <param name="StartRadius">Start circle center radius</param>
    ''' <param name="EndCenterX">End circle center x position</param>
    ''' <param name="EndCenterY">End circle center y position</param>
    ''' <param name="EndRadius">End circle center radius</param>
    ''' <param name="Mapping">Mapping mode (relative absolute)</param>
    Public Sub SetGradientDirection(ByVal StartCenterX As Double, ByVal StartCenterY As Double, ByVal StartRadius As Double, ByVal EndCenterX As Double, ByVal EndCenterY As Double, ByVal EndRadius As Double, ByVal Mapping As MappingMode)
        Me.StartCenterX = StartCenterX
        Me.StartCenterY = StartCenterY
        Me.StartRadius = StartRadius
        Me.EndCenterX = EndCenterX
        Me.EndCenterY = EndCenterY
        Me.EndRadius = EndRadius
        Me.Mapping = Mapping
        Return
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' <summary>
    ''' Sets anti-alias parameter
    ''' </summary>
    ''' <param name="Value">Anti-alias true or false</param>
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Sub AntiAlias(ByVal Value As Boolean)
        Dictionary.AddBoolean("/AntiAlias", Value)
        Return
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' <summary>
    ''' Extend shading beyond axis
    ''' </summary>
    ''' <param name="Before">Before (true or false)</param>
    ''' <param name="After">After (true or false)</param>
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Sub ExtendShading(ByVal Before As Boolean, ByVal After As Boolean)
        ExtendShadingBefore = Before
        ExtendShadingAfter = After
        Return
    End Sub

    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Write object to PDF file
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Friend Overrides Sub WriteObjectToPdfFile()
        ' bounding box
        Dictionary.AddRectangle("/BBox", BBoxLeft, BBoxBottom, BBoxRight, BBoxTop)

        ' absolute mapping mode
        If Mapping = MappingMode.Absolute Then
            ' relative mapping mode
            Dictionary.AddFormat("/Coords", "[{0} {1} {2} {3} {4} {5}]", ToPt(StartCenterX), ToPt(StartCenterY), ToPt(StartRadius), ToPt(EndCenterX), ToPt(EndCenterY), ToPt(EndRadius))
        Else
            Dim RelStartCenterX = BBoxLeft * (1.0 - StartCenterX) + BBoxRight * StartCenterX
            Dim RelStartCenterY = BBoxBottom * (1.0 - StartCenterY) + BBoxTop * StartCenterY
            Dim BBoxSide = stdNum.Min(stdNum.Abs(BBoxRight - BBoxLeft), stdNum.Abs(BBoxTop - BBoxBottom))
            Dim RelStartRadius = BBoxSide * StartRadius
            Dim RelEndCenterX = BBoxLeft * (1.0 - EndCenterX) + BBoxRight * EndCenterX
            Dim RelEndCenterY = BBoxBottom * (1.0 - EndCenterY) + BBoxTop * EndCenterY
            Dim RelEndRadius = BBoxSide * EndRadius
            Dictionary.AddFormat("/Coords", "[{0} {1} {2} {3} {4} {5}]", ToPt(RelStartCenterX), ToPt(RelStartCenterY), ToPt(RelStartRadius), ToPt(RelEndCenterX), ToPt(RelEndCenterY), ToPt(RelEndRadius))
        End If

        ' extend shading
        Dictionary.AddFormat("/Extend", "[{0} {1}]", If(ExtendShadingBefore, "true", "false"), If(ExtendShadingAfter, "true", "false"))

        ' call PdfObject base routine
        MyBase.WriteObjectToPdfFile()

        ' exit
        Return
    End Sub
End Class
