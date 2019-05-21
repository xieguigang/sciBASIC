#Region "Microsoft.VisualBasic::20157b1c1fd6d6c84ee14d2e8c98a95d, mime\application%netcdf\HDF5\HDF5Reader.vb"

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

'     Class HDF5Reader
' 
'         Properties: chunks, dataBTree, datasetName, fileName, headerSize
'                     layout, reader
' 
'         Constructor: (+2 Overloads) Sub New
'         Sub: close, parseHeader
' 
' 
' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure

Namespace HDF5

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
    ''' </remarks>
    Public Class HDF5Reader : Implements IDisposable

        Private m_filename As String
        Private m_datasetName As String
        Private m_layout As Layout
        Private m_dataTree As DataBTree
        Private m_chunks As List(Of DataChunk)
        Private m_headerLength As Long

        Public ReadOnly Property reader() As BinaryReader

        Public Sub New(filename As String, datasetName As String)
            Me.m_filename = filename
            Me.reader = New BinaryFileReader(Me.m_filename)
            Me.m_datasetName = datasetName
            Me.m_layout = Nothing
            Me.m_dataTree = Nothing
            Me.m_chunks = New List(Of DataChunk)()
            Me.m_headerLength = 0
        End Sub

        Public Sub New([in] As BinaryReader, datasetName As String)
            Me.m_filename = Nothing
            Me.reader = [in]
            Me.m_datasetName = datasetName
            Me.m_layout = Nothing
            Me.m_dataTree = Nothing
            Me.m_chunks = New List(Of DataChunk)()
            Me.m_headerLength = 0
        End Sub

        Public Overridable Sub parseHeader()
            Dim sb As New Superblock(Me.reader, 0)
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.reader, sb, objectFacade)
            Dim objects As List(Of DataObjectFacade) = rootGroup.objects

            For Each dobj As DataObjectFacade In objects
                ' compare dataset name
                If dobj.symbolName.TextEquals(Me.m_datasetName) Then
                    Call parserObject(dobj, sb)
                    Exit For
                End If
            Next

            Me.m_headerLength = Me.reader.maxOffset
        End Sub

        Private Sub parserObject(dobj As DataObjectFacade, sb As Superblock)
            ' parse or get layout
            Dim layout As Layout = dobj.layout
            Me.m_layout = layout
            ' parse btree index of the data
            Dim dataTree As New DataBTree(layout)
            Me.m_dataTree = dataTree

            Dim iter As DataChunkIterator = dataTree.getChunkIterator(Me.reader, sb)
            Dim chunk As DataChunk

            While iter.hasNext(Me.reader, sb)
                chunk = iter.[next](Me.reader, sb)
                ' read/add a new data chunk block
                m_chunks.Add(chunk)
            End While
        End Sub

        Public Overridable ReadOnly Property headerSize() As Long
            Get
                Return Me.m_headerLength
            End Get
        End Property

        Public Overridable ReadOnly Property fileName() As String
            Get
                Return Me.m_filename
            End Get
        End Property

        Public Overridable ReadOnly Property datasetName() As String
            Get
                Return Me.m_datasetName
            End Get
        End Property

        Public Overridable ReadOnly Property layout() As Layout
            Get
                Return Me.m_layout
            End Get
        End Property

        Public Overridable ReadOnly Property dataBTree() As DataBTree
            Get
                Return Me.m_dataTree
            End Get
        End Property

        Public Overridable ReadOnly Property chunks() As List(Of DataChunk)
            Get
                Return Me.m_chunks
            End Get
        End Property

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    Call Me.reader.Dispose()
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub

        ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码以正确实现可释放模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
            Dispose(True)
            ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
