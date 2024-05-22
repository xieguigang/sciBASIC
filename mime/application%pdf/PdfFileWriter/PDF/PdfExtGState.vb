#Region "Microsoft.VisualBasic::cee8aabef390313427e2c811e2b681a4, mime\application%pdf\PdfFileWriter\PDF\PdfExtGState.vb"

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

    '   Total Lines: 91
    '    Code Lines: 34 (37.36%)
    ' Comment Lines: 41 (45.05%)
    '    - Xml Docs: 12.20%
    ' 
    '   Blank Lines: 16 (17.58%)
    '     File Size: 2.93 KB


    '     Class PdfExtGState
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CompareTo, CreateExtGState
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfExtGState
'	External graphics state dictionary.
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
Imports System.Collections.Generic


    Friend Class PdfExtGState
        Inherits PdfObject
        Implements IComparable(Of PdfExtGState)

        Friend Key As String
        Friend Value As String

        ' search constructor
        Friend Sub New(Key As String, Value As String)
            ' save value
            Me.Key = Key
            Me.Value = Value

            ' exit
            Return
        End Sub

        ' object constructor
        Friend Sub New(Document As PdfDocument, Key As String, Value As String)
            MyBase.New(Document, ObjectType.Dictionary, "/ExtGState")
            ' save value
            Me.Key = Key
            Me.Value = Value

            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("G"c)
            Return
        End Sub

        Friend Shared Function CreateExtGState(Document As PdfDocument, Key As String, Value As String) As PdfExtGState
            If Document.ExtGStateArray Is Nothing Then Document.ExtGStateArray = New List(Of PdfExtGState)()

            ' search list for a duplicate
            Dim Index As Integer = Document.ExtGStateArray.BinarySearch(New PdfExtGState(Key, Value))

            ' this value is a duplicate
            If Index >= 0 Then Return Document.ExtGStateArray(Index)

            ' new blend object
            Dim ExtGState As PdfExtGState = New PdfExtGState(Document, Key, Value)

            ' save new string in array
            Document.ExtGStateArray.Insert(Not Index, ExtGState)

            ' update dictionary
            ExtGState.Dictionary.Add(Key, Value)

            ' exit
            Return ExtGState
        End Function

    ''' <summary>
    ''' Compare two PdfExtGState objects.
    ''' </summary>
    ''' <param name="Other">Other object.</param>
    ''' <returns>Compare result.</returns>
    Public Overloads Function CompareTo(Other As PdfExtGState) As Integer Implements IComparable(Of PdfExtGState).CompareTo
        Dim Cmp = String.Compare(Key, Other.Key)
        If Cmp <> 0 Then Return Cmp
        Return String.Compare(Value, Other.Value)
    End Function
End Class
