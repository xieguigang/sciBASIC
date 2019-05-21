#Region "Microsoft.VisualBasic::9a983f6b53c01abb812689496e64e0cf, Data\BinaryData\DataStorage\HDF5\HDF5Reader.vb"

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
'         Properties: chunks, dataBTree, dataGroups, datasetName, fileName
'                     headerSize, layout, reader
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: ToString
' 
'         Sub: (+2 Overloads) Dispose, parseHeader, parserObject
' 
' 
' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure

Namespace HDF5

    ''' <summary>
    ''' 这个reader只会读取一个<see cref="datasetName"/>的数据，如果需要读取其他的dataset的话，则会需要创建多个<see cref="HDF5Reader"/>对象来进行数据的读取操作
    ''' </summary>
    ''' <remarks>
    ''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
    ''' </remarks>
    Public Class HDF5Reader : Implements IDisposable

        Public ReadOnly Property reader As BinaryReader
        Public ReadOnly Property dataGroups As Group

        ''' <summary>
        ''' header length
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property headerSize As Long
        Public ReadOnly Property fileName As String
        Public ReadOnly Property datasetName As String
        Public ReadOnly Property layout As Layout
        Public ReadOnly Property dataBTree As DataBTree
        Public ReadOnly Property chunks As List(Of DataChunk)

        Public ReadOnly Property Superblock As Superblock
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Superblock(reader, Scan0)
            End Get
        End Property

        Public Sub New(filename As String, datasetName As String)
            Me.fileName = filename
            Me.reader = New BinaryFileReader(filename)
            Me.datasetName = datasetName
            Me.layout = Nothing
            Me.dataBTree = Nothing
            Me.chunks = New List(Of DataChunk)()
            Me.headerSize = 0

            Call parseHeader()
        End Sub

        Private Sub New([in] As BinaryReader, datasetName As String)
            Me.fileName = Nothing
            Me.reader = [in]
            Me.datasetName = datasetName
            Me.layout = Nothing
            Me.dataBTree = Nothing
            Me.chunks = New List(Of DataChunk)()
            Me.headerSize = 0
        End Sub

        Public Function ParseDataObject(dataSetName As String) As HDF5Reader
            Dim reader As New HDF5Reader(Me.reader, dataSetName)
            Dim dobj As DataObjectFacade = dataGroups.objects.FirstOrDefault(Function(d) d.symbolName.TextEquals(dataSetName))

            parserObject(dobj, sb:=Superblock)
            _headerSize = Me.reader.maxOffset

            Return reader
        End Function

        Private Sub parseHeader()
            Dim sb As Superblock = Me.Superblock
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.reader, sb, objectFacade)
            Dim objects As List(Of DataObjectFacade) = rootGroup.objects

            For Each dobj As DataObjectFacade In objects
                ' compare dataset name
                If dobj.symbolName.TextEquals(Me.datasetName) Then
                    Call parserObject(dobj, sb)
                    Exit For
                End If
            Next

            _headerSize = Me.reader.maxOffset
        End Sub

        Private Sub parserObject(dobj As DataObjectFacade, sb As Superblock)
            ' parse or get layout
            Dim layout As Layout = dobj.layout

            _layout = layout

            If layout.IsEmpty Then
                _dataGroups = New Group(reader, sb, dobj)
            Else
                ' parse btree index of the data
                Dim dataTree As New DataBTree(layout)

                _dataBTree = dataTree

                Dim iter As DataChunkIterator = dataTree.getChunkIterator(Me.reader, sb)
                Dim chunk As DataChunk

                While iter.hasNext(Me.reader, sb)
                    chunk = iter.[next](Me.reader, sb)
                    ' read/add a new data chunk block
                    chunks.Add(chunk)
                End While
            End If
        End Sub

        Public Overrides Function ToString() As String
            If Not layout.IsEmpty Then
                Return $"{reader} => {layout}"
            Else
                Return $"{reader} => {dataGroups}"
            End If
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
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
