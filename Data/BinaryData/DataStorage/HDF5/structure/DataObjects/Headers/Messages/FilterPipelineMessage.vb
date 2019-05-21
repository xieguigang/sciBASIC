Imports Microsoft.VisualBasic.Data.IO.HDF5.IO

Namespace HDF5.[Structure]

    ''' <summary>
    ''' This message describes the filter pipeline which should be applied to 
    ''' the data stream by providing filter identification numbers, flags, 
    ''' a name, and client data.
    '''
    ''' This message may be present In the Object headers Of both dataset And 
    ''' group objects. For datasets, it specifies the filters To apply To 
    ''' raw data. For groups, it specifies the filters To apply To the group's 
    ''' fractal heap. Currently, only datasets using chunked data storage use 
    ''' the filter pipeline on their raw data.
    ''' </summary>
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