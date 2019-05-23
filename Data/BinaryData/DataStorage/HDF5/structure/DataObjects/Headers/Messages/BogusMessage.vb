Imports System.IO

Namespace HDF5.struct

    ''' <summary>
    ''' This message is used for testing the HDF5 Library’s response to an “unknown” message 
    ''' type and should never be encountered in a valid HDF5 file.
    ''' 
    ''' For testing only; should never be stored in a valid file.
    ''' </summary>
    Public Class BogusMessage : Inherits Message

        Public ReadOnly Property value As Integer

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            value = sb.FileReader(address).readInt
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            Call console.WriteLine($"Bogus Value: {value}")
        End Sub
    End Class
End Namespace