''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
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
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

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
        Private Sub New(ByVal WebLinkStr As String)
            ' save string
            Me.WebLinkStr = WebLinkStr

            ' exit
            Return
        End Sub

        ' create new web link
        Private Sub New(ByVal Document As PdfDocument, ByVal WebLinkStr As String)
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
        Public Shared Function AddWebLink(ByVal Document As PdfDocument, ByVal WebLinkStr As String) As PdfWebLink
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
        Public Function CompareTo(ByVal Other As PdfWebLink) As Integer Implements IComparable(Of PdfWebLink).CompareTo
            Return String.Compare(WebLinkStr, Other.WebLinkStr)
        End Function
    End Class

