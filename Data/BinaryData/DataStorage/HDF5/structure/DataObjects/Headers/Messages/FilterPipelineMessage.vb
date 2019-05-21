Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure]

    Public Class FilterPipelineMessage : Inherits Message

        Public ReadOnly Property version As Integer
        Public ReadOnly Property numberOfFilters As Integer


        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            MyBase.New(address)

            Me.version = [in].readByte()
            Me.numberOfFilters = [in].readByte

            Throw New NotImplementedException
        End Sub
    End Class
End Namespace