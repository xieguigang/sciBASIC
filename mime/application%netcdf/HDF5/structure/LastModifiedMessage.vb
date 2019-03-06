
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class LastModifiedMessage
        Private m_address As Long
        Private m_version As Integer
        Private m_seconds As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_version = [in].readByte()

            [in].skipBytes(3)

            Me.m_seconds = [in].readInt()
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property seconds() As Integer
            Get
                Return Me.m_seconds
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("LastModifiedMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("seconds : " & Me.m_seconds)

            Console.WriteLine("LastModifiedMessage <<<")
        End Sub
    End Class

End Namespace
