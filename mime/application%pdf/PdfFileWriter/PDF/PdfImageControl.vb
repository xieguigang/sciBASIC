#Region "Microsoft.VisualBasic::125ecbec32618a3ee94bd43327504ff4, mime\application%pdf\PdfFileWriter\PDF\PdfImageControl.vb"

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

    '   Total Lines: 88
    '    Code Lines: 52 (59.09%)
    ' Comment Lines: 28 (31.82%)
    '    - Xml Docs: 10.71%
    ' 
    '   Blank Lines: 8 (9.09%)
    '     File Size: 2.82 KB


    ' Class PdfImageControl
    ' 
    '     Properties: GrayToBWCutoff, ImageQuality
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfImageControl
'	PDF Image control.
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
Imports System.Drawing


''' <summary>
''' PdfImageControl is obolete. See latest documentation
''' </summary>
Public Class PdfImageControl
    Private Const ObsoleteError As Boolean = False
    Private Const ObsoleteMsg As String = "This PdfImageControl class is obsolete. See latest documentation."
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public CropRect As Rectangle
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public CropPercent As RectangleF
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public ReverseBW As Boolean
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Resolution As Double
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public SaveAs As SaveImageAs
    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Const DefaultQuality As Integer = -1

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New()
        CropRect = Rectangle.Empty
        CropPercent = RectangleF.Empty
        ReverseBW = False
        _GrayToBWCutoff = 50
        Resolution = 0.0
        _ImageQuality = DefaultQuality
        SaveAs = SaveImageAs.Jpeg
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Property ImageQuality As Integer
        Get
            Return _ImageQuality
        End Get
        Set(value As Integer)
            ' set image quality
            If value <> DefaultQuality AndAlso (value < 0 OrElse value > 100) Then Throw New ApplicationException("PdfImageControl.ImageQuality must be DefaultQuality or 0 to 100")
            _ImageQuality = value
            Return
        End Set
    End Property

    Friend _ImageQuality As Integer

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Property GrayToBWCutoff As Integer
        Get
            Return _GrayToBWCutoff
        End Get
        Set(value As Integer)
            If value < 1 OrElse value > 99 Then Throw New ApplicationException("PdfImageControl.GrayToBWCutoff must be 1 to 99")
            _GrayToBWCutoff = value
        End Set
    End Property

    Friend _GrayToBWCutoff As Integer
End Class
