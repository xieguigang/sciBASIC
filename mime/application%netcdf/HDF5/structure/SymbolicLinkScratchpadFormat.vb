
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class SymbolicLinkScratchpadFormat
        Private m_address As Long
        Private m_offsetToLinkValue As Integer

        Private m_totalSymbolicLinkScratchpadFormatSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address

            Me.m_offsetToLinkValue = [in].readInt()

            Me.m_totalSymbolicLinkScratchpadFormatSize = 4
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property offsetToLinkValue() As Integer
            Get
                Return Me.m_offsetToLinkValue
            End Get
        End Property

        Public Overridable ReadOnly Property totalSymbolicLinkScratchpadFormatSize() As Integer
            Get
                Return Me.m_totalSymbolicLinkScratchpadFormatSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("SymbolicLinkScratchpadFormat >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset to link value : " & Me.m_offsetToLinkValue)

            Console.WriteLine("total symbolic link scratchpad format size : " & Me.m_totalSymbolicLinkScratchpadFormatSize)
            Console.WriteLine("SymbolicLinkScratchpadFormat <<<")
        End Sub
    End Class

End Namespace
