#Region "Microsoft.VisualBasic::dc3562eeccd02b69f63f0414dae86959, mime\application%pdf\PdfFileWriter\PDF\PdfAxialShading.vb"

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

    '   Total Lines: 198
    '    Code Lines: 77 (38.89%)
    ' Comment Lines: 91 (45.96%)
    '    - Xml Docs: 58.24%
    ' 
    '   Blank Lines: 30 (15.15%)
    '     File Size: 7.27 KB


    ' Enum MappingMode
    ' 
    '     Absolute, Relative
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class PdfAxialShading
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: AntiAlias, ExtendShading, SetAxisDirection, SetBoundingBox, WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfAxialShading
'	PDF Axial shading indirect object.
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

''' <summary>
''' Mapping mode for axial and radial shading
''' </summary>
Public Enum MappingMode
        ''' <summary>
        ''' Relative to bounding box
        ''' </summary>
        Relative
        ''' <summary>
        ''' Absolute
        ''' </summary>
        Absolute
    End Enum

    
    ''' <summary>
    ''' PDF axial shading resource class
    ''' </summary>
    ''' <remarks>
    ''' Derived class from PdfObject
    ''' </remarks>
    
    Public Class PdfAxialShading
        Inherits PdfObject

        Private BBoxLeft As Double
        Private BBoxBottom As Double
        Private BBoxRight As Double
        Private BBoxTop As Double
        Private Mapping As MappingMode
        Private StartPointX As Double
        Private StartPointY As Double
        Private EndPointX As Double
        Private EndPointY As Double
        Private ExtendShadingBefore As Boolean = True
        Private ExtendShadingAfter As Boolean = True

        
        ''' <summary>
        ''' PDF axial shading constructor.
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <param name="BBoxLeft">Bounding box left position</param>
        ''' <param name="BBoxBottom">Bounding box bottom position</param>
        ''' <param name="BBoxWidth">Bounding box width</param>
        ''' <param name="BBoxHeight">Bounding box height</param>
        ''' <param name="ShadingFunction">Shading function</param>
        
        Public Sub New(Document As PdfDocument, BBoxLeft As Double, BBoxBottom As Double, BBoxWidth As Double, BBoxHeight As Double, ShadingFunction As PdfShadingFunction)
            MyBase.New(Document)
            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("S"c)

            ' color space red, green and blue
            Dictionary.Add("/ColorSpace", "/DeviceRGB")

            ' shading type axial
            Dictionary.Add("/ShadingType", "2")

            ' add shading function to shading dictionary
            Dictionary.AddIndirectReference("/Function", ShadingFunction)

            ' bounding box
            Me.BBoxLeft = BBoxLeft
            Me.BBoxBottom = BBoxBottom
            BBoxRight = BBoxLeft + BBoxWidth
            BBoxTop = BBoxBottom + BBoxHeight

            ' assume the direction of color change is along x axis
            Mapping = MappingMode.Relative
            StartPointX = 0.0
            StartPointY = 0.0
            EndPointX = 1.0
            EndPointY = 0.0
            Return
        End Sub

        ''' <summary>
        ''' PDF axial shading constructor for unit bounding box
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <param name="ShadingFunction">Shading function</param>
        Public Sub New(Document As PdfDocument, ShadingFunction As PdfShadingFunction)
            Me.New(Document, 0.0, 0.0, 1.0, 1.0, ShadingFunction)
        End Sub

    ''' <summary>
    ''' Set bounding box
    ''' </summary>
    ''' <param name="BBoxLeft">Bounding box left</param>
    ''' <param name="BBoxBottom">Bounding box bottom</param>
    ''' <param name="BBoxWidth">Bounding box width</param>
    ''' <param name="BBoxHeight">Bounding box height</param>
    Public Sub SetBoundingBox(BBoxLeft As Double, BBoxBottom As Double, BBoxWidth As Double, BBoxHeight As Double)
            ' bounding box
            Me.BBoxLeft = BBoxLeft
            Me.BBoxBottom = BBoxBottom
            BBoxRight = BBoxLeft + BBoxWidth
            BBoxTop = BBoxBottom + BBoxHeight
            Return
        End Sub

        ''' <summary>
        ''' Set gradient axis direction
        ''' </summary>
        ''' <param name="StartPointX">Start point x</param>
        ''' <param name="StartPointY">Start point y</param>
        ''' <param name="EndPointX">End point x</param>
        ''' <param name="EndPointY">End point y</param>
        ''' <param name="Mapping">Mapping mode (Relative or Absolute)</param>
        Public Sub SetAxisDirection(StartPointX As Double, StartPointY As Double, EndPointX As Double, EndPointY As Double, Mapping As MappingMode)
            Me.StartPointX = StartPointX
            Me.StartPointY = StartPointY
            Me.EndPointX = EndPointX
            Me.EndPointY = EndPointY
            Me.Mapping = Mapping
            Return
        End Sub

        
        ''' <summary>
        ''' Sets anti-alias parameter
        ''' </summary>
        ''' <param name="Value">Anti-alias true or false</param>
        
        Public Sub AntiAlias(Value As Boolean)
            Dictionary.AddBoolean("/AntiAlias", Value)
            Return
        End Sub

        
        ''' <summary>
        ''' Extend shading beyond axis
        ''' </summary>
        ''' <param name="Before">Before (true or false)</param>
        ''' <param name="After">After (true or false)</param>
        
        Public Sub ExtendShading(Before As Boolean, After As Boolean)
            ExtendShadingBefore = Before
            ExtendShadingAfter = After
            Return
        End Sub

        
        ' Write object to PDF file
        

        Friend Overrides Sub WriteObjectToPdfFile()
            ' bounding box
            Dictionary.AddRectangle("/BBox", BBoxLeft, BBoxBottom, BBoxRight, BBoxTop)

            ' absolute axis direction
            If Mapping = MappingMode.Absolute Then
                ' relative axit direction
                Dictionary.AddRectangle("/Coords", StartPointX, StartPointY, EndPointX, EndPointY)
            Else
                Dim RelStartPointX = BBoxLeft * (1.0 - StartPointX) + BBoxRight * StartPointX
                Dim RelStartPointY = BBoxBottom * (1.0 - StartPointY) + BBoxTop * StartPointY
                Dim RelEndPointX = BBoxLeft * (1.0 - EndPointX) + BBoxRight * EndPointX
                Dim RelEndPointY = BBoxBottom * (1.0 - EndPointY) + BBoxTop * EndPointY
                Dictionary.AddRectangle("/Coords", RelStartPointX, RelStartPointY, RelEndPointX, RelEndPointY)
            End If

            ' extend shading
            Dictionary.AddFormat("/Extend", "[{0} {1}]", If(ExtendShadingBefore, "true", "false"), If(ExtendShadingAfter, "true", "false"))

            ' call PdfObject base routine
            MyBase.WriteObjectToPdfFile()

            ' exit
            Return
        End Sub
    End Class
