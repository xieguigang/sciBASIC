#Region "Microsoft.VisualBasic::a9deed247308230ebef5e212d69cb4e6, Data\BinaryData\HDF5\HDF5Reader.vb"

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

    '   Total Lines: 205
    '    Code Lines: 138
    ' Comment Lines: 38
    '   Blank Lines: 29
    '     File Size: 7.37 KB


    ' Class HDF5Reader
    ' 
    '     Properties: attributes, chunks, data, dataBTree, dataGroup
    '                 dataset, datasetName, dataSpace, dataType, headerSize
    '                 isDataSet, layout, reader, superblock
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: ParseDataObject, parserObject, ToString
    ' 
    '     Sub: parseHeader, printValues
    ' 
    ' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

''' <summary>
''' 这个reader只会读取一个<see cref="datasetName"/>的数据，如果需要读取其他的dataset的话，
''' 则会需要创建多个<see cref="HDF5Reader"/>对象来进行数据的读取操作
''' 
''' 这个对象相当于文件系统之中的一个文件或者文件夹
''' </summary>
''' <remarks>
''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
''' </remarks>
Public Class HDF5Reader : Implements IFileDump

    Public ReadOnly Property reader As BinaryReader
    Public ReadOnly Property dataGroup As Group

    ''' <summary>
    ''' header length
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property headerSize As Long
    Public ReadOnly Property datasetName As String
    Public ReadOnly Property layout As Layout
    Public ReadOnly Property dataBTree As DataBTree
    Public ReadOnly Property chunks As List(Of DataChunk)
    Public ReadOnly Property dataType As DataTypeMessage
    Public ReadOnly Property dataSpace As DataspaceMessage
    Public ReadOnly Property dataset As Hdf5Dataset
    Public ReadOnly Property attributes As Dictionary(Of String, Object)

    Dim file As HDF5File

    Public ReadOnly Property superblock As Superblock
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return file.superblock
        End Get
    End Property

    ''' <summary>
    ''' 当前的这个对象是一个不进行数据存储的文件夹还是存储具体的数据的文件对象？
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property isDataSet As Boolean
        Get
            Return dataGroup Is Nothing
        End Get
    End Property

    ''' <summary>
    ''' 非数据集的时候总是返回空值
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property data As Object
        Get
            If Not isDataSet Then
                Return Nothing
            Else
                Return dataset.data(file.superblock)
            End If
        End Get
    End Property

    Public Sub New(filename As String, datasetName As String)
        Me.file = New HDF5File(filename)
        Me.reader = New BinaryFileReader(filename)
        Me.datasetName = datasetName
        Me.layout = Nothing
        Me.dataBTree = Nothing
        Me.chunks = New List(Of DataChunk)()
        Me.headerSize = 0

        Call parseHeader()
    End Sub

    Private Sub New(file As HDF5File, datasetName As String)
        Me.file = file
        Me.reader = file.reader
        Me.datasetName = datasetName
        Me.layout = Nothing
        Me.dataBTree = Nothing
        Me.chunks = New List(Of DataChunk)()
        Me.headerSize = 0
    End Sub

    Friend Sub New(file As HDF5File, dataset As DataObjectFacade)
        Me.file = file
        Me.reader = New BinaryFileReader(file.fileName)
        Me.datasetName = dataset.symbolName
        Me.layout = Nothing
        Me.dataBTree = Nothing
        Me.chunks = New List(Of DataChunk)()
        Me.headerSize = 0

        _dataGroup = parserObject(dataset, container:=Me)
        _headerSize = reader.maxOffset
    End Sub

    ''' <summary>
    ''' 如果目标数据集不存在的话，这个函数会返回空值
    ''' </summary>
    ''' <param name="dataSetName"></param>
    ''' <returns></returns>
    Public Function ParseDataObject(dataSetName As String) As HDF5Reader
        Dim reader As New HDF5Reader(Me.file, dataSetName)
        Dim dobj As DataObjectFacade = dataGroup _
            .objects _
            .FirstOrDefault(Function(d)
                                Return d.symbolName.TextEquals(dataSetName)
                            End Function)

        If dobj Is Nothing Then
            Return Nothing
        End If

        reader._dataGroup = parserObject(dobj, container:=reader)
        _headerSize = Me.reader.maxOffset

        Return reader
    End Function

    Private Sub parseHeader()
        Dim sb As Superblock = Me.superblock
        Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
        Dim objectFacade As New DataObjectFacade(sb, "root", rootSymbolTableEntry.objectHeaderAddress)
        Dim rootGroup As New Group(sb, objectFacade)
        Dim objects As List(Of DataObjectFacade) = rootGroup.objects

        For Each dobj As DataObjectFacade In objects
            ' compare dataset name
            If dobj.symbolName.TextEquals(Me.datasetName) Then
                _dataGroup = parserObject(dobj, Me)
                Exit For
            End If
        Next

        _headerSize = Me.reader.maxOffset
    End Sub

    ''' <summary>
    ''' 如果是dataset，则直接返回空值，反之返回<see cref="Group"/>对象
    ''' </summary>
    ''' <param name="dobj"></param>
    ''' <returns></returns>
    Private Shared Function parserObject(dobj As DataObjectFacade, container As HDF5Reader) As Group
        ' parse or get layout
        Dim layout As Layout = dobj.layout
        Dim reader As BinaryReader = container.reader
        Dim sb As Superblock = container.superblock

        container._layout = layout
        container._attributes = HDF5File.attributeTable(dobj.attributes, sb)

        If layout.isEmpty Then
            Return New Group(sb, dobj)
        Else
            ' parse btree index of the data
            Dim dataTree As New DataBTree(layout)
            Dim iter As DataChunkIterator = dataTree.getChunkIterator(sb)
            Dim chunk As DataChunk

            container._dataset = dobj.layout.dataset
            container._dataBTree = dataTree
            container._dataType = dobj.GetMessage(ObjectHeaderMessages.Datatype)
            container._dataSpace = dobj.GetMessage(ObjectHeaderMessages.Dataspace)

            If Not container.dataset Is Nothing Then
                container.dataset.dataSpace = container.dataSpace
                container.dataset.dataType = container.dataType.reader
                container.dataset.dataLayout = layout
                container.dataset.pipeline = dobj.filterMessage
            End If

            While iter.hasNext()
                chunk = iter.[next](reader, sb)
                ' read/add a new data chunk block
                container.chunks.Add(chunk)
            End While

            Return Nothing
        End If
    End Function

    Public Overrides Function ToString() As String
        If Not layout Is Nothing AndAlso Not layout.isEmpty Then
            Return $"Dim {datasetName} As {dataType.reader} = &{layout.dataAddress}"
        Else
            Return dataGroup.ToString
        End If
    End Function

    Private Sub printValues(out As TextWriter) Implements IFileDump.printValues
        Throw New NotImplementedException()
    End Sub
End Class
