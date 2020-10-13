''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
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
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

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
        Set(ByVal value As Integer)
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
        Set(ByVal value As Integer)
            If value < 1 OrElse value > 99 Then Throw New ApplicationException("PdfImageControl.GrayToBWCutoff must be 1 to 99")
            _GrayToBWCutoff = value
        End Set
    End Property

    Friend _GrayToBWCutoff As Integer
End Class

