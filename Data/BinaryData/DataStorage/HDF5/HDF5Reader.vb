#Region "Microsoft.VisualBasic::729d6fc47ecefa701da0e82c1faf409d, Data\BinaryData\DataStorage\HDF5\HDF5Reader.vb"

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
    '         Properties: chunks, dataBTree, dataGroups, datasetName, headerSize
    '                     layout, reader, Superblock
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: ParseDataObject, parserObject, ToString
    ' 
    '         Sub: parseHeader, printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5

    ''' <summary>
    ''' 这个reader只会读取一个<see cref="datasetName"/>的数据，如果需要读取其他的dataset的话，则会需要创建多个<see cref="HDF5Reader"/>对象来进行数据的读取操作
    ''' </summary>
    ''' <remarks>
    ''' A VB.NET continues work of this java project: https://github.com/iychoi/HDF5HadoopReader
    ''' </remarks>
    Public Class HDF5Reader : Implements IFileDump

        Public ReadOnly Property reader As BinaryReader
        Public ReadOnly Property dataGroups As Group

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

        Dim file As HDF5File

        Public ReadOnly Property Superblock As Superblock
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Superblock(file, Scan0)
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

            _dataGroups = parserObject(dataset, sb:=Superblock, container:=Me)
            _headerSize = reader.maxOffset
        End Sub

        Public Function ParseDataObject(dataSetName As String) As HDF5Reader
            Dim reader As New HDF5Reader(Me.file, dataSetName)
            Dim dobj As DataObjectFacade = dataGroups.objects.FirstOrDefault(Function(d) d.symbolName.TextEquals(dataSetName))

            reader._dataGroups = parserObject(dobj, sb:=Superblock, container:=reader)
            _headerSize = Me.reader.maxOffset

            Return reader
        End Function

        Private Sub parseHeader()
            Dim sb As Superblock = Me.Superblock
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(sb, objectFacade)
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
                Return New Group(sb, dobj)
            Else
                ' parse btree index of the data
                Dim dataTree As New DataBTree(layout)
                Dim iter As DataChunkIterator = dataTree.getChunkIterator(sb)
                Dim chunk As DataChunk

                container._dataBTree = dataTree
                container._dataType = dobj.GetMessage(ObjectHeaderMessages.Datatype)
                container._dataSpace = dobj.GetMessage(ObjectHeaderMessages.Dataspace)

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
    End Class
End Namespace
