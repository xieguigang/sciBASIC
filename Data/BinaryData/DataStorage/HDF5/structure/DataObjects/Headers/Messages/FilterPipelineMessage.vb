Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Language

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
        Public ReadOnly Property description As New List(Of FilterDescription)

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            MyBase.New(address)

            Me.version = [in].readByte()
            Me.numberOfFilters = [in].readByte

            ' skip 3 reserved zero bytes
            Dim reserved = [in].readBytes(3)

            If reserved.Any(Function(b) b <> 0) Then
                Throw New InvalidProgramException
            End If

            ' read filter descriptions
            For i As Integer = 0 To numberOfFilters - 1
                description += New FilterDescription([in], [in].offset)
            Next
        End Sub
    End Class

    Public Class FilterDescription : Inherits HDF5Ptr

        ''' <summary>
        ''' 2 bytes
        ''' </summary>
        ''' <returns></returns>
        Public Property uid As Byte()

        Sub New([in] As BinaryReader, address&)
            Call MyBase.New(address)

            uid = [in].readBytes(2)


            Throw New NotImplementedException
        End Sub
    End Class
End Namespace