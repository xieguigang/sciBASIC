#Region "Microsoft.VisualBasic::3430dcf28e114b23c85faa8a051c53d2, Data\BinaryData\HDF5\dataset\ChunkedDatasetV3.vb"

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

    '   Total Lines: 297
    '    Code Lines: 161
    ' Comment Lines: 81
    '   Blank Lines: 55
    '     File Size: 11.82 KB


    '     Class ChunkedDatasetV3
    ' 
    '         Properties: BtreeAddress, byteSize, dimensionality, dimensions, dimensionSize
    '                     diskSize, maxSize, size
    ' 
    '         Function: decodeChunk, dimensionIndexToLinearIndex, (+2 Overloads) getBuffer, getChunkOffset, getDataBuffer
    '                   getDecodedChunk, linearIndexToDimensionIndex
    ' 
    '     Class ChunkLookup
    ' 
    '         Properties: chunkValues, sb
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class ChunkOffsetKey
    ' 
    '         Properties: key
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace dataset

    ''' <summary>
    ''' Chunked: The array domain is regularly decomposed into chunks, and each chunk is allocated and 
    ''' stored separately. This layout supports arbitrary element traversals, compression, encryption, 
    ''' and checksums (these features are described in other messages). The message stores the size of 
    ''' a chunk instead of the size of the entire array; the storage size of the entire array can be 
    ''' calculated by traversing the chunk index that stores the chunk addresses.
    ''' 
    ''' This represents chunked datasets using a b-tree for indexing raw data chunks.
    ''' It supports filters for use when reading the dataset for example to
    ''' decompress.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class ChunkedDatasetV3 : Inherits Hdf5Dataset

        ''' <summary>
        ''' A chunk has a fixed dimensionality. This field specifies the number of dimension size 
        ''' fields later in the message.
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionality As Integer
        ''' <summary>
        ''' This is the address of the v1 B-tree that is used to look up the addresses of the chunks 
        ''' that actually store portions of the array data. The address may have the 
        ''' “undefined address” value, to indicate that storage has not yet been allocated for this 
        ''' array.
        ''' </summary>
        ''' <returns></returns>
        Public Property BtreeAddress As Long

        ''' <summary>
        ''' These values define the dimension size of a single chunk, in units of array elements 
        ''' (not bytes). The first dimension stored in the list of dimensions is the slowest changing 
        ''' dimension and the last dimension stored is the fastest changing dimension.
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionSize As Integer()

        ''' <summary>
        ''' ###### Dataset Element Size
        ''' 
        ''' The size of a dataset element, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public Property byteSize As Integer

        ReadOnly decodedChunkLookup As New Dictionary(Of ChunkOffsetKey, Byte())()

        Public Overridable ReadOnly Property size As Long
            Get
                Return dataSpace.totalLength
            End Get
        End Property

        Public Overridable ReadOnly Property diskSize As Long
            Get
                Return size * dataType.size
            End Get
        End Property

        Public Overrides ReadOnly Property dimensions As Integer()
            Get
                Return dataSpace.dimensionLength
            End Get
        End Property

        Public Overridable ReadOnly Property maxSize As Integer()
            Get
                If Not dataSpace.maxDimensionLength.IsNullOrEmpty Then
                    Return dataSpace.maxDimensionLength
                Else
                    Return dimensions
                End If
            End Get
        End Property

        ''' <summary>
        ''' 将一个线型的数组下标转换为多维矩阵数组的坐标信息
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="dimensions"></param>
        ''' <returns></returns>
        Private Function linearIndexToDimensionIndex(index As Integer, dimensions As Integer()) As Integer()
            Dim dimIndex As Integer() = New Integer(dimensions.Length - 1) {}

            For i As Integer = dimIndex.Length - 1 To 0 Step -1
                dimIndex(i) = index Mod dimensions(i)
                index = index \ dimensions(i)
            Next
            Return dimIndex
        End Function

        ''' <summary>
        ''' 将一个多维矩阵数组的坐标信息转换为一个一维线性区域的数组下标
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="dimensions"></param>
        ''' <returns></returns>
        Private Function dimensionIndexToLinearIndex(index As Integer(), dimensions As Integer()) As Integer
            Dim linear As Integer = 0

            If index.All(Function(i) i = 0) Then
                ' 所有的元素都是零，则其肯定是线性数组之中的第一个元素
                Return 0
            End If

            For i As Integer = 0 To dimensions.Length - 1
                Dim temp As Integer = index(i)

                For j As Integer = i + 1 To dimensions.Length - 1
                    temp *= dimensions(j)
                Next

                linear += temp
            Next

            Return linear
        End Function

        Private Function getChunkOffset(dimensionedIndex As Integer()) As Long()
            Dim chunkOffset As Long() = New Long(dimensionedIndex.Length - 1) {}

            For i As Integer = 0 To chunkOffset.Length - 1
                Dim temp As Long = dataLayout.chunkSize(i)
                chunkOffset(i) = (dimensionedIndex(i) \ temp) * temp
            Next

            Return chunkOffset
        End Function

        Protected Overrides Function getBuffer(sb As Superblock) As MemoryStream
            Return getBuffer(sb, New ChunkLookup(sb, Me))
        End Function

        Private Overloads Function getBuffer(sb As Superblock, chunkLookup As ChunkLookup) As MemoryStream
            ' Need to load the full buffer into memory so create the array
            Dim dataArray As Byte() = New Byte(diskSize - 1) {}
            Dim elementSize As Integer = dataType.size

            ' size 是元素的总数量，在这个循环之中，分别计算坐标，将每一个元素的数据字节从对应的chunk之中复制到dataArray之中
            For i As Integer = 0 To size - 1

                ' 在这里首先根据元素的字节占用数量计算元素所处的chunk的编号
                Dim dimensionedIndex As Integer() = linearIndexToDimensionIndex(i, dimensions)
                Dim chunkOffset As Long() = getChunkOffset(dimensionedIndex)

                ' Now figure out which element inside the chunk
                Dim insideChunk As Integer() = New Integer(chunkOffset.Length - 1) {}

                For j As Integer = 0 To chunkOffset.Length - 1
                    insideChunk(j) = CInt(dimensionedIndex(j) - chunkOffset(j))
                Next

                ' 然后下面的代码根据所计算出来的chunk编号查找出对应的chunk
                Dim insideChunkLinearOffset As Integer = dimensionIndexToLinearIndex(insideChunk, dataLayout.chunkSize)
                Dim key As New ChunkOffsetKey(chunkOffset)
                Dim chunkData As Byte() = getDecodedChunk(chunkLookup, key)

                ' Copy that data into the overall buffer
                ' 然后从所查找出来的chunk之中复制对应的元素到dataarray之中
                Dim sourcePos = insideChunkLinearOffset * elementSize
                Dim dataOffset = i * elementSize

                Array.Copy(chunkData, sourcePos, dataArray, dataOffset, elementSize)
            Next

            Return New MemoryStream(dataArray)
        End Function

        Private Function getDecodedChunk(chunkLookup As ChunkLookup, chunkKey As ChunkOffsetKey) As Byte()
            Return decodedChunkLookup.ComputeIfAbsent(
                chunkKey, Function(key)
                              Dim entry = chunkLookup(chunkKey)
                              Return decodeChunk(entry, chunkLookup.sb)
                          End Function)
        End Function

        Private Function decodeChunk(chunk As DataChunk, sb As Superblock) As Byte()
            ' Get the encoded (i.e. compressed buffer)
            ' Get the encoded data from buffer
            Dim encodedBytes As Byte() = getDataBuffer(sb, chunk)

            If pipeline Is Nothing Then
                ' No filters
                Return encodedBytes
            Else
                ' Decode using the pipeline applying the filters
                Dim decodedBytes As Byte() = pipeline.decode(encodedBytes)
                Return decodedBytes
            End If
        End Function

        Private Function getDataBuffer(sb As Superblock, chunk As DataChunk) As Byte()
            Return sb.FileReader(chunk.filePosition).readBytes(chunk.sizeOfChunk)
        End Function
    End Class

    Public Class ChunkLookup

        ReadOnly lookup As Dictionary(Of String, DataChunk)

        Public ReadOnly Property sb As Superblock

        Public ReadOnly Property chunkValues As DataChunk()
            Get
                Return lookup.Values.ToArray
            End Get
        End Property

        Default Public ReadOnly Property GetChunk(offsetKey As ChunkOffsetKey) As DataChunk
            Get
                Return lookup(offsetKey.key)
            End Get
        End Property

        Sub New(sb As Superblock, dataset As ChunkedDatasetV3)
            Dim bTree As New DataBTree(dataset.dataLayout)
            Dim chunkLookupMap As New Dictionary(Of String, DataChunk)()

            For Each chunk As DataChunk In bTree.EnumerateChunks(sb)
                chunkLookupMap(New ChunkOffsetKey(chunk.offsets).key) = chunk
            Next

            Me.lookup = chunkLookupMap
            Me.sb = sb
        End Sub

    End Class

    ''' <summary>
    ''' Custom key object for indexing chunks. It is optimised for fast hashcode and
    ''' equals when looking up chunks.
    ''' </summary>
    Public Class ChunkOffsetKey

        Friend ReadOnly hashcode As Integer
        Friend ReadOnly chunkOffset As Long()

        ''' <summary>
        ''' 字典查找的主键名
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为在java之中和在VB.NET之中的字典查找的原理不一样，所以在这里使用这个字符串作为主键进行查找
        ''' </remarks>
        Public ReadOnly Property key As String
            Get
                Return chunkOffset.GetJson
            End Get
        End Property

        Friend Sub New(chunkOffset As Long())
            Me.chunkOffset = chunkOffset
            Me.hashcode = chunkOffset.GetHashCode
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return hashcode
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            ElseIf obj Is Nothing Then
                Return False
            ElseIf GetType(ChunkOffsetKey) IsNot obj.[GetType]() Then
                Return False
            End If

            Dim other As ChunkOffsetKey = DirectCast(obj, ChunkOffsetKey)

            Return chunkOffset.SequenceEqual(other.chunkOffset)
        End Function

        Public Overrides Function ToString() As String
            Return "ChunkOffsetKey [chunkOffset=" & chunkOffset.GetJson & ", hashcode=" & hashcode & "]"
        End Function

    End Class
End Namespace
