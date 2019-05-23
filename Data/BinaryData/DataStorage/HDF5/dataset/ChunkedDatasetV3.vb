
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HDF5.dataset

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
        ''' ¡°undefined address¡± value, to indicate that storage has not yet been allocated for this 
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

        Public Overridable ReadOnly Property size() As Long
            Get
                Return dataSpace.totalLength
            End Get
        End Property

        Public Overridable ReadOnly Property diskSize() As Long
            Get
                Return size * dataType.size
            End Get
        End Property

        Public Overridable ReadOnly Property dimensions() As Integer()
            Get
                Return dataSpace.dimensionLength
            End Get
        End Property

        Public Overridable ReadOnly Property maxSize() As Integer()
            Get
                If Not dataSpace.maxDimensionLength.IsNullOrEmpty Then
                    Return dataSpace.maxDimensionLength
                Else
                    Return dimensions
                End If
            End Get
        End Property

        Private Function linearIndexToDimensionIndex(index As Integer, dimensions As Integer()) As Integer()
            Dim dimIndex As Integer() = New Integer(dimensions.Length - 1) {}

            For i As Integer = dimIndex.Length - 1 To 0 Step -1
                dimIndex(i) = index Mod dimensions(i)
                index = index \ dimensions(i)
            Next
            Return dimIndex
        End Function

        Private Function dimensionIndexToLinearIndex(index As Integer(), dimensions As Integer()) As Integer
            Dim linear As Integer = 0

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

            For i As Integer = 0 To size - 1
                Dim dimensionedIndex As Integer() = linearIndexToDimensionIndex(i, dimensions)
                Dim chunkOffset As Long() = getChunkOffset(dimensionedIndex)

                ' Now figure out which element inside the chunk
                Dim insideChunk As Integer() = New Integer(chunkOffset.Length - 1) {}

                For j As Integer = 0 To chunkOffset.Length - 1
                    insideChunk(j) = CInt(dimensionedIndex(j) - chunkOffset(j))
                Next

                Dim insideChunkLinearOffset As Integer = dimensionIndexToLinearIndex(insideChunk, dataLayout.chunkSize)
                Dim chunkData As Byte() = getDecodedChunk(chunkLookup, New ChunkOffsetKey(chunkOffset))

                ' Copy that data into the overall buffer
                Array.Copy(chunkData, insideChunkLinearOffset * elementSize, dataArray, i * elementSize, elementSize)
            Next

            Return New MemoryStream(dataArray)
        End Function

        Private Function getDecodedChunk(chunkLookup As ChunkLookup, chunkKey As ChunkOffsetKey) As Byte()
            Return decodedChunkLookup.ComputeIfAbsent(chunkKey, Function(key) decodeChunk(chunkLookup(chunkKey), chunkLookup.sb))
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
            Return sb.FileReader(chunk.filePosition).readBytes(chunk.size)
        End Function
    End Class

    Public Class ChunkLookup

        ReadOnly lookup As Dictionary(Of ChunkOffsetKey, DataChunk)

        Public ReadOnly Property sb As Superblock
        Default Public ReadOnly Property GetChunk(key As ChunkOffsetKey) As DataChunk
            Get
                Return lookup(key)
            End Get
        End Property

        Sub New(sb As Superblock, dataset As ChunkedDatasetV3)
            Dim bTree As New DataBTree(dataset.dataLayout)
            Dim chunkLookupMap As New Dictionary(Of ChunkOffsetKey, DataChunk)()

            For Each chunk As DataChunk In bTree.EnumerateChunks(sb)
                chunkLookupMap(New ChunkOffsetKey(chunk.offsets)) = chunk
            Next

            lookup = chunkLookupMap
        End Sub

    End Class
    ''' <summary>
    ''' Custom key object for indexing chunks. It is optimised for fast hashcode and
    ''' equals when looking up chunks.
    ''' </summary>
    Public Class ChunkOffsetKey

        Friend ReadOnly hashcode As Integer
        Friend ReadOnly chunkOffset As Long()

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
