''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	CreateMetafile
'	Create Metafile with graphics object.
'	It was used to test the PdfObject class with Metafile image.
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
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices


    ''' <summary>
    ''' Create image metafile class
    ''' </summary>
    Public Class CreateMetafile
        Implements IDisposable

        ''' <summary>
        ''' Gets graphics object form image metafile.
        ''' </summary>
        Private _Metafile As System.Drawing.Imaging.Metafile, _Graphics As System.Drawing.Graphics

        ''' <summary>
        ''' Gets image metafile.
        ''' </summary>
        Public Property Metafile As Metafile
            Get
                Return _Metafile
            End Get
            Protected Set(ByVal value As Metafile)
                _Metafile = value
            End Set
        End Property

        Public Property Graphics As Graphics
            Get
                Return _Graphics
            End Get
            Protected Set(ByVal value As Graphics)
                _Graphics = value
            End Set
        End Property

        ''' <summary>
        ''' Create image metafile constructor
        ''' </summary>
        ''' <param name="Width">Image width in pixels.</param>
        ''' <param name="Height">Image height in pixels.</param>
        Public Sub New(ByVal Width As Integer, ByVal Height As Integer)
            Using Stream As MemoryStream = New MemoryStream()

                Using MemoryGraphics = Graphics.FromHwndInternal(IntPtr.Zero)
                    Dim deviceContextHandle As IntPtr = MemoryGraphics.GetHdc()
                    Metafile = New Metafile(Stream, deviceContextHandle, New RectangleF(0, 0, Width, Height), MetafileFrameUnit.Pixel, EmfType.EmfPlusOnly)
                    MemoryGraphics.ReleaseHdc()
                End Using
            End Using

            Graphics = Graphics.FromImage(Metafile)

            ' Set everything to high quality
            Graphics.SmoothingMode = SmoothingMode.HighQuality
            Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
            Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality
            Graphics.CompositingQuality = CompositingQuality.HighQuality
            Graphics.PageUnit = GraphicsUnit.Pixel
            Return
        End Sub

        <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
        Private Shared Function CopyEnhMetaFile(ByVal MetaFileHandle As IntPtr, ByVal FileName As IntPtr) As IntPtr
        End Function

        ''' <summary>
        ''' Save image metafile
        ''' </summary>
        ''' <param name="FileName">File name</param>
        Public Sub SaveMetafile(ByVal FileName As String)
            ' Get a handle to the metafile
            Dim MetafileHandle As IntPtr = Metafile.GetHenhmetafile()

            ' allocate character table buffer in global memory (two bytes per char)
            Dim CharBuffer = Marshal.AllocHGlobal(2 * FileName.Length + 2)

            ' move file name inclusing terminating zer0 to the buffer
            For Index = 0 To FileName.Length - 1
                Marshal.WriteInt16(CharBuffer, 2 * Index, CShort(Microsoft.VisualBasic.AscW(FileName(Index))))
            Next

            Marshal.WriteInt16(CharBuffer, 2 * FileName.Length, 0)

            ' Export metafile to an image file
            CopyEnhMetaFile(MetafileHandle, CharBuffer)

            ' free local buffer
            Marshal.FreeHGlobal(CharBuffer)
            Return
        End Sub

        <DllImport("gdi32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall, SetLastError:=True)>
        Private Shared Function DeleteEnhMetaFile(ByVal MetaFileHandle As IntPtr) As IntPtr
        End Function

        ''' <summary>
        ''' Delete image metafile.
        ''' </summary>
        Public Sub DeleteMetafile()
            ' Get a handle to the metafile
            Dim MetafileHandle As IntPtr = Metafile.GetHenhmetafile()

            ' Delete the metafile from memory
            DeleteEnhMetaFile(MetafileHandle)
            Return
        End Sub

        ''' <summary>
        ''' Dispose object
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Graphics IsNot Nothing Then
                Graphics.Dispose()
                Graphics = Nothing
            End If

            If Metafile IsNot Nothing Then
                Metafile.Dispose()
                Metafile = Nothing
            End If

            Return
        End Sub
    End Class

