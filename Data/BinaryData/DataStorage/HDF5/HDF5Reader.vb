#Region "Microsoft.VisualBasic::6f2d09f70090b50322e529710c87f8f3, Data\BinaryData\DataStorage\HDF5\HDF5Reader.vb"

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
'                     headerSize, layout, reader, Superblock
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: ParseDataObject, ToString
' 
'         Sub: (+2 Overloads) Dispose, parseHeader, parserObject, printValues
' 
' 
' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5

    Public Class HDF5File : Implements IDisposable

        ReadOnly reader As BinaryReader
        ReadOnly fileName As String

        Public ReadOnly Property Superblock As Superblock
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Superblock(reader, Scan0)
            End Get
        End Property

        Public ReadOnly Property rootObjects As Dictionary(Of String, DataObjectFacade)

        Default Public ReadOnly Property GetObject(symbolName As String) As DataObjectFacade
            Get
                Return rootObjects _
                    .Keys _
                    .First(Function(name) name.TextEquals(symbolName)) _
                    .GetValueOrDefault(rootObjects)
            End Get
        End Property

        Sub New(fileName As String)
            Me.reader = New BinaryFileReader(fileName)
            Me.fileName = fileName.FileName
        End Sub

        Private Sub parseHeader()
            Dim sb As Superblock = Me.Superblock
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.reader, sb, objectFacade)
            Dim objects As List(Of DataObjectFacade) = rootGroup.objects

            _rootObjects = objects.ToDictionary(Function(o) o.symbolName)
        End Sub

        Public Overrides Function ToString() As String
            Return fileName
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
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

    Public Class HDF5GroupReader : Implements IDisposable, IFileDump

        Public ReadOnly Property folder As Group

        Sub New()

        End Sub

        Friend Sub printValues(console As TextWriter) Implements IFileDump.printValues
            Throw New NotImplementedException()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
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

    ''' <summary>
    ''' 这个reader只会读取一个<see cref="datasetName"/>的数据，如果需要读取其他的dataset的话，则会需要创建多个<see cref="HDF5Reader"/>对象来进行数据的读取操作
    ''' </summary>
    ''' <remarks>
    ''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
    ''' </remarks>
    Public Class HDF5Reader : Implements IDisposable, IFileDump

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

            reader._dataGroups = parserObject(dobj, sb:=Superblock, container:=reader)
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
                    _dataGroups = parserObject(dobj, sb, Me)
                    Exit For
                End If
            Next

            _headerSize = Me.reader.maxOffset
        End Sub

        ''' <summary>
        ''' 如果是dataset，则直接返回空值，反之返回<see cref="Group"/>对象
        ''' </summary>
        ''' <param name="dobj"></param>
        ''' <param name="sb"></param>
        ''' <returns></returns>
        Private Shared Function parserObject(dobj As DataObjectFacade, sb As Superblock, container As HDF5Reader) As Group
            ' parse or get layout
            Dim layout As Layout = dobj.layout
            Dim reader = container.reader

            container._layout = layout

            If layout.IsEmpty Then
                Return New Group(reader, sb, dobj)
            Else
                ' parse btree index of the data
                Dim dataTree As New DataBTree(layout)
                Dim iter As DataChunkIterator = dataTree.getChunkIterator(reader, sb)
                Dim chunk As DataChunk

                container._dataBTree = dataTree

                While iter.hasNext(reader, sb)
                    chunk = iter.[next](reader, sb)
                    ' read/add a new data chunk block
                    container.chunks.Add(chunk)
                End While

                Return Nothing
            End If
        End Function

        Public Overrides Function ToString() As String
            If Not layout Is Nothing AndAlso Not layout.IsEmpty Then
                Return $"{reader} => {layout}"
            Else
                Return $"{reader} => {dataGroups}"
            End If
        End Function

        Private Sub printValues(out As TextWriter) Implements IFileDump.printValues
            Throw New NotImplementedException()
        End Sub

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
