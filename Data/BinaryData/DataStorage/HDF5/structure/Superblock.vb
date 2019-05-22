#Region "Microsoft.VisualBasic::26088a1790c1c16bc0b61d7ae3ae09e1, Data\BinaryData\DataStorage\HDF5\structure\Superblock.vb"

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

    '     Class Superblock
    ' 
    '         Properties: addressOfFileFreeSpaceInfo, baseAddress, driverInformationBlockAddress, endOfFileAddress, fileConsistencyFlags
    '                     formatSignature, groupInternalNodeK, groupLeafNodeK, indexedStorageInterNodeK, rootGroupSymbolTableEntry
    '                     sizeOfLengths, sizeOfOffsets, totalSuperBlockSize, validFormatSignature, versionOfFileFreeSpaceStorage
    '                     versionOfRootGroupSymbolTableEntry, versionOfShardedHeaderMessageFormat, versionOfSuperblock
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: printValues, readVersion1, readVersion2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    ''' <summary>
    ''' The superblock may begin at certain predefined offsets within the HDF5 file, allowing a 
    ''' block of unspecified content for users to place additional information at the beginning 
    ''' (and end) of the HDF5 file without limiting the HDF5 Library's ability to manage the 
    ''' objects within the file itself. This feature was designed to accommodate wrapping an 
    ''' HDF5 file in another file format or adding descriptive information to an HDF5 file without 
    ''' requiring the modification of the actual file's information. The superblock is located 
    ''' by searching for the HDF5 format signature at byte offset 0, byte offset 512, and at 
    ''' successive locations in the file, each a multiple of two of the previous location; 
    ''' in other words, at these byte offsets: 0, 512, 1024, 2048, and so on.
    '''
    ''' The superblock Is composed Of the format signature, followed by a superblock version number 
    ''' And information that Is specific To Each version Of the superblock.
    ''' </summary>
    Public Class Superblock : Inherits HDF5Ptr

        Shared ReadOnly SUPERBLOCK_SIGNATURE As Byte() = {&H89, &H48, &H44, &H46, &HD, &HA, &H1A, &HA} _
            .Select(Function(i) CByte(i)) _
            .ToArray

        Public Overridable ReadOnly Property formatSignature As Byte()

        Public Overridable ReadOnly Property validFormatSignature() As Boolean
            Get
                For i As Integer = 0 To 7
                    If Me.formatSignature(i) <> SUPERBLOCK_SIGNATURE(i) Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        Dim reserved0 As Integer
        Dim reserved1 As Integer
        Dim reserved2 As Integer

        Public Overridable ReadOnly Property versionOfSuperblock() As Integer
        Public Overridable ReadOnly Property versionOfFileFreeSpaceStorage() As Integer
        Public Overridable ReadOnly Property versionOfRootGroupSymbolTableEntry() As Integer
        Public Overridable ReadOnly Property versionOfShardedHeaderMessageFormat() As Integer
        Public Overridable ReadOnly Property sizeOfOffsets() As Integer
        Public Overridable ReadOnly Property sizeOfLengths() As Integer
        Public Overridable ReadOnly Property groupLeafNodeK() As Integer
        Public Overridable ReadOnly Property groupInternalNodeK() As Integer
        Public Overridable ReadOnly Property fileConsistencyFlags() As Integer
        ' for ver1
        Public Overridable ReadOnly Property indexedStorageInterNodeK() As Integer
        Public Overridable ReadOnly Property baseAddress() As Long
        Public Overridable ReadOnly Property addressOfFileFreeSpaceInfo() As Long
        Public Overridable ReadOnly Property endOfFileAddress() As Long
        Public Overridable ReadOnly Property driverInformationBlockAddress() As Long
        Public Overridable ReadOnly Property rootGroupSymbolTableEntry() As SymbolTableEntry
        Public Overridable ReadOnly Property totalSuperBlockSize() As Integer

        Public Sub New([in] As BinaryReader, address As Long)
            Call MyBase.New(address)

            [in].offset = address

            ' signature
            Me.formatSignature = [in].readBytes(8)

            If Not Me.validFormatSignature Then
                Throw New IOException("signature is not valid")
            End If

            Me.versionOfSuperblock = [in].readByte()

            If Me.versionOfSuperblock <= 1 Then
                readVersion1([in])
            ElseIf Me.versionOfSuperblock = 2 Then
                readVersion2([in])
            Else
                Throw New IOException("Unknown superblock version " & Me.versionOfSuperblock)
            End If
        End Sub

        Private Sub readVersion1([in] As BinaryReader)
            _versionOfFileFreeSpaceStorage = [in].readByte()
            _versionOfRootGroupSymbolTableEntry = [in].readByte()

            reserved0 = [in].readByte()

            _versionOfShardedHeaderMessageFormat = [in].readByte()
            _sizeOfOffsets = [in].readByte()
            _sizeOfLengths = [in].readByte()

            reserved1 = [in].readByte()

            _groupLeafNodeK = [in].readShort()
            _groupInternalNodeK = [in].readShort()
            _fileConsistencyFlags = [in].readInt()

            _totalSuperBlockSize = 24

            If _versionOfSuperblock = 1 Then
                _indexedStorageInterNodeK = [in].readShort()
                reserved2 = [in].readShort()
                _totalSuperBlockSize += 4
            End If

            _baseAddress = ReadHelper.readO([in], Me)
            _addressOfFileFreeSpaceInfo = ReadHelper.readO([in], Me)
            _endOfFileAddress = ReadHelper.readO([in], Me)
            _driverInformationBlockAddress = ReadHelper.readO([in], Me)
            _totalSuperBlockSize += _sizeOfOffsets * 4
            _rootGroupSymbolTableEntry = New SymbolTableEntry([in], Me, [in].offset)
            _totalSuperBlockSize += _rootGroupSymbolTableEntry.totalSymbolTableEntrySize
        End Sub

        Private Sub readVersion2([in] As BinaryReader)
            Throw New IOException("version 2 is not implemented")
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("Superblock >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("signature : " &
                              (formatSignature(0) And &HFF).ToString("x") &
                              (formatSignature(1) And &HFF).ToString("x") &
                              (formatSignature(2) And &HFF).ToString("x") &
                              (formatSignature(3) And &HFF).ToString("x") &
                              (formatSignature(4) And &HFF).ToString("x") &
                              (formatSignature(5) And &HFF).ToString("x") &
                              (formatSignature(6) And &HFF).ToString("x") &
                              (formatSignature(7) And &HFF).ToString("x")
                             )

            console.WriteLine("version of super block : " & versionOfSuperblock)
            console.WriteLine("version of file free space storage : " & versionOfFileFreeSpaceStorage)
            console.WriteLine("version of root group symbol table entry : " & versionOfRootGroupSymbolTableEntry)
            console.WriteLine("reserved 0 : " & reserved0)
            console.WriteLine("version of sharded header message format : " & versionOfShardedHeaderMessageFormat)
            console.WriteLine("size of offsets : " & sizeOfOffsets)
            console.WriteLine("size of lengths : " & sizeOfLengths)
            console.WriteLine("reserved 1 : " & reserved1)
            console.WriteLine("group leaf node k : " & groupLeafNodeK)
            console.WriteLine("group internal node k : " & groupInternalNodeK)
            console.WriteLine("file consistency flags : " & fileConsistencyFlags)

            If versionOfSuperblock >= 1 Then
                console.WriteLine("indexed storage internode k : " & indexedStorageInterNodeK)
                console.WriteLine("reserved 2 : " & reserved2)
            End If

            console.WriteLine("base address : " & baseAddress)
            console.WriteLine("address of file free space info : " & addressOfFileFreeSpaceInfo)
            console.WriteLine("end of file address : " & endOfFileAddress)
            console.WriteLine("driver information block address : " & driverInformationBlockAddress)

            rootGroupSymbolTableEntry.printValues(console)

            console.WriteLine("total super block size : " & totalSuperBlockSize)
            console.WriteLine("Superblock <<<")
        End Sub
    End Class

End Namespace
