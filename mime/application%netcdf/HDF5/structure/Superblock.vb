
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    ''' <summary>
    ''' The superblock may begin at certain predefined offsets within the HDF5 file, allowing a 
    ''' block of unspecified content for users to place additional information at the beginning 
    ''' (and end) of the HDF5 file without limiting the HDF5 Library¡¯s ability to manage the 
    ''' objects within the file itself. This feature was designed to accommodate wrapping an 
    ''' HDF5 file in another file format or adding descriptive information to an HDF5 file without 
    ''' requiring the modification of the actual file¡¯s information. The superblock is located 
    ''' by searching for the HDF5 format signature at byte offset 0, byte offset 512, and at 
    ''' successive locations in the file, each a multiple of two of the previous location; 
    ''' in other words, at these byte offsets: 0, 512, 1024, 2048, and so on.
    '''
    ''' The superblock Is composed Of the format signature, followed by a superblock version number 
    ''' And information that Is specific To Each version Of the superblock.
    ''' </summary>
    Public Class Superblock

        Shared ReadOnly SUPERBLOCK_SIGNATURE As Byte() = {&H89, &H48, &H44, &H46, &HD, &HA, &H1A, &HA} _
            .Select(Function(i) CByte(i)) _
            .ToArray

        Private m_address As Long

        Private m_formatSignature As Byte()
        Private m_versionOfSuperblock As Integer
        Private m_versionOfFileFreeSpaceStorage As Integer
        Private m_versionOfRootGroupSymbolTableEntry As Integer
        Private m_reserved0 As Integer
        Private m_versionOfShardedHeaderMessageFormat As Integer
        Private m_sizeOfOffsets As Integer
        Private m_sizeOfLengths As Integer
        Private m_reserved1 As Integer
        Private m_groupLeafNodeK As Integer
        Private m_groupInternalNodeK As Integer
        Private m_fileConsistencyFlags As Integer

        ' for ver1
        Private m_indexedStorageInterNodeK As Integer
        Private m_reserved2 As Integer

        Private m_baseAddress As Long
        Private m_addressOfFileFreeSpaceInfo As Long
        Private m_endOfFileAddress As Long
        Private m_driverInformationBlockAddress As Long

        Private m_rootGroupSymbolTableEntry As SymbolTableEntry

        Private m_totalSuperBlockSize As Integer

        Public Sub New([in] As BinaryReader, address As Long)
            [in].offset = address

            Me.m_address = address

            ' signature
            Me.m_formatSignature = [in].readBytes(8)

            If Not Me.validFormatSignature Then
                Throw New IOException("signature is not valid")
            End If

            Me.m_versionOfSuperblock = [in].readByte()

            If Me.m_versionOfSuperblock <= 1 Then
                readVersion1([in])
            ElseIf Me.m_versionOfSuperblock = 2 Then
                readVersion2([in])
            Else
                Throw New IOException("Unknown superblock version " & Me.m_versionOfSuperblock)
            End If
        End Sub

        Private Sub readVersion1([in] As BinaryReader)
            Me.m_versionOfFileFreeSpaceStorage = [in].readByte()
            Me.m_versionOfRootGroupSymbolTableEntry = [in].readByte()
            Me.m_reserved0 = [in].readByte()
            Me.m_versionOfShardedHeaderMessageFormat = [in].readByte()
            Me.m_sizeOfOffsets = [in].readByte()
            Me.m_sizeOfLengths = [in].readByte()
            Me.m_reserved1 = [in].readByte()

            Me.m_groupLeafNodeK = [in].readShort()
            Me.m_groupInternalNodeK = [in].readShort()
            Me.m_fileConsistencyFlags = [in].readInt()

            Me.m_totalSuperBlockSize = 24

            If Me.m_versionOfSuperblock = 1 Then
                Me.m_indexedStorageInterNodeK = [in].readShort()
                Me.m_reserved2 = [in].readShort()

                Me.m_totalSuperBlockSize += 4
            End If

            Me.m_baseAddress = ReadHelper.readO([in], Me)
            Me.m_addressOfFileFreeSpaceInfo = ReadHelper.readO([in], Me)
            Me.m_endOfFileAddress = ReadHelper.readO([in], Me)
            Me.m_driverInformationBlockAddress = ReadHelper.readO([in], Me)

            Me.m_totalSuperBlockSize += Me.m_sizeOfOffsets * 4

            Me.m_rootGroupSymbolTableEntry = New SymbolTableEntry([in], Me, [in].offset)

            Me.m_totalSuperBlockSize += Me.m_rootGroupSymbolTableEntry.totalSymbolTableEntrySize
        End Sub

        Private Sub readVersion2([in] As BinaryReader)
            Throw New IOException("version 2 is not implemented")
        End Sub

        Public Overridable ReadOnly Property formatSignature() As Byte()
            Get
                Return Me.m_formatSignature
            End Get
        End Property

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property validFormatSignature() As Boolean
            Get
                For i As Integer = 0 To 7
                    If Me.m_formatSignature(i) <> SUPERBLOCK_SIGNATURE(i) Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        Public Overridable ReadOnly Property versionOfSuperblock() As Integer
            Get
                Return Me.m_versionOfSuperblock
            End Get
        End Property

        Public Overridable ReadOnly Property versionOfFileFreeSpaceStorage() As Integer
            Get
                Return Me.m_versionOfFileFreeSpaceStorage
            End Get
        End Property

        Public Overridable ReadOnly Property versionOfRootGroupSymbolTableEntry() As Integer
            Get
                Return Me.m_versionOfRootGroupSymbolTableEntry
            End Get
        End Property

        Public Overridable ReadOnly Property versionOfShardedHeaderMessageFormat() As Integer
            Get
                Return Me.m_versionOfShardedHeaderMessageFormat
            End Get
        End Property

        Public Overridable ReadOnly Property sizeOfOffsets() As Integer
            Get
                Return Me.m_sizeOfOffsets
            End Get
        End Property

        Public Overridable ReadOnly Property sizeOfLengths() As Integer
            Get
                Return Me.m_sizeOfLengths
            End Get
        End Property

        Public Overridable ReadOnly Property groupLeafNodeK() As Integer
            Get
                Return Me.m_groupLeafNodeK
            End Get
        End Property

        Public Overridable ReadOnly Property groupInternalNodeK() As Integer
            Get
                Return Me.m_groupInternalNodeK
            End Get
        End Property

        Public Overridable ReadOnly Property fileConsistencyFlags() As Integer
            Get
                Return Me.m_fileConsistencyFlags
            End Get
        End Property

        ' for ver1
        Public Overridable ReadOnly Property indexedStorageInterNodeK() As Integer
            Get
                Return Me.m_indexedStorageInterNodeK
            End Get
        End Property

        Public Overridable ReadOnly Property baseAddress() As Long
            Get
                Return Me.m_baseAddress
            End Get
        End Property

        Public Overridable ReadOnly Property addressOfFileFreeSpaceInfo() As Long
            Get
                Return Me.m_addressOfFileFreeSpaceInfo
            End Get
        End Property

        Public Overridable ReadOnly Property endOfFileAddress() As Long
            Get
                Return Me.m_endOfFileAddress
            End Get
        End Property

        Public Overridable ReadOnly Property driverInformationBlockAddress() As Long
            Get
                Return Me.m_driverInformationBlockAddress
            End Get
        End Property

        Public Overridable ReadOnly Property rootGroupSymbolTableEntry() As SymbolTableEntry
            Get
                Return Me.m_rootGroupSymbolTableEntry
            End Get
        End Property

        Public Overridable ReadOnly Property totalSuperBlockSize() As Integer
            Get
                Return Me.m_totalSuperBlockSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("Superblock >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("signature : " & (Me.m_formatSignature(0) And &HFF).ToString("x") & (Me.m_formatSignature(1) And &HFF).ToString("x") & (Me.m_formatSignature(2) And &HFF).ToString("x") & (Me.m_formatSignature(3) And &HFF).ToString("x") & (Me.m_formatSignature(4) And &HFF).ToString("x") & (Me.m_formatSignature(5) And &HFF).ToString("x") & (Me.m_formatSignature(6) And &HFF).ToString("x") & (Me.m_formatSignature(7) And &HFF).ToString("x"))

            Console.WriteLine("version of super block : " & Me.m_versionOfSuperblock)
            Console.WriteLine("version of file free space storage : " & Me.m_versionOfFileFreeSpaceStorage)
            Console.WriteLine("version of root group symbol table entry : " & Me.m_versionOfRootGroupSymbolTableEntry)
            Console.WriteLine("reserved 0 : " & Me.m_reserved0)
            Console.WriteLine("version of sharded header message format : " & Me.m_versionOfShardedHeaderMessageFormat)
            Console.WriteLine("size of offsets : " & Me.m_sizeOfOffsets)
            Console.WriteLine("size of lengths : " & Me.m_sizeOfLengths)
            Console.WriteLine("reserved 1 : " & Me.m_reserved1)
            Console.WriteLine("group leaf node k : " & Me.m_groupLeafNodeK)
            Console.WriteLine("group internal node k : " & Me.m_groupInternalNodeK)
            Console.WriteLine("file consistency flags : " & Me.m_fileConsistencyFlags)

            If Me.m_versionOfSuperblock >= 1 Then
                Console.WriteLine("indexed storage internode k : " & Me.m_indexedStorageInterNodeK)
                Console.WriteLine("reserved 2 : " & Me.m_reserved2)
            End If

            Console.WriteLine("base address : " & Me.m_baseAddress)
            Console.WriteLine("address of file free space info : " & Me.m_addressOfFileFreeSpaceInfo)
            Console.WriteLine("end of file address : " & Me.m_endOfFileAddress)
            Console.WriteLine("driver information block address : " & Me.m_driverInformationBlockAddress)

            Me.m_rootGroupSymbolTableEntry.printValues()

            Console.WriteLine("total super block size : " & Me.m_totalSuperBlockSize)
            Console.WriteLine("Superblock <<<")
        End Sub
    End Class

End Namespace
