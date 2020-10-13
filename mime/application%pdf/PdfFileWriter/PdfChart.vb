''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfChart
'	Display charts in the PDF document.
'  Charts are displayed as images.
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
Imports System.Drawing
Imports System.Windows.Forms.DataVisualization.Charting


    ''' <summary>
    ''' Font size units for PdfChart.CreateFont method enumeration
    ''' </summary>
    Public Enum FontSizeUnit
        ''' <summary>
        ''' Pixel
        ''' </summary>
        Pixel

        ''' <summary>
        ''' Point
        ''' </summary>
        Point

        ''' <summary>
        ''' PDF document user unit
        ''' </summary>
        UserUnit

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
    End Enum

    ''' <summary>
    ''' PDF chart resource class
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#ChartingSupport">2.10 Charting Support</a>
    ''' </para>
    ''' <para>
    ''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawChart">For example of drawing image see 3.11. Draw Pie Chart</a>
    ''' </para>
    ''' </remarks>
    Public Class PdfChart
        Inherits PdfImage
        ''' <summary>
        ''' Chart object (.NET).
        ''' </summary>

        ''' <summary>
        ''' Chart width in user units.
        ''' </summary>

        ''' <summary>
        ''' Chart height in user units.
        ''' </summary>
        Private _MSChart As System.Windows.Forms.DataVisualization.Charting.Chart, _Width As Double, _Height As Double

        Public Property MSChart As Chart
            Get
                Return _MSChart
            End Get
            Private Set(ByVal value As Chart)
                _MSChart = value
            End Set
        End Property            ' chart object

        Public Property Width As Double
            Get
                Return _Width
            End Get
            Private Set(ByVal value As Double)
                _Width = value
            End Set
        End Property            ' width in user units

        Public Property Height As Double
            Get
                Return _Height
            End Get
            Private Set(ByVal value As Double)
                _Height = value
            End Set
        End Property        ' height in user units

        ''' <summary>
        ''' Set output resolution 
        ''' </summary>
        Public Overrides Property Resolution As Double
            Get
                Return _Resolution
            End Get
            Set(ByVal value As Double)
                If value < 0 Then Throw New ApplicationException("Resolution must be greater than zero, or zero for default")

                ' save resolution
                _Resolution = value

                ' take resolution from MS chart
                If _Resolution = 0 Then
                    _Resolution = MSChart.RenderingDpiY
                Else
                    MSChart.RenderingDpiY = _Resolution
                End If

                ' calculate chart size in user coordinates
                Width = WidthPix * 72.0 / (Resolution * Document.ScaleFactor)
                Height = HeightPix * 72.0 / (Resolution * Document.ScaleFactor)
            End Set
        End Property

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''' <summary>
        ''' PDF chart constructor
        ''' </summary>
        ''' <param name="Document">Document object parent of this chart.</param>
        ''' <param name="MSChart">.NET Chart object.</param>
        ''' <remarks>
        ''' It is the responsibility of the calling program to release
        ''' the resources of the input chart object. After PdfChart
        ''' is commited to the PDF file
        ''' </remarks>
        ''' <example> 
        ''' <code>
        ''' 	// create chart
        ''' 	Chart MyChart = new Chart();
        ''' 	// build chart
        ''' 	// ...
        ''' 	// ...
        ''' PdfImageControl ImageControl = new PdfImageControl();
        ''' 	ImageControl.SaveAs = SaveImageAs.IndexedImage;
        ''' 	PdfChart MyPdfChart = new PdfChart(Document, MyChart, ImageControl);
        ''' 	MyPdfChart.CommitToPdfFile();
        ''' 	MyChart.Dispose();
        ''' </code>
        ''' </example>
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Sub New(ByVal Document As PdfDocument, ByVal MSChart As Chart)
            MyBase.New(Document)
            ' save chart
            Me.MSChart = MSChart
            WidthPix = MSChart.Width
            HeightPix = MSChart.Height

            ' take resolution from MS chart
            Resolution = MSChart.RenderingDpiY

            ' calculate chart size in user coordinates
            Width = WidthPix * 72.0 / (Resolution * Document.ScaleFactor)
            Height = HeightPix * 72.0 / (Resolution * Document.ScaleFactor)

            ' exit
            Return
        End Sub

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''' <summary>
        ''' Static method to create .NET Chart object.
        ''' </summary>
        ''' <param name="Document">Current document object.</param>
        ''' <param name="Width">Chart width in user units.</param>
        ''' <param name="Height">Chart height in user units.</param>
        ''' <param name="Resolution">Resolution in pixels per inch (optional argument).</param>
        ''' <returns>.NET Chart object</returns>
        ''' <remarks>
        ''' The returned chart has the correct width and height in pixels.
        ''' </remarks>
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Shared Function CreateChart(ByVal Document As PdfDocument, ByVal Width As Double, ByVal Height As Double, ByVal Optional Resolution As Double = 0.0) As Chart
            ' create chart
            Dim MSChart As Chart = New Chart()

            ' save resolution
            If Resolution <> 0 Then MSChart.RenderingDpiY = Resolution

            ' image size in pixels
            MSChart.Width = CInt(MSChart.RenderingDpiY * Width * Document.ScaleFactor / 72.0 + 0.5)
            MSChart.Height = CInt(MSChart.RenderingDpiY * Height * Document.ScaleFactor / 72.0 + 0.5)

            ' return chart
            Return MSChart
        End Function

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''' <summary>
        ''' Helper method to create a font for chart drawing.
        ''' </summary>
        ''' <param name="FontFamilyName">Font family name.</param>
        ''' <param name="FontStyle">Font style.</param>
        ''' <param name="FontSize">Font size per unit argument.</param>
        ''' <param name="Unit">Font size unit.</param>
        ''' <returns>.NET font</returns>
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Function CreateFont(ByVal FontFamilyName As String, ByVal FontStyle As FontStyle, ByVal FontSize As Double, ByVal Unit As FontSizeUnit) As Font  ' font family name
            ' font style
            ' as per units below
            ' unit of measure
            ' calculate size
            Dim SizeInPixels = 0

            Select Case Unit
                Case FontSizeUnit.Pixel
                    SizeInPixels = CInt(FontSize + 0.5)
                Case FontSizeUnit.Point
                    SizeInPixels = CInt(FontSize * _Resolution / 72.0 + 0.5)
                Case FontSizeUnit.UserUnit
                    SizeInPixels = CInt(FontSize * _Resolution * Document.ScaleFactor / 72.0 + 0.5)
                Case FontSizeUnit.Inch
                    SizeInPixels = CInt(FontSize * _Resolution + 0.5)
                Case FontSizeUnit.cm
                    SizeInPixels = CInt(FontSize * _Resolution / 2.54 + 0.5)
                Case FontSizeUnit.mm
                    SizeInPixels = CInt(FontSize * _Resolution / 25.4 + 0.5)
            End Select

            ' create font
            Return New Font(FontFamilyName, SizeInPixels, FontStyle, GraphicsUnit.Pixel)
        End Function

        ''' <summary>
        ''' Commit object to PDF file
        ''' </summary>
        ''' <param name="DisposeChart">Dispose Chart object</param>
        ''' <param name="GCCollect">Activate Garbage Collector</param>
        Public Sub CommitToPdfFile(ByVal DisposeChart As Boolean, ByVal GCCollect As Boolean)
            ' make sure not to do it twice
            If FilePosition = 0 Then
                ' call PdfObject routine
                WriteObjectToPdfFile()

                ' dispose chart
                If DisposeChart Then
                    MSChart.Dispose()
                    MSChart = Nothing
                End If

                ' activate garbage collector
                If GCCollect Then GC.Collect()
            End If

            ' exit
            Return
        End Sub

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' Write object to PDF file
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        Friend Overrides Sub WriteObjectToPdfFile()
            ' convert chart to bitmap
            Picture = New Bitmap(WidthPix, HeightPix)
            MSChart.DrawToBitmap(Picture, New Rectangle(0, 0, WidthPix, HeightPix))
            DisposePicture = True

            ' call Image class WriteObjectToPdfFile
            MyBase.WriteObjectToPdfFile()
            Return
        End Sub
    End Class

