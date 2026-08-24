#Region "Microsoft.VisualBasic::692eae46dfd852155520f851c60379ab, Data\BinaryData\HDF5\structure\DataObjects\Headers\Messages\DataLayoutMessage.vb"

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

    '   Total Lines: 227
    '    Code Lines: 137 (60.35%)
    ' Comment Lines: 56 (24.67%)
    '    - Xml Docs: 67.86%
    ' 
    '   Blank Lines: 34 (14.98%)
    '     File Size: 10.08 KB


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

            ' 诊断：转储原始字节
            Try
                Dim savedPos = [in].offset
                Dim raw = sb.FileReader(address).readBytes(32).ToArray()
                Call Console.WriteLine($"[DIAG] DataLayoutMessage@{address}: version={version}, raw={BitConverter.ToString(raw)}")
                [in].offset = savedPos
            Catch
            End Try

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

                ' 诊断：打印 readO 前的位置和接下来的字节
                Call Console.WriteLine($"[DIAG]   before readO: [in].offset={[in].offset}, msgAddr={address}, expected_offset={address + 3}")
                Try
                    Dim peekPos = [in].offset
                    Dim peek = [in].readBytes(8).ToArray()
                    Call Console.WriteLine($"[DIAG]   peek 8 bytes: {BitConverter.ToString(peek)}")
                    [in].offset = peekPos
                Catch
                End Try

                Me._dataAddress = ReadHelper.readO([in], sb)
                Me._chunkSize = New Integer(Me.dimensionality - 2) {}

                For i As Integer = 0 To Me.dimensionality - 2
                    Me.chunkSize(i) = [in].readInt()
                Next

                Me._dataElementSize = [in].readInt

                Call Console.WriteLine($"[DIAG] LayoutMessage v3 chunked: dataAddr={dataAddress}, dim={dimensionality}, elemSize={dataElementSize}")
                If dataAddress > 0 Then
                    Try
                        Dim savedPos = [in].offset
                        Dim probe = sb.FileReader(dataAddress).readBytes(4).ToArray()
                        Dim sigStr = System.Text.Encoding.ASCII.GetString(probe)
                        Call Console.WriteLine($"[DIAG]   dataAddr {dataAddress} -> sig='{sigStr}', hex={BitConverter.ToString(probe)}")
                        [in].offset = savedPos
                    Catch
                    End Try
                End If
            End If
        End Sub

        Private Sub parseVersion1Or2([in] As BinaryReader, sb As Superblock)
            Me._dimensionality = [in].readByte()
            Me._type = CInt([in].readByte)

            ' Reserved (zero) 1 byte，随后紧跟数据地址
            [in].skipBytes(1)

            Dim isCompact As Boolean = (Me.type = LayoutClass.CompactStorage)

            If Not isCompact Then
                ' Data Address
                Me._dataAddress = ReadHelper.readO([in], sb)
            End If

            ' Dimension sizes 仅存在于 ChunkedStorage 布局中。
            ' Compact 和 Contiguous 布局没有 dimension sizes 字段，
            ' 此前无条件读取会导致 reader 偏移错位，使后续字段读到错误数据。
            If type = LayoutClass.ChunkedStorage Then
                Me._chunkSize = New Integer(Me.dimensionality - 2) {}

                For i As Integer = 0 To Me.dimensionality - 2
                    Me.chunkSize(i) = [in].readInt()
                Next

                Me._dataElementSize = [in].readInt

                Call Console.WriteLine($"[DIAG] LayoutMessage v{version} chunked: dataAddr={dataAddress}, dim={dimensionality}, elemSize={dataElementSize}")
                ' Check if address points to TREE or continuation
                If dataAddress > 0 Then
                    Try
                        Dim savedPos = [in].offset
                        Dim probe = sb.FileReader(dataAddress).readBytes(4).ToArray()
                        Dim sigStr = System.Text.Encoding.ASCII.GetString(probe)
                        Call Console.WriteLine($"[DIAG]   dataAddr {dataAddress} -> sig='{sigStr}', hex={BitConverter.ToString(probe)}")
                        [in].offset = savedPos
                    Catch
                    End Try
                End If
            ElseIf isCompact Then
                ' Dataset Element Size
                Me._dataElementSize = [in].readInt()
                Me._dataAddress = [in].offset
            ElseIf type = LayoutClass.ContiguousStorage Then
                ' Total Size of Dataset Storage (in bytes)
                Me._continuousSize = ReadHelper.readL([in], sb)
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
