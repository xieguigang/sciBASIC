#Region "Microsoft.VisualBasic::d36a78a4851bbc84d5a2a60cc82a95e0, mime\application%pdf\PdfFileWriter\AnnotAction\Flags.vb"

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

    '   Total Lines: 63
    '    Code Lines: 16
    ' Comment Lines: 42
    '   Blank Lines: 5
    '     File Size: 1.04 KB


    ' Enum FileAttachIcon
    ' 
    '     Graph, NoIcon, Paperclip, PushPin, Tag
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum StickyNoteIcon
    ' 
    '     Comment, Help, Insert, Key, NewParagraph
    '     Note, Paragraph
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' File attachement icon
''' </summary>
Public Enum FileAttachIcon
    ''' <summary>
    ''' Graph
    ''' </summary>
    Graph

    ''' <summary>
    ''' Paperclip
    ''' </summary>
    Paperclip

    ''' <summary>
    ''' PushPin (default)
    ''' </summary>
    PushPin

    ''' <summary>
    ''' Tag
    ''' </summary>
    Tag

    ''' <summary>
    ''' no icon 
    ''' </summary>
    NoIcon
End Enum

''' <summary>
''' Sticky note icon
''' </summary>
Public Enum StickyNoteIcon
    ''' <summary>
    ''' Comment (note: no icon)
    ''' </summary>
    Comment
    ''' <summary>
    ''' Key
    ''' </summary>
    Key
    ''' <summary>
    ''' Note (default)
    ''' </summary>
    Note
    ''' <summary>
    ''' Help
    ''' </summary>
    Help
    ''' <summary>
    ''' New paragraph
    ''' </summary>
    NewParagraph
    ''' <summary>
    ''' Paragraph
    ''' </summary>
    Paragraph
    ''' <summary>
    ''' Insert
    ''' </summary>
    Insert
End Enum
