
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 



Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class FillValueOldMessage
        Private m_address As Long
        Private m_size As Integer
        Private m_value As SByte()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_size = [in].readInt()
            Me.m_value = [in].readBytes(Me.m_size)
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property size() As Integer
            Get
                Return Me.m_size
            End Get
        End Property

        Public Overridable ReadOnly Property value() As SByte()
            Get
                Return Me.m_value
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("FillValueOldMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("size : " & Me.m_size)

            Console.WriteLine("FillValueOldMessage <<<")
        End Sub
    End Class

End Namespace
