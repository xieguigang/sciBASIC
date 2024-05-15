#Region "Microsoft.VisualBasic::806232cc522de1f4e6195df0f114f7c8, Data\BinaryData\HDF5\structure\Superblock.vb"

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

    '   Total Lines: 209
    '    Code Lines: 144
    ' Comment Lines: 27
    '   Blank Lines: 38
    '     File Size: 9.42 KB


    '     Class Superblock
    ' 
    '         Properties: addressOfFileFreeSpaceInfo, baseAddress, driverInformationBlockAddress, endOfFileAddress, fileConsistencyFlags
    '                     globalHeaps, groupInternalNodeK, groupLeafNodeK, indexedStorageInterNodeK, magic
    '                     rootGroupSymbolTableEntry, sizeOfLengths, sizeOfOffsets, totalSuperBlockSize, versionOfFileFreeSpaceStorage
    '                     versionOfRootGroupSymbolTableEntry, versionOfShardedHeaderMessageFormat, versionOfSuperblock
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FileReader, GetCacheObject, ToString
    ' 
    '         Sub: AddCacheObject, printValues, readVersion1, readVersion2
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
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

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
        Implements IMagicBlock

        Shared ReadOnly SUPERBLOCK_SIGNATURE As Byte() = {&H89, &H48, &H44, &H46, &HD, &HA, &H1A, &HA} _
            .Select(Function(i) CByte(i)) _
            .ToArray

        Public ReadOnly Property magic As String Implements IMagicBlock.magic

        Dim reserved0 As Integer
        Dim reserved1 As Integer
        Dim reserved2 As Integer

        ReadOnly file As HDF5File

        Public ReadOnly Property versionOfSuperblock() As Integer
        Public ReadOnly Property versionOfFileFreeSpaceStorage() As Integer
        Public ReadOnly Property versionOfRootGroupSymbolTableEntry() As Integer
        Public ReadOnly Property versionOfShardedHeaderMessageFormat() As Integer
        Public ReadOnly Property sizeOfOffsets() As Integer
        Public ReadOnly Property sizeOfLengths() As Integer
        Public ReadOnly Property groupLeafNodeK() As Integer
        Public ReadOnly Property groupInternalNodeK() As Integer
        Public ReadOnly Property fileConsistencyFlags() As Integer
        ' for ver1
        Public ReadOnly Property indexedStorageInterNodeK() As Integer
        Public ReadOnly Property baseAddress() As Long
        Public ReadOnly Property addressOfFileFreeSpaceInfo() As Long
        Public ReadOnly Property endOfFileAddress() As Long
        Public ReadOnly Property driverInformationBlockAddress() As Long
        Public ReadOnly Property rootGroupSymbolTableEntry() As SymbolTableEntry
        Public ReadOnly Property totalSuperBlockSize() As Integer

        Public ReadOnly Property globalHeaps As Dictionary(Of Long, GlobalHeap)
            Get
                Return file.globalHeaps
            End Get
        End Property

        Public Sub New(file As HDF5File, address As Long)
            Call MyBase.New(address)

            Dim [in] = file.reader

            [in].offset = address

            ' signature
            Me.magic = [in].readBytes(8).ToBase64String
            Me.file = file

            If Not Me.VerifyMagicSignature(SUPERBLOCK_SIGNATURE) Then
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCacheObject(address As Long) As DataObject
            Return file.GetCacheObject(address:=address)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddCacheObject(obj As DataObject)
            Call file.addCache(obj)
        End Sub

        ''' <summary>
        ''' 可以通过这个函数来设置文件读取对象的当前读取位置
        ''' </summary>
        ''' <param name="address">小于零的数表示不进行位移</param>
        ''' <returns></returns>
        Public Function FileReader(address As Long) As BinaryReader
            If address >= 0 Then
                file.reader.offset = address
            End If

            Return file.reader
        End Function

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
            _rootGroupSymbolTableEntry = New SymbolTableEntry(Me, [in].offset)
            _totalSuperBlockSize += _rootGroupSymbolTableEntry.totalSymbolTableEntrySize
        End Sub

        Private Sub readVersion2([in] As BinaryReader)
            Throw New IOException("version 2 is not implemented")
        End Sub

        Public Overrides Function ToString() As String
            With New StringBuilder
                Call printValues(New System.IO.StringWriter(.ByRef))
                Return .ToString
            End With
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Dim formatSignature = Encoding.ASCII.GetBytes(magic)

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
