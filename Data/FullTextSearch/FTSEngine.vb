Imports System.IO
Imports Microsoft.VisualBasic.Linq

Public Class FTSEngine : Implements IDisposable

    ReadOnly index As InvertedIndex
    ReadOnly documents As FileStorage
    ReadOnly repo_dir As String

    Private disposedValue As Boolean

    Sub New(repo_dir As String)
        Dim offsets As Long() = Nothing

        Me.index = FileStorage.ReadIndex($"{repo_dir}/index.dat".Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False), offsets)
        Me.documents = New FileStorage(offsets, $"{repo_dir}/documents.dat".Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False))
        Me.repo_dir = repo_dir
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
        If index.Add(doc) Then
            Call documents.Save(doc)
        End If
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

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
                Call FileStorage.WriteIndex(index, documents.AsEnumerable.ToArray, $"{repo_dir}/index.dat".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
                Call documents.Dispose()
            End If

            ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
            ' TODO: 将大型字段设置为 null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
    ' Protected Overrides Sub Finalize()
    '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
