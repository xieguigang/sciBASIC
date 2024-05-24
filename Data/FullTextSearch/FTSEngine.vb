Imports System.IO
Imports Microsoft.VisualBasic.Linq

Public Class FTSEngine

    ReadOnly index As InvertedIndex
    ReadOnly documents As FileStorage

    Sub New(repo_dir As String)
        Dim offsets As Long() = Nothing

        index = FileStorage.ReadIndex($"{repo_dir}/index.dat".OpenReadonly, offsets)
        documents = New FileStorage(offsets, $"{repo_dir}/documents.dat".Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False))
    End Sub

    Public Sub Indexing(doc As IEnumerable(Of String))
        For Each par As String In doc
            Call Indexing(par)
        Next
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <remarks>
    ''' thread unsafe
    ''' </remarks>
    Public Sub Indexing(doc As String)
        Call index.Add(doc)
        Call documents.Save(doc)
    End Sub

    Public Iterator Function Search(text As String) As IEnumerable(Of SeqValue(Of String))
        Dim ids = index.Search(text)

        If ids Is Nothing Then
            Return
        End If

        For Each id As Integer In ids
            Yield New SeqValue(Of String)(id, documents.GetDocument(id))
        Next
    End Function
End Class
