#Region "Microsoft.VisualBasic::792022a1b33f7114f036eb1bcb66ed3d, mime\application%pdf\PdfFileWriter\PDF\PdfBookmark.vb"

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

    '   Total Lines: 306
    '    Code Lines: 93
    ' Comment Lines: 163
    '   Blank Lines: 50
    '     File Size: 12.28 KB


    '     Class PdfBookmark
    ' 
    ' 
    '         Enum TextStyle
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+4 Overloads) AddBookmark, GetChild
    ' 
    '     Sub: WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfBookmark
'	Bookmars or document outline support.
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

Imports System.Drawing


    ''' <summary>
    ''' PDF bookmark class
    ''' </summary>
    ''' <remarks>
    ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#BookmarkSupport">2.9 Bookmark Support</a>
    ''' </remarks>
    Public Class PdfBookmark
        Inherits PdfObject

        Private OpenEntries As Boolean
        Private Parent As PdfBookmark
        Private FirstChild As PdfBookmark
        Private PrevSibling As PdfBookmark
        Private NextSibling As PdfBookmark
        Private LastChild As PdfBookmark
        Private Count As Integer

        ''' <summary>
        ''' Bookmark text style enumeration
        ''' </summary>
        Public Enum TextStyle
            ''' <summary>
            ''' Normal
            ''' </summary>
            Normal = 0

            ''' <summary>
            ''' Italic
            ''' </summary>
            Italic = 1

            ''' <summary>
            ''' Bold
            ''' </summary>
            Bold = 2

            ''' <summary>
            ''' Bold and italic
            ''' </summary>
            BoldItalic = 3
        End Enum

        
        ' Bookmarks (Document Outline) Root Constructor
        ' Must be called from PdfDocument.GetBookmarksRoot() method
        ' This constructor is called one time only
        

        Friend Sub New(Document As PdfDocument)
            MyBase.New(Document)
            ' open first level bookmarks
            OpenEntries = True

            ' add /Outlines to catalog dictionary
            Document.CatalogObject.Dictionary.AddIndirectReference("/Outlines", Me)
            Return
        End Sub

        
        ' Create bookmark item
        ' Must be called from AddBookmark method below
        ' This constructor is called for each bookmark
        

        Private Sub New(Document As PdfDocument, OpenEntries As Boolean)
            MyBase.New(Document)
            ' open first level bookmarks
            Me.OpenEntries = OpenEntries
            Return
        End Sub

        
        ''' <summary>
        ''' Add child bookmark
        ''' </summary>
        ''' <param name="Title">Bookmark title.</param>
        ''' <param name="Page">Page</param>
        ''' <param name="YPos">Vertical position.</param>
        ''' <param name="OpenEntries">Open child bookmarks attached to this one.</param>
        ''' <returns>Bookmark object</returns>
        ''' <remarks>
        ''' Add bookmark as a child to this bookmark.
        ''' This method creates a new child bookmark item attached
        ''' to this parent
        ''' </remarks>
        
        Public Function AddBookmark(Title As String, Page As PdfPage, YPos As Double, OpenEntries As Boolean) As PdfBookmark            ' bookmark title
            ' bookmark page
            ' bookmark vertical position relative to bottom left corner of the page
            ' true is display children. false hide children
            Return AddBookmark(Title, Page, 0.0, YPos, 0.0, Color.Empty, TextStyle.Normal, OpenEntries)
        End Function

        
        ''' <summary>
        ''' Add child bookmark
        ''' </summary>
        ''' <param name="Title">Bookmark title.</param>
        ''' <param name="Page">Page</param>
        ''' <param name="YPos">Vertical position.</param>
        ''' <param name="Paint">Bookmark color.</param>
        ''' <param name="TextStyle">Bookmark text style.</param>
        ''' <param name="OpenEntries">Open child bookmarks attached to this one.</param>
        ''' <returns>Bookmark object</returns>
        ''' <remarks>
        ''' Add bookmark as a child to this bookmark.
        ''' This method creates a new child bookmark item attached
        ''' to this parent
        ''' </remarks>
        
        Public Function AddBookmark(Title As String, Page As PdfPage, YPos As Double, Paint As Color, TextStyle As TextStyle, OpenEntries As Boolean) As PdfBookmark            ' bookmark title
            ' bookmark page
            ' bookmark vertical position relative to bottom left corner of the page
            ' bookmark color
            ' bookmark text style: normal, bold, italic, bold-italic
            ' true is display children. false hide children
            Return AddBookmark(Title, Page, 0.0, YPos, 0.0, Paint, TextStyle, OpenEntries)
        End Function

        
        ''' <summary>
        ''' Add child bookmark
        ''' </summary>
        ''' <param name="Title">Bookmark title.</param>
        ''' <param name="Page">Page</param>
        ''' <param name="XPos">Horizontal position</param>
        ''' <param name="YPos">Vertical position.</param>
        ''' <param name="Zoom">Zoom factor (1.0 is 100%. 0.0 is no change from existing zoom).</param>
        ''' <param name="OpenEntries">Open child bookmarks attached to this one.</param>
        ''' <returns>Bookmark object</returns>
        ''' <remarks>
        ''' Add bookmark as a child to this bookmark.
        ''' This method creates a new child bookmark item attached
        ''' to this parent
        ''' </remarks>
        
        Public Function AddBookmark(Title As String, Page As PdfPage, XPos As Double, YPos As Double, Zoom As Double, OpenEntries As Boolean) As PdfBookmark            ' bookmark title
            ' bookmark page
            ' bookmark horizontal position relative to bottom left corner of the page
            ' bookmark vertical position relative to bottom left corner of the page
            ' Zoom factor. 1.0 is 100%. 0.0 is no change from existing zoom.
            ' true is display children. false hide children
            Return AddBookmark(Title, Page, XPos, YPos, Zoom, Color.Empty, TextStyle.Normal, OpenEntries)
        End Function

        
        ''' <summary>
        ''' Add child bookmark
        ''' </summary>
        ''' <param name="Title">Bookmark title.</param>
        ''' <param name="Page">Page</param>
        ''' <param name="XPos">Horizontal position</param>
        ''' <param name="YPos">Vertical position.</param>
        ''' <param name="Zoom">Zoom factor (1.0 is 100%. 0.0 is no change from existing zoom).</param>
        ''' <param name="Paint">Bookmark color.</param>
        ''' <param name="TextStyle">Bookmark text style.</param>
        ''' <param name="OpenEntries">Open child bookmarks attached to this one.</param>
        ''' <returns>Bookmark object</returns>
        ''' <remarks>
        ''' Add bookmark as a child to this bookmark.
        ''' This method creates a new child bookmark item attached
        ''' to this parent
        ''' </remarks>
        
        Public Function AddBookmark(Title As String, Page As PdfPage, XPos As Double, YPos As Double, Zoom As Double, Paint As Color, TextStyle As TextStyle, OpenEntries As Boolean) As PdfBookmark            ' bookmark title
            ' bookmark page
            ' bookmark horizontal position relative to bottom left corner of the page
            ' bookmark vertical position relative to bottom left corner of the page
            ' Zoom factor. 1.0 is 100%. 0.0 is no change from existing zoom.
            ' bookmark color
            ' bookmark text style: normal, bold, italic, bold-italic
            ' true is display children. false hide children
            ' create new bookmark
            Dim Bookmark As PdfBookmark = New PdfBookmark(Document, OpenEntries)

            ' attach to parent
            Bookmark.Parent = Me

            ' this bookmark is first child
            If FirstChild Is Nothing Then
                FirstChild = Bookmark

                ' this bookmark is not first child
                LastChild = Bookmark
            Else
                LastChild.NextSibling = Bookmark
                Bookmark.PrevSibling = LastChild
                LastChild = Bookmark
            End If

            ' the parent of this bookmark displays all children
            If Me.OpenEntries Then
                ' update count
                Count += 1
                ' the parent of this bookmark does not display all children
                Dim TestParent = Parent

                While TestParent IsNot Nothing AndAlso TestParent.OpenEntries
                    TestParent.Count += 1
                    TestParent = TestParent.Parent
                End While
            Else
                Count -= 1
            End If

            ' build dictionary
            Bookmark.Dictionary.AddPdfString("/Title", Title)
            Bookmark.Dictionary.AddIndirectReference("/Parent", Me)
            Bookmark.Dictionary.AddFormat("/Dest", "[{0} 0 R /XYZ {1} {2} {3}]", Page.ObjectNumber, ToPt(XPos), ToPt(YPos), Round(Zoom))
            If Paint <> Color.Empty Then Bookmark.Dictionary.AddFormat("/C", "[{0} {1} {2}]", Round(Paint.R / 255.0), Round(Paint.G / 255.0), Round(Paint.B / 255.0))
            If TextStyle <> TextStyle.Normal Then Bookmark.Dictionary.AddInteger("/F", TextStyle)
            Return Bookmark
        End Function

        
        ''' <summary>
        ''' Gets child bookmark
        ''' </summary>
        ''' <param name="IndexArray">Array of indices</param>
        ''' <returns>Child bookmark or null if not found.</returns>
        ''' <remarks>
        ''' Gets PdfBookmark object based on index.
        ''' You can have multiple indices separated by commas
        ''' i.e. GetChild(2, 3);
        ''' Index is zero based. In the example we are looking first for
        ''' the third bookmark child and then the forth bookmark of the 
        ''' next level.
        ''' </remarks>
        
        Public Function GetChild(ParamArray IndexArray As Integer()) As PdfBookmark
            Dim Bookmark = Me
            Dim Child As PdfBookmark = Nothing
            Dim Level = 0

            While Level < IndexArray.Length
                ' get index number for level
                Dim Index = IndexArray(Level)

                ' find the child at this level with the given index
                Child = Bookmark.FirstChild

                While Index > 0 AndAlso Child IsNot Nothing
                    Child = Child.NextSibling
                    Index -= 1
                End While

                ' not found
                If Child Is Nothing Then Return Nothing
                Level += 1
                Bookmark = Child
            End While

            Return Child
        End Function

        
        ' Write object to PDF file
        

        Friend Overrides Sub WriteObjectToPdfFile()
            ' update dictionary
            If FirstChild IsNot Nothing Then Dictionary.AddIndirectReference("/First", FirstChild)
            If LastChild IsNot Nothing Then Dictionary.AddIndirectReference("/Last", LastChild)
            If Count <> 0 Then Dictionary.AddInteger("/Count", Count)

            ' all but root
            If Parent IsNot Nothing Then
                If PrevSibling IsNot Nothing Then Dictionary.AddIndirectReference("/Prev", PrevSibling)
                If NextSibling IsNot Nothing Then Dictionary.AddIndirectReference("/Next", NextSibling)
            End If

            ' call PdfObject base routine
            MyBase.WriteObjectToPdfFile()

            ' exit
            Return
        End Sub
    End Class
