#Region "Microsoft.VisualBasic::510c30536c384df0aaf6b7cce2e597fc, mime\application%pdf\PdfFileWriter\PDF\PdfDictionary.vb"

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

    ' Enum ValueType
    ' 
    '     [String], Dictionary, Other
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfDictionary
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: Find, GetValue
    ' 
    '     Sub: (+2 Overloads) Add, AddBoolean, AddDictionary, AddFormat, AddIndirectReference
    '          AddInteger, AddName, AddPdfString, (+2 Overloads) AddReal, (+2 Overloads) AddRectangle
    '          Remove, (+2 Overloads) WriteToPdfFile
    ' 
    ' Class PdfKeyValue
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
'	Barcode
'	Single diminsion barcode class.
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
Imports System.Text
Imports stdNum = System.Math

Friend Enum ValueType
    Other
    [String]
    Dictionary
End Enum

''' <summary>
''' PDF dictionary class
''' </summary>
''' <remarks>
''' <para>
''' Dictionary key value pair class. Holds one key value pair.
''' </para>
''' </remarks>
Public Class PdfDictionary
    Friend KeyValue As List(Of PdfKeyValue)
    Friend Parent As PdfObject
    Friend Document As PdfDocument

    Friend Sub New(Parent As PdfObject)
        KeyValue = New List(Of PdfKeyValue)()
        Me.Parent = Parent
        Document = Parent.Document
        Return
    End Sub

    Friend Sub New(Document As PdfDocument)
        KeyValue = New List(Of PdfKeyValue)()
        Me.Document = Document
        Return
    End Sub

    
    ' Find key value pair in dictionary.
    ' return index number or -1 if not found.
    

    Friend Function Find(Key As String) As Integer        ' key (first character must be forward slash /)
        ' look through the dictionary
        For Index = 0 To KeyValue.Count - 1
            If Equals(KeyValue(Index).Key, Key) Then Return Index
        Next

        ' not found
        Return -1
    End Function

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub Add(Key As String, Str As String)        ' key (first character must be forward slash /)
        Add(Key, Str, ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddName(Key As String, Str As String)        ' key (first character must be forward slash /)
        Add(Key, "/" & Str, ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddInteger(Key As String, [Integer] As Integer)      ' key (first character must be forward slash /)
        Add(Key, [Integer].ToString(), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddReal(Key As String, Real As Double)       ' key (first character must be forward slash /)
        If stdNum.Abs(Real) < 0.0001 Then Real = 0
        Add(Key, String.Format(PeriodDecSep, "{0}", CSng(Real)), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddReal(Key As String, Real As Single)       ' key (first character must be forward slash /)
        If stdNum.Abs(Real) < 0.0001 Then Real = 0
        Add(Key, String.Format(PeriodDecSep, "{0}", Real), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddRectangle(Key As String, Rect As PdfRectangle)        ' key (first character must be forward slash /)
        AddRectangle(Key, Rect.Left, Rect.Bottom, Rect.Right, Rect.Top)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddRectangle(Key As String, Left As Double, Bottom As Double, Right As Double, Top As Double)      ' key (first character must be forward slash /)
        Add(Key, String.Format(PeriodDecSep, "[{0} {1} {2} {3}]", Parent.ToPt(Left), Parent.ToPt(Bottom), Parent.ToPt(Right), Parent.ToPt(Top)))
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddBoolean(Key As String, Bool As Boolean)       ' key (first character must be forward slash /)
        Add(Key, If(Bool, "true", "false"), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddPdfString(Key As String, Str As String)       ' key (first character must be forward slash /)
        Add(Key, Str, ValueType.String)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is string format
    

    Friend Sub AddFormat(Key As String, FormatStr As String, ParamArray FormatList As Object())     ' key (first character must be forward slash /)
        Add(Key, String.Format(PeriodDecSep, FormatStr, FormatList), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' The value is a reference to indirect object number.
    

    Friend Sub AddIndirectReference(Key As String, Obj As PdfObject)    ' key (first character must be forward slash /)
        ' PdfObject. The method creates an indirect reference "n 0 R" to the object.
        Add(Key, String.Format("{0} 0 R", Obj.ObjectNumber), ValueType.Other)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' If dictionary does not exist, create it.
    ' If key is not found, add the pair as new entry.
    ' If key is found, replace old pair with new one.
    

    Friend Sub AddDictionary(Key As String, Value As PdfDictionary)     ' key (first character must be forward slash /)
        ' value
        Add(Key, Value, ValueType.Dictionary)
        Return
    End Sub

    
    ' Add key value pair to dictionary.
    ' If dictionary does not exist, create it.
    ' If key is not found, add the pair as new entry.
    ' If key is found, replace old pair with new one.
    

    Private Sub Add(Key As String, Value As Object, Type As ValueType)        ' key (first character must be forward slash /)
        ' value
        ' value type
        ' search for existing key
        Dim Index = Find(Key)

        ' not found - add new pair

        ' found replace value
        If Index < 0 Then
            KeyValue.Add(New PdfKeyValue(Key, Value, Type))
        Else
            KeyValue(Index).Value = Value
            KeyValue(Index).Type = Type
        End If

        ' exit
        Return
    End Sub

    
    ' Get dictionary value
    ' Return string if key is found, null if not
    

    Friend Function GetValue(Key As String) As PdfKeyValue        ' key (first character must be forward slash /)
        Dim Index = Find(Key)
        Return If(Index >= 0, KeyValue(Index), Nothing)
    End Function

    
    ' Remove key value pair from dictionary
    

    Friend Sub Remove(Key As String)      ' key (first character must be forward slash /)
        Dim Index = Find(Key)
        If Index >= 0 Then KeyValue.RemoveAt(Index)
        Return
    End Sub

    
    ' Write dictionary to PDF file
    ' Called from WriteObjectToPdfFile to output a dictionary
    

    Friend Sub WriteToPdfFile()
        Dim EolMarker = 100
        Dim Str As StringBuilder = New StringBuilder()
        WriteToPdfFile(Str, EolMarker)

        ' write to pdf file
        Document.PdfFile.WriteString(Str)
        Return
    End Sub

    Private Sub WriteToPdfFile(Str As StringBuilder, ByRef EolMarker As Integer)
        Str.Append("<<")

        ' output dictionary
        For Each KeyValueItem In KeyValue
            ' add new line to cut down very long lines (just appearance)
            If Str.Length > EolMarker Then
                Str.Append(Microsoft.VisualBasic.Constants.vbLf)
                EolMarker = Str.Length + 100
            End If

            ' append the key
            Str.Append(KeyValueItem.Key)

            ' dictionary type
            Select Case KeyValueItem.Type
                ' dictionary
                Case ValueType.Dictionary
                    CType(KeyValueItem.Value, PdfDictionary).WriteToPdfFile(Str, EolMarker)

                ' PDF string special case
                Case ValueType.String

                    ' all other key value pairs
                    Str.Append(Document.TextToPdfString(CStr(KeyValueItem.Value), Parent))
                Case Else
                    ' add one space between key and value unless value starts with a clear separator
                    Dim FirstChar = CStr(KeyValueItem.Value)(0)
                    If FirstChar <> "/"c AndAlso FirstChar <> "["c AndAlso FirstChar <> "<"c AndAlso FirstChar <> "("c Then Str.Append(" "c)

                    ' add value
                    Str.Append(KeyValueItem.Value)
            End Select
        Next

        ' terminate dictionary
        Str.Append(">>" & Microsoft.VisualBasic.Constants.vbLf)
        Return
    End Sub
End Class

Friend Class PdfKeyValue
    Friend Key As String        ' key first character must be forward slash ?
    Friend Value As Object      ' value associated with key
    Friend Type As ValueType        ' value is a PDF string

    
    ' Constructor
    

    Friend Sub New(Key As String, Value As Object, Type As ValueType)     ' key first character must be forward slash ?
        ' value associated with key
        ' value type
        If Key(0) <> "/"c Then Throw New ApplicationException("Dictionary key must start with /")
        Me.Key = Key
        Me.Value = Value
        Me.Type = Type
        Return
    End Sub
End Class
