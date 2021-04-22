Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfIndirectObjects
        Inherits PdfObject

        Private _ids As Dictionary(Of Integer, PdfIndirectObjectId) = New Dictionary(Of Integer, PdfIndirectObjectId)()

        Public Sub New(ByVal parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return _ids.Count
            End Get
        End Property

        Public Function ContainsId(ByVal id As Integer) As Boolean
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

        Default Public ReadOnly Property Item(ByVal id As Integer) As PdfIndirectObjectId
            Get
                Return _ids(id)
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal id As Integer, ByVal gen As Integer) As PdfIndirectObject
            Get
                Return _ids(id)(gen)
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal reference As PdfObjectReference) As PdfIndirectObject
            Get
                Return Me(reference.Id, reference.Gen)
            End Get
        End Property

        Public Function OptionalValue(Of T As PdfObject)(ByVal reference As PdfObjectReference) As T
            Dim obj = Document.ResolveReference(reference.Id, reference.Gen)

            If obj IsNot Nothing Then
                If Not (TypeOf obj Is T) Then Throw New ApplicationException($"Optional indirect object ({reference.Id},{reference.Gen}) incorrect type.")
                Return obj
            End If

            Return Nothing
        End Function

        Public Function MandatoryValue(Of T As PdfObject)(ByVal reference As PdfObjectReference) As T
            Dim obj = Document.ResolveReference(reference.Id, reference.Gen)
            If obj Is Nothing OrElse Not (TypeOf obj Is T) Then Throw New ApplicationException($"Mandatory indirect object ({reference.Id},{reference.Gen}) missing or incorrect type.")
            Return obj
        End Function

        Public Sub ResolveAllReferences(ByVal document As PdfDocument)
            For Each id In Values
                id.ResolveAllReferences(document)
            Next
        End Sub

        Public Sub AddXRef(ByVal xref As TokenXRefEntry)
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
