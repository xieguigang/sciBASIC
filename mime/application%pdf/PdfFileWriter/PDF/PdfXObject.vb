#Region "Microsoft.VisualBasic::7b9eceda682a05a3da920e9658c30d4b, mime\application%pdf\PdfFileWriter\PDF\PdfXObject.vb"

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

    '   Total Lines: 131
    '    Code Lines: 61
    ' Comment Lines: 57
    '   Blank Lines: 13
    '     File Size: 3.90 KB


    '     Class PdfXObject
    ' 
    '         Properties: Bottom, Left, Rect, Right, Top
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: LayerControl
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfXObject
'	PDF X Object resource class.
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
    ''' PDF X object resource class
    ''' </summary>
    Public Class PdfXObject
        Inherits PdfContents
        ''' <summary>
        ''' Bounding box rectangle
        ''' </summary>
        Public Property Rect As PdfRectangle
            Get
                Return New PdfRectangle(BBox)
            End Get
            Set(value As PdfRectangle)
                BBox = value
                Dictionary.AddRectangle("/BBox", BBox)
            End Set
        End Property

        ''' <summary>
        ''' Bounding box left side
        ''' </summary>
        Public Property Left As Double
            Get
                Return BBox.Left
            End Get
            Set(value As Double)
                BBox.Left = value
                Dictionary.AddRectangle("/BBox", BBox)
            End Set
        End Property

        ''' <summary>
        ''' Bounding box bottom side
        ''' </summary>
        Public Property Bottom As Double
            Get
                Return BBox.Bottom
            End Get
            Set(value As Double)
                BBox.Bottom = value
                Dictionary.AddRectangle("/BBox", BBox)
            End Set
        End Property

        ''' <summary>
        ''' Bounding box right side
        ''' </summary>
        Public Property Right As Double
            Get
                Return BBox.Right
            End Get
            Set(value As Double)
                BBox.Right = value
                Dictionary.AddRectangle("/BBox", BBox)
            End Set
        End Property

        ''' <summary>
        ''' Bounding box top side
        ''' </summary>
        Public Property Top As Double
            Get
                Return BBox.Top
            End Get
            Set(value As Double)
                BBox.Top = value
                Dictionary.AddRectangle("/BBox", BBox)
            End Set
        End Property

        ' bounding rectangle
        Friend BBox As PdfRectangle

        ''' <summary>
        ''' PDF X Object constructor
        ''' </summary>
        ''' <param name="Document">PDF document</param>
        ''' <param name="Width">X Object width</param>
        ''' <param name="Height">X Object height</param>
        Public Sub New(Document As PdfDocument, Optional Width As Double = 1.0, Optional Height As Double = 1.0)
            MyBase.New(Document, "/XObject")
            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("X"c)

            ' add subtype to dictionary
            Dictionary.Add("/Subtype", "/Form")

            ' set boundig box rectangle
            BBox = New PdfRectangle(0.0, 0.0, Width, Height)

            ' bounding box
            Dictionary.AddRectangle("/BBox", BBox)
            Return
        End Sub

        ''' <summary>
        ''' Layer control
        ''' </summary>
        ''' <param name="Layer">PdfLayer object</param>
        Public Sub LayerControl(Layer As PdfObject)
            Dictionary.AddIndirectReference("/OC", Layer)
            Return
        End Sub
    End Class
