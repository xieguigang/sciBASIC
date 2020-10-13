''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
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
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



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
            Set(ByVal value As PdfRectangle)
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
            Set(ByVal value As Double)
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
            Set(ByVal value As Double)
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
            Set(ByVal value As Double)
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
            Set(ByVal value As Double)
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
        Public Sub New(ByVal Document As PdfDocument, ByVal Optional Width As Double = 1.0, ByVal Optional Height As Double = 1.0)
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
        Public Sub LayerControl(ByVal Layer As PdfObject)
            Dictionary.AddIndirectReference("/OC", Layer)
            Return
        End Sub
    End Class

