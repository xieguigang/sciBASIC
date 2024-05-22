#Region "Microsoft.VisualBasic::940caf473459a49e8bbb72906e8a90cd, mime\application%pdf\PdfFileWriter\PDF\PdfRectangle.vb"

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

    '   Total Lines: 134
    '    Code Lines: 46 (34.33%)
    ' Comment Lines: 73 (54.48%)
    '    - Xml Docs: 67.12%
    ' 
    '   Blank Lines: 15 (11.19%)
    '     File Size: 3.79 KB


    '     Class PdfRectangle
    ' 
    '         Properties: Bottom, Height, Left, Right, Top
    '                     Width
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

'
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
'



    
    ''' <summary>
    ''' PDF rectangle in double precision class
    ''' </summary>
    ''' <remarks>
    ''' Note: Microsoft rectangle is left, top, width and height.
    ''' PDF rectangle is left, bottom, right and top.
    ''' PDF numeric precision is double and Microsoft is Single.
    ''' </remarks>
    
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
        Public Sub New(Left As Double, Bottom As Double, Right As Double, Top As Double)
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
        Public Sub New(Rect As PdfRectangle)
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
        Public Sub New(AllTheSame As Double)
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
        Public Sub New(Hor As Double, Vert As Double)
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
