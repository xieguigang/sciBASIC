Namespace Network

    Public Interface NodeMetaDataAccessor(Of Node As {New, Network.Node})

        Function hasMetadata(v As Node, key As String) As Boolean
        Function getMetadata(v As Node, key As String) As String

    End Interface
End Namespace