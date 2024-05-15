#Region "Microsoft.VisualBasic::3ce208a97656be2be36b50d1c39458f6, mime\application%pdf\PdfFileWriter\Barcode\Barcode.vb"

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

    '   Total Lines: 107
    '    Code Lines: 28
    ' Comment Lines: 66
    '   Blank Lines: 13
    '     File Size: 3.59 KB


    ' Class Barcode
    ' 
    '     Properties: BarCount, CodeArray, Text, TotalWidth
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: BarWidth, GetBarcodeBox
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	Barcode
'	Single diminsion barcode class.
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
''

''' <summary>
''' One dimension barcode base class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#BarcodeSupport">2.5 Barcode Support</a>
''' </para>
''' </remarks>
Public Class Barcode

    ''' <summary>
    ''' Gets a copy of CodeArray
    ''' </summary>
    Public ReadOnly Property CodeArray As Integer()
        Get
            Return CType(_CodeArray.Clone(), Integer())
        End Get
    End Property

    Friend _CodeArray As Integer()

    ''' <summary>
    ''' Text string
    ''' </summary>
    Public Property Text As String

    ''' <summary>
    ''' Total number of black and white bars
    ''' </summary>
    Public Property BarCount As Integer

    ''' <summary>
    ''' Total barcode width in narrow bar units.
    ''' </summary>
    Public Property TotalWidth As Integer

    ''' <summary>
    ''' Protected barcode constructor
    ''' </summary>
    ''' <remarks>This class cannot be instantiated by itself.</remarks>
    Protected Sub New()
    End Sub

    ''' <summary>
    ''' Width of single bar code at indexed position expressed in narrow bar units.
    ''' </summary>
    ''' <param name="Index">Bar's index number.</param>
    ''' <returns>Bar's width in narrow bar units.</returns>
    ''' <remarks>This virtual function must be implemented by derived class 
    ''' Index range is 0 to BarCount - 1</remarks>
    Public Overridable Function BarWidth(Index As Integer) As Integer
        Throw New ApplicationException("Barcode.BarWidth: Not defined in derived class")
    End Function

    ''' <summary>
    ''' Calculate total barcode height including text
    ''' </summary>
    ''' <param name="BarWidth">Narrow bar width</param>
    ''' <param name="BarcodeHeight">Barcode height</param>
    ''' <param name="TextFont">Text font</param>
    ''' <param name="FontSize">Text font size</param>
    ''' <returns>BarcodeBox result</returns>
    Public Overridable Function GetBarcodeBox(BarWidth As Double, BarcodeHeight As Double, TextFont As PdfFont, FontSize As Double) As BarcodeBox
        ' no text
        If TextFont Is Nothing Then Return New BarcodeBox(BarWidth * TotalWidth, BarcodeHeight)

        ' calculate width
        Dim BarcodeWidth = BarWidth * TotalWidth
        Dim TextWidth = TextFont.TextWidth(FontSize, Text)
        Dim OriginX As Double = 0

        If TextWidth > BarcodeWidth Then
            OriginX = 0.5 * (TextWidth - BarcodeWidth)
            BarcodeWidth = TextWidth
        End If

        ' calculate height
        Dim TextHeight = TextFont.LineSpacing(FontSize)

        ' Barcode box
        Return New BarcodeBox(OriginX, TextHeight, BarcodeWidth, BarcodeHeight + TextHeight)
    End Function
End Class
