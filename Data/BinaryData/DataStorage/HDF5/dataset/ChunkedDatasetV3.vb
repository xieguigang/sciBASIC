
'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Namespace HDF5.dataset

    ''' <summary>
    ''' This represents chunked datasets using a b-tree for indexing raw data chunks.
    ''' It supports filters for use when reading the dataset for example to
    ''' decompress.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class ChunkedDatasetV3
        Inherits DatasetBase


        Private ReadOnly chunkLookup As LazyInitializer(Of IDictionary(Of ChunkOffsetKey, BTreeV1Data.Chunk))
        Private ReadOnly decodedChunkLookup As ConcurrentMap(Of ChunkOffsetKey, SByte()) = New ConcurrentDictionary(Of ChunkOffsetKey, SByte())()

        Private ReadOnly layoutMessage As ChunkedDataLayoutMessageV3

        Private ReadOnly pipeline As FilterPipeline

        Public Sub New(hdfFc As HdfFileChannel, address As Long, name As String, parent As Group, oh As ObjectHeader)
            MyBase.New(hdfFc, address, name, parent, oh)

            layoutMessage = oh.getMessageOfType(GetType(ChunkedDataLayoutMessageV3))

            ' If the dataset has filters get the message
            If oh.hasMessageOfType(GetType(FilterPipelineMessage)) Then
                Dim filterPipelineMessage As FilterPipelineMessage = oh.getMessageOfType(GetType(FilterPipelineMessage))
                pipeline = FilterManager.getPipeline(filterPipelineMessage)
            Else
                pipeline = Nothing
            End If

            chunkLookup = New ChunkLookupLazyInitializer(Me)
        End Sub

        Public Overrides ReadOnly Property dataBuffer() As ByteBuffer
            Get

                ' Need to load the full buffer into memory so create the array
                Dim dataArray As SByte() = New SByte(toIntExact(diskSize) - 1) {}
                logger.trace("Created data buffer for '{}' of size {} bytes", path, dataArray.Length)

                Dim elementSize As Integer = dataType.size
                For i As Integer = 0 To size - 1
                    Dim dimensionedIndex As Integer() = linearIndexToDimensionIndex(i, dimensions)
                    Dim chunkOffset As Long() = getChunkOffset(dimensionedIndex)

                    ' Now figure out which element inside the chunk
                    Dim insideChunk As Integer() = New Integer(chunkOffset.Length - 1) {}
                    For j As Integer = 0 To chunkOffset.Length - 1
                        insideChunk(j) = CInt(dimensionedIndex(j) - chunkOffset(j))
                    Next
                    Dim insideChunkLinearOffset As Integer = dimensionIndexToLinearIndex(insideChunk, layoutMessage.chunkDimensions)
                    Dim chunkData As SByte() = getDecodedChunk(New ChunkOffsetKey(Me, chunkOffset))

                    ' Copy that data into the overall buffer
                    Array.Copy(chunkData, insideChunkLinearOffset * elementSize, dataArray, i * elementSize, elementSize)
                Next

                Return ByteBuffer.wrap(dataArray)
            End Get
        End Property

        Private Function getDecodedChunk(chunkKey As ChunkOffsetKey) As SByte()
            Return decodedChunkLookup.computeIfAbsent(chunkKey, AddressOf Me.decodeChunk)
        End Function

        Private Function decodeChunk(key As ChunkOffsetKey) As SByte()
            Dim chunk As BTreeV1Data.Chunk = getChunk(key)
            ' Get the encoded (i.e. compressed buffer)
            Dim encodedBuffer As ByteBuffer = getDataBuffer(chunk)
            ' Get the encoded data from buffer
            Dim encodedBytes As SByte() = New SByte(encodedBuffer.remaining() - 1) {}

            encodedBuffer.[get](encodedBytes)

            If pipeline Is Nothing Then
                Return encodedBytes
            End If

            ' Decode using the pipeline applying the filters
            Dim decodedBytes As SByte() = pipeline.decode(encodedBytes)

            Return decodedBytes
        End Function

        Private Function getChunk(key As ChunkOffsetKey) As BTreeV1Data.Chunk
            Try
                Return chunkLookup.[get]().[get](key)
            Catch generatedExceptionName As Exception
                Throw New HdfException("Failed to create chunk lookup for '" & path & "'")
            End Try
        End Function

        Private Function getDataBuffer(chunk As BTreeV1Data.Chunk) As ByteBuffer
            Try
                Return hdfFc.map(chunk.address, chunk.size)
            Catch generatedExceptionName As Exception
                Throw New HdfException("Failed to read chunk for dataset '" & path & "' at address " & chunk.address)
            End Try
        End Function

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
                Dim temp As Long = toIntExact(layoutMessage.chunkDimensions(i))
                chunkOffset(i) = (dimensionedIndex(i) \ temp) * temp
            Next
            Return chunkOffset
        End Function

        Private NotInheritable Class ChunkLookupLazyInitializer
            Inherits LazyInitializer(Of IDictionary(Of ChunkOffsetKey, BTreeV1Data.Chunk))
            Private ReadOnly outerInstance As ChunkedDatasetV3

            Public Sub New(outerInstance As ChunkedDatasetV3)
                Me.outerInstance = outerInstance
            End Sub

            Protected Friend Overrides Function initialize() As IDictionary(Of ChunkOffsetKey, BTreeV1Data.Chunk)
                Dim bTree As BTreeV1Data = BTreeV1.createDataBTree(outerInstance.hdfFc, outerInstance.layoutMessage.bTreeAddress, outerInstance.dimensions.Length)
                Dim chunks As IList(Of BTreeV1Data.Chunk) = bTree.chunks
                Dim chunkLookupMap As IDictionary(Of ChunkOffsetKey, BTreeV1Data.Chunk) = New Dictionary(Of ChunkOffsetKey, BTreeV1Data.Chunk)(chunks.Count)

                For Each chunk As BTreeV1Data.Chunk In chunks
                    chunkLookupMap(New ChunkOffsetKey(outerInstance, chunk.chunkOffset)) = chunk
                Next

                Return chunkLookupMap
            End Function
        End Class

        ''' <summary>
        ''' Custom key object for indexing chunks. It is optimised for fast hashcode and
        ''' equals when looking up chunks.
        ''' </summary>
        Private Class ChunkOffsetKey
            Private ReadOnly outerInstance As ChunkedDatasetV3

            Friend ReadOnly hashcode As Integer
            Friend ReadOnly chunkOffset As Long()

            Friend Sub New(outerInstance As ChunkedDatasetV3, chunkOffset As Long())
                Me.outerInstance = outerInstance
                Me.chunkOffset = chunkOffset
                hashcode = Arrays.GetHashCode(chunkOffset)
            End Sub

            Public Overrides Function GetHashCode() As Integer
                Return hashcode
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                If Me Is obj Then
                    Return True
                End If
                If obj Is Nothing Then
                    Return False
                End If
                If GetType(ChunkOffsetKey) IsNot obj.[GetType]() Then
                    Return False
                End If
                Dim other As ChunkOffsetKey = DirectCast(obj, ChunkOffsetKey)
                Return Arrays.Equals(chunkOffset, other.chunkOffset)
            End Function

            Public Overrides Function ToString() As String
                Return "ChunkOffsetKey [chunkOffset=" & Arrays.ToString(chunkOffset) & ", hashcode=" & hashcode & "]"
            End Function

        End Class

    End Class

End Namespace
