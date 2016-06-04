Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Settings

Public Module ProfileStreams

    <Extension>
    Public Function WriteProfiles(Of T As IProfile)(x As T, path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean

    End Function

    <Extension>
    Public Function GenerateProfiles(Of T As IProfile)(x As T) As KeyValuePair()

    End Function
End Module