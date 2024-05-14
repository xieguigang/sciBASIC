#Region "Microsoft.VisualBasic::26d75acd019a033c7ceb6f004c067f56, mime\application%pdf\PdfFileWriter\PDF\PdfDisplayMedia.vb"

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

    '   Total Lines: 473
    '    Code Lines: 143
    ' Comment Lines: 260
    '   Blank Lines: 70
    '     File Size: 13.15 KB


    ' Enum TempFilePermission
    ' 
    '     TEMPACCESS, TEMPALWAYS, TEMPEXTRACT, TEMPNEVER
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum MediaWindow
    ' 
    '     Annotation, Floating, FullScreen, Hidden
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum WindowPosition
    ' 
    '     Center, CenterLeft, CenterRight, LowerCenter, LowerLeft
    '     LowerRight, UpperCenter, UpperLeft, UpperRight
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum WindowTitleBar
    ' 
    '     NoTitleBar, TitleBar, TitleBarWithCloseButton
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum WindowResize
    ' 
    '     KeepAspectRatio, NoAspectRatio, NoResize
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum MediaOperation
    ' 
    '     [Resume], [Stop], Pause, Play, PlayAfterPause
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum ScaleMediaCode
    ' 
    '     FillAnotationRect, KeepAspectRatioShowAll, KeepAspectRatioSlice, NoScaleSlice, NoScaleWithScroll
    '     PlayerDefault
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfDisplayMedia
    ' 
    '     Properties: MediaFile
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: DisplayControls, InitialMediaOperation, MediaTempFilePermission, RepeatCount, ScaleMedia
    '          SetMediaWindow
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfDisplayMedia
'	PDF display media class. 
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


''' <summary>
''' Temporary file permission enumeration
''' </summary>
Public Enum TempFilePermission
    ''' <summary>
    ''' Never allow PDF reader to write temporary file.
    ''' </summary>
    TEMPNEVER

    ''' <summary>
    ''' Allow PDF reader to write temporary file based on extract permission flag.
    ''' </summary>
    TEMPEXTRACT

    ''' <summary>
    ''' Allow PDF reader to write temporary file based on access permission flag.
    ''' </summary>
    TEMPACCESS

    ''' <summary>
    ''' Always allow PDF reader to write temporary file.
    ''' </summary>
    TEMPALWAYS
End Enum

''' <summary>
''' Media window position
''' </summary>
Public Enum MediaWindow
    ''' <summary>
    ''' Floating window
    ''' </summary>
    Floating

    ''' <summary>
    ''' Full screen
    ''' </summary>
    FullScreen

    ''' <summary>
    ''' Hidden
    ''' </summary>
    Hidden

    ''' <summary>
    ''' Annotation rectangle
    ''' </summary>
    Annotation
End Enum

''' <summary>
''' Media image position within window
''' </summary>
Public Enum WindowPosition
    ''' <summary>
    ''' Upper left
    ''' </summary>
    UpperLeft

    ''' <summary>
    ''' Upper center
    ''' </summary>
    UpperCenter

    ''' <summary>
    ''' Upper right
    ''' </summary>
    UpperRight

    ''' <summary>
    ''' Center left
    ''' </summary>
    CenterLeft

    ''' <summary>
    ''' Center
    ''' </summary>
    Center

    ''' <summary>
    ''' Center right
    ''' </summary>
    CenterRight

    ''' <summary>
    ''' Lower left
    ''' </summary>
    LowerLeft

    ''' <summary>
    ''' lower center
    ''' </summary>
    LowerCenter

    ''' <summary>
    ''' Lower right
    ''' </summary>
    LowerRight
End Enum

''' <summary>
''' Floating window title bar
''' </summary>
Public Enum WindowTitleBar
    ''' <summary>
    ''' No title bar
    ''' </summary>
    NoTitleBar

    ''' <summary>
    ''' Window has title bar
    ''' </summary>
    TitleBar

    ''' <summary>
    ''' Window has title bar with close button
    ''' </summary>
    TitleBarWithCloseButton
End Enum

''' <summary>
''' Floating window resize options
''' </summary>
Public Enum WindowResize
    ''' <summary>
    ''' No resize
    ''' </summary>
    NoResize

    ''' <summary>
    ''' Resize with correct aspect ratio
    ''' </summary>
    KeepAspectRatio

    ''' <summary>
    ''' Resize without aspect ratio
    ''' </summary>
    NoAspectRatio
End Enum

''' <summary>
''' Media operation code
''' </summary>
''' <remarks>
''' <para>
''' Operation to perform when rendition action is triggered.
''' Page 669 T 8.64 S 8.5
''' </para>
''' </remarks>
Public Enum MediaOperation
    ''' <summary>
    ''' Play
    ''' </summary>
    Play

    ''' <summary>
    ''' Stop
    ''' </summary>
    [Stop]

    ''' <summary>
    ''' Pause
    ''' </summary>
    Pause

    ''' <summary>
    ''' Resume
    ''' </summary>
    [Resume]

    ''' <summary>
    ''' Play after pause
    ''' </summary>
    PlayAfterPause
End Enum

''' <summary>
''' Scale media code
''' </summary>
''' <remarks>
''' <para>
''' Value 0 to 5 How to scale the media to fit annotation area page 770 T 9.15
''' </para>
''' </remarks>
Public Enum ScaleMediaCode
    ''' <summary>
    ''' Keep aspect ratio and show all.
    ''' </summary>
    KeepAspectRatioShowAll

    ''' <summary>
    ''' Keep aspect ratio fit the one side and slice the other
    ''' </summary>
    KeepAspectRatioSlice

    ''' <summary>
    ''' Ignore aspect ratio and fill annotation rectangle
    ''' </summary>
    FillAnotationRect

    ''' <summary>
    ''' No scaling. Provide scroll if required
    ''' </summary>
    NoScaleWithScroll

    ''' <summary>
    ''' No scaling. Show what fits
    ''' </summary>
    NoScaleSlice

    ''' <summary>
    ''' Let media player handle it
    ''' </summary>
    PlayerDefault
End Enum

''' <summary>
''' PDF Screen annotation
''' </summary>
Public Class PdfDisplayMedia
    Inherits PdfObject
    ''' <summary>
    ''' Gets embedded media file class
    ''' </summary>
    Private _MediaFile As PdfEmbeddedFile

    Public Property MediaFile As PdfEmbeddedFile
        Get
            Return _MediaFile
        End Get
        Private Set(value As PdfEmbeddedFile)
            _MediaFile = value
        End Set
    End Property

    Private Rendition As PdfDictionary
    Private MediaClip As PdfDictionary
    Private MediaPlay As PdfDictionary
    Private MediaPlayBE As PdfDictionary
    Private MediaScreenParam As PdfDictionary
    Private MediaScreenParamBE As PdfDictionary
    Private TempFilePermissions As PdfDictionary

    ''' <summary>
    ''' Display media constructor
    ''' </summary>
    ''' <param name="MediaFile">Embedded media file</param>
    ''' <param name="MimeType">Mime type</param>
    ''' <remarks>
    ''' <para>
    ''' If mime type is null the program will try to convert file extension
    ''' to mime type. If conversion is not available application exception will be raised.
    ''' </para>
    ''' </remarks>
    Public Sub New(MediaFile As PdfEmbeddedFile, Optional MimeType As String = Nothing)
        MyBase.New(MediaFile.Document)
        ' save media file
        Me.MediaFile = MediaFile

        ' save mimetype
        If Equals(MimeType, Nothing) Then MimeType = MediaFile.MimeType
        If String.IsNullOrWhiteSpace(MimeType) Then Throw New ApplicationException("MIME type is not defined")

        ' rendition dictionary page 759 Section 9.1.2 Table 9.1
        Rendition = New PdfDictionary(Me)
        Dictionary.AddDictionary("/R", Rendition)

        ' media clip
        MediaClip = New PdfDictionary(Me)
        Rendition.AddDictionary("/C", MediaClip)

        ' Media clip dictionary T 9.9
        TempFilePermissions = New PdfDictionary(Me)
        MediaClip.AddDictionary("/P", TempFilePermissions)

        ' media play
        MediaPlay = New PdfDictionary(Me)
        Rendition.AddDictionary("/P", MediaPlay)

        ' media play BE
        MediaPlayBE = New PdfDictionary(Me)
        MediaPlay.AddDictionary("/BE", MediaPlayBE)

        ' media screen parameters
        MediaScreenParam = New PdfDictionary(Me)
        Rendition.AddDictionary("/SP", MediaScreenParam)

        ' media screen parameters BE
        MediaScreenParamBE = New PdfDictionary(Me)
        MediaScreenParam.AddDictionary("/BE", MediaScreenParamBE)

        ' Section 8.5 page 669 table 8.64
        ' type of action playing multimedia content
        Dictionary.Add("/S", "/Rendition")

        ' media clip data page 762
        Rendition.Add("/S", "/MR")

        ' Table 9.6 page 762
        MediaClip.AddPdfString("/CT", MimeType)
        MediaClip.AddIndirectReference("/D", MediaFile)
        MediaClip.Add("/S", "/MCD")
        MediaClip.Add("/Type", "/MediaClip")

        ' Operation to perform when action is triggered. Valid options are 0 or 4
        ' OP=0 force the Rendition dictionary to take over the annotation
        Dictionary.Add("/OP", "0")

        ' allow reader to always create temporary file (other options do not work)
        ' Media clip dictionary T 9.10 page 766
        TempFilePermissions.AddPdfString("/TF", "TEMPALWAYS")

        ' do not display control
        MediaPlayBE.AddBoolean("/C", False)

        ' repeat count of 1
        MediaPlayBE.Add("/RC", "1.0")

        ' media scale and position within annotation rectangle PDF default is 5
        ' /F=2 strech media to fit annotation
        MediaPlayBE.Add("/F", "2")

        ' play rendition in annotation rectangle
        MediaScreenParamBE.Add("/W", "3")

        ' exit
        Return
    End Sub

    ''' <summary>
    ''' Display media player controls
    ''' </summary>
    ''' <param name="Display">Display/no display command</param>
    Public Sub DisplayControls(Display As Boolean)
        MediaPlayBE.AddBoolean("/C", Display)
        Return
    End Sub

    ''' <summary>
    ''' Repeat count
    ''' </summary>
    ''' <param name="Count">Count</param>
    ''' <remarks>
    ''' 	<para>
    ''' 	Count of zero means replay indefinitly.
    ''' 	</para>
    ''' 	<para>
    ''' 	Negative count is an error.
    ''' 	</para>
    ''' 	<para>
    ''' 	Count is a real (float) number. The PDF specification does not
    ''' 	define how non integers are treated.
    ''' 	</para>
    ''' </remarks>
    Public Sub RepeatCount(Count As Single)
        MediaPlayBE.AddReal("/RC", Count)
        Return
    End Sub

    ''' <summary>
    ''' Set media window
    ''' </summary>
    ''' <param name="MediaWindow">Media window</param>
    ''' <param name="Width">Floating window width</param>
    ''' <param name="Height">Floating window height</param>
    ''' <param name="Position">Floating window position</param>
    ''' <param name="TitleBar">Floating window title bar</param>
    ''' <param name="Resize">Floating window resize</param>
    ''' <param name="Title">Floating window title</param>
    ''' <remarks>
    ''' <para>
    ''' All optional arguments are applicable to floating window only.
    ''' </para>
    ''' </remarks>
    Public Sub SetMediaWindow(MediaWindow As MediaWindow, Optional Width As Integer = 0, Optional Height As Integer = 0, Optional Position As WindowPosition = WindowPosition.Center, Optional TitleBar As WindowTitleBar = WindowTitleBar.TitleBarWithCloseButton, Optional Resize As WindowResize = WindowResize.KeepAspectRatio, Optional Title As String = Nothing)
        ' set media play window code
        MediaScreenParamBE.AddInteger("/W", MediaWindow)

        ' all choices but floating window
        If MediaWindow <> MediaWindow.Floating Then
            MediaScreenParamBE.Remove("/F")
            Return
        End If

        ' play rendition in floating window
        ' Table 9.19 page 774
        Dim FloatingWindow As PdfDictionary = New PdfDictionary(Me)
        MediaScreenParamBE.AddDictionary("/F", FloatingWindow)

        ' window's dimensions
        If Width = 0 OrElse Height = 0 Then
            Width = 320
            Height = 180
        End If

        FloatingWindow.AddFormat("/D", "[{0} {1}]", Width, Height)
        FloatingWindow.AddInteger("/P", Position)
        FloatingWindow.AddBoolean("/T", TitleBar <> WindowTitleBar.NoTitleBar)
        If TitleBar = WindowTitleBar.NoTitleBar Then Return
        FloatingWindow.AddInteger("/R", Resize)

        If Not Equals(Title, Nothing) Then
            FloatingWindow.AddFormat("/TT", "[{0} {1}]", Document.TextToPdfString(String.Empty, Me), Document.TextToPdfString(Title, Me))
        End If

        Return
    End Sub

    ''' <summary>
    ''' Scale media
    ''' </summary>
    ''' <param name="ScaleCode">Scale media code</param>
    Public Sub ScaleMedia(ScaleCode As ScaleMediaCode)
        ' media scale and position within annotation rectangle
        ' Value 0 to 5 How to scale the media to fit annotation area page 770 T 9.15
        MediaPlayBE.AddInteger("/F", ScaleCode)
        Return
    End Sub

    ''' <summary>
    ''' Initial media operation
    ''' </summary>
    ''' <param name="OperationCode">Media operation code enumeration</param>
    Public Sub InitialMediaOperation(OperationCode As MediaOperation)
        ' Operation to perform when rendition action is triggered.
        ' Page 669 T 8.64 S 8.5
        Dictionary.AddInteger("/OP", OperationCode)
        Return
    End Sub

    ''' <summary>
    ''' Media temporary file permission
    ''' </summary>
    ''' <param name="Permission">Permissions flags</param>
    ''' <remarks><para>
    ''' The PDF reader must save the media file to a temporary file
    ''' in order for the player to play it.
    ''' </para></remarks>
    Public Sub MediaTempFilePermission(Permission As TempFilePermission)
        ' allow reader to always create temporary file (other options do not work)
        ' Media clip dictionary T 9.10 page 766
        TempFilePermissions.AddPdfString("/TF", Permission.ToString())
        Return
    End Sub
End Class
