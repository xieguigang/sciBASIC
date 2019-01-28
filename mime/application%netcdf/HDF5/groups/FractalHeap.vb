Namespace org.renjin.hdf5.groups

    Public Class FractalHeap
        Private ReadOnly version As SByte
        Private ReadOnly heapIdLength As Integer
        Private ReadOnly maximumSizeOfManagedObjects As Long
        Private ReadOnly rootBlockAddress As Long
        Private ReadOnly currentNumOfRowsInRootIndirectBlock As Long
        Private ReadOnly startNumOfRowsInRootIndirectBlock As Integer
        Private ReadOnly startingBlockSize As Long
        Private ReadOnly tableWidth As Integer
        Private ReadOnly nextHugeObjectId As Long
        Private ReadOnly btreeAddressOfHugeObjects As Long
        Private ReadOnly amountOfFreeSpaceInManagedBlocks As Long
        Private ReadOnly addressOfManagedBlockFreeSpaceManager As Long
        Private ReadOnly amountOfManagedSpaceInHeap As Long
        Private ReadOnly amountOfAllocatedManagedSpaceInHeap As Long
        Private ReadOnly offsetOfDirectBlockAllocationIteratorInManagedSpace As Long
        Private ReadOnly numberOfManagedObjectsInHeap As Long
        Private ReadOnly sizeOfHugeObjectsInHeap As Long
        Private ReadOnly numberOfHugeObjectsInHeap As Long
        Private ReadOnly sizeOfTinyObjectsInHeap As Long
        Private ReadOnly numberOfTinyObjectsInHeap As Long
        Private ReadOnly maximumDirectBlockSize As Long
        Private ReadOnly maximumHeapSize As Integer
        Private ReadOnly headerFlags As org.renjin.hdf5.Flags
        Private file As org.renjin.hdf5.Hdf5Data

        Private ReadOnly Property DirectBlockChecksummed As Boolean
            Get
                Return headerFlags.isSet(1)
            End Get
        End Property

        'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public FractalHeap(org.renjin.hdf5.Hdf5Data file, long address) throws java.io.IOException
        Public Sub New(file As org.renjin.hdf5.Hdf5Data, address As Long)
            Me.file = file

            Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(address, maxHeaderSize(file.Superblock))
            reader.checkSignature("FRHP")

            version = reader.readByte()
            heapIdLength = reader.readUInt16()
            Dim ioFiltersEncodedLength As Integer = reader.readUInt16()
            headerFlags = reader.readFlags()
            maximumSizeOfManagedObjects = reader.readUInt32()
            nextHugeObjectId = reader.readLength()
            btreeAddressOfHugeObjects = reader.readOffset()
            amountOfFreeSpaceInManagedBlocks = reader.readLength()
            addressOfManagedBlockFreeSpaceManager = reader.readOffset()
            amountOfManagedSpaceInHeap = reader.readLength()
            amountOfAllocatedManagedSpaceInHeap = reader.readLength()
            offsetOfDirectBlockAllocationIteratorInManagedSpace = reader.readLength()
            numberOfManagedObjectsInHeap = reader.readLength()
            sizeOfHugeObjectsInHeap = reader.readLength()
            numberOfHugeObjectsInHeap = reader.readLength()
            sizeOfTinyObjectsInHeap = reader.readLength()
            numberOfTinyObjectsInHeap = reader.readLength()
            tableWidth = reader.readUInt16()
            startingBlockSize = reader.readLength()
            maximumDirectBlockSize = reader.readLength()
            maximumHeapSize = reader.readUInt16()
            startNumOfRowsInRootIndirectBlock = reader.readUInt16()
            rootBlockAddress = reader.readOffset()
            currentNumOfRowsInRootIndirectBlock = reader.readUInt16()
            If ioFiltersEncodedLength > 0 Then
                Dim sizeOfFilteredRootDirectBlock As Long = reader.readLength()
                Dim ioFilterMask As Long = reader.readUInt()
                Dim ioFilterInformation() As SByte = reader.readBytes(ioFiltersEncodedLength)
            End If
            Dim checkSum As Integer = reader.readUInt()
        End Sub

        Public Shared Function maxHeaderSize(superblock As org.renjin.hdf5.Superblock) As Long
            Return 4 + 1 + 4 + 1 + 4 + 13 * superblock.LengthSize + 3 * superblock.OffsetSize + 2 + 4 + 2 + 4 + 100 + 4 ' checksum -  I/O filter information ?? -  i/o filter mask -  current row -  max heap size + start # fo rows -  table width -  offset fields -  length fields -  Max size of managed objects -  Flags -  Heap ID Length + I/O filters encoded length -  Version -  Signature

        End Function


        ''' <summary>
        ''' The number of bytes used to encode this field is the Maximum Heap Size (in the heap’s header) divided by
        ''' 8 and rounded up to the next highest integer, for values that are not a multiple of 8. This value is
        ''' principally used for file integrity checking.
        ''' </summary>
        Private Function blockOffsetSize() As Integer
            Dim bytes As Integer = maximumHeapSize \ 8
            If maximumHeapSize Mod 8 <> 0 Then
                bytes += 1
            End If
            Return bytes
        End Function

        'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public DirectBlock getRootBlock() throws java.io.IOException
        Public Overridable ReadOnly Property RootBlock As DirectBlock
            Get
                Return New DirectBlock(Me, rootBlockAddress, startingBlockSize)
            End Get
        End Property

        Public Class DirectBlock
            Private ReadOnly outerInstance As FractalHeap


            Private ReadOnly buffer As java.nio.ByteBuffer

            'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
            'ORIGINAL LINE: public DirectBlock(long address, long size) throws java.io.IOException
            Public Sub New(outerInstance As FractalHeap, address As Long, size As Long)
                Me.outerInstance = outerInstance

                buffer = outerInstance.file.bufferAt(address, size)
                Dim reader As New org.renjin.hdf5.HeaderReader(outerInstance.file.Superblock, buffer)

                reader.checkSignature("FHDB")
                Dim version As SByte = reader.readByte()
                If version <> 0 Then
                    Throw New Exception("Direct block version " & version)
                End If

                Dim heapHeaderAddress As Long = reader.readOffset()
                Dim blockOffset As Long = reader.readUInt(outerInstance.blockOffsetSize())
                If outerInstance.DirectBlockChecksummed Then
                    Dim checkSum As Integer = reader.readUInt()
                End If
            End Sub

            'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
            'ORIGINAL LINE: public org.renjin.hdf5.message.LinkMessage readLinkMessage() throws java.io.IOException
            Public Overridable Function readLinkMessage() As org.renjin.hdf5.message.LinkMessage
                Return New org.renjin.hdf5.message.LinkMessage(New org.renjin.hdf5.HeaderReader(outerInstance.file.Superblock, buffer.slice()))
            End Function

        End Class

    End Class

End Namespace