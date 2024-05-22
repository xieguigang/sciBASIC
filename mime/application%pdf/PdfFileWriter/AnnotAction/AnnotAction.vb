#Region "Microsoft.VisualBasic::079bab98795d37c6f6bbea5809d57d99, mime\application%pdf\PdfFileWriter\AnnotAction\AnnotAction.vb"

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

    '   Total Lines: 170
    '    Code Lines: 72 (42.35%)
    ' Comment Lines: 80 (47.06%)
    '    - Xml Docs: 70.00%
    ' 
    '   Blank Lines: 18 (10.59%)
    '     File Size: 5.10 KB


    ' Class AnnotWebLink
    ' 
    '     Properties: WebLinkStr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: IsEqual
    ' 
    ' Class AnnotLinkAction
    ' 
    '     Properties: LocMarkerName
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: IsEqual
    ' 
    ' Class AnnotDisplayMedia
    ' 
    '     Properties: DisplayMedia
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: IsEqual
    ' 
    ' Class AnnotFileAttachment
    ' 
    '     Properties: EmbeddedFile
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: IsEqual
    ' 
    ' Class AnnotStickyNote
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: IsEqual
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	AnnotAction
'	Annotation action classes 
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
''' Web link annotation action
''' </summary>
Public Class AnnotWebLink
    Inherits AnnotAction
    ''' <summary>
    ''' Gets or sets web link string
    ''' </summary>
    Public Property WebLinkStr As String

    ''' <summary>
    ''' Web link constructor
    ''' </summary>
    ''' <param name="WebLinkStr">Web link string</param>
    Public Sub New(WebLinkStr As String)
        MyBase.New("/Link")
        Me.WebLinkStr = WebLinkStr
        Return
    End Sub

    Friend Overrides Function IsEqual(Other As AnnotAction) As Boolean
        Return Equals(WebLinkStr, CType(Other, AnnotWebLink).WebLinkStr)
    End Function
End Class

''' <summary>
''' Link to location marker within the document
''' </summary>
Public Class AnnotLinkAction
    Inherits AnnotAction
    ''' <summary>
    ''' Gets or sets the location marker name
    ''' </summary>
    Public Property LocMarkerName As String

    ''' <summary>
    ''' Go to annotation action constructor
    ''' </summary>
    ''' <param name="LocMarkerName">Location marker name</param>
    Public Sub New(LocMarkerName As String)
        MyBase.New("/Link")
        Me.LocMarkerName = LocMarkerName
        Return
    End Sub

    Friend Overrides Function IsEqual(Other As AnnotAction) As Boolean
        Return Equals(LocMarkerName, CType(Other, AnnotLinkAction).LocMarkerName)
    End Function
End Class

''' <summary>
''' Display video or play sound class
''' </summary>
Public Class AnnotDisplayMedia
    Inherits AnnotAction
    ''' <summary>
    ''' Gets or sets PdfDisplayMedia object
    ''' </summary>
    Public Property DisplayMedia As PdfDisplayMedia

    ''' <summary>
    ''' Display media annotation action constructor
    ''' </summary>
    ''' <param name="DisplayMedia">PdfDisplayMedia</param>
    Public Sub New(DisplayMedia As PdfDisplayMedia)
        MyBase.New("/Screen")
        Me.DisplayMedia = DisplayMedia
        Return
    End Sub

    Friend Overrides Function IsEqual(Other As AnnotAction) As Boolean
        Return Equals(DisplayMedia.MediaFile.FileName, CType(Other, AnnotDisplayMedia).DisplayMedia.MediaFile.FileName)
    End Function
End Class

''' <summary>
''' Save or view embedded file
''' </summary>
Public Class AnnotFileAttachment
    Inherits AnnotAction
    ''' <summary>
    ''' Gets or sets embedded file
    ''' </summary>
    Public Property EmbeddedFile As PdfEmbeddedFile

    ''' <summary>
    ''' Gets or sets associated icon
    ''' </summary>
    Public Icon As FileAttachIcon

    ''' <summary>
    ''' File attachement constructor
    ''' </summary>
    ''' <param name="EmbeddedFile">Embedded file</param>
    ''' <param name="Icon">Icon enumeration</param>
    Public Sub New(EmbeddedFile As PdfEmbeddedFile, Icon As FileAttachIcon)
        MyBase.New("/FileAttachment")
        Me.EmbeddedFile = EmbeddedFile
        Me.Icon = Icon
        Return
    End Sub

    ''' <summary>
    ''' File attachement constructor (no icon)
    ''' </summary>
    ''' <param name="EmbeddedFile">Embedded file</param>
    Public Sub New(EmbeddedFile As PdfEmbeddedFile)
        MyBase.New("/FileAttachment")
        Me.EmbeddedFile = EmbeddedFile
        Icon = FileAttachIcon.NoIcon
        Return
    End Sub

    Friend Overrides Function IsEqual(Other As AnnotAction) As Boolean
        Dim FileAttach = CType(Other, AnnotFileAttachment)
        Return Equals(EmbeddedFile.FileName, FileAttach.EmbeddedFile.FileName) AndAlso Icon = FileAttach.Icon
    End Function
End Class

''' <summary>
''' Display sticky note
''' </summary>
Public Class AnnotStickyNote
    Inherits AnnotAction

    Friend Note As String
    Friend Icon As StickyNoteIcon

    ''' <summary>
    ''' Sticky note annotation action constructor
    ''' </summary>
    ''' <param name="Note">Sticky note text</param>
    ''' <param name="Icon">Sticky note icon</param>
    Public Sub New(Note As String, Icon As StickyNoteIcon)
        MyBase.New("/Text")
        Me.Note = Note
        Me.Icon = Icon
        Return
    End Sub

    Friend Overrides Function IsEqual(Other As AnnotAction) As Boolean
        Dim StickyNote = CType(Other, AnnotStickyNote)
        Return Equals(Note, StickyNote.Note) AndAlso Icon = StickyNote.Icon
    End Function
End Class
