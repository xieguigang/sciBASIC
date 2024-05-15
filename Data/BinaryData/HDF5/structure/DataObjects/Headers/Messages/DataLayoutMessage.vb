#Region "Microsoft.VisualBasic::6e7ab7acad7f8ee14afa83f429906ae5, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\DataLayoutMessage.vb"

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

    '   Total Lines: 180
    '    Code Lines: 98
    ' Comment Lines: 51
    '   Blank Lines: 31
    '     File Size: 7.29 KB


    '     Class DataLayoutMessage
    ' 
    '         Properties: chunkSize, continuousSize, dataAddress, dataElementSize, dataset
    '                     dimensionality, type, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: parseVersion1Or2, parseVersion3, printValues
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct.messages

    ''' <summary>
    ''' The Data Layout message describes how the elements of a multi-dimensional array 
    ''' are stored in the HDF5 file. Required for datasets; may not be repeated.
    ''' </summary>
    Public Class DataLayoutMessage : Inherits Message

        ''' <summary>
        ''' The version number information is used for changes in the format of the 
        ''' data layout message and is described here:
        ''' 
        ''' + 0: Never used.
        ''' + 1: Used by version 1.4 And before of the library to encode layout information. 
        '''      Data space Is always allocated when the data set Is created.
        ''' + 2: Used by version 1.6.x of the library to encode layout information. Data 
        '''      space Is allocated only when it Is necessary.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property version As Integer

        ''' <summary>
        ''' number of Dimensions
        ''' 
        ''' An array has a fixed dimensionality. This field specifies the number of dimension 
        ''' size fields later in the message. The value stored for chunked storage is 1 greater 
        ''' than the number of dimensions in the dataset’s dataspace. For example, 2 is stored 
        ''' for a 1 dimensional dataset.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dimensionality As Integer

        ''' <summary>
        ''' Layout Class
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property type As LayoutClass

        ''' <summary>
        ''' For contiguous storage, this is the address of the raw data in the file. For chunked 
        ''' storage this is the address of the v1 B-tree that is used to look up the addresses 
        ''' of the chunks. This field is not present for compact storage. If the version for 
        ''' this message is greater than 1, the address may have the “undefined address” value, 
        ''' to indicate that storage has not yet been allocated for this array.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dataAddress As Long
        Public ReadOnly Property continuousSize As Long
        Public ReadOnly Property chunkSize As Integer()
        ''' <summary>
        ''' Dataset element size/Compact Data Size(Compact Data/chunked data)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dataElementSize As Integer

        Public ReadOnly Property dataset As Hdf5Dataset

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.version = [in].readByte()

            If Me.version < 3 Then
                Call parseVersion1Or2([in], sb)
            Else
                Call parseVersion3([in], sb)
            End If

            Select Case type
                Case LayoutClass.ChunkedStorage
                    dataset = New ChunkedDatasetV3 With {
                        .BtreeAddress = dataAddress,
                        .byteSize = dataElementSize,
                        .dimensionality = dimensionality,
                        .dimensionSize = chunkSize
                    }
                Case LayoutClass.CompactStorage
                    dataset = New CompactDataset With {
                        .size = dataElementSize,
                        .rawData = [in].readBytes(dataElementSize)
                    }
                Case LayoutClass.ContiguousStorage
                    dataset = New ContiguousDataset With {
                        .dataAddress = dataAddress,
                        .size = continuousSize
                    }
                Case Else
                    Throw New NotImplementedException
            End Select
        End Sub

        Private Sub parseVersion3([in] As BinaryReader, sb As Superblock)
            Me._type = CType(CInt([in].readByte), LayoutClass)

            If Me.type = LayoutClass.CompactStorage Then
                Me._dataElementSize = [in].readShort()
                Me._dataAddress = [in].offset
            ElseIf Me.type = LayoutClass.ContiguousStorage Then
                Me._dataAddress = ReadHelper.readO([in], sb)
                Me._continuousSize = ReadHelper.readL([in], sb)
            ElseIf Me.type = LayoutClass.ChunkedStorage Then
                Me._dimensionality = [in].readByte()

                ' Call [in].skipBytes(3)

                Me._dataAddress = ReadHelper.readO([in], sb)
                Me._chunkSize = New Integer(Me.dimensionality - 2) {}

                For i As Integer = 0 To Me.dimensionality - 2
                    Me.chunkSize(i) = [in].readInt()
                Next

                Me._dataElementSize = [in].readInt
            End If
        End Sub

        Private Sub parseVersion1Or2([in] As BinaryReader, sb As Superblock)
            Me._dimensionality = [in].readByte()
            Me._type = CInt([in].readByte)

            ' Reserved (zero) 1 + 4 = 5 bytes
            [in].skipBytes(5)

            Dim isCompact As Boolean = (Me.type = LayoutClass.CompactStorage)

            If Not isCompact Then
                ' Data AddressO (optional)
                Me._dataAddress = ReadHelper.readO([in], sb)
            End If

            Me._chunkSize = New Integer(Me.dimensionality - 2) {}

            For i As Integer = 0 To Me.dimensionality - 2
                ' Dimension #n Size
                Me.chunkSize(i) = [in].readInt()
            Next

            If isCompact Then
                ' Dataset Element Size (optional)
                Me._dataElementSize = [in].readInt()
                Me._dataAddress = [in].offset
            ElseIf type = LayoutClass.ChunkedStorage Then
                Me._dataElementSize = [in].readInt
            End If
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("LayoutMessage >>>")

            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("number of dimensions : " & Me.dimensionality)
            console.WriteLine("type : " & Me.type)
            console.WriteLine("data address : " & Me.dataAddress)
            console.WriteLine("continuous size : " & Me.continuousSize)
            console.WriteLine("data size : " & Me.dataElementSize)

            For i As Integer = 0 To Me.chunkSize.Length - 1
                console.WriteLine("chunk size [" & i & "] : " & Me.chunkSize(i))
            Next

            console.WriteLine("LayoutMessage <<<")
        End Sub
    End Class

End Namespace
