
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 



Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class BTreeEntry

        Private m_address As Long
        Private m_key As Long
        Private m_targetAddress As Long

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address
            Me.m_key = ReadHelper.readL([in], sb)
            Me.m_targetAddress = ReadHelper.readO([in], sb)
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property targetAddress() As Long
            Get
                Return Me.m_targetAddress
            End Get
        End Property

        Public Overridable ReadOnly Property key() As Long
            Get
                Return Me.m_key
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("BTreeEntry >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("key : " & Me.m_key)
            Console.WriteLine("target address : " & Me.m_targetAddress)
            Console.WriteLine("BTreeEntry <<<")
        End Sub
    End Class

End Namespace
