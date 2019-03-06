
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]


    Public Class ContinueMessage
        Private m_address As Long
        Private m_offset As Long
        Private m_length As Long

        Private m_totalObjectHeaderMessageContinueSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)

            [in].offset = address

            Me.m_address = address
            Me.m_offset = ReadHelper.readO([in], sb)
            Me.m_length = ReadHelper.readL([in], sb)

            Me.m_totalObjectHeaderMessageContinueSize = sb.sizeOfOffsets + sb.sizeOfLengths
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property offset() As Long
            Get
                Return Me.m_offset
            End Get
        End Property

        Public Overridable ReadOnly Property length() As Long
            Get
                Return Me.m_length
            End Get
        End Property

        Public Overridable ReadOnly Property totalObjectHeaderMessageContinueSize() As Integer
            Get
                Return Me.m_totalObjectHeaderMessageContinueSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("ObjectHeaderMessageContinue >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("offset : " & Me.m_offset)
            Console.WriteLine("length : " & Me.m_length)
            Console.WriteLine("total header message continue size : " & Me.m_totalObjectHeaderMessageContinueSize)
            Console.WriteLine("ObjectHeaderMessageContinue <<<")
        End Sub
    End Class

End Namespace
