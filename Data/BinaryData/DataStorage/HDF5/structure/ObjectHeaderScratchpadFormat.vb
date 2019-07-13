#Region "Microsoft.VisualBasic::7941013d725e59de6e721a74bf180eab, Data\BinaryData\DataStorage\HDF5\structure\ObjectHeaderScratchpadFormat.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class ObjectHeaderScratchpadFormat
    ' 
    '         Properties: addressOfBTree, addressOfNameHeap, totalObjectHeaderScratchpadFormatSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: printValues
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
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5.struct

    Public Class ObjectHeaderScratchpadFormat : Inherits HDF5Ptr

        Public  ReadOnly Property addressOfBTree As Long
        Public  ReadOnly Property addressOfNameHeap As Long
        Public  ReadOnly Property totalObjectHeaderScratchpadFormatSize As Integer

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.addressOfBTree = ReadHelper.readO([in], sb)
            Me.addressOfNameHeap = ReadHelper.readO([in], sb)
            Me.totalObjectHeaderScratchpadFormatSize = sb.sizeOfOffsets * 2
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString}  btree=&{addressOfBTree}"
        End Function

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("ObjectHeaderScratchpadFormat >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("address of BTree : " & Me.addressOfBTree)
            console.WriteLine("address of name heap : " & Me.addressOfNameHeap)

            console.WriteLine("total object header scratchpad format size : " & Me.totalObjectHeaderScratchpadFormatSize)
            console.WriteLine("ObjectHeaderScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
