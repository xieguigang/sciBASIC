#Region "Microsoft.VisualBasic::87f3aeb44d7de6ec57b4a6a1cf1323a9, mime\application%pdf\PdfFileWriter\PDF\PdfPrintDocument.vb"

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

    '   Total Lines: 247
    '    Code Lines: 97 (39.27%)
    ' Comment Lines: 108 (43.72%)
    '    - Xml Docs: 54.63%
    ' 
    '   Blank Lines: 42 (17.00%)
    '     File Size: 9.10 KB


    '     Class PdfPrintDocument
    ' 
    '         Properties: DocumentInColor, GetMargins, GrayToBWCutoff, ImageQuality, PageCropRect
    '                     Resolution, SaveAs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: AddPagesToPdfDocument, SetMargins
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PrintPdfDocument
'	Create PDF document from PrintDocument page images.
'  Each page is saved as bitmap image.
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
Imports System.Drawing.Printing


    ''' <summary>
    ''' PDF print document class
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' It is a derived class of PrintDocument.
    ''' The class converts the metafile output of PrintDocument
    ''' to an image. The image is displayed in the PDF document.
    ''' </para>
    ''' <para>
    ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#PrintDocumentSupport">2.11 Print Document Support</a>
    ''' </para>
    ''' </remarks>
    Public Class PdfPrintDocument
        Inherits PrintDocument
        ''' <summary>
        ''' Document page crop rectangle
        ''' </summary>
        ''' <remarks>
        ''' Dimensions are in user units. The origin is top left corner.
        ''' </remarks>
        Public Property PageCropRect As RectangleF

        ''' <summary>
        ''' Image resolution in pixels per inch (default is 96)
        ''' </summary>
        Public Property Resolution As Double

        ''' <summary>
        ''' Save image as (default is jpeg)
        ''' </summary>
        Public Property SaveAs As SaveImageAs

        ''' <summary>
        ''' Gets or sets Jpeg image quality
        ''' </summary>
        Public Property ImageQuality As Integer
            Get
                Return _ImageQuality
            End Get
            Set(value As Integer)
                ' set image quality
                If value <> PdfImage.DefaultQuality AndAlso (value < 0 OrElse value > 100) Then Throw New ApplicationException("PdfImageControl.ImageQuality must be PdfImage.DefaultQuality or 0 to 100")
                _ImageQuality = value
                Return
            End Set
        End Property

        Friend _ImageQuality As Integer = PdfImage.DefaultQuality

        ''' <summary>
        ''' Gray to BW cutoff level
        ''' </summary>
        Public Property GrayToBWCutoff As Integer
            Get
                Return _GrayToBWCutoff
            End Get
            Set(value As Integer)
                If value < 1 OrElse value > 99 Then Throw New ApplicationException("PdfImageControl.GrayToBWCutoff must be 1 to 99")
                _GrayToBWCutoff = value
            End Set
        End Property

        Friend _GrayToBWCutoff As Integer = 50

        ''' <summary>
        ''' Current PDF document
        ''' </summary>
        Protected Document As PdfDocument

        
        ''' <summary>
        ''' PDF print document constructor
        ''' </summary>
        ''' <param name="Document">Current PDF document</param>
        
        Public Sub New(Document As PdfDocument)
            ' save document
            Me.Document = Document

            ' set default resolution to 96 pixels per inch
            Resolution = 96.0

            ' save as jpeg
            SaveAs = SaveImageAs.Jpeg

            ' create print document and preview controller objects
            PrintController = New PreviewPrintController()

            ' copy document's page size to default settings
            ' convert page size from points to 100th of inch
            ' do not set lanscape flag
            Dim PSize As PaperSize = New PaperSize()
            PSize.Width = CInt(Document.PageSize.Width / 0.72 + 0.5)
            PSize.Height = CInt(Document.PageSize.Height / 0.72 + 0.5)
            DefaultPageSettings.PaperSize = PSize

            ' assume document is in color
            DefaultPageSettings.Color = True
            Return
        End Sub

        
        ''' <summary>
        ''' Gets or sets DocumentInColor flag.
        ''' </summary>
        
        Public Property DocumentInColor As Boolean
            Set(value As Boolean)
                DefaultPageSettings.Color = value
                Return
            End Set
            Get
                Return DefaultPageSettings.Color
            End Get
        End Property

        
        ''' <summary>
        ''' Gets margins in 100th of an inch
        ''' </summary>
        
        Public ReadOnly Property GetMargins As Margins
            Get
                Return DefaultPageSettings.Margins
            End Get
        End Property

        
        ''' <summary>
        ''' Sets margins in user units.
        ''' </summary>
        ''' <param name="LeftMargin">Left margin</param>
        ''' <param name="TopMargin">Top margin</param>
        ''' <param name="RightMargin">Right margin</param>
        ''' <param name="BottomMargin">Bottom margin</param>
        
        Public Sub SetMargins(LeftMargin As Double, TopMargin As Double, RightMargin As Double, BottomMargin As Double)
            Dim Margins = DefaultPageSettings.Margins
            Margins.Left = CInt(LeftMargin * Document.ScaleFactor / 0.72 + 0.5)
            Margins.Top = CInt(TopMargin * Document.ScaleFactor / 0.72 + 0.5)
            Margins.Right = CInt(RightMargin * Document.ScaleFactor / 0.72 + 0.5)
            Margins.Bottom = CInt(BottomMargin * Document.ScaleFactor / 0.72 + 0.5)
            Return
        End Sub

        
        ''' <summary>
        ''' Add pages to PDF document
        ''' </summary>
        ''' <remarks>
        ''' The PrintDoc.Print method will call BeginPrint method,
        ''' next it will call multiple times PrintPage method and finally
        ''' it will call EndPrint method. 
        ''' </remarks>
        
        Public Sub AddPagesToPdfDocument()
            ' print the document by calling BeginPrint, PrintPage multiple times and finally EndPrint
            Print()

            ' get printing results in the form of array of images one per page
            ' image format is Metafile
            Dim PageInfo As PreviewPageInfo() = CType(PrintController, PreviewPrintController).GetPreviewPageInfo()

            ' page size in user units
            Dim PageWidth = Document.PageSize.Width / Document.ScaleFactor
            Dim PageHeight = Document.PageSize.Height / Document.ScaleFactor

            ' add pages to pdf document
            For ImageIndex = 0 To PageInfo.Length - 1
                ' add page to document
                Dim Page As PdfPage = New PdfPage(Document)

                ' add contents to the page
                Dim Contents As PdfContents = New PdfContents(Page)

                ' page image
                Dim PageImage = PageInfo(ImageIndex).Image

                ' empty pdf image
                Dim PdfImage As PdfImage = New PdfImage(Contents.Document)
                PdfImage.Resolution = Resolution
                PdfImage.SaveAs = SaveAs
                PdfImage.ImageQuality = ImageQuality
                PdfImage.GrayToBWCutoff = GrayToBWCutoff
                PdfImage.CropRect = Rectangle.Empty
                PdfImage.CropPercent = RectangleF.Empty

                ' no crop
                If PageCropRect.IsEmpty Then
                    ' convert metafile image to PdfImage
                    PdfImage.LoadImage(PageImage)

                    ' draw the image

                    ' crop
                    Contents.DrawImage(PdfImage, 0.0, 0.0, PageWidth, PageHeight)
                Else
                    Dim ImageWidth = PageImage.Width
                    Dim ImageHeight = PageImage.Height
                    PdfImage.CropRect.X = CInt(ImageWidth * PageCropRect.X / PageWidth + 0.5)
                    PdfImage.CropRect.Y = CInt(ImageHeight * PageCropRect.Y / PageHeight + 0.5)
                    PdfImage.CropRect.Width = CInt(ImageWidth * PageCropRect.Width / PageWidth + 0.5)
                    PdfImage.CropRect.Height = CInt(ImageHeight * PageCropRect.Height / PageHeight + 0.5)

                    ' convert metafile image to PdfImage
                    PdfImage.LoadImage(PageImage)

                    ' draw the image
                    Contents.DrawImage(PdfImage, PageCropRect.X, PageHeight - PageCropRect.Y - PageCropRect.Height, PageCropRect.Width, PageCropRect.Height)
                End If
            Next

            Return
        End Sub
    End Class
