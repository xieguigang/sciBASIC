#Region "Microsoft.VisualBasic::2ed443b258c85fc96be0f52a8fc2e51c, Data\FullTextSearch\FTSEngine.vb"

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

    '   Total Lines: 77
    '    Code Lines: 46 (59.74%)
    ' Comment Lines: 17 (22.08%)
    '    - Xml Docs: 35.29%
    ' 
    '   Blank Lines: 14 (18.18%)
    '     File Size: 2.68 KB


    ' Class FTSEngine
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Search
    ' 
    '     Sub: (+2 Overloads) Dispose, (+2 Overloads) Indexing
    ' 
    ' /********************************************************************************/

#End Region

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

