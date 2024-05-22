#Region "Microsoft.VisualBasic::9a4ecfe50ac2f18a6e9dde7d23476422, mime\application%pdf\PdfFileWriter\PDF\PdfEmbeddedFile.vb"

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

    '   Total Lines: 207
    '    Code Lines: 79 (38.16%)
    ' Comment Lines: 91 (43.96%)
    '    - Xml Docs: 47.25%
    ' 
    '   Blank Lines: 37 (17.87%)
    '     File Size: 7.56 KB


    ' Class PdfEmbeddedFile
    ' 
    '     Properties: FileName, MimeType
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: CompareTo, CreateEmbeddedFile
    ' 
    ' Class ExtToMime
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CompareTo, TranslateExtToMime
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfEmbeddedFile
'	PDF embedded file class. 
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

Imports System.IO

''' <summary>
''' PDF Embedded file class
''' </summary>
Public Class PdfEmbeddedFile
    Inherits PdfObject
    Implements IComparable(Of PdfEmbeddedFile)

    ''' <summary>
    ''' Gets file name
    ''' </summary>
    Public Property FileName As String

    ''' <summary>
    ''' Gets Mime type
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The PDF embedded file translates the file extension into mime type string.
    ''' If the translation fails the MimeType is set to null.
    ''' </para>
    ''' </remarks>
    Public Property MimeType As String

    Private Sub New()
    End Sub

    Private Sub New(Document As PdfDocument, FileName As String, PdfFileName As String)
        MyBase.New(Document, ObjectType.Dictionary, "/Filespec")
        ' save file name
        Me.FileName = FileName

        ' test exitance
        If Not File.Exists(FileName) Then Throw New ApplicationException("Embedded file " & FileName & " does not exist")

        ' get file length
        Dim FI As FileInfo = New FileInfo(FileName)
        If FI.Length > Integer.MaxValue - 4095 Then Throw New ApplicationException("Embedded file " & FileName & " too long")
        Dim FileLength As Integer = FI.Length

        ' translate file extension to mime type string
        MimeType = ExtToMime.TranslateExtToMime(FI.Extension)

        ' create embedded file object
        Dim EmbeddedFile As PdfObject = New PdfObject(Document, ObjectType.Stream, "/EmbeddedFile")

        ' save uncompressed file length
        EmbeddedFile.Dictionary.AddFormat("/Params", "<</Size {0}>>", FileLength)

        ' file data content byte array
        EmbeddedFile.ObjectValueArray = New Byte(FileLength - 1) {}

        ' load all the file's data
        Dim DataStream As FileStream = Nothing

        Try
            ' open the file
            DataStream = New FileStream(FileName, FileMode.Open, FileAccess.Read)

            ' read all the file
            If DataStream.Read(EmbeddedFile.ObjectValueArray, 0, FileLength) <> FileLength Then Throw New Exception()

            ' loading file failed
        Catch __unusedException1__ As Exception
            Throw New ApplicationException("Invalid media file: " & FileName)
        End Try

        ' close the file
        DataStream.Close()

        ' debug
        If Document.Debug Then EmbeddedFile.ObjectValueArray = Document.TextToByteArray("*** MEDIA FILE PLACE HOLDER ***")

        ' write stream
        EmbeddedFile.WriteObjectToPdfFile()

        ' file spec object type
        Dictionary.Add("/Type", "/Filespec")

        ' PDF file name
        If String.IsNullOrWhiteSpace(PdfFileName) Then PdfFileName = FI.Name
        Dictionary.AddPdfString("/F", PdfFileName)
        Dictionary.AddPdfString("/UF", PdfFileName)

        ' add reference
        Dictionary.AddFormat("/EF", "<</F {0} 0 R /UF {0} 0 R>>", EmbeddedFile.ObjectNumber)
    End Sub

    Private Sub New(FileName As String)
        ' save file name
        Me.FileName = FileName
    End Sub

    ''' <summary>
    ''' PDF embedded file class constructor
    ''' </summary>
    ''' <param name="Document">Current document</param>
    ''' <param name="FileName">File name</param>
    ''' <param name="PdfFileName">PDF file name (see remarks)</param>
    ''' <returns>PdfEmbeddedFile object</returns>
    ''' <remarks>
    ''' <para>
    ''' FileName is the name of the source file on the hard disk.
    ''' PDFFileName is the name of the as saved within the PDF document file.
    ''' If PDFFileName is not given or it is set to null, the class takes
    ''' the hard disk's file name without the path.
    ''' </para>
    ''' </remarks>
    Public Shared Function CreateEmbeddedFile(Document As PdfDocument, FileName As String, Optional PdfFileName As String = Nothing) As PdfEmbeddedFile
        ' first time
        If Document.EmbeddedFileArray Is Nothing Then Document.EmbeddedFileArray = New List(Of PdfEmbeddedFile)()

        ' search list for a duplicate
        Dim Index As Integer = Document.EmbeddedFileArray.BinarySearch(New PdfEmbeddedFile(FileName))

        ' this is a duplicate
        If Index >= 0 Then Return Document.EmbeddedFileArray(Index)

        ' new object
        Dim EmbeddedFile As PdfEmbeddedFile = New PdfEmbeddedFile(Document, FileName, PdfFileName)

        ' save new string in array
        Document.EmbeddedFileArray.Insert(Not Index, EmbeddedFile)

        ' exit
        Return EmbeddedFile
    End Function

    ''' <summary>
    ''' Compare two PdfEmbededFile objects
    ''' </summary>
    ''' <param name="Other">Other argument</param>
    ''' <returns>Compare result</returns>
    Public Overloads Function CompareTo(Other As PdfEmbeddedFile) As Integer Implements IComparable(Of PdfEmbeddedFile).CompareTo
        Return String.Compare(FileName, Other.FileName, True)
    End Function
End Class

Friend Class ExtToMime : Implements IComparable(Of ExtToMime)

    Friend Ext As String
    Friend Mime As String

    Friend Sub New(Ext As String, Mime As String)
        Me.Ext = Ext
        Me.Mime = Mime
    End Sub

    Friend Shared Function TranslateExtToMime(Ext As String) As String
        Dim Index As Integer = Array.BinarySearch(ExtToMimeArray, New ExtToMime(Ext, Nothing))
        Return If(Index >= 0, ExtToMimeArray(Index).Mime, Nothing)
    End Function

    ''' <summary>
    ''' Compare ExtToMime records
    ''' </summary>
    ''' <param name="Other">Other record</param>
    ''' <returns></returns>
    Public Function CompareTo(Other As ExtToMime) As Integer Implements IComparable(Of ExtToMime).CompareTo
        Return String.Compare(Ext, Other.Ext, True)
    End Function

    ''' <summary>
    ''' Covers most Windows-compatible formats including .avi and .divx
    ''' </summary>
    Private Shared ExtToMimeArray As ExtToMime() = {
        New ExtToMime(".avi", "video/avi"),
        New ExtToMime(".divx", "video/avi"),      ' Covers most Windows-compatible formats including .avi and .divx
        New ExtToMime(".mpg", "video/mpeg"),      ' MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046
        New ExtToMime(".mpeg", "video/mpeg"),     ' MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046
        New ExtToMime(".mp4", "video/mp4"),       ' MP4 video; Defined in RFC 4337
        New ExtToMime(".mov", "video/quicktime"), ' QuickTime video .mov
        New ExtToMime(".wav", "audio/wav"),       ' audio
        New ExtToMime(".wma", "audio/x-ms-wma"),  ' audio
        New ExtToMime(".mp3", "audio/mpeg")       ' audio
    }

    Shared Sub New()
        Array.Sort(ExtToMimeArray)
    End Sub
End Class
