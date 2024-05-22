#Region "Microsoft.VisualBasic::768cbe67ab07f9570bd16b2c77bf6be8, mime\application%pdf\PdfFileWriter\PDF\PdfDocument.vb"

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

    '   Total Lines: 1147
    '    Code Lines: 339 (29.56%)
    ' Comment Lines: 625 (54.49%)
    '    - Xml Docs: 56.16%
    ' 
    '   Blank Lines: 183 (15.95%)
    '     File Size: 41.72 KB


    ' Enum UnitOfMeasure
    ' 
    '     cm, Inch, m, mm, Point
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum PaperType
    ' 
    '     Legal, Letter
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum InitialDocDisplay
    ' 
    '     FullScreen, UseAttachments, UseBookmarks, UseLayers, UseNone
    '     UseThumbs
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module NFI
    ' 
    '     Properties: PeriodDecSep
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class PdfDocument
    ' 
    '     Properties: Epsilon, InitialDocDisplay, PageCount, ScaleFactor
    ' 
    '     Constructor: (+8 Overloads) Sub New
    ' 
    '     Function: ByteArrayToPdfHexString, ByteArrayToPdfString, GenerateResourceNumber, GetBookmarksRoot, GetPage
    '               RandomByteArray, TextToByteArray, TextToPdfString
    ' 
    '     Sub: AddDestToLinkAnnot, ConstructorHelper, CreateFile, CreateNamedDestinations, Dispose
    '          MovePage, (+4 Overloads) SetEncryption
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfDocument
'	The main class of PDF object.
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
'	Version History:
'
'	Version 1.0 2013/04/01
'		Original revision
'	Version 1.1 2013/04/09
'		Allow program to be compiled in regions that define
'		decimal separator to be non period (comma)
'	Version 1.2 2013/07/21
'		The original revision supported image resources with
'		jpeg file format only.
'		Version 1.2 support all image files acceptable to Bitmap class.
'		See ImageFormat class. The program was tested with:
'		Bmp, Gif, Icon, Jpeg, Png and Tiff.
'	Version 1.3 2014/02/07
'		Fix bug in PdfContents.DrawBezierNoP2(PointD P1, PointD P3)
'	Version 1.4 2014/03/01
'		PdfContents class
'		Add method: public void TranslateScaleRotate(double OrigX,
'			double OrigY, double ScaleX, double ScaleY, double Rotate);
'		Add method: public string ReverseString(Strint Text);
'		Fix some problems with DrawXObject(...); methods
'		PdfFont
'		Extensive changes to font substitution (see article)
'		PdfImage
'		Add method: public SizeD ImageSizeAndDensity(double Width,
'			double Height, double Density);
'		This method controls the size of the bitmap (see article)
'		Add method: public void SetImageQuality(int ImageQuality);
'		This method controls the image quality (see article)
'		PdfTilingPattern
'		Fix bug in public static PdfTilingPattern SetWeavePattern(...);
'	Version 1.5 2014/05/05
'		Barcode feature. Supported barcodes are:
'		Code-128, Code39, UPC-A, EAN-13
'	Version 1.6 2014/07/09
'		Fix FontApi unanaged code resource disposition.
'		Clear PdfDocument object after CreateFile.
'	Version 1.7 2014/08/25
'		Encryption support
'		Web link support
'		QRCode support
'		Change compression to .net System.io.compression
'	Version 1.8 2014/09/12
'		Bookmark (document outline) support
'	Version 1.9 2014/10/06
'		New features
'		Support for Microsoft Charting
'		Support for Metafile images
'		Support for image cropping
'		Support for PrintDocument output to PDF file
'		Fixs
'		Font loading. Fix the problem of missing table.
'	Version 1.9.1 2014/10/12
'		Fix decimal separator problem in regions that define
'		decimal separator to be non period (comma) in the
'		ChartExample.cs code
'	Version 1.10.0 2014/12/02
'		Support for data tables. Add source code documentation.
'		Increase maximum number of images per document.
'	Version 1.11.0 2015/01/19
'		Support for video, sound and attached files.
'	Version 1.12.0 2015/04/13
'		Page order control.
'		Rewrite of table borders and grid lines.
'	Version 1.13.0 2015/05/08
'		The resulted Pdf document can be saved to a file or to a stream.
'		Encryption support for Standard 128 mode.
'		DrawRow method of PdfTable can force a new page.
'		Image quality control.
'	Version 1.14.0 2015/06/08
'		PdfTable will split large text columns into separate pages.
'	Version 1.14.1 2015/06/09
'		PdfTableStyle fix Copy method.
'	Version 1.15.0 2015/06/17
'		PDF document information dictionary (Title, Author, Subject,
'		Keywords, Creator, Producer, Creation date, Modify date).
'		PdfImage class add support for indexed bitmap, gray image,
'		black and white image.
'	Version 1.15.1 2015/06/18
'		Remove unused source from solution explorer
'	Version 1.16.0 2015/07/27
'		Unicode support. Commit page method.
'	Version 1.16.1 2015/08/06
'		Fix problem of converting small real numbers (<0.0001) to string.
'	Version 1.16.2 2015/09/01
'		Fix problem related to undefined character.
'	Version 1.16.3 2015/09/22
'		PdfTable constructor uses current page size to calculate
'		the table area rectangle. When PdfTable creates a new page,
'		it copies page size from previous page.
'	Version 1.16.4 2015/09/30
'		Consistent use of IDisposable interface to release
'		unmanaged resources.
'	Version 1.17.0 2016/01/26
'		New features added to the library:
'		WPF graphics path drawing.
'		Elliptical arcs drawing.
'		Support for color alpha component. In other words, support for transparency or opacity.
'		Support for color bland.
'		Support for quadratic Bezier curves.
'	Version 1.17.1 2016/02/29
'		Fix PdfTable. Header column 0 is TextBox.
'	Version 1.17.2 2016/03/22
'		Fix PdfInfo. Document properties.
'	Version 1.17.3 2016/04/14
'		Fix problem with non integer font size in regions that define
'		decimal separator to be non period (comma)
'	Version 1.18.0 2016/05/24
'		Named destinations and creation of PdfFont resource.
'	Version 1.18.1 2016/06/02 (same as 1.17.3)
'		Fix problem with non integer font size in regions that define
'		decimal separator to be non period (comma)
'	Version 1.19.0 2016/06/13
'		Document links and enhanced multi media and file attachment.
'	Version 1.19.1 2016/07/27
'		Fix location marker problem.
'	Version 1.19.2 2017/08/30
'		Remove debug working directory from project settings
'	Version 1.19.3 2018/06/24
'		Fix PdfFontFile.BuildLocaTable method. Long format buffer
'		pointer initialization.
'		Fix PdfTableCell add value type of DBNull.
'	Version 1.20.0 2018/07/15
'		Modify PdfQRCode class to include optional ModuleSize argument.
'	Version 1.21.0 2019/02/06
'		Add support for PDF417 barcode.
'	Version 1.21.1 2019/02/13
'		Fix PDF417 barcode quiet zone.
'	Version 1.22.0 2019/02/18
'		Add support for sticky notes.
'	Version 1.23.0 2019/05/26
'		Add support for sticky notes.
'	Version 1.24.0 2019/06/06
'		Support for layers control of images and annotations.
'	Version 1.24.1 2019/06/20
'		Add meter as unit of measue and fix version number and data strings.
'	Version 1.25.0 2019/07/15
'		Support for collection of fonts. Support for non ascii font names
'
'

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports i32 = Microsoft.VisualBasic.Language.i32

''' <summary>
''' Unit of measure enumeration
''' </summary>
''' <remarks>
''' User unit of measure enumeration.
''' </remarks>
Public Enum UnitOfMeasure
    ''' <summary>
    ''' Point
    ''' </summary>
    Point

    ''' <summary>
    ''' Inch
    ''' </summary>
    Inch

    ''' <summary>
    ''' CM
    ''' </summary>
    cm

    ''' <summary>
    ''' MM
    ''' </summary>
    mm

    ''' <summary>
    ''' Meter
    ''' </summary>
    m
End Enum

''' <summary>
''' Standard paper size enumeration 
''' </summary>
Public Enum PaperType
    ''' <summary>
    ''' Letter
    ''' </summary>
    Letter

    ''' <summary>
    ''' Legal
    ''' </summary>
    Legal

    ''' <summary>
    ''' A3
    ''' </summary>
    A3          ' 297mm 420mm

    ''' <summary>
    ''' A4
    ''' </summary>
    A4          ' 210mm 297mm

    ''' <summary>
    ''' A5
    ''' </summary>
    A5          ' 148mm 210mm
End Enum

''' <summary>
''' Initial document display enumeration
''' </summary>
Public Enum InitialDocDisplay
    ''' <summary>
    ''' Take no action
    ''' </summary>
    UseNone

    ''' <summary>
    ''' Display bookmarks panel
    ''' </summary>
    UseBookmarks

    ''' <summary>
    ''' Display thumbnail panel
    ''' </summary>
    UseThumbs

    ''' <summary>
    ''' Full screen
    ''' </summary>
    FullScreen

    ''' <summary>
    ''' Display layers panel
    ''' </summary>
    UseLayers

    ''' <summary>
    ''' Display attachment panel
    ''' </summary>
    UseAttachments
End Enum

''' <summary>
''' Number Format Information static class
''' </summary>
''' <remarks>
''' Adobe readers expect decimal separator to be a period.
''' Some countries define decimal separator as a comma.
''' The project uses NFI.DecSep to force period for all regions.
''' </remarks>
Public Module NFI
    ''' <summary>
    ''' Define period as number decimal separator.
    ''' </summary>
    ''' <remarks>
    ''' NumberFormatInfo is used with string formatting to set the
    ''' decimal separator to a period regardless of region.
    ''' </remarks>
    Private _PeriodDecSep As System.Globalization.NumberFormatInfo

    Public Property PeriodDecSep As NumberFormatInfo
        Get
            Return _PeriodDecSep
        End Get
        Private Set(value As NumberFormatInfo)
            _PeriodDecSep = value
        End Set
    End Property

    ' static constructor
    Sub New()
        ' number format (decimal separator is period)
        PeriodDecSep = New NumberFormatInfo()
        PeriodDecSep.NumberDecimalSeparator = "."
        Return
    End Sub
End Module

''' <summary>
''' PDF document class
''' </summary>
''' <remarks>
''' <para>
''' The main class for controlling the production of the PDF document.
''' </para>
''' <para>
''' Creating a PDF is a six steps process.
''' </para>
''' <para>
''' Step 1: Create one document object this PdfDocument class.
''' </para>
''' <para>
''' Step 2: Create resource objects such as fonts or images (i.e. PdfFont or PdfImage).
''' </para>
''' <para>
''' Step 3: Create page object PdfPage.
''' </para>
''' <para>
''' Step 4: Create contents object PdfContents.
''' </para>
''' <para>
''' Step 5: Add text and graphics to the contents object (using PdfContents methods).
''' </para>
''' <para>
''' Repeat steps 3, 4 and 5 for additional pages
''' </para>
''' <para>
''' Step 6: Create your PDF document file by calling CreateFile method of PdfDocument.
''' </para>
''' <para>
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DocumentCreation">For example of document creation see 3.1. Document Creation Overview</a>
''' </para>
''' </remarks>
Public Class PdfDocument
    Implements IDisposable



    ''' <summary>
    ''' Epsilon 1/300 of an inch in user units
    ''' </summary>
    ''' <remarks>
    ''' Small distance for compare calculation to eliminate rounding errors.
    ''' </remarks>
    Private _ScaleFactor As Double, _Epsilon As Double
    ''' <summary>
    ''' Library revision number
    ''' </summary>
    Public Shared ReadOnly RevisionNumber As String = "1.25.0"

    ''' <summary>
    ''' Library revision date
    ''' </summary>
    Public Shared ReadOnly RevisionDate As String = "2019/07/15"

    ''' <summary>
    ''' Scale factor
    ''' </summary>
    ''' <remarks>
    ''' From user unit of measure to points.
    ''' </remarks>
    Public Property ScaleFactor As Double
        Get
            Return _ScaleFactor
        End Get
        Friend Set(value As Double)
            _ScaleFactor = value
        End Set
    End Property

    Public Property Epsilon As Double
        Get
            Return _Epsilon
        End Get
        Friend Set(value As Double)
            _Epsilon = value
        End Set
    End Property

    ''' <summary>
    ''' Initial document display
    ''' </summary>
    Public Property InitialDocDisplay As InitialDocDisplay

    ''' <summary>
    ''' Page count
    ''' </summary>
    ''' <remarks>
    ''' Current page count
    ''' </remarks>
    Public ReadOnly Property PageCount As Integer
        Get
            Return PageArray.Count
        End Get
    End Property

    ''' <summary>
    ''' Get page object
    ''' </summary>
    ''' <param name="Index">Page index (zero based)</param>
    ''' <returns>PdfPage object</returns>
    Public Function GetPage(Index As Integer) As PdfPage
        If Index < 0 OrElse Index >= PageArray.Count Then Throw New ApplicationException("GetPage invalid argument")
        Return PageArray(Index)
    End Function

    Friend FileName As String           ' PDF document file name
    Friend PdfFile As PdfBinaryWriter           ' PDF document file stream
    Friend PageSize As SizeD            ' in points
    Friend ObjectArray As List(Of PdfObject) = New List(Of PdfObject)() ' list of all PDF indirect objects for this document
    Friend PageArray As List(Of PdfPage) = New List(Of PdfPage)()
    Friend Layers As PdfLayers              ' Layers control
    Friend CatalogObject As PdfObject       ' catalog object
    Friend PagesObject As PdfObject     ' parent object of all pages
    Friend TrailerDict As PdfDictionary     ' trailer dictionary
    Friend Encryption As PdfEncryption          ' encryption dictionary
    Friend BookmarksRoot As PdfBookmark     ' bookmarks (document outline) dictionary
    Friend ResCodeNo As Integer() = New Integer(5) {} ' resource code next number
    Friend InfoObject As PdfInfo
    Friend DocumentID As Byte()         ' document ID
    Friend EmbeddedFileArray As List(Of PdfEmbeddedFile)
    Friend ExtGStateArray As List(Of PdfExtGState)
    Friend FontArray As List(Of PdfFont)
    Friend LinkAnnotArray As List(Of PdfAnnotation)
    Friend LocMarkerArray As List(Of LocationMarker)
    Friend WebLinkArray As List(Of PdfWebLink)
    Friend InitDocDispText As String() = New String() {"/UseNone", "/UseOutlines", "/UseThumbs", "/FullScreen", "/UseOC", "/UseAttachments"}

    ''' <summary>
    ''' Debug flag
    ''' </summary>
    ''' <remarks>
    ''' Debug flag. Default is false. The program will generate normal PDF file.
    ''' If debug flag is true, the library will not compress contents, will replace images and font file with text place holder.
    ''' The generated file can be viewed with a text editor but cannot be loaded into PDF reader.
    ''' </remarks>
    Public Debug As Boolean = False

    ' translation of user units to points
    ' must agree with UnitOfMeasure enumeration
    Friend Shared UnitInPoints As Double() = New Double() {1.0, 72.0, 72.0 / 2.54, 72.0 / 25.4, 72.0 / 0.0254}          ' Point
    ' Inch
    ' cm
    ' mm
    ' meter

    ' standard paper sizes (in points)
    ' must agree with PaperType enumeration
    Friend Shared PaperTypeSize As SizeD() = New SizeD() {New SizeD(8.5 * 72, 11.0 * 72), New SizeD(8.5 * 72, 14.0 * 72), New SizeD(29.7 * 72 / 2.54, 42.0 * 72 / 2.54), New SizeD(21.0 * 72 / 2.54, 29.7 * 72 / 2.54), New SizeD(14.8 * 72 / 2.54, 21.0 * 72 / 2.54)}                  ' letter
    ' legal
    ' A3
    ' A4
    ' A5


    ''' <summary>
    ''' PDF document constructor.
    ''' </summary>
    ''' <param name="FileName">Document file name.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default letter size
    ''' page (height 11”, width 8.5”).</para>
    ''' <para>Page orientation is portrait.</para>
    ''' <para>Unit of measure is points (1/72 inch).</para>
    ''' <para>Scale factor is 1.0.</para>
    ''' <para>The PDF document will be saved in a file named FileName.</para>
    ''' </remarks>

    Public Sub New(FileName As String)
        ' constructor helper
        ConstructorHelper(8.5 * 72.0, 11.0 * 72.0, 1.0, FileName, Nothing)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor.
    ''' </summary>
    ''' <param name="Stream">File or memory stream.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default letter size
    ''' page (height 11”, width 8.5”).</para>
    ''' <para>Page orientation is portrait.</para>
    ''' <para>Unit of measure is points (1/72 inch).</para>
    ''' <para>Scale factor is 1.0.</para>
    ''' <para>The PDF document will be saved in the stream argument. The stream can 
    ''' be either a MemoryStream or a FileStream. It is the calling program
    ''' responsibiliy to close the stream after CreateFile() method
    ''' is called.</para>
    ''' </remarks>

    Public Sub New(Stream As Stream)
        ' constructor helper
        ConstructorHelper(8.5 * 72.0, 11.0 * 72.0, 1.0, Nothing, Stream)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor.
    ''' </summary>
    ''' <param name="Width">Page Width</param>
    ''' <param name="Height">Page height</param>
    ''' <param name="ScaleFactor">Scale factor</param>
    ''' <param name="FileName">Document file name.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' as per width and height arguments in user units.</para>
    ''' <para>Page orientation is portrait if width is less than height.
    ''' Otherwise it is landscape.</para>
    ''' <para>Scale factor is user unit of measure expressed in points.
    ''' For example, Inch has scale factor of 72.0.</para>
    ''' <para>The PDF document will be saved in a file named FileName.</para>
    ''' </remarks>

    Public Sub New(Width As Double, Height As Double, ScaleFactor As Double, FileName As String)            ' page width
        ' page height
        ' scale factor from user units to points (i.e. 72.0 for inch)
        ' constructor helper
        ConstructorHelper(ScaleFactor * Width, ScaleFactor * Height, ScaleFactor, FileName, Nothing)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor.
    ''' </summary>
    ''' <param name="Width">Page Width</param>
    ''' <param name="Height">Page height</param>
    ''' <param name="ScaleFactor">Scale factor</param>
    ''' <param name="Stream">File or memory stream.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' as per width and height arguments in user units.</para>
    ''' <para>Page orientation is portrait if width is less than height.
    ''' Otherwise it is landscape.</para>
    ''' <para>Scale factor is user unit of measure expressed in points.
    ''' For example, Inch has scale factor of 72.0.</para>
    ''' <para>The PDF document will be saved in the stream argument. The stream can 
    ''' be either a MemoryStream or a FileStream. It is the calling program
    ''' responsibiliy to close the stream after CreateFile() method
    ''' is called.</para>
    ''' </remarks>

    Public Sub New(Width As Double, Height As Double, ScaleFactor As Double, Stream As Stream)          ' page width
        ' page height
        ' scale factor from user units to points (i.e. 72.0 for inch)
        ' constructor helper
        ConstructorHelper(ScaleFactor * Width, ScaleFactor * Height, ScaleFactor, Nothing, Stream)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor
    ''' </summary>
    ''' <param name="Width">Page width.</param>
    ''' <param name="Height">Page height.</param>
    ''' <param name="UnitOfMeasure">Unit of measure code.</param>
    ''' <param name="FileName">Document file name.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' as per width and height arguments in user units.</para>
    ''' <para>Page orientation is portrait if width is less than height.
    ''' Otherwise it is landscape.</para>
    ''' <para>Unit of measure is a an enumeration constant (Point, Inch, cm, mm, m)</para>
    ''' <para>The PDF document will be saved in a file named FileName.</para>
    ''' </remarks>

    Public Sub New(Width As Double, Height As Double, UnitOfMeasure As UnitOfMeasure, FileName As String)           ' page width
        ' page height
        ' unit of measure: Point, Inch, cm, mm, m
        ' constructor helper
        Dim Scale = UnitInPoints(UnitOfMeasure)
        ConstructorHelper(Scale * Width, Scale * Height, Scale, FileName, Nothing)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor
    ''' </summary>
    ''' <param name="Width">Page width.</param>
    ''' <param name="Height">Page height.</param>
    ''' <param name="UnitOfMeasure">Unit of measure code.</param>
    ''' <param name="Stream">File or memory stream.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' as per width and height arguments in user units.</para>
    ''' <para>Page orientation is portrait if width is less than height.
    ''' Otherwise it is landscape.</para>
    ''' <para>Unit of measure is a an enumeration constant (Point, Inch, cm, mm, m)</para>
    ''' <para>The PDF document will be saved in the stream argument. The stream can 
    ''' be either a MemoryStream or a FileStream. It is the calling program
    ''' responsibiliy to close the stream after CreateFile() method
    ''' is called.</para>
    ''' </remarks>

    Public Sub New(Width As Double, Height As Double, UnitOfMeasure As UnitOfMeasure, Stream As Stream)         ' page width
        ' page height
        ' unit of measure: Point, Inch, cm, mm, m
        ' constructor helper
        Dim Scale = UnitInPoints(UnitOfMeasure)
        ConstructorHelper(Scale * Width, Scale * Height, Scale, Nothing, Stream)
        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor
    ''' </summary>
    ''' <param name="PaperType">Paper type</param>
    ''' <param name="Landscape">True for landscape, false for portrait.</param>
    ''' <param name="UnitOfMeasure">Unit of measure code.</param>
    ''' <param name="FileName">Document file name.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' of Letter, Leagal, A3, A4 or A5.</para>
    ''' <para>Page orientation is determined by the landscape argument.</para>
    ''' <para>Unit of measure is a an enumeration constant (Point, Inch, cm, mm, m)</para>
    ''' <para>The PDF document will be saved in a file named FileName.</para>
    ''' </remarks>

    Public Sub New(PaperType As PaperType, Landscape As Boolean, UnitOfMeasure As UnitOfMeasure, FileName As String)    ' unit of measure: Point, Inch, cm, mm, m
        ' set scale factor (user units to points)
        Dim Scale = UnitInPoints(UnitOfMeasure)
        Dim Width = PaperTypeSize(PaperType).Width
        Dim Height = PaperTypeSize(PaperType).Height

        ' for landscape swap width and height
        If Landscape Then
            ConstructorHelper(Height, Width, Scale, FileName, Nothing)
        Else
            ConstructorHelper(Width, Height, Scale, FileName, Nothing)
        End If

        Return
    End Sub


    ''' <summary>
    ''' PDF document constructor
    ''' </summary>
    ''' <param name="PaperType">Paper type</param>
    ''' <param name="Landscape">True for landscape, false for portrait.</param>
    ''' <param name="UnitOfMeasure">Unit of measure code.</param>
    ''' <param name="Stream">File or memory stream.</param>
    ''' <remarks>
    ''' <para>This constructor generates a document with default page size
    ''' of Letter, Leagal, A3, A4 or A5.</para>
    ''' <para>Page orientation is determined by the landscape argument.</para>
    ''' <para>Unit of measure is a an enumeration constant (Point, Inch, cm, mm, m)</para>
    ''' <para>The PDF document will be saved in the stream argument. The stream can 
    ''' be either a MemoryStream or a FileStream. It is the calling program
    ''' responsibiliy to close the stream after CreateFile() method
    ''' is called.</para>
    ''' </remarks>

    Public Sub New(PaperType As PaperType, Landscape As Boolean, UnitOfMeasure As UnitOfMeasure, Stream As Stream)  ' unit of measure: Point, Inch, cm, mm, m
        ' set scale factor (user units to points)
        Dim Scale = UnitInPoints(UnitOfMeasure)
        Dim Width = PaperTypeSize(PaperType).Width
        Dim Height = PaperTypeSize(PaperType).Height

        ' for landscape swap width and height
        If Landscape Then
            ConstructorHelper(Height, Width, Scale, Nothing, Stream)
        Else
            ConstructorHelper(Width, Height, Scale, Nothing, Stream)
        End If

        Return
    End Sub


    ' Initial Object Array


    Private Sub ConstructorHelper(Width As Double, Height As Double, ScaleFactor As Double, FileName As String, OutputStream As Stream)           ' page width
        ' page height
        ' scale factor from user units to points (i.e. 72.0 for inch)
        ' set scale factor (user units to points)
        Me.ScaleFactor = ScaleFactor

        ' set epsilon (1/300 of an inch in user units)
        Epsilon = 72.0 / (300.0 * ScaleFactor)

        ' save page default size
        PageSize = New SizeD(Width, Height)

        ' PDF document root object the Catalog object
        CatalogObject = New PdfObject(Me, ObjectType.Dictionary, "/Catalog")

        ' add viewer preferences
        CatalogObject.Dictionary.Add("/ViewerPreferences", "<</PrintScaling/None>>")

        ' Parent object for all pages
        PagesObject = New PdfObject(Me, ObjectType.Dictionary, "/Pages")

        ' add indirect reference to pages within the catalog object
        CatalogObject.Dictionary.AddIndirectReference("/Pages", PagesObject)

        ' create trailer dictionary
        TrailerDict = New PdfDictionary(Me)

        ' add /Root
        TrailerDict.AddIndirectReference("/Root", CatalogObject)

        ' document id
        DocumentID = RandomByteArray(16)

        ' add /ID
        TrailerDict.AddFormat("/ID", "[{0}{0}]", ByteArrayToPdfHexString(DocumentID))

        ' create file using file name
        If Not Equals(FileName, Nothing) Then
            ' save file name
            Me.FileName = FileName

            ' constructor helper

            ' write to caller's file or memory stream
            PdfFile = New PdfBinaryWriter(New FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
        Else
            PdfFile = New PdfBinaryWriter(OutputStream)
        End If

        ' write PDF version number
        PdfFile.WriteString("%PDF-1.7" & Microsoft.VisualBasic.Constants.vbLf)

        ' add this comment to tell compression programs that this is a binary file
        PdfFile.WriteString("%µµµµ" & Microsoft.VisualBasic.Constants.vbLf)

        ' exit
        Return
    End Sub


    ''' <summary>
    ''' Set encryption
    ''' </summary>
    ''' <remarks>
    ''' The PDF File Writer library will encrypt the PDF document
    ''' using AES-128 encryption. User password set to default. Owner 
    ''' password is set to a random number.
    ''' A PDF reader such as Acrobat will open the document with the 
    ''' default user password. Permissions flags are set to allow all.
    ''' </remarks>

    Public Sub SetEncryption()
        SetEncryption(Nothing, Nothing, Permission.All, EncryptionType.Aes128)
        Return
    End Sub


    ''' <summary>
    ''' Set encryption
    ''' </summary>
    ''' <param name="Permissions">Permission flags.</param>
    ''' <remarks>
    ''' The PDF File Writer library will encrypt the PDF document
    ''' using AES-128 encryption. User password set to default. Owner 
    ''' password is set to a random number.
    ''' A PDF reader such as Acrobat will open the document with the 
    ''' default user password. Permissions flags are set as per argument.
    ''' </remarks>

    Public Sub SetEncryption(Permissions As Permission)
        SetEncryption(Nothing, Nothing, Permissions, EncryptionType.Aes128)
        Return
    End Sub


    ''' <summary>
    ''' Set encryption
    ''' </summary>
    ''' <param name="UserPassword">User password</param>
    ''' <param name="Permissions">Permission flags</param>
    ''' <remarks>
    ''' The PDF File Writer library will encrypt the PDF document
    ''' using AES-128 encryption. User password is as per argument. Owner 
    ''' password is set to a random number.
    ''' A PDF reader such as Acrobat will request the user to enter a password.
    ''' The document can only be opened with the user password. The owner password
    ''' being random is effectively unknown.
    ''' Permissions flags are set as per argument.
    ''' </remarks>

    Public Sub SetEncryption(UserPassword As String, Permissions As Permission)
        SetEncryption(UserPassword, Nothing, Permissions, EncryptionType.Aes128)
        Return
    End Sub


    ''' <summary>
    ''' Set encryption
    ''' </summary>
    ''' <param name="UserPassword">User password</param>
    ''' <param name="OwnerPassword">Owner password</param>
    ''' <param name="Permissions">Permission flags</param>
    ''' <param name="EncryptionType">Encryption type</param>
    ''' <remarks>
    ''' The PDF File Writer library will encrypt the PDF document
    ''' using either AES-128 encryption or standard 128 (RC4) encryption.
    ''' Encryption type is specified by the last argument. Note: the 
    ''' standard 128 (RC4) is considered unsafe and should not be used.
    ''' User and owner passwords are as per
    ''' the two arguments. A PDF reader such as Acrobat will request the 
    ''' user to enter a password. The user can supply either the user
    ''' or the owner password. Permissions flags are set as per argument.
    ''' </remarks>

    Public Sub SetEncryption(UserPassword As String, OwnerPassword As String, Permissions As Permission, Optional EncryptionType As EncryptionType = EncryptionType.Aes128)
        ' encryption can be set only once
        If Encryption IsNot Nothing Then Throw New ApplicationException("Encryption is already set")

        ' create encryption dictionary object
        Encryption = New PdfEncryption(Me, UserPassword, OwnerPassword, Permissions, EncryptionType)

        ' exit
        Return
    End Sub


    ''' <summary>
    ''' Gets bookmarks root
    ''' </summary>
    ''' <returns>Root bookmark object</returns>

    Public Function GetBookmarksRoot() As PdfBookmark
        ' create bookmarks root node if this is the first time
        If BookmarksRoot Is Nothing Then BookmarksRoot = New PdfBookmark(Me)

        ' return bookmarks node to the user
        Return BookmarksRoot
    End Function

    ''' <summary>
    ''' Move page to another position
    ''' </summary>
    ''' <param name="SourceIndex">Page's current position</param>
    ''' <param name="DestinationIndex">Page's new position</param>
    Public Sub MovePage(SourceIndex As Integer, DestinationIndex As Integer)
        If SourceIndex < 0 OrElse SourceIndex >= PageCount OrElse DestinationIndex < 0 OrElse DestinationIndex > PageCount Then Throw New ApplicationException("Move page invalid argument")

        ' there is only one page or no move
        If DestinationIndex <> SourceIndex AndAlso DestinationIndex <> SourceIndex + 1 Then
            Dim SourcePage = PageArray(SourceIndex)
            PageArray.RemoveAt(SourceIndex)
            If DestinationIndex > SourceIndex Then DestinationIndex -= 1
            PageArray.Insert(DestinationIndex, SourcePage)
        End If

        Return
    End Sub


    ''' <summary>
    ''' Create PDF document file
    ''' </summary>
    ''' <remarks>
    ''' <para>The last step of document creation after all pages were constructed.</para>
    ''' <para>If PdfDocument was constructed with a file name,
    ''' the CreateFile method will close the file after the file is
    ''' written to. If the PdfDocument was constructed with a stream,
    ''' the CreateFile does not close the stream. It is the user application
    ''' that should close the stream after the stream was used.</para>
    ''' </remarks>

    Public Sub CreateFile()
        ' add destinations to link annotation
        AddDestToLinkAnnot()

        ' create named destinations
        CreateNamedDestinations()

        ' Optional content properties dictionary
        If Layers IsNot Nothing AndAlso Layers.LayerList.Count > 0 Then
            ' create optional content dictionary
            Layers.CreateDictionary()

            ' add to catalog object
            CatalogObject.Dictionary.AddIndirectReference("/OCProperties", Layers)
        End If

        ' create page array
        Dim Kids As StringBuilder = New StringBuilder("[")

        For Index = 0 To PageArray.Count - 1
            Kids.AppendFormat("{0} 0 R ", PageArray(Index).ObjectNumber)
        Next

        If Kids.Length > 1 Then Kids.Length -= 1
        Kids.Append("]")
        PagesObject.Dictionary.Add("/Kids", Kids.ToString())

        ' page count
        PagesObject.Dictionary.AddInteger("/Count", PageArray.Count)

        ' page mode
        If InitialDocDisplay <> InitialDocDisplay.UseNone Then CatalogObject.Dictionary.Add("/PageMode", InitDocDispText(InitialDocDisplay))

        ' objects
        For Index = 0 To ObjectArray.Count - 1
            If ObjectArray(Index).FilePosition = 0 Then ObjectArray(Index).WriteObjectToPdfFile()
        Next

        ' save cross reference table position
        Dim XRefPos As Integer = PdfFile.BaseStream.Position

        ' cross reference
        PdfFile.WriteFormat("xref" & Microsoft.VisualBasic.Constants.vbLf & "0 {0}" & Microsoft.VisualBasic.Constants.vbLf & "0000000000 65535 f " & Microsoft.VisualBasic.Constants.vbLf, ObjectArray.Count + 1)

        For Each PO In ObjectArray

            If PO.FilePosition <> 0 Then
                PdfFile.WriteFormat("{0:0000000000} 00000 n " & Microsoft.VisualBasic.Constants.vbLf, PO.FilePosition)
            Else
                PdfFile.WriteString("0000000000 00000 f " & Microsoft.VisualBasic.Constants.vbLf)
            End If
        Next

        ' finalize trailer dictionary
        TrailerDict.AddInteger("/Size", ObjectArray.Count + 1)

        ' trailer
        PdfFile.WriteString("trailer" & Microsoft.VisualBasic.Constants.vbLf)
        TrailerDict.WriteToPdfFile()
        PdfFile.WriteFormat("startxref" & Microsoft.VisualBasic.Constants.vbLf & "{0}" & Microsoft.VisualBasic.Constants.vbLf, XRefPos)

        ' write PDF end of file marker
        PdfFile.WriteString("%%EOF" & Microsoft.VisualBasic.Constants.vbLf)

        ' close file and dispose all open resources
        Dispose()

        ' successful exit
        Return
    End Sub

    Friend Sub AddDestToLinkAnnot()
        If LinkAnnotArray Is Nothing Then Return

        For Each Annot In LinkAnnotArray
            ' search for location marker name
            Dim LocMarkerName = CType(Annot.AnnotAction, AnnotLinkAction).LocMarkerName
            Dim Index As Integer = LocMarkerArray.BinarySearch(New LocationMarker(LocMarkerName))

            ' no location marker was defined for this name
            If Index < 0 Then Throw New ApplicationException("No location marker was defined for: " & LocMarkerName)

            ' add action
            Annot.Dictionary.AddFormat("/A", "<</Type/Action/S/GoTo/D{0}>>", Annot.Document.LocMarkerArray(Index).DestStr)
        Next

        Return
    End Sub

    Friend Sub CreateNamedDestinations()
        ' destination array is empty
        If LocMarkerArray Is Nothing Then Return
        Dim NamedDest As PdfObject = Nothing
        Dim DestStr As StringBuilder = Nothing

        For Each LocMarker In LocMarkerArray
            If LocMarker.Scope <> LocMarkerScope.NamedDest Then Continue For

            If NamedDest Is Nothing Then
                NamedDest = New PdfObject(Me)
                DestStr = New StringBuilder("[")
            End If

            DestStr.AppendFormat("{0}{1}", TextToPdfString(LocMarker.LocMarkerName, NamedDest), LocMarker.DestStr)
        Next

        If NamedDest Is Nothing Then Return

        ' add one dictionary entry
        DestStr.Append("]")
        NamedDest.Dictionary.Add("/Names", DestStr.ToString())

        ' attach it to PDF catalog
        CatalogObject.Dictionary.AddFormat("/Names", "<</Dests {0} 0 R>>", NamedDest.ObjectNumber)
        Return
    End Sub


    ' Convert byte array to PDF string
    ' used for document id and encryption


    Friend Function ByteArrayToPdfHexString(ByteArray As Byte()) As String
        ' convert to hex string
        Dim HexText As StringBuilder = New StringBuilder("<")

        For index = 0 To ByteArray.Length - 1
            HexText.AppendFormat("{0:x2}", CInt(ByteArray(index)))
        Next

        HexText.Append(">")
        Return HexText.ToString()
    End Function


    ' C# string text to PDF strings only


    Friend Function TextToPdfString(Text As String, Parent As PdfObject) As String
        ' convert C# string to byte array
        Dim ByteArray = TextToByteArray(Text)

        ' encryption is active. PDF string must be encrypted except for encryption dictionary
        If Parent IsNot Nothing AndAlso Encryption IsNot Nothing AndAlso Encryption IsNot Parent Then ByteArray = Encryption.EncryptByteArray(Parent.ObjectNumber, ByteArray)

        ' convert byte array to PDF string format
        Return ByteArrayToPdfString(ByteArray)
    End Function


    ' C# string text to byte array
    ' This method is used for PDF strings only


    Friend Function TextToByteArray(Text As String) As Byte()
        ' scan input text for Unicode characters and for non printing characters
        Dim Unicode = False

        For Each TestChar In Text
            ' test for non printable characters
            If TestChar < " "c OrElse TestChar > "~"c AndAlso AscW(TestChar) < 160 Then Throw New ApplicationException("Text string must be made of printable characters")

            ' test for Unicode string
            If AscW(TestChar) > 255 Then Unicode = True
        Next

        ' declare output byte array
        Dim ByteArray As Byte() = Nothing

        ' all characters are one byte long
        If Not Unicode Then
            ' save each imput character in one byte
            ByteArray = New Byte(Text.Length - 1) {}
            Dim Index As i32 = 0

            ' Unicode case. we have some two bytes characters
            For Each TestChar In Text
                ByteArray(++Index) = Microsoft.VisualBasic.AscW(TestChar)
            Next
        Else
            ' allocate output byte array
            ByteArray = New Byte(2 * Text.Length + 2 - 1) {}

            ' add Unicode marker at the start of the string
            ByteArray(0) = &HFE
            ByteArray(1) = &HFF

            ' save each character as two bytes
            Dim Index As i32 = 2

            For Each TestChar In Text
                ByteArray(++Index) = CByte(AscW(TestChar) >> 8)
                ByteArray(++Index) = Microsoft.VisualBasic.AscW(TestChar)
            Next
        End If

        ' return output byte array
        Return ByteArray
    End Function


    ' byte array to PDF string
    ' This method is used for PDF strings only


    Friend Function ByteArrayToPdfString(ByteArray As Byte()) As String
        ' create output string with open and closing parenthesis
        Dim Str As StringBuilder = New StringBuilder("(")

        For Each TestByte In ByteArray
            ' CR and NL must be replaced by \r and \n
            ' Otherwise PDF readers will convert CR or NL or CR-NL to NL
            If TestByte = 13 Then
                Str.Append("\r")

                ' the three characters \ ( ) must be preceded by \
            ElseIf TestByte = 10 Then
                Str.Append("\n")
            Else
                If TestByte = Microsoft.VisualBasic.AscW("\"c) OrElse TestByte = Microsoft.VisualBasic.AscW("("c) OrElse TestByte = Microsoft.VisualBasic.AscW(")"c) Then Str.Append("\"c)
                Str.Append(Microsoft.VisualBasic.ChrW(TestByte))
            End If
        Next

        Str.Append(")"c)
        Return Str.ToString()
    End Function


    ' Create random byte array


    Friend Shared Function RandomByteArray(Length As Integer) As Byte()
        Dim ByteArray = New Byte(Length - 1) {}

        Using RandNumGen As RNGCryptoServiceProvider = New RNGCryptoServiceProvider()
            RandNumGen.GetBytes(ByteArray)
        End Using

        Return ByteArray
    End Function


    ' Generate unique resource number


    Friend Function GenerateResourceNumber(Code As Char) As String        ' one letter code for each type of resource
        ' create resource code
        Return String.Format("/{0}{1}", Code, Threading.Interlocked.Increment(ResCodeNo(PdfObject.ResCodeLetter.IndexOf(Code))))
    End Function


    ''' <summary>
    ''' Dispose PDF document object
    ''' </summary>

    Public Sub Dispose() Implements IDisposable.Dispose
        ' close output file
        ' Note: stream input will not be closed
        If Not Equals(FileName, Nothing) AndAlso PdfFile IsNot Nothing Then
            PdfFile.Close()
            PdfFile = Nothing
        End If

        ' dispose all objects with IDisposable interface
        For Each Obj In ObjectArray
            If TypeOf Obj Is IDisposable Then CType(Obj, IDisposable).Dispose()
        Next

        Return
    End Sub
End Class
