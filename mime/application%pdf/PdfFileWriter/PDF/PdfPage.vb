#Region "Microsoft.VisualBasic::d124bd7e5e96cf56fd437a96d8593eec, mime\application%pdf\PdfFileWriter\PDF\PdfPage.vb"

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

    '   Total Lines: 390
    '    Code Lines: 136 (34.87%)
    ' Comment Lines: 192 (49.23%)
    '    - Xml Docs: 73.96%
    ' 
    '   Blank Lines: 62 (15.90%)
    '     File Size: 16.05 KB


    '     Class PdfPage
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: AddAnnotation, (+2 Overloads) AddFileAttachment, AddLinkAction, AddScreenAction, AddStickyNote
    '                   (+2 Overloads) AddWebLink, GetCurrentContents, PageSize
    ' 
    '         Sub: AddAnnotInternal, AddContents, AddLocationMarker, ConstructorHelper, WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfPage
'	PDF page class. An indirect PDF object.
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

Imports System.Collections.Generic
Imports System.Text


    ''' <summary>
    ''' PDF page class
    ''' </summary>
    ''' <remarks>
    ''' PDF page class represent one page in the document.
    ''' </remarks>
    Public Class PdfPage
        Inherits PdfObject

        Friend Width As Double      ' in points
        Friend Height As Double     ' in points
        Friend ContentsArray As List(Of PdfContents)

        
        ''' <summary>
        ''' Default constructor
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <remarks>
        ''' Page size is taken from PdfDocument
        ''' </remarks>
        
        Public Sub New(Document As PdfDocument)
            MyBase.New(Document, ObjectType.Dictionary, "/Page")
            Width = Document.PageSize.Width
            Height = Document.PageSize.Height
            ConstructorHelper()
            Return
        End Sub

        
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <param name="PageSize">Paper size for this page</param>
        ''' <remarks>
        ''' PageSize override the default page size
        ''' </remarks>
        

        Public Sub New(Document As PdfDocument, PageSize As SizeD)
            MyBase.New(Document, ObjectType.Dictionary, "/Page")
            Width = ScaleFactor * PageSize.Width
            Height = ScaleFactor * PageSize.Height
            ConstructorHelper()
            Return
        End Sub

        
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <param name="PaperType">Paper type</param>
        ''' <param name="Landscape">If Lanscape is true, width and height are swapped.</param>
        ''' <remarks>
        ''' PaperType and orientation override the default page size.
        ''' </remarks>
        
        Public Sub New(Document As PdfDocument, PaperType As PaperType, Landscape As Boolean)
            MyBase.New(Document, ObjectType.Dictionary, "/Page")
            ' get standard paper size
            Width = PdfDocument.PaperTypeSize(PaperType).Width
            Height = PdfDocument.PaperTypeSize(PaperType).Height

            ' for landscape swap width and height
            If Landscape Then
                Dim Temp = Width
                Width = Height
                Height = Temp
            End If

            ' exit
            ConstructorHelper()
            Return
        End Sub

        
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="Document">Parent PDF document object</param>
        ''' <param name="Width">Page width</param>
        ''' <param name="Height">Page height</param>
        ''' <remarks>
        ''' Width and Height override the default page size
        ''' </remarks>
        
        Public Sub New(Document As PdfDocument, Width As Double, Height As Double)
            MyBase.New(Document, ObjectType.Dictionary, "/Page")
            Me.Width = ScaleFactor * Width
            Me.Height = ScaleFactor * Height
            ConstructorHelper()
            Return
        End Sub

        ''' <summary>
        ''' Clone Constructor
        ''' </summary>
        ''' <param name="Page">Existing page object</param>
        Public Sub New(Page As PdfPage)
            MyBase.New(Page.Document, ObjectType.Dictionary, "/Page")
            Width = Page.Width
            Height = Page.Height
            ConstructorHelper()
            Return
        End Sub

        
        ' Constructor common method
        

        Private Sub ConstructorHelper()
            ' add page to parent array of pages
            Document.PageArray.Add(Me)

            ' link page to parent
            Dictionary.AddIndirectReference("/Parent", Document.PagesObject)

            ' add page size in points
            Dictionary.AddFormat("/MediaBox", "[0 0 {0} {1}]", Round(Width), Round(Height))

            ' exit
            Return
        End Sub

        ''' <summary>
        ''' Page size
        ''' </summary>
        ''' <returns>Page size</returns>
        ''' <remarks>Page size in user units of measure. If Width is less than height
        ''' orientation is portrait. Otherwise orientation is landscape.</remarks>
        Public Function PageSize() As SizeD
            Return New SizeD(Width / ScaleFactor, Height / ScaleFactor)
        End Function

        
        ''' <summary>
        ''' Add existing contents to page
        ''' </summary>
        ''' <param name="Contents">Contents object</param>
        
        Public Sub AddContents(Contents As PdfContents)
            ' set page contents flag
            Contents.PageContents = True

            ' add content to content array
            If ContentsArray Is Nothing Then ContentsArray = New List(Of PdfContents)()
            ContentsArray.Add(Contents)

            ' exit
            Return
        End Sub

        ''' <summary>
        ''' Gets the current contents of this page
        ''' </summary>
        ''' <returns>Page's current contents</returns>
        Public Function GetCurrentContents() As PdfContents
            Return If(ContentsArray Is Nothing OrElse ContentsArray.Count = 0, Nothing, ContentsArray(ContentsArray.Count - 1))
        End Function

        ''' <summary>
        ''' Add annotation action
        ''' </summary>
        ''' <param name="AnnotRect">Annotation rectangle</param>
        ''' <param name="AnnotAction">Annotation action derived class</param>
        ''' <returns>PdfAnnotation object</returns>
        Public Function AddAnnotation(AnnotRect As PdfRectangle, AnnotAction As AnnotAction) As PdfAnnotation
            If AnnotAction.GetType() Is GetType(AnnotLinkAction) Then
                Return AddLinkAction(CType(AnnotAction, AnnotLinkAction).LocMarkerName, AnnotRect)
            End If

            Return New PdfAnnotation(Me, AnnotRect, AnnotAction)
        End Function

        Friend Sub AddAnnotInternal(AnnotRect As PdfRectangle, AnnotAction As AnnotAction)
            If AnnotAction.GetType() Is GetType(AnnotLinkAction) Then
                AddLinkAction(CType(AnnotAction, AnnotLinkAction).LocMarkerName, AnnotRect)
            Else
                If AnnotAction.GetType() Is GetType(AnnotFileAttachment) Then CType(AnnotAction, AnnotFileAttachment).Icon = FileAttachIcon.NoIcon
                Dim null As New PdfAnnotation(Me, AnnotRect, AnnotAction)
            End If

            Return
        End Sub

        
        ''' <summary>
        ''' Add weblink to this page
        ''' </summary>
        ''' <param name="LeftAbsPos">Left position of weblink area</param>
        ''' <param name="BottomAbsPos">Bottom position of weblink area</param>
        ''' <param name="RightAbsPos">Right position of weblink area</param>
        ''' <param name="TopAbsPos">Top position of weblink area</param>
        ''' <param name="WebLinkStr">Hyperlink string</param>
        ''' <returns>PdfAnnotation object</returns>
        ''' <remarks>
        ''' <para>
        ''' 	The four position arguments are in relation to the
        ''' 	bottom left corner of the paper.
        ''' 	If web link is empty, ignore this call.
        ''' </para>
        ''' <para>
        ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#WeblinkSupport">2.7 Web Link Support</a>
        ''' </para>
        ''' </remarks>
        
        Public Function AddWebLink(LeftAbsPos As Double, BottomAbsPos As Double, RightAbsPos As Double, TopAbsPos As Double, WebLinkStr As String) As PdfAnnotation
            If String.IsNullOrWhiteSpace(WebLinkStr) Then Return Nothing
            Return AddWebLink(New PdfRectangle(LeftAbsPos, BottomAbsPos, RightAbsPos, TopAbsPos), WebLinkStr)
        End Function

        
        ''' <summary>
        ''' Add weblink to this page
        ''' </summary>
        ''' <param name="AnnotRect">Weblink area</param>
        ''' <param name="WebLinkStr">Hyperlink string</param>
        ''' <returns>PdfAnnotation object</returns>
        ''' <remarks>
        ''' <para>
        ''' 	The four position arguments are in relation to the
        ''' 	bottom left corner of the paper.
        ''' 	If web link is empty, ignore this call.
        ''' </para>
        ''' <para>
        ''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#WeblinkSupport">2.7 Web Link Support</a>
        ''' </para>
        ''' </remarks>
        
        Public Function AddWebLink(AnnotRect As PdfRectangle, WebLinkStr As String) As PdfAnnotation
            If String.IsNullOrWhiteSpace(WebLinkStr) Then Return Nothing
            Return New PdfAnnotation(Me, AnnotRect, New AnnotWebLink(WebLinkStr))
        End Function

        ''' <summary>
        ''' Add location marker
        ''' </summary>
        ''' <param name="LocMarkerName">Location marker name</param>
        ''' <param name="Scope">Location marker scope</param>
        ''' <param name="FitArg">PDF reader display control</param>
        ''' <param name="SideArg">Optional dimensions for FitArg control</param>
        Public Sub AddLocationMarker(LocMarkerName As String, Scope As LocMarkerScope, FitArg As DestFit, ParamArray SideArg As Double())
            LocationMarker.Create(LocMarkerName, Me, Scope, FitArg, SideArg)
            Return
        End Sub

        ''' <summary>
        ''' Add go to action
        ''' </summary>
        ''' <param name="LocMarkerName">Destination name</param>
        ''' <param name="AnnotRect">Annotation rectangle</param>
        ''' <returns>PdfAnnotation object</returns>
        Public Function AddLinkAction(LocMarkerName As String, AnnotRect As PdfRectangle) As PdfAnnotation
            Return New PdfAnnotation(Me, AnnotRect, New AnnotLinkAction(LocMarkerName))
        End Function

        ''' <summary>
        ''' Add rendering screen action to page
        ''' </summary>
        ''' <param name="AnnotRect">Annotation rectangle</param>
        ''' <param name="DisplayMedia">Display media object</param>
        ''' <returns>PdfAnnotation</returns>
        Public Function AddScreenAction(AnnotRect As PdfRectangle, DisplayMedia As PdfDisplayMedia) As PdfAnnotation
            Return New PdfAnnotation(Me, AnnotRect, New AnnotDisplayMedia(DisplayMedia))
        End Function

        ''' <summary>
        ''' Add annotation file attachement with icon
        ''' </summary>
        ''' <param name="AnnotRect">Annotation rectangle</param>
        ''' <param name="EmbeddedFile">Embedded file</param>
        ''' <param name="Icon">Icon</param>
        ''' <returns>PdfAnnotation</returns>
        ''' <remarks>The AnnotRect is the icon rectangle area. To access the file
        ''' the user has to right click on the icon.</remarks>
        Public Function AddFileAttachment(AnnotRect As PdfRectangle, EmbeddedFile As PdfEmbeddedFile, Icon As FileAttachIcon) As PdfAnnotation
            Return New PdfAnnotation(Me, AnnotRect, New AnnotFileAttachment(EmbeddedFile, Icon))
        End Function

        ''' <summary>
        ''' Add annotation file attachement with no icon
        ''' </summary>
        ''' <param name="AnnotRect">Annotation rectangle</param>
        ''' <param name="EmbeddedFile">Embedded file</param>
        ''' <returns>PdfAnnotation</returns>
        ''' <remarks>The AnnotRect is any area on the page. When the user right click this
        ''' area a floating menu will be displayed.</remarks>
        Public Function AddFileAttachment(AnnotRect As PdfRectangle, EmbeddedFile As PdfEmbeddedFile) As PdfAnnotation
            Return New PdfAnnotation(Me, AnnotRect, New AnnotFileAttachment(EmbeddedFile, FileAttachIcon.NoIcon))
        End Function

        ''' <summary>
        ''' Add sticky note to this page
        ''' </summary>
        ''' <param name="AbsLeft">Icon page absolute left position</param>
        ''' <param name="AbsTop">Icon page absolute top position</param>
        ''' <param name="Note">Sticky note text string</param>
        ''' <param name="Icon">Sticky note icon enumeration</param>
        ''' <returns>PdfAnnotation</returns>
        Public Function AddStickyNote(AbsLeft As Double, AbsTop As Double, Note As String, Icon As StickyNoteIcon) As PdfAnnotation
            Return New PdfAnnotation(Me, New PdfRectangle(AbsLeft, AbsTop, AbsLeft, AbsTop), New AnnotStickyNote(Note, Icon))
        End Function

        
        ' Write object to PDF file
        

        Friend Overrides Sub WriteObjectToPdfFile()
            ' we have at least one contents object
            If ContentsArray IsNot Nothing Then
                ' page has one contents object
                If ContentsArray.Count = 1 Then
                    Dictionary.AddFormat("/Contents", "[{0} 0 R]", ContentsArray(0).ObjectNumber)

                    ' page is made of multiple contents
                    Dictionary.Add("/Resources", BuildResourcesDictionary(ContentsArray(0).ResObjects, True))
                Else
                    ' contents dictionary entry
                    Dim ContentsStr As StringBuilder = New StringBuilder("[")

                    ' build contents dictionary entry
                    For Each Contents In ContentsArray
                        ContentsStr.AppendFormat("{0} 0 R ", Contents.ObjectNumber)
                    Next

                    ' add terminating bracket
                    ContentsStr.Length -= 1
                    ContentsStr.Append("]"c)
                    Dictionary.Add("/Contents", ContentsStr.ToString())

                    ' resources array of all contents objects
                    Dim ResObjects As List(Of PdfObject) = New List(Of PdfObject)()

                    ' loop for all contents objects
                    For Each Contents In ContentsArray
                        ' make sure we have resources
                        If Contents.ResObjects IsNot Nothing Then
                            ' loop for resources within this contents object
                            For Each ResObject In Contents.ResObjects
                                ' check if we have it already
                                Dim Ptr = ResObjects.BinarySearch(ResObject)
                                If Ptr < 0 Then ResObjects.Insert(Not Ptr, ResObject)
                            Next
                        End If
                    Next

                    ' save to dictionary
                    Dictionary.Add("/Resources", BuildResourcesDictionary(ResObjects, True))
                End If
            End If

            ' call PdfObject routine
            MyBase.WriteObjectToPdfFile()

            ' exit
            Return
        End Sub
    End Class
