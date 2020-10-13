''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
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
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Imports System
Imports System.Collections.Generic


    Friend Class PdfExtGState
        Inherits PdfObject
        Implements IComparable(Of PdfExtGState)

        Friend Key As String
        Friend Value As String

        ' search constructor
        Friend Sub New(ByVal Key As String, ByVal Value As String)
            ' save value
            Me.Key = Key
            Me.Value = Value

            ' exit
            Return
        End Sub

        ' object constructor
        Friend Sub New(ByVal Document As PdfDocument, ByVal Key As String, ByVal Value As String)
            MyBase.New(Document, ObjectType.Dictionary, "/ExtGState")
            ' save value
            Me.Key = Key
            Me.Value = Value

            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("G"c)
            Return
        End Sub

        Friend Shared Function CreateExtGState(ByVal Document As PdfDocument, ByVal Key As String, ByVal Value As String) As PdfExtGState
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
        Public Function CompareTo(ByVal Other As PdfExtGState) As Integer Implements IComparable(Of PdfExtGState).CompareTo
            Dim Cmp = String.Compare(Key, Other.Key)
            If Cmp <> 0 Then Return Cmp
            Return String.Compare(Value, Other.Value)
        End Function
    End Class

