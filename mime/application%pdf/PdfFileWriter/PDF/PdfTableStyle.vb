#Region "Microsoft.VisualBasic::76f5245e484f082df09c49df5b589b1d, mime\application%pdf\PdfFileWriter\PDF\PdfTableStyle.vb"

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

    '   Total Lines: 304
    '    Code Lines: 86
    ' Comment Lines: 187
    '   Blank Lines: 31
    '     File Size: 10.80 KB


    '     Class PdfTableStyle
    ' 
    '         Properties: Alignment, BackgroundColor, BarcodeBarWidth, BarcodeHeight, Font
    '                     FontAscent, FontDescent, FontLineSpacing, FontSize, ForegroundColor
    '                     Format, Margin, MinHeight, MultiLineText, NumberFormatInfo
    '                     RaiseCustomDrawCellEvent, TextBoxFirstLineIndent, TextBoxLineBreakFactor, TextBoxLineExtraSpace, TextBoxPageBreakLines
    '                     TextBoxParagraphExtraSpace, TextBoxTextJustify, TextDrawStyle
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: Copy
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfTableStyle
'	Data table style support.
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

Imports System
Imports System.Drawing
Imports System.Globalization


    ''' <summary>
    ''' PDF table cell or header style class
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DataTableSupport">2.12 Data Table Support</a>
    ''' </para>
    ''' </remarks>
    Public Class PdfTableStyle
        ''' <summary>
        ''' Gets or sets content alignment.
        ''' </summary>
        ''' <remarks>
        ''' Alignment property align the content within the client area of the cell.
        ''' </remarks>
        Public Property Alignment As ContentAlignment

        ''' <summary>
        ''' Gets or sets background color.
        ''' </summary>
        ''' <remarks>
        ''' If background color is not empty, the frame area of the cell will 
        ''' be painted by this color. Default is Color.Empty.
        ''' </remarks>
        Public Property BackgroundColor As Color

        ''' <summary>
        ''' Gets or sets barcode narrow bar width
        ''' </summary>
        ''' <remarks>
        ''' The width of the bar code narrow bar.
        ''' </remarks>
        Public Property BarcodeBarWidth As Double

        ''' <summary>
        ''' Gets or sets barcode height
        ''' </summary>
        ''' <remarks>
        ''' The height of the barcode excluding optional text.
        ''' </remarks>
        Public Property BarcodeHeight As Double

        ''' <summary>
        ''' Gets or sets first line indent for text box items.
        ''' </summary>
        Public Property TextBoxFirstLineIndent As Double

        ''' <summary>
        ''' Gets or sets text box line break factor.
        ''' </summary>
        Public Property TextBoxLineBreakFactor As Double

        ''' <summary>
        ''' Gets or sets extra line spacing for text box items.
        ''' </summary>
        Public Property TextBoxLineExtraSpace As Double

        ''' <summary>
        ''' Gets or sets minimum text lines for page break calculations.
        ''' </summary>
        ''' <remarks>
        ''' If TextBoxPageBreakLines is zero, the software will keep
        ''' all of the TextBox together. If the TextBox height is too
        ''' big to fit in the table, an exception will be raised. If
        ''' TextBoxPageBreakLines is not zero and TextBox height is too
        ''' big, the height of TextBoxPageBreakLines will be used
        ''' to start a new page. The remaining lines will be printed
        ''' on the next page or pages.
        ''' </remarks>
        Public Property TextBoxPageBreakLines As Integer

        ''' <summary>
        ''' Gets or sets extra paragraph spacing for text box items.
        ''' </summary>
        Public Property TextBoxParagraphExtraSpace As Double

        ''' <summary>
        ''' Gets or sets text justify within text box.
        ''' </summary>
        Public Property TextBoxTextJustify As TextBoxJustify

        ''' <summary>
        ''' Gets or sets font.
        ''' </summary>
        ''' <remarks>
        ''' If cell's value type is barcode, a null font signal no text under the barcode.
        ''' </remarks>
        Public Property Font As PdfFont

        ''' <summary>
        ''' Gets or sets font size.
        ''' </summary>
        Public Property FontSize As Double

        ''' <summary>
        ''' Gets or sets foreground color.
        ''' </summary>
        ''' <remarks>
        ''' Foreground color is used for text and Barcode.
        ''' </remarks>
        Public Property ForegroundColor As Color

        ''' <summary>
        ''' Gets or sets format string.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' All basic numeric values are converted to string using: 
        ''' </para>
        ''' <code>
        ''' Value.ToString(Format, NumberFormatInfo);
        ''' </code>
        ''' <para>
        ''' The NumberFormatInfo allows for regional formatting.
        ''' </para>
        ''' <para>
        ''' Both Format and NumberFormatInfo are set to null by default.
        ''' In other words by default the conversion is:
        ''' </para>
        ''' <code>
        ''' Value.ToString();
        ''' </code>
        ''' </remarks>
        Public Property Format As String

        ''' <summary>
        ''' Gets or sets cell's margins.
        ''' </summary>
        Public Property Margin As PdfRectangle

        ''' <summary>
        ''' Gets or sets raise custom draw cell event flag.
        ''' </summary>
        ''' <remarks>
        ''' With this flag you can control which columns call the draw cell event handler.
        ''' </remarks>
        Public Property RaiseCustomDrawCellEvent As Boolean

        ''' <summary>
        ''' Gets or sets text draw style.
        ''' </summary>
        Public Property TextDrawStyle As DrawStyle

        ''' <summary>
        ''' Gets or sets minimum cell height.
        ''' </summary>
        Public Property MinHeight As Double

        ''' <summary>
        ''' Gets or sets multi-line text flag.
        ''' </summary>
        ''' <remarks>
        ''' String value will be converted to text box value.
        ''' </remarks>
        Public Property MultiLineText As Boolean

        ''' <summary>
        ''' Gets or sets number format information.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' All basic numeric values are converted to string using: 
        ''' </para>
        ''' <code>
        ''' Value.ToString(Format, NumberFormatInfo);
        ''' </code>
        ''' <para>
        ''' The NumberFormatInfo allows for regional formatting.
        ''' </para>
        ''' <para>
        ''' Both Format and NumberFormatInfo are set to null by default.
        ''' In other words by default the conversion is:
        ''' </para>
        ''' <code>
        ''' Value.ToString();
        ''' </code>
        ''' </remarks>
        Public Property NumberFormatInfo As NumberFormatInfo

        ''' <summary>
        ''' Gets font ascent for current font and font size.
        ''' </summary>
        Public ReadOnly Property FontAscent As Double
            Get
                If Font Is Nothing Then Throw New ApplicationException("PdfTableStyle: Font is not defined.")
                Return Font.AscentPlusLeading(FontSize)
            End Get
        End Property

        ''' <summary>
        ''' Gets font descent for current font and font size.
        ''' </summary>
        Public ReadOnly Property FontDescent As Double
            Get
                If Font Is Nothing Then Throw New ApplicationException("PdfTableStyle: Font is not defined.")
                Return Font.DescentPlusLeading(FontSize)
            End Get
        End Property

        ''' <summary>
        ''' Gets font line spacing for current font and font size.
        ''' </summary>
        Public ReadOnly Property FontLineSpacing As Double
            Get
                If Font Is Nothing Then Throw New ApplicationException("PdfTableStyle: Font is not defined.")
                Return Font.LineSpacing(FontSize)
            End Get
        End Property

        ''' <summary>
        ''' PDF table style default constructor.
        ''' </summary>
        ''' <param name="Font">Font</param>
        Public Sub New(Optional Font As PdfFont = Nothing)
            Alignment = ContentAlignment.TopLeft
            BackgroundColor = Color.Empty
            TextBoxLineBreakFactor = 0.5
            TextBoxTextJustify = TextBoxJustify.Left
            Me.Font = Font
            FontSize = 9.0
            Margin = New PdfRectangle()
            ForegroundColor = Color.Black
            TextDrawStyle = DrawStyle.Normal
            Return
        End Sub

        ''' <summary>
        ''' PDF table style constructor based on table's default cell style.
        ''' </summary>
        ''' <param name="Table">Table</param>
        Public Sub New(Table As PdfTable)
            Copy(Table.DefaultCellStyle)
            Return
        End Sub

        ''' <summary>
        ''' PDF table style constructor as a copy of another style.
        ''' </summary>
        ''' <param name="Other">Copy constructor.</param>
        Public Sub New(Other As PdfTableStyle)
            Copy(Other)
            Return
        End Sub

        
        ''' <summary>
        ''' Copy one style to another 
        ''' </summary>
        ''' <param name="SourceStyle">Source style</param>
        
        Public Sub Copy(SourceStyle As PdfTableStyle)
            Alignment = SourceStyle.Alignment
            BackgroundColor = SourceStyle.BackgroundColor
            BarcodeBarWidth = SourceStyle.BarcodeBarWidth
            BarcodeHeight = SourceStyle.BarcodeHeight
            Font = SourceStyle.Font
            FontSize = SourceStyle.FontSize
            ForegroundColor = SourceStyle.ForegroundColor
            Format = SourceStyle.Format
            Margin = New PdfRectangle(SourceStyle.Margin)
            MinHeight = SourceStyle.MinHeight
            MultiLineText = SourceStyle.MultiLineText
            NumberFormatInfo = SourceStyle.NumberFormatInfo
            RaiseCustomDrawCellEvent = SourceStyle.RaiseCustomDrawCellEvent
            TextBoxFirstLineIndent = SourceStyle.TextBoxFirstLineIndent
            TextBoxLineBreakFactor = SourceStyle.TextBoxLineBreakFactor
            TextBoxLineExtraSpace = SourceStyle.TextBoxLineExtraSpace
            TextBoxPageBreakLines = SourceStyle.TextBoxPageBreakLines
            TextBoxParagraphExtraSpace = SourceStyle.TextBoxParagraphExtraSpace
            TextBoxTextJustify = SourceStyle.TextBoxTextJustify
            TextDrawStyle = SourceStyle.TextDrawStyle
            Return
        End Sub
    End Class
