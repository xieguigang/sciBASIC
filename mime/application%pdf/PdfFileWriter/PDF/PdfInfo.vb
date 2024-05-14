#Region "Microsoft.VisualBasic::5226191c6ecf7811df3dcb0a34f69ba0, mime\application%pdf\PdfFileWriter\PDF\PdfInfo.vb"

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

    '   Total Lines: 147
    '    Code Lines: 51
    ' Comment Lines: 82
    '   Blank Lines: 14
    '     File Size: 5.34 KB


    '     Class PdfInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreatePdfInfo
    ' 
    '         Sub: Author, CreationDate, Creator, Keywords, ModDate
    '              Producer, Subject, Title
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfInfo
'	PDF document information dictionary.
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
    ''' PDF document information dictionary
    ''' </summary>
    Public Class PdfInfo
        Inherits PdfObject
    ''' <summary>
    ''' Constructor for PdfInfo class
    ''' </summary>
    ''' <param name="Document">Main document class</param>
    ''' <returns>PdfInfo object</returns>
    ''' <remarks>
    ''' <para>The constructor initialize the /Info dictionary with 4 key value pairs. </para>
    ''' <list type="table">
    ''' <item><description>Creation date set to current local system date</description></item>
    ''' <item><description>Modification date set to current local system date</description></item>
    ''' <item><description>Creator is PdfFileWriter C# Class Library Version No</description></item>
    ''' <item><description>Producer is PdfFileWriter C# Class Library Version No</description></item>
    ''' </list>
    ''' </remarks>
    Public Shared Function CreatePdfInfo(Document As PdfDocument) As PdfInfo
            ' create a new default info object
            If Document.InfoObject Is Nothing Then
                ' create and add info object to trailer dictionary
                Document.InfoObject = New PdfInfo(Document)
                Document.TrailerDict.AddIndirectReference("/Info", Document.InfoObject)
            End If

            ' exit with either existing object or a new one
            Return Document.InfoObject
        End Function

        ''' <summary>
        ''' Protected constructor
        ''' </summary>
        ''' <param name="Document">Main document object</param>
        Protected Sub New(Document As PdfDocument)
            MyBase.New(Document, ObjectType.Dictionary)
            ' set creation and modify dates
            Dim LocalTime = Date.Now
            CreationDate(LocalTime)
            ModDate(LocalTime)

            ' set creator and producer
            Creator("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)
            Producer("PdfFileWriter C# Class Library Version " & PdfDocument.RevisionNumber)
            Return
        End Sub

        ''' <summary>
        ''' Sets document creation date and time
        ''' </summary>
        ''' <param name="Date">Creation date and time</param>
        Public Sub CreationDate([Date] As Date)
            Dictionary.AddPdfString("/CreationDate", String.Format("D:{0}", [Date].ToString("yyyyMMddHHmmss")))
            Return
        End Sub

        ''' <summary>
        ''' Sets document last modify date and time
        ''' </summary>
        ''' <param name="Date">Modify date and time</param>
        Public Sub ModDate([Date] As Date)
            Dictionary.AddPdfString("/ModDate", String.Format("D:{0}", [Date].ToString("yyyyMMddHHmmss")))
            Return
        End Sub

        ''' <summary>
        ''' Sets document title
        ''' </summary>
        ''' <param name="pTitle">Title</param>
        Public Sub Title(pTitle As String)
            Dictionary.AddPdfString("/Title", pTitle)
            Return
        End Sub

        ''' <summary>
        ''' Sets document author 
        ''' </summary>
        ''' <param name="pAuthor">Author</param>
        Public Sub Author(pAuthor As String)
            Dictionary.AddPdfString("/Author", pAuthor)
            Return
        End Sub

        ''' <summary>
        ''' Sets document subject
        ''' </summary>
        ''' <param name="pSubject">Subject</param>
        Public Sub Subject(pSubject As String)
            Dictionary.AddPdfString("/Subject", pSubject)
            Return
        End Sub

        ''' <summary>
        ''' Sets keywords associated with the document
        ''' </summary>
        ''' <param name="pKeywords">Keywords list</param>
        Public Sub Keywords(pKeywords As String)
            Dictionary.AddPdfString("/Keywords", pKeywords)
            Return
        End Sub

        ''' <summary>
        ''' Sets the name of the application that created the document
        ''' </summary>
        ''' <param name="pCreator">Creator</param>
        Public Sub Creator(pCreator As String)
            Dictionary.AddPdfString("/Creator", pCreator)
            Return
        End Sub

        ''' <summary>
        ''' Sets the name of the application that produced the document
        ''' </summary>
        ''' <param name="pProducer">Producer</param>
        Public Sub Producer(pProducer As String)
            Dictionary.AddPdfString("/Producer", pProducer)
            Return
        End Sub
    End Class
