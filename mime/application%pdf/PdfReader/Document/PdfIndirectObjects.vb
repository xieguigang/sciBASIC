#Region "Microsoft.VisualBasic::5983793eebd6d1e42f63e4a7337c6072, mime\application%pdf\PdfReader\Document\PdfIndirectObjects.vb"

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

    '   Total Lines: 93
    '    Code Lines: 73 (78.49%)
    ' Comment Lines: 1 (1.08%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (20.43%)
    '     File Size: 3.35 KB


    '     Class PdfIndirectObjects
    ' 
    '         Properties: Count, Ids, Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ContainsId, GetEnumerator, MandatoryValue, OptionalValue
    ' 
    '         Sub: AddXRef, ResolveAllReferences
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfIndirectObjects
        Inherits PdfObject

        Private _ids As Dictionary(Of Integer, PdfIndirectObjectId) = New Dictionary(Of Integer, PdfIndirectObjectId)()

        Public Sub New(parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return _ids.Count
            End Get
        End Property

        Public Function ContainsId(id As Integer) As Boolean
            Return _ids.ContainsKey(id)
        End Function

        Public ReadOnly Property Ids As Dictionary(Of Integer, PdfIndirectObjectId).KeyCollection
            Get
                Return _ids.Keys
            End Get
        End Property

        Public ReadOnly Property Values As Dictionary(Of Integer, PdfIndirectObjectId).ValueCollection
            Get
                Return _ids.Values
            End Get
        End Property

        Public Function GetEnumerator() As Dictionary(Of Integer, PdfIndirectObjectId).Enumerator
            Return _ids.GetEnumerator()
        End Function

        Default Public ReadOnly Property Item(id As Integer) As PdfIndirectObjectId
            Get
                Return _ids(id)
            End Get
        End Property

        Default Public ReadOnly Property Item(id As Integer, gen As Integer) As PdfIndirectObject
            Get
                Return _ids(id)(gen)
            End Get
        End Property

        Default Public ReadOnly Property Item(reference As PdfObjectReference) As PdfIndirectObject
            Get
                Return Me(reference.Id, reference.Gen)
            End Get
        End Property

        Public Function OptionalValue(Of T As PdfObject)(reference As PdfObjectReference) As T
            Dim obj = Document.ResolveReference(reference.Id, reference.Gen)

            If obj IsNot Nothing Then
                If Not (TypeOf obj Is T) Then Throw New ApplicationException($"Optional indirect object ({reference.Id},{reference.Gen}) incorrect type.")
                Return obj
            End If

            Return Nothing
        End Function

        Public Function MandatoryValue(Of T As PdfObject)(reference As PdfObjectReference) As T
            Dim obj = Document.ResolveReference(reference.Id, reference.Gen)
            If obj Is Nothing OrElse Not (TypeOf obj Is T) Then Throw New ApplicationException($"Mandatory indirect object ({reference.Id},{reference.Gen}) missing or incorrect type.")
            Return obj
        End Function

        Public Sub ResolveAllReferences(document As PdfDocument)
            For Each id In Values
                id.ResolveAllReferences(document)
            Next
        End Sub

        Public Sub AddXRef(xref As TokenXRefEntry)
            ' If this is the first time we have encountered this id, then add it
            Dim indirectId As PdfIndirectObjectId = Nothing

            If Not _ids.TryGetValue(xref.Id, indirectId) Then
                indirectId = New PdfIndirectObjectId(Me, xref.Id)
                _ids.Add(xref.Id, indirectId)
            End If

            indirectId.AddXRef(xref)
        End Sub
    End Class
End Namespace
