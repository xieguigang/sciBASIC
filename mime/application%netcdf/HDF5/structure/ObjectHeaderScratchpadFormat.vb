
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class ObjectHeaderScratchpadFormat
        Private m_address As Long
        Private m_addressOfBTree As Long
        Private m_addressOfNameHeap As Long

        Private m_totalObjectHeaderScratchpadFormatSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address
            Me.m_addressOfBTree = ReadHelper.readO([in], sb)
            Me.m_addressOfNameHeap = ReadHelper.readO([in], sb)

            Me.m_totalObjectHeaderScratchpadFormatSize = sb.sizeOfOffsets * 2
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property addressOfBTree() As Long
            Get
                Return Me.m_addressOfBTree
            End Get
        End Property

        Public Overridable ReadOnly Property addressOfNameHeap() As Long
            Get
                Return Me.m_addressOfNameHeap
            End Get
        End Property

        Public Overridable ReadOnly Property totalObjectHeaderScratchpadFormatSize() As Integer
            Get
                Return Me.m_totalObjectHeaderScratchpadFormatSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("ObjectHeaderScratchpadFormat >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("address of BTree : " & Me.m_addressOfBTree)
            Console.WriteLine("address of name heap : " & Me.m_addressOfNameHeap)

            Console.WriteLine("total object header scratchpad format size : " & Me.m_totalObjectHeaderScratchpadFormatSize)
            Console.WriteLine("ObjectHeaderScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
