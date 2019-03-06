
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class GroupMessage
        Private m_address As Long
        Private m_bTreeAddress As Long
        Private m_nameHeapAddress As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address
            Me.m_bTreeAddress = ReadHelper.readO([in], sb)
            Me.m_nameHeapAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property bTreeAddress() As Long
            Get
                Return Me.m_bTreeAddress
            End Get
        End Property

        Public Overridable ReadOnly Property nameHeapAddress() As Long
            Get
                Return Me.m_nameHeapAddress
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("GroupMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("btree address : " & Me.m_bTreeAddress)
            Console.WriteLine("nameheap address : " & Me.m_nameHeapAddress)
            Console.WriteLine("GroupMessage <<<")
        End Sub

    End Class

End Namespace
