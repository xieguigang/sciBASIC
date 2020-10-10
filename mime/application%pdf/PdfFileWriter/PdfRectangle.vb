''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfRectangle
'	PDF rectangle class. 
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



    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' <summary>
    ''' PDF rectangle in double precision class
    ''' </summary>
    ''' <remarks>
    ''' Note: Microsoft rectangle is left, top, width and height.
    ''' PDF rectangle is left, bottom, right and top.
    ''' PDF numeric precision is double and Microsoft is Single.
    ''' </remarks>
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Class PdfRectangle
        ''' <summary>
        ''' Gets or sets Left side.
        ''' </summary>
        Public Property Left As Double

        ''' <summary>
        ''' Gets or sets bottom side.
        ''' </summary>
        Public Property Bottom As Double

        ''' <summary>
        ''' Gets or sets right side.
        ''' </summary>
        Public Property Right As Double

        ''' <summary>
        ''' Gets or sets top side.
        ''' </summary>
        Public Property Top As Double

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="Left">Left side</param>
        ''' <param name="Bottom">Bottom side</param>
        ''' <param name="Right">Right side</param>
        ''' <param name="Top">Top side</param>
        Public Sub New(ByVal Left As Double, ByVal Bottom As Double, ByVal Right As Double, ByVal Top As Double)
            Me.Left = Left
            Me.Bottom = Bottom
            Me.Right = Right
            Me.Top = Top
            Return
        End Sub

        ''' <summary>
        ''' Copy constructor
        ''' </summary>
        ''' <param name="Rect">Source rectangle</param>
        Public Sub New(ByVal Rect As PdfRectangle)
            Left = Rect.Left
            Bottom = Rect.Bottom
            Right = Rect.Right
            Top = Rect.Top
            Return
        End Sub

        ''' <summary>
        ''' Constructor for margin
        ''' </summary>
        ''' <param name="AllTheSame">Single value for all sides</param>
        Public Sub New(ByVal AllTheSame As Double)
            Left = AllTheSame
            Bottom = AllTheSame
            Right = AllTheSame
            Top = AllTheSame
            Return
        End Sub

        ''' <summary>
        ''' Constructor for margin
        ''' </summary>
        ''' <param name="Hor">Left and right value</param>
        ''' <param name="Vert">Top and bottom value</param>
        Public Sub New(ByVal Hor As Double, ByVal Vert As Double)
            Left = Hor
            Bottom = Vert
            Right = Hor
            Top = Vert
            Return
        End Sub

        ''' <summary>
        ''' Gets width
        ''' </summary>
        Public ReadOnly Property Width As Double
            Get
                Return Right - Left
            End Get
        End Property

        ''' <summary>
        ''' Gets height
        ''' </summary>
        Public ReadOnly Property Height As Double
            Get
                Return Top - Bottom
            End Get
        End Property
    End Class

