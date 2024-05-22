#Region "Microsoft.VisualBasic::a92194cc9d1785b577636eb73cc9aa09, mime\application%pdf\PdfReader\Document\PdfIndirectObjectId.vb"

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

    '   Total Lines: 139
    '    Code Lines: 112 (80.58%)
    ' Comment Lines: 2 (1.44%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (17.99%)
    '     File Size: 4.89 KB


    '     Class PdfIndirectObjectId
    ' 
    '         Properties: Count, Gens, Id, Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ContainsGen, GetEnumerator
    ' 
    '         Sub: AddXRef, (+2 Overloads) ResolveAllReferences
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfIndirectObjectId
        Inherits PdfObject

        Private _Id As Integer
        Private _single As PdfIndirectObject
        Private _gens As Dictionary(Of Integer, PdfIndirectObject)

        Public Sub New(parent As PdfObject, id As Integer)
            MyBase.New(parent)
            Me.Id = id
        End Sub

        Public Property Id As Integer
            Get
                Return _Id
            End Get
            Private Set(value As Integer)
                _Id = value
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                If _gens Is Nothing Then Return If(_single Is Nothing, 0, 1)
                Return _gens.Count
            End Get
        End Property

        Public Function ContainsGen(gen As Integer) As Boolean
            If _gens Is Nothing Then
                If _single Is Nothing Then
                    Return False
                Else
                    Return _single.Gen = gen
                End If
            End If

            Return _gens.ContainsKey(gen)
        End Function

        Public ReadOnly Property Gens As Dictionary(Of Integer, PdfIndirectObject).KeyCollection
            Get

                If _gens Is Nothing Then
                    Dim temp = New Dictionary(Of Integer, PdfIndirectObject)()
                    If _single IsNot Nothing Then temp.Add(_single.Gen, _single)
                    Return temp.Keys
                End If

                Return _gens.Keys
            End Get
        End Property

        Public ReadOnly Property Values As Dictionary(Of Integer, PdfIndirectObject).ValueCollection
            Get

                If _gens Is Nothing Then
                    Dim temp = New Dictionary(Of Integer, PdfIndirectObject)()
                    If _single IsNot Nothing Then temp.Add(_single.Gen, _single)
                    Return temp.Values
                End If

                Return _gens.Values
            End Get
        End Property

        Public Function GetEnumerator() As Dictionary(Of Integer, PdfIndirectObject).Enumerator
            If _gens Is Nothing Then
                Dim temp = New Dictionary(Of Integer, PdfIndirectObject)()
                If _single IsNot Nothing Then temp.Add(_single.Gen, _single)
                Return temp.GetEnumerator()
            End If

            Return _gens.GetEnumerator()
        End Function

        Default Public ReadOnly Property Item(gen As Integer) As PdfIndirectObject
            Get

                If _gens Is Nothing Then
                    If _single IsNot Nothing AndAlso _single.Gen = gen Then
                        Return _single
                    Else
                        Return Nothing
                    End If
                End If

                Return _gens(gen)
            End Get
        End Property

        Public Sub ResolveAllReferences(document As PdfDocument)
            If _gens Is Nothing Then
                If _single IsNot Nothing AndAlso _single.Child Is Nothing Then document.ResolveReference(_single)
            Else

                For Each indirect In _gens.Values
                    If indirect.Child IsNot Nothing Then document.ResolveReference(indirect)
                Next
            End If
        End Sub

        Public Sub ResolveAllReferences(parser As Parser, document As PdfDocument)
            If _gens Is Nothing Then
                If _single IsNot Nothing AndAlso _single.Child Is Nothing Then document.ResolveReference(parser, _single)
            Else

                For Each indirect In _gens.Values
                    If indirect.Child IsNot Nothing Then document.ResolveReference(parser, indirect)
                Next
            End If
        End Sub

        Public Sub AddXRef(xref As TokenXRefEntry)
            Dim indirect As PdfIndirectObject = New PdfIndirectObject(Me, xref)

            If _gens Is Nothing Then
                If _single Is Nothing Then
                    ' Cache the single object
                    _single = indirect
                    Return
                Else
                    ' Convert from single entry to using a dictionary
                    _gens = New Dictionary(Of Integer, PdfIndirectObject) From {
                        {_single.Gen, _single}
                    }
                    _single = Nothing
                End If
            End If

            If _gens.ContainsKey(xref.Gen) Then Throw New ApplicationException($"Indirect object with Id:{xref.Id} Gen:{xref.Gen} already exists.")
            _gens.Add(indirect.Gen, indirect)
        End Sub
    End Class
End Namespace
