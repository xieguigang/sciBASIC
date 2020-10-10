''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfBinaryWriter
'	Extension to standard C# BinaryWriter class.
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

Imports System.IO
Imports System.Text


    ''' <summary>
    ''' PDF binary writer class
    ''' </summary>
    ''' <remarks>
    ''' Extends .NET BinaryWriter class.
    ''' </remarks>
    Public Class PdfBinaryWriter
        Inherits BinaryWriter
        ''' <summary>
        ''' PDF binary writer constructor
        ''' </summary>
        ''' <param name="Stream">File or memory stream</param>
        Public Sub New(ByVal Stream As Stream)
            MyBase.New(Stream, Encoding.UTF8)
        End Sub

        ''' <summary>
        ''' Write String.
        ''' </summary>
        ''' <param name="Str">Input string</param>
        ''' <remarks>
        ''' Convert each character from two bytes to one byte.
        ''' </remarks>
        Public Sub WriteString(ByVal Str As String)
            ' byte array
            Dim ByteArray = New Byte(Str.Length - 1) {}

            ' convert content from string to binary
            ' do not use Encoding.ASCII.GetBytes(...)
            For Index = 0 To ByteArray.Length - 1
                ByteArray(Index) = Microsoft.VisualBasic.AscW(Str(Index))
            Next

            ' write to pdf file
            MyBase.Write(ByteArray)
            Return
        End Sub

        ''' <summary>
        ''' Write StringBuilder.
        ''' </summary>
        ''' <param name="Str">String builder input</param>
        ''' <remarks>
        ''' Convert each character from two bytes to one byte.
        ''' </remarks>
        Public Sub WriteString(ByVal Str As StringBuilder)
            ' byte array
            Dim ByteArray = New Byte(Str.Length - 1) {}

            ' convert content from string to binary
            ' do not use Encoding.ASCII.GetBytes(...)
            For Index = 0 To ByteArray.Length - 1
                ByteArray(Index) = Microsoft.VisualBasic.AscW(Str(Index))
            Next

            ' write to pdf file
            MyBase.Write(ByteArray)
            Return
        End Sub

        ''' <summary>
        ''' Combine format string with write string.
        ''' </summary>
        ''' <param name="FormatStr">Standard format string</param>
        ''' <param name="List">Array of objects</param>
        Public Sub WriteFormat(ByVal FormatStr As String, ParamArray List As Object())
            Dim Str = String.Format(FormatStr, List)

            ' byte array
            Dim ByteArray = New Byte(Str.Length - 1) {}

            ' convert content from string to binary
            ' do not use Encoding.ASCII.GetBytes(...)
            For Index = 0 To ByteArray.Length - 1
                ByteArray(Index) = Microsoft.VisualBasic.AscW(Str(Index))
            Next

            ' write to pdf file
            MyBase.Write(ByteArray)
            Return
        End Sub
    End Class

