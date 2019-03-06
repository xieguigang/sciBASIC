'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class DataChunkIterator
        Private m_address As Long
        Private m_root As DataNode

        Public Sub New([in] As BinaryReader, sb As Superblock, layout As Layout)

            Me.m_address = layout.dataAddress

            [in].offset = Me.m_address

            Me.m_root = New DataNode([in], sb, layout, Me.m_address)
            Me.m_root.first([in], sb)
        End Sub

        Public Overridable Function hasNext([in] As BinaryReader, sb As Superblock) As Boolean
            Return Me.m_root.hasNext([in], sb)
        End Function

        Public Overridable Function [next]([in] As BinaryReader, sb As Superblock) As DataChunk
            Return Me.m_root.[next]([in], sb)
        End Function
    End Class

End Namespace
