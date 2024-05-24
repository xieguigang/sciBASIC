Imports System.IO

Public Class FTSEngine

    ReadOnly index As InvertedIndex
    ReadOnly documents As FileStorage

    Sub New(repo_dir As String)
        Dim offsets As Long() = Nothing

        index = FileStorage.ReadIndex($"{repo_dir}/index.dat".OpenReadonly, offsets)
        documents = New FileStorage(offsets, $"{repo_dir}/documents.dat".Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False))
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
End Class
