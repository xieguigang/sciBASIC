#Region "Microsoft.VisualBasic::f94592e6618b77e47752be838268f92f, mime\application%pdf\PdfFileWriter\PDF\PdfBinaryWriter.vb"

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

    '   Total Lines: 111
    '    Code Lines: 33 (29.73%)
    ' Comment Lines: 65 (58.56%)
    '    - Xml Docs: 44.62%
    ' 
    '   Blank Lines: 13 (11.71%)
    '     File Size: 3.57 KB


    '     Class PdfBinaryWriter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: WriteFormat, (+2 Overloads) WriteString
    ' 
    ' /********************************************************************************/

#End Region

'
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
'

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
        Public Sub New(Stream As Stream)
            MyBase.New(Stream, Encoding.UTF8)
        End Sub

        ''' <summary>
        ''' Write String.
        ''' </summary>
        ''' <param name="Str">Input string</param>
        ''' <remarks>
        ''' Convert each character from two bytes to one byte.
        ''' </remarks>
        Public Sub WriteString(Str As String)
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
        Public Sub WriteString(Str As StringBuilder)
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
        Public Sub WriteFormat(FormatStr As String, ParamArray List As Object())
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
