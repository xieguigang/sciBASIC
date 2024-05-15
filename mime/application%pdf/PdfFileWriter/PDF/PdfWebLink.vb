#Region "Microsoft.VisualBasic::3b6c0198649de705418974cc84587850, mime\application%pdf\PdfFileWriter\PDF\PdfWebLink.vb"

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

    '   Total Lines: 114
    '    Code Lines: 29
    ' Comment Lines: 68
    '   Blank Lines: 17
    '     File Size: 3.79 KB


    '     Class PdfWebLink
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: AddWebLink, CompareTo
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfWebLink
'	PDF weblink class. 
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
Imports System.Collections.Generic


    ''' <summary>
    ''' PDF Weblink class
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The library will make sure that all weblinks in the PDF file are unique.
    ''' To create a weblink class you must use a static menthod. This method will
    ''' create a new object for a new weblink. The mothod will return an 
    ''' existing object if it is a duplicate.
    ''' </para>
    ''' </remarks>
    Public Class PdfWebLink
        Inherits PdfObject
        Implements IComparable(Of PdfWebLink)

        Friend WebLinkStr As String

        ' for search only
        Private Sub New(WebLinkStr As String)
            ' save string
            Me.WebLinkStr = WebLinkStr

            ' exit
            Return
        End Sub

        ' create new web link
        Private Sub New(Document As PdfDocument, WebLinkStr As String)
            MyBase.New(Document)
            ' save string
            Me.WebLinkStr = WebLinkStr

            ' type of action uniform resource identifier
            Dictionary.Add("/S", "/URI")

            ' uniform resource identifier
            Dictionary.AddPdfString("/URI", WebLinkStr)

            ' exit
            Return
        End Sub

        ''' <summary>
        ''' Add a weblink
        ''' </summary>
        ''' <param name="Document">PDF document</param>
        ''' <param name="WebLinkStr">Weblink text</param>
        ''' <returns>Weblink object</returns>
        ''' <remarks>
        ''' <para>
        ''' The library will make sure that all weblinks in the PDF file are unique.
        ''' To create a weblink class you must use a static menthod. This method will
        ''' create a new object for a new weblink. The mothod will return an 
        ''' existing object if it is a duplicate.
        ''' </para>
        ''' </remarks>
        Public Shared Function AddWebLink(Document As PdfDocument, WebLinkStr As String) As PdfWebLink
            ' first time
            If Document.WebLinkArray Is Nothing Then Document.WebLinkArray = New List(Of PdfWebLink)()

            ' search list for a duplicate
            Dim Index As Integer = Document.WebLinkArray.BinarySearch(New PdfWebLink(WebLinkStr))

            ' this string is a duplicate
            If Index >= 0 Then Return Document.WebLinkArray(Index)

            ' new link
            Dim WebLink As PdfWebLink = New PdfWebLink(Document, WebLinkStr)

            ' save new string in array
            Document.WebLinkArray.Insert(Not Index, WebLink)

            ' exit
            Return WebLink
        End Function

    ''' <summary>
    ''' Compare two WebLinkStr objects.
    ''' </summary>
    ''' <param name="Other">Other object.</param>
    ''' <returns>Compare result.</returns>
    Public Overloads Function CompareTo(Other As PdfWebLink) As Integer Implements IComparable(Of PdfWebLink).CompareTo
        Return String.Compare(WebLinkStr, Other.WebLinkStr)
    End Function
End Class
